using IVSoftware.Portable.Common.Exceptions;
using IVSoftware.Portable.Disposable;
using IVSoftware.Portable.SQLiteMarkdown.Common;
using IVSoftware.Portable.SQLiteMarkdown.MSTest.Models;
using IVSoftware.Portable.SQLiteMarkdown.Util;
using IVSoftware.WinOS.MSTest.Extensions;
using Newtonsoft.Json;
using SQLite;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using Ignore = Microsoft.VisualStudio.TestTools.UnitTesting.IgnoreAttribute;

namespace IVSoftware.Portable.SQLiteMarkdown.MSTest
{
    [TestClass]
    public partial class TestClass_TestClass_SQLiteMarkdown
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
        /// Supplemental test after production state error detected.
        /// </summary>
        /// <remarks>
        /// State machine failed to return to Cleared after consecutive [X].
        /// </remarks>
        [TestMethod, DoNotParallelize]
        public async Task Test_QueryFilterFSMs()
        {
            using var te = this.TestableEpoch();

            // MSTest internal consideration. This is about tests that hang.
            Assert.IsNull(SynchronizationContext.Current);

            string actual, expected;
            var builder = new List<string>();
            var extQueryHandle = default(List<PrioritizedAffinityQFModel>);
            int COUNT;

            var mmdc = new ModeledMarkdownContext<PrioritizedAffinityQFModel>();

            actual = mmdc.StateReport();
            actual.ToClipboardExpected();
            { }
            expected = @" 
[IME Len: 0, IsFiltering: False], [Net: null, CC: 0, PMC: 0], [QueryAndFilter: SearchEntryState.Cleared, FilteringState.Ineligible]"
            ;
            Assert.AreEqual(expected.NormalizeResult(), actual.NormalizeResult(), "Expecting MDC Defaults.");


            await subtestExtQueryNoResult();
            await subtestExtQueryOneResult();
            await subtestExtQueryTwoResults();

            await subtestClearAwaiterOnly();
            await subtestQueryWithResultsClearSequence();
            await subtestQueryWithFilteredResultsClearSequence();

            #region S U B T E S T S 

            // A query that returns no results should *visually indicate* SearchEntryState
            // Correct  : QueryCompleteNoResults
            // Incorrect: Cleared
            async Task subtestExtQueryNoResult()
            {
                COUNT = 0;  // The 'query' has returned no matches.
                mmdc.LoadCanon(extQueryHandle.PopulateForDemo(COUNT));
                actual = mmdc.StateReport();
                actual.ToClipboardExpected();
                { } // <- FIRST TIME ONLY: Adjust the message.
                actual.ToClipboardAssert("Expecting result to match.");
                { }
                expected = @" 
[IME Len: 0, IsFiltering: False], [Net: null, CC: 0, PMC: 0], [QueryAndFilter: SearchEntryState.QueryCompleteNoResults, FilteringState.Ineligible]";

                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting result to match."
                );
            }

            async Task subtestExtQueryOneResult()
            {
                COUNT = 1;
                mmdc.LoadCanon(extQueryHandle.PopulateForDemo(COUNT));
                Assert.AreEqual(COUNT, mmdc.CanonicalCount);
                Assert.AreEqual(SearchEntryState.QueryCompleteWithResults, mmdc.SearchEntryState);
                Assert.AreEqual(FilteringState.Ineligible, mmdc.FilteringState);
                Assert.IsFalse(mmdc.IsFiltering);
            }
            async Task subtestExtQueryTwoResults()
            {
                void localMDC_PropertyChanged(object? sender, PropertyChangedEventArgs e)
                {
                    builder.Add(e.PropertyName ?? "null");
                }
                using var local = this.WithOnDispose(
                    onInit: (sender, e) =>
                    {
                        mmdc.PropertyChanged += localMDC_PropertyChanged;
                    },
                    onDispose: (sender, e) =>
                    {
                        mmdc.PropertyChanged -= localMDC_PropertyChanged;
                    });

                COUNT = 2;
                builder.Clear();

                mmdc.InputText = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

                // The point of this is that there should *not* be any need to settle!
                // We're giving a window for Running to go true - which it *should not* do!
                // In other words, we're testing for the absence of something.
                await Task.Delay(250);

                actual = string.Join(Environment.NewLine, builder);
                expected = @" 
SearchEntryState
InputText"
                ;

                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting *no* changes to Running."
                );

                Assert.AreEqual(SearchEntryState.QueryEN, mmdc.SearchEntryState, "Expecting mdc perceives a valid query.");

                // SIMULATE - Now perform the external QUERY.
                mmdc.LoadCanon(extQueryHandle.PopulateForDemo(COUNT));
                actual = mmdc.StateReport();
                actual.ToClipboardExpected();
                { }
                expected = @" 
[IME Len: 62, IsFiltering: True], [Net: null, CC: 2, PMC: 2], [QueryAndFilter: SearchEntryState.QueryCompleteWithResults, FilteringState.Armed]"
                ;
                Assert.AreEqual(expected.NormalizeResult(), actual.NormalizeResult(), "NEW RECORDSET 2 ITEMS");

