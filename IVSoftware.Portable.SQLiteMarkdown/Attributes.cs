using System;
using System.Collections.Generic;
using System.Text;

namespace IVSoftware.Portable.SQLiteMarkdown
{

    /// <summary>
    /// Base attribute for marking properties as indexing terms within the  framework.
    /// Attributes derived from <see cref="MarkdownTermAttribute"/> enable  to
    /// recognize and manage these properties with specific indexing behaviors, such as
    /// SQL-like, Contains, and Tag Match searches.
    ///
    /// Properties decorated with any derived attribute of <see cref="MarkdownTermAttribute"/>
    /// participate in indexing workflows, allowing the  framework to differentiate
    /// between various search types.
    ///
    /// This attribute, along with its derived types, can only be applied to properties and is 
    /// inherited by derived classes, enabling seamless discovery in subclasses as well.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public abstract class MarkdownTermAttribute : Attribute
    {
        /// <summary>
        /// Specifies the casing transformation to be applied to the term.
        /// Default is <see cref="StringCasing.Lower"/>.
        /// </summary>
        public StringCasing StringCasing { get; set; } = StringCasing.Lower;
    }

    /// <summary>
    /// Derived attribute for properties that should participate in SQL-like search functionality.
    /// </summary>
    public class SqlLikeTermAttribute : MarkdownTermAttribute
    {
    }

    /// <summary>
    /// Derived attribute for properties that should participate in "Contains" search functionality.
    /// </summary>
    [Obsolete("Specify QueryTemplate and FilterTemplate properties instead")]
    public class FilterContainsTermAttribute : MarkdownTermAttribute
    {
    }

    /// <summary>
    /// Derived attribute for properties that should participate in tag-based match searches.
    /// </summary>
    public class TagMatchTermAttribute : MarkdownTermAttribute
    {
    }

    /// <summary>
    /// Custom attribute that works synergistically with <see cref="SQLite.IgnoreAttribute"/> to exclude properties from 
    /// individual schema columns while still allowing these properties to be stored in a serialized form within the 
    /// database record’s <c>Properties</c> dictionary. Although <see cref="SQLite.IgnoreAttribute"/> can be used alongside 
    /// this attribute, the <c>PersistenceMode</c> setting will determine whether the property is added 
    /// to the <c>Properties</c> dictionary as part of the JSON-formatted backing store.
    /// </summary>
    /// <remarks>
    /// - This attribute instructs implementing classes to bypass adding the property as a table column, 
    ///   helping to maintain a clean and optimized schema by avoiding unnecessary columns.
    /// - Instead, the marked property’s value can be stored in the <c>Properties</c> dictionary, which is serialized 
    ///   into the record, ensuring that non-queryable properties are still stored in a compact, JSON-formatted structure.
    /// - Use this attribute to efficiently store auxiliary data without adding it to the table structure, 
    ///   particularly when data is self-indexed and does not require individual columns for querying.
    /// - Note that <c>[Ignore]</c> can coexist with this attribute to provide additional control over schema inclusion,
    ///   making it a versatile solution for selectively persisted properties.
    /// </remarks>
    public class SelfIndexedAttribute : Attribute
    {
        public SelfIndexedAttribute(
            IndexingMode indexingMode = IndexingMode.LikeOrContains,
            PersistenceMode persistenceMode = PersistenceMode.Json)
        {
            IndexingMode = indexingMode;
            PersistenceMode = persistenceMode;
        }

        public IndexingMode IndexingMode { get; private set; }

        /// <summary>
        /// Determines if the property will be persisted within the <c>Properties</c> dictionary 
        /// in a JSON-formatted store, depending on the specified <c>PersistenceMode</c>. 
        /// </summary>
        public PersistenceMode PersistenceMode { get; private set; }
    }
}
