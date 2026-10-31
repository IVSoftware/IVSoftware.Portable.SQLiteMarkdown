using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SQLite;

namespace IVSoftware.Portable.SQLiteMarkdown;

/// <summary>
/// Resolves parser table identity without opening a database or involving a collection model.
/// </summary>
internal static class SQLiteContractMapping
{
    private static readonly ConcurrentDictionary<Type, TableMapping> Mappings = new();

    internal static IEnumerable<Type> BaseTypes(this Type type, bool includeSelf = false)
    {
        for (Type? current = includeSelf ? type : type.BaseType; current is not null; current = current.BaseType)
            yield return current;
    }

    internal static TableMapping GetSQLiteMapping(this Type type, Type? contractType = null, bool @throw = true)
    {
        var hierarchy = new List<Type>();
        for (Type? current = type; current is not null && current != typeof(object); current = current.BaseType)
            hierarchy.Add(current);
        Type owner = hierarchy.LastOrDefault(candidate => candidate.GetCustomAttribute<TableAttribute>(false) is not null) ?? type;
        TableMapping mapping = Mappings.GetOrAdd(owner, candidate => new TableMapping(candidate, CreateFlags.None));
        if (contractType is null || contractType == type) return mapping;
        TableMapping contract = contractType.GetSQLiteMapping();
        if (mapping.TableName == contract.TableName || contractType.IsAssignableFrom(type) || type.IsAssignableFrom(contractType))
            return contract;
        if (@throw) throw new InvalidOperationException("Proxy type does not resolve to the contract table.");
        return null!;
    }
}
