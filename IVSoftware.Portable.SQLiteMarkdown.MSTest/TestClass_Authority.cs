using IVSoftware.Portable.SQLiteMarkdown.Common;
using IVSoftware.Portable.StateMachine;
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
                        _mmdc.CanonicalCollectionChanged -= localOnCollectionChangedForward;
                    }
                    _mmdc = value;
                    if (_mmdc is not null)
                    {
                        _mmdc.CanonicalCollectionChanged += localOnCollectionChangedForward;
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
        subtest_ExternalONP();

        #region S U B T E S T S
        void subtest_ExternalONP()
        {
            builder.Clear();
            CompositedObservableCollection<SelectableQFModel> onp = new();
            onp.CollectionChanged += (sender, e) =>
            {
                builder.Add(e.ToString(ReferenceEquals(sender, onp)));
            };
            var mmdc = new ModeledMarkdownContext<SelectableQFModel>
            {
                ObservableNetProjection = onp,
            };
            onp.MMDC = mmdc;
            mmdc.Clear(all: true);
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
        }
        #endregion
    }
}
