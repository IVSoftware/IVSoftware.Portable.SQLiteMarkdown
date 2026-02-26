using IVSoftware.Portable.Common.Exceptions;
using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using EphemeralAttribute = SQLite.IgnoreAttribute;

namespace IVSoftware.Portable.SQLiteMarkdown.Common
{
    partial class AffinityQFModel : IAffinityItem
    {
        public void UpdateAffinityUtcNow(
            DateTimeOffset? affinityUtcNow,
            Dictionary<AffinityRole, object?>? affinities = null)
        {
            _affinities ??= new Dictionary<AffinityRole, object?>();
            _affinityUtcNow = affinityUtcNow;
        }

        protected virtual void UpdateAffinityUtcNow()
        {

        }

        /// <summary>
        /// Active affinity context for temporal and role-based coordination.
        /// </summary>
        /// <remarks>
        /// Never null. Replaced atomically.
        /// </remarks>
        protected Dictionary<AffinityRole, object?> Affinities
        {
            get => _affinities!;
            set => _affinities = value ??= new();
        }
        Dictionary<AffinityRole, object?> _affinities = new();


        /// <summary>
        /// Captured affinity time for protected calculations.
        /// </summary>
        protected DateTimeOffset? AffinityUtcNow
        {
            get => _affinityUtcNow;
            set
            {
                if (!Equals(_affinityUtcNow, value))
                {
                    _affinityUtcNow = value;
                    UpdateAffinityUtcNow();
                }
            }
        }
        private DateTimeOffset? _affinityUtcNow = default;

        /// <summary>
        /// Materialized Path Policy.
        /// </summary>
        public string ParentPath
        {
            get => _parentPath;
            set
            {
                value ??= string.Empty;
                if (!Equals(_parentPath, value))
                {
                    _parentPath = value;
                    _parentId = _parentPath.Split('\\').Last();
                    OnPropertyChanged();
                }
            }
        }
        string _parentPath = string.Empty;

        public string ParentId
        {
            get => _parentId;
            set { }
        }
        string _parentId = string.Empty;


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
                    UpdateAffinityUtcNow();
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

        /// <summary>
        /// Priority ticks that are also the timebase for AffinityMode.Fixed flags.
        /// </summary>
        public long Priority
        {
            get
            {
                if (_priority == 0)
                {
                    _priority = Created.UtcTicks;
                }
                return _priority;
            }
            set
            {
                if (!Equals(_priority, value))
                {
                    _priority = value;
                    OnPropertyChanged();
                }
            }
        }
        long _priority = 0;

        #region A F F I N I T Y    E P H E M E R A L


        [Ephemeral]
        public DateTimeOffset? UtcStart
        {
            get
            {
                switch (AffinityMode)
                {
                    case null:
                        break;
                    case SQLiteMarkdown.AffinityMode.Asap:
                        break;
                    case SQLiteMarkdown.AffinityMode.FixedTime:
                        break;
                    case SQLiteMarkdown.AffinityMode.FixedDate:
                        break;
                    case SQLiteMarkdown.AffinityMode.FixedDateAndTime:
                        break;
                    default:
                        break;
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

        [Ephemeral]
        public AffinityTimeDomain? AffinityTimeDomain => null;

        [Ephemeral]
        public bool IsRoot => string.IsNullOrWhiteSpace(ParentId); 
        
        [Ephemeral]
        public bool IsTimeDomainEnabled => !(_affinityUtcNow is null || AffinityMode is null);

        [Ephemeral]
        public DateTimeOffset? UtcEnd
        {
            get
            {
                if (IsTimeDomainEnabled)
                {
                    return UtcStart + Duration;
                }
                else
                {
                    return null;
                }
            }
        }

        [Ephemeral]
        public bool? IsDone
        {
            get
            {
                if (IsTimeDomainEnabled)
                {
                    return _isDone;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if (!Equals(_isDone, value))
                {
                    _isDone = value == true;
                    OnPropertyChanged();
                }
            }
        }
        bool? _isDone = false;

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
            switch (propertyName)
            {
                case nameof(AffinityMode):
                    UpdateAffinityUtcNow();
                    break;
            }
        }
    }
}
