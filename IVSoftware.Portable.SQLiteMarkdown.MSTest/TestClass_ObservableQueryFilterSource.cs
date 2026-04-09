using IVSoftware.Portable.Common.Collections;
using IVSoftware.Portable.Collections.Preview;
using IVSoftware.Portable.Common.Attributes;
using IVSoftware.Portable.Common.Exceptions;
using IVSoftware.Portable.Disposable;
using IVSoftware.Portable.SQLiteMarkdown.Collections;
using IVSoftware.Portable.SQLiteMarkdown.Common;
using IVSoftware.Portable.SQLiteMarkdown.Events;
using IVSoftware.Portable.SQLiteMarkdown.MSTest.DemoDB;
using IVSoftware.Portable.SQLiteMarkdown.MSTest.Models;
using IVSoftware.Portable.SQLiteMarkdown.MSTest.Models.DemoDB;
using IVSoftware.Portable.SQLiteMarkdown.MSTest.Models.QFTemplates;
using IVSoftware.Portable.SQLiteMarkdown.Util;
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
using System.Xml.Linq;
using IVSoftware.Portable.Xml.Linq.Collections;

namespace IVSoftware.Portable.SQLiteMarkdown.MSTest
{
    // Namespace with test-only classes.
    namespace DemoDB
    {
        public class NavSearchBar : INotifyPropertyChanged
        {
            public IList<SelectableQFModel>? ItemsSource
            {
                get => _itemsSource;
                set
                {
                    if (!Equals(_itemsSource, value))
                    {
                        INotifyCollectionChanged? incc = _itemsSource as INotifyCollectionChanged;
                        if (_itemsSource is not null)
                        {
                            if (incc is not null)
                            {
                                incc.CollectionChanged -= OnCollectionChanged;
                            }
                        }
                        _itemsSource = value;
                        if (_itemsSource is not null)
                        {
                            if (incc is not null)
                            {
                                incc.CollectionChanged += OnCollectionChanged;
                            }
                        }
                        INotifyPropertyChanged? inpc = _itemsSource as INotifyPropertyChanged;
                        if (_itemsSource is not null)
                        {
                            if (inpc is not null)
                            {
                                inpc.PropertyChanged -= OnPropertyChanged;
                            }
                        }
                        _itemsSource = value;
                        if (_itemsSource is not null)
                        {
                            if (inpc is not null)
                            {
                                inpc.PropertyChanged += OnPropertyChanged;
                            }
                        }
                        OnPropertyChanged();
                    }
                }
            }

            public string InputText
            {
                get
                {
                    if (ItemsSource is IObservableQueryFilterSource qfs)
                    {
                        return qfs.InputText;
                    }
                    else
                    {
                        return _inputText;
                    }
                }
                set
                {
                    if (ItemsSource is IObservableQueryFilterSource qfs)
                    {
                        qfs.InputText = value;
                    }
                    else
                    {
                        if (!Equals(_inputText, value))
                        {
                            _inputText = value;
                            OnPropertyChanged();
                        }
                    }
                }
            }
            string _inputText = string.Empty;

            protected virtual void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
            {
                throw new NotImplementedException();
            }

            IList<SelectableQFModel>? _itemsSource = null;
            protected void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
            OnPropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            protected virtual void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
            {
                PropertyChanged?.Invoke(sender, e);
                switch (e.PropertyName)
                {
                    case nameof(ItemsSource):
                        break;
                    case nameof(InputText):
                        break;
                    default:
                        break;
                }
            }
            public event PropertyChangedEventHandler? PropertyChanged;
        }
    }

    [TestClass]
    public class TestClass_ObservableQueryFilterSource
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

