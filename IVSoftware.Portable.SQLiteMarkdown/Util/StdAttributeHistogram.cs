using IVSoftware.Portable.Common.Attributes;
using IVSoftware.Portable.Common.Exceptions;
using System;
using System.Collections;
using System.Collections.Generic;

namespace IVSoftware.Portable.SQLiteMarkdown.Util
{
    public enum ZeroCountOption
    {
        Remove,
        Preserve,
    }
    public sealed class StdAttributeHistogrammer : IEnumerable<StdMarkdownAttribute>
    {
        public StdAttributeHistogrammer(ZeroCountOption zeroCountOption) => ZeroCountOption = zeroCountOption;
        public ZeroCountOption ZeroCountOption { get; }
        private readonly Dictionary<StdMarkdownAttribute, int> _histo = new();

        [Indexer]
        public int this[StdMarkdownAttribute key]
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

        public static StdAttributeHistogrammer operator +(
            StdAttributeHistogrammer h,
            StdMarkdownAttribute key)
        {
            h[key] = h[key] + 1;
            return h;
        }

        public static StdAttributeHistogrammer operator -(
            StdAttributeHistogrammer h,
            StdMarkdownAttribute key)
        {
            h[key] = h[key] - 1;
            return h;
        }

        public IEnumerator<StdMarkdownAttribute> GetEnumerator()
            => _histo.Keys.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
