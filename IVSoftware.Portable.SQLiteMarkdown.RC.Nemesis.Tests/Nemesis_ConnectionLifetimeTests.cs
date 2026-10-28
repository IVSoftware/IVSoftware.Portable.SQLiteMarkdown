using System.Diagnostics;
using IVSoftware.Portable.Common.Exceptions;
using IVSoftware.Portable.Disposable;
using IVSoftware.Portable.SQLiteMarkdown;
using SQLite;

namespace IVSoftware.Portable.SQLiteMarkdown.Nemesis.Tests;

/// <summary>
/// Nemesis-generated verification of SQLite lifetime, asynchronous participation, and exclusive file release.
/// </summary>
/// <remarks>
/// - Authorship: Nemesis; independent peer checks using synthetic on-disk stores.
/// - Native database locks hold real SQLite work across WDT expiry and explicit release.
/// - No production data, clipboard, UI, or STA thread is required by these portable checks.
/// </remarks>
[TestClass, DoNotParallelize]
public sealed class Nemesis_ConnectionLifetimeTests
{
    [Table("records")]
    public sealed class Row
    {
        [PrimaryKey] public int Id { get; set; }
        public string Value { get; set; } = "";
    }

    static SQLitePrismaConnection Create() => new(Path.Combine(
        AppContext.BaseDirectory, "synthetic", Guid.NewGuid().ToString("N") + ".db"));