        [TestMethod]
        public void Test_ViewDatabase()
        {
            string actual, expected;
            using (var cnx = InitializeInMemoryDatabase())
            {
                var allRecords = cnx.Query<SelectableQFModelLTOQO>("Select * from items");
                actual = string.Join(Environment.NewLine, allRecords.Select(_ => _.ToString()));
                actual.ToClipboard();
                actual.ToClipboardExpected();
                { }
                expected = @" 
Brown Dog ""loyal"",""friend"",""furry"" [canine] [color]
Green Apple ""tart"",""snack"",""healthy"" [fruit] [color]
Yellow Banana  [fruit] [color]
Blue Bird ""sky"",""feathered"",""song"" [bird] [color]
Red Cherry ""sweet"",""summer"",""dessert"" [fruit] [color]
Black Cat  [animal] [color]
Orange Fox  [animal] [color]
White Rabbit ""bunny"",""soft"",""jump"" [animal] [color]
Purple Grape  [fruit] [color]
Gray Wolf ""pack"",""howl"",""wild"" [animal] [color]
Pink Flamingo  [bird] [color]
Golden Lion  [animal] [color]
Brown Bear ""strong"",""wild"",""forest"" [animal] [color]
Green Pear  [fruit] [color]
Red Strawberry  [fruit] [color]
Black Panther ""stealthy"",""feline"",""night"" [animal] [color]
Yellow Lemon  [fruit] [color]
White Swan  [bird] [color]
Purple Plum  [fruit] [color]
Blue Whale ""ocean"",""mammal"",""giant"" [marine-mammal] [ocean]
Elephant ""trunk"",""herd"",""safari"" [animal]
Pineapple  [fruit]
Shark  [fish]
Owl  [bird]
Giraffe  [animal]
Coconut  [fruit]
Kangaroo ""bounce"",""outback"",""marsupial"" [animal]
Dragonfruit  [fruit]
Turtle  [animal]
Mango  [fruit]
Should NOT match an expression with an ""animal"" tag.  [not animal]";

                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting values to match."
                );
            }
        }

        [TestMethod]
        public async Task Test_ApplyExprManuallySingle()
        {
            string actual, expected;
            List<SelectableQFModelLTOQO> results;
            var builder = new List<string>();
            List<SelectableQFModelLTOQO> unfiltered = new List<SelectableQFModelLTOQO>();
            List<SelectableQFModelLTOQO> filtered = new List<SelectableQFModelLTOQO>();
            MarkdownContextOR context;
            ValidationState state;
            using (var cnx = InitializeInMemoryDatabaseSingle())
            {
                subtestExpr();
                await subtestReport();
                subtestMoreExprs();
                subtestApplyFilter();


                #region S U B T E S T S
                void subtestExpr()
                {
                    actual = "animal".ParseSqlMarkdown<SelectableQFModelLTOQO>();
                    expected = @" 
SELECT * FROM items WHERE (QueryTerm LIKE '%animal%')"
                    ;

                    Assert.AreEqual(
                        expected.NormalizeResult(),
                        actual.NormalizeResult(),
                        "Expecting query mode expression with QueryTerm indexed only."
                    );

                    actual = "animal".ParseSqlMarkdown<SelectableQFModelLTOQO>(QueryFilterMode.Filter);
                    expected = @" 
SELECT * FROM items WHERE (FilterTerm LIKE '%animal%')"
                    ;

                    Assert.AreEqual(
                        expected.NormalizeResult(),
                        actual.NormalizeResult(),
                        "Expecting filter mode expression with FilterTerm indexed only."
                    );

                }
                async Task subtestReport()
                {
                    unfiltered = cnx.Query<SelectableQFModelLTOQO>("Select * from items");
                    await Task.Delay(TimeSpan.FromSeconds(0.5));
                    foreach (var item in unfiltered)
                    {
                        builder.Add(item.Report());
                        builder.Add(string.Empty);
                    }

                    var joined = string.Join(Environment.NewLine, builder);

                    actual = joined;
                    actual.ToClipboardExpected();
                    { }
                    expected = @" 
Description    =""Brown Dog""
Keywords       =""[""loyal"",""friend"",""furry""]""
KeywordsDisplay=""""loyal"",""friend"",""furry""""
Tags           =""[canine] [color] [atomic tag]""
TagsDisplay    =""[canine] [color] [atomic tag]""
IsChecked      =""False""
Selection      =""None""
IsEditing      =""False""
QueryTerm      =""brown~dog~loyal~friend~furry~[canine]~[color]~[atomic tag]""
FilterTerm     =""brown~dog~loyal~friend~furry""
TagMatchTerm   =""[canine] [color] [atomic tag]""
Properties     =""{
  ""Description"": ""Brown Dog"",
  ""Tags"": ""[canine] [color] [atomic tag]"",
  ""Keywords"": ""[\""loyal\"",\""friend\"",\""furry\""]""
}""
"
                    ;

                    Assert.AreEqual(
                        expected.NormalizeResult(),
                        actual.NormalizeResult(),
                        "Expecting report to match"
                    );
                }
                // "Might" need await...
                void subtestMoreExprs()
                {
                    actual = "bro".ParseSqlMarkdown<SelectableQFModelLTOQO>();
                    expected = @" 
SELECT * FROM items WHERE (QueryTerm LIKE '%bro%')"
                    ;

                    Assert.AreEqual(
                        expected.NormalizeResult(),
                        actual.NormalizeResult(),
                        "Expecting correct expr"
                    );
                    Assert.IsNotNull(cnx.Query<SelectableQFModelLTOQO>(actual).SingleOrDefault());

                    actual = "bro dog".ParseSqlMarkdown<SelectableQFModelLTOQO>();
                    Assert.IsNotNull(cnx.Query<SelectableQFModelLTOQO>(actual).SingleOrDefault());

                    actual = "brown furry".ParseSqlMarkdown<SelectableQFModelLTOQO>();
                    expected = @" 
SELECT * FROM items WHERE (QueryTerm LIKE '%brown%') AND (QueryTerm LIKE '%furry%')"
                    ;
                    Assert.IsNotNull(cnx.Query<SelectableQFModelLTOQO>(actual).SingleOrDefault());
                    actual = "brown !dog".ParseSqlMarkdown<SelectableQFModelLTOQO>();

                    actual.ToClipboardExpected();
                    // IS null
                    Assert.IsNull(cnx.Query<SelectableQFModelLTOQO>(actual).SingleOrDefault());
                }
                void subtestApplyFilter()
                {
                    using (var subcnx = new SQLiteConnection(":memory:"))
                    {
                        subcnx.CreateTable<SelectableQFModelLTOQO>();
                        subcnx.InsertAll(unfiltered);
                        builder.Clear();

                        actual = "brown furry".ParseSqlMarkdown<SelectableQFModelLTOQO>(QueryFilterMode.Filter);
                        actual.ToClipboardExpected();
                        { }
                        expected = @" 
SELECT * FROM items WHERE
(FilterTerm LIKE '%brown%') AND (FilterTerm LIKE '%furry%')"
                        ;

                        Assert.AreEqual(
                            expected.NormalizeResult(),
                            actual.NormalizeResult(),
                            "Expecting correct expr."
                        );
                        filtered = subcnx.Query<SelectableQFModelLTOQO>(actual);

                        Assert.IsNotNull(filtered.SingleOrDefault());
                        actual = "brown canine".ParseSqlMarkdown<SelectableQFModelLTOQO>(QueryFilterMode.Filter);
                        filtered = subcnx.Query<SelectableQFModelLTOQO>(actual);
                        // IS null...
                        Assert.IsNull(filtered.SingleOrDefault());
                    }
                }
#if ABSTRACT
            
                expected = @" 
Description    =""Brown Dog""
Keywords       =""[""loyal"",""friend"",""furry""]""
KeywordsDisplay=""""loyal"",""friend"",""furry""""
Tags           =""[canine][color]""
TagsDisplay    =""[canine][color]""
IsChecked      =""False""
FilterValue    =""Brown Dog""
Selection      =""None""
LikeTerm       =""brown~dog~loyal~friend~furry~canine~color""
ContainsTerm   =""brown~dog~loyal~friend~furry""
TagMatchTerm   =""canine~color""
Properties     =""{
  ""Description"": ""Brown Dog"",
  ""Tags"": ""[canine][color]"",
  ""Keywords"": ""[\""loyal\"",\""friend\"",\""furry\""]""
}""
"
#endif
                #endregion S U B T E S T S
            }
        }
        /// <summary>
        /// End-to-end verification of markdown parsing, literal quote handling,
        /// SQL generation, database execution, and JSON serialization consistency.
        /// </summary>
        /// <remarks>
        /// Exercises three boundaries:
        /// 1. SQL generation from a user-escaped input (animal\").
        /// 2. Stable term construction during indexing.
        /// 3. Correct round-trip JSON escaping of literal quote characters.
        /// 
        /// Confirms that literal quotes are preserved as data while remaining
        /// syntactically valid in generated SQL and serialized output.
        /// </remarks>
        [TestMethod]
        public async Task Test_ApplyExprManually()
        {
            string actual, expected, sql;
            List<SelectableQFModelLTOQO> allRecords;
            SelectableQFModelLTOQO
                testLiteralQuotes,
                testLiteralQuery;

            #region P R O L O G U E 
            var mdc = new ModeledMarkdownContext<SelectableQFModelLTOQO>();
            mdc.InputText = @"animal\""";
            await mdc;
            sql = mdc.ParseSqlMarkdown();
            actual = sql;
            actual.ToClipboardExpected();
            { }
            expected = @" 
SELECT * FROM items WHERE 
(QueryTerm LIKE '%animal""%')";

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting valid sql query with escaped single quote."
            );
            #endregion P R O L O G U E

            var builder = new List<string>();
            using (var cnx = InitializeInMemoryDatabase())
            {
                allRecords = cnx.Query<SelectableQFModelLTOQO>("Select * from items");
                foreach (var item in allRecords)
                {
                    builder.Add(item.Report());
                    builder.Add(string.Empty);
                }
                testLiteralQuotes =
                    cnx
                    .Query<SelectableQFModelLTOQO>(
                    "Select * from items where Description LIKE 'Should NOT match an expression with%'")
                    .Single();
                testLiteralQuery =
                    cnx
                    .Query<SelectableQFModelLTOQO>(sql)
                    .Single();
            }
            subtestReport();
            subtest_JsonQuoteRendering();
            subtest_EscapedQuery();


            #region S U B T E S T S
            /// <summary>
            /// Verifies deterministic report rendering across all records,
            /// ensuring term generation and literal quote preservation are stable.
            /// </summary>
            void subtestReport()
            {

                var joined = string.Join(Environment.NewLine, builder);

                actual = joined;
                actual.ToClipboardExpected();
                { }
                expected = @" 
Description    =""Brown Dog""
Keywords       =""[""loyal"",""friend"",""furry""]""
KeywordsDisplay=""""loyal"",""friend"",""furry""""
Tags           =""[canine] [color]""
TagsDisplay    =""[canine] [color]""
IsChecked      =""False""
Selection      =""None""
IsEditing      =""False""
QueryTerm      =""brown~dog~loyal~friend~furry~[canine]~[color]""
FilterTerm     =""brown~dog~loyal~friend~furry""
TagMatchTerm   =""[canine] [color]""
Properties     =""{
  ""Description"": ""Brown Dog"",
  ""Tags"": ""[canine] [color]"",
  ""Keywords"": ""[\""loyal\"",\""friend\"",\""furry\""]""
}""

Description    =""Green Apple""
Keywords       =""[""tart"",""snack"",""healthy""]""
KeywordsDisplay=""""tart"",""snack"",""healthy""""
Tags           =""[fruit] [color]""
TagsDisplay    =""[fruit] [color]""
IsChecked      =""False""
Selection      =""None""
IsEditing      =""False""
QueryTerm      =""green~apple~tart~snack~healthy~[fruit]~[color]""
FilterTerm     =""green~apple~tart~snack~healthy""
TagMatchTerm   =""[fruit] [color]""
Properties     =""{
  ""Description"": ""Green Apple"",
  ""Tags"": ""[fruit] [color]"",
  ""Keywords"": ""[\""tart\"",\""snack\"",\""healthy\""]""
}""

Description    =""Yellow Banana""
Keywords       =""[]""
KeywordsDisplay=""""
Tags           =""[fruit] [color]""
TagsDisplay    =""[fruit] [color]""
IsChecked      =""False""
Selection      =""None""
IsEditing      =""False""
QueryTerm      =""yellow~banana~[fruit]~[color]""
FilterTerm     =""yellow~banana""
TagMatchTerm   =""[fruit] [color]""
Properties     =""{
  ""Description"": ""Yellow Banana"",
  ""Tags"": ""[fruit] [color]""
}""

Description    =""Blue Bird""
Keywords       =""[""sky"",""feathered"",""song""]""
KeywordsDisplay=""""sky"",""feathered"",""song""""
Tags           =""[bird] [color]""
TagsDisplay    =""[bird] [color]""
IsChecked      =""False""
Selection      =""None""
IsEditing      =""False""
QueryTerm      =""blue~bird~sky~feathered~song~[bird]~[color]""
FilterTerm     =""blue~bird~sky~feathered~song""
TagMatchTerm   =""[bird] [color]""
Properties     =""{
  ""Description"": ""Blue Bird"",
  ""Tags"": ""[bird] [color]"",
  ""Keywords"": ""[\""sky\"",\""feathered\"",\""song\""]""
}""

Description    =""Red Cherry""
Keywords       =""[""sweet"",""summer"",""dessert""]""
KeywordsDisplay=""""sweet"",""summer"",""dessert""""
Tags           =""[fruit] [color]""
TagsDisplay    =""[fruit] [color]""
IsChecked      =""False""
Selection      =""None""
IsEditing      =""False""
QueryTerm      =""red~cherry~sweet~summer~dessert~[fruit]~[color]""
FilterTerm     =""red~cherry~sweet~summer~dessert""
TagMatchTerm   =""[fruit] [color]""
Properties     =""{
  ""Description"": ""Red Cherry"",
  ""Tags"": ""[fruit] [color]"",
  ""Keywords"": ""[\""sweet\"",\""summer\"",\""dessert\""]""
}""

Description    =""Black Cat""
Keywords       =""[]""
KeywordsDisplay=""""
Tags           =""[animal] [color]""
TagsDisplay    =""[animal] [color]""
IsChecked      =""False""
Selection      =""None""
IsEditing      =""False""
QueryTerm      =""black~cat~[animal]~[color]""
FilterTerm     =""black~cat""
TagMatchTerm   =""[animal] [color]""
Properties     =""{
  ""Description"": ""Black Cat"",
  ""Tags"": ""[animal] [color]""
}""

Description    =""Orange Fox""
Keywords       =""[]""
KeywordsDisplay=""""
Tags           =""[animal] [color]""
TagsDisplay    =""[animal] [color]""
IsChecked      =""False""
Selection      =""None""
IsEditing      =""False""
QueryTerm      =""orange~fox~[animal]~[color]""
FilterTerm     =""orange~fox""
TagMatchTerm   =""[animal] [color]""
Properties     =""{
  ""Description"": ""Orange Fox"",
  ""Tags"": ""[animal] [color]""
}""

Description    =""White Rabbit""
Keywords       =""[""bunny"",""soft"",""jump""]""
KeywordsDisplay=""""bunny"",""soft"",""jump""""
Tags           =""[animal] [color]""
TagsDisplay    =""[animal] [color]""
IsChecked      =""False""
Selection      =""None""
IsEditing      =""False""
QueryTerm      =""white~rabbit~bunny~soft~jump~[animal]~[color]""
FilterTerm     =""white~rabbit~bunny~soft~jump""
TagMatchTerm   =""[animal] [color]""
Properties     =""{
  ""Description"": ""White Rabbit"",
  ""Tags"": ""[animal] [color]"",
  ""Keywords"": ""[\""bunny\"",\""soft\"",\""jump\""]""
}""

Description    =""Purple Grape""
Keywords       =""[]""
KeywordsDisplay=""""
Tags           =""[fruit] [color]""
TagsDisplay    =""[fruit] [color]""
IsChecked      =""False""
Selection      =""None""
IsEditing      =""False""
QueryTerm      =""purple~grape~[fruit]~[color]""
FilterTerm     =""purple~grape""
TagMatchTerm   =""[fruit] [color]""
Properties     =""{
  ""Description"": ""Purple Grape"",
  ""Tags"": ""[fruit] [color]""
}""

Description    =""Gray Wolf""
Keywords       =""[""pack"",""howl"",""wild""]""
KeywordsDisplay=""""pack"",""howl"",""wild""""
Tags           =""[animal] [color]""
TagsDisplay    =""[animal] [color]""
IsChecked      =""False""
Selection      =""None""
IsEditing      =""False""
QueryTerm      =""gray~wolf~pack~howl~wild~[animal]~[color]""
FilterTerm     =""gray~wolf~pack~howl~wild""
TagMatchTerm   =""[animal] [color]""
Properties     =""{
  ""Description"": ""Gray Wolf"",
  ""Tags"": ""[animal] [color]"",
  ""Keywords"": ""[\""pack\"",\""howl\"",\""wild\""]""
}""

Description    =""Pink Flamingo""
Keywords       =""[]""
KeywordsDisplay=""""
Tags           =""[bird] [color]""
TagsDisplay    =""[bird] [color]""
IsChecked      =""False""
Selection      =""None""
IsEditing      =""False""
QueryTerm      =""pink~flamingo~[bird]~[color]""
FilterTerm     =""pink~flamingo""
TagMatchTerm   =""[bird] [color]""
Properties     =""{
  ""Description"": ""Pink Flamingo"",
  ""Tags"": ""[bird] [color]""
}""

Description    =""Golden Lion""
Keywords       =""[]""
KeywordsDisplay=""""
Tags           =""[animal] [color]""
TagsDisplay    =""[animal] [color]""
IsChecked      =""False""
Selection      =""None""
IsEditing      =""False""
QueryTerm      =""golden~lion~[animal]~[color]""
FilterTerm     =""golden~lion""
TagMatchTerm   =""[animal] [color]""
Properties     =""{
  ""Description"": ""Golden Lion"",
  ""Tags"": ""[animal] [color]""
}""

Description    =""Brown Bear""
Keywords       =""[""strong"",""wild"",""forest""]""
KeywordsDisplay=""""strong"",""wild"",""forest""""
Tags           =""[animal] [color]""
TagsDisplay    =""[animal] [color]""
IsChecked      =""False""
Selection      =""None""
IsEditing      =""False""
QueryTerm      =""brown~bear~strong~wild~forest~[animal]~[color]""
FilterTerm     =""brown~bear~strong~wild~forest""
TagMatchTerm   =""[animal] [color]""
Properties     =""{
  ""Description"": ""Brown Bear"",
  ""Tags"": ""[animal] [color]"",
  ""Keywords"": ""[\""strong\"",\""wild\"",\""forest\""]""
}""

Description    =""Green Pear""
Keywords       =""[]""
KeywordsDisplay=""""
Tags           =""[fruit] [color]""
TagsDisplay    =""[fruit] [color]""
IsChecked      =""False""
Selection      =""None""
IsEditing      =""False""
QueryTerm      =""green~pear~[fruit]~[color]""
FilterTerm     =""green~pear""
TagMatchTerm   =""[fruit] [color]""
Properties     =""{
  ""Description"": ""Green Pear"",
  ""Tags"": ""[fruit] [color]""
}""

Description    =""Red Strawberry""
Keywords       =""[]""
KeywordsDisplay=""""
Tags           =""[fruit] [color]""
TagsDisplay    =""[fruit] [color]""
IsChecked      =""False""
Selection      =""None""
IsEditing      =""False""
QueryTerm      =""red~strawberry~[fruit]~[color]""
FilterTerm     =""red~strawberry""
TagMatchTerm   =""[fruit] [color]""
Properties     =""{
  ""Description"": ""Red Strawberry"",
  ""Tags"": ""[fruit] [color]""
}""

Description    =""Black Panther""
Keywords       =""[""stealthy"",""feline"",""night""]""
KeywordsDisplay=""""stealthy"",""feline"",""night""""
Tags           =""[animal] [color]""
TagsDisplay    =""[animal] [color]""
IsChecked      =""False""
Selection      =""None""
IsEditing      =""False""
QueryTerm      =""black~panther~stealthy~feline~night~[animal]~[color]""
FilterTerm     =""black~panther~stealthy~feline~night""
TagMatchTerm   =""[animal] [color]""
Properties     =""{
  ""Description"": ""Black Panther"",
  ""Tags"": ""[animal] [color]"",
  ""Keywords"": ""[\""stealthy\"",\""feline\"",\""night\""]""
}""

Description    =""Yellow Lemon""
Keywords       =""[]""
KeywordsDisplay=""""
Tags           =""[fruit] [color]""
TagsDisplay    =""[fruit] [color]""
IsChecked      =""False""
Selection      =""None""
IsEditing      =""False""
QueryTerm      =""yellow~lemon~[fruit]~[color]""
FilterTerm     =""yellow~lemon""
TagMatchTerm   =""[fruit] [color]""
Properties     =""{
  ""Description"": ""Yellow Lemon"",
  ""Tags"": ""[fruit] [color]""
}""

Description    =""White Swan""
Keywords       =""[]""
KeywordsDisplay=""""
Tags           =""[bird] [color]""
TagsDisplay    =""[bird] [color]""
IsChecked      =""False""
Selection      =""None""
IsEditing      =""False""
QueryTerm      =""white~swan~[bird]~[color]""
FilterTerm     =""white~swan""
TagMatchTerm   =""[bird] [color]""
Properties     =""{
  ""Description"": ""White Swan"",
  ""Tags"": ""[bird] [color]""
}""

Description    =""Purple Plum""
Keywords       =""[]""
KeywordsDisplay=""""
Tags           =""[fruit] [color]""
TagsDisplay    =""[fruit] [color]""
IsChecked      =""False""
Selection      =""None""
IsEditing      =""False""
QueryTerm      =""purple~plum~[fruit]~[color]""
FilterTerm     =""purple~plum""
TagMatchTerm   =""[fruit] [color]""
Properties     =""{
  ""Description"": ""Purple Plum"",
  ""Tags"": ""[fruit] [color]""
}""

Description    =""Blue Whale""
Keywords       =""[""ocean"",""mammal"",""giant""]""
KeywordsDisplay=""""ocean"",""mammal"",""giant""""
Tags           =""[marine-mammal] [ocean]""
TagsDisplay    =""[marine-mammal] [ocean]""
IsChecked      =""False""
Selection      =""None""
IsEditing      =""False""
QueryTerm      =""blue~whale~ocean~mammal~giant~[marine-mammal]~[ocean]""
FilterTerm     =""blue~whale~ocean~mammal~giant""
TagMatchTerm   =""[marine-mammal] [ocean]""
Properties     =""{
  ""Description"": ""Blue Whale"",
  ""Tags"": ""[marine-mammal] [ocean]"",
  ""Keywords"": ""[\""ocean\"",\""mammal\"",\""giant\""]""
}""

Description    =""Elephant""
Keywords       =""[""trunk"",""herd"",""safari""]""
KeywordsDisplay=""""trunk"",""herd"",""safari""""
Tags           =""[animal]""
TagsDisplay    =""[animal]""
IsChecked      =""False""
Selection      =""None""
IsEditing      =""False""
QueryTerm      =""elephant~trunk~herd~safari~[animal]""
FilterTerm     =""elephant~trunk~herd~safari""
TagMatchTerm   =""[animal]""
Properties     =""{
  ""Description"": ""Elephant"",
  ""Tags"": ""[animal]"",
  ""Keywords"": ""[\""trunk\"",\""herd\"",\""safari\""]""
}""

Description    =""Pineapple""
Keywords       =""[]""
KeywordsDisplay=""""
Tags           =""[fruit]""
TagsDisplay    =""[fruit]""
IsChecked      =""False""
Selection      =""None""
IsEditing      =""False""
QueryTerm      =""pineapple~[fruit]""
FilterTerm     =""pineapple""
TagMatchTerm   =""[fruit]""
Properties     =""{
  ""Description"": ""Pineapple"",
  ""Tags"": ""[fruit]""
}""

Description    =""Shark""
Keywords       =""[]""
KeywordsDisplay=""""
Tags           =""[fish]""
TagsDisplay    =""[fish]""
IsChecked      =""False""
Selection      =""None""
IsEditing      =""False""
QueryTerm      =""shark~[fish]""
FilterTerm     =""shark""
TagMatchTerm   =""[fish]""
Properties     =""{
  ""Description"": ""Shark"",
  ""Tags"": ""[fish]""
}""

Description    =""Owl""
Keywords       =""[]""
KeywordsDisplay=""""
Tags           =""[bird]""
TagsDisplay    =""[bird]""
IsChecked      =""False""
Selection      =""None""
IsEditing      =""False""
QueryTerm      =""owl~[bird]""
FilterTerm     =""owl""
TagMatchTerm   =""[bird]""
Properties     =""{
  ""Description"": ""Owl"",
  ""Tags"": ""[bird]""
}""

Description    =""Giraffe""
Keywords       =""[]""
KeywordsDisplay=""""
Tags           =""[animal]""
TagsDisplay    =""[animal]""
IsChecked      =""False""
Selection      =""None""
IsEditing      =""False""
QueryTerm      =""giraffe~[animal]""
FilterTerm     =""giraffe""
TagMatchTerm   =""[animal]""
Properties     =""{
  ""Description"": ""Giraffe"",
  ""Tags"": ""[animal]""
}""

Description    =""Coconut""
Keywords       =""[]""
KeywordsDisplay=""""
Tags           =""[fruit]""
TagsDisplay    =""[fruit]""
IsChecked      =""False""
Selection      =""None""
IsEditing      =""False""
QueryTerm      =""coconut~[fruit]""
FilterTerm     =""coconut""
TagMatchTerm   =""[fruit]""
Properties     =""{
  ""Description"": ""Coconut"",
  ""Tags"": ""[fruit]""
}""

Description    =""Kangaroo""
Keywords       =""[""bounce"",""outback"",""marsupial""]""
KeywordsDisplay=""""bounce"",""outback"",""marsupial""""
Tags           =""[animal]""
TagsDisplay    =""[animal]""
IsChecked      =""False""
Selection      =""None""
IsEditing      =""False""
QueryTerm      =""kangaroo~bounce~outback~marsupial~[animal]""
FilterTerm     =""kangaroo~bounce~outback~marsupial""
TagMatchTerm   =""[animal]""
Properties     =""{
  ""Description"": ""Kangaroo"",
  ""Tags"": ""[animal]"",
  ""Keywords"": ""[\""bounce\"",\""outback\"",\""marsupial\""]""
}""

Description    =""Dragonfruit""
Keywords       =""[]""
KeywordsDisplay=""""
Tags           =""[fruit]""
TagsDisplay    =""[fruit]""
IsChecked      =""False""
Selection      =""None""
IsEditing      =""False""
QueryTerm      =""dragonfruit~[fruit]""
FilterTerm     =""dragonfruit""
TagMatchTerm   =""[fruit]""
Properties     =""{
  ""Description"": ""Dragonfruit"",
  ""Tags"": ""[fruit]""
}""

Description    =""Turtle""
Keywords       =""[]""
KeywordsDisplay=""""
Tags           =""[animal]""
TagsDisplay    =""[animal]""
IsChecked      =""False""
Selection      =""None""
IsEditing      =""False""
QueryTerm      =""turtle~[animal]""
FilterTerm     =""turtle""
TagMatchTerm   =""[animal]""
Properties     =""{
  ""Description"": ""Turtle"",
  ""Tags"": ""[animal]""
}""

Description    =""Mango""
Keywords       =""[]""
KeywordsDisplay=""""
Tags           =""[fruit]""
TagsDisplay    =""[fruit]""
IsChecked      =""False""
Selection      =""None""
IsEditing      =""False""
QueryTerm      =""mango~[fruit]""
FilterTerm     =""mango""
TagMatchTerm   =""[fruit]""
Properties     =""{
  ""Description"": ""Mango"",
  ""Tags"": ""[fruit]""
}""

Description    =""Should NOT match an expression with an ""animal"" tag.""
Keywords       =""[]""
KeywordsDisplay=""""
Tags           =""[not animal]""
TagsDisplay    =""[not animal]""
IsChecked      =""False""
Selection      =""None""
IsEditing      =""False""
QueryTerm      =""should~not~match~an~expression~with~""animal""~tag.~[not animal]""
FilterTerm     =""should~not~match~an~expression~with~""animal""~tag.""
TagMatchTerm   =""[not animal]""
Properties     =""{
  ""Description"": ""Should NOT match an expression with an \""animal\"" tag."",
  ""Tags"": ""[not animal]""
}""
"
                ;

                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting report to match"
                );
            }

            /// <summary>
            /// Verifies JSON serialization preserves escaped quotes in term properties
            /// without corrupting underlying literal data.
            /// </summary>
            void subtest_JsonQuoteRendering()
            {
                testLiteralQuotes.Id = "0"; // Make random Id deterministic
                actual = JsonConvert.SerializeObject(testLiteralQuotes, Newtonsoft.Json.Formatting.Indented);
                expected = @" 
{
  ""Id"": ""0"",
  ""Description"": ""Should NOT match an expression with an \""animal\"" tag."",
  ""Keywords"": ""[]"",
  ""KeywordsDisplay"": """",
  ""Tags"": ""[not animal]"",
  ""TagsDisplay"": ""[not animal]"",
  ""IsChecked"": false,
  ""Selection"": 0,
  ""IsEditing"": false,
  ""PrimaryKey"": ""0"",
  ""QueryTerm"": ""should~not~match~an~expression~with~\""animal\""~tag.~[not animal]"",
  ""FilterTerm"": ""should~not~match~an~expression~with~\""animal\""~tag."",
  ""TagMatchTerm"": ""[not animal]"",
  ""Properties"": ""{\r\n  \""Description\"": \""Should NOT match an expression with an \\\""animal\\\"" tag.\"",\r\n  \""Tags\"": \""[not animal]\""\r\n}""
}"
                ;

                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting json to parse with correct quote escaping."
                );
            }

            /// <summary>
            /// Verifies that a user-escaped query (e.g., animal\") produces a valid SQL statement
            /// and returns the expected literal-quote match.
            /// </summary>
            void subtest_EscapedQuery()
            {
                testLiteralQuery.Id = "1"; // Make random Id deterministic
                actual = JsonConvert.SerializeObject(testLiteralQuery, Newtonsoft.Json.Formatting.Indented);
                expected = @" 
{
  ""Id"": ""1"",
  ""Description"": ""Should NOT match an expression with an \""animal\"" tag."",
  ""Keywords"": ""[]"",
  ""KeywordsDisplay"": """",
  ""Tags"": ""[not animal]"",
  ""TagsDisplay"": ""[not animal]"",
  ""IsChecked"": false,
  ""Selection"": 0,
  ""IsEditing"": false,
  ""PrimaryKey"": ""1"",
  ""QueryTerm"": ""should~not~match~an~expression~with~\""animal\""~tag.~[not animal]"",
  ""FilterTerm"": ""should~not~match~an~expression~with~\""animal\""~tag."",
  ""TagMatchTerm"": ""[not animal]"",
  ""Properties"": ""{\r\n  \""Description\"": \""Should NOT match an expression with an \\\""animal\\\"" tag.\"",\r\n  \""Tags\"": \""[not animal]\""\r\n}""
}"
                ;

                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting json to parse with correct quote escaping."
                );
            }
            #endregion S U B T E S T S
        }

        [TestMethod]
        public void Test_DefaultExpressions()
        {
            string actual, expected, sql;
            List<SelectableQFModelLTOQO> results;
            MarkdownContextOR context;
            ValidationState state;
            using (var cnx = InitializeInMemoryDatabase())
            {
                subtestBasicQueryAnimal();
                subtestPluralQueryAnimals();

                #region S U B T E S T S
                void subtestBasicQueryAnimal()
                {
                    sql = "animal".ParseSqlMarkdown<SelectableQFModelLTOQO>();

                    actual = sql;
                    expected = @" 
SELECT * FROM items WHERE (QueryTerm LIKE '%animal%')"
                    ;

                    Assert.AreEqual(
                        expected.NormalizeResult(),
                        actual.NormalizeResult(),
                        "Expecting expr does not include a Tag term"
                    );

                    results = cnx.Query<SelectableQFModelLTOQO>(sql);

                    actual = string.Join(Environment.NewLine, results.Select(_ => _.ToString()));
                    actual.ToClipboardExpected();
                    { }
                    expected = @" 
Black Cat  [animal] [color]
Orange Fox  [animal] [color]
White Rabbit ""bunny"",""soft"",""jump"" [animal] [color]
Gray Wolf ""pack"",""howl"",""wild"" [animal] [color]
Golden Lion  [animal] [color]
Brown Bear ""strong"",""wild"",""forest"" [animal] [color]
Black Panther ""stealthy"",""feline"",""night"" [animal] [color]
Elephant ""trunk"",""herd"",""safari"" [animal]
Giraffe  [animal]
Kangaroo ""bounce"",""outback"",""marsupial"" [animal]
Turtle  [animal]
Should NOT match an expression with an ""animal"" tag.  [not animal]"
                    ;

                    Assert.AreEqual(
                        expected.NormalizeResult(),
                        actual.NormalizeResult(),
                        "Expecting values to match."
                    );
                }

                void subtestPluralQueryAnimals()
                {
                    sql =
                        "animals"
                        .ParseSqlMarkdown<SelectableQFModelLTOQO>();

                    sql = sql.ToFuzzyQuery();

                    results = cnx.Query<SelectableQFModelLTOQO>(sql);

                    actual = string.Join(Environment.NewLine, results.Select(_ => _.ToString()));
                    actual.ToClipboardExpected();
                    { }
                    expected = @" 
Black Cat  [animal] [color]
Orange Fox  [animal] [color]
White Rabbit ""bunny"",""soft"",""jump"" [animal] [color]
Gray Wolf ""pack"",""howl"",""wild"" [animal] [color]
Golden Lion  [animal] [color]
Brown Bear ""strong"",""wild"",""forest"" [animal] [color]
Black Panther ""stealthy"",""feline"",""night"" [animal] [color]
Elephant ""trunk"",""herd"",""safari"" [animal]
Giraffe  [animal]
Kangaroo ""bounce"",""outback"",""marsupial"" [animal]
Turtle  [animal]
Should NOT match an expression with an ""animal"" tag.  [not animal]"
                    ;

                    Assert.AreEqual(
                        expected.NormalizeResult(),
                        actual.NormalizeResult(),
                        "Expecting values to match."
                    );
                }
                #endregion S U B T E S T S
            }
        }

        [TestMethod]
        public void Test_ObservableQueryFilterSource()
        {
            string actual, expected, sql;
            var builder = new List<string>();
            SenderEventPair sep;
            NotifyCollectionChangedEventArgs ecc;
            Queue<SenderEventPair> eventQueue = new();
            List<SelectableQFModelLTOQO> results;
            var itemsSource = new ObservableQueryFilterSource<SelectableQFModelLTOQO>();
            using (var cnx = InitializeInMemoryDatabase())
            {
                // This is just to skip to the second temporarily
                // Debug.Assert(DateTime.Now.Date == new DateTime(2026, 4, 05).Date, "Don't forget disabled");

                subtestBasicQueryAnimal();
                subtestBasicQueryAnimalINPC();

                #region S U B T E S T S
                void subtestBasicQueryAnimal()
                {
                    // This subtest only. For real do not hoist or generalize this.
                    using var local = this.WithOnDispose(
                        onInit: (sender, e) =>
                        {
                            itemsSource.CollectionChanged += localOnCollectionChanged;
                        },
                        onDispose: (sender, e) =>
                        {
                            itemsSource.CollectionChanged -= localOnCollectionChanged;
                        });
                    void localOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
                    {
                        builder.Add(e.ToString(ReferenceEquals(sender, itemsSource)));
                    }

                    // 260311.A RETROFIT - StateReport came online later. Let's see if it agrees.
                    actual = itemsSource.StateReport();
                    actual.ToClipboardExpected();
                    { }
                    expected = @" 
[IME Len: 0, IsFiltering: False], [Net: null, CC: 0, PMC: 0], [QueryAndFilter: SearchEntryState.Cleared, FilteringState.Ineligible]"
                    ;
                    Assert.AreEqual(
                        expected.NormalizeResult(),
                        actual.NormalizeResult(),
                        "Expecting StateReport shows FIRST CAUSE."
                    );

                    sql = "animal".ParseSqlMarkdown<SelectableQFModelLTOQO>();
                    results = cnx.Query<SelectableQFModelLTOQO>(sql);

                    // NEW 260311
                    actual = itemsSource.TopologyReport();
                    actual.ToClipboardExpected();
                    { }
                    expected = @" 
NetProjectionTopology.Routed, ReplaceItemsEventingOption.StructuralReplaceEvent"
                    ;

                    Assert.AreEqual(
                        expected.NormalizeResult(),
                        actual.NormalizeResult(),
                        "Expecting topology disvcovery to match."
                    );

                    itemsSource.ReplaceItems(results);

                    actual = string.Join(Environment.NewLine, builder);
                    actual.ToClipboardExpected();
                    { }
                    expected = @" 
NetProjection.Add     NewItems=12 ModelSettledEventArgs           "
                    ;

                    // 260311.B RETROFIT - StateReport came online later. Let's see if it agrees.
                    actual = itemsSource.StateReport();
                    actual.ToClipboardExpected();
                    { }
                    expected = @" 
[IME Len: 0, IsFiltering: True], [Net: null, CC: 12, PMC: 12], [QueryAndFilter: SearchEntryState.QueryCompleteWithResults, FilteringState.Armed]"
                    ;
                    Assert.AreEqual(
                        expected.NormalizeResult(),
                        actual.NormalizeResult(),
                        "Expecting REPLACE ITEMS semantics:" +
                        "StateReport shows INITIAL QUERY RECORDSET N=12 and PMC: 0 until filter activity takes place."
                    );

                    Assert.AreEqual(
                        expected.NormalizeResult(),
                        actual.NormalizeResult(),
                        "."
                    );

                    actual = string.Join(Environment.NewLine, itemsSource.Select(_ => _.ToString()));
                    actual.ToClipboardExpected();
                    { }
                    expected = @" 
Black Cat  [animal] [color]
Orange Fox  [animal] [color]
White Rabbit ""bunny"",""soft"",""jump"" [animal] [color]
Gray Wolf ""pack"",""howl"",""wild"" [animal] [color]
Golden Lion  [animal] [color]
Brown Bear ""strong"",""wild"",""forest"" [animal] [color]
Black Panther ""stealthy"",""feline"",""night"" [animal] [color]
Elephant ""trunk"",""herd"",""safari"" [animal]
Giraffe  [animal]
Kangaroo ""bounce"",""outback"",""marsupial"" [animal]
Turtle  [animal]
Should NOT match an expression with an ""animal"" tag.  [not animal]"
                    ;

                    Assert.AreEqual(
                        expected.NormalizeResult(),
                        actual.NormalizeResult(),
                        "Expecting filtered results to match."
                    );
                }
                void subtestBasicQueryAnimalINPC()
                {
                    #region L o c a l F x
                    // This subtest only. For real do not hoist or generalize this.
                    using var local = this.WithOnDispose(
                        onInit: (sender, e) =>
                        {
                            itemsSource.CollectionChanged += localOnCollectionChanged;
                        },
                        onDispose: (sender, e) =>
                        {
                            itemsSource.CollectionChanged -= localOnCollectionChanged;
                        });
                    void localOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
                    {
                        eventQueue.Enqueue((sender, e));
                        builder.Add(e.ToString(ReferenceEquals(sender, itemsSource)));
                    }
                    #endregion L o c a l F x

                    // 260311.C RETROFIT - StateReport came online later. Let's see if it agrees.
                    actual = itemsSource.StateReport();
                    actual.ToClipboardExpected();
                    { }
                    expected = @" 
[IME Len: 0, IsFiltering: True], [Net: null, CC: 12, PMC: 12], [QueryAndFilter: SearchEntryState.QueryCompleteWithResults, FilteringState.Armed]"
                    ;
                    Assert.AreEqual(
                        expected.NormalizeResult(),
                        actual.NormalizeResult(),
                        "Expecting StateReport shows RESUME WITH CURRENT STATE."
                    );

                    Assert.AreEqual(string.Empty, itemsSource.InputText, "Confirm before clear.");

                    // Expecting "no surprises" here.
                    builder.Clear();
                    eventQueue.Clear();
                    Assert.IsNull(itemsSource.ObservableNetProjection, "[Reminder] Expecting routed config.");
                    itemsSource.Clear();

                    actual = string.Join(Environment.NewLine, builder);
                    actual.ToClipboardExpected();
                    { }
                    expected = @"     
NetProjection.Reset   ModelSettledEventArgs           "
                    ;

                    // 260311.D RETROFIT - StateReport came online later. Let's see if it agrees.
                    actual = itemsSource.StateReport();
                    actual.ToClipboardExpected();
                    { }
                    expected = @" 
[IME Len: 0, IsFiltering: False], [Net: null, CC: 0, PMC: 0], [QueryAndFilter: SearchEntryState.Cleared, FilteringState.Ineligible]"
                    ;
                    Assert.AreEqual(
                        expected.NormalizeResult(),
                        actual.NormalizeResult(),
                        "Expecting StateReport shows ??."
                    );

                    ecc = (NotifyCollectionChangedEventArgs)eventQueue.DequeueSingle().e;
                    Assert.AreEqual(
                        NotifyCollectionChangedAction.Reset,
                        ecc.Action);
                    { }

                    builder.Clear();
                    sql = "animal".ParseSqlMarkdown<SelectableQFModelLTOQO>();
                    results = cnx.Query<SelectableQFModelLTOQO>(sql);

                    itemsSource.ReplaceItems(results);

                    actual = string.Join(Environment.NewLine, builder);
                    actual.ToClipboardExpected();
                    { }
                    expected = @" 
NetProjection.Reset   NotifyCollectionChangedEventArgs           
NetProjection.Add     NewItems=12 NewStartingIndex= 0 NotifyCollectionChangedEventArgs           "
                    ;
                    Assert.AreEqual(
                        expected.NormalizeResult(),
                        actual.NormalizeResult(),
                        "Expecting add component (first) + rest component (last)."
                    );

                    ecc = (NotifyCollectionChangedEventArgs)eventQueue.Dequeue().e;
                    Assert.AreEqual(
                        NotifyCollectionChangedAction.Reset,
                        ecc.Action,
                        "ReplaceItems -> LoadCanon -> Reset + Add");

                    ecc = (NotifyCollectionChangedEventArgs)eventQueue.DequeueSingle().e;
                    actual = string.Join(
                        Environment.NewLine,
                        ecc
                        .NewItems?.OfType<SelectableQFModelLTOQO>()
                        .Select(_ => _.ToString()) ?? []);
                    actual.ToClipboardExpected();
                    { }
                    expected = @" 
Black Cat  [animal] [color]
Orange Fox  [animal] [color]
White Rabbit ""bunny"",""soft"",""jump"" [animal] [color]
Gray Wolf ""pack"",""howl"",""wild"" [animal] [color]
Golden Lion  [animal] [color]
Brown Bear ""strong"",""wild"",""forest"" [animal] [color]
Black Panther ""stealthy"",""feline"",""night"" [animal] [color]
Elephant ""trunk"",""herd"",""safari"" [animal]
Giraffe  [animal]
Kangaroo ""bounce"",""outback"",""marsupial"" [animal]
Turtle  [animal]
Should NOT match an expression with an ""animal"" tag.  [not animal]"
                    ;
                }
                #endregion S U B T E S T S
            }
        }

        /// <summary>
        /// Verifies the progressive input FSM and commit pipeline.
        /// </summary>
        /// <remarks>
        /// This test exercises the full lifecycle of query entry, commit, and filtering to ensure
        /// deterministic state transitions, property-change ordering, and canonical recordset integrity.
        ///
        /// The commit pipeline is validated under three acquisition modes:
        ///
        /// 1. **Handler-supplied canonical recordset**  
        ///    Handler is assigned to `RecordsetRequest`; it intercepts the commit and provides the
        ///    canonical superset directly. The context must transition through the expected Busy/settlement
        ///    states while populating the canonical store and enabling filtering.
        ///
        /// 2. **Direct canonical injection (no commit pipeline)**  
        ///    `ReplaceItemsAsync` is used to populate the canonical superset from an externally
        ///    acquired recordset. This simulates a completed query result while bypassing the
        ///    commit pipeline entirely.
        ///
        /// 3. **Commit with `MemoryDatabase` augmentation**  
        ///    When `MemoryDatabase` is assigned, commit resolves through the internal query
        ///    pathway, allowing the context to execute the SQL against the memory-backed
        ///    database rather than requiring a handler to supply the recordset.
        ///
        /// Additional assertions confirm:
        /// - Progressive `InputText` transitions (`Cleared → QueryENB → QueryEN → QueryComplete`).
        /// - Deterministic `PropertyChanged` sequencing during commit and clear epochs.
        /// - Correct routing between canonical superset and filtered projection.
        /// - Stable filtering behavior when clearing input or re-entering filter expressions.
        /// </remarks>
        [TestMethod, DoNotParallelize]
        public async Task Test_TrackProgressiveInputState()
        {
            var id1 = Thread.CurrentThread.ManagedThreadId;

            using var te = this.TestableEpoch();

            var builder = new List<string>();

            string actual, expected, sql;
            NotifyCollectionChangedEventArgs ecc;
            PropertyChangedEventArgs epc;

            // Test for current version scheme
            await localTest<SelectableQFModelLTOQO>();

            // Test for early adopter (beta) migration support.
            await localTest<SelectableQueryModelOR>();

            async Task localTest<T>() where T : new()
            {
                @"\& \| \! \( \) \[ \] \' \"" \\".ParseSqlMarkdown<PetProfileN>();
                Queue<SenderEventPair> eventQueue = new();
                List<T> recordset;
                var items = new ObservableQueryFilterSource<T>();

                Assert.IsNull(
                    items.ObservableNetProjection,
                    "Expecting raw, portable list with no ONP.");
                Assert.AreEqual(
                    NetProjectionTopology.Routed, 
                    items.ProjectionTopology,
                    "Expecting detection of INotifyCollectionChanged in CTor.");

                string caller = string.Empty;


                items.InputTextSettled += async (sender, e) =>
                {
                    if (e is CancelEventArgs eCancel)
                    {
                        if (eCancel.Cancel)
                        {
                            await Task.Delay(TimeSpan.FromSeconds(0.25));
                        }
                        else
                        {
                            await Task.Delay(TimeSpan.FromSeconds(0.25));
                        }
                    }
                };

                // P R O P E R T Y   E V E N T    Q U E U E
                items.PropertyChanged += (sender, e) =>
                {
                    switch (e.PropertyName)
                    {
                        case nameof(items.Busy):
                            break;
                        case nameof(SearchEntryState):
                            if (caller == "subtestCommit")
                            {

                            }
                            builder.Add($"{e.PropertyName}='{items.SearchEntryState}'");
                            break;
                        case nameof(FilteringState):
                            if (caller == "subtestCommit")
                            {

                            }
                            break;
                        case nameof(items.RouteToFullRecordset):
                            if (caller == "subtestCommit")
                            {

                            }
                            break;
                        case nameof(items.ProxyType):
                            break;
                    }
                    eventQueue.Enqueue((sender, e));
                };

                using (var cnx = InitializeInMemoryDatabase())
                {
                    subtestClearedToFirstChar();
                    subtestFirstCharToEmpty();
                    subtestEmptyToFirstChar();
                    subtestSecondChar();
                    subtestThirdCharEnableQuery();

                    using (this.WithOnDispose(
                        onInit: (sender, e) =>
                        {
                            caller = nameof(subtestCommit);
                        },
                        onDispose: (sender, e) =>
                        {
                            caller = string.Empty;
                        }))
                    {
                        await subtestCommit();
                    }

                    #region S U B T E S T S

                    void subtestClearedToFirstChar()
                    {
                        actual = items.StateReport();
                        actual.ToClipboardExpected();
                        { }
                        expected = @" 
[IME Len: 0, IsFiltering: False], [Net: null, CC: 0, PMC: 0], [QueryAndFilter: SearchEntryState.Cleared, FilteringState.Ineligible]"
                        ;
                        Assert.AreEqual(expected.NormalizeResult(), actual.NormalizeResult(), "Expecting StateReport to match.");


                        Assert.AreEqual(
                            "Search Items",
                            items.Placeholder);

                        // "a"
                        items.InputText += 'a';
                        actual =
                            string
                            .Join(Environment.NewLine, eventQueue.Select(_ => _.e)
                            .OfType<PropertyChangedEventArgs>()
                            .Select(_ => _.PropertyName));

                        // Limits touched 260304
                        // First, make sure we're testing QueryAndFilter
                        Assert.AreEqual(QueryFilterConfig.QueryAndFilter, items.QueryFilterConfig);
                        // Now, this *did* have a Running change showing up but we don't want that.
                        expected = @" 
SearchEntryState
InputText"
                        ;

                        Assert.AreEqual(
                            expected.NormalizeResult(),
                            actual.NormalizeResult(),
                            "Expecting " +
                            "1. OnInputTextChanged runs FIRST and restarts Running." +
                            "2. OnPropertyChanged() notifies generically."
                        );
                        eventQueue.Clear();


                        actual = items.StateReport();
                        actual.ToClipboardExpected();
                        { }
                        expected = @" 
[IME Len: 1, IsFiltering: False], [Net: null, CC: 0, PMC: 0], [QueryAndFilter: SearchEntryState.QueryENB, FilteringState.Ineligible]"
                        ;
                        Assert.AreEqual(expected.NormalizeResult(), actual.NormalizeResult(), "Expecting StateReport to match.");
                    }

                    void subtestFirstCharToEmpty()
                    {
                        actual = items.StateReport();
                        actual.ToClipboardExpected();
                        { }
                        expected = @" 
[IME Len: 1, IsFiltering: False], [Net: null, CC: 0, PMC: 0], [QueryAndFilter: SearchEntryState.QueryENB, FilteringState.Ineligible]"
                        ;
                        Assert.AreEqual(expected.NormalizeResult(), actual.NormalizeResult(), "Expecting starting state is ENB.");

                        // Backspace
                        items.InputText = string.Empty;

                        actual = items.StateReport();
                        actual.ToClipboardExpected();
                        { }
                        expected = @" 
[IME Len: 0, IsFiltering: False], [Net: null, CC: 0, PMC: 0], [QueryAndFilter: SearchEntryState.Cleared, FilteringState.Ineligible]"
                        ;
                        Assert.AreEqual(expected.NormalizeResult(), actual.NormalizeResult(), "Expecting StateReport to match.");
                        eventQueue.Clear();
                    }

                    void subtestEmptyToFirstChar()
                    {
                        actual = items.StateReport();
                        actual.ToClipboardExpected();
                        { }
                        expected = @" 
[IME Len: 0, IsFiltering: False], [Net: null, CC: 0, PMC: 0], [QueryAndFilter: SearchEntryState.Cleared, FilteringState.Ineligible]"
                        ;
                        Assert.AreEqual(expected.NormalizeResult(), actual.NormalizeResult(), "Expecting coming in at Cleared.");

                        Assert.AreEqual(
                            "Search Items",
                            items.Placeholder);
                        // "a"
                        items.InputText += 'a';
                        actual =
                            string
                            .Join(Environment.NewLine, eventQueue.Select(_ => _.e)
                            .OfType<PropertyChangedEventArgs>()
                            .Select(_ => _.PropertyName));

                        expected = @"
SearchEntryState
InputText";

                        Assert.AreEqual(
                            expected.NormalizeResult(),
                            actual.NormalizeResult(),
                            "Expecting specific property changes."
                        );
                        Assert.AreEqual(
                            SearchEntryState.QueryENB,
                            items.SearchEntryState,
                            "Expecting ending state is ENB."
                        );
                        eventQueue.Clear();
                    }

                    void subtestSecondChar()
                    {
                        Assert.AreEqual(
                            SearchEntryState.QueryENB,
                            items.SearchEntryState,
                            "Expecting starting state is ENB."
                        );

                        // "an"
                        items.InputText += 'n';
                        actual =
                            string
                            .Join(Environment.NewLine, eventQueue.Select(_ => _.e)
                            .OfType<PropertyChangedEventArgs>()
                            .Select(_ => _.PropertyName));
                        actual.ToClipboardExpected();
                        expected = @"
InputText";

                        Assert.AreEqual(
                            expected.NormalizeResult(),
                            actual.NormalizeResult(),
                            "Expecting specific property changes."
                        );
                        Assert.AreEqual(
                            SearchEntryState.QueryENB,
                            items.SearchEntryState,
                            "Expecting ending state is 'still' ENB."
                        );
                        eventQueue.Clear();
                    }

                    void subtestThirdCharEnableQuery()
                    {
                        Assert.AreEqual(
                            SearchEntryState.QueryENB,
                            items.SearchEntryState,
                            "Expecting starting state is ENB."
                        );

                        // "ani"
                        items.InputText += 'i';
                        actual =
                            string
                            .Join(Environment.NewLine, eventQueue.Select(_ => _.e)
                            .OfType<PropertyChangedEventArgs>()
                            .Select(_ => _.PropertyName));
                        actual.ToClipboardExpected();
                        expected = @"
SearchEntryState
InputText";

                        Assert.AreEqual(
                            expected.NormalizeResult(),
                            actual.NormalizeResult(),
                            "Expecting specific property changes."
                        );
                        eventQueue.Clear();

                        Assert.AreEqual(
                            SearchEntryState.QueryEN,
                            items.SearchEntryState,
                            "Expecting specific state has now CHANGED."
                        );
                    }

                    async Task subtestCommit()
                    {
                        // "animal"
                        items.InputText += "mal";
                        Assert.AreEqual(
                            SearchEntryState.QueryEN,
                            items.SearchEntryState,
                            "Expecting specific state UNCHANGED."
                        );
                        Assert.IsFalse(items.IsFiltering, "Expecting NO NEED TO AWAIT HERE.");

                        actual = items.TopologyReport();
                        actual.ToClipboardExpected();
                        { }
                        expected = @" 
NetProjectionTopology.Routed, ReplaceItemsEventingOption.StructuralReplaceEvent";

                        Assert.AreEqual(
                            expected.NormalizeResult(),
                            actual.NormalizeResult(),
                            "Expecting routed topology."
                        );

                        #region C O M M I T
                        // This section wraps the RECORDSET REQUEST EVENT as a
                        // sim then calls the Commit method;
                        #region L o c a l F x
                        void localOnRecordsetRequestA(object? sender, RecordsetRequestEventArgs e)
                        {
                            actual = e.SQL;
                            actual.ToClipboardExpected();
                            { }
                            expected = @" 
SELECT * FROM items WHERE
(QueryTerm LIKE '%animal%')";

                            Assert.AreEqual(
                                expected.NormalizeResult(),
                                actual.NormalizeResult(),
                                "Expecting propertly formed query on 'items'."
                            );
                            e.CanonicalSuperset = cnx.Query<T>(e.SQL);
                        }
                        #endregion L o c a l F x
                        using (items.WithOnDispose(
                            onInit: (sender, e) =>
                            {
                                items.RecordsetRequest += localOnRecordsetRequestA;
                            },
                            onDispose: (sender, e) =>
                            {
                                items.RecordsetRequest -= localOnRecordsetRequestA;
                            }))
                        {
                            // ☆☆☆☆☆
                            // C O M M I T
                            ((MarkdownContext)items).Commit();
                            // ☆☆☆☆☆
                        }
                        #endregion C O M M I T

                        actual =
                            string
                            .Join(Environment.NewLine, eventQueue.Select(_ => _.e)
                            .OfType<PropertyChangedEventArgs>()
                            .Select(_ => _.PropertyName));

                        actual.ToClipboardExpected();
                        { }
                        // Limit touched 260316
                        // REMOVED: Running - we don't want that.
                        // ADDED: What we see now is a cradle-to-grave Commit epoch.
                        expected = @" 
InputText
ProxyType
TableName
Busy
SearchEntryState
FilteringState
IsFiltering
Busy"
                        ;

                        Assert.AreEqual(
                            expected.NormalizeResult(),
                            actual.NormalizeResult(),
                            "Expecting specific property changes."
                        );

                        actual = items.StateReport();
                        actual.ToClipboardExpected();
                        { }
                        expected = @" 
[IME Len: 6, IsFiltering: True], [Net: null, CC: 12, PMC: 12], [QueryAndFilter: SearchEntryState.QueryCompleteWithResults, FilteringState.Armed]"
                        ;
                        Assert.AreEqual(
                            expected.NormalizeResult(), 
                            actual.NormalizeResult(), 
                            "Expecting TURNKEY COMMIT FLOW.");

                        builder.Clear();
                        eventQueue.Clear();

                        // T E R M I N A L    C L E A R
                        items.Clear();

                        actual = items.StateReport();
                        actual.ToClipboardExpected();
                        { }
                        expected = @" 
[IME Len: 0, IsFiltering: False], [Net: null, CC: 0, PMC: 0], [QueryAndFilter: SearchEntryState.Cleared, FilteringState.Ineligible]"
                        ;
                        Assert.AreEqual(
                            expected.NormalizeResult(), 
                            actual.NormalizeResult(), 
                            "Expecting parameterless TERMINAL CLEAR.");

                        actual =
                            string
                            .Join(Environment.NewLine, eventQueue.Select(_ => _.e)
                            .OfType<PropertyChangedEventArgs>()
                            .Select(_ => _.PropertyName));
                        actual.ToClipboardExpected();
                        { }
                        expected = @" 
InputText
FilteringState
IsFiltering
SearchEntryState";

                        Assert.AreEqual(
                            expected.NormalizeResult(),
                            actual.NormalizeResult(),
                            "Expecting TERMINAL CLEAR EVENTS."
                        );

                        builder.Clear();
                        eventQueue.Clear();

                        // I N J E C T    I N S T E A D
                        Assert.AreEqual(string.Empty, items.InputText, "[Remember] - We did a terminal clear.");
                        sql = "animal".ParseSqlMarkdown<T>();
                        recordset = cnx.Query<T>(sql);

                        // DIFFERENT - Async version
                        await items.ReplaceItemsAsync(recordset);

                        actual = items.StateReport();
                        actual.ToClipboardExpected();
                        { }
                        expected = @" 
[IME Len: 0, IsFiltering: True], [Net: null, CC: 12, PMC: 12], [QueryAndFilter: SearchEntryState.QueryCompleteWithResults, FilteringState.Armed]"
                        ;
                        Assert.AreEqual(
                            expected.NormalizeResult(),
                            actual.NormalizeResult(),
                            "Expecting INJECTED CANON FLOW.");

                        actual =
                            string
                            .Join(Environment.NewLine, eventQueue.Select(_ => _.e)
                            .OfType<PropertyChangedEventArgs>()
                            .Select(_ => _.PropertyName));
                        actual.ToClipboardExpected();
                        { }
                        expected = @" 
SearchEntryState
FilteringState
IsFiltering"
                        ;

                        eventQueue.Clear();

                        Assert.AreEqual(SearchEntryState.QueryCompleteWithResults, items.SearchEntryState);
                        Assert.AreEqual(FilteringState.Armed, items.FilteringState);
                        Assert.AreNotEqual(0, recordset.Count);
                        Assert.AreNotEqual(0, items.Count);

                        actual = string.Join(Environment.NewLine, items.OfType<object>().Select(_ => _.ToString()));
                        actual.ToClipboardExpected();
                        { }
                        expected = @" 
Black Cat  [animal] [color]
Orange Fox  [animal] [color]
White Rabbit ""bunny"",""soft"",""jump"" [animal] [color]
Gray Wolf ""pack"",""howl"",""wild"" [animal] [color]
Golden Lion  [animal] [color]
Brown Bear ""strong"",""wild"",""forest"" [animal] [color]
Black Panther ""stealthy"",""feline"",""night"" [animal] [color]
Elephant ""trunk"",""herd"",""safari"" [animal]
Giraffe  [animal]
Kangaroo ""bounce"",""outback"",""marsupial"" [animal]
Turtle  [animal]
Should NOT match an expression with an ""animal"" tag.  [not animal]"
                        ;

                        Assert.AreEqual(
                            expected.NormalizeResult(),
                            actual.NormalizeResult(),
                            "Expecting filtered results to match."
                        );

                        // T E R M I N A L    C L E A R
                        items.Clear();

                        Assert.IsNull(items.MemoryDatabase, "Expecting this hasn't been set up yet for this test.");
                        items.MemoryDatabase = cnx;
                        items.InputText = "animal";



                        #region L o c a l F x
                        void localOnRecordsetRequestB(object? sender, RecordsetRequestEventArgs e)
                        {
                            actual = e.SQL;
                            expected = @" 
SELECT * FROM items WHERE
(QueryTerm LIKE '%animal%')";

                            Assert.AreEqual(
                                expected.NormalizeResult(),
                                actual.NormalizeResult(),
                                "Expecting propertly formed query on 'items'."
                            );

                            // T H I S    T I M E
                            // Allow the MDC to perform a query on MemoryDatabase.
                            // SKIP: e.CanonicalSuperset = cnx.Query<T>(e.SQL);
                        }
                        #endregion L o c a l F x
                        using (items.WithOnDispose(
                            onInit: (sender, e) =>
                            {
                                items.RecordsetRequest += localOnRecordsetRequestB;
                            },
                            onDispose: (sender, e) =>
                            {
                                items.RecordsetRequest -= localOnRecordsetRequestB;
                            }))
                        {
                            ((MarkdownContext)items).Commit();
                        }

                        // Now clear
                        // [Careful]
                        // This is an IList subclass *not* an MDC.
                        // Don't do this because it calls the "no suprises" version as per policy.
                        // items.Clear();   // <- NOPE!

                        // The intended Clear is the regressive version.
                        // Looking for a return value is one way to be sure.

                        Assert.AreEqual(
                            FilteringState.Armed,
                            items.Clear(all: false),     // <- EXECUTES THE CLEAR
                            "Expecting EMPTY input text WITHOUT regressing to Query. THIS MEANS ALL 12 ITEMS ARE SHOWN!"
                        );

                        Assert.AreEqual(
                            string.Empty,
                            items.InputText,
                            "Expecting empty input text."
                        );

#if ABSTRACT
                        // SEE FULL CONTEXT: Canonical #{5932CB31-B914-4DE8-9457-7A668CDB7D08}
                        
                        // Basically, if there is entry text but the filtering
                        // is still only armed not active, that indicates that
                        // what we're seeing in the list is the result of a full
                        // db query that just occurred. So now, when we CLEAR that
                        // text, it's assumed to be in the interest of filtering
                        // that query result, so filtering stays Armed in this case.

#endif

                        // Before
                        actual = items.StateReport();
                        actual.ToClipboardExpected();
                        { }

                        expected = @" 
[IME Len: 0, IsFiltering: True], [Net: null, CC: 12, PMC: 12], [QueryAndFilter: SearchEntryState.QueryCompleteWithResults, FilteringState.Armed]"
                        ;

                        Assert.AreEqual(expected.NormalizeResult(), actual.NormalizeResult(), "Expecting StateReport to match.");

                        actual = items.TopologyReport();
                        actual.ToClipboardExpected();
                        { }
                        expected = @" 
NetProjectionTopology.Routed, ReplaceItemsEventingOption.StructuralReplaceEvent"
                        ;

                        Assert.AreEqual(expected.NormalizeResult(), actual.NormalizeResult(), "Expecting StateReport to match.");

                        // PLEASE: Do not remove.
                        Assert.IsTrue(items.ValidationPredicate("b"), "This was a BUGIRL for the test itself.");

                        // animal.b
                        // Expecting Filter mode and an internal query.
                        // See also: {24048258-8BE4-40C4-BF85-8863E98BED51}
                        items.InputText += "b";
                        await items;

                        // After
                        actual = items.StateReport();
                        actual.ToClipboardExpected();
                        { }
                        expected = @" 
[IME Len: 1, IsFiltering: True], [Net: null, CC: 12, PMC: 5], [QueryAndFilter: SearchEntryState.QueryCompleteWithResults, FilteringState.Active]"
                        ;
                        Assert.AreEqual(
                            expected.NormalizeResult(), 
                            actual.NormalizeResult(), 
                            "Expecting the PMC is routed to the ONP. BOTH SHOULD SHOW 5.");


                        actual = string.Join(Environment.NewLine, items.Select(_ => _.ToString()));
                        actual.ToClipboardExpected();
                        { }
                        expected = @" 
Black Cat  [animal] [color]
White Rabbit ""bunny"",""soft"",""jump"" [animal] [color]
Brown Bear ""strong"",""wild"",""forest"" [animal] [color]
Black Panther ""stealthy"",""feline"",""night"" [animal] [color]
Kangaroo ""bounce"",""outback"",""marsupial"" [animal]"
                        ;

                        Assert.AreEqual(
                            expected.NormalizeResult(),
                            actual.NormalizeResult(),
                            "Expecting filtered items containing filter expr"
                        );

                        // [Careful("This polarity was wrong, and has been fixed.")]
                        // Limit touched 260301
                        Assert.IsFalse(items.RouteToFullRecordset);
                        { }
                    }
                    #endregion S U B T E S T S
                }
            }
        }

        [TestMethod]
        public void Test_FuzzyPlural()
        {
            string actual, expected, sql;
            List<SelectableQFModelLTOQO> recordset;
            using (var cnx = InitializeInMemoryDatabase())
            {
                subtestBasicQueryAnimal();
                subtestPluralQueryAnimals();

                #region S U B T E S T S
                void subtestBasicQueryAnimal()
                {
                    sql = "animal".ParseSqlMarkdown<SelectableQFModelLTOQO>();

                    actual = sql;
                    expected = @" 
SELECT * FROM items WHERE (QueryTerm LIKE '%animal%')"
                    ;

                    Assert.AreEqual(
                        expected.NormalizeResult(),
                        actual.NormalizeResult(),
                        "Expecting expr does not include a Tag term"
                    );

                    recordset = cnx.Query<SelectableQFModelLTOQO>(sql);

                    actual = string.Join(Environment.NewLine, recordset.Select(_ => _.ToString()));
                    actual.ToClipboardExpected();
                    { }
                    expected = @" 
Black Cat  [animal] [color]
Orange Fox  [animal] [color]
White Rabbit ""bunny"",""soft"",""jump"" [animal] [color]
Gray Wolf ""pack"",""howl"",""wild"" [animal] [color]
Golden Lion  [animal] [color]
Brown Bear ""strong"",""wild"",""forest"" [animal] [color]
Black Panther ""stealthy"",""feline"",""night"" [animal] [color]
Elephant ""trunk"",""herd"",""safari"" [animal]
Giraffe  [animal]
Kangaroo ""bounce"",""outback"",""marsupial"" [animal]
Turtle  [animal]
Should NOT match an expression with an ""animal"" tag.  [not animal]"
                    ;

                    Assert.AreEqual(
                        expected.NormalizeResult(),
                        actual.NormalizeResult(),
                        "Expecting values to match."
                    );
                }

                void subtestPluralQueryAnimals()
                {
                    sql =
                        "animals"
                        .ParseSqlMarkdown<SelectableQFModelLTOQO>();

                    sql = sql.ToFuzzyQuery();

                    recordset = cnx.Query<SelectableQFModelLTOQO>(sql);

                    actual = string.Join(Environment.NewLine, recordset.Select(_ => _.ToString()));
                    actual.ToClipboardExpected();
                    { }
                    expected = @" 
Black Cat  [animal] [color]
Orange Fox  [animal] [color]
White Rabbit ""bunny"",""soft"",""jump"" [animal] [color]
Gray Wolf ""pack"",""howl"",""wild"" [animal] [color]
Golden Lion  [animal] [color]
Brown Bear ""strong"",""wild"",""forest"" [animal] [color]
Black Panther ""stealthy"",""feline"",""night"" [animal] [color]
Elephant ""trunk"",""herd"",""safari"" [animal]
Giraffe  [animal]
Kangaroo ""bounce"",""outback"",""marsupial"" [animal]
Turtle  [animal]
Should NOT match an expression with an ""animal"" tag.  [not animal]"
                    ;

                    Assert.AreEqual(
                        expected.NormalizeResult(),
                        actual.NormalizeResult(),
                        "Expecting values to match."
                    );
                }
                #endregion S U B T E S T S
            }
        }

        [TestMethod]
        public void Test_PositionalOR()
        {
            string actual, expected, sql;
            XElement xast;

            // Test for early adopter (beta) migration support.
            localTest<SelectableQueryModelOR>();

            // Test for current model
            localTest<SelectableQFModelLTOQO>();

            void localTest<T>() where T : new()
            {
                List<T> recordset;
                MarkdownContextOR context;
                MarkdownContext mc;
                SQLiteCommand cmd;
                var validationState = ValidationState.Empty;

                using (var cnx = InitializeInMemoryDatabase())
                {
                    #region S U B T E S T S

                    sql = "color".ParseSqlMarkdown<T>(QueryFilterMode.Query, out xast);

                    actual = sql;
                    actual.ToClipboardExpected();
                    expected = @" 
SELECT * FROM items WHERE 
(QueryTerm LIKE '%color%')";

                    Assert.AreEqual(
                        expected.NormalizeResult(),
                        actual.NormalizeResult(),
                        "Expecting generated sql to match"
                    );

                    Assert.IsNotNull((mc = xast.To<MarkdownContext>()));

                    actual = mc.PositionalQuery;
                    expected = @" 
SELECT * FROM items WHERE (QueryTerm LIKE ?)";

                    Assert.AreEqual(
                        expected.NormalizeResult(),
                        actual.NormalizeResult(),
                        "Expecting templated query with '?'"
                    );


                    actual = mc.NamedQuery;
                    expected = @" 
SELECT * FROM items WHERE (QueryTerm LIKE @param0000)";

                    Assert.AreEqual(
                        expected.NormalizeResult(),
                        actual.NormalizeResult(),
                        "Expecting named query with '?'"
                    );


                    cmd = cnx.CreateCommand(mc.NamedQuery, mc.NamedArgs);

                    recordset = cmd.ExecuteQuery<T>();
                    Assert.AreEqual(19, recordset.Count);


                    cmd = cnx.CreateCommand(mc.PositionalQuery, mc.PositionalArgs);

                    recordset = cmd.ExecuteQuery<T>();
                    Assert.AreEqual(19, recordset.Count);

                    Assert.AreEqual(
                        expected.NormalizeResult(),
                        actual.NormalizeResult(),
                        "Expecting generated sql to match"
                    );
                    recordset = cnx.Query<T>(mc.PositionalQuery, mc.PositionalArgs);
                    Assert.AreEqual(19, recordset.Count);

                    sql = "animal".ParseSqlMarkdown<T>(QueryFilterMode.Query, out xast);
                    Assert.IsNotNull((mc = xast.To<MarkdownContext>()));

                    actual = sql;
                    expected = @" 
SELECT * FROM items WHERE 
(QueryTerm LIKE '%animal%')";

                    Assert.AreEqual(
                        expected.NormalizeResult(),
                        actual.NormalizeResult(),
                        "Expecting generated sql to match"
                    );

                    recordset = cnx.Query<T>(sql);
                    Assert.AreEqual(12, recordset.Count);


                    actual = mc.ToString();
                    actual.ToClipboard();
                    actual.ToClipboardExpected();
                    actual.ToClipboardAssert("Expecting ToString forms Sql");
                    { }
                    expected = @" 
SELECT * FROM items WHERE (QueryTerm LIKE '%animal%')";

                    Assert.AreEqual(
                        expected.NormalizeResult(),
                        actual.NormalizeResult(),
                        "Expecting ToString forms Sql"
                    );

                    recordset = cnx.Query<T>(mc.ToString());
                    Assert.AreEqual(12, recordset.Count);
#if ABSTRACT
                    HOWEVER:
                    - This doesn't work and isn't intended to.
                    - NOTE the CreateCommand version PASSED earlier and works fine.
                    - NOTE the Positional version  PASSED earlier and works fine.
                    But this permutation? Not so much. Don't use it.
                    recordset = cnx.Query<T>(mc.NamedQuery, mc.NamedArgs);
                    Assert.AreEqual(12, recordset.Count);
#endif

                    #endregion S U B T E S T S
                }
            }
        }

        [TestMethod]
        public void Test_PositionalExtraction()
        {
            string actual, expected;
            var bentDict = new Dictionary<string, object>
            {
                { "malformedKey", "junk" },
                { "@param-005", "also junk" },
                { "@param0001", "dog" },
                { "@param0000", "cat" },
                { "@param20EF", "hex key" },
            };

            IReadOnlyDictionary<string, object> NamedArgs =
                bentDict.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value is string s ? $"%{s}%" : kvp.Value
                );

            string input;

            input = @" SELECT * FROM items WHERE (NOT ((QueryTerm LIKE @param0000) OR (QueryTerm LIKE @param0001)))";
            actual = string.Join(
                Environment.NewLine,
                localSimulatePositionalArgsProperty(input)
            );
            expected = @" 
