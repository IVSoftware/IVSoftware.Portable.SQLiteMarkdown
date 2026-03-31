using IVSoftware.Portable.Common.Attributes;
using IVSoftware.Portable.SQLiteMarkdown.Collections.Preview;
using IVSoftware.Portable.SQLiteMarkdown.Common;
using IVSoftware.Portable.Xml.Linq.XBoundObject;
using Newtonsoft.Json;
using SQLite;
using System.CodeDom;
using System.Collections;

namespace IVSoftware.Portable.SQLiteMarkdown.MSTest
{
    [Flags]
    public enum PopulateOptions
    {
        RandomChecks = 0x1,
        DetectIRangeable = 0x2,
    }
    public static partial class SQLiteMarkdownTestExtensions
    {
        public static void PopulateDemoDatabase<TItem>(this SQLiteConnection @this, bool includeLive = false, PopulateOptions? options = null)
            where TItem : class, new()
        {
            @this.CreateTable<TItem>();

            var list = new List<TItem>().PopulateForDemo(includeLiveDemo: includeLive, options);
            @this.InsertAll(list);
        }

        public static TItem AddDynamic<TItem>(this IList<TItem>? @this, string description, string tags = "[]", bool isChecked = false, List<string>? keywords = null)
            where TItem : class, new()
            => ((IList?)@this).AddDynamic<TItem>(description, tags, isChecked, keywords);

        [Canonical]
        public static TItem AddDynamic<TItem>(this IList? @this, string description, string tags = "[]", bool isChecked = false, List<string>? keywords = null)
            where TItem : class, new()
        {
            var itemT = new TItem();
            typeof(TItem).GetProperty("Description")?.SetValue(itemT, description);
            typeof(TItem).GetProperty("Tags")?.SetValue(itemT, tags);
            typeof(TItem).GetProperty("IsChecked")?.SetValue(itemT, isChecked);
            if (keywords != null)
            {
                var json = JsonConvert.SerializeObject(keywords);
                typeof(TItem).GetProperty("Keywords")?.SetValue(itemT, json);
            }
            @this?.Add(itemT);
            return itemT;
        }

        public static IList<TItem> PopulateForDemo<TItem>(this IList<TItem>? @this, bool includeLiveDemo = false, PopulateOptions? options = null)
            where TItem : class, new()
        {
            Random rando = new(10);

            if (@this is null)
            {
                @this = new List<TItem>();
            }
            else
            {
                @this.Clear();
            }

            void Add(string description, string tags, bool isChecked, List<string>? keywords = null)
            {
                var itemT = @this.AddDynamic<TItem>(description, tags, isChecked, keywords);
                if (options?.HasFlag(PopulateOptions.RandomChecks) == true)
                {
                    isChecked = rando.Next(2) == 1;
                }
            }

            Add("Brown Dog", "[canine][color]", false, new() { "loyal", "friend", "furry" });
            Add("Green Apple", "[fruit][color]", false, new() { "tart", "snack", "healthy" });
            Add("Yellow Banana", "[fruit][color]", false);
            Add("Blue Bird", "[bird][color]", false, new() { "sky", "feathered", "song" });
            Add("Red Cherry", "[fruit][color]", false, new() { "sweet", "summer", "dessert" });
            Add("Black Cat", "[animal][color]", false);
            Add("Orange Fox", "[animal][color]", false);
            Add("White Rabbit", "[animal][color]", false, new() { "bunny", "soft", "jump" });
            Add("Purple Grape", "[fruit][color]", false);
            Add("Gray Wolf", "[animal][color]", false, new() { "pack", "howl", "wild" });
            Add("Pink Flamingo", "[bird][color]", false);
            Add("Golden Lion", "[animal][color]", false);
            Add("Brown Bear", "[animal][color]", false, new() { "strong", "wild", "forest" });
            Add("Green Pear", "[fruit][color]", false);
            Add("Red Strawberry", "[fruit][color]", false);
            Add("Black Panther", "[animal][color]", false, new() { "stealthy", "feline", "night" });
            Add("Yellow Lemon", "[fruit][color]", false);
            Add("White Swan", "[bird][color]", false);
            Add("Purple Plum", "[fruit][color]", false);
            Add("Blue Whale", "[marine-mammal][ocean]", false, new() { "ocean", "mammal", "giant" });
            Add("Elephant", "[animal]", false, new() { "trunk", "herd", "safari" });
            Add("Pineapple", "[fruit]", false);
            Add("Shark", "[fish]", false);
            Add("Owl", "[bird]", false);
            Add("Giraffe", "[animal]", false);
            Add("Coconut", "[fruit]", false);
            Add("Kangaroo", "[animal]", false, new() { "bounce", "outback", "marsupial" });
            Add("Dragonfruit", "[fruit]", false);
            Add("Turtle", "[animal]", false);
            Add("Mango", "[fruit]", false);
            Add("Should NOT match an expression with an \"animal\" tag.", "[not animal]", false);

            if (includeLiveDemo)
            {
                Add("Appetizer Plate", "[dish]", false, new() { "starter", "appealing", "snack" });
                Add("Errata", "[notes]", false, new() { "crunchy", "green", "appended" });
                Add("Happy Camper", "[phrase]", false, new() { "joyful", "camp", "approach-west" });
                Add("Great example - Markdown Demo", "[app] [portable]", false, new() { "digital", "mobile", "software" });
                Add("Application Form", "[document]", false, new() { "paperwork", "apply" });
                Add("App Store", "[app]", false, new() { "digital", "mobile", "software" });
            }

            return @this;
        }

