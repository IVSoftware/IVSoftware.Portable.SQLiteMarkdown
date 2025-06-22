using SQLite;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace IVSoftware.Portable.SQLiteMarkdown.Collections
{
#if true || SAVE
    public class QueryFilterStateTracker { }
#else
    public class QueryFilterStateTracker : INotifyPropertyChanged, IQueryFilterStateTracker
    {
        public QueryFilterStateTracker(IObservableQueryFilterSource items = null)
        {
            Items = items;
        }
        public IObservableQueryFilterSource Items
        {
            get => _items;
            set
            {
                if (!Equals(_items, value))
                {
                    if (_items != null)
                    {
                        _items.CollectionChanged -= OnCollectionChanged;
                    }
                    if(FilterQueryDB != null)
                    {
                        FilterQueryDB.Dispose();
                    }
                    _items = value;
                    if (_items != null)
                    {
                        _items.CollectionChanged += OnCollectionChanged;
                    }
                    var type = _items.GetType();
                    if ((type.IsGenericType))
                    {
                        ModelType = type.GetGenericArguments().SingleOrDefault(); 
                    }
                    OnPropertyChanged();
                }
            }
        }
        IObservableQueryFilterSource _items = null;

        Type ModelType
        {
            get => _modelType;
            set
            {
                if (!Equals(_modelType, value))
                {
                    _modelType = value;
                    FilterQueryDB = new SQLiteConnection(":memory:");
                    FilterQueryDB.CreateTable(_modelType);
                    ModelTypeMapping = FilterQueryDB.GetMapping(ModelType);
                    OnPropertyChanged();
                }
            }
        }
        Type _modelType = default;

        TableMapping ModelTypeMapping
        {
            get => _modelTypeMapping;
            set
            {
                if (!Equals(_modelTypeMapping, value))
                {
                    _modelTypeMapping = value;
                    OnPropertyChanged();
                }
            }
        }
        TableMapping _modelTypeMapping = default;

        public FilteringState Clear(bool all = false)
        {
            if (InputText.Length > 0)
            {
                InputText = string.Empty;
            }
            else
            {
                switch (FilteringState)
                {
                    case FilteringState.Inactive:
                        break;
                    case FilteringState.Armed:
                        FilteringState = FilteringState.Inactive;
                        break;
                    case FilteringState.Active:
                        FilteringState = FilteringState.Armed;
                        break;
                    default:
                        throw new NotImplementedException($"Bad case: {FilteringState}");
                }
            }
            // Fluent return;
            return FilteringState;
        }

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

                                    if (FilterQueryDB != null && items.Count > 0)
                                    {
                                        FilterQueryDB.DeleteAll(ModelTypeMapping); 
                                        FilterQueryDB.InsertAll(items);
                                    }

                                    SearchEntryState =
                                        items.Count > 0
                                        ? SearchEntryState.QueryCompleteWithResults
                                        : SearchEntryState.QueryCompleteNoResults;

                                    // Once we go into Armed, it takes 2 clears not one.
                                    FilteringState =
                                        items.Count < 2
                                        ? FilteringState.Inactive
                                        : FilteringState.Armed;
                                    break;
                                case NotifyQueryFilterCollectionChangedAction.ApplyFilter:
                                    throw new NotImplementedException("TODO");
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
        public SQLiteConnection FilterQueryDB { get; set; }


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

        protected virtual void OnInputTextChanged()
        {
            var lint = InputText?.Trim() ?? string.Empty;
            if (FilteringState == FilteringState.Inactive)
            {
                if (lint.Length == 0)
                {
                    SearchEntryState = SearchEntryState.QueryEmpty;
                }
                else if (lint.Length < 3)
                {
                    SearchEntryState = SearchEntryState.QueryENB;
                    return;
                }
                else
                {
                    SearchEntryState = SearchEntryState.QueryEN;
                }
            }
            else
            {
                if (lint.Length == 0)
                {
                    if (FilteringState == FilteringState.Active)
                    {
                        // Downgrade to Armed only.
                        FilteringState = FilteringState.Armed;
                    }
                }
                else
                {
                    Items.ApplyFilter();
                    if (Items.IsFiltering)
                    {

                    }
                }
            }
        }

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

        public FilteringState FilteringState
        {
            get => _filteringState;
            protected set
            {
                if (!QueryFilterConfig.HasFlag(QueryFilterConfig.Filter))
                {
                    // The only transition allowed is going back to Inactive.
                    value = FilteringState.Inactive;
                }
                if (!Equals(_filteringState, value))
                {
                    _filteringState = value;
                    OnPropertyChanged();
                }
            }
        }
        FilteringState _filteringState = default;

        public string Placeholder =>
                FilteringState == FilteringState.Inactive
                ? $"Search {Title}"
                : $"Filter {Title}";
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
                    if(_wdtInputTextSettled is WatchdogTimer wdt)
                    {
                        wdt.Interval = value;
                    }
                    _inputTextSettleInterval = value;
                    OnPropertyChanged();
                }
            }
        }

        TimeSpan _inputTextSettleInterval = TimeSpan.FromSeconds(0.25);


        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            OnPropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        protected virtual void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (ReferenceEquals(sender, this))
            {
                PropertyChanged?.Invoke(sender, e);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
#endif
}
