using IVSoftware.Portable.Common.Exceptions;
using IVSoftware.Portable.Disposable;
using IVSoftware.Portable.SQLiteMarkdown.Common;
using IVSoftware.Portable.SQLiteMarkdown.MSTest.Switcheroo;
using IVSoftware.Portable.SQLiteMarkdown.Util;
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


        [TestMethod, DoNotParallelize]
        public void TestMethod_RouteInheritance()
        {
            using var te = this.TestableEpoch();
            string actual, expected;
            int nResult;
            IList<SelectableQFModel> oc;

            var mdci = new ObservableNetProjectionInheritsMDC<SelectableQFModel>();

            subtest_DetectTopology();
            subtest_PopulateAndClearEpoch();
            subtest_FilterTracking();

            #region S U B T E S T S
            void subtest_DetectTopology()
            {
                Assert.AreEqual(
                    ProjectionTopology.Inheritance,
                    mdci.ProjectionTopology,
                    "Expecting INHERITANCE is detectable from the start.");
            }
            void subtest_PopulateAndClearEpoch()
            {
                oc = new ObservableCollection<SelectableQFModel>().PopulateForDemo(2);
                mdci.ObservableNetProjection = (INotifyCollectionChanged)oc;


                actual = JsonConvert.SerializeObject(oc, Formatting.Indented);
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

                actual = mdci.Model.ToString();
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
                Assert.IsTrue(mdci.HasCounts(canonical: 2, matches: 2, database: 2));

                nResult = mdci.FilterQueryDatabase.ExecuteScalar<int>("Select Count(*) FROM items");

                Assert.AreEqual(
                    mdci.CanonicalCount,
                    nResult,
                    "Expecting the database items track the model at all times.");

                #region C L E A R
                oc.Clear();
                Assert.AreEqual(0, mdci.CanonicalCount);
                Assert.AreEqual(0, mdci.PredicateMatchCount);
                Assert.IsFalse(mdci.Model.HasElements);
                nResult = mdci.FilterQueryDatabase.ExecuteScalar<int>("Select Count(*) FROM items");
                Assert.AreEqual(0, nResult);
                { }
                #endregion C L E A R
            }

            void subtest_FilterTracking()
            {
                mdci.QueryFilterConfig = QueryFilterConfig.Filter;
            }
            #endregion S U B T E S T S
        }


        [TestMethod, Ignore]
        public void Test_ResetAndCopy()
        {
            string actual, expected;
            List<string> builder = new();
            subtest_ResetAndCopy1();
            subtest_ResetAndCopy2();
            subtest_ResetAndCopy3();
            subtest_ResetAndCopy4();
            subtest_ResetAndCopy5();
            subtest_ResetAndCopy6();
            subtest_ResetAndCopy7();
            subtest_ResetAndCopy8();
            subtest_ResetAndCopy9();
            subtest_ResetAndCopy10();

            #region S U B T E S T S
            void subtest_ResetAndCopy1()
            {
            }
            void subtest_ResetAndCopy2()
            {
            }
            void subtest_ResetAndCopy3()
            {
            }
            void subtest_ResetAndCopy4()
            {
            }
            void subtest_ResetAndCopy5()
            {
            }
            void subtest_ResetAndCopy6()
            {
            }
            void subtest_ResetAndCopy7()
            {
            }
            void subtest_ResetAndCopy8()
            {
            }
            void subtest_ResetAndCopy9()
            {
            }
            void subtest_ResetAndCopy10()
            {
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
            : MarkdownContext<T>
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
                base.PropertyChanged += (sender, e) =>
                {
                    Debug.WriteLine($"260303 BC PropertyChange '{e.PropertyName}' is advisory only.");
                };
                _mdc.PropertyChanged += (sender, e) => OnPropertyChanged(e.PropertyName);
            }
        }

        /// <summary>
        /// Implementor of the IMarkdownContext interface.
        /// </summary>
        partial class ObservableNetProjectionWithComposition<T> : IMarkdownContext
            
        {
            private readonly MarkdownContext<T> _mdc = new MarkdownContext<T>();

            public XElement Model => ((IMarkdownContext)_mdc).Model;

            public uint DefaultLimit { get => ((IMarkdownContext)_mdc).DefaultLimit; set => ((IMarkdownContext)_mdc).DefaultLimit = value; }

            public bool IsFiltering => ((IMarkdownContext)_mdc).IsFiltering;

            public FilteringState FilteringState => ((IMarkdownContext)_mdc).FilteringState;

            public string InputText { get => ((IMarkdownContext)_mdc).InputText; set => ((IMarkdownContext)_mdc).InputText = value; }
            public QueryFilterConfig QueryFilterConfig { get => ((IMarkdownContext)_mdc).QueryFilterConfig; set => ((IMarkdownContext)_mdc).QueryFilterConfig = value; }

            public SearchEntryState SearchEntryState => ((IMarkdownContext)_mdc).SearchEntryState;

            public int CanonicalCount => ((IMarkdownContext)_mdc).CanonicalCount;

            public event EventHandler? InputTextSettled
            {
                add
                {
                    ((IMarkdownContext)_mdc).InputTextSettled += value;
                }

                remove
                {
                    ((IMarkdownContext)_mdc).InputTextSettled -= value;
                }
            }

            public IDisposable BeginAuthority(CollectionChangeAuthority authority)
            {
                return ((IMarkdownContext)_mdc).BeginAuthority(authority);
            }

            public FilteringState Clear(bool all)
            {
                return ((IMarkdownContext)_mdc).Clear(all);
            }

            public Task LoadCanonAsync(IEnumerable? recordset)
            {
                return ((IMarkdownContext)_mdc).LoadCanonAsync(recordset);
            }

            public string ParseSqlMarkdown()
            {
                return ((IMarkdownContext)_mdc).ParseSqlMarkdown();
            }

            public string ParseSqlMarkdown(string expr, Type proxyType, QueryFilterMode qfMode, out XElement xast)
            {
                return ((IMarkdownContext)_mdc).ParseSqlMarkdown(expr, proxyType, qfMode, out xast);
            }

            public string ParseSqlMarkdown<T1>()
            {
                return ((IMarkdownContext)_mdc).ParseSqlMarkdown<T1>();
            }

            public string ParseSqlMarkdown<T1>(string expr, QueryFilterMode qfMode = QueryFilterMode.Query)
            {
                return ((IMarkdownContext)_mdc).ParseSqlMarkdown<T1>(expr, qfMode);
            }

            public ProjectionTopology ProjectionTopology => ((IMarkdownContext)_mdc).ProjectionTopology;

            public INotifyCollectionChanged ObservableNetProjection 
            {
                get => ((IMarkdownContext)_mdc).ObservableNetProjection;
                set => ((IMarkdownContext)_mdc).ObservableNetProjection = value;
            }

            public int PredicateMatchCount => ((IMarkdownContext)_mdc).PredicateMatchCount;

            INotifyCollectionChanged? _observableNetProjection = default;

            protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

            public void LoadCanon(IEnumerable? recordset)
            {
                ((IMarkdownContext)_mdc).LoadCanon(recordset);
            }

            public bool HasCounts(int canonical, int matches, int? database = null)
            {
                return ((IMarkdownContext)_mdc).HasCounts(canonical, matches, database);
            }

            // We do not care about BC events.
            public new event PropertyChangedEventHandler? PropertyChanged;
        }
    }
}
