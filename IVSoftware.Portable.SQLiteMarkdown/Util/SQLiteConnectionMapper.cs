using IVSoftware.Portable;
using IVSoftware.Portable.Common.Attributes;
using IVSoftware.Portable.Common.Exceptions;
using IVSoftware.Portable.Disposable;
using IVSoftware.Portable.SQLiteMarkdown.Internal;
using IVSoftware.Portable.Xml.Linq;
using IVSoftware.Portable.Xml.Linq.XBoundObject;
using IVSoftware.Portable.Xml.Linq.XBoundObject.Modeling;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SQLite;
using SQLitePCL;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml.Linq;

namespace IVSoftware.Portable.SQLiteMarkdown.Util
{
    /// <summary>
    /// Provides utilities for resolving SQLite table mappings and structural metadata.
    /// </summary>
    /// <remarks>
    /// Includes helpers that:
    /// - Resolve the effective <see cref="TableMapping"/> for a type, honoring base-class
    ///   <c>[Table]</c> declarations and optional contract-type overrides.
    /// - Expose primary-key metadata for reflection-driven scenarios.
    /// - Provide a universal <c>FullPath</c> accessor that works for both
    ///   <see cref="IFullPathAffinity"/> instances and arbitrary objects via
    ///   <see cref="ReadOnlyFullPathAffinity"/> structural adaptation.
    /// </remarks>
    public static class SQLiteConnectionMapper
    {
        /// <summary>
        /// Resolves the effective SQLite <see cref="TableMapping"/> for the specified type.
        /// </summary>
        /// <remarks>
        /// Traverses the inheritance chain to locate the first explicit <c>[Table]</c>
        /// declaration. If none is found, the mapping inferred by SQLite is used.
        /// An optional <paramref name="contractType"/> may override the proxy mapping
        /// when the proxy participates in a contract-bound hierarchy.
        /// </remarks>
        public static TableMapping GetSQLiteMapping(
            this Type type,
            CreateFlags createFlags = CreateFlags.None,
            Type? contractType = null)
            => type.GetSQLiteMapping(out _, out _, createFlags, contractType);

        /// <summary>
        /// Resolves the effective SQLite <see cref="TableMapping"/> for the specified type.
        /// </summary>
        /// <remarks>
        /// Traverses the inheritance chain to locate the first explicit <c>[Table]</c>
        /// declaration. If none is found, the mapping inferred by SQLite is used.
        /// An optional <paramref name="contractType"/> may override the proxy mapping
        /// when the proxy participates in a contract-bound hierarchy.
        /// </remarks>
        [Canonical]
        public static TableMapping GetSQLiteMapping(
            this Type type,
            out string? pkName,
            out string? pkPropertyName,
            CreateFlags createFlags = CreateFlags.None,
            Type? contractType = null)
        {
            pkName = string.Empty;
            pkPropertyName = string.Empty;
            if (type.Name == nameof(Type))
            {
                type.ThrowHard<ArgumentException>(
    $"The mapper expects a model type, but '{nameof(Type)}' itself was supplied. " +
    "Pass the concrete type of the entity to be mapped.");
                return null!; 
            }

            TableMapping? proxyMapping = null;
            foreach (var @base in type.BaseTypes(includeSelf: true))
            {
                if (@base.GetCustomAttribute<SQLite.TableAttribute>(inherit: false) is not null)
                {
                    proxyMapping = Mapper.GetMapping(@base, createFlags);
                    break;
                }
            }
            if (proxyMapping is null)
            {
                // Did not find any explicit [Table] tags.
                proxyMapping = Mapper.GetMapping(type, createFlags);
                Debug.Assert(
                    type.Name == proxyMapping.TableName,
                    "Expecting the name of the Type will be used as the table name.");
            }
            { } // <- L O O K    N E T    R E S U L T    O N    P R O X Y

            if (contractType is not null && contractType != type)
            {
                TableMapping contractMapping = Mapper.GetMapping(contractType, createFlags);
                if (contractMapping.TableName != proxyMapping.TableName)
                {
                    if (contractType.IsAssignableFrom(type))
                    {
                        // This one's interesting! When the contract type is explicitly assigned (as
                        // it is in this call) it tells the proxy, "Don't believe your lying eyes."
                        proxyMapping = contractMapping;
                    }
                    else
                    {
                        TableMapping? adhocMapping = null;
                        foreach (var @base in type.BaseTypes(includeSelf: true).Reverse())
                        {
                            if (@base.GetCustomAttribute<SQLite.TableAttribute>(inherit: false) is not null)
                            {
                                adhocMapping = Mapper.GetMapping(@base, createFlags);
                                if (adhocMapping.TableName == contractMapping.TableName)
                                {
                                    proxyMapping = adhocMapping;
                                    goto breakFromInner;
                                }
                            }
                        }
                        Mapper.ThrowHard<InvalidOperationException>("Proxy type cannot resolve to the contract table.");
                        return null!;
                        breakFromInner:;
                    }
                }
                { } // <- L O O K    N E T    R E S U L T    O N    P R O X Y
            }
            pkName = proxyMapping.PK?.Name;
            pkPropertyName = proxyMapping.PK?.PropertyName;
            return proxyMapping;
        }

