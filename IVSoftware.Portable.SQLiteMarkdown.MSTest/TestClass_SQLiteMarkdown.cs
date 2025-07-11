using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using IVSoftware.Portable.Disposable;
using IVSoftware.Portable.SQLiteMarkdown.MSTest.Models;
using IVSoftware.WinOS.MSTest.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SQLite;
using Ignore = Microsoft.VisualStudio.TestTools.UnitTesting.IgnoreAttribute;

namespace IVSoftware.Portable.SQLiteMarkdown.MSTest
{
    [TestClass]
    public class UnitTestSQLiteParser
    {
        #region D H O S T    D I S P O S A B L E    D A T A B A S E S

        private class DatabaseDisposableHost : DisposableHost
        {
            public DatabaseDisposableHost()
            {
                while (File.Exists(DisposableDatabasePathForTest))
                {
                    try
                    {
                        File.Delete(DisposableDatabasePathForTest);
                    }
                    catch (IOException)
                    {
                        switch (
                            MessageBox.Show(
                                "Make sure database is not open in DB Browser, then click OK",
                                "Deletion failed",
                                MessageBoxButtons.OKCancel))
                        {
                            case DialogResult.OK:
                                try
                                {
                                    File.Delete(DisposableDatabasePathForTest);
                                }
                                catch (IOException)
                                {
                                    Debug.WriteLine($"ADVISORY: Disposable database is still open in DB Browser for SQLite");
                                }
                                break;
                            default:
                            case DialogResult.Cancel:
                                goto breakFromInner;
                        }
                    }
                }
                breakFromInner:

                // NuGet Disposable TODO:
                // - ADD CTOR for default BeginUsing and FinalDispose.
                // - ADD this[Enum] indexer.
                // - ADD GetValue<T>(key, default); 

                BeginUsing += (sender, e) =>
                {
                    _onDisk = new SQLiteConnection(DisposableDatabasePathForTest);
                    _inMemory = new SQLiteConnection(":memory:");
                };
                FinalDispose += (sender, e) =>
                {
                    InMemory.Dispose();
                    OnDisk.Dispose();
                    if (File.Exists(DisposableDatabasePathForTest))
                    {
                        try
                        {
                            File.Delete(DisposableDatabasePathForTest);
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"ADVISORY: Disposable database is still open in DB Browser for SQLite");
                        }
                    }
                };
            }
            public SQLiteConnection InMemory => _inMemory ?? throw new NullReferenceException();
            private SQLiteConnection? _inMemory = null;
            public SQLiteConnection OnDisk => _onDisk ?? throw new NullReferenceException();
            private SQLiteConnection? _onDisk = null;
            public static string DisposableDatabasePathForTest
            {
                get
                {
                    if (_databasePathForTest is null)
                    {
                        _databasePathForTest = Path.Combine(
                            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                            "IVSoftware",
                            "Portable",
                            "SQLiteMarkdown.MSTest",
                            "disposable.db"
                        );
                    }
                    if (_databasePathForTest is null) throw new NullReferenceException();
                    Directory.CreateDirectory(Path.GetDirectoryName(_databasePathForTest)!);
                    _databasePathForTest.ToClipboard();
                    return _databasePathForTest;
                }
            }
            static string? _databasePathForTest = default;
        }
        DatabaseDisposableHost DHostDatabase
        {
            get
            {
                if (_dhostDatabase is null)
                {
                    _dhostDatabase = new DatabaseDisposableHost();
                }
                return _dhostDatabase;
            }
        }
        DatabaseDisposableHost? _dhostDatabase = default;
        #endregion D H O S T    D I S P O S A B L E    D A T A B A S E S

        #region T R A C E    L I S T E N E R
        class MSTestTraceListener : TraceListener
        {
            public override void Write(string? message)
            {
            }

            public override void WriteLine(string? message)
            {
            }
        }

        static MSTestTraceListener trace
        {
            get
            {
                if (_trace is null)
                {
                    _trace = new MSTestTraceListener();
                }
                return _trace;
            }
        }
        static MSTestTraceListener? _trace = default;


