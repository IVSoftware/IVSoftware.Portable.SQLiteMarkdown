using IVSoftware.Portable.Common.Exceptions;
using IVSoftware.Portable.Disposable;
using IVSoftware.Portable.SQLiteMarkdown;
using IVSoftware.Portable.SQLiteMarkdown.Common;
using IVSoftware.Portable.SQLiteMarkdown.MSTest.Switcheroo;
using IVSoftware.Portable.SQLiteMarkdown.Util;
using IVSoftware.Portable.Threading;
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
using Ignore = Microsoft.VisualStudio.TestTools.UnitTesting.IgnoreAttribute;


namespace IVSoftware.Portable.SQLiteMarkdown.MSTest
{
    [TestClass]
    public class TestClass_Switcheroo
    {
        [TestMethod]
        public void Test_DetectTopology()
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

            subtest_Inheritor();
            subtest_Compositor();

            #region S U B T E S T S
            void subtest_Inheritor()
            {
                var mdci = new ObservableNetProjectionInheritsMDC<SelectableQFModel>();

                Assert.AreEqual(
                    ProjectionTopology.Inheritance,
                    mdci.ProjectionTopology,
                    "Expecting INHERITANCE is detectable from the start.");
            }

            void subtest_Compositor()
            {
                var mdcc = new ObservableNetProjectionWithComposition<SelectableQFModel>();
                Assert.AreEqual(
                    ProjectionTopology.Composition,
                    mdcc.ProjectionTopology,
                    "Expecting COMPOSITION as assigned in CTor.");

                mdcc.ObservableNetProjection = null;
                Assert.AreEqual(
                    ProjectionTopology.None,
                    mdcc.ProjectionTopology,
                    "Expecting NONE is the epistemic default.");

                var oc = new ObservableCollection<SelectableQFModel>();
                mdcc.ObservableNetProjection = oc;

                Assert.AreEqual(
                    ProjectionTopology.Composition,
                    mdcc.ProjectionTopology,
                    "Expecting promotion to COMPOSITION now that assignment has been made.");
            }
            #endregion S U B T E S T S
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
        [TestMethod, DoNotParallelize]
        public void TestMethod_RouteInheritance()
        {
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
            subtest_DetectTopology();
            subtest_PopulateAndClearEpoch();
            subtest_FilterTracking();

            #region S U B T E S T S
            void subtest_CheckForExpectedAdvisory()
            {
                actual = string.Join(Environment.NewLine, builderThrow);
                actual.ToClipboardExpected();
                { }
                expected = @" 
Advisory .ctor | Inherited MarkdownContext detected, but no parameterless Clear() was found. Clear(bool all = false) participates in the MDC filtering state machine and may not immediately empty the collection. If your callers expect IList-style behavior, consider implementing Clear() => Clear(true) to provide a deterministic terminal clear. You may also expose Clear(bool all) without a default parameter to make the stateful semantics explicit.";

                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    $"Expecting {nameof(ObservableNetProjectionInheritsMDC<SelectableQFModel>)} advises on missing parameterless Clear()."
                );
            }
            void subtest_DetectTopology()
            {
                Assert.AreEqual(
                    ProjectionTopology.Inheritance,
                    inherited.ProjectionTopology,
                    "Expecting INHERITANCE is detectable from the start.");
            }
            void subtest_PopulateAndClearEpoch()
            {
                inherited.LoadCanon(localCanon);

                actual = JsonConvert.SerializeObject(inherited, Formatting.Indented);

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
<model autocount=""2"" count=""2"" matches=""2"">
  <xitem text=""312d1c21-0000-0000-0000-000000000000"" model=""[SelectableQFModel]"" sort=""0"" />
  <xitem text=""312d1c21-0000-0000-0000-000000000001"" model=""[SelectableQFModel]"" sort=""1"" />
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

                nResult = inherited.FilterQueryDatabase.ExecuteScalar<int>("Select Count(*) FROM items");

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
                    // This is supposed to be an IList "no surprises" clear.
                    // We're listening for Awaited event is raised in the BC clear (with an 'all" key).
                    // This is because we intentionally left out a parameterless Clear() in the subclass.
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
[IME Len: 0, IsFiltering: False], [Net: null, CC: 2, PMC: 2], [QueryAndFilter: SearchEntryState.QueryEmpty, FilteringState.Ineligible]"
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
        }

        [TestMethod]
        public void Test_ResetAndCopy()
        {
            string actual, expected;
            List<string> builder = new();
            ObservableNetProjectionWithComposition<SelectableQFModel> mdc;

            subtest_DetectTopology();

            #region S U B T E S T S
            void subtest_DetectTopology()
            {
                mdc = new ObservableNetProjectionWithComposition<SelectableQFModel>();
                Assert.AreEqual(
                    ProjectionTopology.Composition,
                    mdc.ProjectionTopology,
                    "Expecting ABSENCE OF INHERITANCE is detectable from the start as 'COMPOSITION'.");

                mdc.ObservableNetProjection = null;
                Assert.AreEqual(
                    ProjectionTopology.None,
                    mdc.ProjectionTopology,
                    "Expecting NONE.");
            }
            #endregion S U B T E S T S
        }
    }

    namespace Switcheroo
    {
        /// <summary>
        /// Uses routing for the net projection.
        /// </summary>
        class ObservableNetProjectionInheritsMDC<T>
            : ModeledMarkdownContext<T>
            , INotifyCollectionChanged
        {
            public event NotifyCollectionChangedEventHandler? CollectionChanged;

            // Expose for test.
            public new SQLiteConnection FilterQueryDatabase => base.FilterQueryDatabase;
        }

