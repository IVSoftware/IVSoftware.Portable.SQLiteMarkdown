using System;
using System.Collections.Generic;
using System.Text;

namespace IVSoftware.Portable.SQLiteMarkdown.Common
{
    partial class SelectableQFPrimeModel
    {
        public DateTimeOffset? UtcStart
        {
            get => _utcStart;
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


        /// <summary>
        /// Self-qualifying heuristic for mode.
        /// </summary>
        public UtcEpochMode? UtcEpochMode
        {
            get
            {
                switch (_utcEpochMode)
                {
                    case SQLiteMarkdown.UtcEpochMode.Fixed:
                        // Requires a fixed start time.
                        if(UtcStart is null)
                        {
                            return null;
                        }
                        break;                        
                    case SQLiteMarkdown.UtcEpochMode.Asap:
                        break;
                    case SQLiteMarkdown.UtcEpochMode.AsapBefore:
                    case SQLiteMarkdown.UtcEpochMode.AsapAfter:
                        // Requires a parent to pin to.
                        if (UtcParent is null)
                        {
                            return SQLiteMarkdown.UtcEpochMode.Asap;
                        }
                        break;
                }
                return _utcEpochMode;
            }
            set
            {
                if (!Equals(_utcEpochMode, value))
                {
                    _utcEpochMode = value;
                    OnPropertyChanged();
                }
            }
        }
        UtcEpochMode? _utcEpochMode = default;


        public DateTimeOffset? UtcEnd
        {
            get
            {
                switch (UtcEpochMode)
                {
                    case SQLiteMarkdown.UtcEpochMode.Fixed:
                        break;
                    case SQLiteMarkdown.UtcEpochMode.Asap:
                        break;
                    case SQLiteMarkdown.UtcEpochMode.AsapBefore:
                        break;
                    case SQLiteMarkdown.UtcEpochMode.AsapAfter:
                        break;
                    default:
                        break;
                }
                return null;
            }
        }

        public bool? IsDone =>
            Remaining is { } remaining
            ? remaining.Ticks == 0
            : null; 

        public UtcEpochTimeDomain? UtcEpochTimeDomain => throw new NotImplementedException();

        public bool? IsRunning { get; set; }

        public string? UtcParent { get; set; }

        public List<UtcEpochSlot> Slots { get; } = new List<UtcEpochSlot>();

        public IUtcEpochClock UtcEpochClock
        {
            get
            {
                if (_utcEpochClock is null)
                {
                    _utcEpochClock = new UtcEpochClock();
                }
                return _utcEpochClock;
            }
        }
        IUtcEpochClock? _utcEpochClock = null;
    }
}
