using IVSoftware.Portable.Collections.Common;
using IVSoftware.Portable.Collections.Modeled;
using IVSoftware.Portable.Collections.Preview;
using IVSoftware.Portable.Common.Attributes;
using IVSoftware.Portable.SQLiteMarkdown.Common;
using IVSoftware.WinOS.MSTest.Extensions;
using Newtonsoft.Json;
using System.Xml.Linq;

namespace IVSoftware.Portable.SQLiteMarkdown.MSTest;

[TestClass]
public class TestClass_260401_OPCv2
{
    [TestMethod, DoNotParallelize]
    [Claim("00000000-0000-0000-0000-000000000000")]
    public void Test_ModeledObservableCollection()
    {
        string actual, expected;
        var builder = new List<string>();
        using var te = this.TestableEpoch();

        #region I T E M    G E N
        IList<SelectableQFModel>? eph = null;
        // CREATE (no side effects)
        var i1 = eph.AddDynamic("Item01");
        var i2 = eph.AddDynamic("Item02");
        var i3 = eph.AddDynamic("Item03");
        #endregion I T E M    G E N

        var itemsSource = new ModeledObservableCollection<SelectableQFModel>();

        #region E V E N T S
        itemsSource.CollectionChanged += (sender, e) =>
        {
            builder.Add(e.ToString(ReferenceEquals(sender, itemsSource)));
        };
        #endregion E V E N T S

        subtest_None();
        subtest_Freeze();
        subtest_Preview();

        #region S U B T E S T S

        void subtest_None()
        {
            itemsSource.Add(i1);
            itemsSource.Add(i2);
            itemsSource.Add(i3);

            actual = string.Join(Environment.NewLine, builder);
            actual.ToClipboardExpected();
            { }
            expected = @" 
NetProjection.Add     NewItems= 1 NewStartingIndex= 0 NotifyCollectionChangedEventArgs           
NetProjection.Add     NewItems= 1 NewStartingIndex= 1 NotifyCollectionChangedEventArgs           
NetProjection.Add     NewItems= 1 NewStartingIndex= 2 NotifyCollectionChangedEventArgs           ";

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting 3x Add events."
            );

            builder.Clear();
            itemsSource.RemoveAt(2);

            actual = string.Join(Environment.NewLine, builder);
            actual.ToClipboardExpected();
            { }
            expected = @" 
NetProjection.Remove  OldItems= 1 OldStartingIndex= 2 NotifyCollectionChangedEventArgs           "
            ;

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting 1x Remove events."
            );

            builder.Clear();
            itemsSource[1] = i3;

            actual = string.Join(Environment.NewLine, builder);
            actual.ToClipboardExpected();
            { }
            expected = @" 
NetProjection.Replace NewItems= 1 OldItems= 1 NewStartingIndex= 1 OldStartingIndex= 1 NotifyCollectionChangedEventArgs           "
            ;

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting 1x Replace events."
            );

            builder.Clear();
            itemsSource.Move(1, 0);

            actual = string.Join(Environment.NewLine, builder);
            actual.ToClipboardExpected();
            { }
            expected = @" 
NetProjection.Move    NewItems= 1 OldItems= 1 NewStartingIndex= 0 OldStartingIndex= 1 NotifyCollectionChangedEventArgs           "
            ;

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting 1x Move events."
            );

            actual = JsonConvert.SerializeObject(itemsSource, Formatting.Indented);
            actual.ToClipboardExpected();
            { }
            expected = @" 
[
  {
    ""Id"": ""312d1c21-0000-0000-0000-000000000002"",
    ""Description"": ""Item03"",
    ""Keywords"": ""[]"",
    ""KeywordsDisplay"": """",
    ""Tags"": ""[]"",
    ""IsChecked"": false,
    ""Selection"": 0,
    ""IsEditing"": false,
    ""PrimaryKey"": ""312d1c21-0000-0000-0000-000000000002"",
    ""QueryTerm"": ""item03"",
    ""FilterTerm"": ""item03"",
    ""TagMatchTerm"": """",
    ""Properties"": ""{\r\n  \""Description\"": \""Item03\"",\r\n  \""Tags\"": \""[]\""\r\n}""
  },
  {
    ""Id"": ""312d1c21-0000-0000-0000-000000000000"",
    ""Description"": ""Item01"",
    ""Keywords"": ""[]"",
    ""KeywordsDisplay"": """",
    ""Tags"": ""[]"",
    ""IsChecked"": false,
    ""Selection"": 0,
    ""IsEditing"": false,
    ""PrimaryKey"": ""312d1c21-0000-0000-0000-000000000000"",
    ""QueryTerm"": ""item01"",
    ""FilterTerm"": ""item01"",
    ""TagMatchTerm"": """",
    ""Properties"": ""{\r\n  \""Description\"": \""Item01\"",\r\n  \""Tags\"": \""[]\""\r\n}""
  }
]";

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting list reflects all changes."
            );


            builder.Clear();
            itemsSource.Clear();

            actual = string.Join(Environment.NewLine, builder);
            actual.ToClipboardExpected();
            { }
            expected = @" 
