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
            private readonly ITopology @base;

            private TopologyJsonWrapper(MarkdownContext @base)
            {
                this.@base = (ITopology)@base ?? throw new ArgumentNullException(nameof(@base));
            }

            public static implicit operator TopologyJsonWrapper(MarkdownContext @base)
                => new(@base);

            public IList ObservableNetProjection => @base.ObservableNetProjection ?? new ArrayList();

            public ICollection CanonicalSuperset => (ICollection)@base.CanonicalSuperset;

            public IList PredicateMatchSubset => @base.PredicateMatchSubset;

            [JsonConverter(typeof(StringEnumConverter))]
            public ProjectionTopology ProjectionTopology => @base.ProjectionTopology;

            [JsonConverter(typeof(StringEnumConverter))]
            public NetProjectionOption ProjectionOption => @base.ProjectionOption;

            [JsonConverter(typeof(StringEnumConverter))]
            public ReplaceItemsEventingOption ReplaceItemsEventingOptions => @base.ReplaceItemsEventingOptions;

            public int Count => @base.Count;
        }
        public static string SerializeTopology(
            this MarkdownContext context,
            Formatting formatting = Formatting.Indented)
        {
            if (context is null) throw new ArgumentNullException(nameof(context));

            return JsonConvert.SerializeObject(
                (TopologyJsonWrapper)context,
                formatting,
                new JsonSerializerSettings
                {
                    Converters = { new StringEnumConverter() }
                });
        }
    }
}
