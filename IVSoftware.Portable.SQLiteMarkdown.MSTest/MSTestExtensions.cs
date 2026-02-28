using Newtonsoft.Json;
using SQLite;
using System.Diagnostics;
using System.Xml.Linq;

namespace IVSoftware.Portable.SQLiteMarkdown.MSTest
{
    [Flags]
    public enum PopulateOptions
    {
        RandomChecks = 0x1,
    }
    public static partial class SQLiteMarkdownTestExtensions
    {
        public static T DequeueSingle<T>(this Queue<T> queue)
            => queue.Count switch
            {
                0 => throw new InvalidOperationException("Queue is empty."),
                1 => queue.Dequeue(),
                _ => throw new InvalidOperationException("Multiple items in queue."),
            };

        public static void PopulateDemoDatabase<TItem>(this SQLiteConnection @this, bool includeLive = false, PopulateOptions? options = null) 
            where TItem : new()
        {
            @this.CreateTable<TItem>();

            var list = new List<TItem>().PopulateForDemo<List<TItem>,TItem>(includeLiveDemo: includeLive, options);
            @this.InsertAll(list);
        }
        public static TList PopulateForDemo<TList, TItem>(this TList @this, bool includeLiveDemo = false, PopulateOptions? options = null)
            where TList : IList<TItem>, new()
            where TItem : new()
        {
            Random rando = new(10);

            if (@this is null)
            {
                @this = Activator.CreateInstance<TList>();
            }
            else
            {
                @this.Clear();
            }

            void Add(string description, string tags, bool isChecked, List<string>? keywords = null)
            {
                if(options?.HasFlag(PopulateOptions.RandomChecks) == true)
                {
                    isChecked = rando.Next(2) == 1;
                }
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

            if (includeLiveDemo)
            {
                // Live-demo specific.
                Add("Appetizer Plate", "[dish]", false, new() { "starter", "appealing", "snack" });
                Add("Errata", "[notes]", false, new() { "crunchy", "green", "appended" });
                Add("Happy Camper", "[phrase]", false, new() { "joyful", "camp", "approach-west" });
                Add("Great example - Markdown Demo", "[app] [portable]", false, new() { "digital", "mobile", "software" });
                Add("Application Form", "[document]", false, new() { "paperwork", "apply" });
                Add("App Store", "[app]", false, new() { "digital", "mobile", "software" });
            }
            return @this;
        }
        public static TList PopulateForDemo<TList,TItem>(this TList @this, int count, PopulateOptions? options = null)
            where TList : IList<TItem>, new()
            where TItem : new()
        {
            Random rando = new(10);
            if (@this is null)
            {
                @this = Activator.CreateInstance<TList>();
            }
            else
            {
                @this.Clear();
            }

            for (int i = 1; i <= count; i++)
            {
                Add(
                    description: $"Item{i:d2}", 
                    tags: string.Empty,
                    isChecked: options?.HasFlag(PopulateOptions.RandomChecks) == true && rando.Next(2) == 1);
            }

            void Add(string description, string tags, bool isChecked, List<string>? keywords = null)
            {
                var instance = new T();
                typeof(T).GetProperty("Description")?.SetValue(instance, description);
                typeof(T).GetProperty("Tags")?.SetValue(instance, tags);
                typeof(T).GetProperty("IsChecked")?.SetValue(instance, isChecked);
                if (keywords != null)
                {
                    var json = JsonConvert.SerializeObject(keywords);
                    typeof(T).GetProperty("Keywords")?.SetValue(instance, json);
                }
                @this.Add(instance);
            }
            return @this;
        }

        /// <summary>
        /// Supports await Unk with a timeout for deterministic fail on test instead of hanging. 
        /// </summary>
        /// <remarks>
        /// There is a Task.WaitAsync(TimeSpan) but that’s an instance method 
        /// on Task; an extension on object, overload resolution won’t confuse them.
        /// </remarks>
        public static async Task<object?> WaitAsync(this object awaitable, TimeSpan timeout)
        {
            if (awaitable is null)
                throw new ArgumentNullException(nameof(awaitable));

            Task<object?> ToTask()
            {
                var type = awaitable.GetType();
                var getAwaiter = type.GetMethod("GetAwaiter", Type.EmptyTypes)
                    ?? throw new InvalidOperationException("Object is not awaitable.");

                var awaiter = getAwaiter.Invoke(awaitable, null)
                    ?? throw new InvalidOperationException("GetAwaiter returned null.");

                var awaiterType = awaiter.GetType();

                var isCompletedProp = awaiterType.GetProperty("IsCompleted")
                    ?? throw new InvalidOperationException("Awaiter missing IsCompleted.");

                var onCompleted = awaiterType.GetMethod("OnCompleted", new[] { typeof(Action) })
                    ?? throw new InvalidOperationException("Awaiter missing OnCompleted.");

                var getResult = awaiterType.GetMethod("GetResult")
                    ?? throw new InvalidOperationException("Awaiter missing GetResult.");

                var tcs = new TaskCompletionSource<object?>();

                void Complete()
                {
                    try
                    {
                        var result = getResult.Invoke(awaiter, null);
                        tcs.TrySetResult(result);
                    }
                    catch (Exception ex)
                    {
                        tcs.TrySetException(ex.InnerException ?? ex);
                    }
                }

                if ((bool)isCompletedProp.GetValue(awaiter)!)
                {
                    Complete();
                }
                else
                {
                    onCompleted.Invoke(awaiter, new object[] { (Action)Complete });
                }

                return tcs.Task;
            }

            var settleTask = ToTask();

            var completed = await Task.WhenAny(
                settleTask,
                Task.Delay(timeout));

            if (!ReferenceEquals(settleTask, completed))
                throw new TimeoutException(
                    $"Awaitable did not complete within {timeout}.");

            return await settleTask;
        }
    }
}
