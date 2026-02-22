using System;
using System.Collections.Generic;
using System.Text;

namespace IVSoftware.Portable.SQLiteMarkdown.Common
{
    partial class SelectableQFPrimeModel : IAffinityItem
    {
        public DateTimeOffset? UtcStart
        {
            get
            {
                if (_utcStart is null)
                {
                    _utcStart = Created;
                }
                return _utcStart;
            }
            set
            {
                if (!Equals(_utcStart, value))
                {
                    _utcStart = value;
                    OnPropertyChanged();
                }
            }
        }
        DateTimeOffset? _utcStart = default;

        public long Position
        {
            get
            {
                if (_position == 0)
                {
                    _position = Created.UtcTicks;
                }
                return _position;
            }
            set
            {
                if (!Equals(_position, value))
                {
                    _position = value;
                    OnPropertyChanged();
                }
            }
        }
        long _position = 0;

        public TimeSpan? Duration
        {
            get => _duration;
            set
            {
                if (!Equals(_duration, value))
                {
                    _duration = value;
                    // Duration performs a hard reset on Remaining.
                    Remaining = value;
                    OnPropertyChanged();
                }
            }
        }
        TimeSpan? _duration = default;


        public TimeSpan? Remaining
        {
            get => _remaining;
            set
            {
                if(value is { } remaining && remaining < TimeSpan.Zero)
                {
                    value = TimeSpan.Zero;
                }
                if (!Equals(_remaining, value))
                {
                    _remaining = value;
                    OnPropertyChanged();
                }
            }
        }
        TimeSpan? _remaining = default;


        public AffinityMode? UtcEpochMode
        {
            get => _utcEpochMode;
            set
            {
                if (!Equals(_utcEpochMode, value))
                {
                    _utcEpochMode = value;
                    switch (UtcEpochMode)
                    {
                        case null:
                            break;
                        case SQLiteMarkdown.AffinityMode.Fixed:
                            break;
                        case SQLiteMarkdown.AffinityMode.Asap:
                            break;
                    }
                    OnPropertyChanged();
                }
            }
        }
        AffinityMode? _utcEpochMode = default;



        public DateTimeOffset? UtcEnd
        {
            get
            {
                switch (UtcEpochMode)
                {
                    case null:
                        break;
                    case SQLiteMarkdown.AffinityMode.Fixed:
                        break;
                    case SQLiteMarkdown.AffinityMode.Asap:
                        break;
                }
                throw new NotImplementedException("ToDo");
                return null;
            }
        }

        public bool? IsDone =>
            Remaining is { } remaining
            ? remaining.Ticks == 0
            : null; 


        public bool? IsRunning { get; set; }

        public string? UtcParent
        {
            get => _utcParent;
            set
            {
                if (!Equals(_utcParent, value))
                {
                    _utcParent = value;
                    OnPropertyChanged();
                }
            }
        }
        string? _utcParent = string.Empty;

        public ChildAffinityMode? UtcChildMode
        {
            get => _utcChildMode;
            set
            {
                if (!Equals(_utcChildMode, value))
                {
                    _utcChildMode = value;
                    OnPropertyChanged();
                }
            }
        }
        ChildAffinityMode? _utcChildMode = default;

        public AffinityTimeDomain? UtcEpochTimeDomain => throw new NotImplementedException();

        public List<UtcEpochSlot> Slots { get; } = new List<UtcEpochSlot>();
    }
}
