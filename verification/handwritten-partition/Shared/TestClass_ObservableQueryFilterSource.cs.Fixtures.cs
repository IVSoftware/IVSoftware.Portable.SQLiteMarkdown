// Handwritten verification view; source provenance is in the adjacent manifest.
using IVSoftware.Portable.Collections;
using IVSoftware.Portable.Collections.Events;
using IVSoftware.Portable.Collections.Internal;
using IVSoftware.Portable.Common.Attributes;
using IVSoftware.Portable.Common.Exceptions;
using IVSoftware.Portable.Disposable;
using IVSoftware.Portable.SQLiteMarkdown.Collections;
using IVSoftware.Portable.SQLiteMarkdown.Common;
using IVSoftware.Portable.SQLiteMarkdown.Events;
using IVSoftware.Portable.SQLiteMarkdown.Internal;
using IVSoftware.Portable.SQLiteMarkdown.MSTest.Models;
using IVSoftware.Portable.SQLiteMarkdown.MSTest.Models.DemoDB;
using IVSoftware.Portable.SQLiteMarkdown.MSTest.Models.QFTemplates;
using IVSoftware.Portable.Xml.Linq.XBoundObject;
using IVSoftware.Portable.Xml.Linq.XBoundObject.Modeling;
using IVSoftware.WinOS.MSTest.Extensions;
using Newtonsoft.Json;
using SQLite;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Transactions;
using System.Xml.Linq;
using IgnoreAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.IgnoreAttribute;

