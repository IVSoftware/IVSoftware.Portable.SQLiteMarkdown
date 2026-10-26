using IVSoftware.Portable.SQLiteMarkdown.Common;
using IVSoftware.Portable.Collections;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Linq;
using Ephemeral = SQLite.IgnoreAttribute;

// Historical test fixtures only. The reusable public contracts live in
// IVSoftware.Portable.Affinity; these declarations are not shipped by SQLiteMarkdown.
namespace IVSoftware.Portable.SQLiteMarkdown
{
    /// <summary>
    /// Json property bag for user-defined value;
    /// </summary>
    internal interface ICustomProperties
    {
        IDictionary<string, string> CustomProperties { get; }
    }


    internal enum AffinityRole
    {
        Prev,
    }

    internal interface IAffinityModel
    {
        /// <summary>
        /// Provides access to the hierarchical structure and hosts lateral expansion.
        /// </summary>
        /// <remarks>
        /// XBoundObject is used extensively to attach service objects to XAttributes of this node.
        /// </remarks>
        XElement Model { get; set; } // Xml affinity model.
    }

    /// <summary>
    /// Represents a prioritized node whose relational context ("affinity") is 
    /// implicitly restored when materialized from a query.
    /// </summary>
    /// <remarks>
    /// EXAMPLE:
    /// If the raw recordset of a query returns ONE child item at depth = 2, then the
    /// net query returns THREE. The UI now has greater opportunity to display context.
    /// </remarks>
    internal interface IPrioritizedAffinity : IPathPlaceable, IAffinityModel
    {
        [Ephemeral]
        bool IsRoot { get; }

        /// <summary>
        /// Sortable key that is typically based on DateTimeOffset.Ticks.
        /// </summary>
        /// <remarks>
        /// While not exactly a time and date quantity, consider
        /// that DateTime.Now.Ticks will place an item last, but
        /// also that one might insert Item B as "halfway between"
        /// Item A and Item C.
        /// </remarks>
        long Priority { get; set; }

        /// <summary>
        /// Contextual priority used for non-persistent ordering scenarios.
        /// </summary>
        /// <remarks>
        /// Supports UI-driven ordering such as column-header sorting.
        /// Does not modify or replace the canonical Priority value.
        /// </remarks>
        [Ephemeral]
        long? PriorityOverride { get; set; }

        bool MoveUp();
        bool MoveDown();
        bool IndentLess();
        bool IndentMore();
    }

}
