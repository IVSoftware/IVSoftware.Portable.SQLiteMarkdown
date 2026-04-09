using IVSoftware.Portable.Disposable;
using IVSoftware.Portable.Collections.Preview;
using IVSoftware.Portable.SQLiteMarkdown.Common;
using IVSoftware.Portable.SQLiteMarkdown.Util;
using IVSoftware.Portable.Xml.Linq.XBoundObject.Modeling;
using IVSoftware.WinOS.MSTest.Extensions;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Specialized;
using System.Threading.Channels;
using System.Xml.Linq;
using IVSoftware.Portable.Common.Collections;
using IVSoftware.Portable.Xml.Linq.Collections;

namespace IVSoftware.Portable.SQLiteMarkdown.MSTest;

[TestClass]
public class TestClass_ObservablePreviewCollection
{
    /// <summary>
    /// Prototype of an index gap detector for Diff.
    /// </summary>
    [TestMethod]
    public void Test_GapDetector()
    {
        string actual, expected;

        string
            before = "ABCDE",
            after  = "AbcDe----";

        int 
            current = 0,
            lastReplaceIndex = int.MinValue;
        char replace, replaceWith;
        bool isContiguous = true;

        List <(int index, char a, char b)> changes = new();

        // Block of replace actions where some may be idempotent.
        while (current < before.Length && current < after.Length)
        {
            replace = before[current];
            replaceWith = after[current];
            if (!replace.Equals(replaceWith))
            {
                changes.Add((current, replace, replaceWith));
                if (isContiguous 
                    && lastReplaceIndex != int.MinValue
                    && lastReplaceIndex != current - 1)
                {
                    isContiguous = false;
                }
                lastReplaceIndex = current;
            }
            current++;
        }

        actual = JsonConvert.SerializeObject(changes, Formatting.Indented);
        actual.ToClipboardExpected();
        { }
        expected = @" 
[
  {
    ""Item1"": 1,
    ""Item2"": ""B"",
    ""Item3"": ""b""
  },
  {
    ""Item1"": 2,
    ""Item2"": ""C"",
    ""Item3"": ""c""
  },
  {
    ""Item1"": 4,
    ""Item2"": ""E"",
    ""Item3"": ""e""
  }
]"
        ;

        Assert.AreEqual(
            expected.NormalizeResult(),
            actual.NormalizeResult(),
            "Expecting result to match."
        );
        Assert.IsFalse(isContiguous);
    }

    /// <summary>
    /// Instantiates an ObservablePreviewCollection{T} and exercises it without attaching MDC.
    /// </summary>
    [TestMethod, DoNotParallelize]
    public void Test_PreviewOnly()
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
        subtest_ReplaceWithCancellation();
        subtest_MoveWithCancellation();

