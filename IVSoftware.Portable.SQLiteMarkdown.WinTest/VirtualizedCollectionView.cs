using IVSoftware.Portable;
using IVSoftware.Portable.SQLiteMarkdown;
using IVSoftware.Portable.SQLiteMarkdown.Collections;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using View = System.Windows.Forms.Control;

namespace OnePageCollectionViewSketchpad
{
    public class VirtualizedCollectionView 
        : DataGridView
        , INotifyPropertyChanged
    {

        const int MIN_ROW_HEIGHT = 60;
        public VirtualizedCollectionView() : this(default) { }
        public VirtualizedCollectionView(XElement? xop)
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            DoubleBuffered = true;
            Dock = DockStyle.Fill;
            VirtualMode = true;
            AllowUserToAddRows = false;
            AllowUserToDeleteRows = false;
            RowHeadersVisible = false;
            ColumnHeadersVisible = false;

            CellPainting += (sender, e) =>
            {
                if (ItemsSource is not null &&
                    e.ColumnIndex != -1 &&
                    e.RowIndex != -1 &&
                    e.RowIndex < ItemsSource.Count
                    )
                {
                    using (var brush = new SolidBrush(BackgroundColor))
                    {
                        e.Graphics?.FillRectangle(brush, e.CellBounds);
                    }
                    View? view = null;
                    var row = Rows[e.RowIndex];

                    var mod = e.RowIndex % _templateCount;
                    if (!_recycledViews.TryGetValue(mod, out view))
                    {
                        view = (View)Activator.CreateInstance(DataTemplate.Type)!;
                        _recycledViews[mod] = view;
                        Controls.Add(view);
                    }

                    int desiredHeight = Math.Max(view.PreferredSize.Height, MIN_ROW_HEIGHT);
                    if (row.Height != desiredHeight)
                    {
                        row.Height = desiredHeight;
                        return;
                    }
                    view.DataContext = ItemsSource[mod];
                    view.Visible = true;
                    view.Bounds  = GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, true);
                }
                e.Handled = true;
                return;
            };
            CellValueNeeded += (sender, e) =>
            {
                if (ItemsSource is not null &&
                    e.ColumnIndex != -1 &&
                    e.RowIndex != -1 &&
                    e.RowIndex < ItemsSource.Count
                    )
                {
                    var mod = e.RowIndex % _templateCount;
                    if (e.RowIndex < ItemsSource.Count)
                    {
                        e.Value = ItemsSource[mod];
                    }
                }
            };
            Scroll += (sender, e) =>
            {
                Vacuum();
                WDTScroll.StartOrRestart(e);
            };
            Layout += (sender, e) =>
            {
                Vacuum();
            };
            MouseDoubleClick += (sender, e) =>
            {
                var hit = HitTest(e.X, e.Y);
                if (hit.Type == DataGridViewHitTestType.None)
                {
                    Invalidate();
                }
            };
        }
        Dictionary<int, View> _recycledViews = new();

        public WatchdogTimer WDTScroll
        {
            get
            {
                if (_wdtScroll is null)
                {
                    _wdtScroll = new WatchdogTimer(
                        defaultInitialAction: () =>
                        {
                            Invalidate();
                        },
                        defaultCompleteAction: () =>
                        {
                            Invalidate();
                        })
                    {
                        Interval = TimeSpan.FromSeconds(0.1) 
                    };
                }
                return _wdtScroll;
            }
        }
        WatchdogTimer? _wdtScroll = null;