%cat%
%dog%"
            ;
            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting regex normal order correctly."
            );

            input = @" SELECT * FROM items WHERE (NOT ((QueryTerm LIKE @param0001) OR (QueryTerm LIKE @param0000)))";
            actual = string.Join(
                Environment.NewLine,
                localSimulatePositionalArgsProperty(input)
            );
            expected = @" 
%dog%
%cat%"
            ;
            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting regex detects reversed order and formulates positional args in correct order."
            );

            input = @" SELECT * FROM items WHERE (NOT ((QueryTerm LIKE @param20EF) OR (QueryTerm LIKE @param0000)))";
            actual = string.Join(
                Environment.NewLine,
                localSimulatePositionalArgsProperty(input)
            );
            expected = @" 
%hex key%
%cat%"
            ;
            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting regex detects hex and formulates positional args in correct order."
            );


            #region L o c a l F x
            object[] localSimulatePositionalArgsProperty(string sql)
            {
                return Regex
                    .Matches(sql, @"@param[0-9A-Fa-f]+")
                    .Cast<Match>()
                    .Select(m => NamedArgs[m.Value])
                    .ToArray();
            }
            #endregion L o c a l F x
        }

        [TestMethod]
        public void Test_PositionalV1()
        {
            string actual, expected, limit;
            XElement xast;
            MarkdownContext mc;
            subtestSimpleAnd();
            subtestUnaryNegation();
            subtestUnaryNegationTwoTerms();
            subtestUnarySubNegation();
            subtestUnaryComplexNegation();

            #region S U B T E S T S
            void subtestSimpleAnd()
            {
                actual = "brown dog".ParseSqlMarkdown<SelectableQFModelLTOQO>(QueryFilterMode.Query, out xast);
                expected = @" 
SELECT * FROM items WHERE (QueryTerm LIKE '%brown%') AND (QueryTerm LIKE '%dog%')";
                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting expression to match limit."
                );
                limit = expected;

                Assert.IsNotNull((mc = xast.To<MarkdownContext>()));

                actual = mc.XAST.ToString();
                expected = @" 
<ast clause=""(QueryTerm LIKE '%brown%') AND (QueryTerm LIKE '%dog%')"">
  <term value=""brown"" clause=""(QueryTerm LIKE '%brown%') AND (QueryTerm LIKE '%dog%')"" pos=""[KVPs]"">
    <and clause=""AND (QueryTerm LIKE '%dog%')"">
      <term value=""dog"" clause=""(QueryTerm LIKE '%dog%')"" pos=""[KVPs]"" />
    </and>
  </term>
</ast>"
                ;

                actual = mc.NamedQuery;
                expected = @" 
SELECT * FROM items WHERE (QueryTerm LIKE @param0000) AND (QueryTerm LIKE @param0001)"
                ;
                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult()
                );

                actual = string.Join(Environment.NewLine, mc.NamedArgs);
                expected = @" 
