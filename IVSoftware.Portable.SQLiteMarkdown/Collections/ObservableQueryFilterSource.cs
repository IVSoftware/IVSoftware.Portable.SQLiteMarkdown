using IVSoftware.Portable.Common.Attributes;
using IVSoftware.Portable.Common.Exceptions;
using IVSoftware.Portable.Disposable;
using IVSoftware.Portable.SQLiteMarkdown.Common;
using IVSoftware.Portable.SQLiteMarkdown.Events;
using IVSoftware.Portable.Threading;
using IVSoftware.Portable.Xml.Linq.XBoundObject;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

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
        public ObservableQueryFilterSource()
        {
            // GZ GZ GZ GZ
            base.ObservableNetProjection = this;

            // base.ProjectionOptions = NetProjectionOption.ObservableOnly;

            _canonicalRecordset.CollectionChanged += (sender, e) =>
            {
                if (CollectionChangeAuthority == NotifyCollectionChangedEventAuthority.MarkdownContext)
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
                                    OnCollectionChangedProtected(e);
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
                                OnCollectionChangedProtected(e);
                            }
                            break;
                        case NotifyCollectionChangedAction.Reset:
                            _predicateMatchSubset.Clear();
                            foreach (var inpc in _unsubscribeItems)
                            {
                                inpc.PropertyChanged -= OnItemPropertyChanged;
                            }
                            OnCollectionChangedProtected(e);
                            break;
                    }
                    _unsubscribeItems = _canonicalRecordset.OfType<INotifyPropertyChanged>().ToArray();
                }
            };
        }

        private readonly ObservableCollection<T> _predicateMatchSubset = new ObservableCollection<T>();
        private readonly ObservableCollection<T> _canonicalRecordset = new ObservableCollection<T>();

        public IReadOnlyList<T> CanonicalRecordset => _canonicalRecordset;


        [Obsolete("Use CanonicalRecordset and PredicateMatchSubset for precise semantics.")]
        public IReadOnlyList<T> UnfilteredItems => _canonicalRecordset;

        public async Task ReplaceItemsAsync(IEnumerable<T> items)
        {
            Debug.WriteLine($@"260306.a ADVISORY - {nameof(ReplaceItemsAsync)}.");
            using (DHostBusy.GetToken())
            {
                // --------------
                // UPGRADE 260301
                // Sets UnfilteredCount
                // -> Sets SearchEntryState
                base.LoadCanon(items);
                // --------------

                // This causes a Reset on the main INCC
                _canonicalRecordset.Clear();
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
                    OnCollectionChangedProtected(
                        new NotifyQueryFilterCollectionChangedEventArgs(
                            NotifyQueryFilterCollectionChangedAction.QueryResult | NotifyQueryFilterCollectionChangedAction.Add,
                            _canonicalRecordset.ToList() // snapshot as IList
                        )
                    );
                }
            }
        }

        /// <summary>
        /// Replaces the entire current dataset after a query.
        /// Not valid in Filter-only mode. Raises CollectionChanged and sets FilteringState based on count.
        /// </summary>
        public void ReplaceItems(IEnumerable<T> items)
        {
            using (DHostBusy.GetToken())
            {
                // --------------
                // UPGRADE 260301
                base.LoadCanon(items);
                // --------------
#if DEBUG

                var preview = FilterQueryDatabase.ExecuteScalar<int>("Select count(*) from items");
                var model = Model;
                { }
                if (base.ObservableNetProjection is null)
                {
                }
                else if (ReferenceEquals(this, base.ObservableNetProjection))
                {
                }
                else
                {
                    Debug.Fail($@"ADVISORY - This is nonsensical and you shouldn't be here.");
                }
                int CCB4 = CanonicalCount;
#endif


                // RESET REQUIRED main INCC for NetProjection.
                // Unit Tests expect this, but it can't be left
                // outside of the authority or Model gets cleared..
                // #{4E778EBA-D838-48D0-89D6-3D1FC8229E23}
                // _canonicalRecordset.Clear(); // <- NOT HERE!

                using (base.BeginAuthorityClaim())
                {
                    // Building from the model in V2 is new.

                    _canonicalRecordset.Clear(); // <- HERE!
#if DEBUG
                    if (CCB4 != CanonicalCount)
                    {
                        Debug.Fail("ACTION NEEDED - The CC must survive this.");
                    }
#endif
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
                        OnCollectionChangedProtected(
                            new NotifyQueryFilterCollectionChangedEventArgs(
                                NotifyQueryFilterCollectionChangedAction.QueryResult | NotifyQueryFilterCollectionChangedAction.Add,
                                _canonicalRecordset.ToList() // snapshot as IList
                            )
                        );
                    }
                }
            }
        }

        /// <summary>
        /// Sets Filter-only mode and initializes the dataset for filtering.
        /// Ideal for static lists (e.g., preferences, enums).
        /// When a filter is "cleared" it means the collection view returns to "all items visible".
        /// </summary>
        [PublishedSignature("1.0")]
        public void InitializeFilterOnlyMode(IEnumerable<T> items)
        {
            QueryFilterConfig = QueryFilterConfig.Filter;
            LoadCanon(items);
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
                        OnCollectionChangedProtected(
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

        #region I L I S T
        public int IndexOf(T item) { return _canonicalRecordset.IndexOf(item); }

        void IList.Clear() => Clear(all: true);
        void ICollection<T>.Clear() => Clear(all: true);

        public new void Clear(bool all = false)
        {
            base.Clear(all);
            if (FilteringState < FilteringState.Armed)
            {
#if DEBUG
                CollectionChangedProtectedZ += localCollectionChanged;

                // [Careful] 
                // If we're responding to FilteringState changed to clear the
                // unfiltered items list it MIGHT NOT WORK. For example, manual
                // add-remove changes to Items will bypass the input state machine. 
                _canonicalRecordset.Clear();

                void localCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
                {
                    CollectionChangedProtectedZ -= localCollectionChanged;
                }
#else
                // [Careful] 
                // If we're responding to FilteringState changed to clear the
                // unfiltered items list it MIGHT NOT WORK. For example, manual
                // add-remove changes to Items will bypass the input state machine. 
                _unfilteredItems.Clear();
#endif
            }
        }

        public bool Contains(T item) { return _canonicalRecordset.Contains(item); }

        public void CopyTo(T[] array, int arrayIndex) { _canonicalRecordset.CopyTo(array, arrayIndex); }

        bool IList.Contains(object value) { return ((IList)_canonicalRecordset).Contains(value); }

        int IList.IndexOf(object value) { return ((IList)_canonicalRecordset).IndexOf(value); }
        public void Insert(int index, T item)
        {
            _canonicalRecordset.Insert(index, item);
            OnExternalChange(item);
        }

        public void Add(T item)
        {
            _canonicalRecordset.Add(item);
            OnExternalChange(item);
        }
        public void RemoveAt(int index)
        {
            object item;
            if (index < _canonicalRecordset.Count)
            {
                item = _canonicalRecordset[index];
            }
            else
            {
                item = null;
            }
            _canonicalRecordset.RemoveAt(index);
            OnExternalChange(item);
        }

        int IList.Add(object item)
        {
            if(item is T itemT)
            {
                _canonicalRecordset.Add(itemT);
                OnExternalChange(item);
                return _canonicalRecordset.IndexOf(itemT);
            }
            if(typeof(T) == typeof(StringWrapper))
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
            _canonicalRecordset.Insert(index, (T)item);
            OnExternalChange(item);
        }

        void IList.Remove(object item)
        {
            if (_canonicalRecordset.Contains((T)item))
            {
                _canonicalRecordset.Remove((T)item);
                OnExternalChange(item);
            }
        }

        /// <summary>
        /// We need this, but this implementation is 
        /// probationary and might need some tweaking.
        /// </summary>
        private void OnExternalChange(object value)
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
                    ApplyFilter();
                    break;
                default:
                    break;
            }
        }

        void ICollection.CopyTo(Array array, int index) { ((ICollection)_canonicalRecordset).CopyTo(array, index); }

        bool ICollection.IsSynchronized { get { return ((ICollection)_canonicalRecordset).IsSynchronized; } }

        object ICollection.SyncRoot { get { return ((ICollection)_canonicalRecordset).SyncRoot; } }

        #endregion I L I S T

        public event NotifyCollectionChangedEventHandler CollectionChanged
        {
            add
            {
                CollectionChangedProtectedZ += value;
            }
            remove
            {
                CollectionChangedProtectedZ += value;
            }
        }

        private void OnCollectionChangedProtected(NotifyCollectionChangedEventArgs e)
        {
            CollectionChangedProtectedZ?.Invoke(this, e);
        }
        protected event NotifyCollectionChangedEventHandler CollectionChangedProtectedZ;

        /// <summary>
        /// No client data connection is assumed, but if a persistent
        /// SQLite data connection is provided it will be queried here.
        /// </summary>
        public void Commit()
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
                        OnCollectionChangedProtected(
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
            if(SearchEntryState == SearchEntryState.Cleared)
            {
                // This is old. Do we still need it?
                _canonicalRecordset.Clear();
                OnCollectionChangedProtected(
                    new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }

        /// <summary>
        /// This is a router for whether to show the unfiltered set or the filtered one.
        /// The override allows some intelligence WRT the number of filterable items in the list.
        /// </summary>


        /// <summary>
        /// This is a router for whether to show the unfiltered set or the filtered one.
        /// The override allows some intelligence WRT the number of filterable items in the list.
        /// </summary>
        [Careful("This polarity was wrong, and has been fixed.")]
        public override bool RouteToFullRecordset
        {
            get
            {
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