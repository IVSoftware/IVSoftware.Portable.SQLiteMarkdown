using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IVSoftware.Portable.SQLiteMarkdown.MSTest.Models
{
    /// <summary>
    /// Simple class that implements ISelfIndexedMarkdown by
    /// inheriting <see cref="SelfIndexed"/>.
    /// </summary>
    class SelfIndexedProfile : SelfIndexed, ISelfIndexedMarkdown
    {

        [PrimaryKey]
        public override string Id { get; set; } = "38CFE38E-0D90-4C9F-A4E5-845089CB2BB0";

        [SelfIndexed(IndexingMode.QueryOrFilter)]
        public string FirstName
        {
            get => _firstName;
            set
            {
                if (!Equals(_firstName, value))
                {
                    _firstName = value;
                    OnPropertyChanged();
                }
            }
        }
        string _firstName = string.Empty;

        [SelfIndexed] // Implicit defaults: IndexingMode.LikeOrContains, PersistenceMode.Json
        public string LastName
        {
            get => _lastName;
            set
            {
                if (!Equals(_lastName, value))
                {
                    _lastName = value;
                    OnPropertyChanged();
                }
            }
        }
        string _lastName = string.Empty;

        [SelfIndexed(IndexingMode.TagMatchTerm)]
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
        string _tags = string.Empty;
    }

    [Obsolete("Used in unit tests for early adopter (beta) migration support.")]
    /// <summary>
    /// Simple class that implements ISelfIndexedMarkdown by
    /// inheriting <see cref="SelfIndexed"/>.
    /// </summary>
    class SelfIndexedProfileOR : SelfIndexedOR, ISelfIndexedMarkdown
    {
        [PrimaryKey]
        public override string Id { get; set; } = "38CFE38E-0D90-4C9F-A4E5-845089CB2BB0";

        [SelfIndexed(IndexingMode.QueryOrFilter)]
        public string FirstName
        {
            get => _firstName;
            set
            {
                if (!Equals(_firstName, value))
                {
                    _firstName = value;
                    OnPropertyChanged();
                }
            }
        }
        string _firstName = string.Empty;

        [SelfIndexed] // Implicit defaults: IndexingMode.LikeOrContains, PersistenceMode.Json
        public string LastName
        {
            get => _lastName;
            set
            {
                if (!Equals(_lastName, value))
                {
                    _lastName = value;
                    OnPropertyChanged();
                }
            }
        }
        string _lastName = string.Empty;

        [SelfIndexed(IndexingMode.TagMatchTerm)]
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
        string _tags = string.Empty;
    }
}