NetProjection.Reset   NotifyCollectionChangedEventArgs           "
            ;

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting 1x Reset events."
            );
        }

        void subtest_Freeze()
        {
            te.ResetEpoch();
            itemsSource.PopulateForDemo(5);

            actual = itemsSource.ToString(out XElement _);
            actual.ToClipboardExpected();
            { }
            expected = @" 
<model modeling=""Id"">
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
                "Expecting result to match."
            );

            using (itemsSource.RequestModelEpochAuthority(ModelDataExchangeAuthority.CollectionDeferred, itemsSource))
            {
                itemsSource.RemoveAt(1);                // Remove Item02
                Assert.AreEqual(5, itemsSource.Count);
                itemsSource.RemoveAt(2);                // Remove Item04
                Assert.AreEqual(5, itemsSource.Count);
                itemsSource.RemoveAt(1);                // Remove Item03
                Assert.AreEqual(5, itemsSource.Count);
            }

            actual = itemsSource.ToString(out XElement _);
            actual.ToClipboardExpected();
            { }
            expected = @" 
<model modeling=""Id"">
  <xitem text=""312d1c21-0000-0000-0000-000000000000"" model=""[SelectableQFModel]"" order=""0"" preview=""Item01    "" />
  <xitem text=""312d1c21-0000-0000-0000-000000000004"" model=""[SelectableQFModel]"" order=""1"" preview=""Item05    "" />
</model>"
            ;

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting result to match."
            );
            te.ResetEpoch();
            itemsSource.PopulateForDemo(5);

            using (itemsSource.RequestModelEpochAuthority(ModelDataExchangeAuthority.CollectionDeferred, itemsSource))
            {
                itemsSource.RemoveAt(1);
                Assert.AreEqual(5, itemsSource.Count);
                itemsSource.RemoveAt(2);
                Assert.AreEqual(5, itemsSource.Count);
            }

            actual = itemsSource.ToString(out XElement _);
            actual.ToClipboardExpected();
            { }
            expected = @" 
<model modeling=""Id"">
  <xitem text=""312d1c21-0000-0000-0000-000000000000"" model=""[SelectableQFModel]"" order=""0"" preview=""Item01    "" />
  <xitem text=""312d1c21-0000-0000-0000-000000000002"" model=""[SelectableQFModel]"" order=""1"" preview=""Item03    "" />
  <xitem text=""312d1c21-0000-0000-0000-000000000004"" model=""[SelectableQFModel]"" order=""2"" preview=""Item05    "" />
</model>"
            ;

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting result to match."
            );

            te.ResetEpoch();
            itemsSource.PopulateForDemo(5);

            int liveCount = itemsSource.Count;
            using (itemsSource.RequestModelEpochAuthority(ModelDataExchangeAuthority.CollectionDeferred, itemsSource))
            {
                itemsSource.RemoveAt(1);                        // Remove Item02 (middle)
                liveCount--;
                Assert.AreEqual(5, itemsSource.Count);
                itemsSource.RemoveAt(0);                        // Remove Item01 (front)
                liveCount--;
                Assert.AreEqual(5, itemsSource.Count);
                itemsSource.RemoveAt(liveCount - 1);            // Remove Item05 (tail)
                Assert.AreEqual(5, itemsSource.Count);
            }

            actual = itemsSource.ToString(out XElement _);
            actual.ToClipboardExpected();
            { }
            expected = @" 
<model modeling=""Id"">
  <xitem text=""312d1c21-0000-0000-0000-000000000002"" model=""[SelectableQFModel]"" order=""0"" preview=""Item03    "" />
  <xitem text=""312d1c21-0000-0000-0000-000000000003"" model=""[SelectableQFModel]"" order=""1"" preview=""Item04    "" />
</model>"
            ;

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting result to match."
            );
        }

        void subtest_Preview()
        {            
            builder.Clear();
            itemsSource.Clear();

            actual = string.Join(Environment.NewLine, builder); builder.Clear();
            actual.ToClipboardAssert("Expecting builder content to match.");
            { }
            expected = @" 
NetProjection.Reset   NotifyCollectionChangedEventArgs           ";

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting builder content to match."
            );

            // P R E V I E W
            using (itemsSource.RequestModelEpochAuthority(ModelDataExchangeAuthority.CollectionDeferred, itemsSource))
            {
                itemsSource.Add(i1);
                itemsSource.Add(i2);
                itemsSource.Add(i3);
            }

            actual = string.Join(Environment.NewLine, builder); builder.Clear();
            actual.ToClipboardExpected();
            { }
            expected = @" 
