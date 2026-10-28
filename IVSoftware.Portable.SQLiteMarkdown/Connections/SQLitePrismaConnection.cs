using IVSoftware.Portable.Common.Exceptions;
using IVSoftware.Portable.Disposable;
using SQLite;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace IVSoftware.Portable.SQLiteMarkdown
{
    /// <summary>
    /// Owns synchronous CRUD scopes and token-tracked asynchronous SQLite work for one database.
    /// </summary>
    /// <remarks>
    /// - Compose one owner per database path; do not share its underlying pooled connection externally.
    /// - Async callers await operations, never dispose connections or manage participation tokens.
    /// - Synchronous calls use their own using scope and never join DisposableHost.
    /// - ReleaseAsync retires this owner; its caller owns file deletion and domain eligibility.
    /// </remarks>
    public sealed class SQLitePrismaConnection
    {
        readonly object gate = new object();
        readonly SemaphoreSlim execution = new SemaphoreSlim(1, 1);
        readonly DisposableHost participation = new DisposableHost(nameof(SQLitePrismaConnection));
        readonly object watchdogSender = new object();
        readonly WatchdogTimer watchdog;
        readonly TaskCompletionSource<bool> drained = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        readonly SQLiteOpenFlags flags;
        SQLiteAsyncConnection? connection;
        IDisposable? keepAliveToken;
        TimeSpan keepAlive = TimeSpan.FromSeconds(10);
        long lastActivity;
        bool retiring;
        Task<bool> closing = Task.FromResult(true);
        Task<bool>? release;

        /// <summary>
        /// Binds one connection owner to a physical database with serialized SQLite connections.
        /// </summary>
        public SQLitePrismaConnection(string databasePath,
            SQLiteOpenFlags openFlags = SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.FullMutex)
        {
            DatabasePath = Path.GetFullPath(databasePath);
            flags = (openFlags & ~SQLiteOpenFlags.NoMutex) | SQLiteOpenFlags.FullMutex;
            watchdog = new WatchdogTimer(defaultCompleteAction: ExpireKeepAlive);
            participation.CountChanged += (_, e) =>
            {
                if (ReferenceEquals(e.Token.Sender, watchdogSender)) return;
                lock (gate)
                {
                    if (!retiring) KickWatchdog();
                }
            };
            participation.FinalDispose += (_, __) =>
            {
                lock (gate)
                {
                    // A client CountChanged callback may already have acquired WDT's token.
                    if (participation.ReferenceCount != 0) return;
                    if (retiring) drained.TrySetResult(true);
                    else closing = CloseOwnedConnectionAsync();
                }
            };
        }

        /// <summary>
        /// The full path whose managed connections must be relinquished before exclusive file access.
        /// </summary>
        public string DatabasePath { get; }

        /// <summary>
        /// Idle keep-alive, clamped to one through ten seconds and initially ten seconds.
        /// </summary>
        public TimeSpan KeepAlive
        {
            get { lock (gate) return keepAlive; }
            set
            {
                lock (gate)
                {
                    keepAlive = value < TimeSpan.FromSeconds(1) ? TimeSpan.FromSeconds(1)
                        : value > TimeSpan.FromSeconds(10) ? TimeSpan.FromSeconds(10) : value;
                    if (!retiring && keepAliveToken != null) KickWatchdog();
                }
            }
        }

        /// <summary>
        /// Whether this owner has stopped admitting work for permanent release.
        /// </summary>
        public bool IsReleased { get { lock (gate) return retiring; } }

        /// <summary>
        /// Whether an asynchronous connection is still held, including a close that has not succeeded.
        /// </summary>
        public bool HasAsyncConnection { get { lock (gate) return connection != null; } }

        void KickWatchdog()
        {
            // Caller holds gate; WDT participation does not recursively kick itself.
            if (keepAliveToken == null) keepAliveToken = participation.GetToken(watchdogSender);
            lastActivity = Stopwatch.GetTimestamp();
            watchdog.Interval = keepAlive;
            watchdog.StartOrRestart();
        }

        void ExpireKeepAlive()
        {
            lock (gate)
            {
                if (retiring || keepAliveToken == null) return;
                var elapsed = TimeSpan.FromSeconds((Stopwatch.GetTimestamp() - lastActivity) / (double)Stopwatch.Frequency);
                if (elapsed < keepAlive)
                {
                    // A previous timer callback may already have escaped WDT's revision check.
                    watchdog.Interval = keepAlive - elapsed;
                    watchdog.StartOrRestart();
                    return;
                }
                var token = keepAliveToken;
                keepAliveToken = null;
                token.Dispose();
            }
        }

        async Task<bool> CloseOwnedConnectionAsync()
        {
            var owned = connection;
            if (owned == null) return true;
            try
            {
                // Close on the pool, without capturing an application's UI synchronization context.
                await Task.Run(() => owned.CloseAsync()).ConfigureAwait(false);
                lock (gate)
                {
                    if (ReferenceEquals(connection, owned)) connection = null;
                }
                return true;
            }
            catch (Exception ex)
            {
                this.RethrowSoft(ex, "SQLitePrismaConnection.Close", "Database connection could not be released.", @throw: false);
                return false;
            }
        }

        async Task<T> RunAsync<T>(Func<SQLiteAsyncConnection, Task<T>> operation, T unsuccessful)
        {
            IDisposable token;
            Task<bool> priorClose;
            lock (gate)
            {
                if (retiring) return unsuccessful;
                // Admission and ownership happen before the first await, including fire-and-forget callers.
                token = participation.GetToken(this);
                priorClose = closing;
            }
            {
                try
                {
                    if (!await priorClose.ConfigureAwait(false)) return unsuccessful;
                    await execution.WaitAsync().ConfigureAwait(false);
                    try
                    {
                        SQLiteAsyncConnection owned;
                        bool initialize;
                        lock (gate)
                        {
                            // Admitted work is allowed to finish even after retirement starts.
                            initialize = connection == null;
                            if (connection == null)
                            {
                                connection = new SQLiteAsyncConnection(DatabasePath, flags);
                            }
                            owned = connection;
                        }
                        if (initialize) await owned.SetBusyTimeoutAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
                        return await operation(owned).ConfigureAwait(false);
                    }
                    finally { execution.Release(); }
                }
                catch (Exception ex)
                {
                    this.RethrowSoft(ex, "SQLitePrismaConnection.Operation", "Database operation did not complete.", @throw: false);
                    return unsuccessful;
                }
                finally
                {
                    // Serialize token release with timer expiry, admission, and explicit retirement.
                    lock (gate) token.Dispose();
                }
            }
        }

        T Run<T>(Func<SQLiteConnection, T> operation, T unsuccessful)
        {
            execution.Wait();
            try
            {
                lock (gate) if (retiring) return unsuccessful;
                using (var owned = new SQLiteConnection(DatabasePath, flags))
                {
                    owned.BusyTimeout = TimeSpan.FromSeconds(5);
                    return operation(owned);
                }
            }
            catch (Exception ex)
            {
                this.RethrowSoft(ex, "SQLitePrismaConnection.Operation", "Database operation did not complete.", @throw: false);
                return unsuccessful;
            }
            finally { execution.Release(); }
        }

        /// <summary>
        /// Stops admission and awaits accepted work and connection closure; does not delete the file.
        /// </summary>
        /// <remarks>
        /// Repeated calls join the same release. False means the caller must retain the file.
        /// Idle closure is reopenable; this explicit release is terminal for this instance.
        /// Independent connections are outside this owner's release guarantee.
        /// </remarks>
        public Task<bool> ReleaseAsync()
        {
            lock (gate)
            {
                if (release != null) return release;
                retiring = true;
                watchdog.Cancel();
                var token = keepAliveToken;
                keepAliveToken = null;
                token?.Dispose();
                if (participation.ReferenceCount == 0) drained.TrySetResult(true);
                return release = ReleaseCoreAsync();
            }
        }

        async Task<bool> ReleaseCoreAsync()
        {
            await drained.Task.ConfigureAwait(false);
            await execution.WaitAsync().ConfigureAwait(false);
            try
            {
                await closing.ConfigureAwait(false);
                return await CloseOwnedConnectionAsync().ConfigureAwait(false);
            }
            finally { execution.Release(); }
        }

        /// <summary>
        /// Materializes query results before releasing this operation's participation.
        /// </summary>
        public Task<List<T>> QueryAsync<T>(string sql, params object[] arguments) where T : new() =>
            RunAsync(cnx => cnx.QueryAsync<T>(sql, arguments), new List<T>());

        /// <summary>
        /// Materializes query results within a synchronous connection scope.
        /// </summary>
        public List<T> Query<T>(string sql, params object[] arguments) where T : new() =>
            Run(cnx => cnx.Query<T>(sql, arguments), new List<T>());

        /// <summary>
        /// Creates a table as tracked asynchronous work.
        /// </summary>
        public Task<bool> CreateTableAsync<T>() where T : new() =>
            RunAsync(async cnx => { await cnx.CreateTableAsync<T>().ConfigureAwait(false); return true; }, false);

        /// <summary>
        /// Creates a table within a synchronous connection scope.
        /// </summary>
        public bool CreateTable<T>() where T : new() =>
            Run(cnx => { cnx.CreateTable<T>(); return true; }, false);

        /// <summary>
        /// Inserts a record and retains participation until the write settles.
        /// </summary>
        public Task<int> InsertAsync(object record) => RunAsync(cnx => cnx.InsertAsync(record), 0);

        /// <summary>
        /// Inserts a record within a synchronous connection scope.
        /// </summary>
        public int Insert(object record) => Run(cnx => cnx.Insert(record), 0);

        /// <summary>
        /// Inserts or replaces a record as tracked asynchronous work.
        /// </summary>
        public Task<int> InsertOrReplaceAsync(object record) => RunAsync(cnx => cnx.InsertOrReplaceAsync(record), 0);

        /// <summary>
        /// Inserts or replaces a record within a synchronous connection scope.
        /// </summary>
        public int InsertOrReplace(object record) => Run(cnx => cnx.InsertOrReplace(record), 0);

        /// <summary>
        /// Updates a record; callers may leave the returned task unawaited without losing participation tracking.
        /// </summary>
        public Task<int> UpdateAsync(object record) => RunAsync(cnx => cnx.UpdateAsync(record), 0);

        /// <summary>
        /// Updates a record within a synchronous connection scope.
        /// </summary>
        public int Update(object record) => Run(cnx => cnx.Update(record), 0);

        /// <summary>
        /// Deletes a record as tracked asynchronous work, leaving database-file lifetime to the owner.
        /// </summary>
        public Task<int> DeleteAsync(object record) => RunAsync(cnx => cnx.DeleteAsync(record), 0);

        /// <summary>
        /// Deletes a record within a synchronous connection scope.
        /// </summary>
        public int Delete(object record) => Run(cnx => cnx.Delete(record), 0);

        /// <summary>
        /// Executes a parameterized command as tracked asynchronous work.
        /// </summary>
        public Task<int> ExecuteAsync(string sql, params object[] arguments) =>
            RunAsync(cnx => cnx.ExecuteAsync(sql, arguments), 0);

        /// <summary>
        /// Executes a parameterized command within a synchronous connection scope.
        /// </summary>
        public int Execute(string sql, params object[] arguments) => Run(cnx => cnx.Execute(sql, arguments), 0);
    }
}
