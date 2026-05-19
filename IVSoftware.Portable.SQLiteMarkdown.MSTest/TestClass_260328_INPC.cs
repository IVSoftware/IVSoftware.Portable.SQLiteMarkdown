using IVSoftware.Portable.Collections.Preview;
using IVSoftware.Portable.Common.Attributes;
using IVSoftware.Portable.SQLiteMarkdown.Collections;
using IVSoftware.Portable.SQLiteMarkdown.Collections.Preview;
using IVSoftware.Portable.SQLiteMarkdown.Common;
using IVSoftware.Portable.SQLiteMarkdown.Internal;
using IVSoftware.Portable.Collections;
using IVSoftware.Portable.Collections.Events;
using IVSoftware.Portable.Xml.Linq.XBoundObject;
using IVSoftware.WinOS.MSTest.Extensions;
using Newtonsoft.Json;
using System.Collections;
using System.Xml.Linq;
using IVSoftware.Portable.Disposable;

namespace IVSoftware.Portable.SQLiteMarkdown.MSTest;

[TestClass]
public class TestClass_260328_INPC
{
    /// <summary>
    /// POC that OBQFS exposes INPC of its items as ItemPropertyChangedEventArgs.
    /// </summary>
    /// <remarks>
    /// Features two serializable builders, one each for INCC and INPC.
    /// </remarks>
    [TestMethod, DoNotParallelize]
    public void Test_INPC_OBQFS_QueryMode()
    {
        using var te = this.TestableEpoch();

        string actual, expected;
        List<string> 
            builderINPC = new (),
            builderINCC = new ();
        SelectableQFModel itemT;

        // OBQFS exposes INPC of its items as ItemPropertyChangedEventArgs
        var items = new ObservableQueryFilterSource<SelectableQFModel>
        {
            QueryFilterConfig = QueryFilterConfig.Query
        };


        items.CollectionChanged += (sender, e) =>
        {
            builderINCC.Add(e.ToStringEx());
        };

        // NEW 260412
        var histoV2 = items.Model.To<EnumHistogrammer>();
        histoV2.PropertyChanged += (sender, e) =>
        { 
            // Works
            // This represents ObservableModeledCollection hooking
            // its EHInternal.PropertyChanges in its CTor.
        };

        items.PropertyChanged += (sender, e) =>
        {
            // #{1223BF7B-53C1-4610-A9F4-3F9EDE9FFD19}
            // OLD 260412 - was not working after the integration.
            // PROPOSED SOLUTION
            // - The missing link for legacy integration is taking the
            //   OMC (which internally is ModeledCollectionProtected) and
            //   dropping it laterally into the inherited OnPropertyChanged
            //   of the inherited WDT.
            // - There's also a SOC line drawn here.
            //   1. WDT is a concern of Modeled MDC because of the IME
            //   2. Make sure that a WDT does *not* sneak into the pristine
            //      ObservableModeledCollection. There's no need for it there.
            // - This fulfills the vision that MMDC is a one-to-many host
            //   of ObservableModeledCollection that can be swapped out on 
            //   the CanonicalSupersetProtected property handle.

            switch (e)
            {
                case ItemPropertyChangedEventArgs inpc when inpc.Item is SelectableQFModel item:
                    builderINPC.Add($"{e.PropertyName!}: {item.Description.PadToMaxLength(10, true)}");
                    break;
            }
        };

        actual = items.StateReport();
        actual.ToClipboardExpected();
        { }
        expected = @" 
[IME Len: 0, IsFiltering: False], [Net: 0, CC: 0, PMC: 0], [Query: SearchEntryState.Cleared, FilteringState.Ineligible]"
        ;
        Assert.AreEqual(expected.NormalizeResult(), actual.NormalizeResult(), "Expecting StateReport to match.");

        items.AddDynamic("Brown Dog", "[canine][color]", false, new() { "loyal", "friend", "furry" });

        Assert.IsFalse(items.IsFiltering);
        Assert.AreEqual(1, items.Count);
        actual = items.Model.ToString();
        actual.ToClipboardExpected();
        { }
        // ☆☆☆☆☆
        // FSOL: This pattern [model:1 match:0 qmatch:0 pmatch:0 live:0] is 'one' indication to show all items.
        // ☆☆☆☆☆
        expected = @" 
<model omc=""[OMC]"" mdc=""[MDC]"" histo=""[model:1 match:0 qmatch:0 pmatch:0 live:0]"">
  <item text=""312d1c21-0000-0000-0000-000000000000"" model=""[SelectableQFModel]"" index=""0"" />
</model>"
        ;

        Assert.AreEqual(
            expected.NormalizeResult(),
            actual.NormalizeResult(),
            "Expecting: FILTER MODE => ALWAYS TRACKS."
        );

        var inpcItem = (SelectableQFModel)items[0];

        // Toggle the item in the backend.
        inpcItem.IsChecked = true;

        actual = string.Join(Environment.NewLine, builderINPC);
        actual.ToClipboardExpected();
        { }
        expected = @" 
IsChecked: Brown Dog "
        ;

        Assert.AreEqual(
            expected.NormalizeResult(),
            actual.NormalizeResult(),
            "Expecting property changed event(s)."
        );
    }

