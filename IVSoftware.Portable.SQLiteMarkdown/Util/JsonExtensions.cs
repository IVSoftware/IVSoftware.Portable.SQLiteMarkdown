using IVSoftware.Portable.SQLiteMarkdown.Internal;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;

namespace IVSoftware.Portable.SQLiteMarkdown.Util
{
    public static class JsonExtensions
    {
        public sealed class TopologyJsonWrapper
        {
            private readonly object @base;

            private TopologyJsonWrapper(MarkdownContext @base)
            {
                this.@base = (IModeledMarkdownContext)@base ?? throw new ArgumentNullException(nameof(@base));
            }

            public static implicit operator TopologyJsonWrapper(MarkdownContext @base)
                => new(@base);

            public ICollection CanonicalSuperset
                => (ICollection)@base.CanonicalSuperset;

            public int Count
                => @base.Count;

            public IList ObservableNetProjection
                => _projection?.ObservableNetProjection ?? [];

            public IList PredicateMatchSubset
                => _projection?.PredicateMatchSubset ?? [];

            [JsonConverter(typeof(StringEnumConverter))]
            public ProjectionTopology ProjectionTopology
                => _projection?.ProjectionTopology ?? default;

            [JsonConverter(typeof(StringEnumConverter))]
            public ReplaceItemsEventingOption ReplaceItemsEventingOptions
                => _projection?.ReplaceItemsEventingOptions ?? default;
        }


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