    static async Task Initialize(SQLitePrismaConnection owner)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(owner.DatabasePath)!);
        Assert.IsTrue(await owner.CreateTableAsync<Row>());
        Assert.AreEqual(1, await owner.InsertAsync(new Row { Id = 1, Value = "original" }));
    }

    static async Task Cleanup(SQLitePrismaConnection owner)
    {
        Assert.IsTrue(await owner.ReleaseAsync().WaitAsync(TimeSpan.FromSeconds(10)));
        File.Delete(owner.DatabasePath);
    }

    static async Task WaitForIdleClose(SQLitePrismaConnection owner)
    {
        var deadline = Stopwatch.StartNew();
        while (owner.HasAsyncConnection && deadline.Elapsed < TimeSpan.FromSeconds(4))
            await Task.Delay(20);
        Assert.IsFalse(owner.HasAsyncConnection, "WDT must eventually release the otherwise idle connection.");
    }

    [TestMethod]
    public async Task UnifiedCrudAndUnawaitedUpdatesPersistBeforeReleaseAndPermitFileDeletion()
    {
        var owner = Create();
        try
        {
            await Initialize(owner);
            Assert.AreEqual(TimeSpan.FromSeconds(10), owner.KeepAlive);
            Assert.AreEqual(1, owner.Update(new Row { Id = 1, Value = "sync" }));
            Assert.AreEqual("sync", (await owner.QueryAsync<Row>("SELECT * FROM records")).Single().Value);
            for (int i = 0; i < 40; i++)
                _ = owner.UpdateAsync(new Row { Id = 1, Value = "edit " + i });

            var release = owner.ReleaseAsync();
            Assert.AreSame(release, owner.ReleaseAsync(), "Repeated release joins the same completion.");
            Assert.IsTrue(await release.WaitAsync(TimeSpan.FromSeconds(10)));
            Assert.IsFalse(owner.HasAsyncConnection);
            using (var verify = new SQLiteConnection(owner.DatabasePath, SQLiteOpenFlags.ReadOnly))
                Assert.AreEqual("edit 39", verify.Query<Row>("SELECT * FROM records").Single().Value);
            File.Delete(owner.DatabasePath);
            Assert.IsFalse(File.Exists(owner.DatabasePath), "The persistent source must actually be removable.");
        }
        finally { await Cleanup(owner); }
    }

    [TestMethod]
    public async Task KeepAliveClampsRestartsAndReopensOnlyAfterCompletedIdleClosure()
    {
        var owner = Create();
        try
        {
            owner.KeepAlive = TimeSpan.FromMinutes(1);
            Assert.AreEqual(TimeSpan.FromSeconds(10), owner.KeepAlive);
            owner.KeepAlive = TimeSpan.Zero;
            Assert.AreEqual(TimeSpan.FromSeconds(1), owner.KeepAlive);
            await Initialize(owner);
            await Task.Delay(700);
            Assert.AreEqual(1, await owner.UpdateAsync(new Row { Id = 1, Value = "renewed" }));
            await Task.Delay(500);
            Assert.IsTrue(owner.HasAsyncConnection, "The first deadline must not close the renewed lifetime.");
            await WaitForIdleClose(owner);
            Assert.IsFalse(owner.IsReleased, "Idle closure must not annihilate the data owner.");
            Assert.AreEqual("renewed", (await owner.QueryAsync<Row>("SELECT * FROM records")).Single().Value);
            Assert.IsTrue(owner.HasAsyncConnection);
        }
        finally { await Cleanup(owner); }
    }

    [TestMethod]
    public async Task ExpiredWatchdogCannotCloseAnActiveOperationAndReleaseBypassesTheRenewedDelay()
    {
        var owner = Create();
        try
        {
            owner.KeepAlive = TimeSpan.FromSeconds(1);
            await Initialize(owner);
            Task<int> update;
            Task<bool> release;
            using (var blocker = new SQLiteConnection(owner.DatabasePath, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.FullMutex))
            {
                blocker.Execute("BEGIN EXCLUSIVE");
                try
                {
                    update = owner.UpdateAsync(new Row { Id = 1, Value = "accepted" });
                    await Task.Delay(1250);
                    Assert.IsFalse(update.IsCompleted, "The native lock must still hold the accepted write.");
                    Assert.IsTrue(owner.HasAsyncConnection, "An elapsed keep-alive cannot pull the rug out from active work.");
                    release = owner.ReleaseAsync();
                    Assert.IsFalse(release.IsCompleted, "Release must wait for the operation token, even after WDT expiry.");
                    Assert.AreEqual(0, await owner.UpdateAsync(new Row { Id = 1, Value = "too late" }));
                }
                finally { blocker.Execute("ROLLBACK"); }
            }
            Assert.AreEqual(1, await update.WaitAsync(TimeSpan.FromSeconds(8)));
            Assert.IsTrue(await release.WaitAsync(TimeSpan.FromSeconds(8)));
            using (var verify = new SQLiteConnection(owner.DatabasePath, SQLiteOpenFlags.ReadOnly))
                Assert.AreEqual("accepted", verify.Query<Row>("SELECT * FROM records").Single().Value);
        }
        finally { await Cleanup(owner); }

        var idle = Create();
        try
        {
            await Initialize(idle);
            var elapsed = Stopwatch.StartNew();
            Assert.IsTrue(await idle.ReleaseAsync().WaitAsync(TimeSpan.FromSeconds(3)));
            Assert.IsTrue(elapsed.Elapsed < TimeSpan.FromSeconds(3), "Explicit release must bypass the ten-second keep-alive.");
        }
        finally { await Cleanup(idle); }
    }

    [TestMethod]
    public async Task ConcurrentSynchronousAndAsynchronousRequestsUseOneExecutionBoundary()
    {
        var owner = Create();
        try
        {
            await Initialize(owner);
            var writes = Enumerable.Range(2, 80).Select(id => id % 2 == 0
                ? Task.Run(() => owner.Insert(new Row { Id = id, Value = "sync" }))
                : owner.InsertAsync(new Row { Id = id, Value = "async" })).ToArray();
            CollectionAssert.AreEqual(Enumerable.Repeat(1, 80).ToArray(), await Task.WhenAll(writes));
            var rows = owner.Query<Row>("SELECT * FROM records ORDER BY Id");
            Assert.AreEqual(81, rows.Count);
            CollectionAssert.AreEqual(Enumerable.Range(1, 81).ToArray(), rows.Select(row => row.Id).ToArray());
            Assert.AreEqual(1, await owner.DeleteAsync(rows.Last()));
            Assert.AreEqual(1, owner.Delete(rows[0]));
            Assert.AreEqual(79, (await owner.QueryAsync<Row>("SELECT * FROM records")).Count);
        }
        finally { await Cleanup(owner); }
    }

    [TestMethod]
    public async Task RecoverableFailureUsesCommonAndReleasesParticipationWithoutFaultingTheCaller()
    {
        var owner = Create();
        var signals = new List<string>();
        EventHandler<Throw> observe = (sender, e) =>
        {
            if (!ReferenceEquals(sender, owner)) return;
            signals.Add(e.MessageId);
            Assert.IsNotNull(e.Exception);
            e.Handled = true;
        };
        using (this.WithOnDispose(onInit: (_, __) => Throw.BeginThrowOrAdvise += observe,
            onDispose: (_, __) => Throw.BeginThrowOrAdvise -= observe))
        {
            try
            {
                await Initialize(owner);
                Assert.AreEqual(0, await owner.ExecuteAsync("UPDATE missing_table SET Value='no'"));
                CollectionAssert.AreEqual(new[] { "SQLitePrismaConnection.Operation" }, signals);
                Assert.AreEqual("original", owner.Query<Row>("SELECT * FROM records").Single().Value);
                Assert.IsTrue(await owner.ReleaseAsync().WaitAsync(TimeSpan.FromSeconds(3)));
            }
            finally { await Cleanup(owner); }
        }
    }
}
