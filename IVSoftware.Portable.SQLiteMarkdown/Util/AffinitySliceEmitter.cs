using IVSoftware.Portable.Common.Exceptions;
using IVSoftware.Portable.Disposable;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace IVSoftware.Portable.SQLiteMarkdown.Util
{
    /// <summary>
    /// Concrete implementation of <see cref="IAffinitySliceEmitter"/> that exposes
    /// a captured UTC timestamp and its component parts.
    /// </summary>
    /// <remarks>
    /// <see cref="AffinityEpochTimeSource"/> acts as the authoritative snapshot of time.
    /// When updated, all derived temporal components (Year, Month, Day,
    /// Hour, Minute, Second) are synchronized and raise change notifications.
    /// 
    /// This design allows consumers to reason against a stable, externally
    /// supplied moment in time, preventing race conditions that can arise
    /// from repeatedly querying <see cref="DateTimeOffset.UtcNow"/>.
    /// </remarks>
    internal class AffinitySliceEmitter : IAffinitySliceEmitter
    {
        private static readonly IAffinitySliceEmitter _system = new AffinitySliceEmitter();
        public static IAffinitySliceEmitter System => _system;

        protected int _run = 0;

        public virtual void Start()
        {
            if (Interlocked.Exchange(ref _run, 1) == 0)
            {
                _ = RunAsync();
            }
        }

        public virtual void Stop()
        {
            Volatile.Write(ref _run, 0);
        }

        private async Task RunAsync()
        {
            while(Volatile.Read(ref _run) == 1)
            {
                DisplayTime = AffinityEpochTimeSource = DateTimeOffset.UtcNow;
                await Task.Delay(250);
            }
        }

        public DisposableHost DHostSuspend { get; } = new();

        public DateTimeOffset AffinityEpochTimeSource
        {
            get => _utcEpochNow;
            set
            {
                if (DHostSuspend.IsZero())
                {
                    if (Interlocked.Exchange(ref _busy, 1) == 0)
                    { 
                        try
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
                        catch (Exception ex)
                        {
                            this.RethrowSoft(ex);
                        }
                        finally
                        {
                            Volatile.Write(ref _busy, 0);
                        }
                    }
                }
            }
        }
        private int _busy = 0;

        DateTimeOffset _utcEpochNow = default; 

        public DateTimeOffset DisplayTime
        {
            get => _displayTime;
            set
            {
                value = new DateTimeOffset(value.Ticks - (value.Ticks % TimeSpan.TicksPerSecond), value.Offset);
                if (!Equals(_displayTime, value))
                {
                    _displayTime = value;
                    OnPropertyChanged();
                }
            }
        }
        DateTimeOffset _displayTime;


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
