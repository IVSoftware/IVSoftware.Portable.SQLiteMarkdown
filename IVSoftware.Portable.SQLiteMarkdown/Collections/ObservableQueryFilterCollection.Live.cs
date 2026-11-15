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
using System.Xml.Linq;

namespace IVSoftware.Portable.SQLiteMarkdown.Collections
{
    public partial class ObservableQueryFilterCollection<T>
    {
        private readonly Dictionary<XElement, T> pendingRows = new Dictionary<XElement, T>();
        private readonly object pendingGate = new object();
        private string? appliedFilterInput;

        protected override bool HasActiveVisibilityFilter => hasAppliedTextFilter || base.HasActiveVisibilityFilter;

        protected override StdRouteKey MatchRoute => hasAppliedTextFilter && restrictiveTextFilter
            ? base.MatchRoute is StdRouteKey.PMatch or StdRouteKey.AND ? StdRouteKey.AND : StdRouteKey.QMatch
            : base.MatchRoute;

        /// <summary>
        /// Reads the model's match route with collapsed ancestors excluded from presentation.
        /// </summary>
        public IEnumerable<T> VisibleHierarchy() => ReadModeledItems(ModelNodesForRoute(MatchRoute));

        protected override IEnumerable<T> ReadModeledItems(IEnumerable<XElement> nodes) =>
            base.ReadModeledItems(nodes.Where(node => !node.Ancestors().Any(ancestor => (bool?)ancestor.Attribute("collapsed") == true)));

        private IEnumerable<KeyValuePair<XElement, T>> BoundRecords()
        {
            foreach (var node in Model.Descendants().Where(node => node.Attribute("model") is XBoundAttribute))
                foreach (var record in base.ReadModeledItems(new[] { node }))
                    yield return new KeyValuePair<XElement, T>(node, record);
        }

        private void ApplyTextFilter()
        {
            var database = FilterQueryDatabase;
            if (database is null) return;
            string requested = InputText.TrimEndTransients().TrimEnd();
            string sql = ParseSqlMarkdown<T>(InputText, QueryFilterMode.Filter);
            var mapping = database.GetMapping(typeof(T));
            if (mapping.PK is null) throw new InvalidOperationException("A filtered record requires a primary-key mapping.");
            var matches = new HashSet<object>(database.Query(mapping, sql).Select(row => mapping.PK.GetValue(row)));
            var records = BoundRecords().ToArray();
            using (RequestAuthority(ModelDataExchangeAuthority.ModelDeferred))
            {
                if (!hasAppliedTextFilter || appliedFilterInput != requested) RetireLiveVisibility();
                appliedFilterInput = requested;
                hasAppliedTextFilter = true;
                restrictiveTextFilter = matches.Count < Items.Count;
                foreach (var record in records)
                    record.Key.SetAttributeValue("qmatch", matches.Contains(mapping.PK.GetValue(record.Value)) ? true : (bool?)null);
                RouteKey = MatchRoute;
            }
        }

        private void ClearTextMatches(bool retireLive)
        {
            if (retireLive) RetireLiveVisibility();
            hasAppliedTextFilter = restrictiveTextFilter = false;
            appliedFilterInput = null;
            Model.Descendants().Attributes("qmatch").ToArray().Remove();
            RouteKey = MatchRoute;
        }

        private static readonly HashSet<string> FilterContentProperties = new HashSet<string>(typeof(T)
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where(property => property.GetCustomAttribute<FilterLikeTermAttribute>() is not null
                || property.GetCustomAttribute<TagMatchTermAttribute>() is not null
                || property.GetCustomAttribute<SelfIndexedAttribute>() is { } indexed
                    && (indexed.IndexingMode & (IndexingMode.FilterLikeTerm | IndexingMode.TagMatchTerm)) != 0)
            .Select(property => property.Name));

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (context is not null && e is ItemPropertyChangedEventArgs edit && edit.Item is T record
                && !refreshingRows && FilterContentProperties.Contains(edit.PropertyName!))
                QueueEditedRecord(record, edit.PropertyName);
            base.OnPropertyChanged(e);
        }

        /// <summary>
        /// Refreshes an accepted canonical edit through the existing query mirror and settlement boundary.
        /// </summary>
        public void RefreshEditedRecord(T record) => QueueEditedRecord(record, propertyName: null);

        private void QueueEditedRecord(T record, string? propertyName)
        {
            var nodes = BoundRecords().Where(pair => ReferenceEquals(pair.Value, record)).ToArray();
            if (nodes.Length == 0) return;
            if (hasAppliedTextFilter && (propertyName is null || !ParticipatesInActivePredicate(propertyName)))
                PreserveVisibleRecordEdit(record);
            lock (pendingGate)
                foreach (var pair in nodes) pendingRows[pair.Key] = record;
            context.StartOrRestart();
            settlement = SettleAsync(revision, inputChanged: false);
        }

        private void ResetPendingRows()
        {
            lock (pendingGate) pendingRows.Clear();
        }

        private bool RefreshPendingRows()
        {
            KeyValuePair<XElement, T>[] pending;
            lock (pendingGate) { pending = pendingRows.ToArray(); pendingRows.Clear(); }
            if (pending.Length == 0 || FilterQueryDatabase is not { } database) return false;
            refreshingRows = true;
            try
            {
                using (database.RequestAuthority(SQLiteAuthority.FullControl))
                    foreach (var pair in pending)
                        if (pair.Key.Ancestors().Any(node => ReferenceEquals(node, Model))
                            && base.ReadModeledItems(new[] { pair.Key }).Any(current => ReferenceEquals(current, pair.Value)))
                            database.Update(pair.Value);
            }
            finally { refreshingRows = false; }
            return true;
        }
    }
}