                // This will clear the IME.
                // IsFiltering=TRUE. Don't dip below SearchEntryState.QueryCompleteWithResults.
                mmdc.Clear(false);
                actual = mmdc.StateReport();
                actual.ToClipboardExpected();
                { }
                expected = @" 
[IME Len: 0, IsFiltering: True], [Net: null, CC: 2, PMC: 2], [QueryAndFilter: SearchEntryState.QueryCompleteWithResults, FilteringState.Armed]"
                ;

                Assert.AreEqual(expected.NormalizeResult(), actual.NormalizeResult(), "IME CLEAR ONLY");

                // This will exit filter mode leaving list intact.
                mmdc.Clear(false);

                actual = mmdc.StateReport();
                actual.ToClipboardExpected();
                { }
                expected = @" 
[IME Len: 0, IsFiltering: False], [Net: null, CC: 2, PMC: 2], [QueryAndFilter: SearchEntryState.QueryEmpty, FilteringState.Ineligible]"
                ;
                Assert.AreEqual(expected.NormalizeResult(), actual.NormalizeResult(), "Expecting StateReport to match.");
                Assert.IsTrue(mmdc.CanonicalCount == 2);

                // This is the terminal state and will clear the projection.
                mmdc.Clear(false);

                actual = mmdc.StateReport();
                actual.ToClipboardExpected();
                { }
                expected = @" 
[IME Len: 0, IsFiltering: False], [Net: null, CC: 0, PMC: 0], [QueryAndFilter: SearchEntryState.Cleared, FilteringState.Ineligible]"
                ;
                Assert.AreEqual(expected.NormalizeResult(), actual.NormalizeResult(), "Expecting StateReport to match.");
            }
            async Task subtestClearAwaiterOnly()
            {
                mmdc.Clear(all: true);
                await mmdc;
                mmdc.Clear(all: true);
                await mmdc;
            };
            async Task subtestQueryWithResultsClearSequence()
            {
                mmdc.Clear(all: true);
                mmdc.InputText = "valid query";

                Assert.AreEqual(SearchEntryState.QueryEN, mmdc.SearchEntryState, "Expecting initial state.");
                Assert.AreEqual(FilteringState.Ineligible, mmdc.FilteringState, "Expecting initial state.");

                // Query occurs.
                mmdc.LoadCanon(extQueryHandle.PopulateForDemo(2));

                Assert.AreEqual(SearchEntryState.QueryCompleteWithResults, mmdc.SearchEntryState, "Expecting initial state.");
                Assert.AreEqual(FilteringState.Armed, mmdc.FilteringState, "Expecting initial state.");

                // #1 [X]
                // User clears the input text, but *not* the recordset.
                // FilteringState remains Armed because the transition is from non-empty input text to empty.
                // IsFiltering
                mmdc.Clear();

                actual = mmdc.StateReport();
                actual.ToClipboardExpected();
                expected = @" 
[IME Len: 0, IsFiltering: True], [Net: null, CC: 2, PMC: 2], [QueryAndFilter: SearchEntryState.QueryCompleteWithResults, FilteringState.Armed]"
                ;
                Assert.AreEqual(expected.NormalizeResult(), actual.NormalizeResult(), "Expecting StateReport to match.");

                // #2 [X]
                // User returns to Query without emptying the list.
                mmdc.Clear();
                actual = mmdc.StateReport();
                actual.ToClipboardExpected();
                { }
                expected = @" 
[IME Len: 0, IsFiltering: False], [Net: null, CC: 2, PMC: 2], [QueryAndFilter: SearchEntryState.QueryEmpty, FilteringState.Ineligible]"
                ;
                Assert.AreEqual(expected.NormalizeResult(), actual.NormalizeResult(), "Expecting StateReport to match.");

                Assert.AreEqual(SearchEntryState.QueryEmpty, mmdc.SearchEntryState, "Expecting initial state.");
                Assert.AreEqual(FilteringState.Ineligible, mmdc.FilteringState, "Expecting initial state.");

                // #3 [X]
                // The MCD can clear its own state heuristically, rather than epistemically.
                // Even without knowledge of the list contents, these combined states are the signal:
                // - FilteringState.Ineligible | SearchEntryState.QueryCompleteWithResults
                // THIS IS THE ACTION THAT WAS FAILING IN PRODUCTION and REPLICATED before fixing.
                mmdc.Clear();
                actual = mmdc.StateReport();
                actual.ToClipboardExpected();
                { }
                expected = @" 
[IME Len: 0, IsFiltering: False], [Net: null, CC: 0, PMC: 0], [QueryAndFilter: SearchEntryState.Cleared, FilteringState.Ineligible]"
                ;
                Assert.AreEqual(expected.NormalizeResult(), actual.NormalizeResult(), "Expecting StateReport to match.");
            }
            async Task subtestQueryWithFilteredResultsClearSequence()
            {
                mmdc.Clear(all: true);
                await mmdc;
                mmdc.InputText = "valid query";

                actual = mmdc.StateReport();
                actual.ToClipboardExpected();
                { }
                expected = @" 
[IME Len: 11, IsFiltering: False], [Net: null, CC: 0, PMC: 0], [QueryAndFilter: SearchEntryState.QueryEN, FilteringState.Ineligible]"
                ;
                Assert.AreEqual(expected.NormalizeResult(), actual.NormalizeResult(), "Expecting StateReport to match.");

                // Query occurs.
                mmdc.LoadCanon(extQueryHandle.PopulateForDemo(2));

                actual = mmdc.StateReport();
                actual.ToClipboardExpected();
                { }
                expected = @" 
[IME Len: 11, IsFiltering: True], [Net: null, CC: 2, PMC: 2], [QueryAndFilter: SearchEntryState.QueryCompleteWithResults, FilteringState.Armed]"
                ;
                Assert.AreEqual(expected.NormalizeResult(), actual.NormalizeResult(), "Expecting StateReport to match.");

                // Filtering occurs
                mmdc.InputText = "item 2";
                await mmdc;

                actual = mmdc.Model.ToString();
                actual.ToClipboardExpected();
                { }
                expected = @" 
<model mdc=""[MMDC]"" autocount=""2"" count=""2"" matches=""1"">
  <xitem text=""312d1c21-0000-0000-0000-000000000005"" model=""[PrioritizedAffinityQFModel]"" preview=""Item01    "" sort=""0"" />
  <xitem text=""312d1c21-0000-0000-0000-000000000006"" model=""[PrioritizedAffinityQFModel]"" preview=""Item02    "" sort=""1"" qmatch=""True"" />
</model>"
                ;

                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting result to match."
                );
                actual = mmdc.StateReport();
                actual.ToClipboardExpected();
                { }
                expected = @" 
