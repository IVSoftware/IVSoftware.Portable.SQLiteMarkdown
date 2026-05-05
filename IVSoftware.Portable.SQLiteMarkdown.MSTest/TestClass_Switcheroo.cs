using IVSoftware.Portable.Common.Exceptions;
using IVSoftware.Portable.Disposable;
using IVSoftware.Portable.SQLiteMarkdown.Common;
using IVSoftware.Portable.SQLiteMarkdown.MSTest.Switcheroo;
using IVSoftware.Portable.Threading;
using IVSoftware.Portable.Collections;
using IVSoftware.Portable.Xml.Linq.XBoundObject;
using IVSoftware.WinOS.MSTest.Extensions;
using Newtonsoft.Json;
using SQLite;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using IVSoftware.Portable.SQLiteMarkdown.Obsolete;
using IgnoreAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.IgnoreAttribute;


namespace IVSoftware.Portable.SQLiteMarkdown.MSTest
{
    [TestClass]
    public class TestClass_Switcheroo
    {
        [TestMethod, Ignore]
        public void Test_DetectTopology()
        {
#if false
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

            subtest_Inheritor();
            subtest_Compositor();

            #region S U B T E S T S
            void subtest_Inheritor()
            {
                var mdci = new ObservableNetProjectionInheritsMDC<SelectableQFModel>();
            }

            void subtest_Compositor()
            {
                var onpc = new ObservableNetProjectionWithComposition<SelectableQFModel>();
                //Assert.AreEqual(
                //    ProjectionTopology.Composition,
                //    onpc.ProjectionTopology,
                //    "Expecting COMPOSITION as assigned in CTor.");

                var mmdc = onpc.Model.To < MarkdownContext<SelectableQFModel>>();

                mmdc.SetObservableNetProjection(null);
                //Assert.AreEqual(
                //    ProjectionTopology.Composition,
                //    onpc.ProjectionTopology,
                //    "Expecting NONE is the epistemic default.");

                var oc = new ObservableCollection<SelectableQFModel>();
                mmdc.SetObservableNetProjection(oc);

                //Assert.AreEqual(
                //    ProjectionTopology.Composition,
                //    onpc.ProjectionTopology,
                //    "Expecting promotion to COMPOSITION now that assignment has been made.");
            }
            #endregion S U B T E S T S
#endif
        }

