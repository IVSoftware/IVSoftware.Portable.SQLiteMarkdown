using IVSoftware.Portable.Disposable;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace IVSoftware.Portable.SQLiteMarkdown.Common
{
    /// <summary>
    /// Concrete implementation of <see cref="IUtcEpochClock"/> that exposes
    /// a captured UTC timestamp and its component parts.
    /// </summary>
    /// <remarks>
    /// <see cref="UtcEpochNow"/> acts as the authoritative snapshot of time.
    /// When updated, all derived temporal components (Year, Month, Day,
    /// Hour, Minute, Second) are synchronized and raise change notifications.
    /// 
    /// This design allows consumers to reason against a stable, externally
    /// supplied moment in time, preventing race conditions that can arise
    /// from repeatedly querying <see cref="DateTimeOffset.UtcNow"/>.
    /// 
    /// The clock is intentionally passive; it does not advance itself.
    /// Time progression must be driven by the host.
    /// </remarks>
    public class UtcEpochClock : IUtcEpochClock
    {
        public virtual void Start() 
        {
            bool start;
            lock(_lock)
            {
                start = !_run;
                _run = true;
            }
            if (start)
            {
                _ = RunAsync();
            }
        }
        public virtual void Stop() => _run = false;

        private async Task RunAsync()
        {
            while(_run)
            {
                UtcEpochNow = DateTimeOffset.UtcNow;
                await Task.Delay(TimeSpan.FromSeconds(0.25));
            }
        }

        public DisposableHost DHostSuspend { get; } = new();

        protected bool _run = false;

        private readonly object _lock = new();

        public DateTimeOffset UtcEpochNow
        {
            get => _utcEpochNow;
            set
            {
                if (!Equals(_utcEpochNow, value))
                {
                    _utcEpochNow = value;

                    Second = _utcEpochNow.Second;
                    Minute = _utcEpochNow.Minute;
                    Hour = _utcEpochNow.Hour;
                    Day = _utcEpochNow.Day;
                    Month = _utcEpochNow.Month;
                    Year = _utcEpochNow.Year;

                    OnPropertyChanged();
                }
            }
        }
        DateTimeOffset _utcEpochNow = default;

        public int Second
        {
            get => _second;
            set
            {
                if (!Equals(_second, value))
                {
                    _second = value;
                    OnPropertyChanged();
                }                
            }
        }
        int _second = default;
        bool _newSecond = false;
        public int Minute
        {
            get => _minute;
            set
            {
                if (!Equals(_minute, value))
                {
                    _minute = value;
                    OnPropertyChanged();
                }
            }
        }
        int _minute = default;

        public int Hour
        {
            get => _hour;
            set
            {
                if (!Equals(_hour, value))
                {
                    _hour = value;
                    OnPropertyChanged();
                }
            }
        }
        int _hour = default;

        public int Day
        {
            get => _day;
            set
            {
                if (!Equals(_day, value))
                {
                    _day = value;
                    OnPropertyChanged();
                }
            }
        }
        int _day = default;

        public int Month
        {
            get => _month;
            set
            {
                if (!Equals(_month, value))
                {
                    _month = value;
                    OnPropertyChanged();
                }
            }
        }
        int _month = default;

        public int Year
        {
            get => _year;
            set
            {
                if (!Equals(_year, value))
                {
                    _year = value;
                    OnPropertyChanged();
                }
            }
        }
        int _year = default;


        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
