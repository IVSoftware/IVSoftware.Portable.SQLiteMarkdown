using IVSoftware.Portable.Common.Attributes;
using IVSoftware.Portable.Common.Exceptions;
using IVSoftware.Portable.SQLiteMarkdown.Common;
using IVSoftware.Portable.SQLiteMarkdown.Events;
using IVSoftware.Portable.Threading;
using IVSoftware.Portable.Xml.Linq;
using IVSoftware.Portable.Xml.Linq.XBoundObject;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using IVSoftware.Portable.SQLiteMarkdown.Internal;

namespace IVSoftware.Portable.SQLiteMarkdown.Collections
{
    /// <summary>
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

    [DebuggerDisplay("Count={Count}")]
    public partial class ObservableQueryFilterSource<T>
        : MarkdownContext<T>
        , IObservableQueryFilterSource<T>
        , IList<T>
        where T : new()
    {
        #region N O N    O B S E R V A B L E    S O U R C E S
        /// <summary>
        /// The IEnumerable<T> collection when IsFiltering = false.
        /// </summary>
        private readonly IList<T> _canonicalRecordset = new List<T>();
        /// <summary>
        /// The IEnumerable<T> collection when IsFiltering = true.
        /// </summary>
        private readonly IList<T> _predicateMatchSubset = new List<T>();
        #endregion N O N    O B S E R V A B L E    S O U R C E S

        public ObservableQueryFilterSource()
        {
            CanonicalRecordset = new ReadOnlyCollection<T>(_canonicalRecordset);
            base.ObservableNetProjection = this;
            base.ProjectionOption = NetProjectionOption.ObservableOnly;

#if false
            _canonicalRecordset.CollectionChanged += (sender, e) =>
            {
                if (DHostAuthorityEpoch.Authority == CollectionChangeAuthority.MarkdownContext)
                {   /* G T K - N O O P */
                }
                else
                {
                    if (_canonicalRecordset.Count < 2)
                    {
                        // 260301
                        // FilteringState = FilteringState.Ineligible;
                    }
                    switch (e.Action)
                    {
                        case NotifyCollectionChangedAction.Add:
                            foreach (var inpc in e.NewItems?.OfType<INotifyPropertyChanged>() ?? [])
                            {
                                inpc.PropertyChanged += OnItemPropertyChanged;
                            }
                            if (Busy)
                            {   /* G T K */
                            }
                            else
                            {
                                if (FilteringState != FilteringState.Active)
                                {
                                    OnCollectionChanged(e);
                                }
                            }
                            break;
                        case NotifyCollectionChangedAction.Remove:
                            foreach (var inpc in e.OldItems?.OfType<INotifyPropertyChanged>() ?? [])
                            {
                                inpc.PropertyChanged -= OnItemPropertyChanged;
                            }
                            if (Busy)
                            {   /* G T K */
                            }
                            else
                            {
                                OnCollectionChanged(e);
                            }
                            break;
                        case NotifyCollectionChangedAction.Reset:
                            _predicateMatchSubset.Clear();
                            foreach (var inpc in _unsubscribeItems)
                            {
                                inpc.PropertyChanged -= OnItemPropertyChanged;
                            }
                            OnCollectionChanged(e);
                            break;
                    }
                    _unsubscribeItems = _canonicalRecordset.OfType<INotifyPropertyChanged>().ToArray();
                }
            };
#endif
        }


        public IReadOnlyList<T> CanonicalRecordset { get; }


        [Obsolete("Use CanonicalRecordset and PredicateMatchSubset for precise semantics.")]
        public IReadOnlyList<T> UnfilteredItems => CanonicalRecordset;

        public virtual async Task ReplaceItemsAsync(IEnumerable<T> items)
        {
            Debug.WriteLine($@"260306.a ADVISORY - {nameof(ReplaceItemsAsync)}.");
            using (DHostBusy.GetToken())
            {
                await Task.Run(() =>
                {
                    base.LoadCanon(items);
                });

                internalLoadONPfromModel();
            }
        }

        /// <summary>
        /// Removes any current items before copying the items passed.
        /// </summary>
        /// <remarks>
        /// Mental Model: "I have a new recordset from my external (e.g., cloud) database."
        /// On completeion SearchEntryState will always be either QueryCompleteNoResults or QueryCompleteWithResults.
        /// </remarks>
        public virtual void ReplaceItems(IEnumerable<T> items)
        {
            using (DHostBusy.GetToken())
            {
                // --------------
                // UPGRADE 260301
                base.LoadCanon(items);
                // --------------

                switch (ProjectionOption)
                {
                    case NetProjectionOption.ObservableOnly:
                        internalLoadONPfromModel();
                        break;
                    case NetProjectionOption.AllowDirectChanges:
                        break;
                    default:
                        this.ThrowHard<NotSupportedException>($"The {ProjectionOption.ToFullKey()} case is not supported.");
                        break;
                }
            }
        }


        /// <summary>
        /// Applies filtering based on incremental changes to the input text,
        /// occurring after the initial query. Operates on the in-memory SQLite store
        /// when in QueryAndFilter or Filter mode. Override to customize filter behavior.
        /// </summary>
        protected override async Task ApplyFilter()
        {
            using (DHostBusy.GetToken())
            {
                await base.ApplyFilter();
                try
                {
                    Debug.Assert(IsFiltering);
                    if (InputText.Length == 0)
                    {
                        // When we're filtering and go to 0 length, we show ALL the items.
                        switch (FilteringState)
                        {
                            case FilteringState.Ineligible:
                                break;
                            case FilteringState.Armed:
                                break;
                            case FilteringState.Active:
                                FilteringState = FilteringState.Armed;
                                break;
                            default:
                                throw new NotImplementedException($"Bad case: {FilteringState}");
                        }
                    }
                    else
                    {
                        switch (FilteringState)
                        {
                            case FilteringState.Ineligible:
                                break;
                            case FilteringState.Armed:
                                FilteringState = FilteringState.Active;
                                break;
                            case FilteringState.Active:
                                break;
                            default:
                                throw new NotImplementedException($"Bad case: {FilteringState}");
                        }

                        var searchEntryState = SearchEntryState;
                        var sql = ParseSqlMarkdown<T>();
                        // Must have "where" and must have at least 1 non whitespace char after it.
                        if (Regex.IsMatch(sql ?? "", @"where\s+\S", RegexOptions.IgnoreCase))
                        {

                        }
                        else
                        {
                            throw new InvalidOperationException($"Expected WHERE clause with content. Parse result was:\n{sql}");
                        }

#if false && DEBUG && SAVE
                        var context = InputText.ParseSqlMarkdown<T>(ref searchEntryState);
                        var cstring = context.ToString();
                        if (sql == cstring)
                        {
                        }
                        else
                        {
                            // Probably 'not' the same so far. Here's what we need to happen:
                            // - Using the string extension is stand-alone and always makes a new context.
                            // - Going forward, the string extension should support only expr (a.k.a. @this), QueryFilterMode, and Minimum Length
                            // - We're done with passing state into the string extension, however. If state is what you want, maintain a context.
                            // - If you want, you can pull that context off the 'out XElement' from the first string call. But honestly this is more intended to be a test feature.
                        }
#endif

                        var filteredRecords = FilterQueryDatabase.Query<T>(sql);

                        // This is 'not' the place for a reconciled sync.
                        // We would do that in the UI if at all.
                        _predicateMatchSubset.Clear();
                        foreach (var item in filteredRecords)
                        {
                            _predicateMatchSubset.Add(item);
                        }
                        // Active REGARDLESS of result because if unfiltered
                        // count < 2 we're not supposed to be here in the first place.
                        Debug.Assert(_canonicalRecordset.Count >= 2, "ADVISORY - Filterable source is required.");
                        FilteringState = FilteringState.Active;
                        OnCollectionChanged(
                            new NotifyQueryFilterCollectionChangedEventArgs(
                                NotifyQueryFilterCollectionChangedAction.ApplyFilter,
                                _predicateMatchSubset.ToList() // snapshot
                            )
                        );
                    }
                }
                finally
                {
                    lock (_lock)
                    {
                        this.OnAwaited();
                    }
                }
            }
        }

        protected override void OnOutgoingCollectionChangedRequest(NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    break;
                case NotifyCollectionChangedAction.Move:
                    break;
                case NotifyCollectionChangedAction.Remove:
                    break;
                case NotifyCollectionChangedAction.Replace:
                    break;
                case NotifyCollectionChangedAction.Reset:
                    break;
            }
            var canonicalItems =
                Model
                .Descendants().Select(_ => _.To<T>())
                .OfType<T>()
                .ToArray();
            _canonicalRecordset.Clear();
            foreach (var item in canonicalItems)
            {
                _canonicalRecordset.Add(item);
            }

            Debug.Assert(
                DHostAuthorityEpoch.Authority == CollectionChangeAuthority.MarkdownContext,
                "Expecting protected operation.");

            base.OnOutgoingCollectionChangedRequest(e);
            CollectionChanged?.Invoke(this, e);
        }

