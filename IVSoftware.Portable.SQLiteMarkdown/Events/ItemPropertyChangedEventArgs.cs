using IVSoftware.Portable.Common.Exceptions;
using System;
using System.ComponentModel;

namespace IVSoftware.Portable.SQLiteMarkdown.Events
{
    public class ItemPropertyChangedEventArgs : PropertyChangedEventArgs
    {
        public ItemPropertyChangedEventArgs(string propertyName, object? item) : base(safe(propertyName))
        {
            Item = item;
        }

        private static string safe(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName))
            {
                nameof(ItemPropertyChangedEventArgs).ThrowHard<InvalidOperationException>();
                return "Missing Property Name"; // We warned you.
            }
            else
            {
                return propertyName;
            }
        }

        public object? Item { get; }
    }
}
