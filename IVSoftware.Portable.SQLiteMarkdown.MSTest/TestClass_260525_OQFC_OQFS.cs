using IVSoftware.Portable.Collections;
using IVSoftware.Portable.Collections.Events;
using IVSoftware.Portable.Disposable;
using IVSoftware.Portable.SQLiteMarkdown.Collections;
using IVSoftware.Portable.SQLiteMarkdown.Common;
using IVSoftware.WinOS.MSTest.Extensions;
using System.Collections;
using System.Collections.Specialized;

namespace IVSoftware.Portable.SQLiteMarkdown.MSTest;

[TestClass]
public class TestClass_260525_OQFC_OQFS
{
    [TestMethod, DoNotParallelize]
    public async Task Test_Rollback()
    {
        IList<SelectableQFModel>[] collections =
        [
            new ObservableQueryFilterCollection<SelectableQFModel>(),
            // new ObservableQueryFilterSource<SelectableQFModel>(),
        ];

        foreach (var collection in collections)
        {
            await localTestRollback(collection);
        }

        async Task localTestRollback(IList<SelectableQFModel> ilist)
        {
            string actual, expected;
            using var te = this.TestableEpoch();
            List<string>
                builderPre = new(),
                builderPost = new();

            localGetInterfaces(
                ilist,
                out IModeledCollection<SelectableQFModel> omc,
                out IRoutedCollection<SelectableQFModel> irc,
                out INotifyCollectionChanging inccPre,
                out INotifyCollectionChanged inccPost);

            var mdeap =
                (ModelDataExchangeAuthorityProvider<SelectableQFModel>)
                omc
                .AsInterface<ITestableOMC>()!
                .ModelDataExchangeAuthorityProvider;

            #region L o c a l F x
            void localGetInterfaces(
                ICollection<SelectableQFModel> icollection,
                out IModeledCollection<SelectableQFModel> ilist,
                out IRoutedCollection<SelectableQFModel> irc,
                out INotifyCollectionChanging inccPre,
                out INotifyCollectionChanged inccPost)
            {
                ilist = (IModeledCollection<SelectableQFModel>)icollection;
                inccPre = ilist.AsInterface<INotifyCollectionChanging>()!;
                inccPost = (INotifyCollectionChanged)icollection;
                irc = (IRoutedCollection<SelectableQFModel>)icollection;
            }
            #endregion L o c a l F x

            subtest_CancelInINCC();
            subtest_CancelDigest();

            #region S U B T E S T S
            void subtest_CancelInINCC()
            {
                int? countINCC = null;
                #region L o c a l F x
                using var local = this.WithOnDispose(
                    onInit: (sender, e) =>
                    {
                        inccPre.CollectionChangingEventingPolicy = CollectionChangingEventingPolicy.Discrete;
                        inccPre.CollectionChanging += localOnCollectionChanging;
                        inccPost.CollectionChanged += localOnCollectionChanged;
                    },
                    onDispose: (sender, e) =>
                    {
                        inccPre.CollectionChanging -= localOnCollectionChanging;
                        inccPost.CollectionChanged -= localOnCollectionChanged;
                    });

                void localOnCollectionChanging(object? sender, NotifyCollectionChangingEventArgs e)
                {
                    if (countINCC is not null)
                    {
                        countINCC++;
                        if (countINCC == 4)
                        {
                            mdeap.CancelAuthorityEpoch();
                        }
                    }
                    builderPre.Add(e.ToStringEx());
                }

                void localOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
                {
                    builderPost.Add(e.ToStringEx());
                }
                #endregion L o c a l F x

                omc.PopulateForDemo(5, PopulateOptions.DetectIRangeable);

                actual = omc.ToString(FormattingOMC.ModelWithPreview);
                actual.ToClipboardExpected();
                { }
                expected = @" 
<model omc=""[OMC]"" mdc=""[MDC]"" histo=""[model:5 qmatch:0 pmatch:0 live:5]"">
  <item text=""312d1c21-0000-0000-0000-000000000000"" model=""[SelectableQFModel]"" preview=""Item01    "" live=""True"" index=""0"" />
  <item text=""312d1c21-0000-0000-0000-000000000001"" model=""[SelectableQFModel]"" preview=""Item02    "" live=""True"" index=""1"" />
  <item text=""312d1c21-0000-0000-0000-000000000002"" model=""[SelectableQFModel]"" preview=""Item03    "" live=""True"" index=""2"" />
  <item text=""312d1c21-0000-0000-0000-000000000003"" model=""[SelectableQFModel]"" preview=""Item04    "" live=""True"" index=""3"" />
  <item text=""312d1c21-0000-0000-0000-000000000004"" model=""[SelectableQFModel]"" preview=""Item05    "" live=""True"" index=""4"" />
</model>";

                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting initial population as revert baseline."
                );

                actual = string.Join(Environment.NewLine, builderPre); builderPre.Clear();
                actual.ToClipboardExpected();
                { }
                expected = @" 
Reset   NewItems=0  OldItems=0  NewStartingIndex=-1 OldStartingIndex=-1 NotifyCollectionChangingEventArgs
Add     NewItems=1  OldItems=0  NewStartingIndex=0  OldStartingIndex=-1 NotifyCollectionChangingEventArgs
Add     NewItems=1  OldItems=0  NewStartingIndex=1  OldStartingIndex=-1 NotifyCollectionChangingEventArgs
Add     NewItems=1  OldItems=0  NewStartingIndex=2  OldStartingIndex=-1 NotifyCollectionChangingEventArgs
Add     NewItems=1  OldItems=0  NewStartingIndex=3  OldStartingIndex=-1 NotifyCollectionChangingEventArgs
Add     NewItems=1  OldItems=0  NewStartingIndex=4  OldStartingIndex=-1 NotifyCollectionChangingEventArgs"
                ;

                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting discrete ADD per CollectionChangingEventingPolicy."
                );
                Assert.AreEqual(CollectionChangingEventingPolicy.Discrete, inccPre.CollectionChangingEventingPolicy);

                actual = string.Join(Environment.NewLine, builderPost); builderPost.Clear();
                actual.ToClipboardExpected();
                { }
                expected = @" 
Reset   NewItems=*  OldItems=*  NewStartingIndex=-1 OldStartingIndex=-1 NotifyCollectionChangedEventArgs 
Add     NewItems=5  OldItems=*  NewStartingIndex=0  OldStartingIndex=-1 NotifyCollectionChangedEventArgs "
                ;

                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting digest for ADD."
                );

