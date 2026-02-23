using IVSoftware.Portable.Common.Exceptions;
using SQLite;
using System;
using System.Collections.Generic;
using EphemeralAttribute = SQLite.IgnoreAttribute;

namespace IVSoftware.Portable.SQLiteMarkdown.Common
{
    partial class SelectableQFAffinityModel : IAffinityItem
    {
        public void UpdateUtc(
            DateTimeOffset? affinityUtcNow,
            Dictionary<string, DateTimeOffset> affinities)
        {
            _affinityUtcNow = affinityUtcNow;
        }
        private DateTimeOffset? _affinityUtcNow;

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
                        $"Policy violation for {nameof(SelectableQFAffinityModel)}.{nameof(Path)}: This value must end with Id");
                }
            }
        }
        string _path = string.Empty;

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

        
        public TimeSpan Duration
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
        TimeSpan _duration = default;


        public TimeSpan Remaining
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
        TimeSpan _remaining = default;


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

        public List<AffinitySlot> Slots { get; } = new List<AffinitySlot>();

        #region A F F I N I T Y    E P H E M E R A L
        [Ephemeral]
        public AffinityTimeDomain? AffinityTimeDomain => null;

        [Ephemeral]
        public DateTimeOffset? UtcEnd
        {
            get
            {
                if (_utcStart is null)
                {
                    return null;
                }
                else
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
                }
            }
        }

        [Ephemeral]
        public bool? IsDone
        {
            get =>
                _affinityUtcNow is null
                ? null 
                : _isDone;
            set
            {
                if (!Equals(_isDone, value))
                {
                    _isDone = value == true;
                    OnPropertyChanged();
                }
            }
        }
        bool _isDone = false;


        [Ephemeral]
        public bool? IsDonePendingConfirmation =>
            IsDone switch
            {
                // Not officially IsDone, but the time has ticked down to zero.
                false => Remaining == TimeSpan.Zero,

                // Affirmatively IsDone (this affirmatively *also* sets remaining to zero.
                true => false,

                // IsDone is presumed null and so is this.
                _ => null,
            };


        [Ephemeral]
        public bool? IsPastDue => null;

        [Ephemeral]
        public TimeSpan? Available
        {
            get => _available;
            set
            {
                if (!Equals(_available, value))
                {
                    _available = value;
                    OnPropertyChanged();
                }
            }
        }
        TimeSpan? _available = default;

        #endregion A F F I N I T Y    E P H E M E R A L
    }
}