        private void internalLoadONPfromModel()
        {
            return;
            var canonicalItems =
                Model
                .Descendants().Select(_ => _.To<T>())
                .OfType<T>()
                .ToArray();
            using(base.BeginCollectionChangeAuthority(authority: CollectionChangeAuthority.None))
            {
                _canonicalRecordset.Clear();
                foreach (var item in canonicalItems)
                {
                    _canonicalRecordset.Add(item);
                }
            }

#if false
            // RESET REQUIRED main INCC for NetProjection.
            // Unit Tests expect this, but it can't be left outside of the authority OTHERWISE MODEL GETS CLEARED.
            // #{4E778EBA-D838-48D0-89D6-3D1FC8229E23}

            // _canonicalRecordset.Clear(); // <- NOT HERE!

            using (base.BeginCollectionChangeAuthority(authority: CollectionChangeAuthority.MarkdownContext))
            using (base.BeginResetEpoch())
            {
                // Building from the model in V2 is new.

                _canonicalRecordset.Clear(); // <- HERE!
                if (CanonicalCount != 0)
                {
                    foreach (var xel in Model.Descendants())
                    {
                        if (xel.To<T>() is { } item)
                        {
                            _canonicalRecordset.Add(item);
                        }
                    }

                    // Raise single event after completing the loop.
                    OnCollectionChanged(
                        new NotifyQueryFilterCollectionChangedEventArgs(
                            NotifyQueryFilterCollectionChangedAction.QueryResult | NotifyQueryFilterCollectionChangedAction.Add,
                            _canonicalRecordset.ToList() // snapshot as IList
                        )
                    );
                }
            }
#endif
        }

