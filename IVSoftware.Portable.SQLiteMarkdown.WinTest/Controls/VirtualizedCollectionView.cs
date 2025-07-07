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
using System.Windows.Forms.VisualStyles;
using System.Xml.Linq;
using View = System.Windows.Forms.Control;

namespace IVSoftware.Portable.SQLiteMarkdown.WinTest.Controls
{
#if false
    public partial class VirtualizedCollectionView 
        : DataGridView
        , INotifyPropertyChanged
        , IMessageFilter
    {

        const int MIN_ROW_HEIGHT = 60;
        public VirtualizedCollectionView() : this(default) { }
        public VirtualizedCollectionView(XElement? xop)
        {
            InitializeComponent();
            Application.AddMessageFilter(this);
            Disposed += (sender, e) => Application.RemoveMessageFilter(this); ;
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
                    e.RowIndex < ItemsSource.Count)
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

                    view.DataContext = ItemsSource[e.RowIndex];

                    var margin = view.Margin;
                    var cellRect = GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, true);

                    var adjustedBounds = new Rectangle(
                        cellRect.X + margin.Left,
                        cellRect.Y + margin.Top,
                        Math.Max(0, cellRect.Width - margin.Horizontal),
                        Math.Max(0, cellRect.Height - margin.Vertical)
                    );

                    int desiredHeight = Math.Max(view.PreferredSize.Height + margin.Vertical, MIN_ROW_HEIGHT);
                    if (row.Height != desiredHeight)
                    {
                        // Causes row validation
                        row.Height = desiredHeight;
                        // Exit early to avoid drawing at incorrect size.
                        // Paint using the corrected row height on the next pass.
                        return;
                    }

                    view.Bounds = adjustedBounds;
                    view.Visible = true;
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
                    e.Value = ItemsSource[e.RowIndex];
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

        /// <summary>
        /// Gets the currently recycled view instances keyed by template slot.
        /// Used internally and by tests to verify view reuse and layout state.
        /// </summary>

        [EditorBrowsable(EditorBrowsableState.Never)]
        internal IReadOnlyDictionary<int, View> RecycledViews => _recycledViews;
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
                if (ColumnCount == 0)
                {
                    Columns.Add(new DataGridViewTextBoxColumn
                    {
                        AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                        MinimumWidth = 8,
                        Name = "Items"
                    });
                }
                if (!Equals(_itemsSource, value))
                {
                    if (_itemsSource is INotifyPropertyChanged)
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
                    RowCount = ItemsSource?.Count ?? 0;
                    Invalidate();
                    OnPropertyChanged();

                    #region L o c a l F x       
                    void localOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
                    {
                        if (sender is IObservableQueryFilterSource qfs &&
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
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
            OnPropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        protected virtual void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(sender, e);
        }
        public bool PreFilterMessage(ref Message m)
        {
            switch ((Win32Message)m.Msg)
            {
                case Win32Message.WM_LBUTTONDOWN:
                    localOnMouse(true);
                    break;
                case Win32Message.WM_LBUTTONUP:
                    localOnMouse(false);
                    break;
            }
            void localOnMouse(bool isDown)
            {
                if (ItemsSource is IObservableQueryFilterSource qfs && 
                    qfs.SelectionMode != SQLiteMarkdown.SelectionMode.None)
                {
                    if (!isDown)
                    {
                        var clientPoint = PointToClient(Cursor.Position);
                        var hit = HitTest(clientPoint.X, clientPoint.Y);
                        if (hit.RowIndex >= 0)
                        {
                            var item = ItemsSource?[hit.RowIndex];
                            if (item is ISelectableQueryFilterItem selectable && selectable.IsReadOnly)
                            {
                                switch (selectable.Selection)
                                {
                                    case ItemSelection.None:
                                        selectable.Selection = ItemSelection.Exclusive;
                                        break;
                                    case ItemSelection.Exclusive:
                                        selectable.Selection = ItemSelection.None;
                                        break;
                                }
                            }
                        }
                    }
                }
            }
            return false; 
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
#endif
}
