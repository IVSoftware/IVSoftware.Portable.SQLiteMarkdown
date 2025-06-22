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
            if(_isPkCheckRequired && !
                GetType()
                .GetProperties()
                .Any(_=>_.GetCustomAttributes<PrimaryKeyAttribute>(inherit: false).Any()))
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
        [SqlLikeTerm]
        public string LikeTerm
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
        [FilterContainsTerm]
        public string ContainsTerm
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
        /// When a getter is called on an an index term, this method
        /// ensures that the indexing is always up do date on-demand.
        /// </summary>
        private string ensure(ref string indexedProperty)
        {
            if(_isIndexingRequired)
            {
                internalExecuteIndexing();
            }
            return indexedProperty;
        }


        // This 'is' a SQLiteColumn.
        public string Properties 
        {
            get
            {
                return JsonConvert.SerializeObject(internalProperties, Formatting.Indented);
            }
            set
            {
                internalProperties = JsonConvert.DeserializeObject<Dictionary<string, object>>(value);
            }
        }

        // This is 'not'.
        private Dictionary<string, object> internalProperties 
        { 
            get
            {
                if(_internalProperties is null)
                {
                    _internalProperties = new Dictionary<string, object>();
                }
                return _internalProperties;
            }
            set => _internalProperties = value;
        }
        Dictionary<string, object> _internalProperties = null;

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
                case nameof(LikeTerm):
                case nameof(TagMatchTerm):
                case nameof(ContainsTerm):
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
            if( persistedProperties[PersistenceMode.Json]
               .FirstOrDefault(_ => _.Name == propertyName)
                is PropertyInfo pi)
            {
                _isIndexingRequired = true;
                var cMe = pi.GetValue(this);
                internalProperties[propertyName] = cMe;
                wdtPropertyChanged.StartOrRestart();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Gets a watchdog timer that triggers term updates when a property
        /// change event occurs, after a brief delay.
        /// </summary>
        private WatchdogTimer wdtPropertyChanged
        {
            get
            {
                if (_wdtPropertyChanged is null)
                {
                    _wdtPropertyChanged = new WatchdogTimer(defaultCompleteAction: () =>
                    {
                        internalExecuteIndexing();
                    })
                    { Interval = TimeSpan.FromSeconds(0.25) };
                }
                return _wdtPropertyChanged;
            }
        }

        private WatchdogTimer _wdtPropertyChanged = default;
        private void internalExecuteIndexing()
        {
            lock (_lock)
            {
                var likeNodes = new HashSet<ASTNode>();
                var containsNodes = new HashSet<ASTNode>();
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

                    if (attr.IndexingMode.HasFlag(IndexingMode.LikeTerm) ||
                        attr.IndexingMode.HasFlag(IndexingMode.ContainsTerm))
                    {
                        for (int j = 0; j < astNodeArray.Length; j++)
                        {
                            var node = astNodeArray[j];
                            if (node.ASTType == NodeType.Term)
                            {
                                if (attr.IndexingMode.HasFlag(IndexingMode.LikeTerm))
                                    likeNodes.Add(node);
                                if (attr.IndexingMode.HasFlag(IndexingMode.ContainsTerm))
                                    containsNodes.Add(node);
                            }
                            else if (node.ASTType == NodeType.Tag &&
                                     attr.IndexingMode.HasFlag(IndexingMode.LikeTerm))
                            {
                                // Tags treated as terms when LikeTerm is also set
                                likeNodes.Add(new ASTNode(NodeType.Term, node.Value));
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

                LikeTerm = likeNodes.ToArray().GenerateTerm(NodeTypeFlags.Term);
                ContainsTerm = containsNodes.ToArray().GenerateTerm(NodeTypeFlags.Term);
                TagMatchTerm = tagNodes.ToArray().GenerateTerm(NodeTypeFlags.Tag);

                _isIndexingRequired = false;
            }
        }

#if false
        private void internalExecuteIndexing()
        {
            lock (_lock)
            {
                LikeTerm = localGenerateTermFromProperties(IndexingMode.LikeTerm);
                ContainsTerm = localGenerateTermFromProperties(IndexingMode.ContainsTerm);
                TagMatchTerm = localGenerateTermFromProperties(IndexingMode.TagMatchTerm);
                string localGenerateTermFromProperties(IndexingMode key)
                {
                    var astNodesHashSet = new HashSet<ASTNode>();
                    ASTNode[] astNodeArray;

                    lock (_lock)
                    {
                        foreach (var pi in indexedProperties[key])
                        {
                            bool isLikeTermFromTag =
                                Equals(key, IndexingMode.TagMatchTerm) &&
                                (pi.GetCustomAttribute<SelfIndexedAttribute>()?.IndexingMode.HasFlag(IndexingMode.LikeTerm) ?? false);

                            if (isLikeTermFromTag)
                            {
                            }

                            var cMe = pi.GetValue(this);
                            { }
                            if (cMe is IConvertible conv &&
                                Convert.ToString(conv) is string sval &&
                                !string.IsNullOrWhiteSpace(sval) &&
                                sval.TryTokenize(out astNodeArray))
                            {
                                foreach (var astNode in astNodeArray)
                                {
                                    astNodesHashSet.Add(astNode);
                                }
                            }
                        }
                    }
                    NodeTypeFlags nodeTypes = 0;
                    if (key.HasFlag(IndexingMode.LikeTerm) ||
                        key.HasFlag(IndexingMode.ContainsTerm))
                    {
                        nodeTypes |= NodeTypeFlags.Term;
                    }
                    if (key.HasFlag(IndexingMode.TagMatchTerm))
                    {
                        nodeTypes |= NodeTypeFlags.Tag;
                    }
                    return astNodesHashSet.ToArray().GenerateTerm(nodeTypes);
                }
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
                    _indexedProperties[IndexingMode.LikeTerm] = new List<PropertyInfo>();
                    _indexedProperties[IndexingMode.ContainsTerm] = new List<PropertyInfo>();
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

                            foreach (var mode in new[] { IndexingMode.LikeTerm, IndexingMode.ContainsTerm, IndexingMode.TagMatchTerm })
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
        Dictionary<IndexingMode, List<PropertyInfo>> _indexedProperties = null;

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
}
