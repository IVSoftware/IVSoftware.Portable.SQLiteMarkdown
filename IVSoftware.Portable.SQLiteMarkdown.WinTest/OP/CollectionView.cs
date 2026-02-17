using IVSoftware.Portable.SQLiteMarkdown.Collections;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using View = System.Windows.Forms.Control;


namespace IVSoftware.Portable.SQLiteMarkdown.WinTest.OP
{
    public partial class CollectionView
        : DataGridView
        , INotifyPropertyChanged
        , IMessageFilter
    {
        const int MIN_ROW_HEIGHT = 60;
        public CollectionView() : this(default) { }
        public CollectionView(XElement? xop)
        {
            InitializeComponent();
            Application.AddMessageFilter(this);
            Disposed += (sender, e) => Application.RemoveMessageFilter(this);
            _selectedItems = new ObservableHashSet();
            _selectedItems.CollectionChanged += OnSelectedItemsChanged;
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
        /// Ensures BackColor tracks BackgroundColor so that recycled templates which declare
        /// BackColor = Color.Empty inherit the intended background color. Without this, they fall
        /// back to the DataGridView's BackColor, which is not designer-visible but still influences
        /// rendering, causing unexpected visual results.
        /// </summary>
        protected override void OnBackgroundColorChanged(EventArgs e)
        {
            base.OnBackgroundColorChanged(e);
            BackColor = BackgroundColor;
        }

        public IReadOnlyList<object> SelectedItems => new ReadOnlyCollection<object>(_selectedItems);

        private readonly ObservableHashSet _selectedItems;
        public new SelectionMode SelectionMode
        {
            get => _selectionMode;
            set
            {
                if (!Equals(_selectionMode, value))
                {
                    _selectionMode = value;
                    OnPropertyChanged();
                }
            }
        }
        SelectionMode _selectionMode = SelectionMode.None;
        public Func<bool> CanMultiselect
        {
            get => _canMultiselect ?? (() => SelectionMode == SelectionMode.Multiple);
            set => _canMultiselect = value;
        }
        private Func<bool>? _canMultiselect;

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
                if (!isDown)
                {
                    var clientPoint = PointToClient(Cursor.Position);
                    var hit = HitTest(clientPoint.X, clientPoint.Y);
                    if (hit.RowIndex >= 0)
                    {
                        var item = ItemsSource?[hit.RowIndex];
                        if (SelectionMode != SelectionMode.None)
                        {
                            if (CanMultiselect())
                            {
                                // Special upgrade.
                                if (item is ISelectable selectable &&
                                    selectable.Selection == ItemSelection.Multi &&
                                    MultiselectMode != MultiselectMode.DisablePrimary)
                                {
                                    foreach (var selected in SelectedItems.OfType<ISelectable>())
                                    {
                                        selected.Selection =
                                            ReferenceEquals(selected, item)
                                            ? ItemSelection.Primary
                                            : ItemSelection.Multi;
                                    }
                                    return;
                                }
                            }
                            else
                            {
                                _selectedItems.Clear();
                            }
                            if (_selectedItems.Contains(item))
                            {
                                _selectedItems.Remove(item);
                            }
                            else
                            {
                                _selectedItems.Add(item);
                            }
                        }
                        ItemClicked?.Invoke(this, new ItemMouseEventArgs(item));
                    }
                }
            }
            return false;
        }
        protected virtual void OnSelectedItemsChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            SelectionChanged?.Invoke(this, e);

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (var item in e.NewItems?.OfType<ISelectable>() ?? [])
                    {
                        item.Selection = ItemSelection.Multi;
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (var item in e.OldItems?.OfType<ISelectable>() ?? [])
                    {
                        item.Selection = ItemSelection.None;
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    break;
                case NotifyCollectionChangedAction.Move:
                    break;
                case NotifyCollectionChangedAction.Reset:
                    if (e is NotifyCollectionResetEventArgs eReset)
                    {
                        foreach (var item in eReset.OldItems?.OfType<ISelectable>() ?? [])
                        {
                            item.Selection = ItemSelection.None;
                        }
                    }
                    else
                    { }
                    break;
                default:
                    break;
            }
            var selectables = SelectedItems.OfType<ISelectable>().ToArray();
            switch (selectables.Length)
            {
                case 0:
                    break;
                case 1:
                    selectables[0].Selection = ItemSelection.Exclusive;
                    break;
                default:
                    switch (MultiselectMode)
                    {
                        case MultiselectMode.LastIsPrimary:
                            int i;
                            for (i = 0; i < selectables.Length - 1; i++)
                            {
                                selectables[i].Selection = ItemSelection.Multi;
                            }
                            selectables[i].Selection = ItemSelection.Primary;
                            break;
                        case MultiselectMode.FirstIsPrimary:
                            selectables[0].Selection = ItemSelection.Primary;
                            for (i = 1; i < selectables.Length; i++)
                            {
                                selectables[i].Selection = ItemSelection.Multi;
                            }
                            break;
                        case MultiselectMode.DisablePrimary:
                            break;
                        default:
                            break;
                    }
                    break;
            }
        }
        MultiselectMode MultiselectMode { get; set; } = MultiselectMode.LastIsPrimary;

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
                    if (_itemsSource is INotifyCollectionChanged)
                    {
                        ((INotifyCollectionChanged)_itemsSource).CollectionChanged -= localOnCollectionChanged;
                    }
                    _itemsSource = value;

                    if (_itemsSource is INotifyCollectionChanged)
                    {
                        ((INotifyCollectionChanged)_itemsSource).CollectionChanged += localOnCollectionChanged;
                    }
                    RowCount = ItemsSource?.Count ?? 0;
                    Vacuum();
                    Invalidate();
                    OnPropertyChanged();

                    #region L o c a l F x       
                    void localOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
                    {
                        if (sender is IList items)
                        {
                            RowCount = items.Count;
                        }
                        Vacuum();
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
                if (_dataTemplate == null)
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
        public event PropertyChangedEventHandler? PropertyChanged;

        public new event EventHandler<NotifyCollectionChangedEventArgs>? SelectionChanged;
        public event EventHandler<ItemMouseEventArgs>? ItemClicked;
    }
    public enum MultiselectMode
    {
        LastIsPrimary,
        FirstIsPrimary,
        DisablePrimary,
    }
}
