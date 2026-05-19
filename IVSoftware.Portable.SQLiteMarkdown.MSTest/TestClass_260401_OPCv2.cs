using IVSoftware.Portable.Collections.Preview;
using IVSoftware.Portable.Common.Attributes;
using IVSoftware.Portable.SQLiteMarkdown.Common;
using IVSoftware.Portable.Collections;
using IVSoftware.WinOS.MSTest.Extensions;
using Newtonsoft.Json;
using System.Xml.Linq;
using System.Diagnostics;
using IVSoftware.Portable.Disposable;

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
        // CREATE (no side effects)
        var i1 = "Item01".MakeDynamic<SelectableQFModel>();
        var i2 = "Item02".MakeDynamic<SelectableQFModel>();
        var i3 = "Item03".MakeDynamic<SelectableQFModel>();
        #endregion I T E M    G E N

        var omc = new ObservableModeledCollection<SelectableQFModel>();

        #region E V E N T S
        omc.CollectionChanged += (sender, e) =>
        {
            builder.Add(e.ToStringEx());
        };
        #endregion E V E N T S

        subtest_None();
        subtest_Freeze();
        subtest_Preview();


        #region S U B T E S T S

        void subtest_None()
        {
            omc.Add(i1);
            omc.Add(i2);
            omc.Add(i3);

            actual = string.Join(Environment.NewLine, builder);
            actual.ToClipboardExpected();
            { }
            expected = @" 
Add     NewItems=1 OldItems=* NewStartingIndex= 0 OldStartingIndex=-1 NotifyCollectionChangedEventArgs 
Add     NewItems=1 OldItems=* NewStartingIndex= 1 OldStartingIndex=-1 NotifyCollectionChangedEventArgs 
Add     NewItems=1 OldItems=* NewStartingIndex= 2 OldStartingIndex=-1 NotifyCollectionChangedEventArgs "
            ;
            ;

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting 3x Add events."
            );

            actual = omc.ToString(FormattingOMC.ModelWithPreview);
            actual.ToClipboardExpected();
            { }
            expected = @" 
<model omc=""[OMC]"" histo=""[model:3 match:0 qmatch:0 pmatch:0 live:0]"">
  <item text=""312d1c21-0000-0000-0000-000000000000"" model=""[SelectableQFModel]"" preview=""Item01    "" index=""0"" />
  <item text=""312d1c21-0000-0000-0000-000000000001"" model=""[SelectableQFModel]"" preview=""Item02    "" index=""1"" />
  <item text=""312d1c21-0000-0000-0000-000000000002"" model=""[SelectableQFModel]"" preview=""Item03    "" index=""2"" />
</model>"
            ;

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting THREE items."
            );

            builder.Clear();
            omc.RemoveAt(2);

            actual = omc.ToString(FormattingOMC.ModelWithPreview);
            actual.ToClipboardExpected();
            { }
            expected = @" 
<model omc=""[OMC]"" histo=""[model:2 match:0 qmatch:0 pmatch:0 live:0]"">
  <item text=""312d1c21-0000-0000-0000-000000000000"" model=""[SelectableQFModel]"" preview=""Item01    "" index=""0"" />
  <item text=""312d1c21-0000-0000-0000-000000000001"" model=""[SelectableQFModel]"" preview=""Item02    "" index=""1"" />
</model>"
            ;

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting LAST item REMOVED."
            );

            actual = string.Join(Environment.NewLine, builder);
            actual.ToClipboardExpected();
            { }
            expected = @" 
Remove  NewItems=* OldItems=1 NewStartingIndex=-1 OldStartingIndex= 2 NotifyCollectionChangedEventArgs "
            ;

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting 1x Remove events."
            );

            actual = omc.ToString(FormattingOMC.ModelWithPreview);
            actual.ToClipboardExpected();
            { }
            expected = @" 
<model omc=""[OMC]"" histo=""[model:2 match:0 qmatch:0 pmatch:0 live:0]"">
  <item text=""312d1c21-0000-0000-0000-000000000000"" model=""[SelectableQFModel]"" preview=""Item01    "" index=""0"" />
  <item text=""312d1c21-0000-0000-0000-000000000001"" model=""[SelectableQFModel]"" preview=""Item02    "" index=""1"" />
</model>"
            ;

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting LAST item REMOVED."
            );

            builder.Clear();
            omc[1] = i3;

            actual = string.Join(Environment.NewLine, builder);
            actual.ToClipboardExpected();
            { }
            expected = @" 