        /// <summary>
        /// Sets Filter-only mode and initializes the dataset for filtering.
        /// Ideal for static lists (e.g., preferences, enums).
        /// When a filter is "cleared" it means the collection view returns to "all items visible".
        /// </summary>
        [PublishedContract("1.0")]
        public void InitializeFilterOnlyMode(IEnumerable<T> items)
        {
            QueryFilterConfig = QueryFilterConfig.Filter;
            LoadCanon(items);
        }

        #region I L I S T
        public int IndexOf(T item) { return _canonicalRecordset.IndexOf(item); }

        void IList.Clear() => Clear(all: true);
        void ICollection<T>.Clear() => Clear(all: true);

        /// <summary>
        /// "No Suprises" clear on an IList.
        /// </summary>
        /// <remarks>
        /// Collections that inherit MarkdownContext *must* distinguish clear semantics.
        /// Subclass should implement both:
        /// 1. The parameterless "no surprises" Clear().
        /// 2. The UI-oriented [X] demoting clear state machine.
        /// </remarks>
        [Canonical("#{5932CB31-B914-4DE8-9457-7A668CDB7D08}")]
        public void Clear() => base.Clear(all: true);

        public new FilteringState Clear(bool all = false)
        {
            var fsBase = base.Clear(all);
            if (fsBase < FilteringState.Armed)
            {
                // [Careful] 
                // If we're responding to FilteringState changed to clear the
                // canonical recordset it MIGHT NOT WORK. For example, manual
                // add-remove changes to Items will bypass the input state machine. 
                using (BeginCollectionChangeAuthority(CollectionChangeAuthority.NetProjection))
                {
                    _canonicalRecordset.Clear();
                }
            }
            return fsBase;
        }

