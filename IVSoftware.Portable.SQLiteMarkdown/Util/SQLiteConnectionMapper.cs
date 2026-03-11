using IVSoftware.Portable;
using IVSoftware.Portable.Common.Attributes;
using IVSoftware.Portable.Common.Exceptions;
using IVSoftware.Portable.Disposable;
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
    public static class SQLiteConnectionMapper
    {
        public static TableMapping GetSQLiteMapping(
            this Type type,
            CreateFlags createFlags = CreateFlags.None,
            Type? contractType = null)
            => type.GetSQLiteMapping(out _, out _, createFlags, contractType);
        public static TableMapping GetSQLiteMapping(
            this Type type,
            out string? pkName,
            out string? pkPropertyName,
            CreateFlags createFlags = CreateFlags.None,
            Type? contractType = null)
        {
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
                        pkName = string.Empty;
                        pkPropertyName = string.Empty;
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

        public static TableMapping.Column? GetPK(this Type type)
            => type.GetSQLiteMapping().PK;

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
