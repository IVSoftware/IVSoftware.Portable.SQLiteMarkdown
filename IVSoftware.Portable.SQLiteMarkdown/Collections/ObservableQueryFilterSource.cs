using IVSoftware.Portable.Disposable;
using IVSoftware.Portable.Threading;
using SQLite;
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
using System.Threading;
using System.Threading.Tasks;

namespace IVSoftware.Portable.SQLiteMarkdown.Collections
{
    [DebuggerDisplay("Count={Count}")]
    public partial class ObservableQueryFilterSource<T>
        : MarkdownContext<T>
        , IObservableQueryFilterSource<T>
        , IList<T>
        where T : new()
    {
        public ObservableQueryFilterSource()
        {
            CollectionChanged += OnCollectionChanged;

            _unfilteredItems.CollectionChanged += (sender, e) =>
            {
                if (_unfilteredItems.Count < 2)
                {
                    FilteringState = FilteringState.Ineligible;
                }
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        foreach (var inpc in e.NewItems?.OfType<INotifyPropertyChanged>())
                        {
                            inpc.PropertyChanged += OnItemPropertyChanged;
                        }
                        break;
                    case NotifyCollectionChangedAction.Remove:
                        foreach (var inpc in e.OldItems?.OfType<INotifyPropertyChanged>())
                        {
                            inpc.PropertyChanged -= OnItemPropertyChanged;
                        }
                        break;
                    case NotifyCollectionChangedAction.Reset:
                        _filteredItems.Clear();
                        foreach (var inpc in _unsubscribeItems)
                        {
                            inpc.PropertyChanged -= OnItemPropertyChanged;
                        }
                        FilterQueryDatabase.DeleteAll<T>();
                        break;
                }
                _unsubscribeItems = _unfilteredItems.OfType<INotifyPropertyChanged>().ToArray();
            };
        }

        public ObservableSelectionHashSet<object> SelectedItems
        {
            get
            {
                if (_selectedItems is null)
                {
                    _selectedItems = new ObservableSelectionHashSet<object>();
                    _selectedItems.CollectionChanged += (sender, e) =>
                    {
                        OnSelectionChanged();
                    };
                }
                return _selectedItems;
            }
        }
        ObservableSelectionHashSet<object> _selectedItems = null;

