using IVSoftware.Portable.Common.Attributes;
using IVSoftware.Portable.Common.Exceptions;
using IVSoftware.Portable.SQLiteMarkdown.Common;
using IVSoftware.Portable.Xml.Linq.XBoundObject;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml;
using System.Xml.Linq;
using Formatting = Newtonsoft.Json.Formatting;

namespace IVSoftware.Portable.SQLiteMarkdown.Util
{
    public enum ZeroCountOption
    {
        /// <summary>
        /// When a bin is decremented to zero, remove
        /// its key from underlying dictionary.
        /// </summary>
        Remove,

        /// <summary>
        /// When a bin is decremented to zero, do not
        /// remove its key from underlying dictionary.
        /// </summary>
        Preserve,

        /// <summary>
        /// Do  not allow binds to be decremented.
        /// </summary>
        IncrementOnly,
    }

    /// <summary>
    /// Defines policy for handling explicit value="False" when prior state is unknown.
    /// </summary>
    /// <remarks>
    /// The histogram tracks transitions based on asserted truth. When an explicit "False"
    /// is received, the previous state of the attribute is not observable—it may represent
    /// either a transition from True → False (requiring a decrement) or from Absent → False
    /// (requiring no action).
    ///
    /// Because this distinction cannot be made, handling "False" becomes a policy decision.
    /// Options range from conservative normalization (no decrement) to strict enforcement
    /// that treats such cases as indeterminate and invalid.
    /// </remarks>
    [Flags]
    public enum IncrementFalseOption
    {
        /// <summary>
        /// Suppresses any decrement when an explicit "False" is encountered.
        /// </summary>
        /// <remarks>
        /// Because the prior state is unknown, this option resolves the ambiguity
        /// conservatively by treating the transition as Absent → False (no-op).
        /// </remarks>        
        Remove = 0x1,

        /// <summary>
        /// Raises a policy violation when an explicit "False" is encountered.
        /// </summary>
        /// <remarks>
        /// Treats the indeterminate transition as invalid due to lack of prior state.
        /// </remarks>
        ThrowHard = 0x4,

        /// <summary>
        /// RECOMMENDED: Avoid incorrect decrements and surface indeterminate transitions.
        /// </summary>
        /// <remarks>
        /// Combines conservative handling with strict enforcement.
        /// </remarks>
        All = Remove | ThrowHard,
    }

    [DebuggerDisplay("{DebuggerDisplay}")]
    public sealed class EnumHistogrammer<T> : IEnumerable<T> where T : Enum
    {
        public EnumHistogrammer(ZeroCountOption zeroCountOption) => ZeroCountOption = zeroCountOption;
        public ZeroCountOption ZeroCountOption { get; }
        private readonly Dictionary<T, int> _histo = new();

        [Indexer]
        public int this[T key] => _histo.TryGetValue(key, out var count) ? count : 0;


        /// <summary>
        /// Retrieves the null-tolerant value of key and increments it without validation.
        /// </summary>
        public int Increment(T key)
        {
            var incremented = this[key] + 1;
            _histo[key] = incremented;
            log(key);
            return incremented;
        }

        /// <summary>
        /// Retrieves the null-tolerant value of key and increments it with validation.
        /// </summary>
        /// <remarks>
        /// The key is guaranteed to exist after this call, even if it did not previously.
        /// </remarks>
        public int Increment(T key, XAttribute context, IncrementFalseOption option = IncrementFalseOption.All)
        {
            if(Equals(option & IncrementFalseOption.All, 0))
            {
                this.ThrowHard<ArgumentException>("An error option must be selected.");
            }
            if(bool.TryParse(context.Value, out bool valid) && valid == false)
            {
                if(option.HasFlag(IncrementFalseOption.Remove))
                {
                    Debug.Fail(
                        $"ADVISORY First Time - " +
                        $"This 'false' value shouldn't be causing any reentrant XObject.Change events but make sure.");
                    context.Remove();
                }
                string msg = @"Explicit ""False"" is not allowed because prior state is unknown making the transition indeterminate.";
                if (option.HasFlag(IncrementFalseOption.ThrowHard))
                {
                    this.ThrowHard<MarkdownContextException>(msg);
                }
                else
                {
                    this.ThrowHard<MarkdownContextException>(msg);
                }
                return this[key];
            }
            else
            {
                return Increment(key);
            }
        }

        /// <summary>
        /// Retrieves the null-tolerant value of key and decrements it.
        /// </summary>
        /// <remarks>
        /// If the resulting value is zero, <see cref="ZeroCountOption"/> determines whether the key
        /// is removed or retained with a zero value. Negative results are normalized to zero and
        /// reported via advisory.
        /// </remarks>
        public int Decrement(T key)
        {
            if (ZeroCountOption == ZeroCountOption.IncrementOnly)
            {
                this.ThrowHard<InvalidOperationException>($"{nameof(Decrement)} is not allowed when {ZeroCountOption.ToFullKey()}");
                return this[key];
            }
            else
            {
                var decremented = this[key] - 1;

                if (decremented < 0)
                {
                    // Negative values are advisory unless escalated.
                    this.ThrowSoft<IndexOutOfRangeException>();
                    // Normalize to zero and apply ZeroCountOption.
                    decremented = 0;
                }
                if (decremented == 0)
                {
                    switch (ZeroCountOption)
                    {
                        case ZeroCountOption.Remove:
                            _histo.Remove(key);
                            break;

                        case ZeroCountOption.Preserve:
                            _histo[key] = 0;
                            break;
                    }
                }
                else
                {
                    _histo[key] = decremented;
                    log(key);
                }
                return decremented;
            }
        }
        public IEnumerator<T> GetEnumerator()
            => _histo.Keys.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        public string ToString(Formatting formatting) =>
            JsonConvert.SerializeObject(
                _histo.ToDictionary(
                kvp => kvp.Key.ToString(),
                kvp => kvp.Value), formatting);

        public string ToString(Enum formatting)
        {
            var keys = 
                formatting
                .GetCustomAttribute<HistogrammerFormatAttribute>()?
                .Keys.OfType<T>()
                .ToArray() ?? []; 
            var builder = new List<string>();

            if(keys.Length == 0)
            {
                foreach (var key in _histo.Keys)
                {
                    builder.Add($"{key}:{this[key]}");
                }
            }
            else
            {
                foreach (var key in keys)
                {
                    builder.Add($"{key}:{this[key]}");
                }
            }
            return $"[{string.Join(" ", builder)}]";
        }
        enum DebuggerReserved { Debugger }
        private string DebuggerDisplay => ToString(DebuggerReserved.Debugger);

        private void log(Enum key, [CallerMemberName]string? caller = null)
        {
            Debug.WriteLine($"260329.A - {caller} {key} {DebuggerDisplay}");
        }

        public void Clear() => _histo.Clear();
    }
}
