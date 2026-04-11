using IVSoftware.Portable.Common.Exceptions;
using IVSoftware.Portable.Xml.Linq;
using IVSoftware.Portable.Xml.Linq.Collections;
using IVSoftware.Portable.Xml.Linq.XBoundObject;
using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;

namespace IVSoftware.Portable.SQLiteMarkdown.Internal
{
    /// <summary>
    /// Internal extensions supporting advisory and policy-driven exception flow.
    /// </summary>
    /// <remarks>
    /// These helpers currently host the `ThrowOrAdvise` style dispatch used
    /// throughout the MarkdownContext pipeline. The mechanism is stable but
    /// intentionally scoped to this package while the broader integration
    /// with IVSoftware.Portable.Xml.Linq evolves.
    ///
    /// When the upstream library formally adopts this capability, these
    /// extensions are expected to migrate there and be removed from this
    /// assembly.
    ///
    /// Until that time they remain **internal by design** to prevent the
    /// provisional API surface from leaking into consumer code.
    /// </remarks>
    internal static partial class Extensions
    {
        #region P R E V I E W
        /// <summary>
        /// Returns the <see cref="XBoundAttribute"/> for the specified <see cref="StdModelAttribute"/>.
        /// </summary>
        /// <remarks>
        /// Resolves the attribute using the enum name. If the attribute is missing or not bound
        /// as an <see cref="XBoundAttribute"/>, behavior is governed by <paramref name="throw"/>.
        /// Advisory or unspecified returns <c>null</c>.
        /// </remarks>
        public static XBoundAttribute? XBoundAttribute(
            this XElement @this,
            StdModelAttribute stdEnum,   // This type *only* by design. Do not generalize to Enum.
            ThrowOrAdvise? @throw = null)
        {
            string msg;
            if (@this.Attribute(stdEnum.ToString()) is not XAttribute attr)
            {
                msg = $"The {stdEnum.ToFullKey()} attribute was not found.";
            }
            else if (attr is not XBoundAttribute xba)
            {
                msg = $"The {stdEnum.ToFullKey()} attribute exists but is not an {nameof(XBoundAttribute)}.";
            }
            else
            {
                return xba;
            }
            switch (@throw)
            {
                case ThrowOrAdvise.ThrowHard:
                    @this.ThrowHard<InvalidOperationException>(msg);
                    break;
                case ThrowOrAdvise.ThrowSoft:
                    @this.ThrowSoft<InvalidOperationException>(msg);
                    break;
                case ThrowOrAdvise.ThrowFramework:
                    @this.ThrowFramework<InvalidOperationException>(msg);
                    break;
                case ThrowOrAdvise.Advisory:
                    @this.Advisory(msg);
                    break;
                case null:
                default: break;
            }
            return null;
        }

        /// <summary>
        /// Returns the typed <c>Tag</c> value of the <see cref="XBoundAttribute"/> identified by the specified <see cref="StdModelAttribute"/>.
        /// </summary>
        /// <remarks>
        /// Resolves the attribute via <see cref="XBoundAttribute(XElement, StdModelAttribute, ThrowOrAdvise?)"/>.
        /// If the attribute is missing, not bound, or its <c>Tag</c> is not assignable to
        /// <typeparamref name="T"/>, the result is <c>default</c>.
        /// </remarks>
        public static T? XBoundAttributeValue<T>(
            this XElement @this,
            StdModelAttribute stdEnum,   // This type *only* by design. Do not generalize to Enum.
            ThrowOrAdvise? @throw = null)
        {
            if (@this.XBoundAttribute(stdEnum, @throw) is { } xba && xba.Tag is T valueT)
            {
                return valueT;
            }
            return default;
        }

        public static string PadToMaxLength(
            this string @string,
            out bool isMaxLengthExceeded,
            byte maxLength = byte.MaxValue,
            bool padToMaxLength = false)
        {
            isMaxLengthExceeded = @string.Length > maxLength;
            if (isMaxLengthExceeded)
            {
                @string = @string.Substring(0, maxLength);
            }
            else if (padToMaxLength)
            {
                @string = @string.PadRight(maxLength);
            }
            return @string;
        }