namespace IVSoftware.Portable.SQLiteMarkdown.MSTest
{
[TestClass]
public partial class TestClass_ObservableQueryFilterSource
{
private SQLiteConnection InitializeInMemoryDatabaseSingle()
        {
            var items = new List<SelectableQFModelLTOQO>
            {
                new SelectableQFModelLTOQO
                {
                    Description = "Brown Dog",
                    Tags = "[canine][color][atomic tag]",
                    IsChecked = false,
                    Keywords = JsonConvert.SerializeObject(new List<string> { "loyal", "friend", "furry" })
                },
            };
            var imdb = new SQLiteConnection(":memory:");
            imdb.CreateTable<SelectableQFModelLTOQO>();
            imdb.InsertAll(items);
            return imdb;
        }
private SQLiteConnection InitializeInMemoryDatabase()
        {
            var items = new List<SelectableQFModelLTOQO>
            {
                new SelectableQFModelLTOQO
                {
                    Description = "Brown Dog",
                    Tags = "[canine][color]",
                    IsChecked = false,
                    Keywords = JsonConvert.SerializeObject(new List<string> { "loyal", "friend", "furry" })
                },
                new SelectableQFModelLTOQO
                {
                    Description = "Green Apple",
                    Tags = "[fruit][color]",
                    IsChecked = false,
                    Keywords = JsonConvert.SerializeObject(new List<string> { "tart", "snack", "healthy" })
                },
                new SelectableQFModelLTOQO { Description = "Yellow Banana", Tags = "[fruit][color]", IsChecked = false },
                new SelectableQFModelLTOQO
                {
                    Description = "Blue Bird",
                    Tags = "[bird][color]",
                    IsChecked = false,
                    Keywords = JsonConvert.SerializeObject(new List<string> { "sky", "feathered", "song" })
                },
                new SelectableQFModelLTOQO
                {
                    Description = "Red Cherry",
                    Tags = "[fruit][color]",
                    IsChecked = false,
                    Keywords = JsonConvert.SerializeObject(new List<string> { "sweet", "summer", "dessert" })
                },
                new SelectableQFModelLTOQO { Description = "Black Cat", Tags = "[animal][color]", IsChecked = false },
                new SelectableQFModelLTOQO { Description = "Orange Fox", Tags = "[animal][color]", IsChecked = false },
                new SelectableQFModelLTOQO
                {
                    Description = "White Rabbit",
                    Tags = "[animal][color]",
                    IsChecked = false,
                    Keywords = JsonConvert.SerializeObject(new List<string> { "bunny", "soft", "jump" })
                },
                new SelectableQFModelLTOQO { Description = "Purple Grape", Tags = "[fruit][color]", IsChecked = false },
                new SelectableQFModelLTOQO
                {
                    Description = "Gray Wolf",
                    Tags = "[animal][color]",
                    IsChecked = false,
                    Keywords = JsonConvert.SerializeObject(new List<string> { "pack", "howl", "wild" })
                },
                new SelectableQFModelLTOQO { Description = "Pink Flamingo", Tags = "[bird][color]", IsChecked = false },
                new SelectableQFModelLTOQO { Description = "Golden Lion", Tags = "[animal][color]", IsChecked = false },
                new SelectableQFModelLTOQO
                {
                    Description = "Brown Bear",
                    Tags = "[animal][color]",
                    IsChecked = false,
                    Keywords = JsonConvert.SerializeObject(new List<string> { "strong", "wild", "forest" })
                },
                new SelectableQFModelLTOQO { Description = "Green Pear", Tags = "[fruit][color]", IsChecked = false },
                new SelectableQFModelLTOQO { Description = "Red Strawberry", Tags = "[fruit][color]", IsChecked = false },
                new SelectableQFModelLTOQO
                {
                    Description = "Black Panther",
                    Tags = "[animal][color]",
                    IsChecked = false,
                    Keywords = JsonConvert.SerializeObject(new List<string> { "stealthy", "feline", "night" })
                },
                new SelectableQFModelLTOQO { Description = "Yellow Lemon", Tags = "[fruit][color]", IsChecked = false },
                new SelectableQFModelLTOQO { Description = "White Swan", Tags = "[bird][color]", IsChecked = false },
                new SelectableQFModelLTOQO { Description = "Purple Plum", Tags = "[fruit][color]", IsChecked = false },
                new SelectableQFModelLTOQO
                {
                    Description = "Blue Whale",
                    Tags = "[marine-mammal][ocean]",
                    IsChecked = false,
                    Keywords = JsonConvert.SerializeObject(new List<string> { "ocean", "mammal", "giant" })
                },
                new SelectableQFModelLTOQO
                {
                    Description = "Elephant",
                    Tags = "[animal]",
                    IsChecked = false,
                    Keywords = JsonConvert.SerializeObject(new List<string> { "trunk", "herd", "safari" })
                },
                new SelectableQFModelLTOQO { Description = "Pineapple", Tags = "[fruit]", IsChecked = false },
                new SelectableQFModelLTOQO { Description = "Shark", Tags = "[fish]", IsChecked = false },
                new SelectableQFModelLTOQO { Description = "Owl", Tags = "[bird]", IsChecked = false },
                new SelectableQFModelLTOQO { Description = "Giraffe", Tags = "[animal]", IsChecked = false },
                new SelectableQFModelLTOQO { Description = "Coconut", Tags = "[fruit]", IsChecked = false },
                new SelectableQFModelLTOQO
                {
                    Description = "Kangaroo",
                    Tags = "[animal]",
                    IsChecked = false,
                    Keywords = JsonConvert.SerializeObject(new List<string> { "bounce", "outback", "marsupial" })
                },
                new SelectableQFModelLTOQO { Description = "Dragonfruit", Tags = "[fruit]", IsChecked = false },
                new SelectableQFModelLTOQO { Description = "Turtle", Tags = "[animal]", IsChecked = false },
                new SelectableQFModelLTOQO { Description = "Mango", Tags = "[fruit]", IsChecked = false },
                new SelectableQFModelLTOQO { Description = "Should NOT match an expression with an \"animal\" tag.", Tags = "[not animal]", IsChecked = false }
            };
            var imdb = new SQLiteConnection(":memory:");
            imdb.CreateTable<SelectableQFModelLTOQO>();
            imdb.InsertAll(items);
            return imdb;
        }
private SQLiteConnection InitializeInMemoryDatabase<T>() where T : new()
        {
            var imdb = new SQLiteConnection(":memory:");
            imdb.CreateTable<T>();

            var list = new List<T>();

            void Add(string description, string tags, bool isChecked, List<string> keywords = null)
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
                list.Add(instance);
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

            imdb.InsertAll(list);
            return imdb;
        }
private SQLiteConnection InitializeInMemoryDatabaseAtomicQuotes()
        {
            var items = new List<SelectableQFModelLTOQO>
            {
            };
            var imdb = new SQLiteConnection(":memory:");
            imdb.CreateTable<SelectableQFModelLTOQO>();
            imdb.InsertAll(items);
            return imdb;
        }
}
}
