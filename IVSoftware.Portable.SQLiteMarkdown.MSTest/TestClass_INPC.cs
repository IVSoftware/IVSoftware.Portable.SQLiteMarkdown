using IVSoftware.Portable.SQLiteMarkdown.Collections;
using IVSoftware.Portable.SQLiteMarkdown.Common;
using IVSoftware.WinOS.MSTest.Extensions;
using IVSoftware.Portable.SQLiteMarkdown.Internal;
using IVSoftware.Portable.SQLiteMarkdown.Util;

namespace IVSoftware.Portable.SQLiteMarkdown.MSTest;

[TestClass]
public class TestClass_INPC
{
    [TestMethod, DoNotParallelize]
    public void Test_QueryModeINPC()
    {
        using var te = this.TestableEpoch();

        string actual, expected;
        List<string> 
            builderINPC = new (),
            builderINCC = new ();

        var items = new ObservableQueryFilterSource<SelectableQFModel>
        {
            QueryFilterConfig = QueryFilterConfig.Query
        };
        items.CollectionChanged += (sender, e) =>
        {
            builderINCC.Add(e.GetFormatted(true));
        };
        items.PropertyChanged += (sender, e) =>
        {
            if (sender is SelectableQFModel item)
            {
                builderINPC.Add($"{e.PropertyName!}: {item.Description.PadToMaxLength(10, true)}");
            }
        };

        actual = items.StateReport();
        actual.ToClipboardExpected();
        { }
        expected = @" 
[IME Len: 0, IsFiltering: False], [Net: 0, CC: 0, PMC: 0], [Query: SearchEntryState.Cleared, FilteringState.Ineligible]"
        ;
        Assert.AreEqual(expected.NormalizeResult(), actual.NormalizeResult(), "Expecting StateReport to match.");

        items.AddDynamic("Brown Dog", "[canine] [color]", false, new() { "loyal", "friend", "furry" });

        Assert.AreEqual(1, items.Count);
        { }
        return;

        actual = items.Model.ToString();
        actual.ToClipboardExpected();
        { }
        expected = @" 
<model autocount=""1"" count=""1"" matches=""1"">
  <xitem text=""312d1c21-0000-0000-0000-000000000000"" model=""[SelectableQFModel]"" sort=""0"" />
</model>"
        ;

        Assert.AreEqual(
            expected.NormalizeResult(),
            actual.NormalizeResult(),
            "Expecting: FILTER MODE => ALWAYS TRACKS."
        );

    }
}
