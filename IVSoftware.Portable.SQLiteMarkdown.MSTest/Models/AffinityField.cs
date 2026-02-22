using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace IVSoftware.Portable.SQLiteMarkdown.MSTest.Models
{
    class ObservableAffinityField<T>
        : ObservableCollection<T>
        , IAffinityField
        where T : IAffinityItem
    {
        public bool IsRunning
        {
            get => _isRunning;
            set
            {
                if (!Equals(_isRunning, value))
                {
                    _isRunning = value;
                    OnPropertyChanged();
                }
            }
        }
        bool _isRunning = false;


        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));

        // We do not care about INPC in the BC which is mainly Count semantics.
        public new event PropertyChangedEventHandler? PropertyChanged;
    }
}
