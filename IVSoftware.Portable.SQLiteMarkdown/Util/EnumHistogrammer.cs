using IVSoftware.Portable.Common.Attributes;
using IVSoftware.Portable.Common.Exceptions;
using IVSoftware.Portable.Xml.Linq.XBoundObject;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace IVSoftware.Portable.SQLiteMarkdown.Util
{
    enum ZeroCountOption
    {
        Remove,
        Preserve,
    }

    sealed class EnumHistogrammer<T> : IEnumerable<T> where T : Enum
    {
        public EnumHistogrammer(ZeroCountOption zeroCountOption) => ZeroCountOption = zeroCountOption;
        public ZeroCountOption ZeroCountOption { get; }
        private readonly Dictionary<T, int> _histo = new();

        [Indexer]
        public int this[T key]
        {
            get => _histo.TryGetValue(key, out var count) ? count : 0;
            private set
            {
                if (value < 0)
                {
                    // Negative values are advisory unless escalated.
                    this.ThrowSoft<IndexOutOfRangeException>();
                    // Normalize to zero and apply ZeroCountOption.
                    value = 0;
                }
                if (value == 0)
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
                    _histo[key] = value;
                }
            }
        }

        public static EnumHistogrammer<T> operator +(
            EnumHistogrammer<T> h,
            T key)
        {
            h[key] = h[key] + 1;
            return h;
        }

        public static EnumHistogrammer<T> operator -(
            EnumHistogrammer<T> h,
            T key)
        {
            h[key] = h[key] - 1;
            return h;
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
                .GetCustomAttribute<HistogrammerFormatAttribute>()
                ?.Keys.OfType<T>() ?? [];

            var builder = new List<string>();

            foreach (var kvp in _histo)
            {
                // NOW we have a direct read... TODO!
            }

            return string.Join(" ", builder);
        }
    }
}