NetProjection.Add     NewItems= 3 NewStartingIndex= 0 NotifyCollectionChangedEventArgs           "
            ;

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting 1x Add Coalesce events."
            );

            // - This *looks* contiguous but it isn't.
            // ∴We should get a Reset not a BCL-compatible event
            using (itemsSource.RequestModelEpochAuthority(ModelDataExchangeAuthority.CollectionDeferred, itemsSource))
            {
                itemsSource.Remove(i1);         // Remove Item01 from index 0      
                itemsSource.RemoveAt(1);        // Remove item03 from index 1
            }

            actual = string.Join(Environment.NewLine, builder); builder.Clear();
            actual.ToClipboardExpected();
            { }
            expected = @" 
NetProjection.Reset   NotifyCollectionChangedEventArgs           "
            ;

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting 1x Remove events."
            );

            // It might not look like it, but Item02 
            // is the (only) one that should remain
            Assert.AreSame(itemsSource[0], i2);

            using (itemsSource.RequestModelEpochAuthority(ModelDataExchangeAuthority.CollectionDeferred, itemsSource))
            {
                itemsSource.PopulateForDemo(5);
            }

            actual = string.Join(Environment.NewLine, builder); builder.Clear();
            actual.ToClipboardExpected();
            { }
            expected = @" 
NetProjection.Reset   NotifyCollectionChangedEventArgs           ";

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting 1x jagged Reset."
            );

            using (itemsSource.RequestModelEpochAuthority(ModelDataExchangeAuthority.CollectionDeferred, itemsSource))
            {
                // Replace index 1-4 with with Item01 (contiguous)
                for (int i = 1; i < itemsSource.Count; i++)
                {
                    itemsSource[i] = i1;
                }
            }

            actual = string.Join(Environment.NewLine, builder); builder.Clear();
            actual.ToClipboardExpected();
            { }
            expected = @" 
NetProjection.Replace NewItems= 4 OldItems= 4 NewStartingIndex= 0 OldStartingIndex= 0 NotifyCollectionChangedEventArgs           ";

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting 1x contiguous Replace."
            );

            // P R E V I E W
            using (itemsSource.RequestModelEpochAuthority(ModelDataExchangeAuthority.CollectionDeferred, itemsSource))
            {
                itemsSource.Clear();
                Assert.AreEqual(5, itemsSource.Count);  // Remember! We're projecting a different reality.
                itemsSource.Add(i1);
                itemsSource.Add(i2);
                itemsSource.Add(i3);
                Assert.AreEqual(5, itemsSource.Count);
            }
            Assert.AreEqual(3, itemsSource.Count);      // Now count is back to IRL.

            actual = string.Join(Environment.NewLine, builder); builder.Clear();
            actual.ToClipboardExpected();
            { }
            expected = @" 
NetProjection.Reset   NotifyCollectionChangedEventArgs           ";

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting 1x jagged reset."
            );

            using (itemsSource.RequestModelEpochAuthority(ModelDataExchangeAuthority.CollectionDeferred, itemsSource))
            {
                itemsSource.PopulateForDemo(5);
            }

            actual = string.Join(Environment.NewLine, builder); builder.Clear();
            actual.ToClipboardExpected();
            { }
            expected = @" 
NetProjection.Reset   NotifyCollectionChangedEventArgs           ";

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting 1x jagged Reset."
            );

            using (itemsSource.RequestModelEpochAuthority(ModelDataExchangeAuthority.CollectionDeferred, itemsSource))
            {
                // C O N T I G U O U S !
                // - Move is *not* a qualifying ranged operation.
                // - However, the net result affects contiguous indexes.
                // ∴ Produces contiguous Replace.
                for (int srce=1, dest=0; srce < itemsSource.Count; srce++, dest++)
                {
                    itemsSource.Move(srce, dest);
                }
            }

            actual = itemsSource.ToString(out XElement _);
            actual.ToClipboardExpected();
            { }
            expected = @" 
<model modeling=""Id"">
  <xitem text=""312d1c21-0000-0000-0000-00000000000b"" model=""[SelectableQFModel]"" order=""0"" preview=""Item02    "" />
  <xitem text=""312d1c21-0000-0000-0000-00000000000c"" model=""[SelectableQFModel]"" order=""1"" preview=""Item03    "" />
  <xitem text=""312d1c21-0000-0000-0000-00000000000d"" model=""[SelectableQFModel]"" order=""2"" preview=""Item04    "" />
  <xitem text=""312d1c21-0000-0000-0000-00000000000e"" model=""[SelectableQFModel]"" order=""3"" preview=""Item05    "" />
  <xitem text=""312d1c21-0000-0000-0000-00000000000a"" model=""[SelectableQFModel]"" order=""4"" preview=""Item01    "" />
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
NetProjection.Replace NewItems= 5 OldItems= 5 NewStartingIndex= 0 OldStartingIndex= 0 NotifyCollectionChangedEventArgs           ";

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting 1x contiguous replace."
            );
        }
        #endregion S U B T E S T S
    }
}
