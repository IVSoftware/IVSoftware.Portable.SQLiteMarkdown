// Handwritten verification view; source provenance is in the adjacent manifest.
using IVSoftware.Portable.Collections;
using IVSoftware.Portable.Common.Attributes;
using IVSoftware.Portable.Common.Exceptions;
using IVSoftware.Portable.Disposable;
using IVSoftware.Portable.SQLiteMarkdown.Collections;
using IVSoftware.Portable.SQLiteMarkdown.Common;
using IVSoftware.Portable.SQLiteMarkdown.Internal;
using IVSoftware.Portable.SQLiteMarkdown.MSTest.Models;
using IVSoftware.Portable.Xml.Linq.XBoundObject;
using IVSoftware.WinOS.MSTest.Extensions;
using Newtonsoft.Json;
using SQLite;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using IgnoreAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.IgnoreAttribute;

namespace IVSoftware.Portable.SQLiteMarkdown.MSTest
{

public partial class TestClass_TestClass_SQLiteMarkdown
{
[TestMethod]
        public async Task Test_QueryOnlyFSMs()
        {
            string actual, expected;
                
            const int COUNT = 2;
            var extQueryHandle = default(List<SelectableQFModel>);

            var oqfs = new ObservableQueryFilterSource<SelectableQFModel> 
            {
                QueryFilterConfig = QueryFilterConfig.Query, 
            };
            actual = oqfs.StateReport();
            actual.ToClipboardExpected();
            { }
            expected = @" 
[IME Len: 0, IsFiltering: False], [Net: 0, CC: 0, PMC: 0], [Query: SearchEntryState.Cleared, FilteringState.Ineligible]"
            ;
            Assert.AreEqual(expected.NormalizeResult(), actual.NormalizeResult(), "Expecting StateReport to match.");

            oqfs.InputText = "a";
            actual = oqfs.StateReport();
            actual.ToClipboardExpected();
            { }
            expected = @" 
[IME Len: 1, IsFiltering: False], [Net: 0, CC: 0, PMC: 0], [Query: SearchEntryState.QueryENB, FilteringState.Ineligible]"
            ;
            Assert.AreEqual(expected.NormalizeResult(), actual.NormalizeResult(), "Expecting StateReport to match.");

            // Backspace
            oqfs.InputText = string.Empty;
            actual = oqfs.StateReport();
            actual.ToClipboardExpected();
            { }
            expected = @" 
[IME Len: 0, IsFiltering: False], [Net: 0, CC: 0, PMC: 0], [Query: SearchEntryState.Cleared, FilteringState.Ineligible]"
            ;
            Assert.AreEqual(expected.NormalizeResult(), actual.NormalizeResult(), "Expecting StateReport to match.");
            Assert.AreEqual(SearchEntryState.Cleared, oqfs.SearchEntryState);

            oqfs.InputText = "a";
            actual = oqfs.StateReport();
            actual.ToClipboardExpected();
            { }
            expected = @" 
[IME Len: 1, IsFiltering: False], [Net: 0, CC: 0, PMC: 0], [Query: SearchEntryState.QueryENB, FilteringState.Ineligible]"
            ;
            Assert.AreEqual(expected.NormalizeResult(), actual.NormalizeResult(), "Expecting StateReport to match.");

            oqfs.InputText = "an";
            actual = oqfs.StateReport();
            actual.ToClipboardExpected();
            { }
            expected = @" 
[IME Len: 2, IsFiltering: False], [Net: 0, CC: 0, PMC: 0], [Query: SearchEntryState.QueryENB, FilteringState.Ineligible]"
            ;
            Assert.AreEqual(expected.NormalizeResult(), actual.NormalizeResult(), "Expecting StateReport to match.");

            oqfs.InputText = "ani";
            actual = oqfs.StateReport();
            actual.ToClipboardExpected();
            { }
            expected = @" 
[IME Len: 3, IsFiltering: False], [Net: 0, CC: 0, PMC: 0], [Query: SearchEntryState.QueryEN, FilteringState.Ineligible]"
            ;
            Assert.AreEqual(expected.NormalizeResult(), actual.NormalizeResult(), "Expecting StateReport to match.");

            // Commit and load the new recordset.
            // [Remember] Config is Query only; IsFilter is DISABLED.
            oqfs.ReplaceItems(extQueryHandle.PopulateForDemo(COUNT));
            actual = oqfs.StateReport();
            actual.ToClipboardExpected();
            { }
            expected = @" 
[IME Len: 3, IsFiltering: False], [Net: 2, CC: 2, PMC: 0], [Query: SearchEntryState.QueryCompleteWithResults, FilteringState.Ineligible]"
            ;
            Assert.AreEqual(expected.NormalizeResult(), actual.NormalizeResult(), "Expecting Filtering shows DISABLED.");

            // Clear the IME, *not* the recordset.
            // [Remember] Clear on MMDC resolves to Clear(bool).
            oqfs.Clear(all: false);
            actual = oqfs.StateReport();
            actual.ToClipboardExpected();
            { }
            expected = @" 
[IME Len: 0, IsFiltering: False], [Net: 2, CC: 2, PMC: 0], [Query: SearchEntryState.QueryEmpty, FilteringState.Ineligible]"
            ;
            Assert.AreEqual(expected.NormalizeResult(), actual.NormalizeResult(), "Expecting StateReport to match.");

            Assert.IsTrue(oqfs.RouteToFullRecordset, "ROUTE TO CANONICAL");

            // Empty IME + Regressive Clear = TerminalClear.
            // [Remember] Clear on MMDC resolves to Clear(bool).
            oqfs.Clear();
            actual = oqfs.StateReport();
            actual.ToClipboardExpected();
            { }
            expected = @" 
[IME Len: 0, IsFiltering: False], [Net: 0, CC: 0, PMC: 0], [Query: SearchEntryState.Cleared, FilteringState.Ineligible]"
            ;
            Assert.AreEqual(expected.NormalizeResult(), actual.NormalizeResult(), "Expecting StateReport to match.");
        }
[TestMethod]
        public async Task Test_FilterOnlyFSMs()
        {
            string actual, expected;

            var extQueryHandle = default(List<SelectableQFModel>).PopulateForDemo(2);

            ObservableQueryFilterSource<SelectableQFModel> oqfs;
            subtest_ConfigureThenLoad();

            subtest_LoadThenConfigure();

            #region S U B T E S T S
            void subtest_ConfigureThenLoad()
            {
                oqfs = new() { QueryFilterConfig = QueryFilterConfig.Filter };

                actual = oqfs.StateReport();
                actual.ToClipboardExpected();
                { }
                expected = @" 
[IME Len: 0, IsFiltering: True], [Net: 0, CC: 0, PMC: 0], [Filter: SearchEntryState.QueryCompleteNoResults, FilteringState.Armed]"
                ;

                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting FILTER ONLY."
                );
            }

