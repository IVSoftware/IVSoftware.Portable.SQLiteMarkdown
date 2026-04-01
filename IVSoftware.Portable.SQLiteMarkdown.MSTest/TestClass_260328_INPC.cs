using IVSoftware.Portable.SQLiteMarkdown.Collections;
using IVSoftware.Portable.SQLiteMarkdown.Common;
using IVSoftware.WinOS.MSTest.Extensions;
using IVSoftware.Portable.SQLiteMarkdown.Internal;
using IVSoftware.Portable.SQLiteMarkdown.Util;
using IVSoftware.Portable.SQLiteMarkdown.Events;
using System.Collections;
using Newtonsoft.Json;
using IVSoftware.Portable.Collections.Preview;
using IVSoftware.Portable.SQLiteMarkdown.MSTest.Util;
using System.Collections.ObjectModel;

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
}