Replace NewItems=1 OldItems=1 NewStartingIndex= 1 OldStartingIndex= 1 NotifyCollectionChangedEventArgs "
            ;

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting 1x Replace events."
            );

            actual = omc.ToString(FormattingOMC.ModelWithPreview);
            actual.ToClipboardExpected();
            { }
            expected = @" 
<model omc=""[OMC]"" histo=""[model:2 match:0 qmatch:0 pmatch:0 live:0]"">
  <item text=""312d1c21-0000-0000-0000-000000000000"" model=""[SelectableQFModel]"" preview=""Item01    "" index=""0"" />
  <item text=""312d1c21-0000-0000-0000-000000000002"" model=""[SelectableQFModel]"" preview=""Item03    "" index=""1"" />
</model>"
            ;
            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting result to match."
            );

            builder.Clear();
            omc.Move(1, 0);

            actual = string.Join(Environment.NewLine, builder);
            actual.ToClipboardExpected();
            { }
            expected = @" 
Move    NewItems=1 OldItems=1 NewStartingIndex= 0 OldStartingIndex= 1 NotifyCollectionChangedEventArgs "
            ;

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting 1x Move events."
            );

            actual = omc.ToString(FormattingOMC.ModelWithPreview);
            actual.ToClipboardExpected();
            { }
            expected = @" 
<model omc=""[OMC]"" histo=""[model:2 match:0 qmatch:0 pmatch:0 live:0]"">
  <item text=""312d1c21-0000-0000-0000-000000000002"" model=""[SelectableQFModel]"" preview=""Item03    "" index=""0"" />
  <item text=""312d1c21-0000-0000-0000-000000000000"" model=""[SelectableQFModel]"" preview=""Item01    "" index=""1"" />
</model>"
            ;
            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting result to match."
            );

            actual = JsonConvert.SerializeObject(omc, Formatting.Indented);
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
            omc.Clear();

            actual = string.Join(Environment.NewLine, builder);
            actual.ToClipboardExpected();
            { }
            expected = @" 
Reset   NewItems=* OldItems=* NewStartingIndex=-1 OldStartingIndex=-1 NotifyCollectionChangedEventArgs "
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
            omc.PopulateForDemo(5);

            actual = omc.ToString(out XElement _);
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
                "Expecting result to match."
            );

            using (omc.RequestAuthority(ModelDataExchangeAuthority.CollectionDeferred))
            {
                omc.RemoveAt(1);                // Remove Item02
                Assert.AreEqual(5, omc.Count);
                omc.RemoveAt(2);                // Remove Item04
                Assert.AreEqual(5, omc.Count);
                omc.RemoveAt(1);                // Remove Item03
                Assert.AreEqual(5, omc.Count);
            }

            actual = omc.ToString(out XElement _);
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
            te.ResetEpoch();
            omc.PopulateForDemo(5);

            using (omc.RequestAuthority(ModelDataExchangeAuthority.CollectionDeferred))
            {
                omc.RemoveAt(1);
                Assert.AreEqual(5, omc.Count);
                omc.RemoveAt(2);
                Assert.AreEqual(5, omc.Count);
            }

            actual = omc.ToString(out XElement _);
            actual.ToClipboardExpected();
            { }
            expected = @" 
<model mpath=""Id"">
  <item text=""312d1c21-0000-0000-0000-000000000000"" model=""[SelectableQFModel]"" preview=""Item01    "" index=""0"" />
  <item text=""312d1c21-0000-0000-0000-000000000002"" model=""[SelectableQFModel]"" preview=""Item03    "" index=""1"" />
  <item text=""312d1c21-0000-0000-0000-000000000004"" model=""[SelectableQFModel]"" preview=""Item05    "" index=""2"" />
</model>"
            ;

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting result to match."
            );

            te.ResetEpoch();
            omc.PopulateForDemo(5);

            int liveCount = omc.Count;
            using (omc.RequestAuthority(ModelDataExchangeAuthority.CollectionDeferred))
            {
                omc.RemoveAt(1);                        // Remove Item02 (middle)
                liveCount--;
                Assert.AreEqual(5, omc.Count);
                omc.RemoveAt(0);                        // Remove Item01 (front)
                liveCount--;
                Assert.AreEqual(5, omc.Count);
                omc.RemoveAt(liveCount - 1);            // Remove Item05 (tail)
                Assert.AreEqual(5, omc.Count);
            }

            actual = omc.ToString(out XElement _);
            actual.ToClipboardExpected();
            { }
            expected = @" 