            void subtest_LoadThenConfigure()
            {
                oqfs = new();
                actual = oqfs.StateReport();
                actual.ToClipboardExpected();
                { }
                expected = @" 
[IME Len: 0, IsFiltering: False], [Net: 0, CC: 0, PMC: 0], [QueryAndFilter: SearchEntryState.Cleared, FilteringState.Ineligible]"
                ;

                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting QUERY AND FILTER."
                );

                oqfs.QueryFilterConfig = QueryFilterConfig.Filter;
                actual = oqfs.StateReport();
                actual.ToClipboardExpected();
                { }
                expected = @" 
[IME Len: 0, IsFiltering: True], [Net: 0, CC: 0, PMC: 0], [Filter: SearchEntryState.QueryCompleteNoResults, FilteringState.Armed]"
                ;

                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting FILTER ONLY."
                );
            }
            #endregion S U B T E S T S
        }
[TestMethod]
        public void Test_Expression_Table()
        {
            string actual;
            foreach (var eut in ExpressionTable.Values)
            {
                var validationState = ValidationState.Empty;

                // Parse the expression and update the validation state
                var context = IVSoftware.Portable.SQLiteMarkdown.Extensions.ParseSqlMarkdown<SelfIndexedItem>(eut.Expr, ref validationState);
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
            MarkdownContext mdc;

            subtest_TableNameDefaultsToExplicitBC();
            subtest_ExtensionsResolveEffectiveTable();
            subtest_MemberFxResolveEffectiveTable();
            subtest_SubOnSuperTableConflict();
            subtest_SuperOnSubTableConflict();
            subtest_EnforceSingleIsFalse();
            subtest_EnforceSingleIsTrue();

            #region S U B T E S T S
            void subtest_TableNameDefaultsToExplicitBC()
            {
                // POLICY: Base-class table fallback.
                //
                // When the concrete type does not declare its own [Table] attribute,
                // but one or more base classes do, resolve table identity using the
                // closest ancestor that explicitly declares [Table].
                using (var cnx = new SQLiteConnection(":memory:"))
                {
                    // The current .NET 8 SQLite mapping agrees with the parser's inherited table policy.
                    cnx.CreateTable<SelectableQFModelSubclassG>();
                    tableNames = cnx.GetTableNames();

                    actual = JsonConvert.SerializeObject(tableNames, Newtonsoft.Json.Formatting.Indented);
                    expected = @" 
[
  ""items""
]";

                    Assert.AreEqual(
                        expected.NormalizeResult(),
                        actual.NormalizeResult(),
                        "Expecting SQLite to create the table named by the inherited [Table] attribute."
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
                    cnx.Insert(new SelectableQFModelSubclassG { Description = "Animal" });
                    var rows = cnx.Query<SelectableQFModelSubclassG>(actual);
                    Assert.AreEqual(1, rows.Count, "Expecting generated SQL to query the inherited table.");
                    Assert.AreEqual("Animal", rows[0].Description);
                }
            }

            /// <summary>
            /// Verifies that parse-only string extension calls resolve SQL against the
            /// effective table for the requested generic type.
            /// </summary>
            /// <remarks>
            /// This is not an instance-level ContractType/ProxyType controversy test.
            /// It only confirms that the extension surface honors the requested type's
            /// effective mapping in generated SQL.
            /// </remarks>
            void subtest_ExtensionsResolveEffectiveTable()
            {
                actual = "hello".ParseSqlMarkdown<SelectableQFModel>();
                actual.ToClipboardExpected();
                { }
                expected = @" 
SELECT * FROM items WHERE
(QueryTerm LIKE '%hello%')";

                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting 'items' table."
                );

                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting that SQLite will create table named after the subclass when no [Table] is declared."
                );

                actual = "hello".ParseSqlMarkdown<SelectableQFModelSubclassA>();
                actual.ToClipboardExpected();
                { }
                expected = @" 
SELECT * FROM itemsA WHERE
(QueryTerm LIKE '%hello%')";

                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting 'itemsA' table."
                );
            }

            /// <summary>
            /// Verifies that member-based parsing resolves SQL against the effective
            /// contract table when the context owns an injected filter database.
            /// </summary>
            /// <remarks>
            /// This covers the instance surface, where ContractType and ProxyType can
            /// be reasoned about explicitly. The test setup treats the contract table
            /// as the invariant and confirms that generated SQL remains aligned to it.
            /// </remarks>
            void subtest_MemberFxResolveEffectiveTable()
            {
                mdc = new TestableMarkdownContext<SelectableQFModel>
                {
                    FilterQueryDatabase = new (":memory:")
                };
                actual = mdc.ParseSqlMarkdown<SelectableQFModel>("hello");
                actual.ToClipboardExpected();
                { }
                expected = @" 
SELECT * FROM items WHERE
(QueryTerm LIKE '%hello%')";

                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting 'items' table."
                );

                actual = JsonConvert.SerializeObject(mdc.GetTableNames(), Formatting.Indented);
                actual.ToClipboardExpected();
                { }
                expected = @" 