        #region S U B T E S T S
        void subtest_BasicAddRemoveWithCancellation()
        {
            currentItem = opc.AddDynamic("Brown Dog", "[canine][color]", false, new() { "loyal", "friend", "furry" });

            actual = string.Join(Environment.NewLine, builder);
            actual.ToClipboardExpected();
            { }
            expected = @" 
NetProjection.Add     NewItems= 1 NewStartingIndex= 0 NotifyCollectionChangedEventArgs           "
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

        void subtest_ReplaceWithCancellation()
        {
            builder.Clear();
            var item1 = opc.AddDynamic("Alpha", "", false, new());
            var item2 = Ephemeral().AddDynamic("Beta", "", false, new());

            Assert.AreEqual(1, opc.Count, "Expecting ALPHA success only.");

            actual = string.Join(Environment.NewLine, builder);
            actual.ToClipboardExpected();
            { }
            expected = @" 
NetProjection.Add     NewItems= 1 NewStartingIndex= 0 NotifyCollectionChangedEventArgs           "
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
            void localOnCollectionChanging(object? sender, NotifyCollectionChangingEventArgs e)
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
NetProjection.Replace NewItems= 1 OldItems= 1 NewStartingIndex= 0 OldStartingIndex= 0 NotifyCollectionChangedEventArgs           "
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

        void subtest_MoveWithCancellation()
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
NetProjection.Move    NewItems= 1 OldItems= 1 NewStartingIndex= 1 OldStartingIndex= 0 NotifyCollectionChangedEventArgs           "
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
        #endregion S U B T E S T S
    }

    /// <summary>
    /// Instantiates an ObservableRangeCollection{T} and exercises it without attaching MDC.
    /// </summary>
    [TestMethod]
    public void Test_BasicIRangeable()
    {
        string actual, expected;
        using var te = this.TestableEpoch();
        var builder = new List<string>();

        #region I T E M    G E N
        IList<SelectableQFModel>? eph = null;
        #endregion I T E M    G E N

        var opc = new ObservableRangeCollection<SelectableQFModel>();
        var range = (List <SelectableQFModel>)new List<SelectableQFModel>().PopulateForDemo(5);

        #region E V E N T S
        opc.CollectionChanged += (sender, e) =>
        {
            builder.Add(e.ToString(ReferenceEquals(sender, opc)));
        };
        #endregion E V E N T S

        subtest_AddRange();
        subtest_AddRangeDistinct();
        subtest_InsertRange();
        subtest_RemoveRange();
        subtest_RemoveMultiple();
        subtest_FullWipe();

        #region S U B T E S T S
        void subtest_AddRange()
        {
            opc.AddRange(range);

            actual = opc.ToString(out XElement _);
            actual.ToClipboardExpected();
            expected = @" 
<model mpath=""Id"">
  <item text=""312d1c21-0000-0000-0000-000000000000"" model=""[SelectableQFModel]"" order=""0"" preview=""Item01    "" />
  <item text=""312d1c21-0000-0000-0000-000000000001"" model=""[SelectableQFModel]"" order=""1"" preview=""Item02    "" />
  <item text=""312d1c21-0000-0000-0000-000000000002"" model=""[SelectableQFModel]"" order=""2"" preview=""Item03    "" />
  <item text=""312d1c21-0000-0000-0000-000000000003"" model=""[SelectableQFModel]"" order=""3"" preview=""Item04    "" />
  <item text=""312d1c21-0000-0000-0000-000000000004"" model=""[SelectableQFModel]"" order=""4"" preview=""Item05    "" />
</model>"
            ;

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting implicit model to match."
            );


            actual = string.Join(Environment.NewLine, builder);
            actual.ToClipboardExpected();
            { }
            expected = @" 
NetProjection.Add     NewItems= 5 NewStartingIndex= 0 NotifyCollectionChangedEventArgs           ";

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting a single aggregate collection change."
            );
        }

        void subtest_AddRangeDistinct()
        {
            actual = opc.ToString(out XElement _);
            actual.ToClipboardExpected();
            { }
            expected = @" 
<model mpath=""Id"">
  <item text=""312d1c21-0000-0000-0000-000000000000"" model=""[SelectableQFModel]"" order=""0"" preview=""Item01    "" />
  <item text=""312d1c21-0000-0000-0000-000000000001"" model=""[SelectableQFModel]"" order=""1"" preview=""Item02    "" />
  <item text=""312d1c21-0000-0000-0000-000000000002"" model=""[SelectableQFModel]"" order=""2"" preview=""Item03    "" />
  <item text=""312d1c21-0000-0000-0000-000000000003"" model=""[SelectableQFModel]"" order=""3"" preview=""Item04    "" />
  <item text=""312d1c21-0000-0000-0000-000000000004"" model=""[SelectableQFModel]"" order=""4"" preview=""Item05    "" />
</model>";

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Verify carry over from previous subtest."
            );

            var mixedRange = new SelectableQFModel[]
            {
                range[0],
                eph.AddDynamic("Distinct01"),
                range[0],
                range[2],
                eph.AddDynamic("Distinct02"),
                range[4],
            };

            opc.AddRangeDistinct(mixedRange);