        /// <summary>
        /// Extension and general housekeeping.
        /// </summary>
        partial class ObservableNetProjectionWithComposition<T>
            : ObservableCollection<T>
        {
            public ObservableNetProjectionWithComposition()
            {
                Model.SetBoundAttributeValue(_mdc, name: nameof(StdMarkdownAttribute.mdc));
                _mdc.ObservableNetProjection = this;
                base.PropertyChanged += (sender, e) =>
                {
                    Debug.WriteLine($"260303 BC PropertyChange '{e.PropertyName}' is advisory only.");
                };
                _mdc.PropertyChanged += (sender, e) => OnPropertyChanged(e.PropertyName);
            }
        }

        partial class ObservableNetProjectionWithComposition<T> : IMarkdownContext
        {
            private readonly ModeledMarkdownContext<T> _mdc = new ModeledMarkdownContext<T>();

            public XElement Model => ((IMarkdownContext)_mdc).Model;

            public uint DefaultLimit
            {
                get => ((IMarkdownContext)_mdc).DefaultLimit;
                set => ((IMarkdownContext)_mdc).DefaultLimit = value;
            }

            public bool IsFiltering => ((IMarkdownContext)_mdc).IsFiltering;

            public FilteringState FilteringState => ((IMarkdownContext)_mdc).FilteringState;

            public string InputText
            {
                get => ((IMarkdownContext)_mdc).InputText;
                set => ((IMarkdownContext)_mdc).InputText = value;
            }

            public QueryFilterConfig QueryFilterConfig
            {
                get => ((IMarkdownContext)_mdc).QueryFilterConfig;
                set => ((IMarkdownContext)_mdc).QueryFilterConfig = value;
            }

            public SearchEntryState SearchEntryState => ((IMarkdownContext)_mdc).SearchEntryState;

            public int CanonicalCount => ((IMarkdownContext)_mdc).CanonicalCount;

            public event EventHandler? InputTextSettled
            {
                add => ((IMarkdownContext)_mdc).InputTextSettled += value;
                remove => ((IMarkdownContext)_mdc).InputTextSettled -= value;
            }

            public event NotifyCollectionChangedEventHandler ModelSettled
            {
                add => ((IModeledMarkdownContext)_mdc).ModelSettled += value;
                remove => ((IModeledMarkdownContext)_mdc).ModelSettled -= value;
            }

            public IDisposable BeginCollectionChangeAuthority(CollectionChangeAuthority authority)
                => ((IModeledMarkdownContext)_mdc).BeginCollectionChangeAuthority(authority);

            public FilteringState Clear(bool all)
                => ((IMarkdownContext)_mdc).Clear(all);

            public Task LoadCanonAsync(IEnumerable? recordset)
                => ((IModeledMarkdownContext)_mdc).LoadCanonAsync(recordset);

            public string ParseSqlMarkdown()
                => ((IMarkdownContext)_mdc).ParseSqlMarkdown();

            public string ParseSqlMarkdown(string expr, Type proxyType, QueryFilterMode qfMode, out XElement xast)
                => ((IMarkdownContext)_mdc).ParseSqlMarkdown(expr, proxyType, qfMode, out xast);

            public string ParseSqlMarkdown<T1>()
                => ((IMarkdownContext)_mdc).ParseSqlMarkdown<T1>();

            public string ParseSqlMarkdown<T1>(string expr, QueryFilterMode qfMode = QueryFilterMode.Query)
                => ((IMarkdownContext)_mdc).ParseSqlMarkdown<T1>(expr, qfMode);

            public ProjectionTopology ProjectionTopology => ((IModeledMarkdownContext)_mdc).ProjectionTopology;

            public INotifyCollectionChanged? ObservableNetProjection
            {
                get => ((IModeledMarkdownContext)_mdc).ObservableNetProjection;
                set => ((IModeledMarkdownContext)_mdc).ObservableNetProjection = value;
            }

            public int PredicateMatchCount => ((IMarkdownContext)_mdc).PredicateMatchCount;

            public NetProjectionOption ProjectionOption
            {
                get => ((IModeledMarkdownContext)_mdc).ProjectionOption;
                set => ((IModeledMarkdownContext)_mdc).ProjectionOption = value;
            }

            public ReplaceItemsEventingOption ReplaceItemsEventingOptions
            {
                get => ((IModeledMarkdownContext)_mdc).ReplaceItemsEventingOptions;
                set => ((IModeledMarkdownContext)_mdc).ReplaceItemsEventingOptions = value;
            }

            public CollectionChangeAuthority Authority => ((IModeledMarkdownContext)_mdc).Authority;

            public bool Busy => ((IMarkdownContext)_mdc).Busy;

            public TimeSpan InputTextSettlingTime
            {
                get => ((IMarkdownContext)_mdc).InputTextSettlingTime;
                set => ((IMarkdownContext)_mdc).InputTextSettlingTime = value;
            }

            INotifyCollectionChanged? _observableNetProjection = default;

            protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
                => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

            public void LoadCanon(IEnumerable? recordset)
                => ((IModeledMarkdownContext)_mdc).LoadCanon(recordset);

            public string[] GetTableNames()
                => ((IMarkdownContext)_mdc).GetTableNames();

            public IDisposable BeginBusy()
                => ((IMarkdownContext)_mdc).BeginBusy();

            public void Commit()
                => ((IMarkdownContext)_mdc).Commit();

            public new event PropertyChangedEventHandler? PropertyChanged;
        }
    }
}