        public bool Contains(T item) { return _canonicalRecordset.Contains(item); }

        public void CopyTo(T[] array, int arrayIndex) { _canonicalRecordset.CopyTo(array, arrayIndex); }

        bool IList.Contains(object value) { return ((IList)_canonicalRecordset).Contains(value); }

        int IList.IndexOf(object value) { return ((IList)_canonicalRecordset).IndexOf(value); }
        public void Insert(int index, T item)
        {
            using (BeginCollectionChangeAuthority(CollectionChangeAuthority.NetProjection))
            {
                _canonicalRecordset.Insert(index, item);
            }
            OnExternalChange(item);
        }

        public void Add(T item)
        {
            using (BeginCollectionChangeAuthority(CollectionChangeAuthority.NetProjection))
            {
                _canonicalRecordset.Add(item);
            }
            OnExternalChange(item);
        }
        public void RemoveAt(int index)
        {
            object? item;
            if (index < _canonicalRecordset.Count)
            {
                item = _canonicalRecordset[index];
            }
            else
            {
                item = null;
            }
            using (BeginCollectionChangeAuthority(CollectionChangeAuthority.NetProjection))
            {
                _canonicalRecordset.RemoveAt(index);
            }
            OnExternalChange(item);
        }

        int IList.Add(object item)
        {
            if (item is T itemT)
            {
                using (BeginCollectionChangeAuthority(CollectionChangeAuthority.NetProjection))
                {
                    _canonicalRecordset.Add(itemT);
                }
                OnExternalChange(item);
                return _canonicalRecordset.IndexOf(itemT);
            }
            if (typeof(T) == typeof(StringWrapper))
            {
                var wrapper = new StringWrapper(item?.ToString() ?? string.Empty);
                if (wrapper is T itemTT)
                {
                    _canonicalRecordset.Add(itemTT);
                    return _canonicalRecordset.IndexOf(itemTT);
                }
            }
            throw new ArgumentException($"Value of type {item?.GetType()} cannot be added to list of {typeof(T)}");
        }

        public bool Remove(T item)
        {
            var removed = _canonicalRecordset.Remove(item);
            if (removed) OnExternalChange(item);
            return removed;
        }

        void IList.Insert(int index, object item)
        {
            using (BeginCollectionChangeAuthority(CollectionChangeAuthority.NetProjection))
            {
                _canonicalRecordset.Insert(index, (T)item);
            }
            OnExternalChange(item);
        }

        void IList.Remove(object item)
        {
            if (_canonicalRecordset.Contains((T)item))
            {
                using (BeginCollectionChangeAuthority(CollectionChangeAuthority.NetProjection))
                {
                    _canonicalRecordset.Remove((T)item);
                }
                OnExternalChange(item);
            }
        }

        /// <summary>
        /// We need this, but this implementation is probationary and might need some tweaking.
        /// </summary>
        private void OnExternalChange(object? value)
        {
            if (value is ISelectable selectable)
            {
                selectable.Selection = ItemSelection.None;
            }
            FilteringState = FilteringState;
            switch (FilteringState)
            {
                case FilteringState.Ineligible:
                    break;
                case FilteringState.Armed:
                    break;
                case FilteringState.Active:
                    _ = ApplyFilter();
                    break;
                default:
                    break;
            }
        }

