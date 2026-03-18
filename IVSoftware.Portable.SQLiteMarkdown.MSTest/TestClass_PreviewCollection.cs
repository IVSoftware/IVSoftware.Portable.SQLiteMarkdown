using IVSoftware.Portable.Collections;
using IVSoftware.Portable.Disposable;
using IVSoftware.Portable.SQLiteMarkdown.Collections.Preview;
using IVSoftware.Portable.SQLiteMarkdown.Common;
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
        List<SelectableQFModel> Ephemeral() => new List<SelectableQFModel>();
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

        subtest_BasicAddRemoveWithCancellation();
        subtest_ReplaceWithCancellationAndRevert();
        subtest_MoveWithCancellationAndRevert();

        #region S U B T E S T S
        void subtest_BasicAddRemoveWithCancellation()
        {

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

            using (dhostCancel.GetToken())
            {
                opc.Remove(currentItem);
                Assert.AreEqual(1, opc.Count, "Expecting cancellation.");

                // Do it AGAIN to test the REVERT of the underlying REVERTABLE COLLECTION.
                // The thing is, if it hasn't actually reverted than there
                // will be no item to 'preview remove' and this would throw.
                opc.Remove(currentItem);
                Assert.AreEqual(1, opc.Count, "Expecting cancellation.");
            }
            opc.Remove(currentItem);
            Assert.AreEqual(0, opc.Count, "Expecting success.");
            { }
        }

        void subtest_ReplaceWithCancellationAndRevert()
        {
            builder.Clear();
            var item1 = opc.AddDynamic("Alpha", "", false, new());
            var item2 = Ephemeral().AddDynamic("Beta", "", false, new());

            Assert.AreEqual(1, opc.Count, "Expecting ALPHA success only.");

            actual = JsonConvert.SerializeObject(opc[0], Formatting.Indented);
            actual.ToClipboardExpected();
            { }
            expected = @" 
{
  ""Id"": ""312d1c21-0000-0000-0000-000000000001"",
  ""Description"": ""Alpha"",
  ""Keywords"": ""[]"",
  ""KeywordsDisplay"": """",
  ""Tags"": """",
  ""IsChecked"": false,
  ""Selection"": 0,
  ""IsEditing"": false,
  ""PrimaryKey"": ""312d1c21-0000-0000-0000-000000000001"",
  ""QueryTerm"": ""alpha"",
  ""FilterTerm"": ""alpha"",
  ""TagMatchTerm"": """",
  ""Properties"": ""{\r\n  \""Description\"": \""Alpha\""\r\n}""
}";

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting index[0]."
            );

            using (dhostCancel.GetToken())
            {
                opc[0] = item2;

                Assert.AreEqual("Alpha", opc[0].Description, "Replace should be canceled.");

                // Force second attempt to validate revert integrity
                opc[0] = item2;
                Assert.AreEqual("Alpha", opc[0].Description, "Still expecting cancel.");
            }

            opc[0] = item2;

            Assert.AreEqual(1, opc.Count, "Expecting ALPHA success only.");

            actual = JsonConvert.SerializeObject(opc[0], Formatting.Indented);
            actual.ToClipboardExpected();
            { }
            expected = @" 
{
  ""Id"": ""312d1c21-0000-0000-0000-000000000002"",
  ""Description"": ""Beta"",
  ""Keywords"": ""[]"",
  ""KeywordsDisplay"": """",
  ""Tags"": """",
  ""IsChecked"": false,
  ""Selection"": 0,
  ""IsEditing"": false,
  ""PrimaryKey"": ""312d1c21-0000-0000-0000-000000000002"",
  ""QueryTerm"": ""beta"",
  ""FilterTerm"": ""beta"",
  ""TagMatchTerm"": """",
  ""Properties"": ""{\r\n  \""Description\"": \""Beta\""\r\n}""
}"
            ;

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting BETA @ index[0]."
            );
        }

        void subtest_MoveWithCancellationAndRevert()
        {
            Assert.AreNotEqual(0, opc.Count, "Expecting carry-over.");
            using (dhostCancel.GetToken())
            {
                opc.Clear();
                Assert.AreNotEqual(0, opc.Count, "Expecting cancel.");
            }
            opc.Clear();
            Assert.AreEqual(0, opc.Count, "Expecting cancel.");
            { }



            builder.Clear();

            var item1 = opc.AddDynamic("Alpha", "", false, new());
            var item2 = opc.AddDynamic("Beta", "", false, new());

            Assert.AreEqual(2, opc.Count, "Expecting two items.");

            using (dhostCancel.GetToken())
            {
                opc.Move(0, 1);

                Assert.AreEqual("Alpha", opc[0].Description, "Move should be canceled.");
                Assert.AreEqual("Beta", opc[1].Description, "Order should remain unchanged.");

                // Force second attempt to validate revert integrity
                opc.Move(0, 1);

                Assert.AreEqual("Alpha", opc[0].Description, "Still expecting cancel.");
                Assert.AreEqual("Beta", opc[1].Description, "Still unchanged.");
            }

            opc.Move(0, 1);

            Assert.AreEqual(2, opc.Count, "Expecting count unchanged.");

            Assert.AreEqual("Beta", opc[0].Description, "Expecting BETA @ index[0].");
            Assert.AreEqual("Alpha", opc[1].Description, "Expecting ALPHA @ index[1].");

            actual = JsonConvert.SerializeObject(opc, Formatting.Indented);
            actual.ToClipboardExpected();
            { }
            // capture expected after first run

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting reordered collection."
            );
        }
        #endregion S U B T E S T S
    }
}
