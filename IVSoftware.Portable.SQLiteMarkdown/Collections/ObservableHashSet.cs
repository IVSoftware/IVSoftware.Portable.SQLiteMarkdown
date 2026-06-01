using IVSoftware.Portable.Common.Attributes;
using System.Collections;
using System.Collections.Specialized;
using CanonicalObservableHashSet = IVSoftware.Portable.Collections.Dictionaries.ObservableHashSet<object>;

namespace IVSoftware.Portable.SQLiteMarkdown.Collections
{
    /// <summary>
    /// Compatibility shim retaining the legacy SQLiteMarkdown type identity.
    /// </summary>
    /// <remarks>
    /// - Implementation has been pushed down to Xml.Linq.Collections.
    /// - This wrapper preserves the historical namespace for compatibility checks and existing callers.
    /// </remarks>
    [PublishedContract("1.x")]
    public class ObservableHashSet : CanonicalObservableHashSet { }

    /// <summary>
    /// Compatibility shim retaining the legacy SQLiteMarkdown type identity.
    /// </summary>
    /// <remarks>
    /// - Implementation has been pushed down to Xml.Linq.Collections.
    /// - This wrapper preserves the historical namespace for compatibility checks and existing callers.
    /// </remarks>
    [PublishedContract("1.x")]
    public class ObservableHashSet<T> : IVSoftware.Portable.Collections.Dictionaries.ObservableHashSet<T> { }

    public class NotifyCollectionResetEventArgs : NotifyCollectionChangedEventArgs
    {
        public NotifyCollectionResetEventArgs(IList oldItems) : base(NotifyCollectionChangedAction.Reset)
        {
            OldItems = oldItems;
        }

        public new IList OldItems { get; }
    }
}
