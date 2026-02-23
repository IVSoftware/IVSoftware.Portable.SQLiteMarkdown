using IVSoftware.Portable.Common.Attributes;
using Newtonsoft.Json;
using SQLite;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace IVSoftware.Portable.SQLiteMarkdown
{
    /// <summary>
    /// Provides a base class with functionality for tokenizing 
    /// property values based on custom attributes.
    /// </summary>
    public class SelfIndexed : ISelfIndexedMarkdown
    {
        public SelfIndexed()
        {
            if (_isPkCheckRequired && !
                GetType()
                .GetProperties()
                .Any(_ => _.GetCustomAttributes<PrimaryKeyAttribute>(inherit: false).Any()))
            {
                _isPkCheckRequired = false;
                var msg = $"The [PrimaryKey] attribute must be reapplied to override string {nameof(Id)} {{ get; }} in the derived class";
                throw new InvalidOperationException(msg);
            }
        }
        static bool _isPkCheckRequired = true;

        public string PrimaryKey => Id;

        [PrimaryKey]
        public virtual string Id
        {
            get => _id;
            set
            {
                if (!Equals(_id, value))
                {
                    _id = value;
                    OnPropertyChanged();
                }
            }
        }
        string _id = string.Empty;

        /// <summary>
        /// Gets or sets the term for SQL-like search functionality.
        /// Updated based on properties with SqlContainsAttribute.
        /// </summary>
        [QueryLikeTerm]
        public string QueryTerm
        {
            get => ensure(ref _likeTerm);
            set
            {
                if (!Equals(_likeTerm, value))
                {
                    _likeTerm = value;
                    OnPropertyChanged();
                }
            }
        }
        private string _likeTerm = string.Empty;

        /// <summary>
        /// Gets or sets the term used for filter-based searching.
        /// Updated based on properties with FilterContainsAttribute.
        /// </summary>
        [FilterLikeTerm]
        public string FilterTerm
        {
            get => ensure(ref _containsTerm);
            set
            {
                if (!Equals(_containsTerm, value))
                {
                    _containsTerm = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _containsTerm = string.Empty;

        /// <summary>
        /// Gets or sets the term used for tag-based searching.
        /// Updated based on properties with SqlTagsAttribute.
        /// </summary>
        [TagMatchTerm]
        public string TagMatchTerm
        {
            get => ensure(ref _tagMatchTerm);
            set
            {
                if (!Equals(_tagMatchTerm, value))
                {
                    _tagMatchTerm = value;
                    OnPropertyChanged();
                }
            }
        }
        private string _tagMatchTerm = string.Empty;

        /// <summary>
        /// When a getter is called on an index term, this method
        /// ensures that the indexing is always up do date on-demand.
        /// </summary>
        /// <remarks>
        /// USAGE
        /// [QueryLikeTerm] public string QueryTerm => ensure(ref _likeTerm);
        /// </remarks>
        [Canonical("On-demand Indexing engine.")]
        private string ensure(ref string indexedProperty)
        {
            if (_isIndexingRequired)
            if (_isIndexingRequired)

            {
                internalExecuteIndexing();
            }
            return indexedProperty;
        }


#if ABSTRACT
recordset = cnx.Query<SelectableQFModelTOQO>($@"
    Select *
    From items 
    Where {"Properties".JsonExtract("Description")} LIKE '%brown dog%'");
#endif
        /// <summary>
        /// SQLite Column that encapsulates user-defined custom values that can be queried.
        /// </summary>
        public string Properties
        {
            get
            {
                return JsonConvert.SerializeObject(internalProperties, Formatting.Indented);
            }
            set
            {
                if (value is null)
                {
                    internalProperties.Clear();
                }
                else
                {
                    internalProperties = JsonConvert.DeserializeObject<Dictionary<string, object>>(value)!;
                }
            }
        }

        // This is 'not'.
        private Dictionary<string, object> internalProperties
        {
            get
            {
                if (_internalProperties is null)
                {
                    _internalProperties = new Dictionary<string, object>();
                }
                return _internalProperties;
            }
            set => _internalProperties = value;
        }
        Dictionary<string, object>? _internalProperties = null;

        bool _isIndexingRequired = true;
#if false
        /// <summary>
        /// Tracks if the in-memory Values dictionary has been modified. Set to true upon property changes
        /// that affect the dictionary, allowing for selective updates.
        /// </summary>
        private bool _isDictionaryModified = false;

        /// <summary>
        /// Indicates whether serialization is required for persistence. Set to true only when necessary 
        /// to avoid unnecessary re-serialization, triggered by database updates or explicit persistence requests.
        /// </summary>
        bool _isSerializationRequired = false;
#endif


        #region P R O P E R T Y    C H A N G E S
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            switch (propertyName)
            {
                case nameof(QueryTerm):
                case nameof(TagMatchTerm):
                case nameof(FilterTerm):
                    // NOTE: It's recommended that these not be
                    // bindable properties in the first place.
                    break;
                default:
                    updateIfIndexed(propertyName);
                    break;
            }
        }

        /// <summary>
        /// The property is KNOWN TO HAVE CHANGED.
        /// </summary>
        private void updateIfIndexed(string propertyName)
        {
            if (persistedProperties[PersistenceMode.Json]
               .FirstOrDefault(_ => _.Name == propertyName)
                is PropertyInfo pi)
            {
                _isIndexingRequired = true;
                var cMe = pi.GetValue(this);
                internalProperties[propertyName] = cMe;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void internalExecuteIndexing()
        {
            lock (_lock)
            {
                // Used in old and new ways.
                var props = GetType().GetProperties();
                var localMC = new MarkdownContext(GetType());

#if true || NEW_WAY
                for (int i = 0; i < props.Length; i++)
                {
                    var pi = props[i];
                    var attr = pi.GetCustomAttribute<SelfIndexedAttribute>();
                    if (attr == null) continue;
                    var val = pi.GetValue(this);
                    if (!(val is IConvertible)) continue;

                    localMC.InputText = Convert.ToString(val);

                    if(attr.IndexingMode.HasFlag(IndexingMode.QueryLikeTerm))
                    {
                        using (localMC.DHostSelfIndexing.GetToken(nameof(IndexingMode), IndexingMode.QueryLikeTerm))
                        {
                            localMC.ParseSqlMarkdown();
                        }
                    }

                    if(attr.IndexingMode.HasFlag(IndexingMode.FilterLikeTerm))
                    {
                        using (localMC.DHostSelfIndexing.GetToken(nameof(IndexingMode), IndexingMode.FilterLikeTerm))
                        {
                            localMC.ParseSqlMarkdown();
                        }
                    }

                    if(attr.IndexingMode.HasFlag(IndexingMode.TagMatchTerm))
                    {
                        using (localMC.DHostSelfIndexing.GetToken(nameof(IndexingMode), IndexingMode.TagMatchTerm))
                        {
                            localMC.ParseSqlMarkdown();
                        }
                    }
                }

                QueryTerm = localMC.QueryTerm;
                FilterTerm = localMC.FilterTerm;
                TagMatchTerm = localMC.TagMatchTerm;
                { }

#else
                var likeNodes = new HashSet<ASTNode>();
                var containsNodes = new HashSet<ASTNode>();
                var tagNodes = new HashSet<ASTNode>();
                ASTNode[] astNodeArray;
                for (int i = 0; i < props.Length; i++)
                {
                    var pi = props[i];
                    var attr = pi.GetCustomAttribute<SelfIndexedAttribute>();
                    if (attr == null) continue;

                    var val = pi.GetValue(this);
                    if (!(val is IConvertible)) continue;

                    // Transitional preview.
                    localMC.InputText = Convert.ToString(val);
                    localMC.QueryFilterConfig = QueryFilterConfig.Query;
                    localMC.ParseSqlMarkdown();
                    var tagTerm = localMC.TagMatchTerm;
                    var queryTerm = localMC.QueryTerm;
                    localMC.QueryFilterConfig = QueryFilterConfig.Filter;
                    var filterTerm = localMC.FilterTerm;
                    localMC.ParseSqlMarkdown();
                    { }
                    if(!string.IsNullOrEmpty(tagTerm))
                    { }


                    var sval = Convert.ToString(val);
                    if (string.IsNullOrWhiteSpace(sval)) continue;

                    if (!sval.TryTokenizeOR(out astNodeArray)) continue;
                    for (int j = 0; j < astNodeArray.Length; j++)
                    {
                        var node = astNodeArray[j];
                        switch (node.ASTType)
                        {
                            case NodeType.Term:
                                if (attr.IndexingMode.HasFlag(IndexingMode.QueryLikeTerm))
                                {
                                    likeNodes.Add(node);
                                }
                                if (attr.IndexingMode.HasFlag(IndexingMode.FilterLikeTerm))
                                {
                                    containsNodes.Add(node);
                                }
                                break;
                            case NodeType.Tag:
                                if (attr.IndexingMode.HasFlag(IndexingMode.QueryLikeTerm))
                                {
                                    // Tags treated as terms when LikeTerm is also set
                                    likeNodes.Add(new ASTNode(NodeType.Term, node.Value));
                                }
                                if (attr.IndexingMode.HasFlag(IndexingMode.FilterLikeTerm))
                                {

                                }
                                break;
                            default:
                                break;
                        }
                    }

                    if (attr.IndexingMode.HasFlag(IndexingMode.TagMatchTerm))
                    {
                        for (int j = 0; j < astNodeArray.Length; j++)
                        {
                            var node = astNodeArray[j];
                            if (node.ASTType == NodeType.Tag)
                                tagNodes.Add(node);
                        }
                    }
                }

                QueryTerm = likeNodes.ToArray().GenerateTerm(NodeTypeFlags.Term);
                FilterTerm = containsNodes.ToArray().GenerateTerm(NodeTypeFlags.Term);
                TagMatchTerm = tagNodes.ToArray().GenerateTerm(NodeTypeFlags.Tag);
                
#endif
                _isIndexingRequired = false;
            }
        }
#if false
        private void internalExecuteIndexing()
        {
            lock (_lock)
            {
                var queryLikeNodes = new HashSet<ASTNode>();
                var filterLikeNodes = new HashSet<ASTNode>();
                var tagNodes = new HashSet<ASTNode>();
                ASTNode[] astNodeArray;

                var props = GetType().GetProperties();

                for (int i = 0; i < props.Length; i++)
                {
                    var pi = props[i];
                    var attr = pi.GetCustomAttribute<SelfIndexedAttribute>();
                    if (attr == null) continue;

                    var val = pi.GetValue(this);
                    if (!(val is IConvertible)) continue;

                    var sval = Convert.ToString(val);
                    if (string.IsNullOrWhiteSpace(sval)) continue;

                    if (!sval.TryTokenize(out astNodeArray)) continue;

                    if (attr.IndexingMode.HasFlag(IndexingMode.QueryLikeTerm) ||
                        attr.IndexingMode.HasFlag(IndexingMode.FilterLikeTerm))
                    {
                        for (int j = 0; j < astNodeArray.Length; j++)
                        {
                            var node = astNodeArray[j];
                            switch (node.ASTType)
                            {
                                case NodeType.Term:
                                    if (attr.IndexingMode.HasFlag(IndexingMode.QueryLikeTerm))
                                        queryLikeNodes.Add(node);
                                    if (attr.IndexingMode.HasFlag(IndexingMode.FilterLikeTerm))
                                        filterLikeNodes.Add(node);
                                    break;
                                case NodeType.Tag:
                                    var adhocTermNode = new ASTNode(NodeType.Term, node.Value);
                                    // Tags treated as terms when LikeTerm is also set for mode.
                                    if (attr.IndexingMode.HasFlag(IndexingMode.QueryLikeTerm))
                                        queryLikeNodes.Add(adhocTermNode);
                                    if (attr.IndexingMode.HasFlag(IndexingMode.FilterLikeTerm))
                                        filterLikeNodes.Add(adhocTermNode);
                                    break;
                            }
                        }
                    }

                    if (attr.IndexingMode.HasFlag(IndexingMode.TagMatchTerm))
                    {
                        for (int j = 0; j < astNodeArray.Length; j++)
                        {
                            var node = astNodeArray[j];
                            if (node.ASTType == NodeType.Tag)
                                tagNodes.Add(node);
                        }
                    }
                }

                QueryTerm = queryLikeNodes.ToArray().GenerateTerm(NodeTypeFlags.Term);
                FilterTerm = filterLikeNodes.ToArray().GenerateTerm(NodeTypeFlags.Term);
                TagMatchTerm = tagNodes.ToArray().GenerateTerm(NodeTypeFlags.Tag);

                _isIndexingRequired = false;
            }
        }
#endif
        private object _lock = new object();

        #endregion P R O P E R T Y    C H A N G E S

        #region I N D E X I N G    &    P E R S I S T E N C E    M A P S

        /// <summary>
        /// Maps properties to their associated <see cref="IndexingMode"/> roles for search functionality.
        /// Only includes properties decorated with <see cref="SelfIndexedAttribute"/> where:
        /// - The attribute specifies a non-None <see cref="PersistenceMode"/>, or
        /// - The property is not marked with <see cref="IgnoreAttribute"/>
        /// </summary>
        protected Dictionary<IndexingMode, List<PropertyInfo>> indexedProperties
        {
            get
            {
                if (_indexedProperties == null)
                {
                    _indexedProperties = new Dictionary<IndexingMode, List<PropertyInfo>>();
                    _indexedProperties[IndexingMode.QueryLikeTerm] = new List<PropertyInfo>();
                    _indexedProperties[IndexingMode.FilterLikeTerm] = new List<PropertyInfo>();
                    _indexedProperties[IndexingMode.TagMatchTerm] = new List<PropertyInfo>();

                    var props = GetType().GetProperties();

                    foreach (var pi in props)
                    {
                        var attr = pi.GetCustomAttribute<SelfIndexedAttribute>();
                        if (attr != null)
                        {
                            bool isIgnored = pi.GetCustomAttribute<IgnoreAttribute>() != null;
                            bool isPersisted = attr.PersistenceMode != PersistenceMode.None;

                            if (isIgnored && !isPersisted)
                                continue;

                            foreach (var mode in new[] { IndexingMode.QueryLikeTerm, IndexingMode.FilterLikeTerm, IndexingMode.TagMatchTerm })
                            {
                                if ((attr.IndexingMode & mode) == mode)
                                    _indexedProperties[mode].Add(pi);
                            }
                        }
                    }
                }
                return _indexedProperties;
            }
        }
        Dictionary<IndexingMode, List<PropertyInfo>>? _indexedProperties = null;

        /// <summary>
        /// Maps properties to their associated <see cref="PersistenceMode"/> roles for serialization.
        /// Includes only those decorated with <see cref="SelfIndexedAttribute"/> and with non-None <see cref="PersistenceMode"/> values.
        /// </summary>
        protected Dictionary<PersistenceMode, List<PropertyInfo>> persistedProperties
        {
            get
            {
                if (_persistedProperties == null)
                {
                    _persistedProperties = new Dictionary<PersistenceMode, List<PropertyInfo>>();
                    _persistedProperties[PersistenceMode.Json] = new List<PropertyInfo>();

                    var props = GetType().GetProperties();

                    foreach (var pi in props)
                    {
                        var attr = pi.GetCustomAttribute<SelfIndexedAttribute>();
                        if (attr != null)
                        {
                            foreach (var mode in new[] { PersistenceMode.Json })
                            {
                                if ((attr.PersistenceMode & mode) == mode)
                                    _persistedProperties[mode].Add(pi);
                            }
                        }
                    }
                }
                return _persistedProperties;
            }
        }
        Dictionary<PersistenceMode, List<PropertyInfo>> _persistedProperties = null;

        #endregion I N D E X I N G    &    P E R S I S T E N C E    M A P S
    }


    [Obsolete("Used in unit tests for early adopter (beta) migration support.")]
    public class SelfIndexedOR : SelfIndexed
    {
        /// <summary>
        /// Gets or sets the term for SQL-like search functionality.
        /// Updated based on properties with SqlContainsAttribute.
        /// </summary>
        [SqlLikeTerm]
        public new string QueryTerm
        {
            get => base.QueryTerm;
            set => base.QueryTerm = value;
        }

        /// <summary>
        /// Gets or sets the term used for filter-based searching.
        /// Updated based on properties with FilterContainsAttribute.
        /// </summary>
        [FilterContainsTerm]
        public new string FilterTerm
        {
            get => base.FilterTerm;
            set => base.FilterTerm = value;
        }
    }
}