            actual = opc.ToString(out XElement _);
            actual.ToClipboardExpected();
            { }
            expected = @" 
<model mpath=""Id"">
  <item text=""312d1c21-0000-0000-0000-000000000000"" model=""[SelectableQFModel]"" order=""0"" preview=""Item01    "" />
  <item text=""312d1c21-0000-0000-0000-000000000001"" model=""[SelectableQFModel]"" order=""1"" preview=""Item02    "" />
  <item text=""312d1c21-0000-0000-0000-000000000002"" model=""[SelectableQFModel]"" order=""2"" preview=""Item03    "" />
  <item text=""312d1c21-0000-0000-0000-000000000003"" model=""[SelectableQFModel]"" order=""3"" preview=""Item04    "" />
  <item text=""312d1c21-0000-0000-0000-000000000004"" model=""[SelectableQFModel]"" order=""4"" preview=""Item05    "" />
  <item text=""312d1c21-0000-0000-0000-000000000005"" model=""[SelectableQFModel]"" order=""5"" preview=""Distinct01"" />
  <item text=""312d1c21-0000-0000-0000-000000000006"" model=""[SelectableQFModel]"" order=""6"" preview=""Distinct02"" />
</model>";

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting 4 items skipped and 2 items added.."
            );
        }

        void subtest_InsertRange()
        {
            actual = opc.ToString(out XElement _);
            actual.ToClipboardExpected();
            { }
            expected = @" 
<model mpath=""Id"">
  <item text=""312d1c21-0000-0000-0000-000000000000"" model=""[SelectableQFModel]"" order=""0"" preview=""Item01    "" />
  <item text=""312d1c21-0000-0000-0000-000000000001"" model=""[SelectableQFModel]"" order=""1"" preview=""Item02    "" />
  <item text=""312d1c21-0000-0000-0000-000000000002"" model=""[SelectableQFModel]"" order=""2"" preview=""Item03    "" />
  <item text=""312d1c21-0000-0000-0000-000000000003"" model=""[SelectableQFModel]"" order=""3"" preview=""Item04    "" />
  <item text=""312d1c21-0000-0000-0000-000000000004"" model=""[SelectableQFModel]"" order=""4"" preview=""Item05    "" />
  <item text=""312d1c21-0000-0000-0000-000000000005"" model=""[SelectableQFModel]"" order=""5"" preview=""Distinct01"" />
  <item text=""312d1c21-0000-0000-0000-000000000006"" model=""[SelectableQFModel]"" order=""6"" preview=""Distinct02"" />
</model>";

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting carry over from previous subtest."
            );

            opc.InsertRange(2, new[]
            {
                eph.AddDynamic("Insert01"),
                eph.AddDynamic("Insert02"),
                eph.AddDynamic("Insert03"),
                eph.AddDynamic("Insert04"),
                eph.AddDynamic("Insert05"),
            });

            actual = opc.ToString(out XElement _);
            actual.ToClipboardExpected();
            { }
            expected = @" 
<model mpath=""Id"">
  <item text=""312d1c21-0000-0000-0000-000000000000"" model=""[SelectableQFModel]"" order=""0"" preview=""Item01    "" />
  <item text=""312d1c21-0000-0000-0000-000000000001"" model=""[SelectableQFModel]"" order=""1"" preview=""Item02    "" />
  <item text=""312d1c21-0000-0000-0000-000000000007"" model=""[SelectableQFModel]"" order=""2"" preview=""Insert01  "" />
  <item text=""312d1c21-0000-0000-0000-000000000008"" model=""[SelectableQFModel]"" order=""3"" preview=""Insert02  "" />
  <item text=""312d1c21-0000-0000-0000-000000000009"" model=""[SelectableQFModel]"" order=""4"" preview=""Insert03  "" />
  <item text=""312d1c21-0000-0000-0000-00000000000a"" model=""[SelectableQFModel]"" order=""5"" preview=""Insert04  "" />
  <item text=""312d1c21-0000-0000-0000-00000000000b"" model=""[SelectableQFModel]"" order=""6"" preview=""Insert05  "" />
  <item text=""312d1c21-0000-0000-0000-000000000002"" model=""[SelectableQFModel]"" order=""7"" preview=""Item03    "" />
  <item text=""312d1c21-0000-0000-0000-000000000003"" model=""[SelectableQFModel]"" order=""8"" preview=""Item04    "" />
  <item text=""312d1c21-0000-0000-0000-000000000004"" model=""[SelectableQFModel]"" order=""9"" preview=""Item05    "" />
  <item text=""312d1c21-0000-0000-0000-000000000005"" model=""[SelectableQFModel]"" order=""10"" preview=""Distinct01"" />
  <item text=""312d1c21-0000-0000-0000-000000000006"" model=""[SelectableQFModel]"" order=""11"" preview=""Distinct02"" />
