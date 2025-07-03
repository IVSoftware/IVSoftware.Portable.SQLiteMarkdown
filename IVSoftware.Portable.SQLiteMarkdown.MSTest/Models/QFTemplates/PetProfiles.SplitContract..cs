using SQLite;

namespace IVSoftware.Portable.SQLiteMarkdown.MSTest.Models.QFTemplates
{
    /// <summary>
    /// Base target for Split Contract testing.
    /// </summary>
    [Table("pets")]
    class PetProfileSC
    {
        [PrimaryKey]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = string.Empty;
        public string Species { get; set; } = string.Empty;
        public string Tags { get; set; } = string.Empty;
    }

    /// <summary>
    /// Name only, Query only.
    /// </summary>
    class PetProfileN : PetProfileSC
    {
        [QueryLikeTerm]
        public new string Name
        { 
            get => base.Name;
            set => base.Name = value;
        } 
    }

    /// <summary>
    /// Name and Species, Query only.
    /// </summary>
    class PetProfileNS : PetProfileSC
    {
        [QueryLikeTerm]
        public new string Name
        {
            get => base.Name;
            set => base.Name = value;
        }

        [QueryLikeTerm]
        public new string Species
        {
            get => base.Species;
            set => base.Species = value;
        }
    }

    /// <summary>
    /// Query on Name and Species; Filter on Name only.
    /// </summary>
    class PetProfileNS_N : PetProfileSC
    {

        [QueryLikeTerm, FilterLikeTerm]
        public new string Name
        {
            get => base.Name;
            set => base.Name = value;
        }

        [QueryLikeTerm]
        public new string Species
        {
            get => base.Species;
            set => base.Species = value;
        }
    }

    /// <summary>
    /// Name only, Query only, Strict Tag Match mode
    /// </summary>
    class PetProfileN_N_T : PetProfileSC
    {
        [QueryLikeTerm, FilterLikeTerm]
        public new string Name
        {
            get => base.Name;
            set => base.Name = value;
        }

        [TagMatchTerm]
        public new string Tags
        {
            get => base.Tags;
            set => base.Tags = value;
        }
    }

    /// <summary>
    /// Name only, Query only, Soft Tag Match mode
    /// </summary>
    class PetProfileN_NT_T : PetProfileSC
    {
        [QueryLikeTerm, FilterLikeTerm]
        public new string Name
        {
            get => base.Name;
            set => base.Name = value;
        }

        [QueryLikeTerm, FilterLikeTerm, TagMatchTerm]
        public new string Tags
        {
            get => base.Tags;
            set => base.Tags = value;
        }
    }

    /// <summary>
    /// A generous filter with broad reach.
    /// </summary>
    class PetProfileN_NST_T : PetProfileSC
    {
        [QueryLikeTerm, FilterLikeTerm]
        public new string Name
        {
            get => base.Name;
            set => base.Name = value;
        }

        [QueryLikeTerm, FilterLikeTerm]
        public new string Species
        {
            get => base.Species;
            set => base.Species = value;
        }

        [QueryLikeTerm, FilterLikeTerm, TagMatchTerm]
        public new string Tags
        {
            get => base.Tags;
            set => base.Tags = value;
        }
    }
}
