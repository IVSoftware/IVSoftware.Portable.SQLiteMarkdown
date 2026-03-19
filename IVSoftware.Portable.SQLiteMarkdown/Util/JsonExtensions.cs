using IVSoftware.Portable.SQLiteMarkdown.Internal;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IVSoftware.Portable.SQLiteMarkdown.Util
{
    public static class JsonExtensions
    {
        public static string SerializeTopology<T>(
            this IMarkdownContext mmdc,
            Formatting formatting = Formatting.Indented)
        {
            return JsonConvert.SerializeObject(
                mmdc,
                formatting,
                new JsonSerializerSettings
                {
                    ContractResolver = TopologyOnlyResolver<T>.Instance,
                    Converters = { new StringEnumConverter() }
                });
        }
        sealed class TopologyOnlyResolver<T> : DefaultContractResolver
        {
            public static readonly TopologyOnlyResolver<T> Instance = new();

            static readonly Type _topologyType = typeof(Topology<T>);

            protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization ms)
            {
                var props = base.CreateProperties(type, ms);

                // Only filter when we're serializing the root (Topology/MMDC)
                if (_topologyType.IsAssignableFrom(type))
                {
                    return props
                        .Where(p =>
                            p.DeclaringType != null &&
                            _topologyType.IsAssignableFrom(p.DeclaringType))
                        .ToList();
                }

                // For all other types (e.g. items in CanonicalSuperset), leave intact
                return props;
            }
        }
    }
}
