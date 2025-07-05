﻿using System;
using System.Collections.Generic;
using System.Text;

namespace IVSoftware.Portable.SQLiteMarkdown
{
    /// <summary>
    /// Base class for attributes that identify properties as searchable terms
    /// in the markdown expression system. Derived types indicate how each property
    /// participates in indexing during Query or FIlter states, or tag-based matching.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public abstract class MarkdownTermAttribute : Attribute
    {
#if false && PROPOSED
        /// <summary>
        /// Specifies the casing transformation to be applied to the term.
        /// Default is <see cref="StringCasing.Lower"/>.
        /// </summary>
        public StringCasing StringCasing { get; set; } = StringCasing.Lower;
#endif
    }

    /// <summary>
    /// This property contributes to LIKE expressions in Query mode .
    /// </summary>
    public class QueryLikeTermAttribute : MarkdownTermAttribute
    {
    }

    /// <summary>
    /// This property contributes to LIKE expressions in Filter mode .
    /// </summary>
    public class FilterLikeTermAttribute : MarkdownTermAttribute
    {
    }

    /// <summary>
    /// Derived attribute for properties that should participate in tag-based match searches.
    /// </summary>
    public class TagMatchTermAttribute : MarkdownTermAttribute
    {
    }

    /// <summary>
    /// Derived attribute for properties that should participate in SQL-like search functionality.
    /// </summary>
    [Obsolete("Use [QueryLikeTerm] instead.")]
    public class SqlLikeTermAttribute : QueryLikeTermAttribute
    {
    }

    /// <summary>
    /// Derived attribute for properties that should participate in "Contains" search functionality.
    /// </summary>
    [Obsolete("Use [FilterLikeTerm] instead.")]
    public class FilterContainsTermAttribute : FilterLikeTermAttribute
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
            IndexingMode indexingMode = IndexingMode.QueryOrFilter)
        {
            IndexingMode = indexingMode;
        }
        public IndexingMode IndexingMode { get; private set; }

        /// <summary>
        /// Determines if the property will be persisted within the <c>Properties</c> dictionary 
        /// in a JSON-formatted store, depending on the specified <c>PersistenceMode</c>. 
        /// </summary>
        [Obsolete("This property may be removed in future releases.")]
        public PersistenceMode PersistenceMode { get; } = PersistenceMode.Json;
    }
    /// <summary>
    /// Specifies the indexing behavior for a property within OnePage's "query-then-filter" model,
    /// defining how the property will participate in different types of search operations during
    /// both the initial database query and the subsequent in-memory filtering phases.
    ///
    /// These options allow for efficient data retrieval and filtering, enhancing responsiveness
    /// within OnePage applications as users interact with search fields.
    /// </summary>
    /// 
    [Flags]
    public enum IndexingMode
    {
        /// <summary>
        /// Participates in the JSON blob for Query mode SQL where: LIKE '%value"%'.
        /// </summary>
        QueryLikeTerm = 0x01,

        /// <summary>
        /// Participates in the JSON blob for Query mode SQL where: LIKE '%value"%'.
        /// </summary>
        FilterLikeTerm = 0x02,

        /// <summary>
        /// Participates in the JSON blob for explicit tag queries SQL where values are 
        /// surrounded by square brackets and searched: LIKE '%[value]"%'.
        /// </summary>
        TagMatchTerm = 0x04,

        /// <summary>
        /// Combines both query phase and filter phase matching, enabling the property to
        /// support both partial retrieval from the database and broad in-memory search functionality.
        /// </summary>
        QueryOrFilter = 0x03,

        All = 0x7,
    }
}