[@param0000, %brown%]
[@param0001, %dog%]"
                ;
                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult()
                );
            }

            void subtestUnaryNegation()
            {
                actual = "!cat".ParseSqlMarkdown<SelectableQFModelLTOQO>(QueryFilterMode.Query, out xast);
                expected = "SELECT * FROM items WHERE (NOT (QueryTerm LIKE '%cat%'))";
                Assert.AreEqual(expected.NormalizeResult(), actual.NormalizeResult());

                mc = xast.To<MarkdownContext>();
                actual = mc.NamedQuery;
                expected = @" 
SELECT * FROM items WHERE (NOT (QueryTerm LIKE @param0000))"
                ;
                Assert.AreEqual(expected.NormalizeResult(), actual.NormalizeResult());

                actual = string.Join(Environment.NewLine, mc.NamedArgs);
                expected = @" 
[@param0000, %cat%]"
                ;
                Assert.AreEqual(expected.NormalizeResult(), actual.NormalizeResult());
            }

            void subtestUnaryNegationTwoTerms()
            {
                actual = "!cat dog".ParseSqlMarkdown<SelectableQFModelLTOQO>(QueryFilterMode.Query, out xast);
                expected = @" 
SELECT * FROM items WHERE (NOT (QueryTerm LIKE '%cat%')) AND (QueryTerm LIKE '%dog%')";
                Assert.AreEqual(expected.NormalizeResult(), actual.NormalizeResult());

                mc = xast.To<MarkdownContext>();
                actual = mc.NamedQuery;
                expected = @" 
SELECT * FROM items WHERE (NOT (QueryTerm LIKE @param0000)) AND (QueryTerm LIKE @param0001)"
                ;
                Assert.AreEqual(expected.NormalizeResult(), actual.NormalizeResult());

                actual = string.Join(Environment.NewLine, mc.NamedArgs);
                expected = @" 
[@param0000, %cat%]
[@param0001, %dog%]"
                ;
                Assert.AreEqual(expected.NormalizeResult(), actual.NormalizeResult());
            }

            void subtestUnarySubNegation()
            {
                actual = "!(cat|dog)".ParseSqlMarkdown<SelectableQFModelLTOQO>(QueryFilterMode.Query, out xast);
                actual.ToClipboardExpected();
                expected = @" 
SELECT * FROM items WHERE 
(NOT ((QueryTerm LIKE '%cat%') OR (QueryTerm LIKE '%dog%')))"
                ;
                Assert.AreEqual(expected.NormalizeResult(), actual.NormalizeResult());

                mc = xast.To<MarkdownContext>();
                actual = mc.NamedQuery;
                expected = @" 
SELECT * FROM items WHERE (NOT ((QueryTerm LIKE @param0000) OR (QueryTerm LIKE @param0001)))"
                ;
                Assert.AreEqual(expected.NormalizeResult(), actual.NormalizeResult());

                actual = string.Join(Environment.NewLine, mc.NamedArgs);
                actual.ToClipboardExpected();
                expected = @" 
[@param0000, %cat%]
[@param0001, %dog%]"
                ;
                Assert.AreEqual(expected.NormalizeResult(), actual.NormalizeResult());
            }

            void subtestUnaryComplexNegation()
            {
                actual = "pet!(cat|dog)".ParseSqlMarkdown<SelectableQFModelLTOQO>(QueryFilterMode.Query, out xast);
                expected = "SELECT * FROM pets WHERE (Name LIKE '%pet%') AND (NOT ((Name LIKE '%cat%') OR (Name LIKE '%dog%')))";
            }
            #endregion S U B T E S T S
        }

        [TestMethod]
        public void Test_ShortExpr()
        {
            XElement xast;
            string actual, expected, stringFirst;
            MarkdownContext mc;
            SQLiteCommand cmd;
            List<SelectableQFModelLTOQO> recordset;
            int countManual;
            using (var cnx = InitializeInMemoryDatabase())
            {
                actual = "b".ParseSqlMarkdown<SelectableQFModelLTOQO>();
                Assert.AreEqual(string.Empty, actual);

                // Specify minimum length via helper.
                actual = "b".ParseSqlMarkdown<SelectableQFModelLTOQO>(minInputLength: 0);
                // Returns QueryTerm
                expected = @" 
SELECT * FROM items WHERE (QueryTerm LIKE '%b%')"
                ;
                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting expression is considered valid with no minimum length requirement."
                );

                // Specify minimum length via state.
                actual = "b".ParseSqlMarkdown<SelectableQFModelLTOQO>(QueryFilterMode.Filter, out xast);
                // Returns FilterTerm // <= Different!
                expected = @" 
SELECT * FROM items WHERE (FilterTerm LIKE '%b%')"
                ;

                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting expression is considered valid with no minimum length requirement."
                );

                Assert.IsNotNull((mc = xast.To<MarkdownContext>()));

                actual = mc.ParseSqlMarkdown<SelectableQFModelLTOQO>("b", QueryFilterMode.Filter);
                expected = @" 
