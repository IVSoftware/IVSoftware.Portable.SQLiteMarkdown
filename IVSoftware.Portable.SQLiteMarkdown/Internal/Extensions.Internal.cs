using IVSoftware.Portable.Common.Attributes;
using IVSoftware.Portable.Common.Exceptions;
using IVSoftware.Portable.SQLiteMarkdown.Collections;
using IVSoftware.Portable.Xml.Linq;
using IVSoftware.Portable.Xml.Linq.XBoundObject;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
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
        /// Returns the <see cref="XBoundAttribute"/> for the specified <see cref="StdMarkdownAttribute"/>.
        /// </summary>
        /// <remarks>
        /// Resolves the attribute using the enum name. If the attribute is missing or not bound
        /// as an <see cref="XBoundAttribute"/>, behavior is governed by <paramref name="throw"/>.
        /// Advisory or unspecified returns <c>null</c>.
        /// </remarks>
        internal static XBoundAttribute? XBoundAttribute(
            this XElement @this,
            StdMarkdownAttribute stdEnum,   // This type *only* by design. Do not generalize to Enum.
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
        /// Returns the typed <c>Tag</c> value of the <see cref="XBoundAttribute"/> identified by the specified <see cref="StdMarkdownAttribute"/>.
        /// </summary>
        /// <remarks>
        /// Resolves the attribute via <see cref="XBoundAttribute(XElement, StdMarkdownAttribute, ThrowOrAdvise?)"/>.
        /// If the attribute is missing, not bound, or its <c>Tag</c> is not assignable to
        /// <typeparamref name="T"/>, the result is <c>default</c>.
        /// </remarks>
        internal static T? XBoundAttributeValue<T>(
            this XElement @this,
            StdMarkdownAttribute stdEnum,   // This type *only* by design. Do not generalize to Enum.
            ThrowOrAdvise? @throw = null)
        {
            if (@this.XBoundAttribute(stdEnum, @throw) is { } xba && xba.Tag is T valueT)
            {
                return valueT;
            }
            return default;
        }

        /// <summary>
        /// Returns the <see cref="XAttribute"/> identified by the specified <see cref="Enum"/> key.
        /// </summary>
        /// <remarks>
        /// Mental Model: "Enum member → canonical attribute name."
        /// - Resolves the attribute using the enum name as the attribute key.
        /// - When the attribute is not present, behavior is governed by <paramref name="throw"/>. 
        /// - Advisory or unspecified returns <c>null</c>.
        /// </remarks>
        internal static XAttribute? Attribute(
            this XElement @this,
            Enum stdEnum, ThrowOrAdvise? @throw = null)
        {
            string msg;
            if (@this.Attribute(stdEnum.ToString()) is not XAttribute attr)
            {
                msg = $"The {stdEnum.ToFullKey()} attribute was not found.";
            }
            else
            {
                return attr;
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
        /// Returns the attribute value converted to <typeparamref name="T"/>.
        /// </summary>
        /// <remarks>
        /// When the attribute is missing or unusable, recovery proceeds as follows:
        /// 1. SAFE: If T is nullable, returns default(T).
        /// 2. SAFE: If @default is T, @default is returned as T.
        /// 3. ADVISED SAFE: If [DefaultValue] is supplied, attempt conversion.
        /// 4. ADVISED UNSAFE: Throw policy is invoked; if the throw is handled,
        ///    default(T) is returned after the notification.
        /// </remarks>
        internal static T? GetAttributeValue<T>(
            this XElement @this,
            Enum stdEnum,
            object? @default = null,
            ThrowOrAdvise? @throw = null)
        {
            var targetType = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);

            // [Careful] Attribute method already has error handling!
            if (@this.Attribute(stdEnum, @throw) is { } attr)
            {
                try
                {
                    return localConvertValue(attr.Value, targetType);
                }
                catch (Exception ex)
                {
                    @this.RethrowFramework(ex);
                }
            }

            // If explicit default not supplied, consult [DefaultValue].
            // NOTE: This line is a minor duplication that keeps both reflections out of the hot path unless needed.
            @default ??= stdEnum.GetCustomAttribute<DefaultValueAttribute>()?.Value;

            // Direct default match (explicit @default or [DefaultValue])
            if (@default is T defaultT)
                return defaultT;

            // Attempt conversion of default
            if (@default is not null)
            {
                try
                {
                    return localConvertValue(@default, targetType);
                }
                catch
                {
                    // Ignore conversion failure and fall through to policy enforcement.
                }
            }

            // No usable value resolved; enforce non-nullable contract
            bool isNullable = targetType != typeof(T) || !typeof(T).IsValueType;

            if (!isNullable)
            {
                localThrowHelper($"Non-nullable type({typeof(T).Name}) requires {nameof(@default)}");
            }

            return default;

            #region L o c a l F x
            T localConvertValue(object? value, Type targetType)
            {
                var s = value?.ToString() ?? string.Empty;

                if (targetType == typeof(string))
                    return (T)(object)s;

                // Fast reject obvious non-numeric input when numeric expected
                if (value is string sVal && localIsNumericType(targetType))
                {
                    if (!double.TryParse(sVal, out _))
                    {
                        // If explicit default not supplied, consult [DefaultValue].
                        // NOTE: This line is a minor duplication that keeps both reflections out of the hot path unless needed.
                        @default ??= stdEnum.GetCustomAttribute<DefaultValueAttribute>()?.Value;
                        if (@default is T defaultT)
                        {
                            return defaultT;
                        }
                        else
                        {
                            // If no explicit @throw level then throw hard.
                            @throw ??= ThrowOrAdvise.ThrowHard;
                            localThrowHelper($"The string provided '{sVal}' is not numeric.");
                            return default!; // We warned you.
                        }
                    }
                }

                if (targetType.IsEnum)
                    return (T)Enum.Parse(targetType, s, ignoreCase: true);

                return (T)Convert.ChangeType(value, targetType);
            }

            static bool localIsNumericType(Type t)
            {
                return t == typeof(byte) ||
                       t == typeof(sbyte) ||
                       t == typeof(short) ||
                       t == typeof(ushort) ||
                       t == typeof(int) ||
                       t == typeof(uint) ||
                       t == typeof(long) ||
                       t == typeof(ulong) ||
                       t == typeof(float) ||
                       t == typeof(double) ||
                       t == typeof(decimal);
            }

            void localThrowHelper(string msg)
            {
                switch (@throw ?? ThrowOrAdvise.ThrowHard)
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
                }
            }
            #endregion
        }

        /// <summary>
        /// Assigns the value of the attribute identified by the specified <see cref="StdMarkdownAttribute"/>.
        /// </summary>
        /// <remarks>
        /// Mental Model: "Write canonical attribute text with controlled length semantics."
        /// The value is converted to string and written using the enum name as the attribute key.       
        /// - When the resulting string exceeds <paramref name="maxLength"/>, the value is truncated
        ///   and the configured throw policy is invoked.
        /// - When <paramref name="padToMaxLength"/> is <c>true</c>, the value is right-padded to the specified length.
        /// </remarks>
        internal static void SetAttributeValue(
            this XElement @this,
            StdMarkdownAttribute stdEnum,
            object? value,
            byte maxLength = byte.MaxValue,
            bool padToMaxLength = false,
            ThrowOrAdvise? @throw = null)
        {
            if (value is null)
            {
                @this.SetAttributeValue(stdEnum.ToString(), null);
                return;
            }

            string @string = value.ToString() ?? string.Empty;
            if (@string.Length > maxLength)
            {
                @string = @string.Substring(0, maxLength);

                var msg = $"Value for {stdEnum.ToFullKey()} exceeded {maxLength} characters and has been truncated.";
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
                    default:
                        break;
                }
            }
            else if (padToMaxLength)
            {
                @string = @string.PadRight(maxLength);
            }
            @this.SetAttributeValue(stdEnum.ToString(), @string);
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

        /// <summary>
        /// Materializes descendant object whose effective <c>ismatch</c> state is true.
        /// </summary>
        /// <remarks>
        /// <paramref name="allMatch"/> reports whether every descendant satisfied the match rule.
        /// Elements without an explicit <c>ismatch</c> attribute assume <paramref name="default"/>.
        /// </remarks>
        public static object[] Matches(this XElement @this, bool @default = true)
            => @this.Matches(out _, @default);

        [Probationary, Canonical]
        public static object[] Matches(this XElement @this, out bool allMatch, bool @default = true)
        {
            List<object> matched = new();
            int count = 0;

            foreach (var current in @this.Descendants())
            {
                count++;

                bool isMatch =
                    current.Attribute(nameof(StdMarkdownAttribute.ismatch)) is { } attr
                    && bool.TryParse(attr.Value, out var explicitMatch)
                        ? explicitMatch
                        : @default;

                if (isMatch && current.Attribute(StdMarkdownAttribute.model) is XBoundAttribute xba && xba is { } item)
                {
                    matched.Add(item);
                }
            }

            allMatch = matched.Count == count;
            return matched.ToArray();
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
        public static NotifyCollectionChangeReason ToNotifyCollectionChangeReason(this Enum @this)
        {
            var preview = (NotifyCollectionChangeReason) Enum.ToObject(typeof(NotifyCollectionChangeReason), Convert.ToInt32(@this) & ~0x07);
            Debug.Assert(Equals(@this, preview), "Expecting values are no longer OR'ed");
            return preview;
        }
        #endregion A C T I O N    M A S K S

        #endregion L E G I T
    }
}
