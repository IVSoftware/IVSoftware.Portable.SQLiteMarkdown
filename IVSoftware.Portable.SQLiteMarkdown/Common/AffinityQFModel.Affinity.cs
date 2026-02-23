using IVSoftware.Portable.Common.Exceptions;
using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using EphemeralAttribute = SQLite.IgnoreAttribute;

namespace IVSoftware.Portable.SQLiteMarkdown.Common
{
    partial class AffinityQFModel : IAffinityItem
    {
        public void UpdateUtc(
            DateTimeOffset? affinityUtcNow,
            Dictionary<string, object?>? affinities = null)
        {
            affinities ??= new Dictionary<string, object?>();
            _affinityUtcNow = affinityUtcNow;
            if (affinities.Count == 0)
            {
                UtcStart = affinityUtcNow?.FloorToSecond();
            }
        }

        /// <summary>
        /// Local copy for internals to build against and that detects initial or non-affinity states.
        /// </summary>
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
            get
            {
                if (TryGetSafePath(_path, Id, out var safePath))
                {
                    _path = safePath;
                }
                else
                {
                    this.ThrowPolicyException(AffinityPolicy.IdMustBeGuid);
                }
                return _path;
            }
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
                        $"Policy violation for {nameof(AffinityQFModel)}.{nameof(Path)}: This value must end with Id");
                }
            }
        }
        string _path = string.Empty;

        public bool IsRoot => string.Equals(Path, Id, StringComparison.OrdinalIgnoreCase);

        private static bool TryGetSafePath(string path, string id, out string safePath)
        {
            safePath = id; // default fallback

            // Id must be a valid Guid
            Guid idGuid;
            if (!Guid.TryParse(id, out idGuid))
                return false;

            var raw = string.IsNullOrWhiteSpace(path) ? id : path;

            var parts = raw.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length == 0)
                return true; // fallback already set to id

            // Every segment must be a Guid
            for (int i = 0; i < parts.Length; i++)
            {
                Guid parsed;
                if (!Guid.TryParse(parts[i], out parsed))
                    return true; // fallback to id
            }

            // Last segment must match Id
            var last = parts[parts.Length - 1];
            if (!string.Equals(last, id, StringComparison.OrdinalIgnoreCase))
                return true; // fallback to id

            safePath = string.Join("/", parts);
            return true;
        }

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

        /// <summary>
        /// Authoritative duration commitment for the item.
        /// </summary>
        /// <remarks>
        /// - Duration and Remaaining are unlike other temporal fields; they do not 
        ///   yield, reset, or null in response to affinity mode itself becoming null.
        /// - This way, should the item come back into the temporal field domain it
        ///   will find the promise - and any partial delivery - intact.
        /// - Setting Duration resets Remaining to maintain internal consistency.
        /// </remarks>
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
                if (value is { } remaining && remaining < TimeSpan.Zero)
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
        AffinityMode? _utcEpochMode = null;


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
        string? _utcParent = null;

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


        IList<AffinitySlot> IAffinityItem.Slots => Slots;
        public List<AffinitySlot> Slots { get; } = new List<AffinitySlot>();

        #region A F F I N I T Y    E P H E M E R A L
        [Ephemeral]
        public AffinityTimeDomain? AffinityTimeDomain => null;

        [Ephemeral]
        public DateTimeOffset? UtcEnd
        {
            get
            {
                if (AffinityMode is null
                    || _utcStart is null)
                {
                    return null;
                }
                else
                {
                    return UtcStart + Duration;
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

        protected override void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            base.OnPropertyChanged(propertyName);
        }
    }
}