        public static IList<TItem> PopulateForDemo<TItem>(this IList<TItem>? @this, int count, PopulateOptions? options = null)
            where TItem : new()
        {
            Random rando = new(10);
            if (@this is null)
            {
                @this = new List<TItem>();
            }
            else
            {
                @this.Clear();
            }

            if (options?.HasFlag(PopulateOptions.DetectIRangeable) is true && @this is IRangeable rangeable)
            {
                // Safe (not circular) because this object is not IRangeable.
                IList<TItem> stagedForTest = new List<TItem>().PopulateForDemo(count);
#if DEBUG
                var cMe = JsonConvert.SerializeObject(stagedForTest, Formatting.Indented);
#endif

                rangeable.AddRange(stagedForTest);
            }
            else
            {
                for (int i = 1; i <= count; i++)
                {
                    Add(
                        description: $"Item{i:d2}",
                        tags: string.Empty,
                        isChecked: options?.HasFlag(PopulateOptions.RandomChecks) == true && rando.Next(2) == 1);
                }

                void Add(string description, string tags, bool isChecked, List<string>? keywords = null)
                {
                    var instance = new TItem();
                    typeof(TItem).GetProperty("Description")?.SetValue(instance, description);
                    typeof(TItem).GetProperty("Tags")?.SetValue(instance, tags);
                    typeof(TItem).GetProperty("IsChecked")?.SetValue(instance, isChecked);
                    if (keywords != null)
                    {
                        var json = JsonConvert.SerializeObject(keywords);
                        typeof(TItem).GetProperty("Keywords")?.SetValue(instance, json);
                    }
                    @this.Add(instance);
                }
            }
            return @this;
        }

        public static T DequeueSingle<T>(this Queue<T> queue)
            => queue.Count switch
            {
                0 => throw new InvalidOperationException("Queue is empty."),
                1 => queue.Dequeue(),
                _ => throw new InvalidOperationException("Multiple items in queue."),
            };

        public static string StateReport(this MarkdownContext @this)
        {
            var builder = new List<string>();
            builder.Add($"[IME Len: {@this.InputText.Length}");
            builder.Add($"IsFiltering: {@this.IsFiltering}]");
            if (@this is IModeledMarkdownContext mmdc)
            {
                builder.Add($"[Net: {(mmdc.ObservableNetProjection is IList list ? list.Count : "null")}");
            }
            builder.Add($"CC: {@this.CanonicalCount}");
            builder.Add($"PMC: {@this.PredicateMatchCount}]");
            builder.Add($"[{@this.QueryFilterConfig}: {@this.SearchEntryState.ToFullKey()}");
            builder.Add($"{@this.FilteringState.ToFullKey()}]");
            return string.Join(", ", builder);
        }

        public static string TopologyReport(this IModeledMarkdownContext @this)
        {
            var builder = new List<string>();
            builder.Add($"{@this.ProjectionTopology.ToFullKey()}");
            builder.Add($"{@this.ReplaceItemsEventingOptions.ToFullKey()}");
            return string.Join(", ", builder);
        }

        public static ModelPreviewDelegate GetModelPreviewDlgt<T>(this object? _)
        {
            var type = typeof(T);
            if(typeof(SelectableQFModel).IsAssignableFrom(type))
            {
                return (item)=>((SelectableQFModel?)item)?.Description?.PadRight(10).Substring(0, 10) ?? "Not Found";
            }
            else
            {
                throw new NotSupportedException($"No delegat is registered for {type.Name}");
            }
        }        
    }
}
