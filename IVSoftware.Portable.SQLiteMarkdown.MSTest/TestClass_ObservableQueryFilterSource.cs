using IVSoftware.Portable.SQLiteMarkdown.Collections;
using IVSoftware.Portable.SQLiteMarkdown.MSTest.DemoDB;
using IVSoftware.Portable.SQLiteMarkdown.MSTest.Models;
using IVSoftware.Portable.SQLiteMarkdown.MSTest.Models.DemoDB;
using IVSoftware.Portable.SQLiteMarkdown.MSTest.Models.QFTemplates;
using IVSoftware.Portable.Threading;
using IVSoftware.Portable.Xml.Linq.XBoundObject;
using IVSoftware.Portable.Xml.Linq.XBoundObject.Modeling;
using IVSoftware.WinOS.MSTest.Extensions;
using Microsoft.VisualBasic.Logging;
using Newtonsoft.Json;
using SQLite;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Dynamic;
using System.Net.Http.Headers;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using static SQLite.SQLite3;
using static System.Net.Mime.MediaTypeNames;
using Ignore = Microsoft.VisualStudio.TestTools.UnitTesting.IgnoreAttribute;
using static IVSoftware.Portable.Threading.Extensions;

namespace IVSoftware.Portable.SQLiteMarkdown.MSTest
{   
    // Namespace with test-only classes.
    namespace DemoDB
    {
        public class NavSearchBar : INotifyPropertyChanged
        {
            public IList<SelectableQFModelTOQO>? ItemsSource
            {
                get => _itemsSource;
                set
                {
                    if (!Equals(_itemsSource, value))
                    {
                        INotifyCollectionChanged? incc = _itemsSource as INotifyCollectionChanged;
                        if(_itemsSource is not null)
                        {
                            if(incc is not null)
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

            IList<SelectableQFModelTOQO>? _itemsSource = null;
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
            var items = new List<SelectableQFModelTOQO>
            {
                new SelectableQFModelTOQO
                {
                    Description = "Brown Dog",
                    Tags = "[canine] [color] [atomic tag]",
                    IsChecked = false,
                    Keywords = JsonConvert.SerializeObject(new List<string> { "loyal", "friend", "furry" })
                },
            };
            var imdb = new SQLiteConnection(":memory:");
            imdb.CreateTable<SelectableQFModelTOQO>();
            imdb.InsertAll(items);
            return imdb;
        }
        private SQLiteConnection InitializeInMemoryDatabase()
        {
            var items = new List<SelectableQFModelTOQO>
            {
                new SelectableQFModelTOQO
                {
                    Description = "Brown Dog",
                    Tags = "[canine] [color]",
                    IsChecked = false,
                    Keywords = JsonConvert.SerializeObject(new List<string> { "loyal", "friend", "furry" })
                },
                new SelectableQFModelTOQO
                {
                    Description = "Green Apple",
                    Tags = "[fruit] [color]",
                    IsChecked = false,
                    Keywords = JsonConvert.SerializeObject(new List<string> { "tart", "snack", "healthy" })
                },
                new SelectableQFModelTOQO { Description = "Yellow Banana", Tags = "[fruit] [color]", IsChecked = false },
                new SelectableQFModelTOQO
                {
                    Description = "Blue Bird",
                    Tags = "[bird] [color]",
                    IsChecked = false,
                    Keywords = JsonConvert.SerializeObject(new List<string> { "sky", "feathered", "song" })
                },
                new SelectableQFModelTOQO
                {
                    Description = "Red Cherry",
                    Tags = "[fruit] [color]",
                    IsChecked = false,
                    Keywords = JsonConvert.SerializeObject(new List<string> { "sweet", "summer", "dessert" })
                },
                new SelectableQFModelTOQO { Description = "Black Cat", Tags = "[animal] [color]", IsChecked = false },
                new SelectableQFModelTOQO { Description = "Orange Fox", Tags = "[animal] [color]", IsChecked = false },
                new SelectableQFModelTOQO
                {
                    Description = "White Rabbit",
                    Tags = "[animal] [color]",
                    IsChecked = false,
                    Keywords = JsonConvert.SerializeObject(new List<string> { "bunny", "soft", "jump" })
                },
                new SelectableQFModelTOQO { Description = "Purple Grape", Tags = "[fruit] [color]", IsChecked = false },
                new SelectableQFModelTOQO
                {
                    Description = "Gray Wolf",
                    Tags = "[animal] [color]",
                    IsChecked = false,
                    Keywords = JsonConvert.SerializeObject(new List<string> { "pack", "howl", "wild" })
                },
                new SelectableQFModelTOQO { Description = "Pink Flamingo", Tags = "[bird] [color]", IsChecked = false },
                new SelectableQFModelTOQO { Description = "Golden Lion", Tags = "[animal] [color]", IsChecked = false },
                new SelectableQFModelTOQO
                {
                    Description = "Brown Bear",
                    Tags = "[animal] [color]",
                    IsChecked = false,
                    Keywords = JsonConvert.SerializeObject(new List<string> { "strong", "wild", "forest" })
                },
                new SelectableQFModelTOQO { Description = "Green Pear", Tags = "[fruit] [color]", IsChecked = false },
                new SelectableQFModelTOQO { Description = "Red Strawberry", Tags = "[fruit] [color]", IsChecked = false },
                new SelectableQFModelTOQO
                {
                    Description = "Black Panther",
                    Tags = "[animal] [color]",
                    IsChecked = false,
                    Keywords = JsonConvert.SerializeObject(new List<string> { "stealthy", "feline", "night" })
                },
                new SelectableQFModelTOQO { Description = "Yellow Lemon", Tags = "[fruit] [color]", IsChecked = false },
                new SelectableQFModelTOQO { Description = "White Swan", Tags = "[bird] [color]", IsChecked = false },
                new SelectableQFModelTOQO { Description = "Purple Plum", Tags = "[fruit] [color]", IsChecked = false },
                new SelectableQFModelTOQO
                {
                    Description = "Blue Whale",
                    Tags = "[marine-mammal] [ocean]",
                    IsChecked = false,
                    Keywords = JsonConvert.SerializeObject(new List<string> { "ocean", "mammal", "giant" })
                },
                new SelectableQFModelTOQO
                {
                    Description = "Elephant",
                    Tags = "[animal]",
                    IsChecked = false,
                    Keywords = JsonConvert.SerializeObject(new List<string> { "trunk", "herd", "safari" })
                },
                new SelectableQFModelTOQO { Description = "Pineapple", Tags = "[fruit]", IsChecked = false },
                new SelectableQFModelTOQO { Description = "Shark", Tags = "[fish]", IsChecked = false },
                new SelectableQFModelTOQO { Description = "Owl", Tags = "[bird]", IsChecked = false },
                new SelectableQFModelTOQO { Description = "Giraffe", Tags = "[animal]", IsChecked = false },
                new SelectableQFModelTOQO { Description = "Coconut", Tags = "[fruit]", IsChecked = false },
                new SelectableQFModelTOQO
                {
                    Description = "Kangaroo",
                    Tags = "[animal]",
                    IsChecked = false,
                    Keywords = JsonConvert.SerializeObject(new List<string> { "bounce", "outback", "marsupial" })
                },
                new SelectableQFModelTOQO { Description = "Dragonfruit", Tags = "[fruit]", IsChecked = false },
                new SelectableQFModelTOQO { Description = "Turtle", Tags = "[animal]", IsChecked = false },
                new SelectableQFModelTOQO { Description = "Mango", Tags = "[fruit]", IsChecked = false },
                new SelectableQFModelTOQO { Description = "Should NOT match an expression with an \"animal\" tag.", Tags = "[not animal]", IsChecked = false }
            };
            var imdb = new SQLiteConnection(":memory:");
            imdb.CreateTable<SelectableQFModelTOQO>();
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

            imdb.InsertAll(list);
            return imdb;
        }

        private SQLiteConnection InitializeInMemoryDatabaseAtomicQuotes()
        {
            var items = new List<SelectableQFModelTOQO>
            {
            };
            var imdb = new SQLiteConnection(":memory:");
            imdb.CreateTable<SelectableQFModelTOQO>();
            imdb.InsertAll(items);
            return imdb;
        }

        [TestMethod]
        public void Test_ViewDatabase()
        {
            string actual, expected;
            using (var cnx = InitializeInMemoryDatabase())
            {
                var allRecords = cnx.Query<SelectableQFModelTOQO>("Select * from items");
                actual = string.Join(Environment.NewLine, allRecords.Select(_ => _.ToString()));
                actual.ToClipboard();
                actual.ToClipboardExpected();
                actual.ToClipboardAssert();
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
            List<SelectableQFModelTOQO> results;
            var builder = new List<string>();
            List<SelectableQFModelTOQO> unfiltered = new List<SelectableQFModelTOQO>();
            List<SelectableQFModelTOQO> filtered = new List<SelectableQFModelTOQO>();
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
                    actual = "animal".ParseSqlMarkdown<SelectableQFModelTOQO>();
					expected = @" 
SELECT * FROM items WHERE (QueryTerm LIKE '%animal%')"
					;

					Assert.AreEqual(
                        expected.NormalizeResult(),
                        actual.NormalizeResult(),
                        "Expecting query mode expression with QueryTerm indexed only."
                    );

					actual = "animal".ParseSqlMarkdown<SelectableQFModelTOQO>(QueryFilterMode.Filter);
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
                    unfiltered = cnx.Query<SelectableQFModelTOQO>("Select * from items");
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
                    actual = "bro".ParseSqlMarkdown<SelectableQFModelTOQO>();
					expected = @" 
SELECT * FROM items WHERE (QueryTerm LIKE '%bro%')"
					;

					Assert.AreEqual(
                        expected.NormalizeResult(),
                        actual.NormalizeResult(),
                        "Expecting correct expr"
                    );
                    Assert.IsNotNull(cnx.Query<SelectableQFModelTOQO>(actual).SingleOrDefault());

                    actual = "bro dog".ParseSqlMarkdown<SelectableQFModelTOQO>();
                    Assert.IsNotNull(cnx.Query<SelectableQFModelTOQO>(actual).SingleOrDefault());

                    actual = "brown furry".ParseSqlMarkdown<SelectableQFModelTOQO>();
					expected = @" 
SELECT * FROM items WHERE (QueryTerm LIKE '%brown%') AND (QueryTerm LIKE '%furry%')"
					;
                    Assert.IsNotNull(cnx.Query<SelectableQFModelTOQO>(actual).SingleOrDefault());
                    actual = "brown !dog".ParseSqlMarkdown<SelectableQFModelTOQO>();
                    
                    actual.ToClipboardExpected();
                    // IS null
                    Assert.IsNull(cnx.Query<SelectableQFModelTOQO>(actual).SingleOrDefault());
                }
                void subtestApplyFilter()
                {
                    using (var subcnx = new SQLiteConnection(":memory:"))
                    {
                        subcnx.CreateTable<SelectableQFModelTOQO>();
                        subcnx.InsertAll(unfiltered);
                        builder.Clear();

                        actual = "brown furry".ParseSqlMarkdown<SelectableQFModelTOQO>(QueryFilterMode.Filter);
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
                        filtered = subcnx.Query<SelectableQFModelTOQO>(actual);

                        Assert.IsNotNull(filtered.SingleOrDefault());
                        actual = "brown canine".ParseSqlMarkdown<SelectableQFModelTOQO>(QueryFilterMode.Filter);
                        filtered = subcnx.Query<SelectableQFModelTOQO>(actual);
                        // IS null...
                        Assert.IsNull(filtered.SingleOrDefault());
                    }
                }
#if ABSTRACT
            
                expected = @" 
Description    =""Brown Dog""
Keywords       =""[""loyal"",""friend"",""furry""]""
KeywordsDisplay=""""loyal"",""friend"",""furry""""
Tags           =""[canine] [color]""
TagsDisplay    =""[canine] [color]""
IsChecked      =""False""
FilterValue    =""Brown Dog""
Selection      =""None""
LikeTerm       =""brown~dog~loyal~friend~furry~canine~color""
ContainsTerm   =""brown~dog~loyal~friend~furry""
TagMatchTerm   =""canine~color""
Properties     =""{
  ""Description"": ""Brown Dog"",
  ""Tags"": ""[canine] [color]"",
  ""Keywords"": ""[\""loyal\"",\""friend\"",\""furry\""]""
}""
"
#endif
                #endregion S U B T E S T S
            }
        }

        [TestMethod]
        public void Test_ApplyExprManually()
        {
            string actual, expected, sql;
            List<SelectableQFModelTOQO> results;
            MarkdownContextOR context;
            ValidationState state;

            subtestReport();

            #region S U B T E S T S
            void subtestReport()
            {
                var builder = new List<string>();
                using (var cnx = InitializeInMemoryDatabase())
                {
                    var allRecords = cnx.Query<SelectableQFModelTOQO>("Select * from items");
                    foreach (var item in allRecords)
                    {
                        builder.Add(item.Report());
                        builder.Add(string.Empty);
                    }
                }

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
QueryTerm      =""should~not~match~an~expression~with~animal~tag.~[not animal]""
FilterTerm     =""should~not~match~an~expression~with~animal~tag.""
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
            #endregion S U B T E S T S
        }

        [TestMethod]
        public void Test_DefaultExpressions()
        {
            string actual, expected, sql;
            List<SelectableQFModelTOQO> results;
            MarkdownContextOR context;
            ValidationState state;
            using (var cnx = InitializeInMemoryDatabase())
            {
                subtestBasicQueryAnimal();
                subtestPluralQueryAnimals();

                #region S U B T E S T S
                void subtestBasicQueryAnimal()
                {
                    sql = "animal".ParseSqlMarkdown<SelectableQFModelTOQO>();

                    actual = sql;
					expected = @" 
SELECT * FROM items WHERE (QueryTerm LIKE '%animal%')"
					;

                    Assert.AreEqual(
                        expected.NormalizeResult(),
                        actual.NormalizeResult(),
                        "Expecting expr does not include a Tag term"
                    );

                    results = cnx.Query<SelectableQFModelTOQO>(sql);

                    actual = string.Join(Environment.NewLine, results.Select(_ => _.ToString()));
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
Should NOT match an expression with an ""animal"" tag.  [not animal]";

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
                        .ParseSqlMarkdown<SelectableQFModelTOQO>();

                    sql = sql.ToFuzzyQuery();

                    results = cnx.Query<SelectableQFModelTOQO>(sql);

                    actual = string.Join(Environment.NewLine, results.Select(_ => _.ToString()));
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
Should NOT match an expression with an ""animal"" tag.  [not animal]";

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
            SenderEventPair sep;
            NotifyCollectionChangedEventArgs ecc;
            Queue<SenderEventPair> eventQueue = new();
            List<SelectableQFModelTOQO> results;
            var itemsSource = new ObservableQueryFilterSource<SelectableQFModelTOQO>();
            void localOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
            {
                eventQueue.Enqueue((sender, e));
            }
            using (var cnx = InitializeInMemoryDatabase())
            {
                subtestBasicQueryAnimal();
                subtestBasicQueryAnimalINPC();

                #region S U B T E S T S
                void subtestBasicQueryAnimal()
                {
                    sql = "animal".ParseSqlMarkdown<SelectableQFModelTOQO>();
                    results = cnx.Query<SelectableQFModelTOQO>(sql);
                    itemsSource.ReplaceItems(results);
                    actual = string.Join(Environment.NewLine, itemsSource.Select(_ => _.ToString()));
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
Should NOT match an expression with an ""animal"" tag.  [not animal]";

                    Assert.AreEqual(
                        expected.NormalizeResult(),
                        actual.NormalizeResult(),
                        "Expecting filtered results to match."
                    );
                }
                void subtestBasicQueryAnimalINPC()
                {
                    itemsSource.Clear();
                    try
                    {
                        itemsSource.CollectionChanged += localOnCollectionChanged;
                        sql = "animal".ParseSqlMarkdown<SelectableQFModelTOQO>();
                        results = cnx.Query<SelectableQFModelTOQO>(sql);
                        itemsSource.ReplaceItems(results);

                        actual =
                            string
                            .Join(Environment.NewLine, eventQueue.Select(_ => _.ToStringFromEventType()));
                        actual.ToClipboardExpected();
                        { }
                        expected = @"
Reset
Add"
                        ;

                        ecc = (NotifyCollectionChangedEventArgs)eventQueue.Dequeue().e;
                        Assert.AreEqual(
                            NotifyCollectionChangedAction.Reset,
                            ecc.Action);
                        { }
                        ecc = (NotifyCollectionChangedEventArgs)eventQueue.DequeueSingle().e;
                        Assert.AreEqual(
                            NotifyCollectionChangedAction.Add,
                            ecc.Action);

                        actual = string.Join(
                            Environment.NewLine,
                            ecc
                            .NewItems?.OfType<SelectableQFModelTOQO>()
                            .Select(_ => _.ToString()) ?? []);

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
Should NOT match an expression with an ""animal"" tag.  [not animal]";
                    }
                    finally
                    {
                        itemsSource.CollectionChanged -= localOnCollectionChanged;
                    }
                }
                #endregion S U B T E S T S
            }
        }

        [TestMethod]
        public async Task Test_TrackProgressiveInputState()
        {
            var id1 = Thread.CurrentThread.ManagedThreadId;

            string actual, expected, sql;
            NotifyCollectionChangedEventArgs ecc;
            PropertyChangedEventArgs epc;

            // Test for early adopter (beta) migration support.
            await localTest<SelectableQueryModelOR>();

            // Test for current version scheme
            await localTest<SelectableQFModelTOQO>();

            async Task localTest<T>() where T : new()
            {
                @"\& \| \! \( \) \[ \] \' \"" \\".ParseSqlMarkdown<PetProfileN>();
                Queue<SenderEventPair> eventQueue = new();
                List<T> recordset;
                var items = new ObservableQueryFilterSource<T>();
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

                items.PropertyChanged += (sender, e) =>
                {
                    eventQueue.Enqueue((sender, e));
                };
                using (var cnx = InitializeInMemoryDatabase())
                {
                    subtestClearedToFirstChar();
                    subtestFirstCharToEmpty();
                    subtestEmptyToFirstChar();
                    subtestSecondChar();
                    subtestThirdCharEnableQuery();
                    await subtestCommit();

                    #region S U B T E S T S

                    void subtestClearedToFirstChar()
                    {
                        Assert.AreEqual(
                            SearchEntryState.Cleared,
                            items.SearchEntryState,
                            "Expecting starting state is Cleared."
                        );
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
InputText
SearchEntryState
Running";

                        Assert.AreEqual(
                            expected.NormalizeResult(),
                            actual.NormalizeResult(),
                            "Expecting specific property changes."
                        );
                        eventQueue.Clear();
                        Assert.AreEqual(
                            SearchEntryState.QueryENB,
                            items.SearchEntryState,
                            "Expecting ending state is ENB."
                        );
                    }

                    void subtestFirstCharToEmpty()
                    {
                        Assert.AreEqual(
                            SearchEntryState.QueryENB,
                            items.SearchEntryState,
                            "Expecting starting state is ENB."
                        );
                        items.InputText = string.Empty;
                        // Empty is distinct from cleared because
                        // a state of cleared takes out the whole list.
                        Assert.AreEqual(
                            SearchEntryState.QueryEmpty,
                            items.SearchEntryState,
                            "Expecting ending state is Empty."
                        );
                        eventQueue.Clear();
                    }

                    void subtestEmptyToFirstChar()
                    {
                        Assert.AreEqual(
                            SearchEntryState.QueryEmpty,
                            items.SearchEntryState,
                            "Expecting starting state is Empty."
                        );
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
InputText
SearchEntryState";

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
InputText
SearchEntryState";

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
                        await items;
                        actual =
                            string
                            .Join(Environment.NewLine, eventQueue.Select(_ => _.e)
                            .OfType<PropertyChangedEventArgs>()
                            .Select(_ => _.PropertyName));
                        actual.ToClipboardExpected();
                        expected = @"
InputText
Running";

                        Assert.AreEqual(
                            expected.NormalizeResult(),
                            actual.NormalizeResult(),
                            "Expecting specific property changes."
                        );
                        eventQueue.Clear();

                        Assert.AreEqual(
                            SearchEntryState.QueryEN,
                            items.SearchEntryState,
                            "Expecting specific state UNCHANGED."
                        );
                        // Based on UI interaction like return key pressed
                        sql = items.InputText.ParseSqlMarkdown<T>();
                        recordset = cnx.Query<T>(sql);
                        await items.ReplaceItemsAsync(recordset);

                        Assert.AreEqual(SearchEntryState.QueryCompleteWithResults, items.SearchEntryState);
                        Assert.AreEqual(FilteringState.Armed, items.FilteringState);
                        Assert.AreNotEqual(0, recordset.Count);
                        Assert.AreNotEqual(0, items.Count);

                        actual = string.Join(Environment.NewLine, items.OfType<object>().Select(_ => _.ToString()));
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
Should NOT match an expression with an ""animal"" tag.  [not animal]";

                        Assert.AreEqual(
                            expected.NormalizeResult(),
                            actual.NormalizeResult(),
                            "Expecting filtered results to match."
                        );
                        actual =
                            string
                            .Join(Environment.NewLine, eventQueue.Select(_ => _.e)
                            .OfType<PropertyChangedEventArgs>()
                            .Select(_ => _.PropertyName));
                        actual.ToClipboardExpected();
                        { }
                        expected = @" 
Busy
SearchEntryState
SearchEntryState
FilteringState
RouteToFullRecordset
IsFiltering
Busy"
                        ;

                        Assert.AreEqual(
                            expected.NormalizeResult(),
                            actual.NormalizeResult(),
                            "Expecting specific property changes."
                        );
                        eventQueue.Clear();

                        Assert.AreEqual(
                            SearchEntryState.QueryCompleteWithResults,
                            items.SearchEntryState,
                            "Expecting notified entry state change."
                        );

                        Assert.AreEqual(
                            FilteringState.Armed,
                            items.FilteringState,
                            "Expecting notified entry state change."
                        );

                        items.Clear();

                        Assert.AreEqual(
                            string.Empty,
                            items.InputText,
                            "Expecting empty input text."
                        );

                        Assert.AreEqual(
                            FilteringState.Armed,
                            items.FilteringState,
                            "Expecting nuanced behavior:"
                        );

#if ABSTRACT
                        // SEE FULL CONTEXT: Canonical {5932CB31-B914-4DE8-9457-7A668CDB7D08}
                        
                        // Basically, if there is entry text but the filtering
                        // is still only armed not active, that indicates that
                        // what we're seeing in the list is the result of a full
                        // db query that just occurred. So now, when we CLEAR that
                        // text, it's assumed to be in the interest of filtering
                        // that query result, so filtering stays Armed in this case.

#endif
                        // animal.b
                        // Expecting Filter mode and an internal query.
                        // See also: {24048258-8BE4-40C4-BF85-8863E98BED51}
                        items.InputText += "b";
                        await items;

                        actual = string.Join(Environment.NewLine, items.Select(_ => _.ToString()));
                        actual.ToClipboardExpected();
                        { }

                        expected = @" 
Black Cat  [animal] [color]
White Rabbit ""bunny"",""soft"",""jump"" [animal] [color]
Brown Bear ""strong"",""wild"",""forest"" [animal] [color]
Black Panther ""stealthy"",""feline"",""night"" [animal] [color]
Kangaroo ""bounce"",""outback"",""marsupial"" [animal]";

                        Assert.AreEqual(
                            expected.NormalizeResult(),
                            actual.NormalizeResult(),
                            "Expecting filtered items containing filter expr"
                        );

                        Assert.IsTrue(items.RouteToFullRecordset);
                        { }
                    }

                    // G Z

                    #endregion S U B T E S T S
                }
            }
        }

        [TestMethod]
        public void Test_FuzzyPlural()
        {
            string actual, expected, sql;
            List<SelectableQFModelTOQO> recordset;
            using (var cnx = InitializeInMemoryDatabase())
            {
                subtestBasicQueryAnimal();
                subtestPluralQueryAnimals();

                #region S U B T E S T S
                void subtestBasicQueryAnimal()
                {
                    sql = "animal".ParseSqlMarkdown<SelectableQFModelTOQO>();

                    actual = sql;
					expected = @" 
SELECT * FROM items WHERE (QueryTerm LIKE '%animal%')"
					;

                    Assert.AreEqual(
                        expected.NormalizeResult(),
                        actual.NormalizeResult(),
                        "Expecting expr does not include a Tag term"
                    );

                    recordset = cnx.Query<SelectableQFModelTOQO>(sql);

                    actual = string.Join(Environment.NewLine, recordset.Select(_ => _.ToString()));
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
Should NOT match an expression with an ""animal"" tag.  [not animal]";

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
                        .ParseSqlMarkdown<SelectableQFModelTOQO>();

                    sql = sql.ToFuzzyQuery();

                    recordset = cnx.Query<SelectableQFModelTOQO>(sql);

                    actual = string.Join(Environment.NewLine, recordset.Select(_ => _.ToString()));
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
Should NOT match an expression with an ""animal"" tag.  [not animal]";

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
            localTest<SelectableQFModelTOQO>();

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

                    recordset =cmd.ExecuteQuery<T>();
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
                actual = "brown dog".ParseSqlMarkdown<SelectableQFModelTOQO>(QueryFilterMode.Query, out xast);
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
                actual = "!cat".ParseSqlMarkdown<SelectableQFModelTOQO>(QueryFilterMode.Query, out xast);
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
                actual = "!cat dog".ParseSqlMarkdown<SelectableQFModelTOQO>(QueryFilterMode.Query, out xast);
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
                actual = "!(cat|dog)".ParseSqlMarkdown<SelectableQFModelTOQO>(QueryFilterMode.Query, out xast); 
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
                actual = "pet!(cat|dog)".ParseSqlMarkdown<SelectableQFModelTOQO>(QueryFilterMode.Query, out xast);
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
            List<SelectableQFModelTOQO> recordset;
            int countManual;
            using (var cnx = InitializeInMemoryDatabase())
            {
                actual = "b".ParseSqlMarkdown<SelectableQFModelTOQO>();
                Assert.AreEqual(string.Empty, actual);

                // Specify minimum length via helper.
                actual = "b".ParseSqlMarkdown<SelectableQFModelTOQO>(minInputLength: 0);
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
                actual = "b".ParseSqlMarkdown<SelectableQFModelTOQO>(QueryFilterMode.Filter, out xast);
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

                actual = mc.ParseSqlMarkdown<SelectableQFModelTOQO>("b", QueryFilterMode.Filter);
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
                    stringFirst = mc.ParseSqlMarkdown<SelectableQFModelTOQO>("animal b");
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

                    recordset = cnx.Query<SelectableQFModelTOQO>(
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

                    recordset = cmd.ExecuteQuery<SelectableQFModelTOQO>();
                    Assert.AreEqual(
                        countManual, 
                        recordset.Count(),
                        "Expecting that we can compose a command that produced the same result as the long form."
                    );
                }
                void subtestFormPositionalCommand()
                {
                    stringFirst = mc.ParseSqlMarkdown<SelectableQFModelTOQO>("animal b");
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
                    recordset = cmd.ExecuteQuery<SelectableQFModelTOQO>();                
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
Tags           =""[canine] [color]""
TagsDisplay    =""[canine] [color]""
IsChecked      =""False""
Selection      =""None""
IsEditing      =""False""
QueryTerm      =""brown~dog~loyal~friend~furry""
FilterTerm     =""brown~dog~loyal~friend~furry""
TagMatchTerm   =""[canine] [color]""
Properties     =""{
  ""Description"": ""Brown Dog"",
  ""Tags"": ""[canine] [color]"",
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
            List<SelectableQFModelTOQO> recordset;
            MarkdownContextOR context;
            ValidationState validationState = ValidationState.Valid;

            actual = "Tom Tester".ParseSqlMarkdown<SelectableQFModelTOQO>();
			expected = @" 
SELECT * FROM items WHERE (QueryTerm LIKE '%Tom%') AND (QueryTerm LIKE '%Tester%')"
			;
            Assert.AreEqual(expected.NormalizeResult(), actual.NormalizeResult(), "Expecting unquoted term behavior.");

            actual = "Tom Tester'".ParseSqlMarkdown<SelectableQFModelTOQO>();
			expected = @" 
SELECT * FROM items WHERE (QueryTerm LIKE '%Tom%') AND (QueryTerm LIKE '%Tester''%')"
			;
			Assert.AreEqual(expected.NormalizeResult(), actual.NormalizeResult(), "Expecting trailing single quote is escaped.");

            actual = "Tom Tester's".ParseSqlMarkdown<SelectableQFModelTOQO>();
			expected = @" 
SELECT * FROM items WHERE (QueryTerm LIKE '%Tom%') AND (QueryTerm LIKE '%Tester''s%')"
			;
            Assert.AreEqual(expected.NormalizeResult(), actual.NormalizeResult(), "Expecting single quote is escaped.");

            actual = @"""Tom Tester".ParseSqlMarkdown<SelectableQFModelTOQO>();
			expected = @" 
SELECT * FROM items WHERE (QueryTerm LIKE '%""Tom%') AND (QueryTerm LIKE '%Tester%')"
			;
            Assert.AreEqual(expected.NormalizeResult(), actual.NormalizeResult(), "Expecting unclosed double quote treated as literal.");

            actual = @"""Tom Tester'".ParseSqlMarkdown<SelectableQFModelTOQO>();
			expected = @" 
SELECT * FROM items WHERE (QueryTerm LIKE '%""Tom%') AND (QueryTerm LIKE '%Tester''%')"
			;
            Assert.AreEqual(expected.NormalizeResult(), actual.NormalizeResult(), "Expecting inner single quote escaped within unmatched double quote.");

            actual = @"""Tom Tester's".ParseSqlMarkdown<SelectableQFModelTOQO>();
			expected = @" 
SELECT * FROM items WHERE (QueryTerm LIKE '%""Tom%') AND (QueryTerm LIKE '%Tester''s%')"
			;
            Assert.AreEqual(expected.NormalizeResult(), actual.NormalizeResult(), "Expecting unclosed double quote with inner apostrophe.");

            actual = @"""Tom Tester's""".ParseSqlMarkdown<SelectableQFModelTOQO>();
			expected = @" 
SELECT * FROM items WHERE (QueryTerm LIKE '%Tom Tester''s%')"
			;
            Assert.AreEqual(expected.NormalizeResult(), actual.NormalizeResult(), "Expecting atomic term with apostrophe escaped.");
            #region V A R I A N T
            // We successfully have made an atomic term, and it is NOT QUOTED in the query.
            // Now, can you surround that atomic term with quotes?

            actual = @"""""""Tom Tester's""""""".ParseSqlMarkdown<SelectableQFModelTOQO>();
			expected = @" 
SELECT * FROM items WHERE (QueryTerm LIKE '%""""Tom Tester''s""""%')"
			;

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting an atomic term, surrounded by quotes (i.e. the outside quotes pair)."
            );
            #endregion V A R I A N T

            actual = @"You said, """.ParseSqlMarkdown<SelectableQFModelTOQO>();
			expected = @" 
SELECT * FROM items WHERE (QueryTerm LIKE '%You%') AND (QueryTerm LIKE '%said,%') AND (QueryTerm LIKE '%""%')"
			;
            Assert.AreEqual(expected.NormalizeResult(), actual.NormalizeResult(), "Expecting unmatched double quote treated as literal.");

            actual = @"You said, """"".ParseSqlMarkdown<SelectableQFModelTOQO>();
			expected = @" 
SELECT * FROM items WHERE (QueryTerm LIKE '%You%') AND (QueryTerm LIKE '%said,%') AND (QueryTerm LIKE '%""%')"
			;
            Assert.AreEqual(expected.NormalizeResult(), actual.NormalizeResult(), "Expecting escaped double quote treated as literal.");

            actual = @"You said, """"H".ParseSqlMarkdown<SelectableQFModelTOQO>();
			expected = @" 
SELECT * FROM items WHERE (QueryTerm LIKE '%You%') AND (QueryTerm LIKE '%said,%') AND (QueryTerm LIKE '%""H%')"
			;
            Assert.AreEqual(expected.NormalizeResult(), actual.NormalizeResult(), "Expecting escaped quote followed by text.");

            actual = @"You said, """"Hello""".ParseSqlMarkdown<SelectableQFModelTOQO>();
			expected = @" 
SELECT * FROM items WHERE (QueryTerm LIKE '%You%') AND (QueryTerm LIKE '%said,%') AND (QueryTerm LIKE '%""Hello""%')"
			;
            Assert.AreEqual(expected.NormalizeResult(), actual.NormalizeResult(), "SUBTLE! Expecting escaped double quote + Unmatched quote at end is literal.");

            actual = @"You said, """"Hello"""".".ParseSqlMarkdown<SelectableQFModelTOQO>();
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
            List<SelectableQFModelTOQO> recordset;
            MarkdownContextOR context;
            ValidationState validationState = ValidationState.Valid;


            sql = @"'Tom Tester'".ParseSqlMarkdown<SelectableQFModelTOQO>();

            actual = sql;
			expected = @" 
SELECT * FROM items WHERE (QueryTerm LIKE '%Tom Tester%')";

			Assert.AreEqual(
				expected.NormalizeResult(),
				actual.NormalizeResult(),
				"Expecting term between squotes is insulated from linting."
			);

            sql = @"'Tom ""safe inner"" Tester'".ParseSqlMarkdown<SelectableQFModelTOQO>();

            // {93B1FE29-F593-47EC-9CFA-F2706E79AA9E}
            actual = sql;
			expected = @" 
SELECT * FROM items WHERE (QueryTerm LIKE '%Tom safe inner Tester%')";

			Assert.AreEqual(
				expected.NormalizeResult(),
				actual.NormalizeResult(),
				"SUBTLE: The dquote linter runs before the squote linter so both have had an effect."
			);

			sql = @"'Tom """"safe inner"""" Tester'".ParseSqlMarkdown<SelectableQFModelTOQO>();

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
        public void Test_CustomSQLiteFunction()
        {
            using (var cnx = InitializeInMemoryDatabase())
            {
                SQLitePCL.raw.SetProvider(new SQLitePCL.SQLite3Provider_e_sqlite3());
                SQLitePCL.Batteries.Init();
                SQLitePCL.Batteries_V2.Init();

                SQLitePCL.raw.sqlite3_create_function(
                    cnx.Handle,
                    "PropertyValue",
                    2,
                    1,
                    null,
                    (ctx, user_data, args) =>
                    {
                        var json = SQLitePCL.raw.sqlite3_value_text(args[0]).utf8_to_string();
                        var key = SQLitePCL.raw.sqlite3_value_text(args[1]).utf8_to_string();

                        string? value = null;
                        try
                        {
                            if (JsonConvert.DeserializeObject<Dictionary<string, string>>(json) is { } dict)
                            {
                                if (dict.TryGetValue(key, out value))
                                { }
                            }
                            else throw new InvalidOperationException("Dictionary not found.");
                        }
                        catch
                        {
                            Debug.Fail("ADVISORY - Something went wrong..");
                        }
                        SQLitePCL.raw.sqlite3_result_text(ctx, value ?? string.Empty);
                    }
                );

                var recordset = cnx.Query<SelectableQFModelTOQO>(
                    $@"
Select *
From items 
Where PropertyValue({nameof(SelectableQFModelTOQO.Properties)}, '{nameof(SelectableQFModelTOQO.Description)}') LIKE '%brown dog%'");
                { }
            }
        }

        [TestMethod]
        public async Task Test_DemoFlow()
        {
            using (var cnx = InitializeInMemoryDatabase())
            {
                string actual, expected, sql;
                List<SelectableQFModelTOQO> recordset;
                NotifyCollectionChangedEventArgs? ecc;
                SelectableQFModelTOQO[] newItems = [];
                Queue<SenderEventPair> eventQueue = new();

                var items = new ObservableQueryFilterSource<SelectableQFModelTOQO>
                {
                    MemoryDatabase = cnx
                };
                NavSearchBar nsb = new NavSearchBar
                {
                    // NavSearchBar UI controls are designed
                    // to switch out sources many times.
                    ItemsSource = items,
                };
                items.InputTextSettled += async (sender, e) =>
                {
                    if (ReferenceEquals(sender, items))
                    {
                        switch (items.SearchEntryState)
                        {
                            case SearchEntryState.Cleared:
                                break;
                            case SearchEntryState.QueryEmpty:
                                break;
                            case SearchEntryState.QueryENB:
                                break;
                            case SearchEntryState.QueryEN:
                                // Client is responsible for the query at large because
                                // only they know what the data connection is. Once QFS
                                // has the recordset, it can filter it using SQLite.
                                sql = items.ParseSqlMarkdown<SelectableQFModelTOQO>(items.InputText);
                                recordset = cnx.Query<SelectableQFModelTOQO>(sql);
                                items.ReplaceItems(recordset);
                                break;
                            case SearchEntryState.QueryCompleteNoResults:
                            case SearchEntryState.QueryCompleteWithResults:
                                // No need for explicit. This is inherent on the
                                // awaiter but give it a little extra time.

                                // Do this
                                await Task.Delay(TimeSpan.FromSeconds(0.5));

                                // Not this (ApplyFilter is protected now anyway!)
                                // items.ApplyFilter();
                                break;
                            default:
                                break;
                        }
                    }
                    else
                    {
                        throw new InvalidOperationException("Expecting events from OQFS only.");
                    }
                };
                items.CollectionChanged += (sender, e) =>
                {
                    eventQueue.Enqueue((sender, e));
                    // FYI:
                    switch (e.Action)
                    {
                        case NotifyCollectionChangedAction.Add:
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
                await subtestAppendAndRequery();


                #region S U B T E S T S
                async Task subtestQueryInitial()
                {
                    await localCommitOnSettle("animal");

                    Assert.IsNotNull((ecc = (NotifyCollectionChangedEventArgs?)eventQueue.Dequeue()?.e));
                    {
                        switch (ecc.Action)
                        {
                            case NotifyCollectionChangedAction.Reset:
                                break;
                            default:
                                Assert.Fail("Expecting Reset");
                                break;
                        }
                    }
                    Assert.IsNotNull((ecc = (NotifyCollectionChangedEventArgs?)eventQueue.DequeueSingle()?.e));
                    {
                        switch (ecc.Action)
                        {
                            case NotifyCollectionChangedAction.Add:
                                newItems =
                                    ecc
                                    .NewItems
                                    ?.OfType<SelectableQFModelTOQO>()
                                    .ToArray() ?? [];
                                Assert.AreEqual(12, newItems.Length);
                                break;
                            default:
                                Assert.Fail("Expecting Add");
                                break;
                        }
                    }

                    // As a product of the CollectionChangedEvent this
                    // is representative of what we'd see in the visible list.
                    actual = string.Join(Environment.NewLine, newItems.Select(_ => _.ToString()));
                    actual.ToClipboard();
                    actual.ToClipboardExpected();
                    actual.ToClipboardAssert("Expecting items to match");
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
Should NOT match an expression with an ""animal"" tag.  [not animal]";

                    Assert.AreEqual(
                        expected.NormalizeResult(),
                        actual.NormalizeResult(),
                        "Expecting items to match"
                    );
                }

                async Task subtestAppendAndRequery()
                {
                    items.Clear(all: true);   
                    // Live-demo specific.
                    Add("Appetizer Plate", "[dish]", false, new() { "starter", "appealing", "snack" });
                    Add("Errata", "[notes]", false, new() { "crunchy", "green", "appended" });
                    Add("Happy Camper", "[phrase]", false, new() { "joyful", "camp", "approach-west" });
                    Add("Great example - Markdown Demo", "[app] [portable]", false, new() { "digital", "mobile", "software" });
                    Add("Application Form", "[document]", false, new() { "paperwork", "apply" });
                    Add("App Store", "[app]", false, new() { "digital", "mobile", "software" });

                    await localCommitOnSettle("app gre");

                    actual = string.Join(Environment.NewLine, items.Select(_ => _.ToString()));
                    expected = @" 
Green Apple ""tart"",""snack"",""healthy"" [fruit] [color]
Errata ""crunchy"",""green"",""appended"" [notes]
Great example - Markdown Demo ""digital"",""mobile"",""software"" [app] [portable]"
                    ;

                    Assert.AreEqual(SearchEntryState.QueryCompleteWithResults, items.SearchEntryState);

                    // Perform a filter
                    await localCommitOnSettle("[app] gre");

                    Assert.AreEqual(
                        expected.NormalizeResult(),
                        actual.NormalizeResult(),
                        "Expecting items to match"
                    );

                    actual = string.Join(Environment.NewLine, items.Select(_ => _.ToString()));

                    actual.ToClipboardExpected();
                    expected = @" 
Great example - Markdown Demo ""digital"",""mobile"",""software"" [app] [portable]"
                    ;
                }
                #endregion S U B T E S T S

                #region L o c a l F x
                async Task localCommitOnSettle(string expr)
                {
                    nsb.InputText = expr;
                    items.Commit();
                    await items;
                }

                void Add(string description, string tags, bool isChecked, List<string>? keywords = null)
                {
                    var instance = new SelectableQFModelTOQO();
                    var type = typeof(SelectableQFModelTOQO);
                    type.GetProperty("Description")?.SetValue(instance, description);
                    type.GetProperty("Tags")?.SetValue(instance, tags);
                    type.GetProperty("IsChecked")?.SetValue(instance, isChecked);
                    if (keywords != null)
                    {
                        var json = JsonConvert.SerializeObject(keywords);
                        type.GetProperty("Keywords")?.SetValue(instance, json);
                    }
                    cnx.Insert(instance);
                }
                #endregion L o c a l F x
            }
        }
    }
}