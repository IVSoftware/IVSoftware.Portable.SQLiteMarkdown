using SQLite;

namespace IVSoftware.Portable.SQLiteMarkdown.MSTest.Models
{
    /// <summary>
    /// Thin wrapper for <see cref="SelfIndexed"/> that customizes the [Table] attribute.
    /// </summary>
    [Table("items")]
    class SelfIndexedItem : SelfIndexed
    {
    }
}
