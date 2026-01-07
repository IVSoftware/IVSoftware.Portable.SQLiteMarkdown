using IVSoftware.Portable.SQLiteMarkdown.Common;
using SQLite;

namespace IVSoftware.Portable.SQLiteMarkdown.MSTest.Models
{
    /// <summary>
    /// The "Gotcha" subclass. SQLite assumes table identity is 'SelectableQFModelSubclassG' not 'items'.
    /// </summary>
    class SelectableQFModelSubclassG : SelectableQFModel
    {
    }

    /// <summary>
    /// Subclass, where table is consistent with BC.
    /// </summary>
    [Table("items")]
    class SelectableQFModelSubclass : SelectableQFModel
    {
    }

    /// <summary>
    /// Subclass, where table is inconsistent with BC.
    /// </summary>
    [Table("itemsA")]
    class SelectableQFModelSubclassA : SelectableQFModel
    {
    }
}