[
  ""items""
]"
                ;

                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting 'items' table creation is owned by subclass."
                );


                mdc = new TestableMarkdownContext<SelectableQFModelSubclassA>
                {
                    FilterQueryDatabase = new (":memory:")
                };
                actual = mdc.ParseSqlMarkdown<SelectableQFModelSubclassA>("hello");
                actual.ToClipboardExpected();
                { }
                expected = @" 
SELECT * FROM itemsA WHERE
(QueryTerm LIKE '%hello%')";

                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting 'itemsA' table."
                );

                actual = JsonConvert.SerializeObject(mdc.GetTableNames(), Formatting.Indented);
                actual.ToClipboardExpected();
                { }
                expected = @" 
[
  ""itemsA""
]"
                ;
                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting 'itemsA' table creation is owned by subclass."
                );
            }

            void subtest_SubOnSuperTableConflict()
            {
                mdc = new TestableMarkdownContext<SelectableQFModel>
                {
                    FilterQueryDatabase = new(":memory:")
                };
                actual = mdc.ParseSqlMarkdown<SelectableQFModelSubclassA>("hello");
                actual.ToClipboardExpected();
                expected = @" 
SELECT * FROM items WHERE
(QueryTerm LIKE '%hello%')"
                ;

                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting 'items' table under a VERY EXPLICIT POLICY #{69046F5E-879C-46E1-8E35-D7EC3B52150B}."
                );

                Assert.HasCount(0, builderThrow, "Expecting this not exceptional.");

                actual = JsonConvert.SerializeObject(mdc.GetTableNames(), Formatting.Indented);
                actual.ToClipboardExpected();
                { }
                expected = @" 
