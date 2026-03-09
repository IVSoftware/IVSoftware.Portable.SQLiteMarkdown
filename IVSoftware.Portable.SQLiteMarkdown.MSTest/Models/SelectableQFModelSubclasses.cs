using IVSoftware.Portable.SQLiteMarkdown.Common;
using SQLite;

namespace IVSoftware.Portable.SQLiteMarkdown.MSTest.Models
{
    /// <summary>
    /// Different classes, but the explicit [Table] attributes all agree.
    /// </summary>
    /// <remarks>
    /// SQLite mapping: resolves to "items".
    /// Parser behavior: uncontroversial. All mappings converge on the same table.
    /// </remarks>
    [Table("items")]
    class SelectableQFModelSubclass : SelectableQFModel
    {
    }

    /// <summary>
    /// Different classes, and the explicit [Table] attributes conflict.
    /// </summary>
    /// <remarks>
    /// ACCORDING TO SQLITE NATIVE: 
    /// The table name resolves to SelectableQFModelSubclassa.
    /// 
    /// BUT, ACCORDING TO SQLITE MARKDOWN PARSER, IT'S MORE SUBTLE:
    /// 1. If ProxyType and ContractType are different types:
    ///    a. The ContractType mapping will be used
    ///    b. The ProxyType must be able to resolve to it *explicity* in its inheritance;
    ///    c. The explicit [Table] attribute can be disregarded, provided b is true.
    /// 2. If ProxyType and ContractType are the same type.
    ///    a. The most-derived explicit table mapping is used.
    ///    b. If *no* explicit [Table] attributes are found, the mapping 
    ///       for the most derived type is used without exception.
    /// </remarks>
    [Table("itemsA")]
    class SelectableQFModelSubclassA : SelectableQFModel
    {
    }

    /// <summary>
    /// The "Gotcha" subclass.
    /// </summary>
    /// <remarks>
    /// ACCORDING TO SQLITE NATIVE: 
    /// The table name resolves to SelectableQFModelSubclassG.
    /// 
    /// BUT, ACCORDING TO SQLITE MARKDOWN PARSER: 
    /// Since SelectableQFModel offers an explicit table, always use that.
    /// </remarks>
    class SelectableQFModelSubclassG : SelectableQFModel
    {
    }

    /// <summary>
    /// So, you think you tested this? Then why BUGIRL?
    /// </summary>
    [Table(nameof(ItemCardModel))]
    class ItemCardModel : SelectableQFModelSubclass
    {
    }

    /// <summary>
    /// 260309 NEW!
    /// </summary>
    [Table("items")]
    class TrueProxy : SelfIndexed
    {
        public bool SchemaExtended { get; set; } = true;
    }

    /// <summary>
    /// 260309 NEW!
    /// </summary>
    [Table("items"), ExtendMapping]
    class TrueProxyWithExtendSchema : SelfIndexed
    {
        public bool SchemaExtended { get; set; } = true;
    }

    /// <summary>
    /// 260309 NEW!
    /// </summary>
    [Table("containers")]
    class NonCoherentProxy : SelfIndexed
    {
        public bool SchemaExtended { get; set; } = true;
    }
}