<model mpath=""Id"">
  <item text=""312d1c21-0000-0000-0000-000000000002"" model=""[SelectableQFModel]"" preview=""Item03    "" index=""0"" />
  <item text=""312d1c21-0000-0000-0000-000000000003"" model=""[SelectableQFModel]"" preview=""Item04    "" index=""1"" />
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
            omc.Clear();
            localValidateClear();

            actual = string.Join(Environment.NewLine, builder); builder.Clear();
            actual.ToClipboardExpected();
            { }
            expected = @" 
Reset   NewItems=* OldItems=* NewStartingIndex=-1 OldStartingIndex=-1 NotifyCollectionChangedEventArgs ";

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting builder content to match."
            );

            // P R E V I E W
            using (omc.RequestAuthority(ModelDataExchangeAuthority.CollectionDeferred))
            {
                omc.Add(i1);
                omc.Add(i2);
                omc.Add(i3);
            }
            localValidateCount();

            actual = string.Join(Environment.NewLine, builder); builder.Clear();
            actual.ToClipboardExpected();
            { }
            expected = @" 
Add     NewItems=3 OldItems=* NewStartingIndex= 0 OldStartingIndex=-1 NotifyCollectionChangedEventArgs "
            ;

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting 1x Add Coalesce events."
            );

            actual = omc.ToString(FormattingOMC.ModelWithPreview);
            actual.ToClipboardExpected();
            { }
            expected = @" 
<model omc=""[OMC]"" histo=""[model:3 match:0 qmatch:0 pmatch:0 live:0]"">
  <item text=""312d1c21-0000-0000-0000-000000000000"" model=""[SelectableQFModel]"" preview=""Item01    "" index=""0"" />
  <item text=""312d1c21-0000-0000-0000-000000000001"" model=""[SelectableQFModel]"" preview=""Item02    "" index=""1"" />
  <item text=""312d1c21-0000-0000-0000-000000000002"" model=""[SelectableQFModel]"" preview=""Item03    "" index=""2"" />
</model>";

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting result to match."
            );

            // - This *looks* contiguous but it isn't.
            // ∴We should get a Reset not a BCL-compatible event
            using (omc.RequestAuthority(ModelDataExchangeAuthority.CollectionDeferred))
            {
                omc.Remove(i1);         // Remove Item01 from index 0      
                omc.RemoveAt(1);        // Remove item03 from index 1
            }
            localValidateCount();

            actual = omc.ToString(FormattingOMC.ModelWithPreview);
            actual.ToClipboardExpected();
            { }
            expected = @" 
<model omc=""[OMC]"" histo=""[model:1 match:0 qmatch:0 pmatch:0 live:0]"">
  <item text=""312d1c21-0000-0000-0000-000000000001"" model=""[SelectableQFModel]"" preview=""Item02    "" index=""1"" />
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
Reset   NewItems=* OldItems=* NewStartingIndex=-1 OldStartingIndex=-1 NotifyCollectionChangedEventArgs "
            ;

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting 1x Remove events."
            );

            // It might not look like it, but Item02 
            // is the (only) one that should remain
            Assert.AreSame(omc[0], i2);

            using (omc.RequestAuthority(ModelDataExchangeAuthority.CollectionDeferred))
            {
                omc.PopulateForDemo(5);
            }
            localValidateCount();

            actual = string.Join(Environment.NewLine, builder); builder.Clear();
            actual.ToClipboardExpected();
            { }
            expected = @" 
Reset   NewItems=* OldItems=* NewStartingIndex=-1 OldStartingIndex=-1 NotifyCollectionChangedEventArgs "
            ;

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting 1x jagged Reset."
            );

            // WHAT HAPPENS IN MODEL WITH IDENTICAL KEYS ???
            using (omc.RequestAuthority(ModelDataExchangeAuthority.CollectionDeferred))
            {
                for (int i = 1; i < omc.Count; i++)
                {
                    omc[i] = i1;
                }
            }
            localValidateCount();

            actual = omc.ToString(FormattingOMC.ModelWithPreview);
            actual.ToClipboardExpected();
            { }
            expected = @" 
