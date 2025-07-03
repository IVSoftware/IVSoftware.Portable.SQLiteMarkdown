using IVSoftware.Portable.SQLiteMarkdown.MSTest.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace IVSoftware.Portable.SQLiteMarkdown.WinTest
{
    class SelectableQFViewCard : TableLayoutPanel, INotifyPropertyChanged
    {
        public SelectableQFViewCard()
        {
            InitializeComponent();
        }
        private void InitializeComponent()
        {
            AutoSize = true;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;
            Padding = new Padding(4);
            Margin = new Padding(2);
            BorderStyle = BorderStyle.FixedSingle;
            ColumnCount = 2;
            RowCount = 2;
            ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            RowStyles.Add(new RowStyle(SizeType.Percent, 50));
            RowStyles.Add(new RowStyle(SizeType.Percent, 50));

            var labelDescription = new Label
            {
                Name = "labelDescription",
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Fill,
                BackColor = Color.LightBlue,
            };
            Controls.Add(labelDescription, 0, 0);
            SetColumnSpan(labelDescription, 2);

            var gridKeywordsAndTags = new TableLayoutPanel
            {
                Name = "gridKeywordsAndTags",
                ColumnCount = 2,
                RowCount = 1,
                Dock = DockStyle.Fill,
                BackColor = Color.Red,

            };
            gridKeywordsAndTags.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            gridKeywordsAndTags.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

            var labelLeft = new Label
            {
                Name = "labelKeywords",
                Text = "Marklar",
                AutoSize = true,
                Anchor = (AnchorStyles)0xf,
                BackColor = Color.MediumPurple,
            };
            var labelRight = new Label
            {
                Name = "labelKeywords",
                Anchor = (AnchorStyles)0xf,
                BackColor = Color.Green,
                Dock = DockStyle.Fill,
            };

            gridKeywordsAndTags.Controls.Add(labelLeft, 0, 0);
            gridKeywordsAndTags.Controls.Add(labelRight, 1, 0);

            Controls.Add(gridKeywordsAndTags, 0, 1);
            SetColumnSpan(gridKeywordsAndTags, 2);
        }


        public new SelectableQFModel? DataContext
        {
            get => (SelectableQFModel?)base.DataContext;
            set
            {
                if(DataContext is not null)
                {
                    DataContext.PropertyChanged -= OnPropertyChanged;
                }
                base.DataContext = value;
                if(DataContext is not null)
                {
                    DataContext.PropertyChanged -= OnPropertyChanged;
                }
            }
        }

        private void OnBoundPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(DataContext.Description):
                    break;
            }
        }

        protected override void OnDataContextChanged(EventArgs e)
        {
            base.OnDataContextChanged(e);
            Description = DataContext?.Description ?? string.Empty;
            Keywords = DataContext?.Keywords ?? string.Empty;
            Tags = DataContext?.Tags ?? string.Empty;
        }

        public string Description
        {
            get => _description;
            set
            {
                if (!Equals(_description, value))
                {
                    _description = value;
                    OnPropertyChanged();
                }
            }
        }
        string _description = string.Empty;

        public string Keywords
        {
            get => _keywords;
            set
            {
                if (!Equals(_keywords, value))
                {
                    _keywords = value;
                    OnPropertyChanged();
                }
            }
        }
        string _keywords = string.Empty;

        public string Tags
        {
            get => _Tags;
            set
            {
                if (!Equals(_Tags, value))
                {
                    _Tags = value;
                    OnPropertyChanged();
                }
            }
        }
        string _Tags = string.Empty;

        public override Size GetPreferredSize(Size proposedSize)
            => new Size(Size.Width, 80);

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
            OnPropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        protected virtual void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(sender, e);
        }
        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
