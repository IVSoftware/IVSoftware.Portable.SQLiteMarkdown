using IVSoftware.Portable.Common.Exceptions;
using IVSoftware.Portable.Disposable;
using IVSoftware.Portable.SQLiteMarkdown.Common;
using IVSoftware.Portable.SQLiteMarkdown.MSTest.Switcheroo;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace IVSoftware.Portable.SQLiteMarkdown.MSTest
{
    [TestClass]
    public class TestClass_Switcheroo
    {
        [TestMethod]
        public void Test_DetectProjectionMode()
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

            subtest_Compositor();

            #region S U B T E S T S
            void subtest_Compositor()
            {
                var mdcr = new MDCCompositor<SelectableQFModel>();

                Assert.AreEqual(
                    ProjectionMode.None,
                    mdcr.ProjectionMode,
                    "Expecting the compositor to wait for assignment of the ONP");

                var oc = new ObservableCollection<SelectableQFModel>();
                mdcr.ObservableNetProjection = oc;

                Assert.AreEqual(ProjectionMode.Composition, mdcr.ProjectionMode);
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
        class MDCCompositor<T> : MarkdownContext<T>
        {
        }

        class MDCInheritor<T> : ObservableCollection<T>, INotifyPropertyChanged
        {
            public MDCInheritor()
            {
                base.PropertyChanged += (sender, e) =>
                {
                    Debug.WriteLine($"260303 BC PropertyChange '{e.PropertyName}' is advisory only.");
                };
            }
            public INotifyCollectionChanged ObservableNetProjection
            {
                get => _observableNetProjection;
                set
                {
                    if (!Equals(_observableNetProjection, value))
                    {
                        _observableNetProjection = value;
                        OnPropertyChanged();
                    }
                }
            }
            INotifyCollectionChanged _observableNetProjection = default;

            protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

            // We do not care about BC events.
            public new event PropertyChangedEventHandler? PropertyChanged;

        }
    }
}
