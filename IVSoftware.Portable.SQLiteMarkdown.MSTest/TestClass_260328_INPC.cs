using IVSoftware.Portable.SQLiteMarkdown.Collections;
using IVSoftware.Portable.SQLiteMarkdown.Common;
using IVSoftware.WinOS.MSTest.Extensions;
using IVSoftware.Portable.SQLiteMarkdown.Internal;
using IVSoftware.Portable.SQLiteMarkdown.Util;
using IVSoftware.Portable.SQLiteMarkdown.Events;
using System.Collections;
using Newtonsoft.Json;
using System.Xml.Linq;
using IVSoftware.Portable.Collections.Preview;
using IVSoftware.Portable.SQLiteMarkdown.MSTest.Util;
using System.Collections.ObjectModel;
using IVSoftware.Portable.SQLiteMarkdown.Collections.Preview;
using System.Diagnostics;
using IVSoftware.Portable.Common.Attributes;
using IVSoftware.Portable.Common.Collections;

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

        // OBQFS exposes INPC of its items as ItemPropertyChangedEventArgs
        var items = new ObservableQueryFilterSource<SelectableQFModel>
        {
            QueryFilterConfig = QueryFilterConfig.Query
        };

        items.CollectionChanged += (sender, e) =>
        {
            builderINCC.Add(e.ToString(true));
        };

        items.PropertyChanged += (sender, e) =>
        {
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
[IME Len: 0, IsFiltering: False], [Net: null, CC: 0, PMC: 0], [Query: SearchEntryState.Cleared, FilteringState.Ineligible]"
        ;
        Assert.AreEqual(expected.NormalizeResult(), actual.NormalizeResult(), "Expecting StateReport to match.");

        items.AddDynamic("Brown Dog", "[canine][color]", false, new() { "loyal", "friend", "furry" });

        Assert.IsFalse(items.IsFiltering);
        Assert.AreEqual(1, items.Count);
        actual = items.Model.ToString();
        actual.ToClipboardExpected();
        { }

        // ☆☆☆☆☆
        // FSOL: This pattern [model:1 match:0 qmatch:0 pmatch:0] is 'one' indication to show all items.
        // ☆☆☆☆☆
        expected = @" 
<model mdc=""[MDC]"" histo=""[model:1 match:0 qmatch:0 pmatch:0]"" filters=""[No Active Filters]"">
  <xitem text=""312d1c21-0000-0000-0000-000000000000"" model=""[SelectableQFModel]"" order=""0"" />
</model>"
        ;

        Assert.AreEqual(
            expected.NormalizeResult(),
            actual.NormalizeResult(),
            "Expecting: FILTER MODE => ALWAYS TRACKS."
        );

        var inpcItem = items[0];
        Assert.IsInstanceOfType<SelectableQFModel>(inpcItem);
        { }

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
            builder.Add(e.ToString(ReferenceEquals(sender, itemsSource)));
        };
        itemsSource.CollectionChanging += (sender, e) =>
        {
            builder.Add(e.ToString(ReferenceEquals(sender, itemsSource)));
        };
        #endregion E V E N T S

        #region S U B T E S T S

        subtest_RemoveWithCancel();
        void subtest_RemoveWithCancel()
        {
            using (itemsSource.RequestModelEpochAuthority(ModelDataExchangeAuthority.CollectionDeferred, itemsSource))
            {
                itemsSource.PopulateForDemo(5);
            }

            Assert.AreEqual(
                CollectionChangingEventingOption.Discrete, 
                itemsSource.CollectionChangingEventingOption,
                "Expecting USE DISCRETE CHANGING EVENTS.");

            actual = string.Join(Environment.NewLine, builder); builder.Clear();
            actual.ToClipboardExpected();
            { }
            expected = @" 
NetProjection.Reset   NewItems= 0 OldItems= 0 NotifyCollectionChangingEventArgs          
NetProjection.Add     NewItems= 1 OldItems= 0 NewStartingIndex= 0 NotifyCollectionChangingEventArgs          
NetProjection.Add     NewItems= 1 OldItems= 0 NewStartingIndex= 1 NotifyCollectionChangingEventArgs          
NetProjection.Add     NewItems= 1 OldItems= 0 NewStartingIndex= 2 NotifyCollectionChangingEventArgs          
NetProjection.Add     NewItems= 1 OldItems= 0 NewStartingIndex= 3 NotifyCollectionChangingEventArgs          
NetProjection.Add     NewItems= 1 OldItems= 0 NewStartingIndex= 4 NotifyCollectionChangingEventArgs          
NetProjection.Add     NewItems= 5 OldItems= 0 NewStartingIndex= 0 NotifyCollectionChangeReason.Digest        NotifyCollectionChangingEventArgs          
NetProjection.Add     NewItems= 5 NewStartingIndex= 0 NotifyCollectionChangedEventArgs           "
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
  <xitem text=""312d1c21-0000-0000-0000-000000000000"" model=""[SelectableQFModel]"" order=""0"" preview=""Item01    "" />
  <xitem text=""312d1c21-0000-0000-0000-000000000001"" model=""[SelectableQFModel]"" order=""1"" preview=""Item02    "" />
  <xitem text=""312d1c21-0000-0000-0000-000000000002"" model=""[SelectableQFModel]"" order=""2"" preview=""Item03    "" />
  <xitem text=""312d1c21-0000-0000-0000-000000000003"" model=""[SelectableQFModel]"" order=""3"" preview=""Item04    "" />
  <xitem text=""312d1c21-0000-0000-0000-000000000004"" model=""[SelectableQFModel]"" order=""4"" preview=""Item05    "" />
</model>"
            ;

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting ToString(ReportFormat.ModelWithPreview) from active MarkdownContext."
            );

            builder.Clear();
            itemsSource.CollectionChangingEventingOption = CollectionChangingEventingOption.Deferred;
            using (itemsSource.RequestModelEpochAuthority(ModelDataExchangeAuthority.CollectionDeferred, itemsSource))
            {
                itemsSource.RemoveAt(1);
                itemsSource.RemoveAt(1);
                itemsSource.RemoveAt(1);
            }

            // View from the outside
            actual = itemsSource.ToString(out XElement _);
            actual.ToClipboardExpected();
            ;
            expected = @" 
<model modeling=""Id"">
  <xitem text=""312d1c21-0000-0000-0000-000000000000"" model=""[SelectableQFModel]"" order=""0"" preview=""Item01    "" />
  <xitem text=""312d1c21-0000-0000-0000-000000000004"" model=""[SelectableQFModel]"" order=""1"" preview=""Item05    "" />
</model>";

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting result to match."
            );
            actual = string.Join(Environment.NewLine, builder); builder.Clear();
            actual.ToClipboardExpected();
            { }
            expected = @" 
NetProjection.Digest  NewItems= 4 OldItems= 0 NewStartingIndex= 0 NotifyCollectionChangeReason.Digest        NotifyCollectionChangingEventArgs          
NetProjection.Reset   NotifyCollectionChangedEventArgs           "
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
