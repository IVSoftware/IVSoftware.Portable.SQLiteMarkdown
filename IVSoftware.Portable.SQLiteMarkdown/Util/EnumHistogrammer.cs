using IVSoftware.Portable.Common.Attributes;
using IVSoftware.Portable.Common.Exceptions;
using IVSoftware.Portable.Xml.Linq.XBoundObject;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace IVSoftware.Portable.SQLiteMarkdown.Util
{
    public enum ZeroCountOption
    {
        Remove,
        Preserve,
    }

    public sealed class EnumHistogrammer<T> : IEnumerable<T> where T : Enum
    {
        public EnumHistogrammer(ZeroCountOption zeroCountOption) => ZeroCountOption = zeroCountOption;
        public ZeroCountOption ZeroCountOption { get; }
        private readonly Dictionary<T, int> _histo = new();

        [Indexer]
        public int this[T key] => _histo.TryGetValue(key, out var count) ? count : 0;

        /// <summary>
        /// Retrieves the null-tolerant value of key and increments it.
        /// </summary>
        /// <remarks>
        /// The key is guaranteed to exist after this call, even if it did not previously.
        /// </remarks>
        public int Increment(T key)
        {
            var incremented = this[key] + 1;
            _histo[key] = incremented;
            return incremented;
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
            }
            return decremented;
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
    }
}
