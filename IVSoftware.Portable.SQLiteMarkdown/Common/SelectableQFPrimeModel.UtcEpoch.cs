using IVSoftware.Portable.Common.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;

namespace IVSoftware.Portable.SQLiteMarkdown.Common
{
    partial class SelectableQFPrimeModel : IAffinityItem
    {
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

        /// <summary>
        /// Globally-unique identifier that defaults to Materialized Path Policy.
        /// </summary>
        public string Path
        {
            get => _path ?? Id;
            set
            {
                if (value?.EndsWith(Id) == true)
                {
                    if (!Equals(_path, value))
                    {
                        _path = value;
                        OnPropertyChanged();
                    }
                }
                else
                {
                    this.ThrowHard<InvalidOperationException>(
                        $"Policy violation for {nameof(SelectableQFPrimeModel)}.{nameof(Path)}: This value must end with Id");
                }
            }
        }
        string _path = string.Empty;

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


        public AffinityMode? AffinityMode
        {
            get => _utcEpochMode;
            set
            {
                if (!Equals(_utcEpochMode, value))
                {
                    _utcEpochMode = value;
                    switch (AffinityMode)
                    {
                        case null:
                            break;
                        case SQLiteMarkdown.AffinityMode.FixedDateAndTime:
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
                switch (AffinityMode)
                {
                    case null:
                        break;
                    case SQLiteMarkdown.AffinityMode.FixedDateAndTime:
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

        public string? AffinityParent
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

        public ChildAffinityMode? AffinityChildMode
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

        public AffinityTimeDomain? AffinityTimeDomain => throw new NotImplementedException();

        public List<AffinitySlot> Slots { get; } = new List<AffinitySlot>();
    }
}
