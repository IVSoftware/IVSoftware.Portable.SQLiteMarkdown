using IVSoftware.Portable.Disposable;
using IVSoftware.Portable.SQLiteMarkdown.Common;
using IVSoftware.Portable.SQLiteMarkdown.Util;
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
    class ComposedObservableCollection<T> : ObservableCollection<T>
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

    [TestMethod, DoNotParallelize]
    public void Test_Reset()
    {
        string actual, expected;
        using var te = this.TestableEpoch();

        ComposedObservableCollection<SelectableQFModel> onp = new();

        // [Careful]
        // - This isn't a subscription to MMDC at all.
        // - In particular, it doesn't make the ONP event show up first in builder.
        onp.CollectionChanged += (sender, e) =>
        {
            builder.Add(e.ToString(ReferenceEquals(sender, onp)));
        };
        var mmdc = new ModeledMarkdownContext<SelectableQFModel>
        {
            ObservableNetProjection = onp,
        };

        #region E V E N T S
        // This is the order that matters:
        // FIRST - subscribe the MMDC direct
        mmdc.ModelChanged += (sender, e) =>
        {
            builder.Add(e.ToString(ReferenceEquals(sender, onp)));
        };
        // SECOND - subscribe the MMDC in the Composed collection
        onp.MMDC = mmdc; // Cross-linked ONP will now forware the ModelChanged event.
        #endregion E V E N T S

        subtest_ResetWhenEmpty();
        subtest_ResetNonEmpty();

        #region S U B T E S T S
        void subtest_ResetWhenEmpty()
        {
            builder.Clear();

            // Clear, when already empty.
            mmdc.Clear(all: true);

            actual = string.Join(Environment.NewLine, builder);
            actual.ToClipboardExpected();
            expected = @"         
Other.Reset   NotifyCollectionChangedEventArgs           
NetProjection.Reset   NotifyCollectionChangedEventArgs   "
            ;

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting events received in order of subscription."
            );
        }

        void subtest_ResetNonEmpty()
        {
            onp.PopulateForDemo(1);
            Assert.AreEqual(1, mmdc.CanonicalCount);

            actual = onp.MMDC.Model.ToString();
            actual.ToClipboardExpected();
            { }
            expected = @" 
<model autocount=""1"" count=""1"" matches=""1"" mmdc=""[MMDC]"">
  <xitem text=""312d1c21-0000-0000-0000-000000000000"" model=""[SelectableQFModel]"" sort=""0"" />
</model>"
            ;
            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting that an arbitrary item is present in order to test the clear."
            );
            builder.Clear();
            // Clear, when non-empty.
            mmdc.Clear(all: true);

            actual = string.Join(Environment.NewLine, builder);
            actual.ToClipboardExpected();
            { }
            expected = @" 
