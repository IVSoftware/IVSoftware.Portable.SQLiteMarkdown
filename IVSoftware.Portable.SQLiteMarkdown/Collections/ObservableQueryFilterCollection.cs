using IVSoftware.Portable.Collections;
using IVSoftware.Portable.Collections.Events;
using IVSoftware.Portable.Disposable;
using IVSoftware.Portable.Xml.Linq;
using IVSoftware.Portable.Xml.Linq.XBoundObject;
using SQLite;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace IVSoftware.Portable.SQLiteMarkdown.Collections
{
    /// <summary>
    /// Carries the actual matches accepted by one completed query publication.
    /// </summary>
    public sealed class QueryAcceptedEventArgs<T> : EventArgs
    {
        public QueryAcceptedEventArgs(IReadOnlyList<T> matches) => Matches = matches;
        public IReadOnlyList<T> Matches { get; }
    }

    /// <summary>
    /// Composes SQLiteMarkdown's state machine with one authoritative XML-modeled collection.
    /// </summary>
    /// <remarks>
    /// - The concrete collection owns input settlement, match routing and regressive clear.
    /// - QueryProvider supplies application records; the collection owns their accepted publication.
    /// - Publish selects the owner's execution context. It must execute each accepted action once.
    /// - First text clear does not invalidate an unchanged list or alter its selection.
    /// - Stored records and new-record defaults remain the application's responsibility.
    /// </remarks>
    public partial class ObservableQueryFilterCollection<T> : ObservableModeledCollection<T>, IList, ICollection<T>
    {
        public ObservableQueryFilterCollection()
        {
            context = new MarkdownContextProtected(this);
            context.PropertyChanged += OnContextChanged;
            ModelTracking |= ModelTrackingFlag.ItemQueries;
            Routes[StdRouteKey.CanonicalRoute] = () => ReadModeledItems(ModelNodesForRoute(StdRouteKey.CanonicalRoute));
            RouteKey = StdRouteKey.CanonicalRoute;
            Model.SetBoundAttributeValue(context, StdModelAttribute.mdc, "[MDC]");
        }

        private readonly MarkdownContextProtected context;
        private long revision;
        private bool replacingCanon, committing, hasAppliedTextFilter, restrictiveTextFilter, refreshingRows;
        private int observedCanonicalCount;
        private Task settlement = Task.CompletedTask;
        private TaskCompletionSource<bool>? queryRetirement;
        private string lastError = string.Empty;
        private Action<Action> publish = action => action();

        /// <summary>
        /// Supplies the application's existing query operation without assigning it interaction policy.
        /// </summary>
        public Func<string, Task<IReadOnlyList<T>>>? QueryProvider { get; set; }

        /// <summary>
        /// Publishes model changes on the retained presentation's owning execution context.
        /// </summary>
        public Action<Action> Publish
        {
            get => publish;
            set => publish = value ?? throw new ArgumentNullException(nameof(value));
        }

        public void SetPublicationContext(Action<Action> publication) => Publish = publication;
        public string InputText
        {
            get => context.InputText;
            set
            {
                value = value ?? string.Empty;
                if (value == context.InputText) return;
                SupersedePendingQuery();
                long current = revision;
                context.InputText = value;
                settlement = SettleAsync(current, inputChanged: true);
            }
        }

        public bool IsFiltering => context.IsFiltering;
        public FilteringState FilteringState => context.FilteringState;
        public SearchEntryState SearchEntryState => context.SearchEntryState;
        public bool Busy => committing || context.Busy;
        public bool CanSubmitQuery => !Busy && !IsFiltering && !InputText.IsSemanticallyEmpty()
            && context.ValidationPredicate(InputText);
        public string LastError => lastError;
        public QueryFilterConfig QueryFilterConfig { get => context.QueryFilterConfig; set => context.QueryFilterConfig = value; }
        public TimeSpan InputTextSettlingTime { get => context.InputTextSettlingTime; set => context.InputTextSettlingTime = value; }
        public uint DefaultLimit { get => context.DefaultLimit; set => context.DefaultLimit = value; }
        public Type ContractType => context.ContractType;
        public Type ProxyType => context.ProxyType;
        public string SQL => context.Query;
        public SQLiteConnection MemoryDatabase { get => context.MemoryDatabase; set => context.MemoryDatabase = value; }
        public DisposableHost DHostBusy => context.DHostBusy;
        public int CanonicalCount => Items.Count;
        public int PredicateMatchCount => context.PredicateMatchCount;
        public string Title { get; set; } = string.Empty;
        public string Placeholder => (IsFiltering ? "Filter " : "Search ") + Title;

        public event EventHandler? QueryStarting;
        public event EventHandler<QueryAcceptedEventArgs<T>>? QueryAccepted;

        /// <summary>
        /// Reports settled parser input after its model application has entered the publication context.
        /// </summary>
        /// <remarks>
        /// Await WaitForSettlementAsync when the model application itself must be complete.
        /// </remarks>
        public event EventHandler? InputTextSettled;

        /// <summary>
        /// Retires an older query after an accepted application operation supersedes its working set.
        /// </summary>
        public void SupersedePendingQuery()
        {
            ++revision;
            var retiring = queryRetirement;
            if (retiring is null) return;
            queryRetirement = null;
            committing = false;
            retiring.TrySetResult(true);
            NotifySearchState();
        }

        public Task WaitForSettlementAsync() => settlement;
        public new TaskAwaiter GetAwaiter() => WaitForSettlementAsync().GetAwaiter();

        /// <summary>
        /// Runs one valid external query or waits for the source's existing filter operation.
        /// </summary>
        public async Task CommitAsync()
        {
            if (committing) return;
            await WaitForSettlementAsync();
            if (committing || IsFiltering || !CanSubmitQuery) return;
            string expression = InputText;
            long current = ++revision;
            var retirement = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            queryRetirement = retirement;
            committing = true;
            SetError(string.Empty);
            NotifySearchState();
            try
            {
                var starting = PublishAsync(() =>
                {
                    if (current == revision) QueryStarting?.Invoke(this, EventArgs.Empty);
                });
                if (!await FinishOrRetireAsync(starting, retirement.Task) || current != revision) return;
                Task<IReadOnlyList<T>> acquisition;
                if (QueryProvider is { } provider) acquisition = provider(expression);
                else
                {
                    if (MemoryDatabase is null) throw new InvalidOperationException("Supply QueryProvider or MemoryDatabase before submitting a query.");
                    string sql = ParseSqlMarkdown<T>(expression, QueryFilterMode.Query);
                    IReadOnlyList<T> records = MemoryDatabase.Query(MemoryDatabase.GetMapping(typeof(T)), sql).Cast<T>().ToArray();
                    acquisition = Task.FromResult(records);
                }
                if (!await FinishOrRetireAsync(acquisition, retirement.Task) || current != revision) return;
                var accepted = (await acquisition).ToArray();
                var publication = PublishAsync(() =>
                {
                    if (current != revision) return;
                    LoadCanonCore(accepted);
                    QueryAccepted?.Invoke(this, new QueryAcceptedEventArgs<T>(accepted));
                });
                await FinishOrRetireAsync(publication, retirement.Task);
            }
            catch (Exception error)
            {
                if (current == revision)
                {
                    var publication = PublishAsync(() => { if (current == revision) SetError(error.Message); });
                    await FinishOrRetireAsync(publication, retirement.Task);
                }
            }
            finally
            {
                if (ReferenceEquals(queryRetirement, retirement))
                {
                    queryRetirement = null;
                    committing = false;
                    NotifySearchState();
                }
            }
        }

        private static async Task<bool> FinishOrRetireAsync(Task work, Task retired)
        {
            if (!ReferenceEquals(await Task.WhenAny(work, retired), work))
            {
                _ = ObserveRetiredAsync(work);
                return false;
            }
            await work;
            return true;
        }

        private static async Task ObserveRetiredAsync(Task work)
        {
            try { await work; }
            catch (Exception error)
            {
                // An obsolete provider failure must be observed, but cannot replace
                // the current query's state or surface an error in a later operation.
                System.Diagnostics.Trace.WriteLine("Retired query completed with " + error.GetType().Name);
            }
        }

        public void Commit() => _ = CommitAsync();

        /// <summary>
        /// Clears source-owned input first, or explicitly empties the working set without persistence writes.
        /// </summary>
        public FilteringState Clear(bool all = false)
        {
            SupersedePendingQuery();
            var previous = context.FilteringState;
            var state = context.Clear(all);
            bool terminal = all || context.SearchEntryState == SearchEntryState.Cleared
                || previous != FilteringState.Ineligible && state == FilteringState.Ineligible;
            if (terminal)
            {
                ResetPendingRows();
                hasAppliedTextFilter = restrictiveTextFilter = false;
                // An empty source has no native collection transition to publish.
                if (Items.Count != 0)
                    using (RequestAuthority(ModelDataExchangeAuthority.ModelDeferred))
                    {
                        base.Clear();
                        RouteKey = MatchRoute;
                    }
                context.AcceptClear();
            }
            else if (hasAppliedTextFilter)
            {
                using (RequestAuthority(ModelDataExchangeAuthority.ModelDeferred)) ClearTextMatches(retireLive: true);
            }
            // Retire obsolete parser publication; clearing text alone is not a list operation.
            settlement = Task.CompletedTask;
            SetError(string.Empty);
            NotifySearchState();
            return context.FilteringState;
        }

        void IList.Clear() => Clear(all: true);
        void ICollection<T>.Clear() => Clear(all: true);

        /// <summary>
        /// Admits externally materialized records without replacing canonical object identity.
        /// </summary>
        public new void LoadCanon(IList<T> records)
        {
            SupersedePendingQuery();
            LoadCanonCore(records);
        }

        private void LoadCanonCore(IList<T> records)
        {
            ResetPendingRows();
            hasAppliedTextFilter = restrictiveTextFilter = false;
            replacingCanon = true;
            try
            {
                using (RequestAuthority(ModelDataExchangeAuthority.CollectionDeferred))
                {
                    base.LoadCanon(records);
                    context.AcceptQuery(Items.Count);
                    RouteKey = MatchRoute;
                }
            }
            finally { replacingCanon = false; observedCanonicalCount = Items.Count; }
            NotifySearchState();
        }

        public new Task LoadCanonAsync(IList<T> records)
        {
            LoadCanon(records);
            return Task.CompletedTask;
        }

        public void ReplaceItems(IEnumerable<T> records) => LoadCanon(records.ToList());
        public Task ReplaceItemsAsync(IEnumerable<T> records) => LoadCanonAsync(records.ToList());
        public void InitializeFilterOnlyMode(IEnumerable<T> records)
        {
            QueryFilterConfig = QueryFilterConfig.Filter;
            LoadCanon(records.ToList());
        }

        public IDisposable BeginBusy() => context.BeginBusy();
        public string ParseSqlMarkdown() => ParseSqlMarkdown<T>(InputText, IsFiltering ? QueryFilterMode.Filter : QueryFilterMode.Query);
        public string ParseSqlMarkdown<TRecord>() => ParseSqlMarkdown<TRecord>(InputText, IsFiltering ? QueryFilterMode.Filter : QueryFilterMode.Query);
        public string ParseSqlMarkdown<TRecord>(string expression, QueryFilterMode mode = QueryFilterMode.Query)
        {
            using (FilterQueryDatabase?.RequestAuthority(SQLiteAuthority.FullControl))
                return context.ParseSqlMarkdown<TRecord>(expression, mode);
        }
        public string ParseSqlMarkdown(string expression, Type proxyType, QueryFilterMode mode, out XElement ast)
        {
            using (FilterQueryDatabase?.RequestAuthority(SQLiteAuthority.FullControl))
                return context.ParseSqlMarkdown(expression, proxyType, mode, out ast);
        }

        private async Task SettleAsync(long current, bool inputChanged)
        {
            try
            {
                await context;
                if (current != revision) return;
                var publication = PublishAsync(() =>
                {
                    if (current != revision) return;
                    bool refreshedRows = RefreshPendingRows();
                    if (!inputChanged && !refreshedRows) return;
                    if (IsFiltering && !InputText.IsSemanticallyEmpty()) ApplyTextFilter();
                    else if (hasAppliedTextFilter)
                        using (RequestAuthority(ModelDataExchangeAuthority.ModelDeferred)) ClearTextMatches(retireLive: inputChanged);
                });
                if (inputChanged) InputTextSettled?.Invoke(this, EventArgs.Empty);
                await publication;
            }
            catch (Exception error)
            {
                await PublishAsync(() => { if (current == revision) SetError(error.Message); });
            }
        }

        private Task PublishAsync(Action action)
        {
            var completion = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            try
            {
                Publish(() =>
                {
                    try { action(); completion.TrySetResult(true); }
                    catch (Exception error) { completion.TrySetException(error); }
                });
            }
            catch (Exception error) { completion.TrySetException(error); }
            return completion.Task;
        }

        private void OnContextChanged(object? sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(e);
            if (e.PropertyName is nameof(InputText) or nameof(IsFiltering) or nameof(Busy) or nameof(SearchEntryState) or nameof(FilteringState))
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(CanSubmitQuery)));
        }

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (context is not null && !HasAuthority(StdModelAuthority.Rollback)) ReconcileMembership();
            base.OnCollectionChanged(e);
        }

        private void ReconcileMembership()
        {
            int previous = observedCanonicalCount;
            observedCanonicalCount = Items.Count;
            if (replacingCanon || previous == observedCanonicalCount) return;
            SupersedePendingQuery();
            if (observedCanonicalCount == 0) context.AcceptClear();
            else context.AcceptMembership(observedCanonicalCount);
            // Membership can retire filtering without changing the retained input.
            // Settle its route before the collection publishes the resulting view.
            if (!IsFiltering && hasAppliedTextFilter)
                using (RequestAuthority(ModelDataExchangeAuthority.ModelDeferred)) ClearTextMatches(retireLive: true);
            NotifySearchState();
        }

        private void SetError(string error)
        {
            if (lastError == error) return;
            lastError = error;
            OnPropertyChanged(new PropertyChangedEventArgs(nameof(LastError)));
        }

        private void NotifySearchState()
        {
            OnPropertyChanged(new PropertyChangedEventArgs(nameof(Busy)));
            OnPropertyChanged(new PropertyChangedEventArgs(nameof(CanSubmitQuery)));
        }

        protected sealed class MarkdownContextProtected : MarkdownContext<T>
        {
            internal MarkdownContextProtected(ObservableQueryFilterCollection<T> owner)
            {
                this.owner = owner;
                base.FilterQueryDatabase = owner.FilterQueryDatabase;
            }
            private readonly ObservableQueryFilterCollection<T> owner;
            public override int CanonicalCount => owner.Items.Count;
            public override int PredicateMatchCount => owner.RouteKey is StdRouteKey.PMatch or StdRouteKey.AND ? owner.Count : 0;
            internal void AcceptQuery(int count)
            {
                Cancel();
                FilteringState = count > 1 ? FilteringState.Armed : FilteringState.Ineligible;
                SearchEntryState = count == 0 ? SearchEntryState.QueryCompleteNoResults : SearchEntryState.QueryCompleteWithResults;
            }
            internal void AcceptClear()
            {
                Clear(all: true);
            }
            internal void AcceptMembership(int count)
            {
                if (count <= 1 && FilteringState != FilteringState.Ineligible)
                {
                    Cancel();
                    FilteringState = FilteringState.Ineligible;
                }
                SearchEntryState = SearchEntryState.QueryCompleteWithResults;
            }
            internal void LinkQueryMirror() => base.FilterQueryDatabase = owner.FilterQueryDatabase;

            protected override void OnInputTextChanged()
            {
                var previous = SearchEntryState;
                base.OnInputTextChanged();
                // The scalar parser has no retained population of its own. This
                // concrete source retains that completion posture when only its
                // input is emptied, so the next Clear follows the existing FSM.
                if (InputText.Length == 0 && FilteringState == FilteringState.Ineligible
                    && (owner.Items.Count != 0 || previous == SearchEntryState.QueryCompleteNoResults))
                    SearchEntryState = owner.Items.Count == 0
                        ? SearchEntryState.QueryCompleteNoResults : SearchEntryState.QueryCompleteWithResults;
            }
        }

        protected override void OnModelTrackingChanged()
        {
            base.OnModelTrackingChanged();
            if (context is not null) context.LinkQueryMirror();
        }
    }
}
