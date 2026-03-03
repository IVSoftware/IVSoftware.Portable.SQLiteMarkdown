using IVSoftware.Portable.Common.Exceptions;
using IVSoftware.Portable.Disposable;
using IVSoftware.Portable.SQLiteMarkdown.Common;
using IVSoftware.Portable.SQLiteMarkdown.MSTest.Switcheroo;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Xml.Linq;

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
                var mdcc = new ObservableNetProjectionInheritsMDC<SelectableQFModel>();

                Assert.AreEqual(
                    ProjectionTopology.Inheritance,
                    mdcc.ProjectionTopology,
                    "Expecting INHERITANCE is detectable from the start.");

            }

            void subtest_Compositor()
            {
                var mdci = new ObservableNetProjectionWithComposition<SelectableQFModel>();

                Assert.AreEqual(
                    ProjectionTopology.None,
                    mdci.ProjectionTopology,
                    "Expecting NONE is the epistemic default.");

                var oc = new ObservableCollection<SelectableQFModel>();
                mdci.ObservableNetProjection = oc;

                Assert.AreEqual(
                    ProjectionTopology.Composition,
                    mdci.ProjectionTopology,
                    "Expecting promotion to COMPOSITION now that assignment has been made.");
            }
            #endregion S U B T E S T S
        }

        [TestMethod, Ignore]
        public void Test_RouteEnumeratorAndReset()
        {
            string actual, expected;
            List<string> builder = new();
            subtest_RouteEnumeratorAndReset1();
            subtest_RouteEnumeratorAndReset2();
            subtest_RouteEnumeratorAndReset3();
            subtest_RouteEnumeratorAndReset4();
            subtest_RouteEnumeratorAndReset5();
            subtest_RouteEnumeratorAndReset6();
            subtest_RouteEnumeratorAndReset7();
            subtest_RouteEnumeratorAndReset8();
            subtest_RouteEnumeratorAndReset9();
            subtest_RouteEnumeratorAndReset10();

            #region S U B T E S T S
            void subtest_RouteEnumeratorAndReset1()
            {
            }
            void subtest_RouteEnumeratorAndReset2()
            {
            }
            void subtest_RouteEnumeratorAndReset3()
            {
            }
            void subtest_RouteEnumeratorAndReset4()
            {
            }
            void subtest_RouteEnumeratorAndReset5()
            {
            }
            void subtest_RouteEnumeratorAndReset6()
            {
            }
            void subtest_RouteEnumeratorAndReset7()
            {
            }
            void subtest_RouteEnumeratorAndReset8()
            {
            }
            void subtest_RouteEnumeratorAndReset9()
            {
            }
            void subtest_RouteEnumeratorAndReset10()
            {
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
        class ObservableNetProjectionInheritsMDC<T>
            : MarkdownContext<T>
            , INotifyCollectionChanged
        {
            public event NotifyCollectionChangedEventHandler? CollectionChanged;
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

            public int UnfilteredCount => ((IMarkdownContext)_mdc).UnfilteredCount;

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

            public IDisposable BeginAuthorityClaim()
            {
                return ((IMarkdownContext)_mdc).BeginAuthorityClaim();
            }

            public FilteringState Clear(bool all)
            {
                return ((IMarkdownContext)_mdc).Clear(all);
            }

            public Task LoadCanonAsync(IEnumerable recordset)
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

            INotifyCollectionChanged? _observableNetProjection = default;

            protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

            // We do not care about BC events.
            public new event PropertyChangedEventHandler? PropertyChanged;
        }
    }
}