        void ICollection.CopyTo(Array array, int index) { ((ICollection)_canonicalRecordset).CopyTo(array, index); }

        bool ICollection.IsSynchronized { get { return ((ICollection)_canonicalRecordset).IsSynchronized; } }

        object ICollection.SyncRoot { get { return ((ICollection)_canonicalRecordset).SyncRoot; } }

        #endregion I L I S T

        /// <summary>
        /// Public-facing CollectionChanged event, regardless of its source.
        /// </summary>
        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs eBCL)
        {
            switch (eBCL)
            {
                case NotifyQueryFilterCollectionChangedEventArgs eQF:
                    // New bandaid.

                    Debug.Assert(DateTime.Now.Date == new DateTime(2026, 3, 12).Date, "Don't forget disabled");
                    var matches =
                        Model
                        .Descendants()
                        .Where(_ => 
                        {
                            if(_.Attribute(nameof(StdMarkdownAttribute.ismatch)) is { } attr)
                            {
                                if(bool.TryParse(attr.Value, out var parsed))
                                {
                                    return parsed;
                                }
                                else
                                {
                                    return false;
                                }
                            }
                            else
                            {
                                return true; 
                            }
                        })
                        .Select(_ => (_.Attribute(StdMarkdownAttribute.model) as XBoundAttribute)?.Tag)
                        .ToArray();
                    { }
                    if (ObservableNetProjection is IList list)
                    {
                        if(list.Count == matches.Length)
                        { }
                        else 
                        { }
                        //list.Clear();
                        //foreach (var item in v)
                        //{
                        //    list.Add(item);
                        //}
                    }
                    break;
                default:
                    break;
            }
            if(DHostAuthorityEpoch.Authority != CollectionChangeAuthority.None)
            {
                CollectionChanged?.Invoke(this, eBCL);
            }
        }

        public event NotifyCollectionChangedEventHandler? CollectionChanged;

        /// <summary>
        /// No client data connection is assumed, but if a persistent
        /// SQLite data connection is provided it will be queried here.
        /// </summary>
        [PublishedContract("1.0", typeof(IObservableQueryFilterSource))]
        public virtual void Commit()
        {
            if (MemoryDatabase != null)
            {
                switch (SearchEntryState)
                {
                    case SearchEntryState.Cleared:
                        break;
                    case SearchEntryState.QueryEmpty:
                        break;
                    case SearchEntryState.QueryENB:
                        break;
                    case SearchEntryState.QueryEN:
                        // Nullable property, but we're not in
                        // a target framework that supports it.
                        if (MemoryDatabase != null)
                        {
                            // Please don't combine these two lines. This is for cMe purposes.
                            var recordset = MemoryDatabase.Query<T>(InputText.ParseSqlMarkdown<T>());
                            ReplaceItems(recordset);
                        }
                        break;
                    case SearchEntryState.QueryCompleteNoResults:
                        break;
                    case SearchEntryState.QueryCompleteWithResults:
                        break;
                    default:
                        break;
                }
            }
        }

        [Obsolete("Legacy unit test support only.")]
        public MarkdownContextOR MarkdownContextOR
        {
            get
            {
                var searchEntryState = SearchEntryState;
                return InputText.ParseSqlMarkdown<T>(ref searchEntryState);
            }
        }

        public string SQL => MarkdownContextOR?.ToString();

        protected override void OnInputTextChanged()
        {
            base.OnInputTextChanged();
            switch (FilteringState)
            {
                case FilteringState.Armed:
                    // Basically, this is when a backspace in Filter mode results in an
                    // empty entry text field. We want to stay in filtering mode though,
                    // but the UI visuals might change e.g. icon glyph and/or color.
                    if (FilteringStatePrev == FilteringState.Active)
                    {
                        OnCollectionChanged(
                            new NotifyQueryFilterCollectionChangedEventArgs(
                                NotifyQueryFilterCollectionChangedAction.RemoveFilter | NotifyQueryFilterCollectionChangedAction.Add,
                                _canonicalRecordset.ToList() // snapshot as IList
                            )
                        );
                    }
                    break;
            }
        }
        protected override async Task OnInputTextSettled(CancelEventArgs e)
        {
            await base.OnInputTextSettled(e);
            RouteToFullRecordset = string.IsNullOrWhiteSpace(InputText);
        }
        protected virtual void OnItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            ItemPropertyChanged?.Invoke(this, new ItemPropertyChangedEventArgs(e.PropertyName, sender));
        }
        public event EventHandler<ItemPropertyChangedEventArgs>? ItemPropertyChanged;
        private INotifyPropertyChanged[] _unsubscribeItems = new INotifyPropertyChanged[] { };

        public string Placeholder =>
                IsFiltering
                ? $"Filter {Title}"
                : $"Search {Title}";
        public string Title
        {
            get => _title;
            set
            {
                if (!Equals(_title, value))
                {
                    _title = value;
                    OnPropertyChanged();
                }
            }
        }
        string _title = "Items";


        #region R O U T E D    C O N D I T I O N A L S

        protected override void OnSearchEntryStateChanged()
        {
            base.OnSearchEntryStateChanged();
            if (SearchEntryState == SearchEntryState.Cleared)
            {
                _canonicalRecordset.Clear();
            }
        }

        /// <summary>
        /// This is a router for whether to show the unfiltered set or the filtered one.
        /// The override allows some intelligence WRT the number of filterable items in the list.
        /// </summary>
        [Careful("This polarity was wrong, and has been fixed.")]
        public override bool RouteToFullRecordset
        {
            get
            {
                // The FULL RECORDSET has less than 2 items total.
                if (_canonicalRecordset.Count < 2) // Filtering state ineligible. Show all items.
                {
                    return true;
                }
                else
                {
                    if (InputText.Length == 0)
                    {
                        return true;         // Show all items. Full stop.
                    }
                    else
                    {
                        // Initial filtering state (query expr unchanged) OR
                        // any subsequent change where InputText is not empty.
                        return FilteringState != FilteringState.Active;
                    }
                }
            }
            protected set
            {
                this.ThrowSoft<InvalidOperationException>(
                    $"{nameof(RouteToFullRecordset)}.Set is a NOOP in the derived class and should not be called.");
            }
        }

        [Careful("This polarity was wrong, and has been fixed.")]
        private IList<T> _RoutedItems_ =>
            RouteToFullRecordset ? _canonicalRecordset : _predicateMatchSubset;

        public IEnumerator<T> GetEnumerator() => _RoutedItems_.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

        public int Count => _RoutedItems_.Count;

        /// <summary>
        /// Required IList support
        /// </summary>
        public bool IsReadOnly => ((IList)_canonicalRecordset).IsReadOnly;

        /// <summary>
        /// Required IList support
        /// </summary>
        public bool IsFixedSize => ((IList)_canonicalRecordset).IsFixedSize;

        public T this[int index]
        {
            get { return _RoutedItems_[index]; }
            set
            {
                // Eventually we'll want to add an item to a filtered list, but to do so:
                // - New item needs to be added to the clients external (maybe) database.
                // - New item needs to be added to the local FilterQueryDatabase,
                // - Finally, we need to add it to the filtered items regardless
                //   of whether it meets the current filter (otherwise you might
                //   add it and have it disappear due to the filter.
                // WE WILL NEED TO DO THIS CAREFULLY WHEN THE TIME COMES!
                throw new NotImplementedException("ToDo");
            }
        }

        object IList.this[int index]
        {
            get => this[index];
            set
            {
                if (value is T t)
                {
                    this[index] = t;
                }
                else
                {
                    Debug.Fail("ADVISORY - Invalid cast but don't crash.");
                }
            }
        }
        #endregion R O U T E D    C O N D I T I O N A L S
    }
}