SELECT * FROM items WHERE 
(FilterTerm LIKE '%b%')"
                ;

                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting Filter mode to suppress minimum length requirement."
                );

                actual = string.Join(",", mc.NamedQuery);
                expected = @" 
SELECT * FROM items WHERE (FilterTerm LIKE @param0000)"
                ;

                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting args to match."
                );

                actual = string.Join(",", mc.NamedArgs);
                expected = @" 
[@param0000, %b%]"
                ;

                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting args to match."
                );

                actual = string.Join(",", mc.PositionalQuery);
                expected = @" 
SELECT * FROM items WHERE (FilterTerm LIKE ?)"
                ;

                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting args to match."
                );

                actual = string.Join(",", mc.PositionalArgs);
                expected = @" 
%b%"
                ;

                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting args to match."
                );

                subtestFormNamedCommand();
                subtestFormPositionalCommand();

                #region S U B T E S T S

                void subtestFormNamedCommand()
                {
                    // Valid (query)
                    stringFirst = mc.ParseSqlMarkdown<SelectableQFModelLTOQO>("animal b");
                    actual = stringFirst;
                    expected = @" 
SELECT * FROM items WHERE 
(QueryTerm LIKE '%animal%') AND (QueryTerm LIKE '%b%')"
                    ;
                    Assert.AreEqual(
                        expected.NormalizeResult(),
                        actual.NormalizeResult(),
                        "Expecting match."
                    );

                    actual = mc.NamedQuery;
                    expected = @" 
SELECT * FROM items WHERE (QueryTerm LIKE @param0000) AND (QueryTerm LIKE @param0001)"
                    ;

                    Assert.AreEqual(
                        expected.NormalizeResult(),
                        actual.NormalizeResult(),
                        "Expecting match."
                    );

                    recordset = cnx.Query<SelectableQFModelLTOQO>(
                        @"SELECT * FROM items WHERE (QueryTerm LIKE '%animal%') AND (QueryTerm LIKE '%b%')");
                    countManual = recordset.Count();

                    cmd = cnx.CreateCommand(mc.NamedQuery, mc.NamedArgs);

                    actual = cmd.ToString();
                    expected = @" 
SELECT * FROM items WHERE (QueryTerm LIKE @param0000) AND (QueryTerm LIKE @param0001)
  0: %animal%
  1: %b%"
                    ;

                    Assert.AreEqual(
                        expected.NormalizeResult(),
                        actual.NormalizeResult(),
                        "Expecting match."
                    );

                    recordset = cmd.ExecuteQuery<SelectableQFModelLTOQO>();
                    Assert.AreEqual(
                        countManual,
                        recordset.Count(),
                        "Expecting that we can compose a command that produced the same result as the long form."
                    );
                }
                void subtestFormPositionalCommand()
                {
                    stringFirst = mc.ParseSqlMarkdown<SelectableQFModelLTOQO>("animal b");
                    actual = stringFirst;
                    expected = @" 
SELECT * FROM items WHERE 
(QueryTerm LIKE '%animal%') AND (QueryTerm LIKE '%b%')"
                    ;
                    Assert.AreEqual(
                        expected.NormalizeResult(),
                        actual.NormalizeResult(),
                        "Expecting match."
                    );

                    countManual = recordset.Count();

                    cmd = cnx.CreateCommand(mc.PositionalQuery, mc.PositionalArgs);

                    actual = cmd.ToString();
                    actual.ToClipboard();
                    actual.ToClipboardExpected();
                    actual.ToClipboardAssert("Expecting positional command is properly ordered.");
                    { }
                    expected = @" 
SELECT * FROM items WHERE (QueryTerm LIKE ?) AND (QueryTerm LIKE ?)
  0: %animal%
  1: %b%";

                    Assert.AreEqual(
                        expected.NormalizeResult(),
                        actual.NormalizeResult(),
                        "Expecting positional command is properly ordered."
                    );
                    recordset = cmd.ExecuteQuery<SelectableQFModelLTOQO>();
                    Assert.AreEqual(countManual, recordset.Count());
                }
                #endregion S U B T E S T S
            }
        }

        [TestMethod]
        public void Test_StrictTagExpr()
        {
            string actual, expected, unexpected, sql;
            MarkdownContextOR context;
            List<StrictTagQueryModel> recordset;
            ValidationState validationState = ValidationState.Valid;
            using (var cnx = InitializeInMemoryDatabase<StrictTagQueryModel>())
            {
                subtestReportBrownDog();
                subtestStrictNotBracketed();
                subtestStrictBracketed();

                #region S U B T E S T S
                void subtestReportBrownDog()
                {
                    recordset = cnx.Query<StrictTagQueryModel>("brown dog".ParseSqlMarkdown<StrictTagQueryModel>());
                    actual = recordset.Single().Report();

                    actual.ToClipboardExpected();
                    { }
                    expected = @" 
Description    =""Brown Dog""
Keywords       =""[""loyal"",""friend"",""furry""]""
KeywordsDisplay=""""loyal"",""friend"",""furry""""
Tags           =""[canine][color]""
TagsDisplay    =""[canine][color]""
IsChecked      =""False""
Selection      =""None""
IsEditing      =""False""
QueryTerm      =""brown~dog~loyal~friend~furry""
FilterTerm     =""brown~dog~loyal~friend~furry""
TagMatchTerm   =""[canine][color]""
Properties     =""{
  ""Description"": ""Brown Dog"",
  ""Tags"": ""[canine][color]"",
  ""Keywords"": ""[\""loyal\"",\""friend\"",\""furry\""]""
}"""
                    ;

                    Assert.AreEqual(
                        expected.NormalizeResult(),
                        actual.NormalizeResult(),
                        "Expecting strict tag indexing."
                    );
                }
                void subtestStrictNotBracketed()
                {
                    sql = "canine".ParseSqlMarkdown<StrictTagQueryModel>();

                    actual = sql;
                    expected = @" 
SELECT * FROM items WHERE (QueryTerm LIKE '%canine%')"
                    ;

                    Assert.AreEqual(
                        expected.NormalizeResult(),
                        actual.NormalizeResult(),
                        "Expecting expr does not include a Tag term"
                    );
                    recordset = cnx.Query<StrictTagQueryModel>(sql);
                    Assert.IsFalse(recordset.Any());
                }
                void subtestStrictBracketed()
                {
                    sql = "[canine]".ParseSqlMarkdown<StrictTagQueryModel>();

                    actual = sql;
                    expected = @" 
SELECT * FROM items WHERE (QueryTerm LIKE '%[canine]%' OR TagMatchTerm LIKE '%[canine]%')"
                    ;

                    Assert.AreEqual(
                        expected.NormalizeResult(),
                        actual.NormalizeResult(),
                        "Expecting correct expr"
                    );
                    recordset = cnx.Query<StrictTagQueryModel>(sql);
                    Assert.IsNotNull(recordset.SingleOrDefault());
                }
                #endregion S U B T E S T S
            }
        }

        [TestMethod]
        public void Test_AtomicDQuotes()
        {
            string actual, expected;
            List<SelectableQFModelLTOQO> recordset;
            MarkdownContextOR context;
            ValidationState validationState = ValidationState.Valid;

            actual = "Tom Tester".ParseSqlMarkdown<SelectableQFModelLTOQO>();
            expected = @" 
SELECT * FROM items WHERE (QueryTerm LIKE '%Tom%') AND (QueryTerm LIKE '%Tester%')"
            ;
            Assert.AreEqual(expected.NormalizeResult(), actual.NormalizeResult(), "Expecting unquoted term behavior.");

            actual = "Tom Tester'".ParseSqlMarkdown<SelectableQFModelLTOQO>();
            expected = @" 
SELECT * FROM items WHERE (QueryTerm LIKE '%Tom%') AND (QueryTerm LIKE '%Tester''%')"
            ;
            Assert.AreEqual(expected.NormalizeResult(), actual.NormalizeResult(), "Expecting trailing single quote is escaped.");

            actual = "Tom Tester's".ParseSqlMarkdown<SelectableQFModelLTOQO>();
            expected = @" 
SELECT * FROM items WHERE (QueryTerm LIKE '%Tom%') AND (QueryTerm LIKE '%Tester''s%')"
            ;
            Assert.AreEqual(expected.NormalizeResult(), actual.NormalizeResult(), "Expecting single quote is escaped.");

            actual = @"""Tom Tester".ParseSqlMarkdown<SelectableQFModelLTOQO>();
            expected = @" 
SELECT * FROM items WHERE (QueryTerm LIKE '%""Tom%') AND (QueryTerm LIKE '%Tester%')"
            ;
            Assert.AreEqual(expected.NormalizeResult(), actual.NormalizeResult(), "Expecting unclosed double quote treated as literal.");

            actual = @"""Tom Tester'".ParseSqlMarkdown<SelectableQFModelLTOQO>();
            expected = @" 
SELECT * FROM items WHERE (QueryTerm LIKE '%""Tom%') AND (QueryTerm LIKE '%Tester''%')"
            ;
            Assert.AreEqual(expected.NormalizeResult(), actual.NormalizeResult(), "Expecting inner single quote escaped within unmatched double quote.");

            actual = @"""Tom Tester's".ParseSqlMarkdown<SelectableQFModelLTOQO>();
            expected = @" 
SELECT * FROM items WHERE (QueryTerm LIKE '%""Tom%') AND (QueryTerm LIKE '%Tester''s%')"
            ;
            Assert.AreEqual(expected.NormalizeResult(), actual.NormalizeResult(), "Expecting unclosed double quote with inner apostrophe.");

            actual = @"""Tom Tester's""".ParseSqlMarkdown<SelectableQFModelLTOQO>();
            expected = @" 
SELECT * FROM items WHERE (QueryTerm LIKE '%Tom Tester''s%')"
            ;
            Assert.AreEqual(expected.NormalizeResult(), actual.NormalizeResult(), "Expecting atomic term with apostrophe escaped.");
            #region V A R I A N T
            // We successfully have made an atomic term, and it is NOT QUOTED in the query.
            // Now, can you surround that atomic term with quotes?

            actual = @"""""""Tom Tester's""""""".ParseSqlMarkdown<SelectableQFModelLTOQO>();
            expected = @" 
SELECT * FROM items WHERE (QueryTerm LIKE '%""""Tom Tester''s""""%')"
            ;

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting an atomic term, surrounded by quotes (i.e. the outside quotes pair)."
            );
            #endregion V A R I A N T

            actual = @"You said, """.ParseSqlMarkdown<SelectableQFModelLTOQO>();
            expected = @" 
SELECT * FROM items WHERE (QueryTerm LIKE '%You%') AND (QueryTerm LIKE '%said,%') AND (QueryTerm LIKE '%""%')"
            ;
            Assert.AreEqual(expected.NormalizeResult(), actual.NormalizeResult(), "Expecting unmatched double quote treated as literal.");

            actual = @"You said, """"".ParseSqlMarkdown<SelectableQFModelLTOQO>();
            expected = @" 
SELECT * FROM items WHERE (QueryTerm LIKE '%You%') AND (QueryTerm LIKE '%said,%') AND (QueryTerm LIKE '%""%')"
            ;
            Assert.AreEqual(expected.NormalizeResult(), actual.NormalizeResult(), "Expecting escaped double quote treated as literal.");

            actual = @"You said, """"H".ParseSqlMarkdown<SelectableQFModelLTOQO>();
            expected = @" 
SELECT * FROM items WHERE (QueryTerm LIKE '%You%') AND (QueryTerm LIKE '%said,%') AND (QueryTerm LIKE '%""H%')"
            ;
            Assert.AreEqual(expected.NormalizeResult(), actual.NormalizeResult(), "Expecting escaped quote followed by text.");

            actual = @"You said, """"Hello""".ParseSqlMarkdown<SelectableQFModelLTOQO>();
            expected = @" 
SELECT * FROM items WHERE (QueryTerm LIKE '%You%') AND (QueryTerm LIKE '%said,%') AND (QueryTerm LIKE '%""Hello""%')"
            ;
            Assert.AreEqual(expected.NormalizeResult(), actual.NormalizeResult(), "SUBTLE! Expecting escaped double quote + Unmatched quote at end is literal.");

            actual = @"You said, """"Hello"""".".ParseSqlMarkdown<SelectableQFModelLTOQO>();
            expected = @" 
SELECT * FROM items WHERE (QueryTerm LIKE '%You%') AND (QueryTerm LIKE '%said,%') AND (QueryTerm LIKE '%""Hello"".%')"
            ;
            Assert.AreEqual(expected.NormalizeResult(), actual.NormalizeResult(), @"Expecting DOES NOT ATOMIZE Hello. It should be a single term surrounded by quotes instead");
        }

        [TestMethod]
        public void Test_AtomicSQuotes()
        {
            string actual, expected;
            string sql;
            List<SelectableQFModelLTOQO> recordset;
            MarkdownContextOR context;
            ValidationState validationState = ValidationState.Valid;


            sql = @"'Tom Tester'".ParseSqlMarkdown<SelectableQFModelLTOQO>();

            actual = sql;
            expected = @" 
SELECT * FROM items WHERE (QueryTerm LIKE '%Tom Tester%')";

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting term between squotes is insulated from linting."
            );

            sql = @"'Tom ""safe inner"" Tester'".ParseSqlMarkdown<SelectableQFModelLTOQO>();

            // {93B1FE29-F593-47EC-9CFA-F2706E79AA9E}
            actual = sql;
            expected = @" 
SELECT * FROM items WHERE (QueryTerm LIKE '%Tom safe inner Tester%')";

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "SUBTLE: The dquote linter runs before the squote linter so both have had an effect."
            );

            sql = @"'Tom """"safe inner"""" Tester'".ParseSqlMarkdown<SelectableQFModelLTOQO>();

            // {93B1FE29-F593-47EC-9CFA-F2706E79AA9E}
            actual = sql;

            actual.ToClipboardExpected();
            expected = @" 
