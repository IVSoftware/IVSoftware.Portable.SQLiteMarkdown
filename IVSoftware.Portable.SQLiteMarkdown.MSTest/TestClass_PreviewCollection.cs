using IVSoftware.Portable.Disposable;
using IVSoftware.Portable.SQLiteMarkdown.Collections.Preview;
using IVSoftware.Portable.SQLiteMarkdown.Common;
using IVSoftware.Portable.SQLiteMarkdown.Util;
using IVSoftware.WinOS.MSTest.Extensions;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Specialized;

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
        var opc = new ObservablePreviewCollection<SelectableQFModel>(eventScope: NotifyCollectionChangeScope.CancelOnly);
        DisposableHost dhostCancel = new();
        opc.CollectionChanging += (sender, e) =>
        {
            e.Cancel = !dhostCancel.IsZero();

            Assert.IsInstanceOfType<Collections.Preview.NotifyCollectionChangingEventArgs>(e);
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
        subtest_BatchOps();

        #region S U B T E S T S
        void subtest_BasicAddRemoveWithCancellation()
        {
            currentItem = opc.AddDynamic("Brown Dog", "[canine][color]", false, new() { "loyal", "friend", "furry" });

            actual = string.Join(Environment.NewLine, builder);
            actual.ToClipboardExpected();
            { }
            expected = @" 
NetProjection.Add     NewItems= 1 NewIndex= 0 NotifyCollectionChangedEventArgs           "
            ;

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting a single INCC."
            );


            actual = JsonConvert.SerializeObject(opc, Newtonsoft.Json.Formatting.Indented);
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

            actual = string.Join(Environment.NewLine, builder);
            actual.ToClipboardExpected();
            { }
            expected = @" 
NetProjection.Add     NewItems= 1 NewIndex= 0 NotifyCollectionChangedEventArgs           "
            ;

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting a single INCC."
            );

            actual = JsonConvert.SerializeObject(opc[0], Newtonsoft.Json.Formatting.Indented);
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

            builder.Clear();


            #region L o c a l F x
            void localOnCollectionChanging(object? sender, Collections.Preview.NotifyCollectionChangingEventArgs e)
            {
                actual = JsonConvert.SerializeObject(e.NewItems, Newtonsoft.Json.Formatting.Indented);
                actual.ToClipboardExpected();
                { }
                expected = @" 
[
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
  }
]";

                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting IList to match limit."
                );
                actual = JsonConvert.SerializeObject(e.OldItems, Newtonsoft.Json.Formatting.Indented);
                actual.ToClipboardExpected();
                { }
                expected = @" 
[
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
  }
]";

                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting IList to match limit."
                );
            }
            void localOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
            {
                actual = JsonConvert.SerializeObject(e.NewItems, Newtonsoft.Json.Formatting.Indented);
                actual.ToClipboardExpected();
                { }
                expected = @" 
[
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
  }
]";

                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting IList to match limit."
                );
                actual = JsonConvert.SerializeObject(e.OldItems, Newtonsoft.Json.Formatting.Indented);
                actual.ToClipboardExpected();
                { }
                expected = @" 
[
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
  }
]";

                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting IList to match limit."
                );
            }
            #endregion L o c a l F x
            using (opc.WithOnDispose(
                onInit: (sender, e) =>
                {
                    opc.CollectionChanging += localOnCollectionChanging;
                    opc.CollectionChanged += localOnCollectionChanged;
                },
                onDispose: (sender, e) =>
                {
                    opc.CollectionChanging -= localOnCollectionChanging;
                    opc.CollectionChanged -= localOnCollectionChanged;
                }))
            {
                opc[0] = item2;
            }


            Assert.AreEqual(1, opc.Count, "Expecting inert count");

            actual = string.Join(Environment.NewLine, builder);
            actual.ToClipboardExpected();
            { }
            expected = @" 
NetProjection.Replace NewItems= 1 OldItems= 1 NewIndex= 0 OldIndex= 0 NotifyCollectionChangedEventArgs           "
            ;

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting a single INCC."
            );

            actual = JsonConvert.SerializeObject(opc[0], Newtonsoft.Json.Formatting.Indented);
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

            builder.Clear();
            opc.Clear();
            Assert.AreEqual(0, opc.Count, "Expecting success.");

            actual = string.Join(Environment.NewLine, builder);
            actual.ToClipboardExpected();
            { }
            expected = @" 
NetProjection.Reset   NotifyCollectionChangedEventArgs           "
            ;

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting a single INCC."
            );

            builder.Clear();

            var item1 = opc.AddDynamic("Alpha", "", false, new());
            var item2 = opc.AddDynamic("Beta", "", false, new());


            actual = JsonConvert.SerializeObject(opc, Newtonsoft.Json.Formatting.Indented);
            actual.ToClipboardExpected();
            { }
            expected = @" 