                countINCC = 0;
                // Add range, but cancel part of the way through.
                omc
                    .AsInterface<IRangeable>()!
                    .AddRange(default(List<SelectableQFModel>).PopulateForDemo((10, ilist.Count)));


                Assert.IsTrue(mdeap.IsCancelled);

                actual = omc.ToString(FormattingOMC.ModelWithPreview);
                actual.ToClipboardExpected();
                { }
                expected = @" 
<model omc=""[OMC]"" mdc=""[MDC]"" histo=""[model:5 qmatch:0 pmatch:0 live:5]"">
  <item text=""312d1c21-0000-0000-0000-000000000000"" model=""[SelectableQFModel]"" preview=""Item01    "" live=""True"" index=""0"" />
  <item text=""312d1c21-0000-0000-0000-000000000001"" model=""[SelectableQFModel]"" preview=""Item02    "" live=""True"" index=""1"" />
  <item text=""312d1c21-0000-0000-0000-000000000002"" model=""[SelectableQFModel]"" preview=""Item03    "" live=""True"" index=""2"" />
  <item text=""312d1c21-0000-0000-0000-000000000003"" model=""[SelectableQFModel]"" preview=""Item04    "" live=""True"" index=""3"" />
  <item text=""312d1c21-0000-0000-0000-000000000004"" model=""[SelectableQFModel]"" preview=""Item05    "" live=""True"" index=""4"" />
</model>";

                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting initial population as revert baseline."
                );
                Assert.HasCount(5, ilist);
                Assert.HasCount(5, irc.Items);
                Assert.AreEqual(5, omc.Histo[StdModelAttribute.model]);
            }

            void subtest_CancelDigest()
            {
                #region L o c a l F x
                using var local = this.WithOnDispose(
                    onInit: (sender, e) =>
                    {
                        inccPre.CollectionChangingEventingPolicy = CollectionChangingEventingPolicy.Coalesce;
                        inccPre.CollectionChanging += localOnCollectionChanging;
                        inccPost.CollectionChanged += localOnCollectionChanged;
                    },
                    onDispose: (sender, e) =>
                    {
                        inccPre.CollectionChangingEventingPolicy = CollectionChangingEventingPolicy.Discrete;
                        inccPre.CollectionChanging -= localOnCollectionChanging;
                        inccPost.CollectionChanged -= localOnCollectionChanged;
                    });

                void localOnCollectionChanging(object? sender, NotifyCollectionChangingEventArgs e)
                {
                    builderPre.Add(e.ToStringEx());
                }

                void localOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
                {
                    builderPost.Add(e.ToStringEx());
                }
                #endregion L o c a l F x

                ilist.Clear();
                te.ResetEpoch();
                omc.PopulateForDemo(5, PopulateOptions.DetectIRangeable);

                actual = omc.ToString(FormattingOMC.ModelWithPreview);
                actual.ToClipboardExpected();
                { }

                expected = @" 
<model omc=""[OMC]"" mdc=""[MDC]"" histo=""[model:5 qmatch:0 pmatch:0 live:5]"">
  <item text=""312d1c21-0000-0000-0000-000000000000"" model=""[SelectableQFModel]"" preview=""Item01    "" live=""True"" index=""0"" />
  <item text=""312d1c21-0000-0000-0000-000000000001"" model=""[SelectableQFModel]"" preview=""Item02    "" live=""True"" index=""1"" />
  <item text=""312d1c21-0000-0000-0000-000000000002"" model=""[SelectableQFModel]"" preview=""Item03    "" live=""True"" index=""2"" />
  <item text=""312d1c21-0000-0000-0000-000000000003"" model=""[SelectableQFModel]"" preview=""Item04    "" live=""True"" index=""3"" />
  <item text=""312d1c21-0000-0000-0000-000000000004"" model=""[SelectableQFModel]"" preview=""Item05    "" live=""True"" index=""4"" />
</model>";

                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting initial population as revert baseline."
                );

                // Add range, but cancel on the final digest.
                builderPre.Clear();
                builderPost.Clear();
                omc
                    .AsInterface<IRangeable>()!
                    .AddRange(default(List<SelectableQFModel>).PopulateForDemo((5,ilist.Count)));

                actual = string.Join(Environment.NewLine, builderPre); builderPre.Clear();
                actual.ToClipboardExpected();
                { }
                expected = @" 
