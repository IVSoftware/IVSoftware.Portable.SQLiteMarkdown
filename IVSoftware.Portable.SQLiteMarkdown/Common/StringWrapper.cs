﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace IVSoftware.Portable.SQLiteMarkdown.Common
{
    public class StringWrapper
        : ISelectable
        , INotifyPropertyChanged
    {

        public StringWrapper() { }

        public StringWrapper(string value) => Value = value;

        public override string ToString() => Value;

        public static implicit operator StringWrapper(string s) => new StringWrapper(s);

        public static implicit operator string(StringWrapper wrapper) => wrapper?.Value;

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
