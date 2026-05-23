using IVSoftware.Portable.Common.Attributes;
using IVSoftware.Portable.Disposable;
using IVSoftware.Portable.SQLiteMarkdown.Common;
using IVSoftware.Portable.Xml.Linq;
using IVSoftware.Portable.Collections;
using IVSoftware.Portable.Collections.Events;
using IVSoftware.Portable.Collections.Internal;
using IVSoftware.Portable.Xml.Linq.XBoundObject;
using IVSoftware.WinOS.MSTest.Extensions;
using System.Xml.Linq;
using IVSoftware.Portable.SQLiteMarkdown.Collections;
using System.Collections.Specialized;

namespace IVSoftware.Portable.SQLiteMarkdown.MSTest;

[TestClass]
public class TestClass_260328_Model
{
    /// <summary>
    /// Verifies histogram updates from direct XModel mutations.
    /// </summary>
    /// <remarks>
    /// Covers add, hold, decrement, onboarding, offloading, and bound model
    /// tracking on the XElement surface.
    /// </remarks>
    [TestMethod, Canonical("XElement change handling.")]
    public void Test_Histogrammer()
    {
        string actual, expected;
        var builder = new List<string>();

        var omc = new ObservableModeledCollection<SelectableQFModel>();
        var model = omc.Model;
        var histo = model.To<EnumHistogrammer<StdModelAttribute>>();
        histo.AllowRootChanges = true;

        #region L o c a l F x				
        using var local = this.WithOnDispose(
            onInit: (sender, e) =>
            {
                histo.XModelChanged += localOnXModelChanged;
            },
            onDispose: (sender, e) =>
            {
                histo.XModelChanged -= localOnXModelChanged;
            });
        void localOnXModelChanged(object? sender, XModelChangeEventArgs e)
        {
            if (!e.Changing)
            {
                builder.Add(e.ToString());
            }
        }
        #endregion L o c a l F x

        subtest_TrackCurrentChild();
        subtest_TrackAddRemoveChild();

        #region S U B T E S T S

        void subtest_TrackCurrentChild()
        {
            var xel = new XElement(nameof(StdModelElement.item));

            // Add Xel
            model.Add(xel);


            actual = string.Join(Environment.NewLine, builder); builder.Clear();
            actual.ToClipboardExpected();
            { }
            expected = @" 
[Changed] Key=XElementHasNoKey ObjectChange=Add Parent=not null Edge=Hold";

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting CHANGE for XElement Add."
            );

            // Add Xattr
            xel.SetStdAttributeValue(StdModelAttribute.qmatch, true);

            actual = string.Join(Environment.NewLine, builder); builder.Clear();
            actual.ToClipboardExpected();
            { }
            expected = @" 
[Changed] Key=qmatch ObjectChange=Add Parent=not null Edge=Increment";

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting Add INCREMENT +> SINK."
            );

            actual = histo.ToString();
            actual.ToClipboardExpected();
            { }
            expected = @" 
[model:0 qmatch:1 pmatch:0 live:0]"
            ;

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting histogram to match."
            );

            actual = model.ToString();
            actual.ToClipboardExpected();
            { }
            expected = @" 
<model omc=""[OMC]"" histo=""[model:0 qmatch:1 pmatch:0 live:0]"">
  <item qmatch=""True"" />
</model>"
            ;

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting result to match."
            );

            // CONFIRMED:
            // - Setting to same value *does* raise raw XObject.Change events.
            // - However these are intercepted prior to OnXAttributeChanged.
            xel.SetStdAttributeValue(StdModelAttribute.qmatch, true);

            actual = string.Join(Environment.NewLine, builder); builder.Clear();
            actual.ToClipboardExpected();
            { }
            expected = @" 
[Changed] Key=qmatch ObjectChange=Value Parent=not null Edge=Hold";

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting Add + VALUE + HOLD."
            );

            actual = histo.ToString();
            actual.ToClipboardExpected();
            { }
            expected = @" 
[model:0 qmatch:1 pmatch:0 live:0]"
            ;

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting NO CHANGE."
            );

            // Remove from Model
            model.RemoveDescendantAttributes(StdModelAttribute.qmatch);
            actual = histo.ToString();
            actual.ToClipboardExpected();
            { }
            expected = @" 
[model:0 qmatch:0 pmatch:0 live:0]"
            ;

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting empty histogram."
            );

            actual = model.ToString();
            actual.ToClipboardExpected();
            { }
            expected = @" 
<model omc=""[OMC]"" histo=""[model:0 qmatch:0 pmatch:0 live:0]"">
  <item />
</model>"
            ;

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting implicit false attributes are now removed."
            );
        }

        void subtest_TrackAddRemoveChild()
        {
            actual = model.ToString();
            actual.ToClipboardExpected();
            { }
            expected = @" 
<model omc=""[OMC]"" histo=""[model:0 qmatch:0 pmatch:0 live:0]"">
  <item />
</model>"
            ;

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting carryover."
            );

            // Add 'qmatch' offline - before this node is parented.
            var xel = new XElement(
                nameof(StdModelElement.item),
                new XAttribute(nameof(StdModelAttribute.qmatch), bool.TrueString));

            // Onboard
            model.Add(xel);

            actual = model.ToString();
            actual.ToClipboardExpected();
            { }
            expected = @" 