        protected virtual void OnSelectionChanged()
        {
            SelectionChanged?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler SelectionChanged;
        public SelectionMode SelectionMode
        {
            get => SelectedItems.SelectionMode;
            set
            {
                SelectedItems.SelectionMode = value;
            }
        }

        private readonly ObservableCollection<T> _filteredItems = new ObservableCollection<T>();
        private readonly ObservableCollection<T> _unfilteredItems = new ObservableCollection<T>();

        public IReadOnlyList<T> UnfilteredItems => _unfilteredItems;

        /// <summary>
        /// Replaces the entire current dataset after a query.
        /// Not valid in Filter-only mode. Raises CollectionChanged and sets FilteringState based on count.
        /// </summary>
        public void ReplaceItems(IEnumerable<T> items)
        {
            if (QueryFilterConfig == QueryFilterConfig.Filter)
            {
                throw new InvalidOperationException("You must turn off File-Only mode to use this method");
            }
            else
            {
                ReplaceItemsInternal(items);
            }
        }
        public async Task ReplaceItemsAsync(IEnumerable<T> items)
        {
            ReplaceItemsInternal(items);
            await this;
            _ready.Release();
        }

        private void ReplaceItemsInternal(IEnumerable<T> items)
        {
            using (DHostBusy.GetToken())
            {
                try
                {
                    CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));

                    _unfilteredItems.Clear();
                    if (items.Any())
                    {
                        foreach (T item in items.ToArray())
                        {
                            _unfilteredItems.Add(item);
                        }
                        CollectionChanged?.Invoke(
                            this,
                            new NotifyQueryFilterCollectionChangedEventArgs(
                                NotifyQueryFilterCollectionChangedAction.QueryResult | NotifyQueryFilterCollectionChangedAction.Add,
                                _unfilteredItems.ToList() // snapshot as IList
                            )
                        );
                        SearchEntryState = SearchEntryState.QueryCompleteWithResults;
                    }
                    else
                    {
                        SearchEntryState = SearchEntryState.QueryCompleteNoResults;
                    }
                    FilteringState =
                        _unfilteredItems.Count < 2
                        ? FilteringState.Ineligible
                        : FilteringState.Armed;
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

        /// <summary>
        /// Sets Filter-only mode and initializes the dataset for filtering.
        /// Ideal for static lists (e.g., preferences, enums).
        /// When a filter is "cleared" it means the collection view returns to "all items visible".
        /// </summary>
        public void InitializeFilterOnlyMode(IEnumerable<T> items)
        {
            ReplaceItemsInternal(items.ToArray());
            QueryFilterConfig = QueryFilterConfig.Filter;
        }

        /// <summary>
        /// Applies filtering based on incremental changes to the input text,
        /// occurring after the initial query. Operates on the in-memory SQLite store
        /// when in QueryAndFilter or Filter mode. Override to customize filter behavior.
        /// </summary>
        protected virtual void ApplyFilter()
        {
            using (DHostBusy.GetToken())
            {
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
#if DEBUG
                        var count = FilterQueryDatabase.ExecuteScalar<int>("SELECT COUNT(*) FROM items");
                        if (count == 0)
                        {
                            Debug.Fail("ADVISORY - Did you remember to populate the FilterQueryDatabase?");
                            return;
                        }
#endif

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

#if DEBUG
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
                        _filteredItems.Clear();
                        foreach (var item in filteredRecords)
                        {
                            _filteredItems.Add(item);
                        }
                        // Active REGARDLESS of result because if unfiltered
                        // count < 2 we're not supposed to be here in the first place.
                        Debug.Assert(_unfilteredItems.Count >= 2, "ADVISORY - Filterable source is required.");
                        FilteringState = FilteringState.Active;
                        CollectionChanged?.Invoke(
                            this,
                            new NotifyQueryFilterCollectionChangedEventArgs(
                                NotifyQueryFilterCollectionChangedAction.ApplyFilter,
                                _filteredItems.ToList() // snapshot
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
        public int IndexOf(T item) { return _unfilteredItems.IndexOf(item); }


        public void Add(T item)
        {
            _unfilteredItems.Add(item);
            OnExternalChange();
        }

        void IList.Clear() => Clear();
        void ICollection<T>.Clear() => Clear();

        public bool Contains(T item) { return _unfilteredItems.Contains(item); }

        public void CopyTo(T[] array, int arrayIndex) { _unfilteredItems.CopyTo(array, arrayIndex); }

        bool IList.Contains(object value) { return ((IList)_unfilteredItems).Contains(value); }

        int IList.IndexOf(object value) { return ((IList)_unfilteredItems).IndexOf(value); }
        public void Insert(int index, T item)
        {
            _unfilteredItems.Insert(index, item);
            OnExternalChange();
        }

        public void RemoveAt(int index)
        {
            _unfilteredItems.RemoveAt(index);
            OnExternalChange();
        }

        int IList.Add(object value)
        {
            _unfilteredItems.Add((T)value);
            OnExternalChange();
            return _unfilteredItems.IndexOf((T)value);
        }
        public bool Remove(T item)
        {
            var removed = _unfilteredItems.Remove(item);
            if (removed) OnExternalChange();
            return removed;
        }

        void IList.Insert(int index, object value)
        {
            _unfilteredItems.Insert(index, (T)value);
            OnExternalChange();
        }

        void IList.Remove(object value)
        {
            if (_unfilteredItems.Contains((T)value))
            {
                _unfilteredItems.Remove((T)value);
                OnExternalChange();
            }
        }

        /// <summary>
        /// We need this, but this implementation is 
        /// probationary and might need some tweaking.
        /// </summary>
        private void OnExternalChange()
        {
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

        void ICollection.CopyTo(Array array, int index) { ((ICollection)_unfilteredItems).CopyTo(array, index); }

        bool ICollection.IsSynchronized { get { return ((ICollection)_unfilteredItems).IsSynchronized; } }

        object ICollection.SyncRoot { get { return ((ICollection)_unfilteredItems).SyncRoot; } }

        #endregion I L I S T

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        /// <summary>
        /// No client data connection is assumed, but if a persistent
        /// SQLite data connection is provided it will be queried here.
        /// </summary>
        public void Commit()
        {
            if(MemoryDatabase != null)
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
                        var cMe = MemoryDatabase.Query<T>(InputText.ParseSqlMarkdown<T>());
                        ReplaceItems(cMe);
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
                    if (InputText.Length == 0 && FilteringStatePrev == FilteringState.Active)
                    {
                        CollectionChanged?.Invoke(
                            this,
                            new NotifyQueryFilterCollectionChangedEventArgs(
                                NotifyQueryFilterCollectionChangedAction.RemoveFilter | NotifyQueryFilterCollectionChangedAction.Add,
                                _unfilteredItems.ToList() // snapshot as IList
                            )
                        );
                    }
                    break;
            }
        }
        protected override void OnInputTextSettled(CancelEventArgs e)
        {
            base.OnInputTextSettled(e);
            if (!e.Cancel)
            {
                if (FilteringState == FilteringState.Active)
                {
                    ApplyFilter();
                }
            }
        }
        protected virtual void OnItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ISelectableQueryFilterItem.Selection):
                    if (sender is ISelectableQueryFilterItem selectable)
                    {
                        switch (selectable.Selection)
                        {
                            case ItemSelection.None:
                                SelectedItems.Remove(selectable);
                                break;
                            case ItemSelection.Exclusive:
                                SelectedItems.Add(selectable);
                                break;
                        }
                    }
                    break;
            }
        }
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

        protected virtual void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs eUnk)
        {
            if (sender is IList items)
            {
                NotifyCollectionChangedAction action
                    = (NotifyCollectionChangedAction)(((int)eUnk.Action) & 0x7);

                NotifyQueryFilterCollectionChangedAction actionContext =
                    (eUnk is NotifyQueryFilterCollectionChangedEventArgs eAz)
                    ? (NotifyQueryFilterCollectionChangedAction)(((int)eAz.Action) & ~0x7)
                    : 0;

                switch (eUnk.Action)
                {
                    case NotifyCollectionChangedAction.Reset:
                        SearchEntryState = SearchEntryState.Cleared;
                        break;
                    default:
                        if (eUnk is NotifyQueryFilterCollectionChangedEventArgs e)
                        {
                            switch (actionContext)
                            {
                                case 0:     // NOTE: This reads as 'Add'
                                    break;
                                case NotifyQueryFilterCollectionChangedAction.QueryResult:

                                    if (FilterQueryDatabase != null && items.Count > 0)
                                    {
                                        FilterQueryDatabase.DeleteAll<T>();
                                        FilterQueryDatabase.InsertAll(items);
                                    }

                                    SearchEntryState =
                                        items.Count > 0
                                        ? SearchEntryState.QueryCompleteWithResults
                                        : SearchEntryState.QueryCompleteNoResults;

                                    // Once we go into Armed, it takes 2 clears not one.
                                    FilteringState =
                                        items.Count < 2
                                        ? FilteringState.Ineligible
                                        : FilteringState.Armed;
                                    break;
                                case NotifyQueryFilterCollectionChangedAction.ApplyFilter:
                                    break;
                                case NotifyQueryFilterCollectionChangedAction.RemoveFilter:
                                    break;
                                default:
                                    throw new NotImplementedException($"Bad case: {actionContext}");
                            }
                        }
                        break;
                }
            }
            else
            {
                Debug.Fail("ADVISORY - Sender is not IList. Is this intentional?");
            }
        }


        #region R O U T E D    C O N D I T I O N A L S

        protected override async void OnFilteringStateChanged()
        {
            // Relies on BC functionality, except where firing the CollectionChanged event is concerned.
            base.OnFilteringStateChanged();

            // List-specific.
            switch (FilteringState)
            {
                case FilteringState.Ineligible:
                    // Clear, then event ADHOC. That is, it's not always
                    // in our best interest to simply forward the clear.
                    _unfilteredItems.Clear();
                    CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                    break;
                case FilteringState.Armed:
                    if(FilteringStatePrev == FilteringState.Ineligible)
                    {
                        await Task.Delay(TimeSpan.FromTicks(1));
                        FilterQueryDatabase.DeleteAll<T>();
                        FilterQueryDatabase.InsertAll(_unfilteredItems);
                    }
                    break;
                case FilteringState.Active:
                    break;
            }
        }

        /// <summary>
        /// This is a router for whether to show the unfiltered set or the filtered one.
        /// The override allows some intelligence WRT the number of filterable items in the list.
        /// </summary>
        public override bool RouteToFullRecordset
        {
            get
            {
                if (_unfilteredItems.Count < 2) // Filtering state ineligible. Show all items.
                {
                    return false;
                }
                else
                {
                    if (InputText.Length == 0)
                    {
                        return false;         // Show all items. Full stop.
                    }
                    else
                    {
                        return FilteringState == FilteringState.Active;
                    }
                }
            }
        }

        private IList<T> _RoutedItems_ =>
            RouteToFullRecordset ? _filteredItems : _unfilteredItems;

        public IEnumerator<T> GetEnumerator() => _RoutedItems_.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

        public int Count => _RoutedItems_.Count;

        /// <summary>
        /// Required IList support
        /// </summary>
        public bool IsReadOnly => ((IList)_unfilteredItems).IsReadOnly;

        /// <summary>
        /// Required IList support
        /// </summary>
        public bool IsFixedSize => ((IList)_unfilteredItems).IsFixedSize;

        IList IObservableQueryFilterSource<T>.SelectedItems => SelectedItems;

        public T this[int index]
        {
            get { return _RoutedItems_[index]; }
            set
            {
                // Eventually we'll want to add an item to a filtered list, but to do so:
                // - New item needs to be added to the clients external (maybe) database.
                // - New item needs to be added to the local FilterQueryDatabase,
                // - Finally, we need to add it to the filetered items regardless
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
