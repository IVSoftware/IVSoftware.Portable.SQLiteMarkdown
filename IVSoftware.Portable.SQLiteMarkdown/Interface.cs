using IVSoftware.Portable.SQLiteMarkdown.Collections;
using SQLite;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Text;

namespace IVSoftware.Portable.SQLiteMarkdown
{
    /// <summary>
    /// Defines a self-indexing class that updates special, reserved property names within a database 
    /// or other keyed collection. This interface provides mechanisms for in-memory filtering, 
    /// partial and exact term matching, and efficient serialization.
    /// </summary>
    /// <remarks>
    /// - The implementing class should use the generic IVSoftware.Portable.SQLiteMarkdown.ParseSqlMarkdown
    ///   expression to parse and index terms.
    /// - A typical usage pattern includes a brief settling timer in an OnPropertyChanged override to update 
    ///   the indexed terms and the Values backing store.
    /// - Most queries operate on the exposed indexed values, reducing the need for each property to consume 
    ///   individual columns (e.g., in an SQLite schema).
    /// - The [SelfIndexedIgnore] attribute, inheriting from [SQLite.Ignore], instructs the implementing class 
    ///   to update the Values store without serializing changes in marked properties directly.
    /// </remarks>
    public interface ISelfIndexedMarkdown : INotifyPropertyChanged
    {
        [PrimaryKey]
        string PrimaryKey { get; }

        /// <summary>
        /// Provides a broad in-memory filter during the **filter phase** of searching. After the initial query, 
        /// this property allows full-text searches within each property for any occurrence of the specified term.
        /// </summary>
        string LikeTerm { get; set; }

        /// <summary>
        /// Enables partial matching within the **database query phase** using SQLite LIKE expressions. 
        /// This allows retrieval of records where the term appears anywhere within a property's value 
        /// (e.g., "book" matches "notebook" and "booking").
        /// </summary>
        string ContainsTerm { get; set; }

        /// <summary>
        /// Supports exact matching of terms enclosed in square brackets (e.g., "[tag]") for specific tags or values 
        /// during querying. The string is trimmed and any internal whitespace is normalized to a single space character.
        /// </summary>
        string TagMatchTerm { get; set; }

        /// <summary>
        /// Persisted JSON representation of the Values dictionary, stored for efficient access.
        /// </summary>
        string Properties
        {
            // When IsSerializationRequired is true due to changes in the dictionary, 
            // a private accessor re-serializes the dictionary to JSON to avoid circular references.
            get;
            set;
        }
    }

    public interface IObservableQueryFilterSource
        : IList
        , INotifyCollectionChanged
        , INotifyPropertyChanged
    {
        void ApplyFilter();
        bool IsFiltering { get; }
        string InputText { get; set; }

        event EventHandler InputTextSettled;
        SearchEntryState SearchEntryState { get; }
        FilteringState FilteringState { get; }
        string Placeholder { get; }
        bool Busy { get; }
        QueryFilterConfig QueryFilterConfig { get; set; }
        string Title { get; set; }
        MarkdownContext MarkdownContext { get; }
        string SQL { get; }
        SQLiteConnection MemoryDatabase { get; set; }
        FilteringState Clear(bool all = false);
        void Commit();
    }
}