<model omc=""[OMC]"" histo=""[model:5 match:0 qmatch:0 pmatch:0 live:0]"">
  <item text=""312d1c21-0000-0000-0000-000000000005"" model=""[SelectableQFModel]"" preview=""Item01    "" index=""0"" />
  <item text=""312d1c21-0000-0000-0000-000000000000"" model=""[SelectableQFModel]"" preview=""Item01    "" index=""1"" />
  <item text=""312d1c21-0000-0000-0000-000000000000"" model=""[SelectableQFModel]"" preview=""Item01    "" index=""2"" />
  <item text=""312d1c21-0000-0000-0000-000000000000"" model=""[SelectableQFModel]"" preview=""Item01    "" index=""3"" />
  <item text=""312d1c21-0000-0000-0000-000000000000"" model=""[SelectableQFModel]"" preview=""Item01    "" index=""4"" />
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
Replace NewItems=4 OldItems=4 NewStartingIndex= 0 OldStartingIndex= 0 NotifyCollectionChangedEventArgs "
            ;

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting 1x contiguous Replace."
            );

            // P R E V I E W
            using (omc.RequestAuthority(ModelDataExchangeAuthority.CollectionDeferred))
            {
                omc.Clear();
                Assert.AreEqual(5, omc.Count);  // Remember! We're projecting a different reality.
                omc.Add(i1);
                omc.Add(i2);
                omc.Add(i3);
                Assert.AreEqual(5, omc.Count);
            }
            Assert.AreEqual(3, omc.Count);      // Now count is back to IRL.

            actual = string.Join(Environment.NewLine, builder); builder.Clear();
            actual.ToClipboardExpected();
            { }
            expected = @" 
Reset   NewItems=* OldItems=* NewStartingIndex=-1 OldStartingIndex=-1 NotifyCollectionChangedEventArgs "
            ;

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting 1x jagged reset."
            );

            using (omc.RequestAuthority(ModelDataExchangeAuthority.CollectionDeferred))
            {
                omc.PopulateForDemo(5);
            }

            actual = string.Join(Environment.NewLine, builder); builder.Clear();
            actual.ToClipboardExpected();
            { }
            expected = @" 
Reset   NewItems=* OldItems=* NewStartingIndex=-1 OldStartingIndex=-1 NotifyCollectionChangedEventArgs "
            ;

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting 1x jagged Reset."
            );

            using (omc.RequestAuthority(ModelDataExchangeAuthority.CollectionDeferred))
            {
                // C O N T I G U O U S !
                // - Move is *not* a qualifying ranged operation.
                // - However, the net result affects contiguous indexes.
                // ∴ Produces contiguous Replace.
                for (int srce=1, dest=0; srce < omc.Count; srce++, dest++)
                {
                    omc.Move(srce, dest);
                }
            }

            actual = omc.ToString(out XElement _);
            actual.ToClipboardExpected();
            { }
            expected = @" 
<model mpath=""Id"">
  <item text=""312d1c21-0000-0000-0000-00000000000b"" model=""[SelectableQFModel]"" preview=""Item02    "" index=""0"" />
  <item text=""312d1c21-0000-0000-0000-00000000000c"" model=""[SelectableQFModel]"" preview=""Item03    "" index=""1"" />
  <item text=""312d1c21-0000-0000-0000-00000000000d"" model=""[SelectableQFModel]"" preview=""Item04    "" index=""2"" />
  <item text=""312d1c21-0000-0000-0000-00000000000e"" model=""[SelectableQFModel]"" preview=""Item05    "" index=""3"" />
  <item text=""312d1c21-0000-0000-0000-00000000000a"" model=""[SelectableQFModel]"" preview=""Item01    "" index=""4"" />
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
Replace NewItems=5 OldItems=5 NewStartingIndex= 0 OldStartingIndex= 0 NotifyCollectionChangedEventArgs "
            ;

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting 1x contiguous replace."
            );
        }
        #endregion S U B T E S T S

        #region L o c a l F x
        void localValidateClear()
        {
            Assert.HasCount(0, omc);

            actual = omc.ToString(FormattingOMC.ModelWithPreview);
            actual.ToClipboardExpected();
            { }
            expected = @" 
<model omc=""[OMC]"" histo=""[model:0 match:0 qmatch:0 pmatch:0 live:0]"" />";

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting canonical Clear profile."
            );
        }
        void localValidateCount()
        {
            var modelTally = omc.Histo[StdModelAttribute.model];
            Assert.AreEqual(omc.Count, modelTally);
        }
        #endregion L o c a l F x
    }
}