[IME Len: 6, IsFiltering: True], [Net: null, CC: 2, PMC: 1], [QueryAndFilter: SearchEntryState.QueryCompleteWithResults, FilteringState.Active]"
                ;

                // #1 [X]
                // User clears the input text.
                // In this case FilteringState should remain Armed.
                // because the transition is from non-empty input text to empty.
                mmdc.Clear();
                Assert.AreEqual(SearchEntryState.QueryCompleteWithResults, mmdc.SearchEntryState, "Expecting initial state.");
                Assert.AreEqual(FilteringState.Armed, mmdc.FilteringState, "Expecting initial state.");

                // #2 [X]
                // User returns to Query without emptying the list.
                mmdc.Clear();
                Assert.AreEqual(
                    SearchEntryState.QueryEmpty, // TOUCHED this limit on 260304 - QueryEmpty is the correct value
                    mmdc.SearchEntryState, "Expecting initial state.");

                Assert.AreEqual(
                    FilteringState.Ineligible,
                    mmdc.FilteringState, "Expecting initial state.");

                // #3 [X]
                // The MCD can clear its own state heuristically, rather than epistemically.
                // Even without knowledge of the list contents, these combined states are the signal:
                // - FilteringState.Ineligible | SearchEntryState.QueryCompleteWithResults
                // THIS IS THE ACTION THAT WAS FAILING IN PRODUCTION and REPLICATED before fixing.
                mmdc.Clear();
                Assert.AreEqual(SearchEntryState.Cleared, mmdc.SearchEntryState, "Expecting initial state.");
                Assert.AreEqual(FilteringState.Ineligible, mmdc.FilteringState, "Expecting initial state.");
            }
            #endregion S U B T E S T S
        }

        [TestMethod]
        public async Task Test_QueryOnlyFSMs()
        {
            string actual, expected;
                
            const int COUNT = 2;
            var extQueryHandle = default(List<SelectableQFModel>);

            var mmdc = new ModeledMarkdownContext<SelectableQFModel> { QueryFilterConfig = QueryFilterConfig.Query };
            actual = mmdc.StateReport();
            actual.ToClipboardExpected();
            { }
            expected = @" 
[IME Len: 0, IsFiltering: False], [Net: null, CC: 0, PMC: 0], [Query: SearchEntryState.Cleared, FilteringState.Ineligible]"
            ;
            Assert.AreEqual(expected.NormalizeResult(), actual.NormalizeResult(), "Expecting StateReport to match.");

            mmdc.InputText = "a";
            actual = mmdc.StateReport();
            actual.ToClipboardExpected();
            { }
            expected = @" 
[IME Len: 1, IsFiltering: False], [Net: null, CC: 0, PMC: 0], [Query: SearchEntryState.QueryENB, FilteringState.Ineligible]"
            ;
            Assert.AreEqual(expected.NormalizeResult(), actual.NormalizeResult(), "Expecting StateReport to match.");

            // Backspace
            mmdc.InputText = string.Empty;
            actual = mmdc.StateReport();
            actual.ToClipboardExpected();
            { }
            expected = @" 
[IME Len: 0, IsFiltering: False], [Net: null, CC: 0, PMC: 0], [Query: SearchEntryState.Cleared, FilteringState.Ineligible]"
            ;
            Assert.AreEqual(expected.NormalizeResult(), actual.NormalizeResult(), "Expecting StateReport to match.");
            Assert.AreEqual(SearchEntryState.Cleared, mmdc.SearchEntryState);

            mmdc.InputText = "a";
            actual = mmdc.StateReport();
            actual.ToClipboardExpected();
            { }
            expected = @" 
[IME Len: 1, IsFiltering: False], [Net: null, CC: 0, PMC: 0], [Query: SearchEntryState.QueryENB, FilteringState.Ineligible]"
            ;
            Assert.AreEqual(expected.NormalizeResult(), actual.NormalizeResult(), "Expecting StateReport to match.");

            mmdc.InputText = "an";
            actual = mmdc.StateReport();
            actual.ToClipboardExpected();
            { }
            expected = @" 
[IME Len: 2, IsFiltering: False], [Net: null, CC: 0, PMC: 0], [Query: SearchEntryState.QueryENB, FilteringState.Ineligible]"
            ;
            Assert.AreEqual(expected.NormalizeResult(), actual.NormalizeResult(), "Expecting StateReport to match.");

            mmdc.InputText = "ani";
            actual = mmdc.StateReport();
            actual.ToClipboardExpected();
            { }
            expected = @" 
[IME Len: 3, IsFiltering: False], [Net: null, CC: 0, PMC: 0], [Query: SearchEntryState.QueryEN, FilteringState.Ineligible]"
            ;
            Assert.AreEqual(expected.NormalizeResult(), actual.NormalizeResult(), "Expecting StateReport to match.");

            // Commit and load the new recordset.
            // [Remember] IsFilter is DISABLED.
            mmdc.LoadCanon(extQueryHandle.PopulateForDemo(COUNT));
            actual = mmdc.StateReport();
            actual.ToClipboardExpected();
            { }
            // [Remember]
            // The *absence* of any ismatch attributes makes
            // each and every node a perceived match.

            expected = @" 
[IME Len: 3, IsFiltering: False], [Net: null, CC: 2, PMC: 2], [Query: SearchEntryState.QueryCompleteWithResults, FilteringState.Ineligible]"
            ;
            Assert.AreEqual(expected.NormalizeResult(), actual.NormalizeResult(), "Expecting Filtering shows DISABLED.");

            // Clear the IME, *not* the recordset.
            // [Remember] Clear on MMDC resolves to Clear(bool).
            mmdc.Clear();
            actual = mmdc.StateReport();
            actual.ToClipboardExpected();
            { }
            expected = @" 
[IME Len: 0, IsFiltering: False], [Net: null, CC: 2, PMC: 2], [Query: SearchEntryState.QueryEmpty, FilteringState.Ineligible]"
            ;
            Assert.AreEqual(expected.NormalizeResult(), actual.NormalizeResult(), "Expecting StateReport to match.");

            Assert.IsTrue(mmdc.RouteToFullRecordset, "ROUTE TO CANONICAL");

            // Empty IME + Regressive Clear = TerminalClear.
            // [Remember] Clear on MMDC resolves to Clear(bool).
            mmdc.Clear();
            actual = mmdc.StateReport();
            actual.ToClipboardExpected();
            { }
            expected = @" 
[IME Len: 0, IsFiltering: False], [Net: null, CC: 0, PMC: 0], [Query: SearchEntryState.Cleared, FilteringState.Ineligible]"
            ;
            Assert.AreEqual(expected.NormalizeResult(), actual.NormalizeResult(), "Expecting StateReport to match.");
        }

        /// <summary>
        /// Verifies state initialization and transitions when operating in Filter-only mode.
        /// </summary>
        /// <remarks>
        /// Confirms that configuring <see cref="QueryFilterConfig.Filter"/> before or after
        /// context initialization results in the same effective FSM state:
        /// <c>SearchEntryState.QueryCompleteNoResults</c> with <c>FilteringState.Armed</c>.
        ///
        /// Ensures that Filter-only configuration bypasses query semantics and
        /// immediately prepares the filtering pipeline.
        /// </remarks>
        [TestMethod]
        public async Task Test_FilterOnlyFSMs()
        {
            string actual, expected;

            var extQueryHandle = default(List<SelectableQFModel>).PopulateForDemo(2);

            ModeledMarkdownContext<SelectableQFModel> mdc;

            subtest_ConfigureThenLoad();

            subtest_LoadThenConfigure();

            #region S U B T E S T S
            void subtest_ConfigureThenLoad()
            {
                mdc = new() { QueryFilterConfig = QueryFilterConfig.Filter };
                actual = mdc.StateReport();
                actual.ToClipboardExpected();
                { }
                expected = @" 
[IME Len: 0, IsFiltering: True], [Net: null, CC: 0, PMC: 0], [Filter: SearchEntryState.QueryCompleteNoResults, FilteringState.Armed]"
                ;

                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting FILTER ONLY."
                );
            }

            void subtest_LoadThenConfigure()
            {
                mdc = new();
                actual = mdc.StateReport();
                actual.ToClipboardExpected();
                { }
                expected = @" 
[IME Len: 0, IsFiltering: False], [Net: null, CC: 0, PMC: 0], [QueryAndFilter: SearchEntryState.Cleared, FilteringState.Ineligible]"
                ;

                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting QUERY AND FILTER."
                );

                mdc.QueryFilterConfig = QueryFilterConfig.Filter;
                actual = mdc.StateReport();
                actual.ToClipboardExpected();
                { }
                expected = @" 
