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
        internal static XBoundAttribute? XBoundAttribute(
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
        internal static T? XBoundAttributeValue<T>(
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

        internal static string PadToMaxLength(
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

        internal static string PadToMaxLength(
            this string @string,
            byte maxLength = byte.MaxValue,
            bool padToMaxLength = false)
            => @string.PadToMaxLength(out _, maxLength, padToMaxLength);

        // Fluent event attacher. Internal only; goes in Collections, with Preview semantics.
        internal static T WithCollectionChangeHandler<T>(this T @this, NotifyCollectionChangedEventHandler handler)
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

#if false
        /// <summary>
        /// Classifies the structural relationship between the canonical XML model and an incoming recordset.
        /// </summary>
        /// <remarks>
        /// Determines the <see cref="ReplaceItemsEventingTriage"/> describing the transition
        /// from the existing canonical state represented by <paramref name="model"/> to the
        /// incoming sequence <paramref name="newItems"/>.
        ///
        /// The canonical state is considered empty when the model contains no child elements.
        /// The incoming sequence is considered empty when it is null or contains no items.
        ///
        /// When evaluating non-<see cref="ICollection"/> sequences, only a minimal probe is
        /// performed to determine emptiness.
        /// </remarks>
        static ReplaceItemsEventingTriage GetReplacementTriage(
            IList oldItems,
            IList newItems)
        {
            bool oldEmpty = oldItems.Count == 0;
            bool newEmpty = newItems.Count == 0;

            return
                oldEmpty && newEmpty
                ? ReplaceItemsEventingTriage.AlwaysEmpty
                : oldEmpty
                    ? ReplaceItemsEventingTriage.EmptyBefore
                    : newEmpty
                        ? ReplaceItemsEventingTriage.EmptyAfter
                        : ReplaceItemsEventingTriage.NeverEmpty;
        }
        public static ReplaceItemsEventingContext GetReplacementTriageEvents(
            this XElement model,
            NotifyCollectionChangeReason reason,
            IEnumerable? canon,
            ReplaceItemsEventingOption options)
        => new ReplaceItemsEventingContext(model, reason, canon, options);

        /// <summary>
        /// Produces collection change events describing the replacement of the canonical item set.
        /// </summary>
        public class ReplaceItemsEventingContext
        {
            public ReplaceItemsEventingContext(
                XElement model,
                NotifyCollectionChangeReason reason,
                IEnumerable? canon,
                ReplaceItemsEventingOption options)
            {
                Options = options;
                IList
                    oldItems = new List<object>(),
                    newItems = canon?.Cast<object>().ToList() ?? [];
                foreach (var xel in model.Descendants())
                {
                    if (xel.Attribute(nameof(StdMarkdownAttribute.model)) is XBoundAttribute xba
                        && xba.Tag is { } item)
                    {
                        oldItems.Add(item);
                    }
                }
                var triage = GetReplacementTriage(oldItems, newItems);
                switch (triage)
                {
                    case ReplaceItemsEventingTriage.AlwaysEmpty:
                        Structural = null;
                        Reset = null;
                        break;

                    case ReplaceItemsEventingTriage.EmptyBefore:
                        Structural = new ModelSettledEventArgs(
                            NotifyCollectionChangedAction.Add,
                            changedItems: newItems,
                            reason);
                        Reset = new ModelSettledEventArgs(
                            NotifyCollectionChangedAction.Reset,
                            reason);
                        break;

                    case ReplaceItemsEventingTriage.EmptyAfter:
                        Structural = new ModelSettledEventArgs(
                            NotifyCollectionChangedAction.Remove,
                            changedItems: oldItems,
                            reason);
                        Reset = new ModelSettledEventArgs(
                            NotifyCollectionChangedAction.Reset,
                            reason);
                        break;

                    case ReplaceItemsEventingTriage.NeverEmpty:
                        Structural = new ModelSettledEventArgs(
                            NotifyCollectionChangedAction.Replace,
                            newItems: newItems,
                            oldItems: oldItems,
                            reason);
                        Reset = new ModelSettledEventArgs(
                            NotifyCollectionChangedAction.Reset,
                            reason);
                        break;

                    default:
                        this.ThrowHard<NotSupportedException>($"The {triage.ToFullKey()} case is not supported.");
                        break;
                }
            }
            public ReplaceItemsEventingOption Options { get; }
            public ModelSettledEventArgs? Structural
            {
                get => _structural;
                private set
                {
                    if (Options.HasFlag(ReplaceItemsEventingOption.StructuralReplaceEvent))
                    {
                        _structural = value;
                    }
                }
            }
            ModelSettledEventArgs? _structural = default;

            public ModelSettledEventArgs? Reset
            {
                get => _reset;
                private set
                {
                    if (Options.HasFlag(ReplaceItemsEventingOption.ResetOnAnyChange))
                    {
                        _reset = value;
                    }
                }
            }
            ModelSettledEventArgs? _reset = default;
        }

#endif
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