        /// <summary>
        /// Sets up a custom Trace Listener to intercept Debug.WriteLine messages for the test class.
        /// </summary>
        /// <param name="context">The test context, providing information about the test run.</param>
        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            Trace.Listeners.Add(new MSTestTraceListener());
        }
        [ClassCleanup]
        public static void ClassCleanup()
        {
            if (_trace is not null)
            {
                Trace.Listeners.Remove(_trace);
                _trace.Dispose();
                _trace = null;
            }
        }
        #endregion T R A C E    L I S T E N E R


        /// <summary>
        /// Enum representing the review status of each test entry.
        /// </summary>
        enum ReviewStatus
        {
            /// <summary>Indicates that the result is proposed and has not been reviewed yet.</summary>
            Proposed,
            /// <summary>Indicates that the current result is correct.</summary>
            ReviewPassed,
            /// <summary>Indicates that the current result is incorrect.</summary>
            CurrentLimitIsWrong,
        }

        /// <summary>
        /// Represents a test entry for an expression with properties for expression details, 
        /// expected results, validation state, and review status.
        /// </summary>
        private class ExpressionTestEntry
        {
            /// <summary>
            /// Implicitly converts a string to an ExpressionTestEntry by assigning the string to Expr.
            /// </summary>
            /// <param name="expr">The expression string to convert.</param>
            public static implicit operator ExpressionTestEntry(string expr) =>
                new ExpressionTestEntry { Expr = expr };

            /// <summary>Description of the test case.</summary>
            public string? Description { get; set; }
            /// <summary>The SQL expression being tested.</summary>
            public string? Expr { get; set; }
            /// <summary>The expected result of the expression's evaluation.</summary>
            public string? Expected { get; set; }
            /// <summary>The expected validation state of the expression.</summary>
            public ValidationState ExpectedValidationState { get; set; }
            /// <summary>The current review status of this test entry.</summary>
            public ReviewStatus ReviewStatus { get; set; }

            internal string Report(MarkdownContextOR context, ValidationState validationState)
            {
                var builder = new List<string>
                {
                    $"{Expr}: {Description}"
                };
                string actual = context;
                if (string.IsNullOrWhiteSpace(actual))
                {
                    builder.Add($"{validationState}");
                }
                else
                {
                    builder.Add(actual);
                }
                builder.Add($"{ReviewStatus.GetType().Name}.{ReviewStatus}");
                builder.Add(string.Empty);
                return string.Join(Environment.NewLine, builder);
            }
        }

        /// <summary>
        /// A dictionary of expression test entries, dynamically generated or loaded from JSON, 
        /// used to validate SQL expressions against expected results.
        /// </summary>
        Dictionary<string, ExpressionTestEntry> ExpressionTable
        {
            get
            {
                bool asEmpty = true;

                // Check if the table is null and initialize it if needed
                if (_expressionTable is null)
                {
                    _expressionTable = JsonConvert.DeserializeObject<Dictionary<string, ExpressionTestEntry>>
                    (
                        asEmpty ?
                            // Initialize an empty serialized dictionary if `asEmpty` is true
                            JsonConvert.SerializeObject(new Dictionary<string, ExpressionTestEntry>()) :
                           @" 
{
  "":Empty"": {
    ""Description"": ""Empty"",
    ""Expr"": """",
    ""Expected"": """",
    ""ExpectedValidationState"": 0,
    ""ReviewStatus"": 0
  },
  ""a:Initial single character 'a' - expected invalid state"": {
    ""Description"": ""Initial single character 'a' - expected invalid state"",
    ""Expr"": ""a"",
    ""Expected"": """",
    ""ExpectedValidationState"": 1,
    ""ReviewStatus"": 0
  },
  ""an:Intermediate two-character expression 'an' - expected invalid state"": {
    ""Description"": ""Intermediate two-character expression 'an' - expected invalid state"",
    ""Expr"": ""an"",
    ""Expected"": """",
    ""ExpectedValidationState"": 1,
    ""ReviewStatus"": 0
  },
  ""ani:Valid expression 'ani' - expected valid state"": {
    ""Description"": ""Valid single term expression 'ani' - expected valid state"",
    ""Expr"": ""ani"",
    ""Expected"": ""SELECT * FROM items WHERE\r\n(LikeTerm LIKE '%ani%' OR ContainsTerm LIKE '%ani%' OR TagMatchTerm LIKE '%[ani]%')"",
    ""ExpectedValidationState"": 2,
    ""ReviewStatus"": 0
  },
  ""ani color:Basic AND condition between two terms"": {
    ""Description"": ""Basic AND condition between two terms"",
    ""Expr"": ""ani color"",
    ""Expected"": ""SELECT * FROM items WHERE\r\n(LikeTerm LIKE '%ani%' OR ContainsTerm LIKE '%ani%' OR TagMatchTerm LIKE '%[ani]%') AND (LikeTerm LIKE '%color%' OR ContainsTerm LIKE '%color%' OR TagMatchTerm LIKE '%[color]%')"",
    ""ExpectedValidationState"": 2,
    ""ReviewStatus"": 0
  },
  ""ani & color:Alternative AND condition with &"": {
    ""Description"": ""Alternative AND condition with &"",
    ""Expr"": ""ani & color"",
    ""Expected"": ""SELECT * FROM items WHERE\r\n(LikeTerm LIKE '%ani%' OR ContainsTerm LIKE '%ani%' OR TagMatchTerm LIKE '%[ani]%') AND (LikeTerm LIKE '%color%' OR ContainsTerm LIKE '%color%' OR TagMatchTerm LIKE '%[color]%')"",
    ""ExpectedValidationState"": 2,
    ""ReviewStatus"": 0
  },
  "" ani color :Expression with leading and trailing spaces"": {
    ""Description"": ""Expression with leading and trailing spaces"",
    ""Expr"": "" ani color "",
    ""Expected"": ""SELECT * FROM items WHERE\r\n(LikeTerm LIKE '%ani%' OR ContainsTerm LIKE '%ani%' OR TagMatchTerm LIKE '%[ani]%') AND (LikeTerm LIKE '%color%' OR ContainsTerm LIKE '%color%' OR TagMatchTerm LIKE '%[color]%')"",
    ""ExpectedValidationState"": 2,
    ""ReviewStatus"": 0
  },
  ""ani && color:Another AND condition with double ampersands"": {
    ""Description"": ""Another AND condition with double ampersands"",
    ""Expr"": ""ani && color"",
    ""Expected"": ""SELECT * FROM items WHERE\r\n(LikeTerm LIKE '%ani%' OR ContainsTerm LIKE '%ani%' OR TagMatchTerm LIKE '%[ani]%') AND (LikeTerm LIKE '%color%' OR ContainsTerm LIKE '%color%' OR TagMatchTerm LIKE '%[color]%')"",
    ""ExpectedValidationState"": 2,
    ""ReviewStatus"": 0
  },
  ""!fruit:Basic NOT condition"": {
    ""Description"": ""Basic NOT condition"",
    ""Expr"": ""!fruit"",
    ""Expected"": ""SELECT * FROM items WHERE\r\nNOT ((LikeTerm LIKE '%fruit%' OR ContainsTerm LIKE '%fruit%' OR TagMatchTerm LIKE '%[fruit]%'))"",
    ""ExpectedValidationState"": 2,
    ""ReviewStatus"": 0
  },
  ""Tom:Search for the name 'Tom'"": {
    ""Description"": ""Search for the name 'Tom'"",
    ""Expr"": ""Tom"",
    ""Expected"": ""SELECT * FROM items WHERE\r\n(LikeTerm LIKE '%Tom%' OR ContainsTerm LIKE '%Tom%' OR TagMatchTerm LIKE '%[Tom]%')"",
    ""ExpectedValidationState"": 2,
    ""ReviewStatus"": 0
  },
  ""brown:Search for single term 'brown'"": {
    ""Description"": ""Search for single term 'brown'"",
    ""Expr"": ""brown"",
    ""Expected"": ""SELECT * FROM items WHERE\r\n(LikeTerm LIKE '%brown%' OR ContainsTerm LIKE '%brown%' OR TagMatchTerm LIKE '%[brown]%')"",
    ""ExpectedValidationState"": 2,
    ""ReviewStatus"": 0
  },
  ""!!!:Invalid expression with special characters"": {
    ""Description"": ""Invalid expression with special characters"",
    ""Expr"": ""!!!"",
    ""Expected"": """",
    ""ExpectedValidationState"": 1,
    ""ReviewStatus"": 0
  }
}"

                    );

                    /// <summary>
                    /// Populate the ExpressionTable with specific entries if they are missing.
                    /// This ensures that essential test cases are included for validation.
                    /// </summary>
                    foreach (var exprEx in new[]
                    {
                        ":Empty",
                        "a:Initial single character 'a' - expected invalid state",
                        "an:Intermediate two-character expression 'an' - expected invalid state",
                        "ani:Valid expression 'ani' - expected valid state",
                        "ani color:Basic AND condition between two terms",
                        "ani & color:Alternative AND condition with &",
                        " ani color :Expression with leading and trailing spaces",
                        "ani && color:Another AND condition with double ampersands",
                        "!fruit:Basic NOT condition",
                        "Tom:Search for the name 'Tom'",
                        "brown:Search for single term 'brown'",
                        "!!!:Invalid expression with special characters"
                    })
                    {
                        if (!ExpressionTable.ContainsKey(exprEx))
                        {
                            ValidationState state = ValidationState.Empty;
                            var split = exprEx.Split(':');
                            ExpressionTable[exprEx] = new ExpressionTestEntry
                            {
                                Description = split[1],
                                Expr = split[0],
                                Expected = split[0].ParseSqlMarkdown<SelfIndexedItem>(ref state),
                                ExpectedValidationState = state,
                            };
                        }
                    }
                }
                if (_expressionTable is null) throw new NullReferenceException();
                return _expressionTable;
            }
        }

        /// <summary>
        /// Backing field for the ExpressionTable dictionary.
        /// </summary>
        Dictionary<string, ExpressionTestEntry>? _expressionTable = default;

        /// <summary>
        /// Test method to validate the expressions in ExpressionTable by comparing actual results
        /// with expected results and validation states.
        /// </summary>
        [TestMethod]
        public void Test_Expression_Table()
        {
            string actual;
            foreach (var eut in ExpressionTable.Values)
            {
                var validationState = ValidationState.Empty;

                // Parse the expression and update the validation state
                var context = eut.Expr.ParseSqlMarkdown<SelfIndexedItem>(ref validationState);
                // Implicit cast to string
                actual = context;

                #region F i r s t    t i m e    r e v i e w
                actual.ToClipboard();
                actual.ToClipboardAssert();

                Debug.WriteLine(eut.Report(context, validationState));

                /// <summary>
                /// Perform an initial review based on Report.
                /// The user can manually alter the reviewed status for proposed entries
                /// before serialization.
                /// </summary>
                bool 
                    changeToCorrect = false, 
                    changeToWrong = false;
                switch (eut.ReviewStatus)
                {
                    default:
                    case ReviewStatus.Proposed:
                        if(changeToCorrect)
                        {
                            eut.ReviewStatus = ReviewStatus.ReviewPassed;
                        }
                        else if( changeToWrong)
                        {
                            eut.ReviewStatus = ReviewStatus.CurrentLimitIsWrong;
                        }
                        break;
                    case ReviewStatus.ReviewPassed:
                        break;
                    case ReviewStatus.CurrentLimitIsWrong:
                        break;
                }
                #endregion F i r s t    t i m e    r e v i e w

                // Assert that the actual validation state matches the expected state
                Assert.AreEqual(
                    eut.ExpectedValidationState,
                    validationState, $"Unexpected validation state");

                // Assert that the actual generated query matches the expected query
                Assert.IsNotNull(eut.Expected);
                Assert.AreEqual(
                    eut.Expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    $"Expected query does not match for expression '{eut.Expr}'");
            }

            // Output the count of tested entries to the console
            Console.WriteLine($"Tested {ExpressionTable.Count} {nameof(ExpressionTable)} entries");

            // Copy the serialized ExpressionTable to the clipboard.
            // This is a candidate for an updated version of the
            // default JSON and can be pasted into the C# code.
            $@"@"" 
{JsonConvert.SerializeObject(ExpressionTable, Formatting.Indented).Replace(@"""", @"""""")}"" 
".ToClipboard();
            { }
        }

        [TestMethod]
        public void Test_UserIndexedModel()
        {
            using (DHostDatabase.GetToken())
            {
                var cnx = DHostDatabase.OnDisk;
                string actual, expected, expr;
                SelfIndexedProfile[] retrievedItems;
                cnx.CreateTable<SelfIndexedProfile>();
                var items = new SelfIndexedProfile[]
                {
                    new SelfIndexedProfile
                    {
                        FirstName = "Tom",
                        LastName = "Tester",
                        Tags = "C# .NET MAUI,C# WPF, C# WinForms".MakeTags(),
                    }
                };
                actual =
                    JsonConvert
                    .SerializeObject(items, Formatting.Indented)
                    .Replace(@"\r\n", string.Empty);
                actual.ToClipboard();
                actual.ToClipboardExpected();
                { }
                expected = @" 
[
  {
    ""Id"": ""38CFE38E-0D90-4C9F-A4E5-845089CB2BB0"",
    ""FirstName"": ""Tom"",
    ""LastName"": ""Tester"",
    ""Tags"": ""[c# .net maui][c# wpf][c# winforms]"",
    ""PrimaryKey"": ""38CFE38E-0D90-4C9F-A4E5-845089CB2BB0"",
    ""QueryTerm"": ""tom~tester"",
    ""FilterTerm"": ""tom~tester"",
    ""TagMatchTerm"": ""[c# .net maui][c# wpf][c# winforms]"",
    ""Properties"": ""{  \""FirstName\"": \""Tom\"",  \""LastName\"": \""Tester\"",  \""Tags\"": \""[c# .net maui][c# wpf][c# winforms]\""}""
  }
]"
                ;

                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting values to match."
                );

                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Terms are expected to be indexed 'immediately' and without settling."
                );

                cnx.CreateTable<SelfIndexedProfile>();
                Assert.AreEqual(
                    1,
                    cnx.InsertAll(items));
                DatabaseDisposableHost.DisposableDatabasePathForTest.ToClipboard();
                // First, look at ALL the ONE records
                Assert.AreEqual(
                    items.Length, 
                    cnx.Table<SelfIndexedProfile>().Count(),
                    $"Expecting all items have been inserted into the table."
                );

                #region Q U E R Y    W I T H O U T    M A R K D O W N

                // CONTROL:
                // Get 'all" the items (SPOILER: there is 1 item) then
                // make sure the filtered query returns the same result.
                // SETTLE
                // There is also a settling time to take into account.
                retrievedItems = cnx.Table<SelfIndexedProfile>().ToArray();
                actual = JsonConvert.SerializeObject(retrievedItems, Formatting.Indented);
                actual.ToClipboard();
                actual.ToClipboardAssert();

                actual = JsonConvert.SerializeObject(retrievedItems, Formatting.Indented);
                actual.ToClipboard();

                actual.ToClipboardExpected();
                { }
                expected = @" 
[
  {
    ""Id"": ""38CFE38E-0D90-4C9F-A4E5-845089CB2BB0"",
    ""FirstName"": ""Tom"",
    ""LastName"": ""Tester"",
    ""Tags"": ""[c# .net maui][c# wpf][c# winforms]"",
    ""PrimaryKey"": ""38CFE38E-0D90-4C9F-A4E5-845089CB2BB0"",
    ""QueryTerm"": ""tom~tester"",
    ""FilterTerm"": ""tom~tester"",
    ""TagMatchTerm"": ""[c# .net maui][c# wpf][c# winforms]"",
    ""Properties"": ""{\r\n  \""FirstName\"": \""Tom\"",\r\n  \""LastName\"": \""Tester\"",\r\n  \""Tags\"": \""[c# .net maui][c# wpf][c# winforms]\""\r\n}""
  }
]"
                ;

                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Terms are expected to be indexed 'immediately' and without settling."
                );


                expr = "Tom";
                var query = $@"
                SELECT * FROM {nameof(SelfIndexedProfile)}
                WHERE {nameof(SelfIndexedProfile.QueryTerm)} LIKE '%{expr}%'
                   OR {nameof(SelfIndexedProfile.FilterTerm)} LIKE '%{expr}%'
                   OR {nameof(SelfIndexedProfile.TagMatchTerm)} LIKE '%[{expr}]%'";

                query.ToClipboard();
                query.ToClipboardAssert();

                actual = query;

                // Looking at the hand-typed QUERY FORMAT ITSELF.
                expected = @" 

                SELECT * FROM SelfIndexedProfile
                WHERE QueryTerm LIKE '%Tom%'
                   OR FilterTerm LIKE '%Tom%'
                   OR TagMatchTerm LIKE '%[Tom]%'"
                ;

                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting values to match."
                );

                retrievedItems = cnx.Query<SelfIndexedProfile>(query).ToArray();
                actual = JsonConvert.SerializeObject(retrievedItems, Formatting.Indented);

                actual.ToClipboardExpected();
                { }
                expected = @" 
[
  {
    ""Id"": ""38CFE38E-0D90-4C9F-A4E5-845089CB2BB0"",
    ""FirstName"": ""Tom"",
    ""LastName"": ""Tester"",
    ""Tags"": ""[c# .net maui][c# wpf][c# winforms]"",
    ""PrimaryKey"": ""38CFE38E-0D90-4C9F-A4E5-845089CB2BB0"",
    ""QueryTerm"": ""tom~tester"",
    ""FilterTerm"": ""tom~tester"",
    ""TagMatchTerm"": ""[c# .net maui][c# wpf][c# winforms]"",
    ""Properties"": ""{\r\n  \""FirstName\"": \""Tom\"",\r\n  \""LastName\"": \""Tester\"",\r\n  \""Tags\"": \""[c# .net maui][c# wpf][c# winforms]\""\r\n}""
  }
]"
                ;

                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Terms are expected to be indexed 'immediately' and without settling."
                );

                Assert.AreEqual(1, retrievedItems.Length, "Expecting a single item to be retrieved.");
                Assert.AreEqual("Tom", retrievedItems[0].FirstName, "Expecting first name to match.");
                Assert.AreEqual("Tester", retrievedItems[0].LastName, "Expecting last name to match.");
                #endregion Q U E R Y    W I T H O U T    M A R K D O W N
            }
        }

        [TestMethod]
        public void Test_ReflectFieldsOR()
        {
            string actual, expected;
            PropertyInfo[] likeTerms;

            var qfMode = QueryFilterMode.Query;
            var type = typeof(SelfIndexedProfile);

            likeTerms = type
                .GetProperties()
                .Where(_ =>
                    (qfMode == QueryFilterMode.Query && _.GetQueryTermAttribute() != null) ||
                    (qfMode == QueryFilterMode.Filter && _.GetFilterTermAttribute() != null))
                .ToArray();
            actual = likeTerms.Single().Name;
            expected = @" 
QueryTerm";

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting one matching property for canonical query."
            );


            likeTerms = type
                .GetProperties()
                .Where(_ =>
                    (qfMode == QueryFilterMode.Query && _.GetQueryTermAttribute() != null) ||
                    (qfMode == QueryFilterMode.Filter && _.GetFilterTermAttribute() != null))
                .ToArray();

            actual = likeTerms.Single().Name;
            expected = @" 
QueryTerm";

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting one matching property for query-mode obsolete alias redirect."
            );

            qfMode = QueryFilterMode.Filter;

            likeTerms = type
                .GetProperties()
                .Where(_ =>
                    (qfMode == QueryFilterMode.Query && _.GetQueryTermAttribute() != null) ||
                    (qfMode == QueryFilterMode.Filter && _.GetFilterTermAttribute() != null))
                .ToArray();
            actual = likeTerms.Single().Name;
            expected = @" 
FilterTerm";

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting one matching property for canonical query."
            );



            likeTerms = type
                .GetProperties()
                .Where(_ =>
                    (qfMode == QueryFilterMode.Query && _.GetQueryTermAttribute() != null) ||
                    (qfMode == QueryFilterMode.Filter && _.GetFilterTermAttribute() != null))
                .ToArray();

            { }

            actual = likeTerms.Single().Name;
            expected = @" 
FilterTerm";

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting one matching property for query-mode obsolete alias redirect."
            );

            qfMode = QueryFilterMode.Filter;

            var tagTerms = type
                .GetProperties()
                .Where(p =>
                    (p.GetCustomAttribute<TagMatchTermAttribute>() != null))
                .ToArray();
        }
    }
}
