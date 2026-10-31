using System.ComponentModel;

namespace IVSoftware.Portable.SQLiteMarkdown.Events
{
    public class ItemPropertyChangedEventArgs : PropertyChangedEventArgs
    {
        public ItemPropertyChangedEventArgs(string propertyName, object item) : base(propertyName)
        {
            Item = item;
        }
        public object Item { get; }
    }
}