        /// <summary>
        /// Verifies the ability of the MDC to self-identify its <see cref="ProjectionTopology"/>.
        /// </summary>
        /// <remarks>
        /// Mental Model: "Am I (the MDC) inherited by the projection class? Or does the projection class include me as a composed object?"
        ///
        /// The test instantiates a projection type that inherits <see cref="MarkdownContext"/>,
        /// allowing the MDC to infer its topology without configuration. The first assertion
        /// verifies that <see cref="ProjectionTopology.Inheritance"/> is detected immediately.
        ///
        /// An observable collection is then assigned and populated. The test confirms that the
        /// MDC routes structure through its canonical model and backing database by verifying:
        /// - the observable source contents,
        /// - the generated canonical XML model,
        /// - synchronized counts across canonical store, predicate matches, and database.
        ///
        /// Clearing the observable source confirms that routed structural changes propagate
        /// back through the canonical store and database.
        /// </remarks>
        [TestMethod, DoNotParallelize, Ignore]
        public void TestMethod_RouteInheritance()
        {
#if false
            using var te = this.TestableEpoch();
            string actual, expected;
            int nResult;

            #region L o c a l F x
            List<string>
                builder = new(),
                builderThrow = new();
            var localCanon = default(List<SelectableQFModel>).PopulateForDemo(2);
            void localOnBeginThrowOrAdvise(object? sender, Throw e)
            {
                var msg = $"{e.GetType().Name} {e.FormattedMessage}";
                builderThrow.Add(msg);
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

            var inherited = new ObservableNetProjectionInheritsMDC<SelectableQFModel>();

            subtest_CheckForExpectedAdvisory();
            subtest_PopulateAndClearEpoch();
            subtest_FilterTracking();

            #region S U B T E S T S
            void subtest_CheckForExpectedAdvisory()
            {
                actual = string.Join(Environment.NewLine, builderThrow);
                actual.ToClipboardExpected();
                { }
                expected = @" 
Advisory Clear | Clearing histogram while model-bound rebuilds (not clears) counts from current model.
Throw MarkdownContextPolicyViolation.ExplicitClearAdvisory | ExplicitClearAdvisory Policy advisory:
- Inherited MarkdownContext detected, but no parameterless Clear() was found.
- Clear(bool all = false) participates in the MDC filtering state machine and may not
  immediately empty the collection. 
- If your callers expect IList-style behavior, consider implementing Clear() => Clear(true)
  to provide a deterministic terminal clear. You may also expose Clear(bool all) without a 
  default parameter to make the stateful semantics explicit."
                ;

                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    $"Expecting {nameof(ObservableNetProjectionInheritsMDC<SelectableQFModel>)} advises on missing parameterless Clear()."
                );
            }

            void subtest_PopulateAndClearEpoch()
            {
                inherited.LoadCanon(localCanon);

                actual = JsonConvert.SerializeObject(inherited, Newtonsoft.Json.Formatting.Indented);

                actual.ToClipboardExpected();
                { }
                expected = @" 
[
  {
    ""Id"": ""312d1c21-0000-0000-0000-000000000000"",
    ""Description"": ""Item01"",
    ""Keywords"": ""[]"",
    ""KeywordsDisplay"": """",
    ""Tags"": """",
    ""IsChecked"": false,
    ""Selection"": 0,
    ""IsEditing"": false,
    ""PrimaryKey"": ""312d1c21-0000-0000-0000-000000000000"",
    ""QueryTerm"": ""item01"",
    ""FilterTerm"": ""item01"",
    ""TagMatchTerm"": """",
    ""Properties"": ""{\r\n  \""Description\"": \""Item01\""\r\n}""
  },
  {
    ""Id"": ""312d1c21-0000-0000-0000-000000000001"",
    ""Description"": ""Item02"",
    ""Keywords"": ""[]"",
    ""KeywordsDisplay"": """",
    ""Tags"": """",
    ""IsChecked"": false,
    ""Selection"": 0,
    ""IsEditing"": false,
    ""PrimaryKey"": ""312d1c21-0000-0000-0000-000000000001"",
    ""QueryTerm"": ""item02"",
    ""FilterTerm"": ""item02"",
    ""TagMatchTerm"": """",
    ""Properties"": ""{\r\n  \""Description\"": \""Item02\""\r\n}""
  }
]";

                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting TWO items on display."
                );

                actual = inherited.Model.ToString();
                actual.ToClipboardExpected();
                { }
                expected = @" 
<model mdc=""[MDC]"" histo=""[model:2 match:0 qmatch:0 pmatch:0 live:0]"" filters=""[No Active Filters]"">
  <item text=""312d1c21-0000-0000-0000-000000000000"" model=""[SelectableQFModel]"" index=""0"" />
  <item text=""312d1c21-0000-0000-0000-000000000001"" model=""[SelectableQFModel]"" index=""1"" />
</model>"
                ;

                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting updated model."
                );

                actual = inherited.StateReport();
                actual.ToClipboardExpected();
                { }
                expected = @" 
[IME Len: 0, IsFiltering: True], [Net: null, CC: 2, PMC: 2], [QueryAndFilter: SearchEntryState.QueryCompleteWithResults, FilteringState.Armed]"
                ;
                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting StateReport FSOL to match HasCounts."
                );

                nResult = inherited.FilterQueryDatabase.Table<SelectableQFModel>().Count();

                Assert.AreEqual(
                    inherited.CanonicalCount,
                    nResult,
                    "Expecting the database items track the model at all times.");

                #region C L E A R

                #region L o c a l F x 
                void localOnAwaited(object? sender, AwaitedEventArgs e)
                {
                    switch (e.Caller)
                    {
                        case nameof(inherited.Clear) when e.ContainsKey("all"):
                            builder.Add($"{sender} Clear(all={(e["all"])})");
                            break;
                    }
                }
                #endregion L o c a l F x

