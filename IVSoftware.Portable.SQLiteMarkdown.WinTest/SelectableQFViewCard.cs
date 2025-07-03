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
            Controls.Add(_labelDescription, 0, 0);
            SetColumnSpan(_labelDescription, 2);

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

            gridKeywordsAndTags.Controls.Add(_labelKeywords, 0, 0);
            gridKeywordsAndTags.Controls.Add(_labelTags, 1, 0);

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
                    DataContext.PropertyChanged += OnPropertyChanged;
                }
            }
        }

        protected override void OnDataContextChanged(EventArgs e)
        {
            base.OnDataContextChanged(e);
            foreach (var pi in DataContext?.GetType().GetProperties() ?? [])
            {
                OnPropertyChanged(DataContext, new PropertyChangedEventArgs(pi.Name));
            }
        }

        public override Size GetPreferredSize(Size proposedSize)
            => new Size(Size.Width, 80);

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
                    case nameof(SelectableQFModel.Description):
                        _labelDescription.Text = DataContext?.Description ?? string.Empty;
                        break;
                    case nameof(SelectableQFModel.Keywords):
                        _labelKeywords.Text = DataContext?.Keywords ?? string.Empty;
                        break;
                    case nameof(SelectableQFModel.Tags):
                        _labelTags.Text = DataContext?.Tags ?? string.Empty;
                        break;
                }
            }
        }
        public event PropertyChangedEventHandler? PropertyChanged;



        private Label _labelDescription = new Label
        {
            Name = nameof(_labelDescription),
            AutoSize = true,
            TextAlign = ContentAlignment.MiddleLeft,
            Dock = DockStyle.Fill,
            BackColor = Color.LightBlue,
        };

        private Label _labelKeywords = new Label
        {
            Name = nameof(_labelKeywords),
            Text = "Marklar",
            AutoSize = true,
            Anchor = (AnchorStyles)0xf,
            BackColor = Color.MediumPurple,
        };

        private Label _labelTags = new Label
        {
            Name = nameof(_labelTags),
            Anchor = (AnchorStyles)0xf,
            BackColor = Color.Green,
            Dock = DockStyle.Fill,
        };
    }
}
