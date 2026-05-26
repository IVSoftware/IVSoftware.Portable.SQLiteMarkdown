using IVSoftware.Portable.Collections;
using IVSoftware.Portable.Collections.Events;
using IVSoftware.Portable.Disposable;
using IVSoftware.Portable.SQLiteMarkdown.Collections;
using IVSoftware.Portable.SQLiteMarkdown.Common;
using IVSoftware.WinOS.MSTest.Extensions;
using System.Collections.Specialized;

namespace IVSoftware.Portable.SQLiteMarkdown.MSTest;

[TestClass]
public class TestClass_260525_OQFC_OQFS
{
    [TestMethod, DoNotParallelize]
    public async Task Test_Revert()
    {
        IModeledCollection<SelectableQFModel>[] omcs =
        [
            new ObservableQueryFilterCollection<SelectableQFModel>(),
            // new ObservableQueryFilterSource<SelectableQFModel>(),
        ];
        foreach (var omc in omcs)
        {
            await Test_Revert(omc);
        }

        async Task Test_Revert(IModeledCollection<SelectableQFModel> omc)
        {
            string actual, expected;
            using var te = this.TestableEpoch();
            List<string>
                builderPre = new(),
                builderPost = new();
            INotifyCollectionChanging inccPre = omc.AsInterface<INotifyCollectionChanging>()!;
            INotifyCollectionChanged inccPost = (INotifyCollectionChanged) omc;
            var mdeap =
                (ModelDataExchangeAuthorityProvider<SelectableQFModel>)
                omc
                .AsInterface<ITestableOMC>()!
                .ModelDataExchangeAuthorityProvider;

            int? countINCC = null;

            #region L o c a l F x				
            using var local = this.WithOnDispose(
                onInit: (sender, e) =>
                {
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
            omc
                .AsInterface<IRangeable>()!
                .AddRange(default(List<SelectableQFModel>).PopulateForDemo(10));


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
        }
    }
}
