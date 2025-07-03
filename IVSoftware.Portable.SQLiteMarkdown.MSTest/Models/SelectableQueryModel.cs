using Newtonsoft.Json;
using SQLite;
using System.Diagnostics;
using System.Reflection;
using System.Xml.Linq;

namespace IVSoftware.Portable.SQLiteMarkdown.MSTest.Models
{
    [DebuggerDisplay("{Description}")]
    [Table("items")]
    public class SelectableQFModel : SelfIndexed, ISelectableQueryFilterItem
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

        [SelfIndexed(
            IndexingMode.QueryLikeTerm |         // Responds to non-bracketed tokens as if they were bracketed, but not the other way around.
            IndexingMode.TagMatchTerm)]    // Responds to strict bracketed terms
        public string Tags
        {
            get => _tags;
            set
            {
                if (!Equals(_tags, value))
                {
                    _tags = value;
                    OnPropertyChanged();
                }
            }
        }
        private string _tags = string.Empty;

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

        public bool IsReadOnly
        {
            get => _isReadOnly;
            set
            {
                if (!Equals(_isReadOnly, value))
                {
                    _isReadOnly = value;
                    OnPropertyChanged();
                }
            }
        }
        bool _isReadOnly = true;



        public override string ToString() => $"{Description} {KeywordsDisplay} {TagsDisplay}".Trim();

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


    [DebuggerDisplay("{Description}")]
    [Table("items")]

    [Obsolete("Used in unit tests for early adopter (beta) migration support.")]
    public class SelectableQueryModelOR : SelfIndexedOR, ISelectableQueryFilterItem
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

        [SelfIndexed(
            IndexingMode.QueryLikeTerm |         // Responds to non-bracketed tokens as if they were bracketed, but not the other way around.
            IndexingMode.TagMatchTerm)]    // Responds to strict bracketed terms
        public string Tags
        {
            get => _tags;
            set
            {
                if (!Equals(_tags, value))
                {
                    _tags = value;
                    OnPropertyChanged();
                }
            }
        }
        private string _tags = string.Empty;

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

        public bool IsReadOnly
        {
            get => _isReadOnly;
            set
            {
                if (!Equals(_isReadOnly, value))
                {
                    _isReadOnly = value;
                    OnPropertyChanged();
                }
            }
        }
        bool _isReadOnly = true;

        public override string ToString() => $"{Description} {KeywordsDisplay} {TagsDisplay}".Trim();

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
