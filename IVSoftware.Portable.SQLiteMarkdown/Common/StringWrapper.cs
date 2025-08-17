using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace IVSoftware.Portable.SQLiteMarkdown.Common
{
    /// <summary>
    /// A lightweight wrapper that enables simple strings to participate in ObservableQueryFilterSource's 
    /// query-filter state engine. Implements ISelectable and INotifyPropertyChanged to support selection 
    /// scenarios and reactive UI binding, while providing QueryLikeTerm/FilterLikeTerm attributes for 
    /// automatic SQL WHERE clause generation via the SQLiteMarkdown parsing system. Includes implicit 
    /// conversions for seamless interoperability with raw strings during collection initialization.
    /// </summary>
    public class StringWrapper
        : ISelectable
        , INotifyPropertyChanged
    {
        public StringWrapper() { }

        public StringWrapper(string value) => Value = value;

        public override string ToString() => Value;

        public static implicit operator StringWrapper(string s) => new StringWrapper(s);

        public static implicit operator string(StringWrapper wrapper) => wrapper?.Value;

        /// <summary>
        /// Primary key property for SQLite tables. Named "Id" to leverage automatic 
        /// primary key recognition in ORMs like sqlite-net-pcl, avoiding the need to 
        /// reference sqlite-net-pcl solely for the [PrimaryKey] attribute on this single property.
        /// </summary>
        // [PrimaryKey]
        public string Id { get; set; } = 
            Guid
            .NewGuid()
            .ToString()
            .Trim("{}".ToCharArray())
            .ToUpper();

        [QueryLikeTerm, FilterLikeTerm]
        public string Value
        {
            get => _value;
            set
            {
                if (!Equals(_value, value))
                {
                    _value = value;
                    OnPropertyChanged();
                }
            }
        }
        string _value = string.Empty;

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

        public bool IsEditing { get; set; } = false;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
