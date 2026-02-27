using IVSoftware.Portable.Common.Attributes;
using IVSoftware.Portable.Common.Exceptions;
using IVSoftware.Portable.Disposable;
using IVSoftware.Portable.Xml.Linq.XBoundObject;
using System;

namespace IVSoftware.Portable.SQLiteMarkdown.Util
{
    public enum AffinityIncrMode
    {
        Prefix,
        Current,
        Postfix,
    }
    public static class AffinityTestableEpoch
    {
        [Careful(@"
TestableEpoch is single-threaded.
You MUST use [DoNotParallelize] for tests that employ it")]
        public static IDisposable TestableEpoch(this object? @this)
            => DHostTokenDispenser.GetToken(sender: @this);

        static DisposableHost DHostTokenDispenser
        {
            get
            {
                if (_dhostTokenDispenser is null)
                {
                    _dhostTokenDispenser = new DisposableHost();
                    _dhostTokenDispenser.BeginUsing += (sender, e) =>
                    {
                        _guidCurrent = GuidReset;
                        _utcCurrent = UtcReset;
                    };
                }
                return _dhostTokenDispenser;
            }
        }
        static DisposableHost? _dhostTokenDispenser = null;

        #region G U I D
        public static Guid GuidReset { get; } =
            new Guid("312D1C21-0000-0000-0000-000000000000");

        public static Guid WithTestability(
            this Guid @this,
            AffinityIncrMode? mode = AffinityIncrMode.Postfix)
        {
            if (DHostTokenDispenser.IsZero())
            {
                return @this;
            }
            else
            {
                mode ??= AffinityIncrMode.Postfix;

                switch ((AffinityIncrMode)mode)
                {
                    case AffinityIncrMode.Current:
                        return _guidCurrent;
                    case AffinityIncrMode.Prefix:
                    case AffinityIncrMode.Postfix:
                        // Deterministic increment
                        var bytes = _guidCurrent.ToByteArray();
                        for (int i = bytes.Length - 1; i >= 0; i--)
                        {
                            if (++bytes[i] != 0)
                                break; // carry complete
                        }
                        if(mode == AffinityIncrMode.Postfix)
                        {
                            var pre = _guidCurrent;
                            _guidCurrent = new Guid(bytes);
                            return pre;
                        }
                        else
                        {
                            _guidCurrent = new Guid(bytes);
                            return _guidCurrent;
                        }
                        
                    default:
                        @this.ThrowHard<NotSupportedException>($"The {mode.ToFullKey()} case is not supported.");
                        return new Guid();
                }
            }
        }

        #endregion G U I D

        #region U T C
        public static DateTimeOffset UtcReset { get; } =
            new DateTimeOffset(2000, 1, 1, 9, 0, 0, TimeSpan.FromHours(7));

        public static TimeSpan DefaultIncr
        {
            get
            {
                return _defaultIncr < _minIncr ? _minIncr : _defaultIncr;
            }
            set
            {
                _defaultIncr = value;
            }
        }

        // The DEFAULT for the DEFAULT INCREMENTER
        static TimeSpan _defaultIncr = TimeSpan.FromMinutes(1);

        static DateTimeOffset _utcCurrent = UtcReset;

        static Guid _guidCurrent = GuidReset;

        static readonly TimeSpan _minIncr = TimeSpan.FromSeconds(1);

        public static DateTimeOffset WithTestability(
            this DateTimeOffset @this, 
            TimeSpan? incr = null, 
            AffinityIncrMode? mode = AffinityIncrMode.Postfix)
        {
            if (DHostTokenDispenser.IsZero())
            {
                return @this;
            }
            else
            {
                incr ??= DefaultIncr;
                mode ??= AffinityIncrMode.Postfix;

                switch ((AffinityIncrMode)mode)
                {
                    case AffinityIncrMode.Prefix:
                        _utcCurrent += (TimeSpan)incr;
                        return _utcCurrent;
                    case AffinityIncrMode.Current:
                        return _utcCurrent;
                    case AffinityIncrMode.Postfix:
                        var pre = _utcCurrent;
                        _utcCurrent += (TimeSpan)incr;
                        return pre;
                    default:
                        @this.ThrowHard<NotSupportedException>($"The {mode.ToFullKey()} case is not supported.");
                        return DateTimeOffset.MinValue;
                }
            }
        }
        #endregion U T C
    }
}
