using IVSoftware.Portable.SQLiteMarkdown.Collections;
using IVSoftware.Portable.SQLiteMarkdown.MSTest.DemoDB;
using IVSoftware.Portable.SQLiteMarkdown.MSTest.Models;
using IVSoftware.Portable.Xml.Linq.XBoundObject.Modeling;
using IVSoftware.WinOS.MSTest.Extensions;
using Newtonsoft.Json;
using SQLite;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Dynamic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using static SQLite.SQLite3;
using Ignore = Microsoft.VisualStudio.TestTools.UnitTesting.IgnoreAttribute;

namespace IVSoftware.Portable.SQLiteMarkdown.MSTest
{   
    // Namespace with test-only classes.
    namespace DemoDB
    {
        [DebuggerDisplay("{Description}")]
        [Table("items")]
        public class StrictTagQueryModel : SelfIndexed, ISelectableQueryFilterItem
        {
            [PrimaryKey]
            public override string Id { get; set; } = Guid.NewGuid().ToString();

            [SelfIndexed]
            public string Description
            {
                get => _description;
                set
                {
                    if (!Equals(_description, value))
                    {
                        _description = value;
                        OnPropertyChanged();
                    }
                }
            }
            string _description = "New Item";

            [SelfIndexed]
            public string Keywords
            {
                get => _keywords;
                set
                {
                    if (!Equals(_keywords, value))
                    {
                        _keywords = value;
                        OnPropertyChanged();
                    }
                }
            }
            private string _keywords = JsonConvert.SerializeObject(new List<string>());

            public string KeywordsDisplay => Keywords.Trim('[', ']');

            [SelfIndexed(IndexingMode.TagMatchTerm)]    // Responds to strict bracketed terms only
            public string Tags
            {
                get => _tags;
                set
                {
                    if (!Equals(_tags, value))
                    {
                        _tags = value;
                        OnPropertyChanged();
                    }
                }
            }
            private string _tags = string.Empty;

            public string TagsDisplay => Tags;


            public bool IsChecked
            {
                get => _isChecked;
                set
                {
                    if (!Equals(_isChecked, value))
                    {
                        _isChecked = value;
                        OnPropertyChanged();
                    }
                }
            }
            bool _isChecked = default;

            [Obsolete("Use IndexingMode attribute to declare terms to be considered in the filter query.")]
            public IComparable FilterValue => Description;

            public OnePageItemSelection Selection
            {
                get => _selection;
                set
                {
                    if (!Equals(_selection, value))
                    {
                        _selection = value;
                        OnPropertyChanged();
                    }
                }
            }
            private OnePageItemSelection _selection = OnePageItemSelection.None;

            public override string ToString() => $"{Description} {KeywordsDisplay} {TagsDisplay}".Trim();

            public string Report()
            {
                var builder = new List<string>();
                var type = GetType();

                foreach (var pi in type.GetProperties())
                {
                    // Skip the guid, which is new everytime.

                    switch (pi.Name)
                    {
                        case nameof(Id):
                        case nameof(PrimaryKey):
                            continue;
                        default:
                            break;
                    }
                    if (pi.Name == nameof(Id)) continue;

                    var value = pi.GetValue(this);
                    builder.Add($@"{pi.Name,-15}=""{value}""");
                }
                return string.Join(Environment.NewLine, builder);
            }
        }

        [DebuggerDisplay("{Description}")]
        [Table("items")]
        public class AtomicQuoteTestModel : SelfIndexed, ISelectableQueryFilterItem
        {
            public IComparable FilterValue => throw new NotImplementedException();

            public OnePageItemSelection Selection { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        }

        public class NavSearchBar : INotifyPropertyChanged
        {
            public IList<SelectableQueryModel>? ItemsSource
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