                using (this.WithOnDispose(
                    onInit: (sender, e) =>
                    {
                        builder.Clear();
                        Threading.Extensions.Awaited += localOnAwaited;
                    },
                    onDispose: (sender, e) =>
                    {
                        builder.Clear();
                        Threading.Extensions.Awaited -= localOnAwaited;
                    }))
                {
                    // Pathologically, this appears to be an IList "no surprises" clear but IS NOT.
                    // We're listening for Awaited event is raised in the BC clear (with an 'all" key).
                    // This is because we INTERNTIONALLY LEFT OUT a parameterless Clear() in the subclass.
                    inherited.Clear();

                    actual = string.Join(Environment.NewLine, builder);
                    actual.ToClipboardExpected();
                    { }
                    expected = @" 
MarkdownContext Clear(all=False)";

                    Assert.AreEqual(
                        expected.NormalizeResult(),
                        actual.NormalizeResult(),
                        "Expecting BC Clear raises Awaited event @ False."
                    );

                    Assert.AreNotEqual(
                        0,
                        inherited.CanonicalCount,
                        "Expecting 'surprise'! The unintended absence of effect.");

                    // Verify State
                    actual = inherited.StateReport();
                    actual.ToClipboardExpected();
                    { }
                    expected = @" 
[IME Len: 0, IsFiltering: False], [Net: null, CC: 2, PMC: 0], [QueryAndFilter: SearchEntryState.QueryEmpty, FilteringState.Ineligible]"
                    ;
                    Assert.AreEqual(expected.NormalizeResult(), actual.NormalizeResult(), "Expecting non-terminal clear.");

                    // Bunp state back up where it was.
                    inherited.LoadCanon(localCanon);

                    // Verify State
                    actual = inherited.StateReport();
                    actual.ToClipboardExpected();
                    { }
                    expected = @" 
[IME Len: 0, IsFiltering: True], [Net: null, CC: 2, PMC: 2], [QueryAndFilter: SearchEntryState.QueryCompleteWithResults, FilteringState.Armed]"
                    ;
                    Assert.AreEqual(expected.NormalizeResult(), actual.NormalizeResult(), "Expecting QUERY COMPLETE WITH RESULTS.");
                    { }

                    // Perform terminal clear.
                    builder.Clear();
                    inherited.Clear(true);

                    // Verify State
                    actual = inherited.StateReport();
                    actual.ToClipboardExpected();
                    { }
                    expected = @" 
[IME Len: 0, IsFiltering: False], [Net: null, CC: 0, PMC: 0], [QueryAndFilter: SearchEntryState.Cleared, FilteringState.Ineligible]"
                    ;


                    actual = string.Join(Environment.NewLine, builder);
                    actual.ToClipboardExpected();
                    { }
                    expected = @" 
MarkdownContext Clear(all=True)";

                    Assert.AreEqual(
                        expected.NormalizeResult(),
                        actual.NormalizeResult(),
                        "Expecting BC Clear raises Awaited event."
                    );
                }

                Assert.AreEqual(0, inherited.CanonicalCount);
                Assert.AreEqual(0, inherited.PredicateMatchCount);
                Assert.IsFalse(inherited.Model.HasElements);
                Assert.AreEqual(0, inherited.FilterQueryDatabase.ExecuteScalar<int>("Select Count(*) FROM items"));
                #endregion C L E A R
            }

            void subtest_FilterTracking()
            {
                inherited.QueryFilterConfig = QueryFilterConfig.Filter;
            }
            #endregion S U B T E S T S
#endif
        }

        [TestMethod, Ignore]
        public void Test_ResetAndCopy()
        {
#if false
            string actual, expected;
            List<string> builder = new();
            ObservableNetProjectionWithComposition<SelectableQFModel> onp;

            subtest_DetectTopology();

            #region S U B T E S T S
            void subtest_DetectTopology()
            {
                onp = new ObservableNetProjectionWithComposition<SelectableQFModel>();
                //Assert.AreEqual(
                //    ProjectionTopology.Composition,
                //    onp.ProjectionTopology,
                //    "Expecting ABSENCE OF INHERITANCE is detectable from the start as 'COMPOSITION'.");
                var mdcc = onp.Model.To<MarkdownContext<SelectableQFModel>>();
                mdcc.SetObservableNetProjection(null);
                //Assert.AreEqual(
                //    ProjectionTopology.Composition,
                //    onp.ProjectionTopology,
                //    "Expecting NONE.");
            }
            #endregion S U B T E S T S
#endif
        }
    }

    namespace Switcheroo
    {
        /// <summary>
        /// Uses routing for the net projection.
        /// </summary>
        class ObservableNetProjectionInheritsMDC<T>
            : MarkdownContext<T>
            where T : new()
        {
            public XElement Model { get; set; } = StdModelElement.model.MakeXElement();

            public void LoadCanon(IList<SelectableQFModel> localCanon)
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Extension and general housekeeping.
        /// </summary>
        partial class ObservableNetProjectionWithComposition<T> : ObservableModeledCollection<T>
        {
        }

        partial class ObservableNetProjectionWithComposition<T> where T : new()
        {
        }
    }
}