#if false
        [EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int VisibleRowCount
        {
            get => _visibleRowCount;
            private set
            {
                if (!Equals(_visibleRowCount, value))
                {
                    _visibleRowCount = value;
                    _templateCount = Math.Max(_templateCount, _visibleRowCount) + 1;
                    Vacuum();
                }
            }
        }
        int _visibleRowCount = 0;
#endif
        int _templateCount = 10;

        private void Vacuum()
        {
            int first = Math.Max(FirstDisplayedScrollingRowIndex, 0);
            int last = first + DisplayedRowCount(true);
            _templateCount = Math.Max(_templateCount, (last - first) + 1);
            var visibleKeys = new HashSet<int>();
            for (int i = first; i < last; i++)
            {
                visibleKeys.Add(i % _templateCount);
            }
            foreach (var kvp in _recycledViews)
            {
                if (!visibleKeys.Contains(kvp.Key))
                {
                    kvp.Value.Visible = false;
                }
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IList? ItemsSource
        {
            get => _itemsSource;
            set
            {
                if (!Equals(_itemsSource, value))
                {
                    if(_itemsSource is INotifyPropertyChanged)
                    {
                        ((INotifyPropertyChanged)_itemsSource).PropertyChanged -= localOnPropertyChanged;
                    }
                    if (_itemsSource is INotifyCollectionChanged)
                    {
                        ((INotifyCollectionChanged)_itemsSource).CollectionChanged -= localOnCollectionChanged;
                    }
                    _itemsSource = value;
                    if (_itemsSource is INotifyPropertyChanged)
                    {
                        ((INotifyPropertyChanged)_itemsSource).PropertyChanged += localOnPropertyChanged;

                    }
                    if (_itemsSource is INotifyCollectionChanged)
                    {
                        ((INotifyCollectionChanged)_itemsSource).CollectionChanged += localOnCollectionChanged;
                    }
                    OnPropertyChanged();

                    #region L o c a l F x       
                    void localOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
                    {
                        if( sender is IObservableQueryFilterSource qfs && 
                            e is NotifyQueryFilterCollectionChangedEventArgs eqfs)
                        {
                            switch (eqfs.Action)
                            {
                                case NotifyQueryFilterCollectionChangedAction.Add:
                                    break;
                                case NotifyQueryFilterCollectionChangedAction.Remove:
                                    break;
                                case NotifyQueryFilterCollectionChangedAction.Replace:
                                    break;
                                case NotifyQueryFilterCollectionChangedAction.Move:
                                    break;
                                case NotifyQueryFilterCollectionChangedAction.Reset:
                                    break;
                                case NotifyQueryFilterCollectionChangedAction.QueryResult:
                                    break;
                                case NotifyQueryFilterCollectionChangedAction.ApplyFilter:
                                    Debug.Assert(qfs.Count == eqfs.NewItems?.Count);
                                    break;
                                default:
                                    break;
                            }
                            RowCount = qfs.Count;
                        }
                        else if (sender is IList items)
                        {
                            RowCount = items.Count;
                        }
                        Vacuum();
                    }

                    void localOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
                    {
                        if (sender is IObservableQueryFilterSource qfs)
                        {
                            switch (e.PropertyName)
                            {
                                case nameof(IObservableQueryFilterSource.SearchEntryState):
                                    switch (qfs.SearchEntryState)
                                    {
                                        case SearchEntryState.Cleared:
                                            break;
                                        case SearchEntryState.QueryEmpty:
                                            break;
                                        case SearchEntryState.QueryENB:
                                            break;
                                        case SearchEntryState.QueryEN:
                                            break;
                                        case SearchEntryState.QueryCompleteNoResults:
                                            break;
                                        case SearchEntryState.QueryCompleteWithResults:
                                            break;
                                        default:
                                            break;
                                    }
                                    break;
                                case nameof(IObservableQueryFilterSource.FilteringState):
                                    switch (qfs.FilteringState)
                                    {
                                        case FilteringState.Ineligible:
                                            break;
                                        case FilteringState.Armed:
                                            break;
                                        case FilteringState.Active:
                                            break;
                                        default:
                                            break;
                                    }
                                    break;
                                case nameof(IObservableQueryFilterSource.InputText):
                                    break;
                                case nameof(IObservableQueryFilterSource.MemoryDatabase):
                                    break;
                                case nameof(IObservableQueryFilterSource.Busy):
                                    break;
                                default:
                                    break;
                            }
                        }
                    }		
                    #endregion L o c a l F x
                    
                }
            }
        }

        IList? _itemsSource = null;

        public IReadOnlyList<View> Cards => _cards.ToArray();


        private ObservableCollection<View>_cards = new();

        [EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public CollectionViewDataTemplate DataTemplate
        {
            get
            {
                if(_dataTemplate == null)
                {
                    _dataTemplate = new CollectionViewDataTemplate<DefaultCollectionViewCard>();
                }
                return _dataTemplate;
            }
            set
            {
                if (!Equals(_dataTemplate, value))
                {
                    _dataTemplate = value;
                    OnPropertyChanged();
                }
            }
        }
        CollectionViewDataTemplate? _dataTemplate = null;

        public class CollectionViewDataTemplate
        {
            public Type Type
            {
                get => _type ?? typeof(DefaultCollectionViewCard);
                protected set => _type = value;
            }
            Type? _type;
        }
        public class CollectionViewDataTemplate<T>
            : CollectionViewDataTemplate
        {
            public CollectionViewDataTemplate()
            {
                Type = typeof(T);
            }
        }

        public class DefaultCollectionViewCard : Label
        {
            public DefaultCollectionViewCard()
            {
                AutoSize = false;
                TextAlign = ContentAlignment.MiddleLeft;
                Padding = new Padding(4);
                Margin = new Padding(2);
                BorderStyle = BorderStyle.FixedSingle;
            }

            public override object? DataContext
            {
                get => Tag;
                set
                {
                    Tag = value;
                    Text = value?.ToString();
                }
            }
            protected override void OnDataContextChanged(EventArgs e)
            {
                base.OnDataContextChanged(e);
            }
        }
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
            OnPropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        protected virtual void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(sender, e);
        }
        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