            IList<SelectableQueryModel>? _itemsSource = null;
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
            var items = new List<SelectableQueryModel>
            {
                new SelectableQueryModel
                {
                    Description = "Brown Dog",
                    Tags = "[canine] [color]",
                    IsChecked = false,
                    Keywords = JsonConvert.SerializeObject(new List<string> { "loyal", "friend", "furry" })
                },
            };
            var imdb = new SQLiteConnection(":memory:");
            imdb.CreateTable<SelectableQueryModel>();
            imdb.InsertAll(items);
            return imdb;
        }
        private SQLiteConnection InitializeInMemoryDatabase()
        {
            var items = new List<SelectableQueryModel>
            {
                new SelectableQueryModel
                {
                    Description = "Brown Dog",
                    Tags = "[canine] [color]",
                    IsChecked = false,
                    Keywords = JsonConvert.SerializeObject(new List<string> { "loyal", "friend", "furry" })
                },
                new SelectableQueryModel
                {
                    Description = "Green Apple",
                    Tags = "[fruit] [color]",
                    IsChecked = false,
                    Keywords = JsonConvert.SerializeObject(new List<string> { "tart", "snack", "healthy" })
                },
                new SelectableQueryModel { Description = "Yellow Banana", Tags = "[fruit] [color]", IsChecked = false },
                new SelectableQueryModel
                {
                    Description = "Blue Bird",
                    Tags = "[bird] [color]",
                    IsChecked = false,
                    Keywords = JsonConvert.SerializeObject(new List<string> { "sky", "feathered", "song" })
                },
                new SelectableQueryModel
                {
                    Description = "Red Cherry",
                    Tags = "[fruit] [color]",
                    IsChecked = false,
                    Keywords = JsonConvert.SerializeObject(new List<string> { "sweet", "summer", "dessert" })
                },
                new SelectableQueryModel { Description = "Black Cat", Tags = "[animal] [color]", IsChecked = false },
                new SelectableQueryModel { Description = "Orange Fox", Tags = "[animal] [color]", IsChecked = false },
                new SelectableQueryModel
                {
                    Description = "White Rabbit",
                    Tags = "[animal] [color]",
                    IsChecked = false,
                    Keywords = JsonConvert.SerializeObject(new List<string> { "bunny", "soft", "jump" })
                },
                new SelectableQueryModel { Description = "Purple Grape", Tags = "[fruit] [color]", IsChecked = false },
                new SelectableQueryModel
                {
                    Description = "Gray Wolf",
                    Tags = "[animal] [color]",
                    IsChecked = false,
                    Keywords = JsonConvert.SerializeObject(new List<string> { "pack", "howl", "wild" })
                },
                new SelectableQueryModel { Description = "Pink Flamingo", Tags = "[bird] [color]", IsChecked = false },
                new SelectableQueryModel { Description = "Golden Lion", Tags = "[animal] [color]", IsChecked = false },
                new SelectableQueryModel
                {
                    Description = "Brown Bear",
                    Tags = "[animal] [color]",
                    IsChecked = false,
                    Keywords = JsonConvert.SerializeObject(new List<string> { "strong", "wild", "forest" })
                },
                new SelectableQueryModel { Description = "Green Pear", Tags = "[fruit] [color]", IsChecked = false },
                new SelectableQueryModel { Description = "Red Strawberry", Tags = "[fruit] [color]", IsChecked = false },
                new SelectableQueryModel
                {
                    Description = "Black Panther",
                    Tags = "[animal] [color]",
                    IsChecked = false,
                    Keywords = JsonConvert.SerializeObject(new List<string> { "stealthy", "feline", "night" })
                },
                new SelectableQueryModel { Description = "Yellow Lemon", Tags = "[fruit] [color]", IsChecked = false },
                new SelectableQueryModel { Description = "White Swan", Tags = "[bird] [color]", IsChecked = false },
                new SelectableQueryModel { Description = "Purple Plum", Tags = "[fruit] [color]", IsChecked = false },
                new SelectableQueryModel
                {
                    Description = "Blue Whale",
                    Tags = "[marine-mammal] [ocean]",
                    IsChecked = false,
                    Keywords = JsonConvert.SerializeObject(new List<string> { "ocean", "mammal", "giant" })
                },
                new SelectableQueryModel
                {
                    Description = "Elephant",
                    Tags = "[animal]",
                    IsChecked = false,
                    Keywords = JsonConvert.SerializeObject(new List<string> { "trunk", "herd", "safari" })
                },
                new SelectableQueryModel { Description = "Pineapple", Tags = "[fruit]", IsChecked = false },
                new SelectableQueryModel { Description = "Shark", Tags = "[fish]", IsChecked = false },
                new SelectableQueryModel { Description = "Owl", Tags = "[bird]", IsChecked = false },
                new SelectableQueryModel { Description = "Giraffe", Tags = "[animal]", IsChecked = false },
                new SelectableQueryModel { Description = "Coconut", Tags = "[fruit]", IsChecked = false },
                new SelectableQueryModel
                {
                    Description = "Kangaroo",
                    Tags = "[animal]",
                    IsChecked = false,
                    Keywords = JsonConvert.SerializeObject(new List<string> { "bounce", "outback", "marsupial" })
                },
                new SelectableQueryModel { Description = "Dragonfruit", Tags = "[fruit]", IsChecked = false },
                new SelectableQueryModel { Description = "Turtle", Tags = "[animal]", IsChecked = false },
                new SelectableQueryModel { Description = "Mango", Tags = "[fruit]", IsChecked = false },
                new SelectableQueryModel { Description = "Should NOT match an expression with an \"animal\" tag.", Tags = "[not animal]", IsChecked = false }
            };
            var imdb = new SQLiteConnection(":memory:");
            imdb.CreateTable<SelectableQueryModel>();
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
            var items = new List<SelectableQueryModel>
            {
            };
            var imdb = new SQLiteConnection(":memory:");
            imdb.CreateTable<SelectableQueryModel>();
            imdb.InsertAll(items);
            return imdb;
        }

