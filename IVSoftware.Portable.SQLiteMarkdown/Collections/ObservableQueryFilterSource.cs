using IVSoftware.Portable.Disposable;
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

namespace IVSoftware.Portable.SQLiteMarkdown.Collections
{

    public partial class ObservableQueryFilterSource<T>
        : MarkdownContext
        , IObservableQueryFilterSource
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

        protected virtual void OnItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ISelectableQueryFilterItem.Selection):
                    if(sender is ISelectableQueryFilterItem selectable)
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

        public ObservableSelectionHashSet<ISelectableQueryFilterItem> SelectedItems
        {
            get
            {
                if (_selectedItems is null)
                {
                    _selectedItems = new ObservableSelectionHashSet<ISelectableQueryFilterItem>();
                    _selectedItems.PropertyChanged += (sender, e) =>
                    {
                    };
                }
                return _selectedItems;
            }
        }
        ObservableSelectionHashSet<ISelectableQueryFilterItem> _selectedItems = null;

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

        public bool IsReadOnly
        {
            get => _isReadOnly;
            set
            {
                if (!Equals(_isReadOnly, value))
                {
                    _isReadOnly = value;
                    OnPropertyChanged();
                }
            }
        }
        bool _isReadOnly = true;

        public bool IsFixedSize => ((IList)_unfilteredItems).IsFixedSize;

        public bool Busy
        {
            get => _busy;
            set
            {
                if (!Equals(_busy, value))
                {
                    _busy = value;
                    OnPropertyChanged();
                }
            }
        }
        bool _busy = default;

        public void ReplaceItems(IList items)
        {
            try
            {
                Busy = true;
                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));

                _unfilteredItems.Clear();
                if (items.Count == 0)
                {
                    SearchEntryState = SearchEntryState.QueryCompleteNoResults;
                }
                else
                {
                    foreach (T item in items)
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
                FilteringState =
                    _unfilteredItems.Count < 2
                    ? FilteringState.Ineligible
                    : FilteringState.Armed;
            }
            finally
            {
                Busy = false;
            }
        }

        public void ApplyFilter()
        {
            try
            {
                Busy = true;
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
                            FilterQueryDatabase.DeleteAll<T>();
                            FilterQueryDatabase.InsertAll(_unfilteredItems);
                            FilteringState = FilteringState.Active;
                            break;
                        case FilteringState.Active:
                            break;
                        default:
                            throw new NotImplementedException($"Bad case: {FilteringState}");
                    }
#if DEBUG
                    var count = FilterQueryDatabase.ExecuteScalar<int>("SELECT COUNT(*) FROM items");
                    if(count == 0)
                    {
                        Debug.Fail("ADVISORY - Did you remember to populate the FilterQueryDatabase?");
                        return;
                    }
#endif

                    var searchEntryState = SearchEntryState;
                    var context = InputText.ParseSqlMarkdown<T>(ref searchEntryState);

#if DEBUG
                    var cstring = context.ToString();
                    { }

#endif

                    var tmp = FilterQueryDatabase.Query<T>(context.ToString());

#if false && DEBUG
                    if (InputText == "olf")
                    {
                        T item;
                        item = tmp.Single();
                    }
#endif
                    // This is 'not' the place for a reconciled sync.
                    // We would do that in the UI if at all.
                    _filteredItems.Clear();
                    foreach(var item in tmp)
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
                Busy = false;
            }
        }

        public int IndexOf(T item) { return _unfilteredItems.IndexOf(item); }

        public void Insert(int index, T item)
        {
            _unfilteredItems.Insert(index, item);
            ApplyFilter();
        }

        public void RemoveAt(int index)
        {
            _unfilteredItems.RemoveAt(index);
            ApplyFilter();
        }

        public void Add(T item)
        {
            _unfilteredItems.Add(item);
            ApplyFilter();
        }

        void IList.Clear() => Clear();
        void ICollection<T>.Clear() => Clear();

        // Canonical {5932CB31-B914-4DE8-9457-7A668CDB7D08}
        public FilteringState Clear(bool all = false)
        {
            if (InputText.Length > 0)
            {
                // Basically, if there is entry text but the filtering
                // is still only armed not active, that indicates that
                // what we're seeing in the list is the result of a full
                // db query that just occurred. So now, when we CLEAR that
                // text, it's assumed to be in the interest of filtering
                // that query result, so filtering goes Active in theis case.
                InputText = string.Empty;
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
            }
            else
            {
                switch (FilteringState)
                {
                    case FilteringState.Ineligible:
                        break;
                    case FilteringState.Armed:
                    case FilteringState.Active:
                        // If the text is already empty and
                        // you click again, it's a hard reset!
                        FilteringState = FilteringState.Ineligible;
                        break;
                    default:
                        throw new NotImplementedException($"Bad case: {FilteringState}");
                }
            }
            // Fluent return;
            return FilteringState;
        }

        public bool Contains(T item) { return _unfilteredItems.Contains(item); }

        public void CopyTo(T[] array, int arrayIndex) { _unfilteredItems.CopyTo(array, arrayIndex); }

        public bool Remove(T item)
        {
            var removed = _unfilteredItems.Remove(item);
            if (removed) ApplyFilter();
            return removed;
        }
        int IList.Add(object value)
        {
            _unfilteredItems.Add((T)value);
            ApplyFilter();
            return _unfilteredItems.IndexOf((T)value);
        }

        bool IList.Contains(object value) { return ((IList)_unfilteredItems).Contains(value); }

        int IList.IndexOf(object value) { return ((IList)_unfilteredItems).IndexOf(value); }

        void IList.Insert(int index, object value)
        {
            _unfilteredItems.Insert(index, (T)value);
            ApplyFilter();
        }

        void IList.Remove(object value)
        {
            if (_unfilteredItems.Contains((T)value))
            {
                _unfilteredItems.Remove((T)value);
                ApplyFilter();
            }
        }

        void ICollection.CopyTo(Array array, int index) { ((ICollection)_unfilteredItems).CopyTo(array, index); }

        bool ICollection.IsSynchronized { get { return ((ICollection)_unfilteredItems).IsSynchronized; } }

        object ICollection.SyncRoot { get { return ((ICollection)_unfilteredItems).SyncRoot; } }

        public event NotifyCollectionChangedEventHandler CollectionChanged;
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            OnPropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        protected virtual void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(sender, e);
        }

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
        public SQLiteConnection MemoryDatabase
        {
            get => _memoryDatabase;
            set
            {
                if (!Equals(_memoryDatabase, value))
                {
                    if(_memoryDatabase != null)
                    {
                        _memoryDatabase.Dispose();
                    }
                    _memoryDatabase = value;
                    OnPropertyChanged();
                }
            }
        }
        SQLiteConnection _memoryDatabase = default;
        public string InputText
        {
            get => _inputText;
            set
            {
                if (!Equals(_inputText, value))
                {
                    _inputText = value;
                    OnPropertyChanged();
                    OnInputTextChanged();
                }
            }
        }
        string _inputText = string.Empty;

        public MarkdownContextOR MarkdownContextOR
        {
            get
            {
                var searchEntryState = SearchEntryState;
                return InputText.ParseSqlMarkdown<T>(ref searchEntryState);
            }
        }

        public string SQL => MarkdownContextOR?.ToString();

        protected virtual void OnInputTextChanged()
        {
            var trim = InputText?.Trim() ?? string.Empty;
            var filteringStateB4 = FilteringState;
            switch (FilteringState)
            {
                case FilteringState.Ineligible:
                    if (trim.Length == 0)
                    {
                        SearchEntryState = SearchEntryState.QueryEmpty;
                    }
                    else if (trim.Length < 3)
                    {
                        SearchEntryState = SearchEntryState.QueryENB;
                        return;
                    }
                    else
                    {
                        SearchEntryState = SearchEntryState.QueryEN;
                    }
                    break;
                case FilteringState.Armed:
                    if (trim.Length != 0)
                    {
                        FilteringState = FilteringState.Active;
                    }
                    break;
                case FilteringState.Active:
                    if(trim.Length == 0)
                    {
                        // Downgrade but stay armed.
                        FilteringState = FilteringState.Armed;
                        CollectionChanged?.Invoke(
                            this,
                            new NotifyQueryFilterCollectionChangedEventArgs(
                                NotifyQueryFilterCollectionChangedAction.RemoveFilter | NotifyQueryFilterCollectionChangedAction.Add,
                                _unfilteredItems.ToList() // snapshot as IList
                            )
                        );
                    }
                    break;
                default:
                    throw new NotImplementedException($"Bad case: {FilteringState}");
            }
            // If after all of that we manage to be in an active
            // filtering state then go ahead and apply.
            if (FilteringState == FilteringState.Active)
            {
                ApplyFilter();
            }
            WDTInputTextSettled.StartOrRestart();
        }
        public event EventHandler InputTextSettled;

        public QueryFilterConfig QueryFilterConfig
        {
            get => _queryFilterConfig;
            set
            {
                if (!Equals(_queryFilterConfig, value))
                {
                    _queryFilterConfig = value;
                    OnPropertyChanged();
                }
            }
        }
        QueryFilterConfig _queryFilterConfig = QueryFilterConfig.QueryAndFilter;

        private FilteringState FilteringStatePrev { get; set;  }
        public FilteringState FilteringState
        {
            get => _filteringState;
            protected set
            {
                if (!QueryFilterConfig.HasFlag(QueryFilterConfig.Filter))
                {
                    // The only transition allowed is going back to Inactive.
                    value = FilteringState.Ineligible;
                }
                if (!Equals(_filteringState, value))
                {
                    FilteringStatePrev = _filteringState;
                    _filteringState = value;
                    OnFilteringStateChanged();
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(RouteToFullRecordset));
                    OnPropertyChanged(nameof(IsFiltering));
                }
            }
        }
        FilteringState _filteringState = default;

        public FilteringState FilteringStateForTest
        {
            get => FilteringState;
            set => FilteringState = value;
        }


        private void OnFilteringStateChanged()
        {
            switch (FilteringState)
            {
                case FilteringState.Ineligible:
                    // Clear, then event ADHOC. That is, it's not always
                    // in our best interest to simply forward the clear.
                    _unfilteredItems.Clear();
                    CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                    break;
                case FilteringState.Armed:
                    break;
                case FilteringState.Active:
                    break;
                default:
                    break;
            }
        }

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

        public SearchEntryState SearchEntryState
        {
            get => _searchEntryState;
            protected set
            {
                if (!Equals(_searchEntryState, value))
                {
                    _searchEntryState = value;
                    OnSearchEntryStateChanged();
                    OnPropertyChanged();
                }
            }
        }
        SearchEntryState _searchEntryState = default;

        protected virtual void OnSearchEntryStateChanged()
        {
        }

        protected WatchdogTimer WDTInputTextSettled
        {
            get
            {
                if (_wdtInputTextSettled is null)
                {
                    _wdtInputTextSettled =
                        new WatchdogTimer
                        {
                            Interval = InputTextSettleInterval
                        };
                    _wdtInputTextSettled.RanToCompletion += (sender, e) =>
                    {
                        InputTextSettled?.Invoke(this, EventArgs.Empty);
                    };
                }
                return _wdtInputTextSettled;
            }
        }
        WatchdogTimer _wdtInputTextSettled = null;

        public TimeSpan InputTextSettleInterval
        {
            get => _inputTextSettleInterval;
            set
            {
                if (!Equals(_inputTextSettleInterval, value))
                {
                    if (_wdtInputTextSettled is WatchdogTimer wdt)
                    {
                        wdt.Interval = value;
                    }
                    _inputTextSettleInterval = value;
                    OnPropertyChanged();
                }
            }
        }

        TimeSpan _inputTextSettleInterval = TimeSpan.FromSeconds(0.25);

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
        public SQLiteConnection FilterQueryDatabase
        {
            get
            {
                if (_filterQueryDatabase is null)
                {
                    _filterQueryDatabase = new SQLiteConnection(":memory:");
                    _filterQueryDatabase.CreateTable<T>();
                }
                return _filterQueryDatabase;
            }
        }
        SQLiteConnection _filterQueryDatabase = null;


        #region R O U T E D    C O N D I T I O N A L S

        /// <summary>
        /// This is a router for whether to show the unfiltered set or the filtered one.
        /// </summary>
        public bool RouteToFullRecordset
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

        public bool IsFiltering
            =>  FilteringState == FilteringState.Ineligible
                ? false
                : FilteringState == FilteringState.Active
                    ? true
                    : FilteringStatePrev == FilteringState.Active;

        private IList<T> _RoutedItems_ =>
            RouteToFullRecordset ? _filteredItems : _unfilteredItems;

        public IEnumerator<T> GetEnumerator() => _RoutedItems_.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

        public int Count => _RoutedItems_.Count;

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
