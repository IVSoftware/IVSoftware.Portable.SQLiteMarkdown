namespace IVSoftware.Portable.SQLiteMarkdown.Events
{
    /// <summary>
    /// Compatibility shim retaining the legacy SQLiteMarkdown type identity.
    /// </summary>
    /// <remarks>
    /// - Implementation has been pushed down to shared Collections dependency.
    /// - This wrapper preserves the historical namespace for compatibility checks and existing callers.
    /// </remarks>
    [PublishedContract("1.x")]
    public class ItemPropertyChangedEventArgs :
        IVSoftware.Portable.Collections.Events.ItemPropertyChangedEventArgs
    {
        public ItemPropertyChangedEventArgs(string propertyName, object? item)
            : base(propertyName, item) { }
    }
}