Other.Reset   NotifyCollectionChangedEventArgs           
NetProjection.Reset   NotifyCollectionChangedEventArgs           "
            ;

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting events received in order of subscription."
            );
        }
        #endregion S U B T E S T S
    }

    [TestMethod, DoNotParallelize]
    public void Test_Commit()
    {
        string actual, expected;
        using var te = this.TestableEpoch();
        List<SelectableQFModel> recordset = new();

        ComposedObservableCollection<SelectableQFModel> onp = new();

        // [Careful]
        // - This isn't a subscription to MMDC at all.
        // - In particular, it doesn't make the ONP event show up first in builder.
        onp.CollectionChanged += (sender, e) =>
        {
            builder.Add(e.ToString(ReferenceEquals(sender, onp)));
        };
        var mmdc = new ModeledMarkdownContext<SelectableQFModel>
        {
            ObservableNetProjection = onp,
        };

        #region E V E N T S
        // This is the order that matters:
        // FIRST - subscribe the MMDC direct
        mmdc.ModelChanged += (sender, e) =>
        {
            builder.Add(e.ToString(ReferenceEquals(sender, onp)));
        };
        // SECOND - subscribe the MMDC in the Composed collection
        onp.MMDC = mmdc; // Cross-linked ONP will now forware the ModelChanged event.
        #endregion E V E N T S

        #region S U B T E S T S

        subtest_CanonizeOne();
        void subtest_CanonizeOne()
        {
            builder.Clear();
            mmdc.LoadCanon(recordset);
            Assert.AreEqual(0, builder.Count, "TEMPORARY LIMIT");

            actual = onp.MMDC.Model.ToString();
            actual.ToClipboardExpected();
            { }
            expected = @" 
<model autocount=""1"" count=""1"" matches=""1"" mmdc=""[MMDC]"">
  <xitem text=""312d1c21-0000-0000-0000-000000000000"" model=""[SelectableQFModel]"" sort=""0"" />
</model>"
            ;
            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting that an arbitrary item is present in order to test the clear."
            );
        }
        #endregion
    }

    [TestMethod, DoNotParallelize]
    public void Test_Projection()
    {
        string actual, expected;
        using var te = this.TestableEpoch();

        ComposedObservableCollection<SelectableQFModel> onp = new();

        // [Careful]
        // - This isn't a subscription to MMDC at all.
        // - In particular, it doesn't make the ONP event show up first in builder.
        onp.CollectionChanged += (sender, e) =>
        {
            builder.Add(e.ToString(ReferenceEquals(sender, onp)));
        };
        var mmdc = new ModeledMarkdownContext<SelectableQFModel>
        {
            ObservableNetProjection = onp,
        };

        #region E V E N T S
        // This is the order that matters:
        // FIRST - subscribe the MMDC direct
        mmdc.ModelChanged += (sender, e) =>
        {
            builder.Add(e.ToString(ReferenceEquals(sender, onp)));
        };
        // SECOND - subscribe the MMDC in the Composed collection
        onp.MMDC = mmdc; // Cross-linked ONP will now forware the ModelChanged event.
        #endregion E V E N T S

        #region S U B T E S T S
        #endregion
    }

    [TestMethod, DoNotParallelize]
    public void Test_Settle()
    {
        string actual, expected;
        using var te = this.TestableEpoch();

        ComposedObservableCollection<SelectableQFModel> onp = new();

        // [Careful]
        // - This isn't a subscription to MMDC at all.
        // - In particular, it doesn't make the ONP event show up first in builder.
        onp.CollectionChanged += (sender, e) =>
        {
            builder.Add(e.ToString(ReferenceEquals(sender, onp)));
        };
        var mmdc = new ModeledMarkdownContext<SelectableQFModel>
        {
            ObservableNetProjection = onp,
        };

        #region E V E N T S
        // This is the order that matters:
        // FIRST - subscribe the MMDC direct
        mmdc.ModelChanged += (sender, e) =>
        {
            builder.Add(e.ToString(ReferenceEquals(sender, onp)));
        };
        // SECOND - subscribe the MMDC in the Composed collection
        onp.MMDC = mmdc; // Cross-linked ONP will now forware the ModelChanged event.
        #endregion E V E N T S

        #region S U B T E S T S
        #endregion
    }

    [TestMethod, DoNotParallelize]
    public void Test_Predicate()
    {
        string actual, expected;
        using var te = this.TestableEpoch();

        ComposedObservableCollection<SelectableQFModel> onp = new();

        // [Careful]
        // - This isn't a subscription to MMDC at all.
        // - In particular, it doesn't make the ONP event show up first in builder.
        onp.CollectionChanged += (sender, e) =>
        {
            builder.Add(e.ToString(ReferenceEquals(sender, onp)));
        };
        var mmdc = new ModeledMarkdownContext<SelectableQFModel>
        {
            ObservableNetProjection = onp,
        };

        #region E V E N T S
        // This is the order that matters:
        // FIRST - subscribe the MMDC direct
        mmdc.ModelChanged += (sender, e) =>
        {
            builder.Add(e.ToString(ReferenceEquals(sender, onp)));
        };
        // SECOND - subscribe the MMDC in the Composed collection
        onp.MMDC = mmdc; // Cross-linked ONP will now forware the ModelChanged event.
        #endregion E V E N T S

        #region S U B T E S T S
        #endregion
    }
}
