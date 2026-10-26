using IVSoftware.Portable.Collections;
using IVSoftware.Portable.Collections.Events;
using IVSoftware.Portable.Xml.Linq;
using IVSoftware.Portable.Xml.Linq.XBoundObject;
using SQLite;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace IVSoftware.Portable.SQLiteMarkdown.Collections
{
    partial class ObservableQueryFilterCollection<T>
    {
        protected override StdRouteKey MatchRoute =>
            MarkdownContext.QueryIsRestrictive == true && Histo[StdModelAttribute.model] > 1
                ? Histo[StdModelAttribute.pmatch] > 0 || HasExplicitEmptyPredicateRoute ? StdRouteKey.AND : StdRouteKey.QMatch
                : base.MatchRoute;

        void RefreshTextMatchRoute() => RouteKey = MatchRoute;

        static readonly IReadOnlyDictionary<string, bool> FilterContentProperties = typeof(T)
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .GroupBy(property => property.Name)
            .ToDictionary(group => group.Key, group => group.Any(property =>
                property.GetCustomAttribute<FilterLikeTermAttribute>() is not null
                || property.GetCustomAttribute<TagMatchTermAttribute>() is not null
                || property.GetCustomAttribute<SelfIndexedAttribute>() is { } indexed
                    && (indexed.IndexingMode & (IndexingMode.FilterLikeTerm | IndexingMode.TagMatchTerm)) != 0));

        /// <summary>
        /// Grants editing continuity without letting a participating predicate property grant immunity.
        /// </summary>
        /// <remarks>
        /// The established item-notification channel owns this decision. A visible edit
        /// to filterable content grants live immediately. The base predicate owner then
        /// handles participating edits by updating pmatch, without consuming existing live.
        /// SQLite/indexed terms settle afterward; no new item subscription or timer is added.
        /// </remarks>
        void HandleRecordEdit(ItemPropertyChangedEventArgs edit)
        {
            if (edit.Item is not T item || MarkdownContext.RefreshingRows) return;
            var nodes = Model.Descendants().Where(node =>
                ReferenceEquals((node.Attribute("model") as XBoundAttribute)?.Tag, item)).ToArray();
            if (nodes.Length == 0) return;

            if (MarkdownContext.HasAppliedTextFilter && !ParticipatesInActivePredicate(edit.PropertyName!)
                && FilterContentProperties.TryGetValue(edit.PropertyName!, out var content) && content)
                PreserveVisibleRecordEdit(item);

            if (FilterQueryDatabase is not null)
                foreach (var node in nodes) MarkdownContext.QueueRecordRefresh(node, item);
        }

        /// <summary>
        /// Applies explicit filter-request retirement through the existing model epoch.
        /// </summary>
        void RetireTextFilterVisibility() => RetireLiveVisibility();

        protected partial class MarkdownContextProtected
        {
            // Pending work is keyed by its original XML binding. It is not another
            // working set. Replaced or detached bindings cannot be written back later.
            readonly Dictionary<XElement, T> pendingRows = new();
            readonly object pendingGate = new();
            bool inputPending;
            string? appliedFilterInput;
            public bool RefreshingRows { get; private set; }
            public bool HasAppliedTextFilter => appliedFilterInput is { Length: > 0 };
            public bool? QueryIsRestrictive { get; private set; }

            public void QueueRecordRefresh(XElement node, T item)
            {
                lock (pendingGate) pendingRows[node] = item;
                StartOrRestart();
            }

            public void ResetLiveFilterState()
            {
                lock (pendingGate) pendingRows.Clear();
                appliedFilterInput = null;
                QueryIsRestrictive = null;
                inputPending = false;
            }

            protected override void OnInputTextChanged()
            {
                inputPending = IsFiltering && QueryFilterConfig.HasFlag(QueryFilterConfig.Filter);
                if (HasAppliedTextFilter && InputText.IsSemanticallyEmpty()
                    && !@this.HasAuthority(StdModelAuthority.Canon) && !@this.HasAuthority(StdModelAuthority.Rollback))
                {
                    using (@this.RequestAuthority(ModelDataExchangeAuthority.ModelDeferred))
                    {
                        @this.RetireTextFilterVisibility();
                        @this.Model.Descendants().Attributes("qmatch").ToArray().Remove();
                        appliedFilterInput = null;
                        QueryIsRestrictive = null;
                        base.OnInputTextChanged();
                        @this.RefreshTextMatchRoute();
                    }
                }
                else base.OnInputTextChanged();

                if (InputText.IsSemanticallyEmpty())
                {
                    // Clearing text may cancel its timer, but an already accepted row
                    // edit still needs settlement. Drain that work without inventing
                    // another text request or attempting to parse an empty expression.
                    inputPending = false;
                    bool hasPendingRows;
                    lock (pendingGate) hasPendingRows = pendingRows.Count != 0;
                    if (hasPendingRows) StartOrRestart();
                }
            }

            /// <summary>
            /// Refreshes accepted record edits before filtering, without fabricating a new text request.
            /// </summary>
            /// <remarks>
            /// Self-indexed records finish invalidating their derived terms after INPC.
            /// The existing watchdog boundary reads those final values and updates only
            /// still-bound rows. Same-filter refresh keeps live protection and does not
            /// raise InputTextSettled as though the user had typed another expression.
            /// </remarks>
            protected override async Task OnInputTextSettled(CancelEventArgs e)
            {
                KeyValuePair<XElement, T>[] pending;
                lock (pendingGate) { pending = pendingRows.ToArray(); pendingRows.Clear(); }
                bool isInput = inputPending;
                inputPending = false;
                RefreshingRows = true;
                try
                {
                    if (@this.FilterQueryDatabase is { } database)
                        using (database.RequestAuthority(SQLiteAuthority.FullControl))
                            foreach (var edit in pending)
                                if (edit.Key.Ancestors().Any(node => ReferenceEquals(node, @this.Model))
                                    && ReferenceEquals((edit.Key.Attribute("model") as XBoundAttribute)?.Tag, edit.Value))
                                    database.Update(edit.Value);
                }
                finally { RefreshingRows = false; }

                if (isInput || pending.Length == 0) await base.OnInputTextSettled(e);
                else if (!e.Cancel && HasAppliedTextFilter && !InputText.IsSemanticallyEmpty()) await ApplyFilter();
            }

            protected override async Task ApplyFilter()
            {
                var requested = InputText.TrimEndTransients().TrimEnd();
                using (@this.RequestAuthority(ModelDataExchangeAuthority.ModelDeferred))
                {
                    if (appliedFilterInput != requested) @this.RetireTextFilterVisibility();
                    await base.ApplyFilter();
                    appliedFilterInput = requested;
                }
            }

            protected override void OnFilterMatchesApplied(int canonicalCount, int queryMatchCount)
            {
                base.OnFilterMatchesApplied(canonicalCount, queryMatchCount);
                QueryIsRestrictive = queryMatchCount < canonicalCount;
                @this.RefreshTextMatchRoute();
            }
        }
    }
}
