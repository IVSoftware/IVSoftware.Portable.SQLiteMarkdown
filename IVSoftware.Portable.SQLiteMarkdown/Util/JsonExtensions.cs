using IVSoftware.Portable.SQLiteMarkdown.Internal;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace IVSoftware.Portable.SQLiteMarkdown.Util
{
    public static class JsonExtensions
    {
        public static string SerializeTopology(
            this object obj,
            Formatting formatting = Formatting.Indented)
        {
            return JsonConvert.SerializeObject(
                obj,
                formatting,
                new JsonSerializerSettings
                {
                    ContractResolver = new TopologyOnlyResolver(),
                    Converters = { new StringEnumConverter() }
                });
        }

        sealed class TopologyOnlyResolver : DefaultContractResolver
        {
            protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization ms)
            {
                var props = base.CreateProperties(type, ms);

                var topologyType = GetClosedTopologyType(type);
                if (topologyType is null)
                    return props;

                return props
                    .Where(p => p.DeclaringType == topologyType)
                    .ToList();
            }

            static Type? GetClosedTopologyType(Type type)
            {
                while (type != null)
                {
                    if (type.IsGenericType &&
                        type.GetGenericTypeDefinition() == typeof(Topology<>))
                    {
                        return type; // closed Topology<T>
                    }
                    type = type.BaseType!;
                }
                return null;
            }
        }
    }
}