Add     NewItems=1  OldItems=0  NewStartingIndex=5  OldStartingIndex=-1 NotifyCollectionChangingEventArgs
Add     NewItems=1  OldItems=0  NewStartingIndex=6  OldStartingIndex=-1 NotifyCollectionChangingEventArgs
Add     NewItems=1  OldItems=0  NewStartingIndex=7  OldStartingIndex=-1 NotifyCollectionChangingEventArgs
Add     NewItems=1  OldItems=0  NewStartingIndex=8  OldStartingIndex=-1 NotifyCollectionChangingEventArgs
Add     NewItems=1  OldItems=0  NewStartingIndex=9  OldStartingIndex=-1 NotifyCollectionChangingEventArgs"
                ;

                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting discrete ADD per CollectionChangingEventingPolicy."
                );
                Assert.AreEqual(CollectionChangingEventingPolicy.Coalesce, inccPre.CollectionChangingEventingPolicy);

                actual = string.Join(Environment.NewLine, builderPost); builderPost.Clear();
                actual.ToClipboardExpected();
                { }
                expected = @" 
Add     NewItems=5  OldItems=*  NewStartingIndex=0  OldStartingIndex=-1 NotifyCollectionChangedEventArgs "
                ;

                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting digest for ADD."
                );

            }

            actual = omc.ToString(FormattingOMC.ModelWithPreview);
            actual.ToClipboardExpected();
            { }
            expected = @" 
<model omc=""[OMC]"" mdc=""[MDC]"" histo=""[model:5 qmatch:0 pmatch:0 live:5]"">
  <item text=""312d1c21-0000-0000-0000-000000000000"" model=""[SelectableQFModel]"" preview=""Item01    "" live=""True"" index=""0"" />
  <item text=""312d1c21-0000-0000-0000-000000000001"" model=""[SelectableQFModel]"" preview=""Item02    "" live=""True"" index=""1"" />
  <item text=""312d1c21-0000-0000-0000-000000000002"" model=""[SelectableQFModel]"" preview=""Item03    "" live=""True"" index=""2"" />
  <item text=""312d1c21-0000-0000-0000-000000000003"" model=""[SelectableQFModel]"" preview=""Item04    "" live=""True"" index=""3"" />
  <item text=""312d1c21-0000-0000-0000-000000000004"" model=""[SelectableQFModel]"" preview=""Item05    "" live=""True"" index=""4"" />
  <item text=""312d1c21-0000-0000-0000-000000000005"" model=""[SelectableQFModel]"" preview=""Item06    "" live=""True"" index=""5"" />
  <item text=""312d1c21-0000-0000-0000-000000000006"" model=""[SelectableQFModel]"" preview=""Item07    "" live=""True"" index=""6"" />
  <item text=""312d1c21-0000-0000-0000-000000000007"" model=""[SelectableQFModel]"" preview=""Item08    "" live=""True"" index=""7"" />
  <item text=""312d1c21-0000-0000-0000-000000000008"" model=""[SelectableQFModel]"" preview=""Item09    "" live=""True"" index=""8"" />
  <item text=""312d1c21-0000-0000-0000-000000000009"" model=""[SelectableQFModel]"" preview=""Item10    "" live=""True"" index=""9"" />
</model>"
            ;

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting TBD."
            );
            #endregion S U B T E S T S
        }
    }
}
