using IVSoftware.Portable.Common.Attributes;
using IVSoftware.Portable.Common.Collections;
using IVSoftware.Portable.Common.Collections.Internal;
using IVSoftware.Portable.SQLiteMarkdown.Common;
using IVSoftware.Portable.Xml.Linq;
using IVSoftware.Portable.Xml.Linq.XBoundObject;
using IVSoftware.WinOS.MSTest.Extensions;
using System.Xml.Linq;

namespace IVSoftware.Portable.SQLiteMarkdown.MSTest;

[TestClass]
public class TestClass_260328_Model
{
    [TestMethod, Canonical("XElement change handling.")]
    public void Test_Histogrammer()
    {
        string actual, expected;
        int 
            changeCount = 0,
            changeCountB4;
        var mdc = new ModeledMarkdownContext<SelectableQFModel>();
        var model = mdc.Model;
        var histo = model.To<EnumHistogrammer<StdModelAttribute>>();

        subtest_TrackLateral();
        subtest_TrackCurrentChild();
        subtest_TrackAddRemoveChild();

        #region S U B T E S T S
        void subtest_TrackLateral()
        {
            // Add
            model.SetStdModelAttributeValue(StdModelAttribute.qmatch, true);

            actual = histo.ToString(HistogrammerFormat.Default);
            actual.ToClipboardExpected();
            { }
            expected = @" 
[model:0 match:1 qmatch:1 pmatch:0]"
            ;
            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting histogram to match."
            );

            // CONFIRMED:
            // - Setting to same value *does* raise raw XObject.Change events.
            // - However these are intercepted prior to OnXAttributeChanged.
            changeCountB4 = changeCount;
            model.SetStdModelAttributeValue(StdModelAttribute.qmatch, true);
            Assert.AreEqual(changeCountB4, changeCount);

            actual = histo.ToString(HistogrammerFormat.Default);
            actual.ToClipboardExpected();
            { }
            expected = @" 
[model:0 match:1 qmatch:1 pmatch:0]"
            ;

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting NO CHANGE."
            );

            // Remove
            model.RemoveDescendantAttributes(StdModelAttribute.qmatch, includeSelf: true);
            actual = histo.ToString(HistogrammerFormat.Default);
            actual.ToClipboardExpected();
            { }
            var pathological  = @" 
[model:0 match:1 qmatch:1 pmatch:0]"
            ;
            expected = @" 
[model:0 match:0 qmatch:0 pmatch:0]"
            ;

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting empty histogram."
            );
        }

        void subtest_TrackCurrentChild()
        {
            var xel = new XElement(nameof(StdModelElement.item));
            model.Add(xel);

            // Add
            xel.SetStdModelAttributeValue(StdModelAttribute.qmatch, true);

            actual = histo.ToString(HistogrammerFormat.Default);
            actual.ToClipboardExpected();
            { }
            expected = @" 
[model:0 match:1 qmatch:1 pmatch:0]"
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
<model mdc=""[MDC]"" histo=""[model:0 match:1 qmatch:1 pmatch:0]"" filters=""[No Active Filters]"">
  <item qmatch=""True"" match=""True"" />
</model>";

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting result to match."
            );

            // CONFIRMED:
            // - Setting to same value *does* raise raw XObject.Change events.
            // - However these are intercepted prior to OnXAttributeChanged.
            changeCountB4 = changeCount;
            xel.SetStdModelAttributeValue(StdModelAttribute.qmatch, true);
            Assert.AreEqual(changeCountB4, changeCount);


            actual = histo.ToString(HistogrammerFormat.Default);
            actual.ToClipboardExpected();
            { }
            expected = @" 
[model:0 match:1 qmatch:1 pmatch:0]"
            ;

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting NO CHANGE."
            );

            actual = model.ToString();
            actual.ToClipboardExpected();
            { }
            expected = @" 
<model mdc=""[MDC]"" histo=""[model:0 match:1 qmatch:1 pmatch:0]"" filters=""[No Active Filters]"">
  <item qmatch=""True"" match=""True"" />
</model>";

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting NO CHANGE."
            );

            // Remove from Model
            model.RemoveDescendantAttributes(StdModelAttribute.qmatch);
            actual = histo.ToString(HistogrammerFormat.Default);
            actual.ToClipboardExpected();
            { }
            expected = @" 
[model:0 match:0 qmatch:0 pmatch:0]"
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
<model mdc=""[MDC]"" histo=""[model:0 match:0 qmatch:0 pmatch:0]"" filters=""[No Active Filters]"">
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
            var xel = new XElement(
                nameof(StdModelElement.item),
                new XAttribute(nameof(StdModelAttribute.qmatch), bool.TrueString));

            // Add offline - before this node is parented.
            model.Add(xel);

            actual = histo.ToString(HistogrammerFormat.Default);
            actual.ToClipboardExpected();
            { }
            expected = @" 
[model:0 match:1 qmatch:1 pmatch:0]"
            ;

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting offline qmatch to 'join' the histogram when attached to a parent."
            );

            // Remove
            xel.Remove();
            actual = histo.ToString(HistogrammerFormat.Default);
            actual.ToClipboardExpected();
            { }
            expected = @" 
[model:0 match:0 qmatch:0 pmatch:0]"
            ;

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting empty histogram."
            );

            // Add it back in again
            model.Add(xel);

            actual = histo.ToString(HistogrammerFormat.Default);
            actual.ToClipboardExpected();
            { }
            expected = @" 
[model:0 match:1 qmatch:1 pmatch:0]"
            ;

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting histogram to increment."
            );

            xel.SetStdModelAttributeValue(StdModelAttribute.qmatch, false);

            actual = histo.ToString(HistogrammerFormat.Default);
            actual.ToClipboardExpected();
            { }
            expected = @" 
[model:0 match:0 qmatch:0 pmatch:0]"
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

            actual = histo.ToString(HistogrammerFormat.Default);
            actual.ToClipboardExpected();
            { }
            expected = @" 
[model:1 match:1 qmatch:1 pmatch:0]"
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
    public void Test_HistogrammerMDC()
    {
        using var te = this.TestableEpoch();

        string actual, expected;
        ModeledMarkdownContext<SelectableQFModel> mmdc = new(){ QueryFilterConfig = QueryFilterConfig.Query };
        XElement model = mmdc.Model;

        mmdc.LoadCanon(new List<SelectableQFModel>().PopulateForDemo(10));

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
            actual = mmdc.ToString(HistogrammerFormat.Default);
            actual.ToClipboardExpected();
            { }
            expected = @" 
[model:10 match:0 qmatch:0 pmatch:0]"
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