SELECT * FROM items WHERE (QueryTerm LIKE '%Tom ""safe inner"" Tester%')"
            ;

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Escaped literal quotes"
            );
        }

        /// <summary>
        /// PFAW!
        /// </summary>
        [TestMethod]
        [Careful(@"
Shows how to register a custom function. 
But for JsonExtract don't do that!
Use 'json_extract' as shown or wrap it with the string.JsonExtract helper.")]
        public void Test_CustomSQLiteFunction()
        {
            using (var cnx = InitializeInMemoryDatabase())
            {
                SQLitePCL.raw.SetProvider(new SQLitePCL.SQLite3Provider_e_sqlite3());
                SQLitePCL.Batteries.Init();
                SQLitePCL.Batteries_V2.Init();

                SQLitePCL.raw.sqlite3_create_function(
                    cnx.Handle,
                    "JsonExtract",
                    2,
                    1,
                    null,
                    (ctx, user_data, args) =>
                    {
                        var json = SQLitePCL.raw.sqlite3_value_text(args[0]).utf8_to_string();
                        var key = SQLitePCL.raw.sqlite3_value_text(args[1]).utf8_to_string();

                        // O U T
                        string? value = null;

                        (JsonConvert.DeserializeObject<Dictionary<string, string>>(json) as IDictionary<string, string>)
                        ?.TryGetValue(key, out value);

                        SQLitePCL.raw.sqlite3_result_text(ctx, value ?? string.Empty);
                    }
                );

                // Arg0: The Column (*is not* literal)
                // Arg1: The 'Key'  (*is* literal)
                IList recordset;
                recordset = cnx.Query<SelectableQFModelLTOQO>($@"
Select *
From items 
Where JsonExtract(Properties, 'Description') LIKE '%brown dog%'");

                Assert.AreEqual(1, recordset.Count, "Expecting successful query using custom function.");


                // BUT THIS IS HOW YOU DO IT!
                // Arg0: The Column (*is not* literal)
                // Arg1: The 'Key'  (*is* literal and the $. is the ROOT SELECTOR)
                recordset = cnx.Query<SelectableQFModelLTOQO>($@"
Select *
From items 
Where json_extract(Properties, '$.Description') LIKE '%brown dog%'");
                Assert.AreEqual(1, recordset.Count, "Expecting successful query using json_extract.");

                // And this makes it readable.
                recordset = cnx.Query<SelectableQFModelLTOQO>($@"
Select *
From items 
Where {"Properties".JsonExtract("Description")} LIKE '%brown dog%'");
                Assert.AreEqual(1, recordset.Count, "Expecting successful query using JsonExtract helper extension.");
            }
        }

        [TestMethod, DoNotParallelize]
        public async Task Test_DemoFlow()
        {
            using var te = this.TestableEpoch();

            string actual, expected;
            IList newItems = Array.Empty<object>();
            Queue<SenderEventPair> eventQueue = new();
            var builder = new List<string>();

            var items = new ObservableQueryFilterSource<SelectableQFModel>
            {
                MemoryDatabase = InitializeInMemoryDatabase()
            };
            NavSearchBar nsb = new NavSearchBar
            {
                // NavSearchBar UI controls are designed
                // to switch out sources many times.
                ItemsSource = items,
            };
            items.InputTextSettled += async (sender, e) =>
            {
                switch (items.FilteringState)
                {
                    case FilteringState.Ineligible:
                        break;
                    case FilteringState.Armed:
                        break;
                    case FilteringState.Active:
                        break;
                    default:
                        break;
                }
            };
            items.CollectionChanged += (sender, e) =>
            {
                if (ReferenceEquals(sender, items.CanonicalSuperset))
                {
                    Debug.Fail($@"ADVISORY - First Time.");
                }
                eventQueue.Enqueue((sender, e));
                builder.Add(e.ToString(ReferenceEquals(sender, items)));

                // G T K
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        newItems = e.NewItems?.Cast<object>().ToArray() ?? Array.Empty<object>();
                        break;
                    case NotifyCollectionChangedAction.Remove:
                        break;
                    case NotifyCollectionChangedAction.Replace:
                        break;
                    case NotifyCollectionChangedAction.Move:
                        break;
                    case NotifyCollectionChangedAction.Reset:
                        break;
                    default:
                        break;
                }
            };

            await subtestQueryInitial();
            await subtest_Animals();
            await subtestAppendDatabaseAndRequery();

            #region S U B T E S T S
            /// <summary>
            /// Verifies Commit phase for a routed topology.
            /// </summary>
            /// <remarks>
            /// Confirms that Commit() executes synchronously against the memory database, routes through LoadCanon,
            /// emits a structural Reset followed by Add, and yields a canonical superset and projection that are
            /// consistent in count, order, and payload.
            /// </remarks>
            async Task subtestQueryInitial()
            {
                actual = items.StateReport();
                Assert.IsNull(items.ObservableNetProjection);
                expected = @" 
[IME Len: 0, IsFiltering: False], [Net: null, CC: 0, PMC: 0], [QueryAndFilter: SearchEntryState.Cleared, FilteringState.Ineligible]"
                ;
                Assert.AreEqual(expected.NormalizeResult(), actual.NormalizeResult(), "Expecting initial StateReport to match.");

                Assert.AreEqual(0, eventQueue.Count, "YES. It's zero.");

                // Nothing requires await here.
                nsb.InputText = "animal";   // Synchronous because IsFiltering is False.
                items.Commit();             // Query on a synchronous memory connection => ReplaceItems() => LoadCanon().

                // #{4E778EBA-D838-48D0-89D6-3D1FC8229E23}
                // Limit touched 260404
                actual = string.Join(Environment.NewLine, builder);
                actual.ToClipboardExpected();
                { }
                expected = @" 
NetProjection.Reset   NotifyCollectionChangedEventArgs           
NetProjection.Add     NewItems=12 NewStartingIndex= 0 NotifyCollectionChangedEventArgs           "
                ;

                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting the Commit method has an add component (first) and a reset component (last)."
                );

                actual = items.Model.ToString();
                actual.ToClipboardExpected();
                { }
                // [Careful("What?")] No 'preview' attribute? THAT'S BECAUSE THIS IS SelectableQFModel and *not* IAffinityModel.
                expected = @" 
<model mdc=""[MDC]"" histo=""[model:12 match:0 qmatch:0 pmatch:0]"" filters=""[No Active Filters]"">
  <item text=""312d1c21-0000-0000-0000-000000000005"" model=""[SelectableQFModel]"" order=""0"" />
  <item text=""312d1c21-0000-0000-0000-000000000006"" model=""[SelectableQFModel]"" order=""1"" />
  <item text=""312d1c21-0000-0000-0000-000000000007"" model=""[SelectableQFModel]"" order=""2"" />
  <item text=""312d1c21-0000-0000-0000-000000000009"" model=""[SelectableQFModel]"" order=""3"" />
  <item text=""312d1c21-0000-0000-0000-00000000000b"" model=""[SelectableQFModel]"" order=""4"" />
  <item text=""312d1c21-0000-0000-0000-00000000000c"" model=""[SelectableQFModel]"" order=""5"" />
  <item text=""312d1c21-0000-0000-0000-00000000000f"" model=""[SelectableQFModel]"" order=""6"" />
  <item text=""312d1c21-0000-0000-0000-000000000014"" model=""[SelectableQFModel]"" order=""7"" />
  <item text=""312d1c21-0000-0000-0000-000000000018"" model=""[SelectableQFModel]"" order=""8"" />
  <item text=""312d1c21-0000-0000-0000-00000000001a"" model=""[SelectableQFModel]"" order=""9"" />
  <item text=""312d1c21-0000-0000-0000-00000000001c"" model=""[SelectableQFModel]"" order=""10"" />
  <item text=""312d1c21-0000-0000-0000-00000000001e"" model=""[SelectableQFModel]"" order=""11"" />
</model>"
                ;

                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting 12 animal matches."
                );

                // As a product of the CollectionChangedEvent this
                // is representative of what we'd see in the visible list.
                actual = string.Join(Environment.NewLine, newItems.Cast<object>().Select(_ => _.ToString()));
                actual.ToClipboardExpected();
                actual.ToClipboardExpected();
                { }
                var newItemsPayload = @" 
Black Cat  [animal] [color]
Orange Fox  [animal] [color]
White Rabbit ""bunny"",""soft"",""jump"" [animal] [color]
Gray Wolf ""pack"",""howl"",""wild"" [animal] [color]
Golden Lion  [animal] [color]
Brown Bear ""strong"",""wild"",""forest"" [animal] [color]
Black Panther ""stealthy"",""feline"",""night"" [animal] [color]
Elephant ""trunk"",""herd"",""safari"" [animal]
Giraffe  [animal]
Kangaroo ""bounce"",""outback"",""marsupial"" [animal]
Turtle  [animal]
Should NOT match an expression with an ""animal"" tag.  [not animal]"
                ;

                Assert.AreEqual(
                    newItemsPayload.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting new items payload as reported."
                );

                items.InputText += " ca";
                await items;
                { }
            }

            /// <summary>
            /// Verifies that appending new records and requerying transitions into filtering mode.
            /// </summary>
            /// <remarks>
            /// - Clearing and repopulating the dataset establishes a new canonical 
            ///   baseline and enters Armed filtering state with correct results.
            /// - Confirms that attempting Commit while IsFiltering is true produces a
            ///   soft advisory without mutating state. 
            /// - After awaiting stabilization, the filtered projection reflects the expected narrowed resultset.
            /// </remarks>
            async Task subtestAppendDatabaseAndRequery()
            {
                items.Clear(all: true);
                // Live-demo specific.
                localAddToDatabase("Appetizer Plate", "[dish]", false, new() { "starter", "appealing", "snack" });
                localAddToDatabase("Errata", "[notes]", false, new() { "crunchy", "green", "appended" });
                localAddToDatabase("Happy Camper", "[phrase]", false, new() { "joyful", "camp", "approach-west" });
                localAddToDatabase("Great example - Markdown Demo", "[app] [portable]", false, new() { "digital", "mobile", "software" });
                localAddToDatabase("Application Form", "[document]", false, new() { "paperwork", "apply" });
                localAddToDatabase("App Store", "[app]", false, new() { "digital", "mobile", "software" });

                nsb.InputText = "app gre";
                items.Commit();
                await items;

                actual = string.Join(Environment.NewLine, items.Select(_ => _.ToString()));
                expected = @" 
Green Apple ""tart"",""snack"",""healthy"" [fruit] [color]
Errata ""crunchy"",""green"",""appended"" [notes]
Great example - Markdown Demo ""digital"",""mobile"",""software"" [app] [portable]"
                ;

                Assert.AreEqual(SearchEntryState.QueryCompleteWithResults, items.SearchEntryState);
                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting items to match"
                );

                actual = items.StateReport();
                actual.ToClipboardExpected();
                { }
                expected = @" 
[IME Len: 7, IsFiltering: True], [Net: null, CC: 3, PMC: 3], [QueryAndFilter: SearchEntryState.QueryCompleteWithResults, FilteringState.Armed]"
                ;
                Assert.AreEqual(expected.NormalizeResult(), actual.NormalizeResult(), "Expecting State Report to match.");

                // Perform a filter
                nsb.InputText = "[app] gre";

                #region L o c a l F x
                var builderThrow = new List<string>();
                void localOnBeginThrowOrAdvise(object? sender, Throw e)
                {
                    builderThrow.Add($"{e.Mode}: {e.Message}");
                    e.Handled = true;
                }
                #endregion L o c a l F x
                using (this.WithOnDispose(
                    onInit: (sender, e) =>
                    {
                        Throw.BeginThrowOrAdvise += localOnBeginThrowOrAdvise;
                    },
                    onDispose: (sender, e) =>
                    {
                        Throw.BeginThrowOrAdvise -= localOnBeginThrowOrAdvise;
                    }))
                {
                    items.Commit();

                    actual = string.Join(Environment.NewLine, builderThrow);
                    actual.ToClipboardExpected();
                    { }
                    expected = @" 
ThrowSoft: Commit cannot execute while IsFiltering is true. Caller must ensure filtering is not active before invoking Commit.";

                    Assert.AreEqual(
                        expected.NormalizeResult(),
                        actual.NormalizeResult(),
                        "Expecting soft throw."
                    );
                }
                await items;

                actual = string.Join(Environment.NewLine, items.Select(_ => _.ToString()));
                actual.ToClipboardExpected();
                expected = @" 
Great example - Markdown Demo ""digital"",""mobile"",""software"" [app] [portable]"
                ;
                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting items to match"
                );
            }

            async Task subtest_Animals()
            {
                Assert.AreNotEqual(0, items.CanonicalCount, "Expecting carry-over from previous subtest.");
                // No surprises.
                builder.Clear();
                items.Clear();

                actual = string.Join(Environment.NewLine, builder);
                actual.ToClipboardExpected();
                { }
                expected = @" 
NetProjection.Reset   NotifyCollectionChangedEventArgs           "
                ;

                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting RESET event."
                );

                actual = items.StateReport();
                actual.ToClipboardExpected();
                { }
                expected = @" 
[IME Len: 0, IsFiltering: False], [Net: null, CC: 0, PMC: 0], [QueryAndFilter: SearchEntryState.Cleared, FilteringState.Ineligible]";

                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting NO SURPRISES CLEAR."
                );

                Assert.AreEqual(
                    false,
                    items.Settings[StdMarkdownContextSetting.AllowPluralize], 
                    "Expecting object? that is a bool set to false.");

                items.InputText = "animals";
                items.Commit();

                actual = items.StateReport();
                actual.ToClipboardExpected();
                { }
                expected = @" 