        [TestMethod]
        public void Test_ViewDatabase()
        {
            string actual, expected;
            using (var cnx = InitializeInMemoryDatabase())
            {
                var allRecords = cnx.Query<SelectableQueryModel>("Select * from items");
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
            string actual, expected, sql;
            List<SelectableQueryModel> results;
            var builder = new List<string>();
            List<SelectableQueryModel> unfiltered = new List<SelectableQueryModel>();
            List<SelectableQueryModel> filtered = new List<SelectableQueryModel>();
            MarkdownContext context;
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
                    sql = "animal".ParseSqlMarkdown<SelectableQueryModel>();

                    actual = sql;
                    expected = @" 
SELECT * FROM items WHERE
(LikeTerm LIKE '%animal%' OR ContainsTerm LIKE '%animal%')"
                    ;
                    ;
                    Assert.AreEqual(
                        expected.NormalizeResult(),
                        actual.NormalizeResult(),
                        "Expecting correct expression"
                    );
                }
                async Task subtestReport()
                {
                    unfiltered = cnx.Query<SelectableQueryModel>("Select * from items");
                    await Task.Delay(TimeSpan.FromSeconds(0.5));
                    foreach (var item in unfiltered)
                    {
                        builder.Add(item.Report());
                        builder.Add(string.Empty);
                    }

                    var joined = string.Join(Environment.NewLine, builder);

                    actual = joined;
                    actual.ToClipboardExpected();
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
TagMatchTerm   =""[canine]~[color]""
Properties     =""{
  ""Description"": ""Brown Dog"",
  ""Tags"": ""[canine] [color]"",
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
                    sql = "bro".ParseSqlMarkdown<SelectableQueryModel>();

                    actual = sql;
                    expected = @" 
SELECT * FROM items WHERE
(LikeTerm LIKE '%bro%' OR ContainsTerm LIKE '%bro%')"
                    ;

                    Assert.AreEqual(
                        expected.NormalizeResult(),
                        actual.NormalizeResult(),
                        "Expecting correct expr"
                    );
                    Assert.IsNotNull(cnx.Query<SelectableQueryModel>(sql).SingleOrDefault());

                    sql = "bro dog".ParseSqlMarkdown<SelectableQueryModel>();
                    Assert.IsNotNull(cnx.Query<SelectableQueryModel>(sql).SingleOrDefault());

                    sql = "brown furry".ParseSqlMarkdown<SelectableQueryModel>();
                    actual = sql;
                    expected = @" 
SELECT * FROM items WHERE
(LikeTerm LIKE '%brown%' OR ContainsTerm LIKE '%brown%') AND (LikeTerm LIKE '%furry%' OR ContainsTerm LIKE '%furry%')"
                    ;
                    Assert.IsNotNull(cnx.Query<SelectableQueryModel>(sql).SingleOrDefault());
                    sql = "brown !dog".ParseSqlMarkdown<SelectableQueryModel>();
                    actual = sql;
                    actual.ToClipboardExpected();
                    // IS null
                    Assert.IsNull(cnx.Query<SelectableQueryModel>(sql).SingleOrDefault());
                }
                void subtestApplyFilter()
                {
                    using (var subcnx = new SQLiteConnection(":memory:"))
                    {
                        subcnx.CreateTable<SelectableQueryModel>();
                        subcnx.InsertAll(unfiltered);
                        builder.Clear();

                        sql = "brown furry".ParseSqlMarkdown<SelectableQueryModel>(IndexingMode.ContainsTerm);
                        actual = sql;
                        actual.ToClipboard();
                        actual.ToClipboardExpected();
                        expected = @" 
SELECT * FROM items WHERE
(ContainsTerm LIKE '%brown%') AND (ContainsTerm LIKE '%furry%')"
                        ;

                        Assert.AreEqual(
                            expected.NormalizeResult(),
                            actual.NormalizeResult(),
                            "Expecting correct expr."
                        );
                        filtered = subcnx.Query<SelectableQueryModel>(sql);

                        Assert.IsNotNull(filtered.SingleOrDefault());
                        sql = "brown canine".ParseSqlMarkdown<SelectableQueryModel>(IndexingMode.ContainsTerm);
                        filtered = subcnx.Query<SelectableQueryModel>(sql);
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
            List<SelectableQueryModel> results;
            MarkdownContext context;
            ValidationState state;

            subtestReport();

            #region S U B T E S T S
            void subtestReport()
            {
                var builder = new List<string>();
                using (var cnx = InitializeInMemoryDatabase())
                {
                    var allRecords = cnx.Query<SelectableQueryModel>("Select * from items");
                    foreach (var item in allRecords)
                    {
                        builder.Add(item.Report());
                        builder.Add(string.Empty);
                    }
                }

                var joined = string.Join(Environment.NewLine, builder);

                actual = joined;
                actual.ToClipboard();
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
TagMatchTerm   =""[canine]~[color]""
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
FilterValue    =""Green Apple""
Selection      =""None""
LikeTerm       =""green~apple~tart~snack~healthy~fruit~color""
ContainsTerm   =""green~apple~tart~snack~healthy""
TagMatchTerm   =""[fruit]~[color]""
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
FilterValue    =""Yellow Banana""
Selection      =""None""
LikeTerm       =""yellow~banana~~fruit~color""
ContainsTerm   =""yellow~banana""
TagMatchTerm   =""[fruit]~[color]""
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
FilterValue    =""Blue Bird""
Selection      =""None""
LikeTerm       =""blue~bird~sky~feathered~song~bird~color""
ContainsTerm   =""blue~bird~sky~feathered~song""
TagMatchTerm   =""[bird]~[color]""
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
FilterValue    =""Red Cherry""
Selection      =""None""
LikeTerm       =""red~cherry~sweet~summer~dessert~fruit~color""
ContainsTerm   =""red~cherry~sweet~summer~dessert""
TagMatchTerm   =""[fruit]~[color]""
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
FilterValue    =""Black Cat""
Selection      =""None""
LikeTerm       =""black~cat~~animal~color""
ContainsTerm   =""black~cat""
TagMatchTerm   =""[animal]~[color]""
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
FilterValue    =""Orange Fox""
Selection      =""None""
LikeTerm       =""orange~fox~~animal~color""
ContainsTerm   =""orange~fox""
TagMatchTerm   =""[animal]~[color]""
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
FilterValue    =""White Rabbit""
Selection      =""None""
LikeTerm       =""white~rabbit~bunny~soft~jump~animal~color""
ContainsTerm   =""white~rabbit~bunny~soft~jump""
TagMatchTerm   =""[animal]~[color]""
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
FilterValue    =""Purple Grape""
Selection      =""None""
LikeTerm       =""purple~grape~~fruit~color""
ContainsTerm   =""purple~grape""
TagMatchTerm   =""[fruit]~[color]""
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
FilterValue    =""Gray Wolf""
Selection      =""None""
LikeTerm       =""gray~wolf~pack~howl~wild~animal~color""
ContainsTerm   =""gray~wolf~pack~howl~wild""
TagMatchTerm   =""[animal]~[color]""
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
FilterValue    =""Pink Flamingo""
Selection      =""None""
LikeTerm       =""pink~flamingo~~bird~color""
ContainsTerm   =""pink~flamingo""
TagMatchTerm   =""[bird]~[color]""
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
FilterValue    =""Golden Lion""
Selection      =""None""
LikeTerm       =""golden~lion~~animal~color""
ContainsTerm   =""golden~lion""
TagMatchTerm   =""[animal]~[color]""
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
FilterValue    =""Brown Bear""
Selection      =""None""
LikeTerm       =""brown~bear~strong~wild~forest~animal~color""
ContainsTerm   =""brown~bear~strong~wild~forest""
TagMatchTerm   =""[animal]~[color]""
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
FilterValue    =""Green Pear""
Selection      =""None""
LikeTerm       =""green~pear~~fruit~color""
ContainsTerm   =""green~pear""
TagMatchTerm   =""[fruit]~[color]""
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
FilterValue    =""Red Strawberry""
Selection      =""None""
LikeTerm       =""red~strawberry~~fruit~color""
ContainsTerm   =""red~strawberry""
TagMatchTerm   =""[fruit]~[color]""
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
FilterValue    =""Black Panther""
Selection      =""None""
LikeTerm       =""black~panther~stealthy~feline~night~animal~color""
ContainsTerm   =""black~panther~stealthy~feline~night""
TagMatchTerm   =""[animal]~[color]""
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
FilterValue    =""Yellow Lemon""
Selection      =""None""
LikeTerm       =""yellow~lemon~~fruit~color""
ContainsTerm   =""yellow~lemon""
TagMatchTerm   =""[fruit]~[color]""
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
FilterValue    =""White Swan""
Selection      =""None""
LikeTerm       =""white~swan~~bird~color""
ContainsTerm   =""white~swan""
TagMatchTerm   =""[bird]~[color]""
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
FilterValue    =""Purple Plum""
Selection      =""None""
LikeTerm       =""purple~plum~~fruit~color""
ContainsTerm   =""purple~plum""
TagMatchTerm   =""[fruit]~[color]""
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
FilterValue    =""Blue Whale""
Selection      =""None""
LikeTerm       =""blue~whale~ocean~mammal~giant~marine-mammal""
ContainsTerm   =""blue~whale~ocean~mammal~giant""
TagMatchTerm   =""[marine-mammal]~[ocean]""
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
FilterValue    =""Elephant""
Selection      =""None""
LikeTerm       =""elephant~trunk~herd~safari~animal""
ContainsTerm   =""elephant~trunk~herd~safari""
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
FilterValue    =""Pineapple""
Selection      =""None""
LikeTerm       =""pineapple~~fruit""
ContainsTerm   =""pineapple""
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
FilterValue    =""Shark""
Selection      =""None""
LikeTerm       =""shark~~fish""
ContainsTerm   =""shark""
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
FilterValue    =""Owl""
Selection      =""None""
LikeTerm       =""owl~~bird""
ContainsTerm   =""owl""
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
FilterValue    =""Giraffe""
Selection      =""None""
LikeTerm       =""giraffe~~animal""
ContainsTerm   =""giraffe""
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
FilterValue    =""Coconut""
Selection      =""None""
LikeTerm       =""coconut~~fruit""
ContainsTerm   =""coconut""
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
FilterValue    =""Kangaroo""
Selection      =""None""
LikeTerm       =""kangaroo~bounce~outback~marsupial~animal""
ContainsTerm   =""kangaroo~bounce~outback~marsupial""
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
FilterValue    =""Dragonfruit""
Selection      =""None""
LikeTerm       =""dragonfruit~~fruit""
ContainsTerm   =""dragonfruit""
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
FilterValue    =""Turtle""
Selection      =""None""
LikeTerm       =""turtle~~animal""
ContainsTerm   =""turtle""
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
FilterValue    =""Mango""
Selection      =""None""
LikeTerm       =""mango~~fruit""
ContainsTerm   =""mango""
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
FilterValue    =""Should NOT match an expression with an ""animal"" tag.""
Selection      =""None""
LikeTerm       =""should~not~match~an~expression~with~animal~tag.~~not&animal""
ContainsTerm   =""should~not~match~an~expression~with~animal~tag.""
TagMatchTerm   =""[not&animal]""
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
            List<SelectableQueryModel> results;
            MarkdownContext context;
            ValidationState state;
            using (var cnx = InitializeInMemoryDatabase())
            {
                subtestBasicQueryAnimal();
                subtestPluralQueryAnimals();

                #region S U B T E S T S
                void subtestBasicQueryAnimal()
                {
                    sql = "animal".ParseSqlMarkdown<SelectableQueryModel>();

                    actual = sql;
                    expected = @" 
SELECT * FROM items WHERE
(LikeTerm LIKE '%animal%' OR ContainsTerm LIKE '%animal%')"
                    ;

                    Assert.AreEqual(
                        expected.NormalizeResult(),
                        actual.NormalizeResult(),
                        "Expecting expr does not include a Tag term"
                    );

                    results = cnx.Query<SelectableQueryModel>(sql);

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
                        .ParseSqlMarkdown<SelectableQueryModel>();

                    sql = sql.ToFuzzyQuery();

                    results = cnx.Query<SelectableQueryModel>(sql);

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
            List<SelectableQueryModel> results;
            var itemsSource = new ObservableQueryFilterSource<SelectableQueryModel>();
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
                    sql = "animal".ParseSqlMarkdown<SelectableQueryModel>();
                    results = cnx.Query<SelectableQueryModel>(sql);
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
                        sql = "animal".ParseSqlMarkdown<SelectableQueryModel>();
                        results = cnx.Query<SelectableQueryModel>(sql);
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
                            .NewItems?.OfType<SelectableQueryModel>()
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
        public async Task Test_ObservableQueryFilterStateTracker()
        {
            string actual, expected, sql;
            SenderEventPair sep;
            NotifyCollectionChangedEventArgs ecc;
            PropertyChangedEventArgs epc;
            Queue<SenderEventPair> eventQueue = new();

            List<SelectableQueryModel> recordset;
            var items = new ObservableQueryFilterSource<SelectableQueryModel>();


            items.PropertyChanged += (sender, e) =>
            {
                eventQueue.Enqueue((sender, e));
            };
            using (var cnx = InitializeInMemoryDatabase())
            {
                subtestEmptyToFirstChar();
                subtestSecondChar();
                subtestThirdCharEnableQuery();
                await subtestCommit();

                #region S U B T E S T S

                void subtestEmptyToFirstChar()
                {
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
                }

                void subtestSecondChar()
                {
                    Assert.AreEqual(
                        SearchEntryState.QueryENB,
                        items.SearchEntryState,
                        "Expecting specific state CHANGED."
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
                    eventQueue.Clear();
                }

                void subtestThirdCharEnableQuery()
                {
                    Assert.AreEqual(
                        SearchEntryState.QueryENB,
                        items.SearchEntryState,
                        "Expecting specific state UNCHANGED."
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
                        "Expecting specific state CHANGED."
                    );
                }

                async Task subtestCommit()
                {
                    // "animal"
                    items.InputText += "mal";
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
                    eventQueue.Clear();

                    Assert.AreEqual(
                        SearchEntryState.QueryEN,
                        items.SearchEntryState,
                        "Expecting specific state UNCHANGED."
                    );
                    // Based on UI interaction like return key pressed
                    sql = items.InputText.ParseSqlMarkdown<SelectableQueryModel>();
                    recordset = cnx.Query<SelectableQueryModel>(sql);
                    items.ReplaceItems(recordset);

                    Assert.AreEqual(SearchEntryState.QueryCompleteWithResults, items.SearchEntryState);
                    Assert.AreEqual(FilteringState.Armed, items.FilteringState);
                    Assert.AreNotEqual(0, recordset.Count);
                    Assert.AreNotEqual(0, items.Count);

                    actual = string.Join(Environment.NewLine, items.Select(_ => _.ToString()));
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
                        FilteringState.Active,
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
                    // that query result, so filtering goes Active in theis case.

#endif


                    // animal.b
                    // Expecting Filter mode
                    items.InputText += "b";
                    { }

                    actual = string.Join(Environment.NewLine, items.Select(_ => _.ToString()));
                    actual.ToClipboard();
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

                #region L o c a l F x
                async Task localSettle(double seconds = 1.0) =>
                    await Task.Delay(TimeSpan.FromSeconds(seconds));
                #endregion L o c a l F x
            }
        }

        [TestMethod]
        public void Test_FuzzyPlural()
        {
            string actual, expected, sql;
            List<SelectableQueryModel> recordset;
            using (var cnx = InitializeInMemoryDatabase())
            {
                subtestBasicQueryAnimal();
                subtestPluralQueryAnimals();

                #region S U B T E S T S
                void subtestBasicQueryAnimal()
                {
                    sql = "animal".ParseSqlMarkdown<SelectableQueryModel>();

                    actual = sql;
                    expected = @" 
SELECT * FROM items WHERE
(LikeTerm LIKE '%animal%' OR ContainsTerm LIKE '%animal%')"
                    ;

                    Assert.AreEqual(
                        expected.NormalizeResult(),
                        actual.NormalizeResult(),
                        "Expecting expr does not include a Tag term"
                    );

                    recordset = cnx.Query<SelectableQueryModel>(sql);

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
                        .ParseSqlMarkdown<SelectableQueryModel>();

                    sql = sql.ToFuzzyQuery();

                    recordset = cnx.Query<SelectableQueryModel>(sql);

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
        public void Test_Positional()
        {
            string actual, expected, sql;
            List<SelectableQueryModel> recordset;
            MarkdownContext context;
            var validationState = ValidationState.Empty;

            using (var cnx = InitializeInMemoryDatabase())
            {
                #region S U B T E S T S

                context = "color".ParseSqlMarkdown<SelectableQueryModel>(IndexingMode.LikeTerm, ref validationState);
                Assert.AreEqual(ValidationState.Valid, validationState);
                //sql = context.ToString();
                recordset = cnx.Query<SelectableQueryModel>(context.ToString());
                Assert.AreEqual(19, recordset.Count);

                // WORKS
                recordset = cnx.Query<SelectableQueryModel>("SELECT * FROM items WHERE\r\n(LikeTerm LIKE ?)", "%color%");
                Assert.AreEqual(19, recordset.Count);

                // WORKS
                recordset = cnx.Query<SelectableQueryModel>(context.PositionalQuery, context.PositionalArgs);
                Assert.AreEqual(19, recordset.Count);

                // WORKS
                var (query, args) =
                    "color"
                    .ParseSqlMarkdown<SelectableQueryModel>(IndexingMode.LikeTerm, ref validationState)
                    .ToPositional();
                recordset = cnx.Query<SelectableQueryModel>(query, args);
                Assert.AreEqual(19, recordset.Count);


                (query, args) =
                    "animal"
                    .ParseSqlMarkdown<SelectableQueryModel>(IndexingMode.LikeTerm, ref validationState)
                    .ToPositional();
                recordset = cnx.Query<SelectableQueryModel>(query, args);
                Assert.AreEqual(12, recordset.Count);


                #endregion S U B T E S T S
            }
        }

        [TestMethod]
        public void Test_ShortExpr()
        {
            string actual, expected, sql;
            MarkdownContext context;
            ValidationState validationState = ValidationState.Valid;
            using (var cnx = InitializeInMemoryDatabase())
            {
                sql = "b".ParseSqlMarkdown<SelectableQueryModel>();
                Assert.AreEqual(string.Empty, sql);

                sql = "b".ParseSqlMarkdown<SelectableQueryModel>(minInputLength: 0);

                actual = sql;
                expected = @" 
SELECT * FROM items WHERE
(LikeTerm LIKE '%b%' OR ContainsTerm LIKE '%b%')"
                ;

                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting non-throttled expr with no Tag term."
                );

                context = "b".ParseSqlMarkdown<SelectableQueryModel>(ref validationState);
                Assert.AreEqual(string.Empty, context.PositionalQuery);

                // Mutate the state.
                validationState |= ValidationState.DisableMinLength;
                context = "b".ParseSqlMarkdown<SelectableQueryModel>(ref validationState);

                actual = context.PositionalQuery;
                expected = @" 
SELECT * FROM items WHERE
(LikeTerm LIKE ? OR ContainsTerm LIKE ?)"
                ;

                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting non-throttled expr."
                );

                actual = string.Join(",", context.PositionalArgs);
                expected = @" 
%b%,%b%"
                ;

                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting args to match."
                );

                #region S U B T E S T S
                #endregion S U B T E S T S
            }
        }

        [TestMethod]
        public void Test_StrictTagExpr()
        {
            string actual, expected, unexpected, sql;
            MarkdownContext context;
            List<StrictTagQueryModel> recordset;
            ValidationState validationState = ValidationState.Valid;
            using (var cnx = InitializeInMemoryDatabase<StrictTagQueryModel>())
            {
                subtestReportBrownDog();
                subtestStrictNotBracketed();
                subtestStrictBracketed();

                { }

                #region S U B T E S T S
                void subtestReportBrownDog()
                {
                    recordset = cnx.Query<StrictTagQueryModel>("brown dog".ParseSqlMarkdown<StrictTagQueryModel>());
                    actual = recordset.Single().Report(); ;
                    actual.ToClipboard();
                    actual.ToClipboardExpected();
                    actual.ToClipboardAssert("Expecting strict tag indexing.");
                    { }
                    expected = @" 
Description    =""Brown Dog""
Keywords       =""[""loyal"",""friend"",""furry""]""
KeywordsDisplay=""""loyal"",""friend"",""furry""""
Tags           =""[canine] [color]""
TagsDisplay    =""[canine] [color]""
IsChecked      =""False""
FilterValue    =""Brown Dog""
Selection      =""None""
LikeTerm       =""brown~dog~loyal~friend~furry""
ContainsTerm   =""brown~dog~loyal~friend~furry""
TagMatchTerm   =""[canine]~[color]""
Properties     =""{
  ""Description"": ""Brown Dog"",
  ""Tags"": ""[canine] [color]"",
  ""Keywords"": ""[\""loyal\"",\""friend\"",\""furry\""]""
}""";

                    Assert.AreEqual(
                        expected.NormalizeResult(),
                        actual.NormalizeResult(),
                        "Expecting strict tag indexing does NOT INCLUDE 'canine' and 'color' in LikeTerm."
                    );
                }
                void subtestStrictNotBracketed()
                {
                    sql = "canine".ParseSqlMarkdown<StrictTagQueryModel>();

                    actual = sql;
                    expected = @" 
SELECT * FROM items WHERE
(LikeTerm LIKE '%canine%' OR ContainsTerm LIKE '%canine%')"
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

                    actual.ToClipboardExpected();
                    { }
                    expected = @" 
SELECT * FROM items WHERE
(TagMatchTerm LIKE '%[canine]%')"
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
            string sql;
            List<SelectableQueryModel> recordset;
            MarkdownContext context;
            ValidationState validationState = ValidationState.Valid;

            sql = "Tom Tester".ParseSqlMarkdown<SelectableQueryModel>();
            actual = sql;
            expected = @" 
SELECT * FROM items WHERE
(LikeTerm LIKE '%Tom%' OR ContainsTerm LIKE '%Tom%') AND (LikeTerm LIKE '%Tester%' OR ContainsTerm LIKE '%Tester%')";
            Assert.AreEqual(expected.NormalizeResult(), actual.NormalizeResult(), "Expecting unquoted term behavior.");

            sql = "Tom Tester'".ParseSqlMarkdown<SelectableQueryModel>();
            actual = sql;
            actual.ToClipboard();
            actual.ToClipboardExpected();
            expected = @" 
SELECT * FROM items WHERE
(LikeTerm LIKE '%Tom%' OR ContainsTerm LIKE '%Tom%') AND (LikeTerm LIKE '%Tester''%' OR ContainsTerm LIKE '%Tester''%')";
            Assert.AreEqual(expected.NormalizeResult(), actual.NormalizeResult(), "Expecting trailing single quote is escaped.");

            sql = "Tom Tester's".ParseSqlMarkdown<SelectableQueryModel>();
            actual = sql;
            actual.ToClipboard();
            actual.ToClipboardExpected();
            expected = @" 
SELECT * FROM items WHERE
(LikeTerm LIKE '%Tom%' OR ContainsTerm LIKE '%Tom%') AND (LikeTerm LIKE '%Tester''s%' OR ContainsTerm LIKE '%Tester''s%')";
            Assert.AreEqual(expected.NormalizeResult(), actual.NormalizeResult(), "Expecting trailing single quote is escaped.");

            sql = @"""Tom Tester".ParseSqlMarkdown<SelectableQueryModel>();
            actual = sql;
            actual.ToClipboard();
            actual.ToClipboardExpected();
            expected = @" 
SELECT * FROM items WHERE
(LikeTerm LIKE '%""Tom%' OR ContainsTerm LIKE '%""Tom%') AND (LikeTerm LIKE '%Tester%' OR ContainsTerm LIKE '%Tester%')";
            Assert.AreEqual(expected.NormalizeResult(), actual.NormalizeResult(), "Expecting unclosed double quote treated as literal.");

            sql = @"""Tom Tester'".ParseSqlMarkdown<SelectableQueryModel>();
            actual = sql;
            actual.ToClipboard();
            actual.ToClipboardExpected();
            expected = @" 
SELECT * FROM items WHERE
(LikeTerm LIKE '%""Tom%' OR ContainsTerm LIKE '%""Tom%') AND (LikeTerm LIKE '%Tester''%' OR ContainsTerm LIKE '%Tester''%')";
            Assert.AreEqual(expected.NormalizeResult(), actual.NormalizeResult(), "Expecting inner single quote escaped within unmatched double quote.");

            sql = @"""Tom Tester's".ParseSqlMarkdown<SelectableQueryModel>();
            actual = sql;
            actual.ToClipboard();
            actual.ToClipboardExpected();
            expected = @" 
SELECT * FROM items WHERE
(LikeTerm LIKE '%""Tom%' OR ContainsTerm LIKE '%""Tom%') AND (LikeTerm LIKE '%Tester''s%' OR ContainsTerm LIKE '%Tester''s%')";
            Assert.AreEqual(expected.NormalizeResult(), actual.NormalizeResult(), "Expecting unclosed double quote with inner apostrophe.");

            sql = @"""Tom Tester's""".ParseSqlMarkdown<SelectableQueryModel>();
            actual = sql;
            actual.ToClipboard();
            actual.ToClipboardExpected();
            expected = @" 
SELECT * FROM items WHERE
(LikeTerm LIKE '%Tom Tester''s%' OR ContainsTerm LIKE '%Tom Tester''s%')";
            Assert.AreEqual(expected.NormalizeResult(), actual.NormalizeResult(), "Expecting atomic term with apostrophe escaped.");
            #region V A R I A N T
            // We successfully have made an atomic term, and it is NOT QUOTED in the query.
            // Now, can you surround that atomic term with quotes?

            sql = @"""""""Tom Tester's""""""".ParseSqlMarkdown<SelectableQueryModel>();


            actual = sql;
            actual.ToClipboard();
            actual.ToClipboardExpected();
            actual.ToClipboardAssert("Expecting an atomic term, surrounded by quotes (i.e. the outside quotes pair).");
            { }
            expected = @" 
SELECT * FROM items WHERE
(LikeTerm LIKE '%""""Tom Tester''s""""%' OR ContainsTerm LIKE '%""""Tom Tester''s""""%')";

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting an atomic term, surrounded by quotes (i.e. the outside quotes pair)."
            );
            #endregion V A R I A N T

            sql = @"You said, """.ParseSqlMarkdown<SelectableQueryModel>();
            actual = sql;
            actual.ToClipboard();
            actual.ToClipboardExpected();
            expected = @" 
SELECT * FROM items WHERE
(LikeTerm LIKE '%You%' OR ContainsTerm LIKE '%You%') AND (LikeTerm LIKE '%said,%' OR ContainsTerm LIKE '%said,%') AND (LikeTerm LIKE '%""%' OR ContainsTerm LIKE '%""%')";
            Assert.AreEqual(expected.NormalizeResult(), actual.NormalizeResult(), "Expecting unmatched double quote treated as literal.");

            sql = @"You said, """"".ParseSqlMarkdown<SelectableQueryModel>();
            actual = sql;
            actual.ToClipboard();
            actual.ToClipboardExpected();
            { }
            expected = @" 
SELECT * FROM items WHERE
(LikeTerm LIKE '%You%' OR ContainsTerm LIKE '%You%') AND (LikeTerm LIKE '%said,%' OR ContainsTerm LIKE '%said,%') AND (LikeTerm LIKE '%""%' OR ContainsTerm LIKE '%""%')"
            ;
            Assert.AreEqual(expected.NormalizeResult(), actual.NormalizeResult(), "Expecting escaped double quote treated as literal.");

            sql = @"You said, """"H".ParseSqlMarkdown<SelectableQueryModel>();
            actual = sql;
            actual.ToClipboard();
            actual.ToClipboardExpected();
            { }
            expected = @" 
SELECT * FROM items WHERE
(LikeTerm LIKE '%You%' OR ContainsTerm LIKE '%You%') AND (LikeTerm LIKE '%said,%' OR ContainsTerm LIKE '%said,%') AND (LikeTerm LIKE '%""H%' OR ContainsTerm LIKE '%""H%')"
            ;
            Assert.AreEqual(expected.NormalizeResult(), actual.NormalizeResult(), "Expecting escaped quote followed by text.");
            sql = @"You said, """"Hello""".ParseSqlMarkdown<SelectableQueryModel>();
            actual = sql;
            expected = @" 
SELECT * FROM items WHERE
(LikeTerm LIKE '%You%' OR ContainsTerm LIKE '%You%') AND (LikeTerm LIKE '%said,%' OR ContainsTerm LIKE '%said,%') AND (LikeTerm LIKE '%""Hello""%' OR ContainsTerm LIKE '%""Hello""%')"
            ;
            Assert.AreEqual(expected.NormalizeResult(), actual.NormalizeResult(), "Expecting escaped double quote followed by atomic Hello.");



            sql = @"You said, """"Hello"""".".ParseSqlMarkdown<SelectableQueryModel>();
            actual = sql;
            actual.ToClipboard();
            actual.ToClipboardExpected();
            { }
            expected = @" 
SELECT * FROM items WHERE
(LikeTerm LIKE '%You%' OR ContainsTerm LIKE '%You%') AND (LikeTerm LIKE '%said,%' OR ContainsTerm LIKE '%said,%') AND (LikeTerm LIKE '%""Hello"".%' OR ContainsTerm LIKE '%""Hello"".%')"
            ;
            Assert.AreEqual(expected.NormalizeResult(), actual.NormalizeResult(), @"Expecting DOES NOT ATOMIZE Hello. It should be a single term surrounded by quotes instead");
        }

        [TestMethod]
        public void Test_AtomicSQuotes()
        {
            string actual, expected;
            string sql;
            List<SelectableQueryModel> recordset;
            MarkdownContext context;
            ValidationState validationState = ValidationState.Valid;


            sql = @"'Tom Tester'".ParseSqlMarkdown<SelectableQueryModel>();

            actual = sql;
            actual.ToClipboard();
            actual.ToClipboardExpected();
            actual.ToClipboardAssert("Expecting term between squotes is insulated from linting.");
            { }
            expected = @" 
SELECT * FROM items WHERE
(LikeTerm LIKE '%Tom Tester%' OR ContainsTerm LIKE '%Tom Tester%')";

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting term not to change"
            );

            sql = @"'Tom ""safe inner"" Tester'".ParseSqlMarkdown<SelectableQueryModel>();

            // {93B1FE29-F593-47EC-9CFA-F2706E79AA9E}
            actual = sql;
            actual.ToClipboard();
            actual.ToClipboardExpected();
            actual.ToClipboardAssert("");
            { }
            expected = @" 
SELECT * FROM items WHERE
(LikeTerm LIKE '%Tom ""safe inner"" Tester%' OR ContainsTerm LIKE '%Tom ""safe inner"" Tester%')";

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting three terms, because d beats s."
            );

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting three terms, because d beats s."
            );
        }

        /// <summary>
        /// PFAW!
        /// </summary>
        [TestMethod]
        public async Task Test_CustomSQLiteFunction()
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

                var recordset = cnx.Query<SelectableQueryModel>(
                    $@"
Select *
From items 
Where PropertyValue({nameof(SelectableQueryModel.Properties)}, '{nameof(SelectableQueryModel.Description)}') LIKE '%brown dog%'");
                { }
            }
        }
        [TestMethod]
        public async Task Test_DemoFlow()
        {
            using (var cnx = InitializeInMemoryDatabase())
            {
                string actual, expected, sql;
                List<SelectableQueryModel> recordset;
                SelectableQueryModel[] itemsArray;
                MarkdownContext context;
                ValidationState state;
                SenderEventPair sep;
                NotifyCollectionChangedEventArgs? ecc;
                SelectableQueryModel[] newItems = [];
                Queue<SenderEventPair> eventQueue = new();

                var items = new ObservableQueryFilterSource<SelectableQueryModel>();
                NavSearchBar nsb = new NavSearchBar
                {
                    // NavSearchBar UI controls are designed
                    // to switch out sources many times.
                    ItemsSource = items,
                };
                items.InputTextSettled += (sender, e) =>
                {
                    if (sender is IObservableQueryFilterSource qfs)
                    {
                        switch (qfs.SearchEntryState)
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
                                sql = qfs.InputText.ParseSqlMarkdown<SelectableQueryModel>();
                                recordset = cnx.Query<SelectableQueryModel>(sql);
                                items.ReplaceItems(recordset);
                                break;
                            case SearchEntryState.QueryCompleteNoResults:
                                break;
                            case SearchEntryState.QueryCompleteWithResults:
                                break;
                            default:
                                break;
                        }
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

                nsb.InputText = "animal";
                await localSettle();
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
                                ?.OfType<SelectableQueryModel>()
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





                #region S U B T E S T S
                #endregion S U B T E S T S

                #region L o c a l F x
                async Task localSettle(double seconds = 1.0) =>
                    await Task.Delay(TimeSpan.FromSeconds(seconds));
                #endregion L o c a l F x
            }
        }
    }
}