[
  {
    ""Id"": ""312d1c21-0000-0000-0000-000000000003"",
    ""Description"": ""Alpha"",
    ""Keywords"": ""[]"",
    ""KeywordsDisplay"": """",
    ""Tags"": """",
    ""IsChecked"": false,
    ""Selection"": 0,
    ""IsEditing"": false,
    ""PrimaryKey"": ""312d1c21-0000-0000-0000-000000000003"",
    ""QueryTerm"": ""alpha"",
    ""FilterTerm"": ""alpha"",
    ""TagMatchTerm"": """",
    ""Properties"": ""{\r\n  \""Description\"": \""Alpha\""\r\n}""
  },
  {
    ""Id"": ""312d1c21-0000-0000-0000-000000000004"",
    ""Description"": ""Beta"",
    ""Keywords"": ""[]"",
    ""KeywordsDisplay"": """",
    ""Tags"": """",
    ""IsChecked"": false,
    ""Selection"": 0,
    ""IsEditing"": false,
    ""PrimaryKey"": ""312d1c21-0000-0000-0000-000000000004"",
    ""QueryTerm"": ""beta"",
    ""FilterTerm"": ""beta"",
    ""TagMatchTerm"": """",
    ""Properties"": ""{\r\n  \""Description\"": \""Beta\""\r\n}""
  }
]";

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting two items in initial order."
            );

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

            builder.Clear();
            opc.Move(0, 1);

            actual = string.Join(Environment.NewLine, builder);
            actual.ToClipboardExpected();
            { }
            expected = @" 
NetProjection.Move    NewItems= 1 OldItems= 1 NewIndex= 1 OldIndex= 0 NotifyCollectionChangedEventArgs           "
            ;

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting a single INCC."
            );

            actual = JsonConvert.SerializeObject(opc, Newtonsoft.Json.Formatting.Indented);
            actual.ToClipboardExpected();
            { }
            expected = @" 
[
  {
    ""Id"": ""312d1c21-0000-0000-0000-000000000004"",
    ""Description"": ""Beta"",
    ""Keywords"": ""[]"",
    ""KeywordsDisplay"": """",
    ""Tags"": """",
    ""IsChecked"": false,
    ""Selection"": 0,
    ""IsEditing"": false,
    ""PrimaryKey"": ""312d1c21-0000-0000-0000-000000000004"",
    ""QueryTerm"": ""beta"",
    ""FilterTerm"": ""beta"",
    ""TagMatchTerm"": """",
    ""Properties"": ""{\r\n  \""Description\"": \""Beta\""\r\n}""
  },
  {
    ""Id"": ""312d1c21-0000-0000-0000-000000000003"",
    ""Description"": ""Alpha"",
    ""Keywords"": ""[]"",
    ""KeywordsDisplay"": """",
    ""Tags"": """",
    ""IsChecked"": false,
    ""Selection"": 0,
    ""IsEditing"": false,
    ""PrimaryKey"": ""312d1c21-0000-0000-0000-000000000003"",
    ""QueryTerm"": ""alpha"",
    ""FilterTerm"": ""alpha"",
    ""TagMatchTerm"": """",
    ""Properties"": ""{\r\n  \""Description\"": \""Alpha\""\r\n}""
  }
]"
            ;

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting two items in moved order."
            );
        }

        void subtest_BatchOps()
        {
            var opc = new ObservablePreviewCollection<SelectableQFModel>();
            var builder = new List<string>();

            #region L o c a l F x				
            using var local = opc.WithOnDispose(
                onInit: (sender, e) =>
                {
                    opc.CollectionChanged += localOnCollectionChanged;
                },
                onDispose: (sender, e) =>
                {
                    opc.CollectionChanged -= localOnCollectionChanged;
                });
            void localOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
            {
                builder.Add(e.ToString(true));
            }
            #endregion L o c a l F x

            using(opc.BeginBatch())
            {
                foreach (var item in new List<SelectableQFModel>().PopulateForDemo(10))
                {
                    opc.Add(item);
                }
            }

            actual = string.Join(Environment.NewLine, builder);
            actual.ToClipboardExpected();
            { }
            expected = @" 
NetProjection.Add     NewItems=10 NewIndex= 0 NotifyCollectionChangedEventArgs           "
            ;

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting one digest event for batch."
            );

            Assert.AreEqual(10, opc.Count, "Expecting batch apply.");
        }
        #endregion S U B T E S T S
    }
}
