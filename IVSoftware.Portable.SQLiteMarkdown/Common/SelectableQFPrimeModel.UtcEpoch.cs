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
        public UtcEpochMode? UtcEpochMode =>
            UtcStart is not null
            ? SQLiteMarkdown.UtcEpochMode.Fixed
            : Duration is not null
                ? SQLiteMarkdown.UtcEpochMode.Asap
                : null;

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
                }
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

        public UtcChildMode? UtcChildMode
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
        UtcChildMode? _utcChildMode = default;

        public UtcEpochTimeDomain? UtcEpochTimeDomain => throw new NotImplementedException();

        public List<UtcEpochSlot> Slots { get; } = new List<UtcEpochSlot>();
    }
}