</model>"
            ;

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting 4 items skipped and 2 items added.."
            );
        }

        void subtest_RemoveRange()
        {
            actual = opc.ToString(out XElement _);
            actual.ToClipboardExpected();
            { }
            expected = @" 
<model mpath=""Id"">
  <item text=""312d1c21-0000-0000-0000-000000000000"" model=""[SelectableQFModel]"" order=""0"" preview=""Item01    "" />
  <item text=""312d1c21-0000-0000-0000-000000000001"" model=""[SelectableQFModel]"" order=""1"" preview=""Item02    "" />
  <item text=""312d1c21-0000-0000-0000-000000000007"" model=""[SelectableQFModel]"" order=""2"" preview=""Insert01  "" />
  <item text=""312d1c21-0000-0000-0000-000000000008"" model=""[SelectableQFModel]"" order=""3"" preview=""Insert02  "" />
  <item text=""312d1c21-0000-0000-0000-000000000009"" model=""[SelectableQFModel]"" order=""4"" preview=""Insert03  "" />
  <item text=""312d1c21-0000-0000-0000-00000000000a"" model=""[SelectableQFModel]"" order=""5"" preview=""Insert04  "" />
  <item text=""312d1c21-0000-0000-0000-00000000000b"" model=""[SelectableQFModel]"" order=""6"" preview=""Insert05  "" />
  <item text=""312d1c21-0000-0000-0000-000000000002"" model=""[SelectableQFModel]"" order=""7"" preview=""Item03    "" />
  <item text=""312d1c21-0000-0000-0000-000000000003"" model=""[SelectableQFModel]"" order=""8"" preview=""Item04    "" />
  <item text=""312d1c21-0000-0000-0000-000000000004"" model=""[SelectableQFModel]"" order=""9"" preview=""Item05    "" />
  <item text=""312d1c21-0000-0000-0000-000000000005"" model=""[SelectableQFModel]"" order=""10"" preview=""Distinct01"" />
  <item text=""312d1c21-0000-0000-0000-000000000006"" model=""[SelectableQFModel]"" order=""11"" preview=""Distinct02"" />
</model>"
            ;

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting carry over from previous subtest."
            );

            opc.RemoveRange(7, 9);

            actual = opc.ToString(out XElement _);
            actual.ToClipboardExpected();
            { }
            expected = @" 
<model mpath=""Id"">
  <item text=""312d1c21-0000-0000-0000-000000000000"" model=""[SelectableQFModel]"" order=""0"" preview=""Item01    "" />
  <item text=""312d1c21-0000-0000-0000-000000000001"" model=""[SelectableQFModel]"" order=""1"" preview=""Item02    "" />
  <item text=""312d1c21-0000-0000-0000-000000000007"" model=""[SelectableQFModel]"" order=""2"" preview=""Insert01  "" />
  <item text=""312d1c21-0000-0000-0000-000000000008"" model=""[SelectableQFModel]"" order=""3"" preview=""Insert02  "" />
  <item text=""312d1c21-0000-0000-0000-000000000009"" model=""[SelectableQFModel]"" order=""4"" preview=""Insert03  "" />
  <item text=""312d1c21-0000-0000-0000-00000000000a"" model=""[SelectableQFModel]"" order=""5"" preview=""Insert04  "" />
  <item text=""312d1c21-0000-0000-0000-00000000000b"" model=""[SelectableQFModel]"" order=""6"" preview=""Insert05  "" />
  <item text=""312d1c21-0000-0000-0000-000000000005"" model=""[SelectableQFModel]"" order=""7"" preview=""Distinct01"" />
  <item text=""312d1c21-0000-0000-0000-000000000006"" model=""[SelectableQFModel]"" order=""8"" preview=""Distinct02"" />
</model>"
            ;

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting Item 03-05 removed at indexes 7, 8, 9 and ordering is updated."
            );
        }

        void subtest_RemoveMultiple()
        {
            actual = opc.ToString(out XElement _);
            actual.ToClipboardExpected();
            { }
            expected = @" 
<model mpath=""Id"">
  <item text=""312d1c21-0000-0000-0000-000000000000"" model=""[SelectableQFModel]"" order=""0"" preview=""Item01    "" />
  <item text=""312d1c21-0000-0000-0000-000000000001"" model=""[SelectableQFModel]"" order=""1"" preview=""Item02    "" />
  <item text=""312d1c21-0000-0000-0000-000000000007"" model=""[SelectableQFModel]"" order=""2"" preview=""Insert01  "" />
  <item text=""312d1c21-0000-0000-0000-000000000008"" model=""[SelectableQFModel]"" order=""3"" preview=""Insert02  "" />
  <item text=""312d1c21-0000-0000-0000-000000000009"" model=""[SelectableQFModel]"" order=""4"" preview=""Insert03  "" />
  <item text=""312d1c21-0000-0000-0000-00000000000a"" model=""[SelectableQFModel]"" order=""5"" preview=""Insert04  "" />
  <item text=""312d1c21-0000-0000-0000-00000000000b"" model=""[SelectableQFModel]"" order=""6"" preview=""Insert05  "" />
  <item text=""312d1c21-0000-0000-0000-000000000005"" model=""[SelectableQFModel]"" order=""7"" preview=""Distinct01"" />
  <item text=""312d1c21-0000-0000-0000-000000000006"" model=""[SelectableQFModel]"" order=""8"" preview=""Distinct02"" />
</model>"
            ;
            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting carry over from previous subtest."
            );
            var itemsT = opc.Where(_ => _.Description.Contains("01")).ToArray();
            opc.RemoveMultiple(itemsT);

            actual = opc.ToString(out XElement _);
            actual.ToClipboardExpected();
            { }
            expected = @" 
