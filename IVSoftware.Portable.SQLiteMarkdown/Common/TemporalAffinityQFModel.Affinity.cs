using IVSoftware.Portable.Collections.Common;
using IVSoftware.Portable.Common.Exceptions;
using IVSoftware.Portable.Xml.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Xml.Linq;
using EphemeralAttribute = SQLite.IgnoreAttribute;

namespace IVSoftware.Portable.SQLiteMarkdown.Common
{
    [Table("items")]
    internal partial class TemporalAffinityQFModel 
        : PrioritizedAffinityQFModel
        , ITemporalAffinity
    {

        /// <summary>
        /// Authoritative duration commitment for the item.
        /// </summary>
        /// <remarks>
        /// - Duration and Remaining are unlike other temporal fields; they do not 
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


        public TemporalAffinity? TemporalAffinity
        {
            get => _TemporalAffinity;
            set
            {
                if (!Equals(_TemporalAffinity, value))
                {
                    _TemporalAffinity = value;
                    OnPropertyChanged();
                }
            }
        }
        TemporalAffinity? _TemporalAffinity = null;

        public TemporalChildAffinity? TemporalChildAffinity
        {
            get => _temporalChildAffinity;
            set
            {
                if (!Equals(_temporalChildAffinity, value))
                {
                    _temporalChildAffinity = value;
                    OnPropertyChanged();
                }
            }
        }
        TemporalChildAffinity? _temporalChildAffinity = default;


        [Ephemeral]
        public TemporalAffinityTimeDomain? TemporalAffinityCurrentTimeDomain => null;

        IList<AffinitySlot> ITemporalAffinity.Slots => Slots;
        public List<AffinitySlot> Slots { get; } = new List<AffinitySlot>();


        #region A F F I N I T Y    E P H E M E R A L
        [Ephemeral]
        public DateTimeOffset? UtcStart
        {
            get
            {
                if (TemporalAffinity is null)
                {

                }
                else
                {
                    switch (TemporalAffinity)
                    {
                        case IVSoftware.Portable.Collections.Common.TemporalAffinity.Asap:
                            break;
                        case IVSoftware.Portable.Collections.Common.TemporalAffinity.FixedTime:
                            break;
                        case IVSoftware.Portable.Collections.Common.TemporalAffinity.FixedDate:
                            break;
                        case IVSoftware.Portable.Collections.Common.TemporalAffinity.FixedDateAndTime:
                            break;
                        default:
                            break;
                    }
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

        public DateTimeOffset? UtcEnd
        {
            get => 
                TemporalAffinity is null
                ? null 
                : _utcEnd;
            set
            {
                if (!Equals(_utcEnd, value))
                {
                    _utcEnd = value;
                    OnPropertyChanged();
                }
            }
        }
        DateTimeOffset? _utcEnd = default;

        [Ephemeral]
        public TimeSpan? AvailableTimeSpan
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


        [Ephemeral]
        public bool? IsDone
        {
            get =>
                TemporalAffinity is null
                ? null
                : IsChecked;
            set
            {
                if (value is not null && !Equals(IsChecked, value))
                {
                    IsChecked = value == true;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Set the IsDone = True to explicitly check an item off the list.
        /// </summary>
        /// <remarks>
        /// Mental Model: 
        /// - Fixed items eventually run out of remaining time. 
        /// - Such items can be auto-checked but that must be opt-in.
        /// - The danger is that, due to filtering, an auto checked item
        ///   might silently fall out of visiblity, without a human
        ///   confirmation that the task was actually completed.
        /// - This property supports a keep alive that can signal warning 
        ///   semantics when the determinate state of OOT is true.
        /// </remarks>
        [Ephemeral]
        public bool? OutOfTime
        {
            get => _outOfTime;
            set
            {
                if (!Equals(_outOfTime, value))
                {
                    _outOfTime = value;
                    OnPropertyChanged();
                }
            }
        }
        bool? _outOfTime = false;


        [Ephemeral]
        public bool? IsPastDue => null;

        TimeSpan? _available = default;
        #endregion A F F I N I T Y    E P H E M E R A L

        protected override void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            base.OnPropertyChanged(propertyName);
        }
    }
}