[IME Len: 7, IsFiltering: False], [Net: null, CC: 0, PMC: 0], [QueryAndFilter: SearchEntryState.QueryCompleteNoResults, FilteringState.Ineligible]"
                ;

                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting empty due to plural."
                );

                items.Settings[StdMarkdownContextSetting.AllowPluralize] = true;
                items.Commit();

                actual = items.StateReport();
                actual.ToClipboardExpected();
                { }
                expected = @" 
[IME Len: 7, IsFiltering: True], [Net: null, CC: 12, PMC: 12], [QueryAndFilter: SearchEntryState.QueryCompleteWithResults, FilteringState.Armed]"
                ;

                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting CC: 12 due to fuzzy query enabled by setting."
                );

                items.InputText += " ";
                await items;

                actual = items.StateReport();
                actual.ToClipboardExpected();
                { }
                expected = @" 
[IME Len: 8, IsFiltering: True], [Net: null, CC: 12, PMC: 12], [QueryAndFilter: SearchEntryState.QueryCompleteWithResults, FilteringState.Active]"
                ;

                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting -> FilteringState.ACTIVE after APPEND SPACE CHARACTER."
                );

                items.InputText += "c";
                await items;

                actual = items.Model.ToString();
                actual.ToClipboardExpected();
                { }
                expected = @" 
