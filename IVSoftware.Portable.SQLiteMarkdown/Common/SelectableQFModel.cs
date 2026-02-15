using Newtonsoft.Json;
using SQLite;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace IVSoftware.Portable.SQLiteMarkdown.Common
{
    /// <summary>
    /// A fully query-filter–aware data model that participates in both SQLite-backed
    /// querying and in-memory filtering through automatic self-indexing.
    /// 
    /// This class is designed for use with ObservableQueryFilterSource<T> and provides:
    /// 
    /// - Attribute-driven indexing via <see cref="SelfIndexedAttribute"/>, enabling
    ///   Description, Keywords, and Tags to generate QueryTerm, FilterTerm, and
    ///   TagMatchTerm values automatically through the SQLiteMarkdown parsing engine.
    /// 
    /// - A minimal, SQLite-compatible schema using a GUID primary key and a compact
    ///   JSON-backed Keywords field, allowing flexible term expansion without schema churn.
    /// 
    /// - First-class selection semantics through <see cref="ISelectable"/>, enabling
    ///   UI list controls to track Exclusive, Multi, and Primary selections without
    ///   external wrappers.
    /// 
    /// Typical usage: 
    /// - As the row type for a Query/Filter list where users search by typed text,
    ///   bracketed tags, or combined expressions.
    /// - As a building block for tag-centric or keyword-centric item catalogs where
    ///   the UI requires round-trippable filtering between full-recordset queries and
    ///   incremental in-memory refinement.
    /// </summary>
    [DebuggerDisplay("{Description}")]
    [Table("items")]
    public class SelectableQFModel : SelfIndexed, ISelectable
    {
        [PrimaryKey]
        public override string Id { get; set; } = Guid.NewGuid().ToString();

        [SelfIndexed]
        public string Description
        {
            get => _description;
            set
            {
                if (!Equals(_description, value))
                {
                    _description = value;
                    OnPropertyChanged();
                }
            }
        }
        string _description = "New Item";

        [SelfIndexed]
        public string Keywords
        {
            get => _keywords;
            set
            {
                if (!Equals(_keywords, value))
                {
                    _keywords = value;
                    OnPropertyChanged();
                }
            }
        }
        private string _keywords = JsonConvert.SerializeObject(new List<string>());

        public string KeywordsDisplay => Keywords.Trim('[', ']');

        // ADVISORY KNOWN ORDER OF OPERATIONS BUG
        // bla|sho [not animal]

        // [SelfIndexed(IndexingMode.TagMatchTerm)]     // Tag term considered only for explicit brackets.  "color' does not match [color]
        [SelfIndexed(IndexingMode.All)]                 // Tag terms are included in all LIKE queries:      "color' matches [color]
        public string Tags
        {
            get => _tags;
            set
            {
                value = value.NormalizeTags();
                if (!Equals(_tags, value))
                {
                    _tags = value;
                    OnPropertyChanged();
                }
            }
        }
        private string _tags = string.Empty;

        [JsonIgnore, Obsolete]
        public string TagsDisplay => Tags;

        public bool IsChecked
        {
            get => _isChecked;
            set
            {
                if (!Equals(_isChecked, value))
                {
                    _isChecked = value;
                    OnPropertyChanged();
                }
            }
        }
        bool _isChecked = default;

        public ItemSelection Selection
        {
            get => _selection;
            set
            {
                if (!Equals(_selection, value))
                {
                    _selection = value;
                    OnPropertyChanged();
                }
            }
        }
        private ItemSelection _selection = ItemSelection.None;

        [Ignore]
        public bool IsEditing
        {
            get => _isEditing;
            set
            {
                if (!Equals(_isEditing, value))
                {
                    _isEditing = value;
                    OnPropertyChanged();
                }
            }
        }
        bool _isEditing = false;

        public override string ToString() => $"{Description} {KeywordsDisplay} {Tags}".Trim();

        public string Report()
        {
            var builder = new List<string>();
            var type = GetType();

            foreach (var pi in type.GetProperties())
            {
                // Skip the guid, which is new everytime.

                switch (pi.Name)
                {
                    case nameof(Id):
                    case nameof(PrimaryKey):
                        continue;
                    default:
                        break;
                }
                if (pi.Name == nameof(Id)) continue;

                var value = pi.GetValue(this);
                builder.Add($@"{pi.Name,-15}=""{value}""");
            }
            return string.Join(Environment.NewLine, builder);
        }
    }
}