<model omc=""[OMC]"" histo=""[model:0 qmatch:1 pmatch:0 live:0]"">
  <item />
  <item qmatch=""True"" />
</model>"
            ;

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting successful onboarding."
            );

            // Remove
            xel.Remove();
            actual = model.ToString();
            actual.ToClipboardExpected();
            { }
            expected = @" 
<model omc=""[OMC]"" histo=""[model:0 qmatch:0 pmatch:0 live:0]"">
  <item />
</model>"
            ;

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting successful offloading + empty histogram."
            );

            // Add it back in again
            model.Add(xel);

            actual = model.ToString();
            actual.ToClipboardExpected();
            { }
            expected = @" 
<model omc=""[OMC]"" histo=""[model:0 qmatch:1 pmatch:0 live:0]"">
  <item />
  <item qmatch=""True"" />
</model>"
            ;

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting successful onboarding again; QMATCH +> MATCH."
            );

            // Now test EXPLICIT FALSE
            builder.Clear();
            xel.SetStdAttributeValue(StdModelAttribute.qmatch, false);

            actual = string.Join(Environment.NewLine, builder); builder.Clear();
            actual.ToClipboardExpected();
            { }
            expected = @"
[Changed] Key=qmatch ObjectChange=Value Parent=not null Edge=Decrement";

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting DECREMENT EDGE."
            );

            actual = histo.ToString();
            actual.ToClipboardExpected();
            { }
            expected = @" 
[model:0 qmatch:0 pmatch:0 live:0]"
            ;
            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting empty histogram."
            );
        }

        subtest_TrackModel();
        void subtest_TrackModel()
        {
            #region I T E M    G E N
            IList<SelectableQFModel>? eph = null;
            // CREATE (no side effects)
            var i1 = eph.AddDynamic("Item01");
            var i2 = eph.AddDynamic("Item02");
            var i3 = eph.AddDynamic("Item03");
            #endregion I T E M    G E N

            var xel = new XElement(
                nameof(StdModelElement.item),
                new XBoundAttribute(nameof(StdModelAttribute.model), i1),
                new XAttribute(nameof(StdModelAttribute.qmatch), true));

            model.Add(xel);

            actual = histo.ToString();
            actual.ToClipboardExpected();
            { }
            expected = @" 
[model:1 qmatch:1 pmatch:0 live:0]"
            ;

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting histogram to match."
            );
        }
        #endregion S U B T E S T S
    }

    [TestMethod, DoNotParallelize]
    public void Test_HistogrammerOMC()
    {
        string actual, expected;
        using var te = this.TestableEpoch();
        List<string> 
            builderChanging = new (),
            builderChanged  = new ();
        ObservableQueryFilterSource<SelectableQFModel> oqfs = new(){ QueryFilterConfig = QueryFilterConfig.Query };
        XElement model = oqfs.Model;

        #region L o c a l F x				
        using var local = this.WithOnDispose(
            onInit: (sender, e) =>
            {
                oqfs.CollectionChanging += localOnCollectionChanging;
                oqfs.CollectionChanged += localOnCollectionChanged;
            },
            onDispose: (sender, e) =>
            {
                oqfs.CollectionChanged -= localOnCollectionChanged;
            });

        void localOnCollectionChanging(object? sender, NotifyCollectionChangingEventArgs e)
        {
            builderChanging.Add(e.ToStringEx());
        }
        void localOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
        }
        #endregion L o c a l F x

        oqfs.ReplaceItems(new List<SelectableQFModel>().PopulateForDemo(10));

        actual = model.ToString();
        actual.ToClipboardExpected();
        { }
        expected = @" 
<model mdc=""[MMDC]"" histo=""10"" count=""10"" matches=""10"">
  <item text=""312d1c21-0000-0000-0000-000000000000"" model=""[SelectableQFModel]"" sort=""0"" />
  <item text=""312d1c21-0000-0000-0000-000000000001"" model=""[SelectableQFModel]"" sort=""1"" />
  <item text=""312d1c21-0000-0000-0000-000000000002"" model=""[SelectableQFModel]"" sort=""2"" />
  <item text=""312d1c21-0000-0000-0000-000000000003"" model=""[SelectableQFModel]"" sort=""3"" />
  <item text=""312d1c21-0000-0000-0000-000000000004"" model=""[SelectableQFModel]"" sort=""4"" />
  <item text=""312d1c21-0000-0000-0000-000000000005"" model=""[SelectableQFModel]"" sort=""5"" />
  <item text=""312d1c21-0000-0000-0000-000000000006"" model=""[SelectableQFModel]"" sort=""6"" />
  <item text=""312d1c21-0000-0000-0000-000000000007"" model=""[SelectableQFModel]"" sort=""7"" />
  <item text=""312d1c21-0000-0000-0000-000000000008"" model=""[SelectableQFModel]"" sort=""8"" />
  <item text=""312d1c21-0000-0000-0000-000000000009"" model=""[SelectableQFModel]"" sort=""9"" />
</model>"
        ;

        subtest_ToStringHistoDefault();

        #region S U B T E S T S
        void subtest_ToStringHistoDefault()
        {
            actual = oqfs.ToString(FormattingEHM.Matches);
            actual.ToClipboardExpected();
            { }
            expected = @" 
[model:10 qmatch:0 pmatch:0 live:0]"
            ;

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting result to match."
            );
        }
        #endregion S U B T E S T S
    }
}