[IME Len: 0, IsFiltering: True], [Net: null, CC: 0, PMC: 0], [Filter: SearchEntryState.QueryCompleteNoResults, FilteringState.Armed]"
                ;

                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting FILTER ONLY."
                );
            }
            #endregion S U B T E S T S
        }

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
{JsonConvert.SerializeObject(ExpressionTable, Newtonsoft.Json.Formatting.Indented).Replace(@"""", @"""""")}"" 
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
                    .SerializeObject(items, Newtonsoft.Json.Formatting.Indented)
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
    ""Tags"": ""[c# .net maui] [c# wpf] [c# winforms]"",
    ""PrimaryKey"": ""38CFE38E-0D90-4C9F-A4E5-845089CB2BB0"",
    ""QueryTerm"": ""tom~tester"",
    ""FilterTerm"": ""tom~tester"",
    ""TagMatchTerm"": ""[c# .net maui] [c# wpf] [c# winforms]"",
    ""Properties"": ""{  \""FirstName\"": \""Tom\"",  \""LastName\"": \""Tester\"",  \""Tags\"": \""[c# .net maui] [c# wpf] [c# winforms]\""}""
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
                actual = JsonConvert.SerializeObject(retrievedItems, Newtonsoft.Json.Formatting.Indented);
                actual.ToClipboard();
                actual.ToClipboardAssert();

                actual = JsonConvert.SerializeObject(retrievedItems, Newtonsoft.Json.Formatting.Indented);
                actual.ToClipboard();

                actual.ToClipboardExpected();
                { }
                expected = @" 
[
  {
    ""Id"": ""38CFE38E-0D90-4C9F-A4E5-845089CB2BB0"",
    ""FirstName"": ""Tom"",
    ""LastName"": ""Tester"",
    ""Tags"": ""[c# .net maui] [c# wpf] [c# winforms]"",
    ""PrimaryKey"": ""38CFE38E-0D90-4C9F-A4E5-845089CB2BB0"",
    ""QueryTerm"": ""tom~tester"",
    ""FilterTerm"": ""tom~tester"",
    ""TagMatchTerm"": ""[c# .net maui] [c# wpf] [c# winforms]"",
    ""Properties"": ""{\r\n  \""FirstName\"": \""Tom\"",\r\n  \""LastName\"": \""Tester\"",\r\n  \""Tags\"": \""[c# .net maui] [c# wpf] [c# winforms]\""\r\n}""
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
                actual = JsonConvert.SerializeObject(retrievedItems, Newtonsoft.Json.Formatting.Indented);

                actual.ToClipboardExpected();
                { }
                expected = @" 
[
  {
    ""Id"": ""38CFE38E-0D90-4C9F-A4E5-845089CB2BB0"",
    ""FirstName"": ""Tom"",
    ""LastName"": ""Tester"",
    ""Tags"": ""[c# .net maui] [c# wpf] [c# winforms]"",
    ""PrimaryKey"": ""38CFE38E-0D90-4C9F-A4E5-845089CB2BB0"",
    ""QueryTerm"": ""tom~tester"",
    ""FilterTerm"": ""tom~tester"",
    ""TagMatchTerm"": ""[c# .net maui] [c# wpf] [c# winforms]"",
    ""Properties"": ""{\r\n  \""FirstName\"": \""Tom\"",\r\n  \""LastName\"": \""Tester\"",\r\n  \""Tags\"": \""[c# .net maui] [c# wpf] [c# winforms]\""\r\n}""
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

        [TestMethod]
        public void Test_NormalizeTags_False()
        {
            bool revert = Extensions.InsertSpaceBetweenTags;
            using var local = this.WithOnDispose(
                onInit: (sender, e) =>
                {
                    Extensions.InsertSpaceBetweenTags = false;
                },
                onDispose: (sender, e) =>
                {
                    Extensions.InsertSpaceBetweenTags = revert;
                });
            string actual, expected;
            SelectableQFModel model;

            model = new SelectableQFModel 
            { 
                Id = "1",
                Description = "Purple Animal", 
                Tags = "animal color" 
            };

            actual = JsonConvert.SerializeObject(model, Newtonsoft.Json.Formatting.Indented);
            actual.ToClipboardExpected();
            { }
            expected = @" 
{
  ""Id"": ""1"",
  ""Description"": ""Purple Animal"",
  ""Keywords"": ""[]"",
  ""KeywordsDisplay"": """",
  ""Tags"": ""[animal][color]"",
  ""IsChecked"": false,
  ""Selection"": 0,
  ""IsEditing"": false,
  ""PrimaryKey"": ""1"",
  ""QueryTerm"": ""purple~animal~[animal][color]"",
  ""FilterTerm"": ""purple~animal~[animal][color]"",
  ""TagMatchTerm"": ""[animal][color]"",
  ""Properties"": ""{\r\n  \""Description\"": \""Purple Animal\"",\r\n  \""Tags\"": \""[animal][color]\""\r\n}""
}"
            ;

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting correct tag representation in Query and Filter exprs."
            );

            model = new SelectableQFModel
            {
                Id = "1",
                Description = "Purple Animal",
                Tags = "animal,color"
            };

            actual = JsonConvert.SerializeObject(model, Newtonsoft.Json.Formatting.Indented);
            actual.ToClipboardExpected(); // For viewing only

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting that expected DOES NOT CHANGE."
            );


            model = new SelectableQFModel
            {
                Id = "1",
                Description = "Purple Animal",
                Tags = "animal;color"
            };

            actual = JsonConvert.SerializeObject(model, Newtonsoft.Json.Formatting.Indented);
            actual.ToClipboardExpected(); // For viewing only

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting that expected DOES NOT CHANGE."
            );


            model = new SelectableQFModel
            {
                Id = "1",
                Description = "Purple Animal",
                Tags = "animal~color"
            };

            actual = JsonConvert.SerializeObject(model, Newtonsoft.Json.Formatting.Indented);
            actual.ToClipboardExpected(); // For viewing only

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting that expected DOES NOT CHANGE."
            );


            model = new SelectableQFModel
            {
                Id = "1",
                Description = "Purple Animal",
                Tags = "[animal][color]"
            };

            actual = JsonConvert.SerializeObject(model, Newtonsoft.Json.Formatting.Indented);
            actual.ToClipboardExpected(); // For viewing only

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting that expected DOES NOT CHANGE."
            );


            model = new SelectableQFModel
            {
                Id = "1",
                Description = "Purple Animal",
                Tags = "[animal][color]"
            };

            actual = JsonConvert.SerializeObject(model, Newtonsoft.Json.Formatting.Indented);
            actual.ToClipboardExpected(); // For viewing only

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting that expected DOES NOT CHANGE."
            );


            model = new SelectableQFModel
            {
                Id = "1",
                Description = "Purple Animal",
                Tags = "[animal,big]    [color]"
            };

            actual = JsonConvert.SerializeObject(model, Newtonsoft.Json.Formatting.Indented);
            actual.ToClipboardExpected();
            { }
            expected = @" 
{
  ""Id"": ""1"",
  ""Description"": ""Purple Animal"",
  ""Keywords"": ""[]"",
  ""KeywordsDisplay"": """",
  ""Tags"": ""[animal,big][color]"",
  ""IsChecked"": false,
  ""Selection"": 0,
  ""IsEditing"": false,
  ""PrimaryKey"": ""1"",
  ""QueryTerm"": ""purple~animal~[animal,big][color]"",
  ""FilterTerm"": ""purple~animal~[animal,big][color]"",
  ""TagMatchTerm"": ""[animal,big][color]"",
  ""Properties"": ""{\r\n  \""Description\"": \""Purple Animal\"",\r\n  \""Tags\"": \""[animal,big][color]\""\r\n}""
}"
            ;
            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting brackets beat commas."
            );
        }

        [TestMethod]
        public void Test_NormalizeTags_True()
        {
            bool revert = Extensions.InsertSpaceBetweenTags;
            using var local = this.WithOnDispose(
                onInit: (sender, e) =>
                {
                    Extensions.InsertSpaceBetweenTags = true;
                },
                onDispose: (sender, e) =>
                {
                    Extensions.InsertSpaceBetweenTags = revert;
                });
            string actual, expected;
            SelectableQFModel model;

            model = new SelectableQFModel 
            { 
                Id = "1",
                Description = "Purple Animal", 
                Tags = "animal color" 
            };

            actual = JsonConvert.SerializeObject(model, Newtonsoft.Json.Formatting.Indented);
            actual.ToClipboardExpected();
            { }
            expected = @" 
{
  ""Id"": ""1"",
  ""Description"": ""Purple Animal"",
  ""Keywords"": ""[]"",
  ""KeywordsDisplay"": """",
  ""Tags"": ""[animal] [color]"",
  ""IsChecked"": false,
  ""Selection"": 0,
  ""IsEditing"": false,
  ""PrimaryKey"": ""1"",
  ""QueryTerm"": ""purple~animal~[animal]~[color]"",
  ""FilterTerm"": ""purple~animal~[animal]~[color]"",
  ""TagMatchTerm"": ""[animal] [color]"",
  ""Properties"": ""{\r\n  \""Description\"": \""Purple Animal\"",\r\n  \""Tags\"": \""[animal] [color]\""\r\n}""
}"
            ;

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting correct tag representation in Query and Filter exprs."
            );

            model = new SelectableQFModel
            {
                Id = "1",
                Description = "Purple Animal",
                Tags = "animal,color"
            };

            actual = JsonConvert.SerializeObject(model, Newtonsoft.Json.Formatting.Indented);
            actual.ToClipboardExpected(); // For viewing only

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting that expected DOES NOT CHANGE."
            );


            model = new SelectableQFModel
            {
                Id = "1",
                Description = "Purple Animal",
                Tags = "animal;color"
            };

            actual = JsonConvert.SerializeObject(model, Newtonsoft.Json.Formatting.Indented);
            actual.ToClipboardExpected(); // For viewing only

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting that expected DOES NOT CHANGE."
            );


            model = new SelectableQFModel
            {
                Id = "1",
                Description = "Purple Animal",
                Tags = "animal~color"
            };

            actual = JsonConvert.SerializeObject(model, Newtonsoft.Json.Formatting.Indented);
            actual.ToClipboardExpected(); // For viewing only

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting that expected DOES NOT CHANGE."
            );


            model = new SelectableQFModel
            {
                Id = "1",
                Description = "Purple Animal",
                Tags = "[animal][color]"
            };

            actual = JsonConvert.SerializeObject(model, Newtonsoft.Json.Formatting.Indented);
            actual.ToClipboardExpected(); // For viewing only

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting that expected DOES NOT CHANGE."
            );


            model = new SelectableQFModel
            {
                Id = "1",
                Description = "Purple Animal",
                Tags = "[animal,big]    [color]"
            };

            actual = JsonConvert.SerializeObject(model, Newtonsoft.Json.Formatting.Indented);
            actual.ToClipboardExpected();
            { }
            expected = @" 
{
  ""Id"": ""1"",
  ""Description"": ""Purple Animal"",
  ""Keywords"": ""[]"",
  ""KeywordsDisplay"": """",
  ""Tags"": ""[animal,big] [color]"",
  ""IsChecked"": false,
  ""Selection"": 0,
  ""IsEditing"": false,
  ""PrimaryKey"": ""1"",
  ""QueryTerm"": ""purple~animal~[animal,big]~[color]"",
  ""FilterTerm"": ""purple~animal~[animal,big]~[color]"",
  ""TagMatchTerm"": ""[animal,big] [color]"",
  ""Properties"": ""{\r\n  \""Description\"": \""Purple Animal\"",\r\n  \""Tags\"": \""[animal,big] [color]\""\r\n}""
}"
            ;
            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting brackets beat commas."
            );
        }

        /// <summary>
        /// By design, the ParseSQLiteMarkdown method employs inheritance to determine table identity for parsing.
        /// </summary>
        /// <remarks>
        /// SQLite itself, when invoking CreateTable on subclass T, does *not* drill down for table inheritance
        /// This is the original test. There is also a newer streamlined version.
        /// #{B593ED5F-684A-4EF1-AA45-66E3766C7277}
        /// </remarks>
        [TestMethod]
        public void Test_TableAttributeInheritance()
        {
            #region L o c a l F x
            var builderThrow = new List<string>();
            void localOnBeginThrowOrAdvise(object? sender, Throw e)
            {
                builderThrow.Add(e.Message);
                e.Handled = true;
            }
            #endregion L o c a l F x
            using var local = this.WithOnDispose(
                onInit: (sender, e) =>
                {
                    Throw.BeginThrowOrAdvise += localOnBeginThrowOrAdvise;
                },
                onDispose: (sender, e) =>
                {
                    Throw.BeginThrowOrAdvise -= localOnBeginThrowOrAdvise;
                });

            string actual, expected;
            string[] tableNames;


            // TERMINOLOGY: Sources of "controversy".
            // 1. Conflicting [Table] attributes in the inheritance chain.
            // 2. ProxyType whose SQLite table mapping differs from ContractType.
            //
            // NOTWITHSTANDING:
            // Case 1 is not controversial when ContractType and ProxyType are the same type.
            // There is no competing mapping to resolve.
            //
            // Case 2 is treated as non-controversial when ProxyType inherits a mapping
            // that resolves to the same table name as ContractType.

            subtest_TableNameDefaultsToExplicitBC();
            subtest_ProxySameAsContract();
            subtest_UncontroversialExplicitTableAttribute();
            subtest_ParseInputTextInQueryMode();

            #region v2.0+
            subtest_ProxyCoherence1();
            subtest_ProxyCoherence2();
            #endregion v2.0+

            #region S U B T E S T S
            void subtest_TableNameDefaultsToExplicitBC()
            {
                // This is the "main gotcha" we're concerned about:

                // POLICY: Base-class table fallback.
                //
                // When the concrete type does not declare its own [Table] attribute,
                // but one or more base classes do, resolve table identity using the
                // closest ancestor that explicitly declares [Table].
                using (var cnx = new SQLiteConnection(":memory:"))
                {
                    // Subclass "G" means "Gotcha!"
                    cnx.CreateTable<SelectableQFModelSubclassG>();
                    tableNames = cnx.GetTableNames();

                    actual = JsonConvert.SerializeObject(tableNames, Newtonsoft.Json.Formatting.Indented);
                    expected = @" 
[
  ""SelectableQFModelSubclassG""
]";

                    Assert.AreEqual(
                        expected.NormalizeResult(),
                        actual.NormalizeResult(),
                        "Expecting that SQLite will create table named after the subclass when no [Table] is declared."
                    );

                    // APPLY POLICY: Pick up the `[Table("items"]` from the BC.
                    builderThrow.Clear();
                    actual = "animal".ParseSqlMarkdown<SelectableQFModelSubclassG>();
                    expected = @" 
SELECT * FROM items WHERE 
(QueryTerm LIKE '%animal%')";

                    Assert.AreEqual(
                        expected.NormalizeResult(),
                        actual.NormalizeResult(),
                        "Expecting 'items' table identity."
                    );
                }
            }

            // Different classes, but the explicit [Table] attributes all agree.
            void subtest_UncontroversialExplicitTableAttribute()
            {
                var mdc = new ModeledMarkdownContext<SelectableQFModel>();
                mdc.ParseSqlMarkdown<SelectableQFModelSubclass>("hello");
                tableNames = mdc.GetTableNames();
                "hello".ParseSqlMarkdown<SelectableQFModel>();

                actual = JsonConvert.SerializeObject(tableNames, Newtonsoft.Json.Formatting.Indented);
                actual.ToClipboardExpected();
                { }
                expected = @" 
    [
      ""items""
    ]";

                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting 'items' table"
                );
            }

            // When ContractType == ProxyType:
            // - Any explicit [Table] attributes in base classes are moot.
            void subtest_ProxySameAsContract()
            {
                var mdc = new ModeledMarkdownContext<SelectableQFModelSubclassA>();
                mdc.ParseSqlMarkdown<SelectableQFModelSubclassA>("hello");
                tableNames = mdc.GetTableNames();
                "hello".ParseSqlMarkdown<SelectableQFModel>();

                actual = JsonConvert.SerializeObject(tableNames, Newtonsoft.Json.Formatting.Indented);
                actual.ToClipboardExpected();
                { }
                // Even though [Table("items"]) exists in BC.
                expected = @" 
    [
      ""itemsA""
    ]";

                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting 'items' table"
                );
            }

            void subtest_ProxyCoherence1()
            {
                var mdc = new ModeledMarkdownContext<SelectableQFModel>();
                mdc.ParseSqlMarkdown<SelectableQFModelSubclassA>("hello");
                tableNames = mdc.GetTableNames();
                "hello".ParseSqlMarkdown<SelectableQFModel>();

                actual = JsonConvert.SerializeObject(tableNames, Newtonsoft.Json.Formatting.Indented);
                actual.ToClipboardExpected();
                { }
                expected = @" 
    [
      ""items""
    ]";

                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting 'items' table"
                );

            }

            void subtest_ProxyCoherence2()
            {
                var mdc = new ModeledMarkdownContext<SelectableQFModelSubclassA>();
                mdc.ParseSqlMarkdown<SelectableQFModelSubclassA>("hello");

                tableNames = mdc.GetTableNames();
                "hello".ParseSqlMarkdown<SelectableQFModel>();

                actual = JsonConvert.SerializeObject(tableNames, Newtonsoft.Json.Formatting.Indented);
                actual.ToClipboardExpected();
                { }
                expected = @" 
    [
      ""itemsA""
    ]";

                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting 'itemsA' table because proxy and contract are the same."
                );
            }

            void subtest_ParseInputTextInQueryMode()
            {
                // Check parser where declared table identities resolve as same
                var mdc = new ModeledMarkdownContext<SelectableQFModelSubclass>();
                mdc.InputText = "animal";

                actual = mdc.ParseSqlMarkdown();
                actual.ToClipboardExpected();
                { }
                expected = @" 
SELECT * FROM items WHERE 
(QueryTerm LIKE '%animal%')";

                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting table to match."
                );
            }
            #endregion S U B T E S T S
        }

        /// <summary>
        /// Ensure that IME input during IsFiltering epoch does not enter QueryEN or QueryEN
        /// </summary>
        [TestMethod]
        public async Task Test_Detect_QueryENB_or_QueryEN_when_IsFiltering()
        {
            string actual, expected;
            var mdc = new ModeledMarkdownContext<SelectableQFModel> { QueryFilterConfig = QueryFilterConfig.Filter };

            actual = mdc.StateReport();
            expected = @" 
[IME Len: 0, IsFiltering: True], [Net: null, CC: 0, PMC: 0], [Filter: SearchEntryState.QueryCompleteNoResults, FilteringState.Armed]"
            ;
            Assert.AreEqual(expected.NormalizeResult(), actual.NormalizeResult(), "Expecting StateReport to match.");

            mdc.InputText = "a";
            await mdc; // YBYA you need this in filter mode.

            actual = mdc.StateReport();
            expected = @" 
[IME Len: 1, IsFiltering: True], [Net: null, CC: 0, PMC: 0], [Filter: SearchEntryState.QueryCompleteNoResults, FilteringState.Armed]"
            ;
            Assert.AreEqual(expected.NormalizeResult(), actual.NormalizeResult(), "STILL ARMED due to NO ITEMS IN PROJECTION");

            // Reset to empty IME. Do not violate minimum SES.
            mdc.Clear();

            actual = mdc.StateReport();
            actual.ToClipboardExpected();
            { }
            expected = @" 
[IME Len: 0, IsFiltering: True], [Net: null, CC: 0, PMC: 0], [Filter: SearchEntryState.QueryCompleteNoResults, FilteringState.Armed]"
            ;
            Assert.AreEqual(expected.NormalizeResult(), actual.NormalizeResult(), "Expecting StateReport to match.");
        }
    }
}
