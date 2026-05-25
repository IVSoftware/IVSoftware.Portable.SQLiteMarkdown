using IVSoftware.Portable.Collections;
using IVSoftware.Portable.Collections.Events;
using IVSoftware.Portable.Collections.Internal;
using IVSoftware.Portable.Common.Attributes;
using IVSoftware.Portable.Common.Exceptions;
using IVSoftware.Portable.SQLiteMarkdown.Collections.Preview;
using IVSoftware.Portable.SQLiteMarkdown.Events;
using IVSoftware.Portable.SQLiteMarkdown.Internal;
using IVSoftware.Portable.Xml.Linq;
using IVSoftware.Portable.Xml.Linq.XBoundObject;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using PublishedContractAttribute = IVSoftware.Portable.Common.Attributes.PublishedContractAttribute;

namespace IVSoftware.Portable.SQLiteMarkdown.Collections
{
    /// <summary>
    /// 1 of 2 partial classes in this file.
    /// Provides a query-then-filter state engine for collections of <typeparamref name="T"/>, 
    /// supporting expression-based parsing, SQLite-backed filtering, and in-memory dataset routing. 
    /// 
    /// This class is UI-agnostic but designed to work with navigable list views where a shared search
    /// bar drives both initial queries and incremental filtering. It supports both remote query 
    /// and local refinement workflows without assuming any specific platform or UI framework.
    ///
    /// Filtering is driven by attribute-decorated model properties and is internally debounced, 
    /// tracked, and stateful, exposing both query and filter readiness for external observation.
    /// </summary>
    [PublishedContract("1.x")]
    [DebuggerDisplay("Count={Count}")]
    public partial class ObservableQueryFilterSource<T>
        : MarkdownContext<T>
        , IObservableQueryFilterSource<T>
        , IModeledCollection<T>
        , IList
        , IList<T>
        where T : new()
    {
        [Canonical("The parameterless CTor is the only CTor")]
        public ObservableQueryFilterSource() 
        {
            CanonicalSupersetProtected = new ObservablePreviewRangeCollection<T>
            {
                ModelTracking = ModelTrackingFlag.ItemPropertyChanges | ModelTrackingFlag.ItemQueries
            };
        }

        protected override void OnCommit(RecordsetRequestEventArgs e)
        {
            base.OnCommit(e);
            if (e.Recordset is null)
            {
                if (MemoryDatabase is null)
                {
                    this.ThrowHard<InvalidOperationException>(
                        $"{nameof(Commit)} requires either a poulated {nameof(RecordsetRequest)} " +
                        $"or a non-null {nameof(MemoryDatabase)}.");
                    // Reachable only when Throw pattern is handled.
                    return;
                }
                else
                {
                    e.Recordset = MemoryDatabase.Query<T>(e.SQL);

                    // ☆ Pluralize Option ☆
                    if (e.Recordset.Count == 0
                        && Settings[StdMarkdownContextSetting.AllowPluralize] is bool allow && allow)
                    {
                        e.Recordset = MemoryDatabase.Query<T>(e.SQL.ToFuzzyQuery());
                    }
                }
            }

            // SeachEntryState is determined in this method in order
            // to accomodate sites that call ReplaceItems directly.
            ReplaceItems(e.Recordset.OfType<T>());
        }

        protected override void OnClear(bool all)
        {
            base.OnClear(all);
#if DEBUG
            if (CanonicalSupersetProtected.RouteKey is not null)
            {
                if (all)
                {
                    Debug.Assert(
                        Equals(CanonicalSupersetProtected.RouteKey, StdRouteKey.CanonicalRoute),
                        $"Expecting the collection route is nullified.");
                    Debug.Assert(
                        CanonicalSuperset.Count == 0,
                        $"Expecting the collection route to read as empty.");
                }
            }
#endif
        }

        /// <summary>
        /// Routing is a wrapper on CSP.
        /// </summary>
        protected override void OnRouteKeyChanged()
        {
            base.OnRouteKeyChanged();

            switch (RouteKey)
            {
                case StdRouteKey.CanonicalRoute:
                    using (RequestAuthority(ModelDataExchangeAuthority.CollectionDeferred))
                    {
                        CanonicalSupersetProtected.RouteKey = base.RouteKey;
                    }
                    break;
                default:
                    CanonicalSupersetProtected.RouteKey = base.RouteKey;
                    break;
            }
        }

        /// <summary>
        /// Maps affirmative state change to CSP Clear.
        /// </summary>
        protected override void OnSearchEntryStateChanged()
        {
            base.OnSearchEntryStateChanged();
            switch (SearchEntryState)
            {
                case SearchEntryState.Cleared:
                    // Authority DNC: "May or may not" have token.
                    CanonicalSupersetProtected.Clear();
                    break;
                default:
                    // TBD
                    break;
            }
        }

        /// <summary>
        /// Maps config to CSP.ModelTracking.
        /// </summary>
        protected override void OnQueryFilterConfigChanged()
        {
            base.OnQueryFilterConfigChanged();
            if(QueryFilterConfig.HasFlag(QueryFilterConfig.Filter))
            { 
                CanonicalSupersetProtected.ModelTracking |= ModelTrackingFlag.ItemQueries;
            }
            else 
            { 
                CanonicalSupersetProtected.ModelTracking &= ~ModelTrackingFlag.ItemQueries;
            }
        }
        public override int PredicateMatchCount =>
            Equals(RouteKey, StdRouteKey.CanonicalRoute)
            ? 0
            : Count;    // Routed
    }