    /// <summary>
    /// Demonstrates that the tilde ('~') used in source data produces non-idempotent normalization.
    /// </summary>
    /// <remarks>
    /// Two distinct descriptions:
    ///   "Bird~Feathered"
    ///   "Bird Feathered"
    /// both normalize to the same QueryTerm and FilterTerm ("bird~feathered").
    ///
    /// The tilde is always interpreted as an internal delimiter during tokenization,
    /// not as a literal character. As a result, the original distinction is lost.
    ///
    /// Practical effect:
    /// A query such as "bird~feathered" will match both records.
    ///
    /// This is an accepted limitation of the heuristic parser. "The worst that can happen isn't that bad."
    /// </remarks>
    [TestMethod, DoNotParallelize]
    public void Test_WeirdCornerTilde()
    {
        using var te = this.TestableEpoch();

        string actual, expected;

        var items = new ObservableQueryFilterSource<SelectableQFModel>
        {
            QueryFilterConfig = QueryFilterConfig.Query
        };
        ((IList)items).AddDynamic<SelectableQFModel>(description: "Bird~Feathered", tags: "[]", isChecked: false);
        ((IList)items).AddDynamic<SelectableQFModel>(description: "Bird Feathered", tags: "[]", isChecked: false);

        actual = JsonConvert.SerializeObject(items, Newtonsoft.Json.Formatting.Indented);
        actual.ToClipboardExpected();
        { }
        expected = @" 
[
  {
    ""Id"": ""312d1c21-0000-0000-0000-000000000000"",
    ""Description"": ""Bird~Feathered"",
    ""Keywords"": ""[]"",
    ""KeywordsDisplay"": """",
    ""Tags"": ""[]"",
    ""IsChecked"": false,
    ""Selection"": 0,
    ""IsEditing"": false,
    ""PrimaryKey"": ""312d1c21-0000-0000-0000-000000000000"",
    ""QueryTerm"": ""bird~feathered"",
    ""FilterTerm"": ""bird~feathered"",
    ""TagMatchTerm"": """",
    ""Properties"": ""{\r\n  \""Description\"": \""Bird~Feathered\"",\r\n  \""Tags\"": \""[]\""\r\n}""
  },
  {
    ""Id"": ""312d1c21-0000-0000-0000-000000000001"",
    ""Description"": ""Bird Feathered"",
    ""Keywords"": ""[]"",
    ""KeywordsDisplay"": """",
    ""Tags"": ""[]"",
    ""IsChecked"": false,
    ""Selection"": 0,
    ""IsEditing"": false,
    ""PrimaryKey"": ""312d1c21-0000-0000-0000-000000000001"",
    ""QueryTerm"": ""bird~feathered"",
    ""FilterTerm"": ""bird~feathered"",
    ""TagMatchTerm"": """",
    ""Properties"": ""{\r\n  \""Description\"": \""Bird Feathered\"",\r\n  \""Tags\"": \""[]\""\r\n}""
  }
]"
        ;

        Assert.AreEqual(
            expected.NormalizeResult(),
            actual.NormalizeResult(),
            "Expecting output to show the tilde problem."
        );
    }

