using IVSoftware.Portable.SQLiteMarkdown.Collections;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
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
                AuditViewportIndexes();
                // WDTScroll.StartOrRestart(e);
            };
            Layout += (sender, e) =>
            {
                AuditViewportIndexes();
                // WDTScroll.StartOrRestart(e);
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

        private void AuditViewportIndexes()
        {
            if (ItemsSource is not null)
            {
                int
                    firstRow = FirstDisplayedScrollingRowIndex,
                    visibleCount = Math.Max(DisplayedRowCount(true), 0),
                    rowIndex,
                    mod,
                    offset;
                View? view;
                if (firstRow >= 0)
                {
                    for (rowIndex = 0; rowIndex < firstRow; rowIndex++)
                    {
                        mod = rowIndex % _templateCount;
                        if (_recycledViews.TryGetValue(mod, out view))
                        {
                            view.Visible = false;
                        }
                    }
                    for (
                        offset = 0; 
                        offset < _templateCount && offset < visibleCount;
                        offset++)
                    {
                        rowIndex = firstRow + offset;
                        mod = rowIndex % _templateCount;
                        object?
                            model;
                        if (rowIndex >= 0
                            && rowIndex < ItemsSource.Count)
                        {
                            model = ItemsSource[rowIndex];

                            if (!_recycledViews.TryGetValue(mod, out view))
                            {
                                view = (View)Activator.CreateInstance(DataTemplate.Type)!;
                                _recycledViews[mod] = view;
                                Controls.Add(view);
                            }
                            if (!ReferenceEquals(view.DataContext, model))
                            {
                                view.DataContext = model;
                                view.Invalidate();
                            }
                        }
                    }
                }
            }
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
                    // [Careful]Invalidate calls Vacuum indirectly
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
#if DEBUG
            var json = JsonConvert.SerializeObject(this.ItemsSource, Formatting.Indented);
            { }
            var color_d = @"[
  {
    ""Id"": ""dc6dd161-e303-4b33-9ea8-0a1c2258073f"",
    ""Description"": ""Brown Dog"",
    ""Keywords"": ""[\""loyal\"",\""friend\"",\""furry\""]"",
    ""KeywordsDisplay"": ""\""loyal\"",\""friend\"",\""furry\"""",
    ""Tags"": ""[canine] [color]"",
    ""IsChecked"": false,
    ""Selection"": 0,
    ""IsEditing"": false,
    ""PrimaryKey"": ""dc6dd161-e303-4b33-9ea8-0a1c2258073f"",
    ""QueryTerm"": ""brown~dog~loyal~friend~furry~[canine]~[color]"",
    ""FilterTerm"": ""brown~dog~loyal~friend~furry~[canine]~[color]"",
    ""TagMatchTerm"": ""[canine] [color]"",
    ""Properties"": ""{\r\n  \""Description\"": \""Brown Dog\"",\r\n  \""Tags\"": \""[canine] [color]\"",\r\n  \""Keywords\"": \""[\\\""loyal\\\"",\\\""friend\\\"",\\\""furry\\\""]\""\r\n}""
  },
  {
    ""Id"": ""f468cf12-40eb-4d73-930c-e2b09aaa3734"",
    ""Description"": ""Blue Bird"",
    ""Keywords"": ""[\""sky\"",\""feathered\"",\""song\""]"",
    ""KeywordsDisplay"": ""\""sky\"",\""feathered\"",\""song\"""",
    ""Tags"": ""[bird] [color]"",
    ""IsChecked"": false,
    ""Selection"": 0,
    ""IsEditing"": false,
    ""PrimaryKey"": ""f468cf12-40eb-4d73-930c-e2b09aaa3734"",
    ""QueryTerm"": ""blue~bird~sky~feathered~song~[bird]~[color]"",
    ""FilterTerm"": ""blue~bird~sky~feathered~song~[bird]~[color]"",
    ""TagMatchTerm"": ""[bird] [color]"",
    ""Properties"": ""{\r\n  \""Description\"": \""Blue Bird\"",\r\n  \""Tags\"": \""[bird] [color]\"",\r\n  \""Keywords\"": \""[\\\""sky\\\"",\\\""feathered\\\"",\\\""song\\\""]\""\r\n}""
  },
  {
    ""Id"": ""7cf12ce5-1f31-4b9c-9b3a-b75e18002b85"",
    ""Description"": ""Red Cherry"",
    ""Keywords"": ""[\""sweet\"",\""summer\"",\""dessert\""]"",
    ""KeywordsDisplay"": ""\""sweet\"",\""summer\"",\""dessert\"""",
    ""Tags"": ""[fruit] [color]"",
    ""IsChecked"": false,
    ""Selection"": 0,
    ""IsEditing"": false,
    ""PrimaryKey"": ""7cf12ce5-1f31-4b9c-9b3a-b75e18002b85"",
    ""QueryTerm"": ""red~cherry~sweet~summer~dessert~[fruit]~[color]"",
    ""FilterTerm"": ""red~cherry~sweet~summer~dessert~[fruit]~[color]"",
    ""TagMatchTerm"": ""[fruit] [color]"",
    ""Properties"": ""{\r\n  \""Description\"": \""Red Cherry\"",\r\n  \""Tags\"": \""[fruit] [color]\"",\r\n  \""Keywords\"": \""[\\\""sweet\\\"",\\\""summer\\\"",\\\""dessert\\\""]\""\r\n}""
  },
  {
    ""Id"": ""3ad3abbe-f346-44a6-a22e-3f38bb487f43"",
    ""Description"": ""Gray Wolf"",
    ""Keywords"": ""[\""pack\"",\""howl\"",\""wild\""]"",
    ""KeywordsDisplay"": ""\""pack\"",\""howl\"",\""wild\"""",
    ""Tags"": ""[animal] [color]"",
    ""IsChecked"": false,
    ""Selection"": 0,
    ""IsEditing"": false,
    ""PrimaryKey"": ""3ad3abbe-f346-44a6-a22e-3f38bb487f43"",
    ""QueryTerm"": ""gray~wolf~pack~howl~wild~[animal]~[color]"",
    ""FilterTerm"": ""gray~wolf~pack~howl~wild~[animal]~[color]"",
    ""TagMatchTerm"": ""[animal] [color]"",
    ""Properties"": ""{\r\n  \""Description\"": \""Gray Wolf\"",\r\n  \""Tags\"": \""[animal] [color]\"",\r\n  \""Keywords\"": \""[\\\""pack\\\"",\\\""howl\\\"",\\\""wild\\\""]\""\r\n}""
  },
  {
    ""Id"": ""cc47bc75-a8c3-4de1-b315-0939c3e5d65e"",
    ""Description"": ""Pink Flamingo"",
    ""Keywords"": ""[]"",
    ""KeywordsDisplay"": """",
    ""Tags"": ""[bird] [color]"",
    ""IsChecked"": false,
    ""Selection"": 0,
    ""IsEditing"": false,
    ""PrimaryKey"": ""cc47bc75-a8c3-4de1-b315-0939c3e5d65e"",
    ""QueryTerm"": ""pink~flamingo~[bird]~[color]"",
    ""FilterTerm"": ""pink~flamingo~[bird]~[color]"",
    ""TagMatchTerm"": ""[bird] [color]"",
    ""Properties"": ""{\r\n  \""Description\"": \""Pink Flamingo\"",\r\n  \""Tags\"": \""[bird] [color]\""\r\n}""
  },
  {
    ""Id"": ""5ea573d9-ca67-4a98-9a1e-2918c8b89ae9"",
    ""Description"": ""Golden Lion"",
    ""Keywords"": ""[]"",
    ""KeywordsDisplay"": """",
    ""Tags"": ""[animal] [color]"",
    ""IsChecked"": false,
    ""Selection"": 0,
    ""IsEditing"": false,
    ""PrimaryKey"": ""5ea573d9-ca67-4a98-9a1e-2918c8b89ae9"",
    ""QueryTerm"": ""golden~lion~[animal]~[color]"",
    ""FilterTerm"": ""golden~lion~[animal]~[color]"",
    ""TagMatchTerm"": ""[animal] [color]"",
    ""Properties"": ""{\r\n  \""Description\"": \""Golden Lion\"",\r\n  \""Tags\"": \""[animal] [color]\""\r\n}""
  },
  {
    ""Id"": ""690db759-e4f6-4f38-b5ea-3fb1e2718aaf"",
    ""Description"": ""Brown Bear"",
    ""Keywords"": ""[\""strong\"",\""wild\"",\""forest\""]"",
    ""KeywordsDisplay"": ""\""strong\"",\""wild\"",\""forest\"""",
    ""Tags"": ""[animal] [color]"",
    ""IsChecked"": false,
    ""Selection"": 0,
    ""IsEditing"": false,
    ""PrimaryKey"": ""690db759-e4f6-4f38-b5ea-3fb1e2718aaf"",
    ""QueryTerm"": ""brown~bear~strong~wild~forest~[animal]~[color]"",
    ""FilterTerm"": ""brown~bear~strong~wild~forest~[animal]~[color]"",
    ""TagMatchTerm"": ""[animal] [color]"",
    ""Properties"": ""{\r\n  \""Description\"": \""Brown Bear\"",\r\n  \""Tags\"": \""[animal] [color]\"",\r\n  \""Keywords\"": \""[\\\""strong\\\"",\\\""wild\\\"",\\\""forest\\\""]\""\r\n}""
  },
  {
    ""Id"": ""2abbf113-5c1b-41d2-820f-c5a1192ba4c1"",
    ""Description"": ""Red Strawberry"",
    ""Keywords"": ""[]"",
    ""KeywordsDisplay"": """",
    ""Tags"": ""[fruit] [color]"",
    ""IsChecked"": false,
    ""Selection"": 0,
    ""IsEditing"": false,
    ""PrimaryKey"": ""2abbf113-5c1b-41d2-820f-c5a1192ba4c1"",
    ""QueryTerm"": ""red~strawberry~[fruit]~[color]"",
    ""FilterTerm"": ""red~strawberry~[fruit]~[color]"",
    ""TagMatchTerm"": ""[fruit] [color]"",
    ""Properties"": ""{\r\n  \""Description\"": \""Red Strawberry\"",\r\n  \""Tags\"": \""[fruit] [color]\""\r\n}""
  },
  {
    ""Id"": ""70bfb26f-9814-4556-ae82-322ba4c50e55"",
    ""Description"": ""White Swan"",
    ""Keywords"": ""[]"",
    ""KeywordsDisplay"": """",
    ""Tags"": ""[bird] [color]"",
    ""IsChecked"": false,
    ""Selection"": 0,
    ""IsEditing"": false,
    ""PrimaryKey"": ""70bfb26f-9814-4556-ae82-322ba4c50e55"",
    ""QueryTerm"": ""white~swan~[bird]~[color]"",
    ""FilterTerm"": ""white~swan~[bird]~[color]"",
    ""TagMatchTerm"": ""[bird] [color]"",
    ""Properties"": ""{\r\n  \""Description\"": \""White Swan\"",\r\n  \""Tags\"": \""[bird] [color]\""\r\n}""
  }
]";
#endif
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