<model mpath=""Id"">
  <item text=""312d1c21-0000-0000-0000-000000000001"" model=""[SelectableQFModel]"" order=""0"" preview=""Item02    "" />
  <item text=""312d1c21-0000-0000-0000-000000000008"" model=""[SelectableQFModel]"" order=""1"" preview=""Insert02  "" />
  <item text=""312d1c21-0000-0000-0000-000000000009"" model=""[SelectableQFModel]"" order=""2"" preview=""Insert03  "" />
  <item text=""312d1c21-0000-0000-0000-00000000000a"" model=""[SelectableQFModel]"" order=""3"" preview=""Insert04  "" />
  <item text=""312d1c21-0000-0000-0000-00000000000b"" model=""[SelectableQFModel]"" order=""4"" preview=""Insert05  "" />
  <item text=""312d1c21-0000-0000-0000-000000000006"" model=""[SelectableQFModel]"" order=""5"" preview=""Distinct02"" />
</model>"
            ;
            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting that items matches to '01' have been discontiguously removed"
            );

            var indexes = new[] { 1, 0, 5 };
            opc.RemoveMultiple(indexes);

            actual = opc.ToString(out XElement _);
            actual.ToClipboardExpected();
            { }
            expected = @" 
<model mpath=""Id"">
  <item text=""312d1c21-0000-0000-0000-000000000009"" model=""[SelectableQFModel]"" order=""0"" preview=""Insert03  "" />
  <item text=""312d1c21-0000-0000-0000-00000000000a"" model=""[SelectableQFModel]"" order=""1"" preview=""Insert04  "" />
  <item text=""312d1c21-0000-0000-0000-00000000000b"" model=""[SelectableQFModel]"" order=""2"" preview=""Insert05  "" />
</model>"
            ;
            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting discontiguous indexes (corresponding to '02' matches) have been removed"
            );
        }

        void subtest_FullWipe()
        {
            opc.RemoveRange(0, opc.Count - 1);
            actual = opc.ToString(out XElement _);
            actual.ToClipboardExpected();
            { }
            expected = @" 
<model mpath=""Id"">
</model>"
            ;

        }
        #endregion S U B T E S T S
    }


    /// <summary>
    /// Instantiates a Modeled OPC that inherits ObservableCollection and binds itself to MMDC.
    /// </summary>
    [TestMethod, DoNotParallelize]
    public void Test_BasicModeledOPC()
    {
        string actual, expected;
        using var te = this.TestableEpoch();
        var builder = new List<string>();
        ObservableCollectionWithInternalMMDC onp = new ();

        #region E V E N T S
        // Differentiate between the itemsSource being driven by
        // the simView and the simView being driven by itemsSource.
        onp.CollectionChanged += (sender, e) =>
        {
            builder.Add(e.ToString(ReferenceEquals(sender, onp)));
        };
        #endregion E V E N T S

        subtest_PopulateWithDiscreteEvents();
        subtest_PopulateWithRange();

        #region S U B T E S T S
        void subtest_PopulateWithDiscreteEvents()
        {
            onp.PopulateForDemo(5);

            actual = onp.Model.ToString();
            actual.ToClipboardExpected();
            { }
            expected = @" 
<model mdc=""[MDC]"" histo=""[model:5 match:0 qmatch:0 pmatch:0]"" filters=""[No Active Filters]"">
  <item text=""312d1c21-0000-0000-0000-000000000000"" model=""[SelectableQFModel]"" order=""0"" />
  <item text=""312d1c21-0000-0000-0000-000000000001"" model=""[SelectableQFModel]"" order=""1"" />
  <item text=""312d1c21-0000-0000-0000-000000000002"" model=""[SelectableQFModel]"" order=""2"" />
  <item text=""312d1c21-0000-0000-0000-000000000003"" model=""[SelectableQFModel]"" order=""3"" />
  <item text=""312d1c21-0000-0000-0000-000000000004"" model=""[SelectableQFModel]"" order=""4"" />
</model>"
            ;

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting model has tracked."
            );

            actual = string.Join(Environment.NewLine, builder);
            actual.ToClipboardExpected();
            { }
            expected = @" 
NetProjection.Reset   NotifyCollectionChangedEventArgs           
NetProjection.Add     NewItems= 1 NewStartingIndex= 0 NotifyCollectionChangedEventArgs           
NetProjection.Add     NewItems= 1 NewStartingIndex= 1 NotifyCollectionChangedEventArgs           
NetProjection.Add     NewItems= 1 NewStartingIndex= 2 NotifyCollectionChangedEventArgs           
NetProjection.Add     NewItems= 1 NewStartingIndex= 3 NotifyCollectionChangedEventArgs           
NetProjection.Add     NewItems= 1 NewStartingIndex= 4 NotifyCollectionChangedEventArgs           ";

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting model has emitted discrete events."
            );

            actual = onp.ToString(FormattingOMC.StateReport);
            actual.ToClipboardExpected();
            { }
            expected = @" 
