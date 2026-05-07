using IVSoftware.Portable.Collections;
using IVSoftware.Portable.Common.Attributes;
using IVSoftware.Portable.Common.Exceptions;
using IVSoftware.Portable.SQLiteMarkdown.Events;
using IVSoftware.Portable.SQLiteMarkdown.Internal;
using IVSoftware.Portable.Xml.Linq.XBoundObject;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        , IList
        , IList<T>
        where T : new()
    {
        [Canonical("The parameterless CTor is the only CTor")]
        public ObservableQueryFilterSource() { }

        protected override void OnCommit(RecordsetRequestEventArgs e)
        {
            base.OnCommit(e);
            if (!e.Handled)
            {
                var recordset = MemoryDatabase.Query<T>(e.SQL);
                if( recordset.Count == 0
                    && Settings[StdMarkdownContextSetting.AllowPluralize] is bool allow && allow)
                {
                    recordset = MemoryDatabase.Query<T>(e.SQL.ToFuzzyQuery());
                }
                ReplaceItems(recordset);
                if(CanonicalSuperset.Count == 0)
                {
                    SearchEntryState = SearchEntryState.QueryCompleteNoResults;
                }
                else
                {
                    SearchEntryState = SearchEntryState.QueryCompleteWithResults;
                }
            }
        }
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

        public void ReplaceItems(IEnumerable<T> items)
        {
            CanonicalSupersetProtected.LoadCanon((IList)items);
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

        public void SetObservableNetProjection(
            ObservableCollection<T>? onp,
            NetProjectionTopology? topology = null)
        {
            this.RethrowFramework(new NotSupportedException());
        }

        [PublishedContract("1.x")]
        public override bool RouteToFullRecordset
        {
            get
            {
                switch (FilteringState)
                {
                    case FilteringState.Ineligible:
                    case FilteringState.Armed:
                        return true;
                    case FilteringState.Active:
                        if (0 == CanonicalSupersetProtected.Histo[StdModelAttribute.match])
                        {
                            return Equals(Settings[StdMarkdownContextSetting.UseAdaptiveShowAll], true);
                        }
                        else return false;
                    default:
                        this.ThrowFramework<NotSupportedException>(
                            $"The {FilteringState.ToFullKey()} case is not supported.");
                        return true;
                }
            }
        }

        [Obsolete("Use CanonicalSuperset for precise semantics.")]
        public IReadOnlyList<T> UnfilteredItems => CanonicalSupersetProtected;

        [Obsolete("Legacy unit test support only.")]
        public MarkdownContextOR MarkdownContextOR
        {
            get
            {
                var searchEntryState = SearchEntryState;
                return Extensions.ParseSqlMarkdown<T>(InputText, ref searchEntryState);
            }
        }

        [Obsolete("Backward compatibility only.")]
        public new Type ProxyType => base.ProxyType;

        [Obsolete("Backward compatibility only.")]
        public new string Query => base.Query;

        public override string ToString(Enum formatting)
        {
            switch (formatting)
            {
                case FormattingOMC.StateReport:
                    return this.StateReport();
                case FormattingEHM.Matches:
                    return CanonicalSupersetProtected.ToString(FormattingEHM.Matches);
                default:
                    return base.ToString();
            }
        }
    }
}