[
  ""items""
]"
                ;

                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting 'items' table creation is owned by subclass."
                );
            }

            [Probationary("NEED REVIEW OF LIMITS FOR TABLES AND EXCEPTIONS")]
            void subtest_SuperOnSubTableConflict()
            {
                MarkdownContext mdc;

                mdc = new TestableMarkdownContext<SelectableQFModelSubclassA>
                {
                    FilterQueryDatabase = new(":memory:")
                };

                Assert.HasCount(0, builderThrow, "Expecting empty exception queue.");
                actual = mdc.ParseSqlMarkdown<SelectableQFModel>("hello");
                actual.ToClipboardExpected();
                { }
                expected = @" 
SELECT * FROM itemsA WHERE
(QueryTerm LIKE '%hello%')"
                ;

                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting two-way inheritance allows mapping to ContractType - even though it's a subclass of proxy."
                );

                Assert.HasCount(0, builderThrow, "Expecting this not exceptional.");

                actual = JsonConvert.SerializeObject(mdc.GetTableNames(), Formatting.Indented);
                actual.ToClipboardExpected();
                { }
                expected = @" 
[
  ""itemsA""
]"
                ;

                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting 'items' table creation is owned by subclass."
                );
            }

            void subtest_EnforceSingleIsFalse()
            {
                MarkdownContext mdc;

                mdc = new TestableMarkdownContext<SelectableQFModel>
                {
                    FilterQueryDatabase = new(":memory:")
                };

                Assert.HasCount(0, builderThrow, "Expecting empty exception queue.");
                actual = mdc.ParseSqlMarkdown<SxSProxy>("hello");
                actual.ToClipboardExpected();
                { }
                expected = @" 
SELECT * FROM sxs WHERE
(QueryTerm LIKE '%hello%')"
                ;

                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting a new dedicated table."
                );

                Assert.HasCount(0, builderThrow, "Expecting this not exceptional.");

                actual = JsonConvert.SerializeObject(mdc.GetTableNames(), Formatting.Indented);
                actual.ToClipboardExpected();
                { }
                expected = @" 
[
  ""items"",
  ""sxs""
]"
                ;

                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting 'items' table creation is owned by subclass."
                );
            }
            void subtest_EnforceSingleIsTrue()
            {
                MarkdownContext mdc;

                mdc = new TestableMarkdownContext<SelectableQFModel>
                {
                    FilterQueryDatabase = new(":memory:")
                };

                Assert.HasCount(0, builderThrow, "Expecting empty exception queue.");
                actual = mdc.ParseSqlMarkdown<EnforcedProxy>("hello");
                actual.ToClipboardExpected();
                { }
                expected = @" 
"
                ;

                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting proxy failure with empty return."
                );


                actual = string.Join(Environment.NewLine, builderThrow); builderThrow.Clear();
                actual.ToClipboardExpected();
                { }
                expected = @" 
Proxy type resolves to a different table 'sxs' and is not permitted to bypass the single-table contract for 'items'.";

                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting proxy error exception message."
                );

                actual = JsonConvert.SerializeObject(mdc.GetTableNames(), Formatting.Indented);
                actual.ToClipboardExpected();
                { }
                expected = @" 
[
  ""items""
]"
                ;

                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting new sxs table is NOT ALLOWED."
                );
            }
            #endregion S U B T E S T S

#if false && LEGACY_TESTING

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

            // Different classes, but the explicit [Table] attributes all agree.
            void subtest_UncontroversialExplicitTableAttribute()
            {
                var mdc = new MarkdownContext<SelectableQFModel>();
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
                var mdc = new MarkdownContext<SelectableQFModelSubclassA>();
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
                var mdc = new MarkdownContext<SelectableQFModel>();
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
                var mdc = new MarkdownContext<SelectableQFModelSubclassA>();
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
                var mdc = new MarkdownContext<SelectableQFModelSubclass>();
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
#endif
        }
}
}