[IME Len: 0, IsFiltering: False], [Net: 5, CC: 5, PMC: 0], [QueryAndFilter: SearchEntryState.Cleared, FilteringState.Ineligible]";

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting quiescent initial state."
            );
        }

        void subtest_PopulateWithRange()
        {
            te.ResetEpoch();
            builder.Clear();

            onp.PopulateForDemo(5, PopulateOptions.DetectIRangeable);

            actual = onp.Model.ToString();
            actual.ToClipboardExpected();
            { }

            expected = @" 
<model mdc=""[MDC]"" histo=""[model:5 match:0 qmatch:0 pmatch:0]"" filters=""[No Active Filters]"">
  <item text=""312d1c21-0000-0000-0000-000000000000"" model=""[SelectableQFModel]"" order=""0"" />
  <item text=""312d1c21-0000-0000-0000-000000000001"" model=""[SelectableQFModel]"" order=""1"" />
  <item text=""312d1c21-0000-0000-0000-000000000002"" model=""[SelectableQFModel]"" order=""2"" />
  <item text=""312d1c21-0000-0000-0000-000000000003"" model=""[SelectableQFModel]"" order=""3"" />
  <item text=""312d1c21-0000-0000-0000-000000000004"" model=""[SelectableQFModel]"" order=""4"" />
</model>";

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting model has tracked."
            );

            actual = string.Join(Environment.NewLine, builder);
            actual.ToClipboardExpected();
            { }
            expected = @" 
NetProjection.Reset   NotifyCollectionChangedEventArgs           
NetProjection.Add     NewItems= 5 NewStartingIndex= 0 NotifyCollectionChangedEventArgs           ";

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting model has emitted discrete events."
            );

            actual = onp.ToString(FormattingOMC.StateReport);
            actual.ToClipboardExpected();
            { }
            expected = @" 
[IME Len: 0, IsFiltering: False], [Net: 5, CC: 5, PMC: 0], [QueryAndFilter: SearchEntryState.Cleared, FilteringState.Ineligible]";

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting quiescent initial state."
            );
        }
        #endregion S U B T E S T S
    }

    #region L o c a l C l a s s e s
    private class ObservableCollectionWithInternalMMDC : ObservableRangeCollection<SelectableQFModel>
    {
        public ObservableCollectionWithInternalMMDC()
        {
            MMDC = new();
            MMDC.SetObservableNetProjection(this);
        }
        private MMDC MMDC { get; }
    }

    /// <summary>
    /// Exposes FilteringState as public for test.
    /// </summary>
    private class MMDC : ModeledMarkdownContext<SelectableQFModel>
    {
        public new FilteringState FilteringState
        {
            get => FilteringState;
            set
            {
                FilteringState = value;
            }
        }
    }
    #endregion L o c a l C l a s s e s
}
