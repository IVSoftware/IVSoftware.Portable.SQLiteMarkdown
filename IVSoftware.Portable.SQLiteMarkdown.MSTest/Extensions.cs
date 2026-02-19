using Newtonsoft.Json;
using SQLite;
using System.Diagnostics;
using System.Reflection;
using System.Xml.Linq;

namespace IVSoftware.Portable.SQLiteMarkdown.MSTest
{
    public static class SQLiteMarkdownTestExtensions
    {
        public static T DequeueSingle<T>(this Queue<T> queue)
            => queue.Count switch
            {
                0 => throw new InvalidOperationException("Queue is empty."),
                1 => queue.Dequeue(),
                _ => throw new InvalidOperationException("Multiple items in queue."),
            };

        public static void PopulateDemoDatabase<T>(this SQLiteConnection @this) 
            where T : new()
        {
            PropertyInfo?
                piDescription = typeof(T).GetProperty("Description"),
                piTags = typeof(T).GetProperty("Tags"),
                piIsChecked = typeof(T).GetProperty("IsChecked");

            @this.CreateTable<T>();

            var list = new List<T>();

            void Add(string description, string tags, bool isChecked, List<string>? keywords = null)
            {
                var instance = new T();
                piDescription?.SetValue(instance, description);
                piTags?.SetValue(instance, tags);
                piIsChecked?.SetValue(instance, isChecked);
                if (keywords != null)
                {
                    var json = JsonConvert.SerializeObject(keywords);
                    typeof(T).GetProperty("Keywords")?.SetValue(instance, json);
                }
                list.Add(instance);
            }
            // Unit tested
            Add("Brown Dog", "[canine] [color]", false, new() { "loyal", "friend", "furry" });
            Add("Green Apple", "[fruit] [color]", false, new() { "tart", "snack", "healthy" });
            Add("Yellow Banana", "[fruit] [color]", false);
            Add("Blue Bird", "[bird] [color]", false, new() { "sky", "feathered", "song" });
            Add("Red Cherry", "[fruit] [color]", false, new() { "sweet", "summer", "dessert" });
            Add("Black Cat", "[animal] [color]", false);
            Add("Orange Fox", "[animal] [color]", false);
            Add("White Rabbit", "[animal] [color]", false, new() { "bunny", "soft", "jump" });
            Add("Purple Grape", "[fruit] [color]", false);
            Add("Gray Wolf", "[animal] [color]", false, new() { "pack", "howl", "wild" });
            Add("Pink Flamingo", "[bird] [color]", false);
            Add("Golden Lion", "[animal] [color]", false);
            Add("Brown Bear", "[animal] [color]", false, new() { "strong", "wild", "forest" });
            Add("Green Pear", "[fruit] [color]", false);
            Add("Red Strawberry", "[fruit] [color]", false);
            Add("Black Panther", "[animal] [color]", false, new() { "stealthy", "feline", "night" });
            Add("Yellow Lemon", "[fruit] [color]", false);
            Add("White Swan", "[bird] [color]", false);
            Add("Purple Plum", "[fruit] [color]", false);
            Add("Blue Whale", "[marine-mammal] [ocean]", false, new() { "ocean", "mammal", "giant" });
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
            // Live-demo specific.
            Add("Appetizer Plate", "[dish]", false, new() { "starter", "appealing", "snack" });
            Add("Errata", "[notes]", false, new() { "crunchy", "green", "appended" });
            Add("Happy Camper", "[phrase]", false, new() { "joyful", "camp", "approach-west" });
            Add("Great example - Markdown Demo", "[app] [portable]", false, new() { "digital", "mobile", "software" });
            Add("Application Form", "[document]", false, new() { "paperwork", "apply" });
            Add("App Store", "[app]", false, new() { "digital", "mobile", "software" });


            @this.InsertAll(list);
        }
    }
}
