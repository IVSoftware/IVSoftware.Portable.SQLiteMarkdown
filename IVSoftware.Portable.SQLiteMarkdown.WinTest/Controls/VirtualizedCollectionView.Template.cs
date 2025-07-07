using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace IVSoftware.Portable.SQLiteMarkdown.WinTest.Controls
{
    public partial class VirtualizedCollectionView
    {
        public class DefaultCollectionViewCard
            : Label
            , ISelectableQueryFilterItem
            , INotifyPropertyChanged
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

            public ItemSelection Selection { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
            public bool IsReadOnly { get; set; } = false;

            protected override void OnDataContextChanged(EventArgs e)
            {
                base.OnDataContextChanged(e);
            }
            protected void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
                OnPropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            protected virtual void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
            {
                PropertyChanged?.Invoke(sender, e);
                if (ReferenceEquals(sender, this))
                {   /* G T K */
                    // This important guard prevents an infinite loop.
                }
                else
                {
                }
            }
            public event PropertyChangedEventHandler? PropertyChanged;
        }

    }
}
