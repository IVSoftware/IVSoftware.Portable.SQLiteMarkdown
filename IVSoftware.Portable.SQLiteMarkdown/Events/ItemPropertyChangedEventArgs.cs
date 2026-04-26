namespace IVSoftware.Portable.SQLiteMarkdown.Events
{
    /// <summary>
    /// Compatibility shim retaining the legacy SQLiteMarkdown type identity.
    /// </summary>
    /// <remarks>
    /// Strategy 3: the implementation now lives in the shared collections
    /// dependency, while this wrapper preserves the historical namespace and
    /// constructor surface for manifest compatibility.
    /// </remarks>
    public class ItemPropertyChangedEventArgs :
        IVSoftware.Portable.Collections.Events.ItemPropertyChangedEventArgs
    {
        public ItemPropertyChangedEventArgs(string propertyName, object? item)
            : base(propertyName, item) { }
    }
}
