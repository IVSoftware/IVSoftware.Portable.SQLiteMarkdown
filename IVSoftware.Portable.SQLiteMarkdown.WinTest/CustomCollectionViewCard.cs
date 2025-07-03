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
    class CustomCollectionViewCard : TableLayoutPanel, INotifyPropertyChanged
    {
        public CustomCollectionViewCard()
        {
            AutoSize = false;
            Padding = new Padding(4);
            Margin = new Padding(2);
            BorderStyle = BorderStyle.FixedSingle;
        }
        public new SelectableQueryModel? DataContext
        {
            get => (SelectableQueryModel?)base.DataContext;
            set => base.DataContext = value;
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



        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