<model mdc=""[MDC]"" histo=""[model:12 match:9 qmatch:9 pmatch:0]"" filters=""[No Active Filters]"">
  <item text=""312d1c21-0000-0000-0000-000000000005"" model=""[SelectableQFModel]"" order=""0"" qmatch=""True"" match=""True"" />
  <item text=""312d1c21-0000-0000-0000-000000000006"" model=""[SelectableQFModel]"" order=""1"" qmatch=""True"" match=""True"" />
  <item text=""312d1c21-0000-0000-0000-000000000007"" model=""[SelectableQFModel]"" order=""2"" qmatch=""True"" match=""True"" />
  <item text=""312d1c21-0000-0000-0000-000000000009"" model=""[SelectableQFModel]"" order=""3"" qmatch=""True"" match=""True"" />
  <item text=""312d1c21-0000-0000-0000-00000000000b"" model=""[SelectableQFModel]"" order=""4"" qmatch=""True"" match=""True"" />
  <item text=""312d1c21-0000-0000-0000-00000000000c"" model=""[SelectableQFModel]"" order=""5"" qmatch=""True"" match=""True"" />
  <item text=""312d1c21-0000-0000-0000-00000000000f"" model=""[SelectableQFModel]"" order=""6"" qmatch=""True"" match=""True"" />
  <item text=""312d1c21-0000-0000-0000-000000000014"" model=""[SelectableQFModel]"" order=""7"" />
  <item text=""312d1c21-0000-0000-0000-000000000018"" model=""[SelectableQFModel]"" order=""8"" />
  <item text=""312d1c21-0000-0000-0000-00000000001a"" model=""[SelectableQFModel]"" order=""9"" qmatch=""True"" match=""True"" />
  <item text=""312d1c21-0000-0000-0000-00000000001c"" model=""[SelectableQFModel]"" order=""10"" />
  <item text=""312d1c21-0000-0000-0000-00000000001e"" model=""[SelectableQFModel]"" order=""11"" qmatch=""True"" match=""True"" />
</model>"
                ;

                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting that APPLY FILTER will have identified matches."
                );

                actual = items.StateReport();
                actual.ToClipboardExpected();
                { }
                expected = @" 
[IME Len: 9, IsFiltering: True], [Net: null, CC: 12, PMC: 9], [QueryAndFilter: SearchEntryState.QueryCompleteWithResults, FilteringState.Active]"
                ;

                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting NINE matches. This only works if using FUZZY QUERY because of the 'animals' term -> 'animal'."
                );

                // Input text = animals ca
                items.InputText += "a";
                await items;

                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting result to match."
                );

                actual = items.ToString(ReportFormat.ModelWithPreview);
                actual.ToClipboardExpected();
                { }
                expected = @" 
<model mdc=""[MDC]"" histo=""[model:12 match:1 qmatch:1 pmatch:0]"" filters=""[No Active Filters]"">
  <item text=""312d1c21-0000-0000-0000-000000000005"" model=""[SelectableQFModel]"" order=""0"" qmatch=""True"" match=""True"" preview=""Black Cat "" />
  <item text=""312d1c21-0000-0000-0000-000000000006"" model=""[SelectableQFModel]"" order=""1"" preview=""Orange Fox"" />
  <item text=""312d1c21-0000-0000-0000-000000000007"" model=""[SelectableQFModel]"" order=""2"" preview=""White Rabb"" />
  <item text=""312d1c21-0000-0000-0000-000000000009"" model=""[SelectableQFModel]"" order=""3"" preview=""Gray Wolf "" />
  <item text=""312d1c21-0000-0000-0000-00000000000b"" model=""[SelectableQFModel]"" order=""4"" preview=""Golden Lio"" />
  <item text=""312d1c21-0000-0000-0000-00000000000c"" model=""[SelectableQFModel]"" order=""5"" preview=""Brown Bear"" />
  <item text=""312d1c21-0000-0000-0000-00000000000f"" model=""[SelectableQFModel]"" order=""6"" preview=""Black Pant"" />
  <item text=""312d1c21-0000-0000-0000-000000000014"" model=""[SelectableQFModel]"" order=""7"" preview=""Elephant  "" />
  <item text=""312d1c21-0000-0000-0000-000000000018"" model=""[SelectableQFModel]"" order=""8"" preview=""Giraffe   "" />
  <item text=""312d1c21-0000-0000-0000-00000000001a"" model=""[SelectableQFModel]"" order=""9"" preview=""Kangaroo  "" />
  <item text=""312d1c21-0000-0000-0000-00000000001c"" model=""[SelectableQFModel]"" order=""10"" preview=""Turtle    "" />
  <item text=""312d1c21-0000-0000-0000-00000000001e"" model=""[SelectableQFModel]"" order=""11"" preview=""Should NOT"" />
</model>"
                ;

                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting that THE FIRST ITEM is a match. This is IMPORTANT to explain the result below."
                );

                // ☆☆☆☆☆
                // Extension : Model the Model (with active filter) from the OUTSIDE LOOKING IN.
                // ☆☆☆☆☆
                actual = items.ToString(out XElement _);
                actual.ToClipboardExpected();
                { }
                expected = @" 
<model mpath=""Id"">
  <item text=""312d1c21-0000-0000-0000-000000000005"" model=""[SelectableQFModel]"" order=""0"" preview=""Black Cat "" />
</model>"
                ;
                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting that EXTENSION USES THE ROUTED ITERATOR."
                );

                actual = items.StateReport();
                actual.ToClipboardExpected();
                { }
                expected = @" 
[IME Len: 9, IsFiltering: True], [Net: null, CC: 12, PMC: 1], [QueryAndFilter: SearchEntryState.QueryCompleteWithResults, FilteringState.Active]"
                ;

                Assert.IsFalse(items.RouteToFullRecordset);
                Assert.AreEqual(
                    1,
                    items.Count,
                    "Expecting FILTERED.");

                // And now, a BUGIRL.
                // This is supposed to show all the items once again.
                builder.Clear();
                items.Clear(false);

                // The BUGIRL is that there was no Reset or Change event.
                actual = string.Join(Environment.NewLine, builder);
                actual.ToClipboardExpected();
                { }

                // IN THE PROCESS OF FIXING THAT BUG, WE ACCIDENTALLY PROVED SOMETHING COOL
                // - The expectation was for a Reset.
                // - IT WILL ALMOST ALWAYS BE A RESET.
                // - But just as a rando thing, IN THIS CORNER CASE the Diff turned out to be BCL compatible.
                // - The reason: The single PM is at index ZERO.
                // And the thing is, it worked exactly how we designed it.
                expected = @" 
NetProjection.Add     NewItems=11 NewStartingIndex= 0 NotifyCollectionChangedEventArgs           "
                ;

                // ☆☆☆☆☆
                // Extension : Model (with cleared filter) from the OUTSIDE LOOKING IN.
                // ☆☆☆☆☆
                actual = items.ToString(out XElement _);
                actual.ToClipboardExpected();
                { }
                expected = @" 
<model mpath=""Id"">
  <item text=""312d1c21-0000-0000-0000-000000000005"" model=""[SelectableQFModel]"" order=""0"" preview=""Black Cat "" />
  <item text=""312d1c21-0000-0000-0000-000000000006"" model=""[SelectableQFModel]"" order=""1"" preview=""Orange Fox"" />
  <item text=""312d1c21-0000-0000-0000-000000000007"" model=""[SelectableQFModel]"" order=""2"" preview=""White Rabb"" />
  <item text=""312d1c21-0000-0000-0000-000000000009"" model=""[SelectableQFModel]"" order=""3"" preview=""Gray Wolf "" />
  <item text=""312d1c21-0000-0000-0000-00000000000b"" model=""[SelectableQFModel]"" order=""4"" preview=""Golden Lio"" />
  <item text=""312d1c21-0000-0000-0000-00000000000c"" model=""[SelectableQFModel]"" order=""5"" preview=""Brown Bear"" />
  <item text=""312d1c21-0000-0000-0000-00000000000f"" model=""[SelectableQFModel]"" order=""6"" preview=""Black Pant"" />
  <item text=""312d1c21-0000-0000-0000-000000000014"" model=""[SelectableQFModel]"" order=""7"" preview=""Elephant  "" />
  <item text=""312d1c21-0000-0000-0000-000000000018"" model=""[SelectableQFModel]"" order=""8"" preview=""Giraffe   "" />
  <item text=""312d1c21-0000-0000-0000-00000000001a"" model=""[SelectableQFModel]"" order=""9"" preview=""Kangaroo  "" />
  <item text=""312d1c21-0000-0000-0000-00000000001c"" model=""[SelectableQFModel]"" order=""10"" preview=""Turtle    "" />
  <item text=""312d1c21-0000-0000-0000-00000000001e"" model=""[SelectableQFModel]"" order=""11"" preview=""Should NOT"" />
</model>"
                ;
                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting that EXTENSION USES THE ROUTED ITERATOR."
                );

                actual = items.StateReport();
                actual.ToClipboardExpected();
                { }
                expected = @" 
[IME Len: 0, IsFiltering: True], [Net: null, CC: 12, PMC: 1], [QueryAndFilter: SearchEntryState.QueryCompleteWithResults, FilteringState.Armed]"
                ;
                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting full list shown after IME CLEAR but still PMC: 1 because there's no new apply filter."
                );

                Assert.IsTrue(items.RouteToFullRecordset);
                Assert.AreEqual(12, items.Count, "Expecting routing to track via the internal Read property.");

                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting ADD." +
                    "- This is because the PMC item is the first on the list." +
                    " ∴ when we take a DIFF, the delta is contiguous IN THIS CASE."
                );

                // Now force an change event that is not contiguous.
                items.InputText = "brown&bear";
                await items;

                actual = items.ToString(out XElement _);
                actual.ToClipboardExpected();
                { }
                expected = @" 
<model mpath=""Id"">
  <item text=""312d1c21-0000-0000-0000-00000000000c"" model=""[SelectableQFModel]"" order=""0"" preview=""Brown Bear"" />
</model>"
                ;


                actual = items.ToString(ReportFormat.ModelWithPreview);
                actual.ToClipboardExpected();
                { }
                expected = @" 
<model mdc=""[MDC]"" histo=""[model:12 match:1 qmatch:1 pmatch:0]"" filters=""[No Active Filters]"">
  <item text=""312d1c21-0000-0000-0000-000000000005"" model=""[SelectableQFModel]"" order=""0"" preview=""Black Cat "" />
  <item text=""312d1c21-0000-0000-0000-000000000006"" model=""[SelectableQFModel]"" order=""1"" preview=""Orange Fox"" />
  <item text=""312d1c21-0000-0000-0000-000000000007"" model=""[SelectableQFModel]"" order=""2"" preview=""White Rabb"" />
  <item text=""312d1c21-0000-0000-0000-000000000009"" model=""[SelectableQFModel]"" order=""3"" preview=""Gray Wolf "" />
  <item text=""312d1c21-0000-0000-0000-00000000000b"" model=""[SelectableQFModel]"" order=""4"" preview=""Golden Lio"" />
  <item text=""312d1c21-0000-0000-0000-00000000000c"" model=""[SelectableQFModel]"" order=""5"" qmatch=""True"" match=""True"" preview=""Brown Bear"" />
  <item text=""312d1c21-0000-0000-0000-00000000000f"" model=""[SelectableQFModel]"" order=""6"" preview=""Black Pant"" />
  <item text=""312d1c21-0000-0000-0000-000000000014"" model=""[SelectableQFModel]"" order=""7"" preview=""Elephant  "" />
  <item text=""312d1c21-0000-0000-0000-000000000018"" model=""[SelectableQFModel]"" order=""8"" preview=""Giraffe   "" />
  <item text=""312d1c21-0000-0000-0000-00000000001a"" model=""[SelectableQFModel]"" order=""9"" preview=""Kangaroo  "" />
  <item text=""312d1c21-0000-0000-0000-00000000001c"" model=""[SelectableQFModel]"" order=""10"" preview=""Turtle    "" />
  <item text=""312d1c21-0000-0000-0000-00000000001e"" model=""[SelectableQFModel]"" order=""11"" preview=""Should NOT"" />
</model>"
                ;

                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting ONE MATCH that is NOT THE FIRST ITEM."
                );

                actual = items.ToString(ReportFormat.StateReport);
                actual.ToClipboardExpected();
                { }
                expected = @" 
[IME Len: 10, IsFiltering: True], [Net: null, CC: 12, PMC: 1], [QueryAndFilter: SearchEntryState.QueryCompleteWithResults, FilteringState.Active]"
                ;
                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting standard one-item result."
                );

                Assert.IsFalse(items.RouteToFullRecordset);
                Assert.AreEqual(
                    1,
                    items.Count,
                    "Expecting FILTERED.");

                // So, back to that BUGIRL that was mentioned:
                // - Check for event
                // - Expect it to be discontiguous OR for it to arrive as a reset.
                builder.Clear();
                items.Clear(false);

                actual = items.StateReport();
                actual.ToClipboardExpected();
                { }
                expected = @" 
[IME Len: 0, IsFiltering: True], [Net: null, CC: 12, PMC: 1], [QueryAndFilter: SearchEntryState.QueryCompleteWithResults, FilteringState.Armed]"
                ;
                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting full list shown after IME CLEAR but still PMC: 1 because there's no new apply filter."
                );

                Assert.IsTrue(items.RouteToFullRecordset);
                Assert.AreEqual(
                    12,
                    items.Count,
                    "Expecting FULL.");

                // Once again, the BUGIRL is that there was no Reset or Change event.
                // - This time it *will* be a Reset for the CollectionChanged.
                // - But we expect to see a playlist in the CollectionChanging.
                actual = string.Join(Environment.NewLine, builder);
                actual.ToClipboardExpected();
                { }
                expected = @" 
NetProjection.Reset   NotifyCollectionChangedEventArgs           "
                ;

                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting RESET (because it's a mixed message)."
                );

                actual = items.StateReport();
                actual.ToClipboardExpected();
                { }
                expected = @" 
[IME Len: 0, IsFiltering: True], [Net: null, CC: 12, PMC: 1], [QueryAndFilter: SearchEntryState.QueryCompleteWithResults, FilteringState.Armed]"
                ;
                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting full list shown after IME CLEAR but still PMC: 1 because there's no new apply filter."
                );

                Assert.IsTrue(items.RouteToFullRecordset);
                Assert.AreEqual(
                    12, 
                    items.Count,
                    "Expecting FULL.");

                items.InputText = "rabbit|wolf";
                await items;

                actual = items.ToString(out XElement _);
                actual.ToClipboardExpected();
                { }
                expected = @" 
<model mpath=""Id"">
  <item text=""312d1c21-0000-0000-0000-000000000007"" model=""[SelectableQFModel]"" order=""0"" preview=""White Rabb"" />
  <item text=""312d1c21-0000-0000-0000-000000000009"" model=""[SelectableQFModel]"" order=""1"" preview=""Gray Wolf "" />
</model>"
                ;
            }
            #endregion S U B T E S T S

            #region L o c a l F x
            void localAddToDatabase(string description, string tags, bool isChecked, List<string>? keywords = null)
            {
                var instance = new SelectableQFModelLTOQO();
                var type = typeof(SelectableQFModelLTOQO);
                type.GetProperty("Description")?.SetValue(instance, description);
                type.GetProperty("Tags")?.SetValue(instance, tags);
                type.GetProperty("IsChecked")?.SetValue(instance, isChecked);
                if (keywords != null)
                {
                    var json = JsonConvert.SerializeObject(keywords);
                    type.GetProperty("Keywords")?.SetValue(instance, json);
                }
                items.MemoryDatabase.Insert(instance);
            }
            #endregion L o c a l F x
        }
    }
}