    /// <summary>
    /// Exercise the flagship OPRC
    /// </summary>
    [TestMethod, DoNotParallelize]
    [Claim("00000000-0000-0000-0000-000000000000")]
    public void Test_ObservablePreviewRangeCollection()
    {
        string actual, expected;
        var builder = new List<string>();
        using var te = this.TestableEpoch();

        #region I T E M    G E N
        IList<SelectableQFModel>? eph = null;
        #endregion I T E M    G E N

        var itemsSource = new ObservablePreviewRangeCollection<SelectableQFModel>();

        #region E V E N T S
        itemsSource.CollectionChanged += (sender, e) =>
        {
            builder.Add(e.ToStringEx());
        };
        itemsSource.CollectionChanging += (sender, e) =>
        {
            builder.Add(e.ToStringEx());
        };
        #endregion E V E N T S

        #region S U B T E S T S

        subtest_RemoveWithCancel();
        void subtest_RemoveWithCancel()
        {
            using (itemsSource.RequestAuthority(ModelDataExchangeAuthority.CollectionDeferred))
            {
                itemsSource.PopulateForDemo(5);
            }

            Assert.AreEqual(
                CollectionChangingEventingPolicy.Discrete, 
                itemsSource.CollectionChangingEventingPolicy,
                "Expecting USE DISCRETE CHANGING EVENTS.");

            actual = string.Join(Environment.NewLine, builder); builder.Clear();
            actual.ToClipboardExpected();
            { }
            expected = @" 
Reset   NewItems=0 OldItems=0 NewStartingIndex=-1 OldStartingIndex=-1 NotifyCollectionChangingEventArgs
Add     NewItems=1 OldItems=0 NewStartingIndex= 0 OldStartingIndex=-1 NotifyCollectionChangingEventArgs
Add     NewItems=1 OldItems=0 NewStartingIndex= 1 OldStartingIndex=-1 NotifyCollectionChangingEventArgs
Add     NewItems=1 OldItems=0 NewStartingIndex= 2 OldStartingIndex=-1 NotifyCollectionChangingEventArgs
Add     NewItems=1 OldItems=0 NewStartingIndex= 3 OldStartingIndex=-1 NotifyCollectionChangingEventArgs
Add     NewItems=1 OldItems=0 NewStartingIndex= 4 OldStartingIndex=-1 NotifyCollectionChangingEventArgs
Add     NewItems=5 OldItems=* NewStartingIndex= 0 OldStartingIndex=-1 NotifyCollectionChangedEventArgs "
            ;

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting 1x INCC."
            );

            actual = itemsSource.ToString(out XElement _);
            actual.ToClipboardExpected();
            { }
            expected = @" 
<model mpath=""Id"">
  <item text=""312d1c21-0000-0000-0000-000000000000"" model=""[SelectableQFModel]"" preview=""Item01    "" index=""0"" />
  <item text=""312d1c21-0000-0000-0000-000000000001"" model=""[SelectableQFModel]"" preview=""Item02    "" index=""1"" />
  <item text=""312d1c21-0000-0000-0000-000000000002"" model=""[SelectableQFModel]"" preview=""Item03    "" index=""2"" />
  <item text=""312d1c21-0000-0000-0000-000000000003"" model=""[SelectableQFModel]"" preview=""Item04    "" index=""3"" />
  <item text=""312d1c21-0000-0000-0000-000000000004"" model=""[SelectableQFModel]"" preview=""Item05    "" index=""4"" />
</model>"
            ;

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting ToString(ReportFormat.ModelWithPreview) from active MarkdownContext."
            );

            builder.Clear();
            itemsSource.CollectionChangingEventingPolicy = CollectionChangingEventingPolicy.Coalesce;
            using (itemsSource.RequestAuthority(ModelDataExchangeAuthority.CollectionDeferred))
            {
                itemsSource.RemoveAt(1);
                itemsSource.RemoveAt(1);
                itemsSource.RemoveAt(1);
            }

            // View from the outside
            actual = itemsSource.ToString(out XElement _);
            actual.ToClipboardExpected();
            { }
            expected = @" 
<model mpath=""Id"">
  <item text=""312d1c21-0000-0000-0000-000000000000"" model=""[SelectableQFModel]"" preview=""Item01    "" index=""0"" />
  <item text=""312d1c21-0000-0000-0000-000000000004"" model=""[SelectableQFModel]"" preview=""Item05    "" index=""1"" />
</model>"
            ;

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting result to match."
            );
            actual = string.Join(Environment.NewLine, builder); builder.Clear();
            actual.ToClipboardExpected();
            { }
            expected = @" 
Digest  NewItems=4 OldItems=0 NewStartingIndex= 0 OldStartingIndex=-1 NotifyCollectionChangingEventArgs NotifyCollectionChangeReason.Digest
Reset   NewItems=* OldItems=* NewStartingIndex=-1 OldStartingIndex=-1 NotifyCollectionChangedEventArgs "
            ;

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting builder content to match."
            );
        }
        #endregion S U B T E S T S
    }
}
