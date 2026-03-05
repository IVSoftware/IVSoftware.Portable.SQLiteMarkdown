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
        /// Assign or consolidate the XOP associated with this model.
        /// </summary>
        /// <remarks>
        /// A parented XOP is authoritative and cannot be replaced.
        /// - Replacement is allowed only while the current XOP is unparented.
        /// - If the current XOP is already parented, only attribute consolidation may occur.
        /// - An error is raised if both the current XOP and the incoming value are already parented.
        /// </remarks>
        [Ephemeral, JsonIgnore]
        public XElement XAF
        {
            get => _model;
            set => SetXAFAuthority(value);
        }

        protected virtual void SetXAFAuthority(XElement value)
        {
            if (value is null)
            {
                this.ThrowHard<ArgumentNullException>(
                    $"{nameof(XAF)} cannot be set to null.");
            }
            else
            {
                if (ReferenceEquals(value, _model))
                {   /* G T K */
                    // Unexpected but benign.
                }
                else
                {
                    if (_model.Parent is not null && value.Parent is not null)
                    {
                        this.ThrowHard<InvalidOperationException>(
                            $"{nameof(XAF)} cannot be consolidated because both the current and" +
                            $" incoming {nameof(XAF)} instances are already parented. " +
                            $"A parented {nameof(XAF)} is authoritative and cannot be replaced.");
                    }
                    else
                    {
                        if (value.Parent is not null)
                        {
                            var xopSwap = _model;
                            _model = value;
                            value = xopSwap;
                        }
                        TransferXAFXBOAuthority(value.Attributes().ToArray());
                    }
                }
            }
        }

        XElement _model = new XElement(nameof(StdMarkdownElement.model));

        protected virtual void TransferXAFXBOAuthority(XAttribute[] srce)
        {
            foreach (var attrSrce in srce)
            {
#if ABSTRACT_FORWARD_REFERENCE
                // OnePage snippet for unifying OPID.
                if (attrSrce.Name.LocalName == nameof(StdXAttributeName.opid))
                {
                    if (attrSrce is XBoundAttribute xba && xba.Tag is Enum opid)
                    {
                        // Subject to immutability rules.
                        OPID = opid;
                    }
                    else
                    {
                        // Ignore this attribute without correcting it.
                        continue;
                    }
                }
#endif
                switch (attrSrce)
                {
                    case XBoundAttribute xba:
                        if (_model.Attribute(attrSrce.Name) is { } xReplace)
                        {
                            xReplace.Remove();
                        }
                        _model.Add(new XBoundAttribute(xba));
                        break;
                    default:
                        _model.SetAttributeValue(attrSrce.Name, attrSrce.Value);
                        break;
                }
            }
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

        [Ephemeral]
        public string FullPath => ParentPath.LintCombinedSegments(Id);

        /// <summary>
        /// Materialized Path Policy.
        /// </summary>
        public string ParentPath
        {
            get => _parentPath;
            set
            {
                value = value.LintCombinedSegments();
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

        public long? PriorityOverride
        {
            get => _transientPriority;
            set
            {
                if (!Equals(_transientPriority, value))
                {
                    _transientPriority = value;
                    OnPropertyChanged();
                }
            }
        }
        long? _transientPriority = default;


        #region A F F I N I T Y    E P H E M E R A L
        [Ephemeral]
        public DateTimeOffset? UtcStart
        {
            get
            {
                switch (TemporalAffinity)
                {
                    case null:
                        break;
                    case SQLiteMarkdown.TemporalAffinity.Asap:
                        break;
                    case SQLiteMarkdown.TemporalAffinity.FixedTime:
                        break;
                    case SQLiteMarkdown.TemporalAffinity.FixedDate:
                        break;
                    case SQLiteMarkdown.TemporalAffinity.FixedDateAndTime:
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
        public bool IsRoot => string.IsNullOrWhiteSpace(ParentId); 


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