        public static string PadToMaxLength(
            this string @string,
            byte maxLength = byte.MaxValue,
            bool padToMaxLength = false)
            => @string.PadToMaxLength(out _, maxLength, padToMaxLength);

        // Fluent event attacher. Internal only; goes in Collections, with Preview semantics.
        public static T WithCollectionChangeHandler<T>(this T @this, NotifyCollectionChangedEventHandler handler)
            where T : INotifyCollectionChanged
        {
            ((INotifyCollectionChanged)@this).CollectionChanged += handler;
            return @this;
        }
        #endregion  P R E V I E W

        #region L E G I T
        public static XElement RemoveNodes(this XElement @this, Enum reset, params Enum[] moreResets)
        {
            @this.RemoveNodes();
            foreach (Enum @enum in new[] { reset }.Concat(moreResets))
            {
                @this.SetAttributeValue(@enum.ToString(), 0);
            }
            return @this;
        }

        /// <summary>
        /// Inserts the specified attribute at the first position.
        /// </summary>
        /// <remarks>
        /// - Existing attributes are removed and re-added to enforce ordering.
        /// - Any prior attribute with the same local name is replaced.
        /// - Attribute identity is determined by Name.LocalName.
        /// - Preserves relative order of remaining attributes.
        /// </remarks>
        public static XElement AddFirst(this XElement @this, XAttribute attr)
        {
            var attrsB4 = @this.Attributes().ToArray();
            @this.RemoveAttributes();

            @this.Add(attr);

            foreach (var attrB4 in attrsB4.Where(_ => _.Name.LocalName != attr.Name.LocalName))
            {
                @this.Add(attrB4);
            }
            return @this;
        }

        /// <summary>
        /// Moves or inserts the specified attribute to a given index.
        /// </summary>
        /// <remarks>
        /// - Rebuilds the attribute list to enforce positional ordering.
        /// - Any prior attribute with the same local name is replaced.
        /// - Index is clamped to the valid attribute range.
        /// - Attribute identity is determined by Name.LocalName.
        /// - Preserves relative order of unaffected attributes.
        /// </remarks>
        public static XElement Move(this XElement @this, XAttribute attr, int index)
        {
            var attrsB4 = @this.Attributes().ToList();

            // Remove any existing attribute with same name
            attrsB4.RemoveAll(_ => _.Name.LocalName == attr.Name.LocalName);

            // Clamp index
            if (index < 0) index = 0;
            if (index > attrsB4.Count) index = attrsB4.Count;

            // Insert at desired position
            attrsB4.Insert(index, attr);

            // Rebuild
            @this.RemoveAttributes();
            foreach (var a in attrsB4)
            {
                @this.Add(a);
            }

            return @this;
        }


        #region A C T I O N    M A S K S
        [Obsolete("Action and Reason are entirely separate concerns in v2.0")]
        public static NotifyCollectionChangedAction ToNotifyCollectionChangedAction(this Enum @this)
        {
            var preview = (NotifyCollectionChangedAction) Enum.ToObject(typeof(NotifyCollectionChangedAction), Convert.ToInt32(@this) & 0x07);
            Debug.Assert(Equals(@this, preview), "Expecting values are no longer OR'ed");
            return preview;
        }

        [Obsolete("Action and Reason are entirely separate concerns in v2.0")]
        public static NotifyCollectionChangeReason ToNotifyCollectionChangedReason(this Enum @this)
        {
            var preview = (NotifyCollectionChangeReason) Enum.ToObject(typeof(NotifyCollectionChangeReason), Convert.ToInt32(@this) & ~0x07);
            Debug.Assert(Equals(@this, preview), "Expecting values are no longer OR'ed");
            return preview;
        }
        #endregion A C T I O N    M A S K S

        #endregion L E G I T
    }
}