        /// <summary>
        /// Returns the primary-key column mapping for the specified type.
        /// </summary>
        /// <remarks>
        /// This is a convenience wrapper over <see cref="GetSQLiteMapping(Type)"/> used
        /// by reflection-driven infrastructure that treats the SQLite primary key as
        /// the canonical identifier for a model instance.
        /// </remarks>
        public static TableMapping.Column? GetPK(this Type type)
            => type.GetSQLiteMapping().PK;

        /// <summary>
        /// Returns the hierarchical placement path of an object known to be <see cref="IFullPathAffinity"/>.
        /// </summary>
        /// <remarks>
        /// Fast-path accessor when the interface is known. The returned value represents the
        /// path used to position the item within the MarkdownContext <c>Model</c>
        /// element tree.
        /// </remarks>
        public static string GetFullPath(this IFullPathAffinity @this)
            => @this.FullPath;

        /// <summary>
        /// Attempts to determine the hierarchical placement path of an arbitrary object.
        /// </summary>
        /// <remarks>
        /// This is a heuristic. <see cref="ReadOnlyFullPathAffinity"/> is used to interpret
        /// the instance as <see cref="IFullPathAffinity"/> using reflection and SQLite
        /// metadata. The result is suitable for positioning within the MarkdownContext
        /// <c>Model</c> element tree when sufficient structure is present.
        ///
        /// Returns an empty string when no path information can be inferred.
        /// </remarks>
        public static string GetFullPath(this object? @this)
            => ReadOnlyFullPathAffinity.Create(@this)?.FullPath ?? string.Empty;

        /// <summary>
        /// Attempts to determine the value of the property designated as the primary key.
        /// </summary>
        /// <remarks>
        /// This is a heuristic. <see cref="ReadOnlyFullPathAffinity"/> is used to interpret
        /// the instance as <see cref="IFullPathAffinity"/> using reflection and SQLite
        /// metadata. The result is suitable for hash set contains.
        ///
        /// Returns an empty string when no path information can be inferred.
        /// </remarks>
        public static string GetId(this object? @this)
            => ReadOnlyFullPathAffinity.Create(@this)?.Id ?? string.Empty;

        /// <summary>
        /// Provides the shared SQLite mapper connection used for metadata inspection.
        /// </summary>
        /// <remarks>
        /// This in-memory connection exists only so SQLite-net can construct and cache
        /// <see cref="TableMapping"/> metadata (for example primary-key discovery).
        /// 
        /// Because SQLite-net internally caches mappings per connection, keeping a single
        /// shared instance avoids repeated reflection and ensures stable mapping behavior
        /// across the application.
        /// </remarks>
        static SQLiteConnection Mapper
        {
            get
            {
                if (_mapper is null)
                {
                    _mapper = new SQLiteConnection(":memory:");
                }
                return _mapper;
            }
        }
        static SQLiteConnection? _mapper = null;
    }
}
