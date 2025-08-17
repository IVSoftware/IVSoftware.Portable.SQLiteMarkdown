using IVSoftware.Portable.SQLiteMarkdown.Common;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace IVSoftware.Portable.SQLiteMarkdown.WinTest
{
    class SelectableQFViewCard
        : TableLayoutPanel
        , INotifyPropertyChanged
    {
        public SelectableQFViewCard() =>  InitializeComponent();
        private void InitializeComponent()
        {
            Font = new Font(Font.FontFamily, 12);
            AutoSize = true;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;
            Padding = new Padding(2);
            Margin = new Padding(2);
            BorderStyle = BorderStyle.FixedSingle;
            ColumnCount = 2;
            RowCount = 2;
            ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            RowStyles.Add(new RowStyle(SizeType.Percent, 50));
            RowStyles.Add(new RowStyle(SizeType.Percent, 50));
            Controls.Add(_labelDescription, 0, 0);
            SetColumnSpan(_labelDescription, 2);

            var gridKeywordsAndTags = new TableLayoutPanel
            {
                Name = "gridKeywordsAndTags",
                ColumnCount = 2,
                RowCount = 1,
                Dock = DockStyle.Fill,

            };
            gridKeywordsAndTags.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            gridKeywordsAndTags.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

            gridKeywordsAndTags.Controls.Add(_labelKeywords, 0, 0);
            gridKeywordsAndTags.Controls.Add(_labelTags, 1, 0);

            Controls.Add(gridKeywordsAndTags, 0, 1);
            SetColumnSpan(gridKeywordsAndTags, 2);
            _labelKeywords.Font = new Font(Font.FontFamily, Font.Size - 1F);
            _labelTags.Font = new Font(Font.FontFamily, Font.Size - 2F);
        }

        private void OnAnyMouseUp(object? sender, EventArgs e)
        {
            switch (DataContext!.Selection)
            {
                case ItemSelection.None:
                    DataContext!.Selection = ItemSelection.Exclusive;
                    break;
                case ItemSelection.Exclusive:
                    DataContext!.Selection = ItemSelection.None;
                    break;
                case ItemSelection.Multi:
                case ItemSelection.Primary:
                default:
                    // N O O P for now
                    break;
            }
        }

        public new SelectableQFModel? DataContext => (SelectableQFModel?)base.DataContext;

        /// <summary>
        /// Trackable for subscribe and unsubscribe.
        /// </summary>
        public SelectableQFModel? _dataContext = null;

        protected override void OnDataContextChanged(EventArgs e)
        {
            base.OnDataContextChanged(e);

            if (_dataContext is not null)
            {
                _dataContext.PropertyChanged -= OnPropertyChanged;
            }
            _dataContext = DataContext;
            if (_dataContext is not null)
            {
                _dataContext.Selection = ItemSelection.None;
                _dataContext.PropertyChanged += OnPropertyChanged;
            }
            foreach (var pi in DataContext?.GetType().GetProperties() ?? [])
            {
                OnPropertyChanged(DataContext, new PropertyChangedEventArgs(pi.Name));
            }
        }

        public override Size GetPreferredSize(Size proposedSize)
            => new Size(Size.Width, 80);



        private Label _labelDescription = new Label
        {
            Name = nameof(_labelDescription),
            AutoSize = true,
            TextAlign = ContentAlignment.MiddleLeft,
            Dock = DockStyle.Fill,
        };

        private Label _labelKeywords = new Label
        {
            Name = nameof(_labelKeywords),
            Text = "Marklar",
            AutoSize = true,
            Anchor = (AnchorStyles)0xf,
        };

        private Label _labelTags = new Label
        {
            Name = nameof(_labelTags),
            Anchor = (AnchorStyles)0xf,
            Dock = DockStyle.Fill,
        };

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
            OnPropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        protected virtual void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(sender, e);
            if(ReferenceEquals(sender, this))
            { }
            else if (ReferenceEquals(sender, DataContext))
            {
                switch (e.PropertyName)
                {
                    case nameof(DataContext.Description):
                        _labelDescription.Text = DataContext?.Description ?? string.Empty;
                        break;
                    case nameof(DataContext.Keywords):
                        _labelKeywords.Text = DataContext?.KeywordsDisplay ?? string.Empty;
                        break;
                    case nameof(DataContext.Tags):
                        _labelTags.Text = DataContext?.Tags ?? string.Empty;
                        break;
                    case nameof(DataContext.Selection):
                        switch (DataContext?.Selection)
                        {
                            case ItemSelection.None:
                                BackColor = SystemColors.Window;
                                ForeColor = SystemColors.WindowText;
                                break;
                            case ItemSelection.Primary:
                                BackColor = Color.RoyalBlue;
                                ForeColor = Color.White;
                                break;
                            case ItemSelection.Multi:
                            case ItemSelection.Exclusive:
                                BackColor = Color.CornflowerBlue;
                                ForeColor = Color.White;
                                break;
                        }
                        break;
                }
            }
        }
        public event PropertyChangedEventHandler? PropertyChanged;
    }
    static class ExtensionsInternal
    {
        public static IEnumerable<Control> Descendants(this Control @this)
        {
            return localDescendants(@this.Controls);

            IEnumerable<Control> localDescendants(Control.ControlCollection controls)
            {
                foreach (Control view in controls)
                {
                    yield return view;

                    foreach (var child in localDescendants(view.Controls))
                    {
                        yield return child;
                    }
                }
            }
        }
    }
}
