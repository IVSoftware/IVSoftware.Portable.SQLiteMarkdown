using IVSoftware.Portable.Disposable;
using IVSoftware.Portable.SQLiteMarkdown.Common;
using IVSoftware.Portable.StateMachine;
using IVSoftware.Portable.Xml.Linq.XBoundObject.Modeling;
using IVSoftware.WinOS.MSTest.Extensions;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace IVSoftware.Portable.SQLiteMarkdown.MSTest;

[TestClass]
[PublishedContract(type: typeof(AuthorityEpochProvider))]
public class TestClass_Authority
{
    List<string> builder = new();
    class CompositedObservableCollection<T> : ObservableCollection<T>
    {
        public IModeledMarkdownContext? MMDC
        {
            get => _mmdc;
            set
            {
                if (!Equals(_mmdc, value))
                {
                    if(_mmdc is not null)
                    {
                        _mmdc.ModelChanged -= localOnCollectionChangedForward;
                    }
                    _mmdc = value;
                    if (_mmdc is not null)
                    {
                        _mmdc.ModelChanged += localOnCollectionChangedForward;
                    }
                }

                void localOnCollectionChangedForward(object? sender, NotifyCollectionChangedEventArgs e)
                {
                    OnCollectionChanged(e);
                }
            }
        }

        IModeledMarkdownContext? _mmdc = default;

    }

    [TestMethod]
    public void Test_Reset()
    {
        string actual, expected;

        subtest_ExternalONP();

        #region S U B T E S T S
        void subtest_ExternalONP()
        {
            ObservableCollection<SelectableQFModel> onp = new();
            var mmdc = new ModeledMarkdownContext<SelectableQFModel>
            {
                ObservableNetProjection = onp,
            };
            mmdc.ModelChanged += (sender, e) =>
            {
                builder.Add(e.ToString(ReferenceEquals(sender, onp)));
            };
            builder.Clear();

            // Clear when empty.
            mmdc.Clear(all: true);

            actual = string.Join(Environment.NewLine, builder);
            actual.ToClipboardExpected();
            { } // <- FIRST TIME ONLY: Adjust the message.
            actual.ToClipboardAssert("Expecting builder content to match.");
            { }
            expected = @" 
Other.Reset   NotifyCollectionChangedEventArgs           ";

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting builder content to match."
            );
        }
        #endregion S U B T E S T S
    }

    [TestMethod]
    public void Test_Commit()
    {
        subtest_ExternalONP();

        #region S U B T E S T S
        void subtest_ExternalONP()
        {
            builder.Clear();
            ObservableCollection<SelectableQFModel> onp = new();
            onp.CollectionChanged += (sender, e) =>
            {
                builder.Add(e.ToString(ReferenceEquals(sender, onp)));
            };
            var mmdc = new ModeledMarkdownContext<SelectableQFModel>
            {
                ObservableNetProjection = onp,
            };
        }
        #endregion
    }

    [TestMethod]
    public void Test_Projection()
    {
        subtest_ExternalONP();

        #region S U B T E S T S
        void subtest_ExternalONP()
        {
            builder.Clear();
            ObservableCollection<SelectableQFModel> onp = new();
            onp.CollectionChanged += (sender, e) =>
            {
                builder.Add(e.ToString(ReferenceEquals(sender, onp)));
            };
            var mmdc = new ModeledMarkdownContext<SelectableQFModel>
            {
                ObservableNetProjection = onp,
            };
        }
        #endregion
    }

    [TestMethod]
    public void Test_Settle()
    {
        subtest_ExternalONP();

        #region S U B T E S T S
        void subtest_ExternalONP()
        {
            builder.Clear();
            ObservableCollection<SelectableQFModel> onp = new();
            onp.CollectionChanged += (sender, e) =>
            {
                builder.Add(e.ToString(ReferenceEquals(sender, onp)));
            };
            var mmdc = new ModeledMarkdownContext<SelectableQFModel>
            {
                ObservableNetProjection = onp,
            };
        }
        #endregion
    }

    [TestMethod]
    public void Test_Predicate()
    {
        subtest_ExternalONP();

        #region S U B T E S T S
        void subtest_ExternalONP()
        {
            builder.Clear();
            ObservableCollection<SelectableQFModel> onp = new();
            onp.CollectionChanged += (sender, e) =>
            {
                builder.Add(e.ToString(ReferenceEquals(sender, onp)));
            };
            var mmdc = new ModeledMarkdownContext<SelectableQFModel>
            {
                ObservableNetProjection = onp,
            };

            #region L o c a l F x				
            using var local = mmdc.WithOnDispose(
                onInit: (sender, e) =>
                {
                    mmdc.ModelChanged += localOnModelChanged;
                },
                onDispose: (sender, e) =>
                {
                    mmdc.ModelChanged -= localOnModelChanged;
                });
            void localOnModelChanged(object? sender, NotifyCollectionChangedEventArgs eUnk)
            {
                builder.Add(eUnk.ToString(ReferenceEquals(sender, onp)));
            }
            #endregion L o c a l F x

            mmdc.Clear();
        }
        #endregion
    }
}
