using System;
using System.Collections.Generic;
using System.Text;

namespace IVSoftware.Portable.SQLiteMarkdown.Common
{
    partial class SelectableQFPrimeModel
    {
        public DateTimeOffset? UtcStart { get; set; }
        public TimeSpan? Duration { get; set; }
        public TimeSpan? Remaining { get; set; }

        public DateTimeOffset? UtcEnd => throw new NotImplementedException();

        public bool? IsDone => throw new NotImplementedException();

        public UtcEpochMode? EpochMode { get; set; }

        public UtcEpochTimeDomain? EpochTimeDomain => throw new NotImplementedException();

        public bool? IsRunning => throw new NotImplementedException();

        public string? Parent => throw new NotImplementedException();

        public IList<IUtcEpochSlot> Slots => throw new NotImplementedException();

        IUtcEpochClock
    }
}
