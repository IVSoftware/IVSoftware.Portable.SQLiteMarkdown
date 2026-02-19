using IVSoftware.Portable.SQLiteMarkdown.Common;
using SQLite;

namespace IVSoftware.Portable.SQLiteMarkdown.MSTest.Models
{

    /// <summary>
    /// Subclass, where table is consistent with BC.(This is the control case of the experiment.)) 
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

    /// <summary>
    /// The "Gotcha" subclass. SQLite assumes table identity is 'SelectableQFModelSubclassG' not 'items'.
    /// </summary>
    class SelectableQFModelSubclassG : SelectableQFModel
    {
    }
}