    /// <summary>
    /// 2 of 2 partial classes in this file.
    /// </summary>
    partial class ObservableQueryFilterSource<T>
        : IObservableQueryFilterSource
        , IObservableQueryFilterSource<T>
    {
        public string Placeholder =>
            IsFiltering
            ? $"Filter {Title}"
            : $"Search {Title}";

        public string Title
        {
            get;
            set;
        } = string.Empty;

        public string SQL => base.Query;

        public void InitializeFilterOnlyMode(IEnumerable<T> items)
        {
            this.RethrowFramework(new NotSupportedException());
        }

        /// <summary>
        /// Forward to the more accurately named LoadCanon of the CSP.
        /// </summary>
        /// <remarks>
        /// Call it what you will - this is typically a Recordset push.
        /// </remarks>
        public void ReplaceItems(IEnumerable<T> items)
        {
            CanonicalSupersetProtected.LoadCanon(items.ToList());
            SearchEntryState =
                CanonicalSuperset.Count == 0
                ? SearchEntryState.QueryCompleteNoResults
                : SearchEntryState.QueryCompleteWithResults;
        }

        public async Task ReplaceItemsAsync(IEnumerable<T> items)
        {
            await OnReplaceItemsAsync(items);
        }

        /// <summary>
        /// Provides an asynchronous entry point for replacing the collection.
        /// </summary>
        /// <remarks>
        /// The default implementation offloads ReplaceItems to a background
        /// thread using Task.Run.
        ///
        /// Override to supply a custom scheduling strategy, integrate with an
        /// existing async pipeline, or coordinate with UI/thread affinity
        /// requirements.
        ///
        /// Implementations should preserve the atomic "replace canon" semantic
        /// of ReplaceItems and avoid interleaving partial updates.
        /// </remarks>
        protected virtual Task OnReplaceItemsAsync(IEnumerable<T> items)
            => Task.Run(() => ReplaceItems(items));


        [Obsolete("Use CanonicalSuperset for precise semantics.")]
        public IReadOnlyList<T> UnfilteredItems => CanonicalSuperset;

        [Obsolete("Legacy unit test support only.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public MarkdownContextOR MarkdownContextOR
        {
            get
            {
                var searchEntryState = SearchEntryState;
                return Extensions.ParseSqlMarkdown<T>(InputText, ref searchEntryState);
            }
        }

        [PublishedContract("1.x")]
        public new Type ProxyType => base.ProxyType;

        [PublishedContract("1.x")]
        public new string Query => base.Query;

        public override string ToString(Enum formatting)
            => ToString(formatting, []);
        public virtual string ToString(Enum formatting, object[] args)
        {
            switch (formatting)
            {
                case FormattingOMC.StateReport:
                    return this.StateReport();
                case FormattingEHM.Matches:
                    return CanonicalSupersetProtected.ToString(FormattingEHM.Matches);
                case FormattingOMC.ModelWithPreview:
                    if (args.FirstOrDefault() is int previewLength)
                    {
                        return Model.CloneWithXBindings(previewLength).ToString();
                    }
                    else
                    {
                        return Model.CloneWithXBindings(previewLength: 10).ToString();
                    }
                default:
                    return base.ToString();
            }
        }
        protected override void OnPropertyChanged(PropertyChangedEventArgs eUnk)
        {
            base.OnPropertyChanged(eUnk);
            switch (eUnk)
            {
                case EHPropertyChangedEventArgs e:
                    localOnEHPropertyChanged(e);
                    break;
                default:
                    localOnDefaultPropertyChanged(eUnk);
                    break;
            }
            #region L o c a l F x
            void localOnEHPropertyChanged(EHPropertyChangedEventArgs e)
            {
                switch (e.Key)
                {
                    case StdModelAttribute.model:
                        if (QueryFilterConfig.HasFlag(QueryFilterConfig.Filter))
                        {
                            // Look for a combination of:
                            // model
                            // + Filter flag
                            // + Increment
                            // + !Canon
                            if (!HasAuthority(StdModelAuthority.Canon))
                            {
                                // Canonical collection is being modified out-of-band.
                                if (Equals(e.PropertyName, nameof(HistogramEdge.Increment)))
                                {
                                    // Designated out-of-band match until a new canonical recordset becomes available.
                                    if (e.XOB is XBoundAttribute xba
                                        && xba.Name.LocalName == nameof(StdModelAttribute.model))
                                    {
                                        xba.Parent?.SetStdAttributeValue(StdModelAttribute.live, bool.TrueString);
                                    }
                                }
                                int itemCount = Histo[StdModelAttribute.model];
                                switch (itemCount)
                                {
                                    case 0:
                                        SearchEntryState = SearchEntryState.QueryCompleteNoResults;
                                        FilteringState = FilteringState.Ineligible;
                                        break;
                                    case 1:
                                    default:
                                        SearchEntryState = SearchEntryState.QueryCompleteWithResults;
                                        if (itemCount == 1)
                                        {
                                            FilteringState = FilteringState.Ineligible;
                                        }
                                        else
                                        {
                                            // [Probationary]
                                            // Does this need to sync up with IME state?
                                            FilteringState = FilteringState.Armed;
                                        }
                                        break;
                                }
                            }
                        }
                        break;
                    case StdModelAttribute.live:
                        /* G T K */
                        break;
                }
            }
            void localOnDefaultPropertyChanged(PropertyChangedEventArgs e)
            {
                switch (e.PropertyName)
                {
                    case nameof(ModelTracking):
                        // Raised by CSP not the base class.
                        // ∴ Wire it here.
                        if (ModelTracking.HasFlag(ModelTrackingFlag.ItemQueries))
                        {
                            QueryFilterConfig |= QueryFilterConfig.Filter;
                        }
                        else
                        {
                            QueryFilterConfig &= ~QueryFilterConfig.Filter;
                        }
                        break;
                }
            }
            #endregion L o c a l F x
        }
    }
}
