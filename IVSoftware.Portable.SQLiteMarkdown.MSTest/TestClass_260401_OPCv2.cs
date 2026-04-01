using IVSoftware.Portable.Collections.Preview;
using IVSoftware.Portable.Common.Attributes;
using IVSoftware.Portable.Common.Exceptions;
using IVSoftware.Portable.SQLiteMarkdown.Collections.Preview;
using IVSoftware.Portable.SQLiteMarkdown.Common;
using IVSoftware.Portable.SQLiteMarkdown.Internal;
using IVSoftware.Portable.SQLiteMarkdown.Util;
using IVSoftware.Portable.Xml.Linq.XBoundObject;
using IVSoftware.WinOS.MSTest.Extensions;
using Newtonsoft.Json;
using System.Collections.ObjectModel;

namespace IVSoftware.Portable.SQLiteMarkdown.MSTest;

[TestClass]
public class TestClass_260401_OPCv2
{
    [TestMethod, DoNotParallelize]
    [Claim("00000000-0000-0000-0000-000000000000")]
    public void Test_Suppressible()
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

        var itemsSource = new SuppressibleObservableCollection<SelectableQFModel>();

        #region E V E N T S
        itemsSource.CollectionChanged += (sender, e) =>
        {
            builder.Add(e.ToString(ReferenceEquals(sender, itemsSource)));
        };
        #endregion E V E N T S

        subtest_None();
        subtest_Freeze();

        // subtest_Preview();

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
            itemsSource.PopulateForDemo(5);

            actual = itemsSource.ToString(this.GetModelPreviewDlgt<SelectableQFModel>());
            actual.ToClipboardExpected();
            { }
            expected = @" 
<model>
  <xitem text=""312d1c21-0000-0000-0000-000000000003"" model=""[SelectableQFModel]"" order=""0"" preview=""Item01    "" />
  <xitem text=""312d1c21-0000-0000-0000-000000000004"" model=""[SelectableQFModel]"" order=""1"" preview=""Item02    "" />
  <xitem text=""312d1c21-0000-0000-0000-000000000005"" model=""[SelectableQFModel]"" order=""2"" preview=""Item03    "" />
  <xitem text=""312d1c21-0000-0000-0000-000000000006"" model=""[SelectableQFModel]"" order=""3"" preview=""Item04    "" />
  <xitem text=""312d1c21-0000-0000-0000-000000000007"" model=""[SelectableQFModel]"" order=""4"" preview=""Item05    "" />
</model>"
            ;

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting result to match."
            );

            using (itemsSource.BeginSuppress())
            {
                itemsSource.RemoveAt(1);
                Assert.AreEqual(5, itemsSource.Count);
                itemsSource.RemoveAt(2);
                Assert.AreEqual(5, itemsSource.Count);
                itemsSource.RemoveAt(1);
                Assert.AreEqual(5, itemsSource.Count);
            }

            actual = itemsSource.ToString(this.GetModelPreviewDlgt<SelectableQFModel>());
            actual.ToClipboardExpected();
            { }
            expected = @" 
<model>
  <xitem text=""312d1c21-0000-0000-0000-000000000003"" model=""[SelectableQFModel]"" order=""0"" preview=""Item01    "" />
  <xitem text=""312d1c21-0000-0000-0000-000000000007"" model=""[SelectableQFModel]"" order=""1"" preview=""Item05    "" />
</model>"
            ;

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting result to match."
            );

            itemsSource.PopulateForDemo(5);

            using (itemsSource.BeginSuppress())
            {
                itemsSource.RemoveAt(1);
                Assert.AreEqual(5, itemsSource.Count);
                itemsSource.RemoveAt(2);
                Assert.AreEqual(5, itemsSource.Count);
            }

            actual = itemsSource.ToString(this.GetModelPreviewDlgt<SelectableQFModel>());
            actual.ToClipboardExpected();
            { }
            expected = @" 
<model>
  <xitem text=""312d1c21-0000-0000-0000-000000000008"" model=""[SelectableQFModel]"" order=""0"" preview=""Item01    "" />
  <xitem text=""312d1c21-0000-0000-0000-00000000000a"" model=""[SelectableQFModel]"" order=""1"" preview=""Item03    "" />
  <xitem text=""312d1c21-0000-0000-0000-00000000000c"" model=""[SelectableQFModel]"" order=""2"" preview=""Item05    "" />
</model>"
            ;

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting result to match."
            );


            itemsSource.PopulateForDemo(5);

            int liveCount = itemsSource.Count;
            using (itemsSource.BeginSuppress())
            {
                itemsSource.RemoveAt(1);                        // middle
                liveCount--;
                Assert.AreEqual(5, itemsSource.Count);
                itemsSource.RemoveAt(0);                        // front
                liveCount--;
                Assert.AreEqual(5, itemsSource.Count);
                itemsSource.RemoveAt(liveCount - 1);              // tail
                Assert.AreEqual(5, itemsSource.Count);
            }

            actual = itemsSource.ToString(this.GetModelPreviewDlgt<SelectableQFModel>());
            actual.ToClipboardExpected();
            { }
            expected = @" 
<model>
  <xitem text=""312d1c21-0000-0000-0000-00000000000f"" model=""[SelectableQFModel]"" order=""0"" preview=""Item03    "" />
  <xitem text=""312d1c21-0000-0000-0000-000000000010"" model=""[SelectableQFModel]"" order=""1"" preview=""Item04    "" />
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

            // P R E V I E W
            using (itemsSource.BeginSuppress())
            {
                itemsSource.Add(i1);
                itemsSource.Add(i2);
                itemsSource.Add(i3);
            }

            actual = string.Join(Environment.NewLine, builder);
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


            builder.Clear();

            using (itemsSource.BeginSuppress())
            {
                itemsSource.Remove(i1);
                itemsSource.RemoveAt(1);
            }

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
        }
        #endregion S U B T E S T S
    }
}
