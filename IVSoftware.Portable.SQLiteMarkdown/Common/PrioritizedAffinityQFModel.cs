using IVSoftware.Portable.Common.Exceptions;
using IVSoftware.Portable.Xml.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml.Linq;
using EphemeralAttribute = SQLite.IgnoreAttribute;

namespace IVSoftware.Portable.SQLiteMarkdown.Common
{
    internal partial class PrioritizedAffinityQFModel : SelectableQFModel, IPrioritizedAffinity
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
            get => _priorityOverride;
            set
            {
                if (!Equals(_priorityOverride, value))
                {
                    _priorityOverride = value;
                    OnPropertyChanged();
                }
            }
        }
        long? _priorityOverride = default;

        #region A F F I N I T Y    E P H E M E R A L

        [Ephemeral]
        public bool IsRoot => string.IsNullOrWhiteSpace(ParentId);
        #endregion A F F I N I T Y    E P H E M E R A L


        public bool IndentLess()
        {
            throw new NotImplementedException();
        }

        public bool IndentMore()
        {
            throw new NotImplementedException();
        }

        public bool MoveDown()
        {
            throw new NotImplementedException();
        }

        public bool MoveUp()
        {
            throw new NotImplementedException();
        }

        protected override void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            base.OnPropertyChanged(propertyName);
        }
    }
}
