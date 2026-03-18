using IVSoftware.Portable.Disposable;
using IVSoftware.Portable.SQLiteMarkdown.Collections.Preview;
using IVSoftware.Portable.SQLiteMarkdown.Common;
using IVSoftware.Portable.SQLiteMarkdown.Events;
using IVSoftware.Portable.SQLiteMarkdown.Util;
using IVSoftware.WinOS.MSTest.Extensions;
using Newtonsoft.Json;
using System.Collections;

namespace IVSoftware.Portable.SQLiteMarkdown.MSTest;

[TestClass]
public class TestClass_PreviewCollection
{
    [TestMethod, DoNotParallelize]
    public void Test_IListBasics()
    {
        string actual, expected;
        using var te = this.TestableEpoch();
        SelectableQFModel? currentItem;

        var builder = new List<string>();
        var opc = new ObservablePreviewCollection<SelectableQFModel>(useMutablePreviewEvents: false);
        DisposableHost dhostCancel = new();
        opc.CollectionChanging += (sender, e) =>
        {
            e.Cancel = !dhostCancel.IsZero();

            Assert.IsInstanceOfType<NotifyCollectionChangingEventArgs>(e);
            if(e.OldItems is IList list)
            {
                Assert.IsTrue(list.IsReadOnly);
            }
        };

        opc.CollectionChanged += (sender, e) =>
        {
            builder.Add(e.ToString(ReferenceEquals(sender, opc)));
        };

        currentItem = opc.AddDynamic("Brown Dog", "[canine][color]", false, new() { "loyal", "friend", "furry" });

        actual = string.Join(Environment.NewLine, builder);
        actual.ToClipboardExpected();
        { }
        expected = @" 
NetProjection.Add     NewItems= 1 NotifyCollectionChangedEventArgs           ";

        Assert.AreEqual(
            expected.NormalizeResult(),
            actual.NormalizeResult(),
            "Expecting a single INCC."
        );


        actual = JsonConvert.SerializeObject(opc, Formatting.Indented);
        actual.ToClipboardExpected();
        { }
        expected = @" 
[
  {
    ""Id"": ""312d1c21-0000-0000-0000-000000000000"",
    ""Description"": ""Brown Dog"",
    ""Keywords"": ""[\""loyal\"",\""friend\"",\""furry\""]"",
    ""KeywordsDisplay"": ""\""loyal\"",\""friend\"",\""furry\"""",
    ""Tags"": ""[canine] [color]"",
    ""IsChecked"": false,
    ""Selection"": 0,
    ""IsEditing"": false,
    ""PrimaryKey"": ""312d1c21-0000-0000-0000-000000000000"",
    ""QueryTerm"": ""brown~dog~loyal~friend~furry~[canine]~[color]"",
    ""FilterTerm"": ""brown~dog~loyal~friend~furry~[canine]~[color]"",
    ""TagMatchTerm"": ""[canine] [color]"",
    ""Properties"": ""{\r\n  \""Description\"": \""Brown Dog\"",\r\n  \""Tags\"": \""[canine] [color]\"",\r\n  \""Keywords\"": \""[\\\""loyal\\\"",\\\""friend\\\"",\\\""furry\\\""]\""\r\n}""
  }
]";

        Assert.AreEqual(
            expected.NormalizeResult(),
            actual.NormalizeResult(),
            "Expecting json serialization to match."
        );

        using(dhostCancel.GetToken())
        {
            opc.Remove(currentItem);
            Assert.AreEqual(1, opc.Count, "Expecting cancellation.");

            // Do it AGAIN to test the REVERT of the underlying REVERTABLE COLLECTION.
            // The thing is, if it hasn't actually reverted than there
            // will be no item to 'preview remove' and this would throw.
            opc.Remove(currentItem);
            Assert.AreEqual(1, opc.Count, "Expecting cancellation.");
        }
        Assert.AreEqual(1, opc.Count, "Expecting cancellation.");
        { }
    }
}
