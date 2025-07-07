using System.ComponentModel;
using System.Runtime.CompilerServices;

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
                get => base.DataContext;
                set
                {
                    base.DataContext = value;
                    Text = value?.ToString();
                }
            }

            public ItemSelection Selection
            {
                get => _selection;
                set
                {
                    if (!Equals(_selection, value))
                    {
                        _selection = value;
                        OnPropertyChanged();
                    }
                }
            }
            ItemSelection _selection = default;


            /// <summary>
            /// Default true means that it will belectable when not editing.
            /// </summary>
            public bool IsReadOnly { get; set; } = true;

            protected override void OnDataContextChanged(EventArgs e)
            {
                base.OnDataContextChanged(e);

                if (_dataContext is INotifyPropertyChanged)
                {
                    ((INotifyPropertyChanged)_dataContext).PropertyChanged -= OnPropertyChanged;
                }
                _dataContext = DataContext;
                if (_dataContext is INotifyPropertyChanged)
                {
                    ((INotifyPropertyChanged)_dataContext).PropertyChanged += OnPropertyChanged;
                }
                foreach (var pi in DataContext?.GetType().GetProperties() ?? [])
                {
                    OnPropertyChanged(DataContext, new PropertyChangedEventArgs(pi.Name));
                }
            }
            // For unsubscribing.
            object? _dataContext = null;

            protected void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
                OnPropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            protected virtual void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
            {
                PropertyChanged?.Invoke(sender, e);
                if (ReferenceEquals(sender, this))
                {   /* G T K */
                    // This important guard prevents an infinite loop.
                }
                else if (ReferenceEquals(sender, DataContext))
                {
                    switch (e.PropertyName)
                    {
                        case nameof(ISelectableQueryFilterItem.Selection):
                            if (DataContext is ISelectableQueryFilterItem selectable)
                            {
                                switch (selectable?.Selection)
                                {
                                    case ItemSelection.None:
                                        BackColor = Color.Empty;
                                        ForeColor = Color.Empty;
                                        break;
                                    case ItemSelection.Exclusive:
                                        BackColor = Color.CornflowerBlue;
                                        ForeColor = Color.White;
                                        break;
                                }
                            }
                            break;
                    }
                }
            }
            public event PropertyChangedEventHandler? PropertyChanged;
        }
    }
}
