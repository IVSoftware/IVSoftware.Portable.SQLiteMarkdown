using IVSoftware.Portable.Common.Attributes;
using IVSoftware.Portable.Common.Exceptions;
using IVSoftware.Portable.SQLiteMarkdown.Common;
using IVSoftware.Portable.SQLiteMarkdown.Internal;
using IVSoftware.Portable.SQLiteMarkdown.Util;
using IVSoftware.Portable.Xml.Linq;
using IVSoftware.Portable.Xml.Linq.XBoundObject;
using IVSoftware.WinOS.MSTest.Extensions;
using Newtonsoft.Json;
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

        Dictionary<XObject, XElement> parentsOfRemoved = new();
        Dictionary<XAttribute, bool?> oldValues = new();
        EnumHistogrammer<StdMarkdownAttribute> histo = new(ZeroCountOption.Remove);
        var model = new XElement(
            nameof(StdMarkdownElement.model),
            new XBoundAttribute(nameof(StdMarkdownAttribute.mdc), new MarkdownContext<SelectableQFModel>(), "[MDC]"),
            new XAttribute(nameof(StdMarkdownAttribute.autocount), 0));

        model.Changing += (sender, e) =>
        {
            if (sender is XObject xob)
            {
                switch (e.ObjectChange)
                {
                    case XObjectChange.Remove:
                        parentsOfRemoved[xob] = xob.Parent ?? throw new NullReferenceException();
                        break;
                    case XObjectChange.Value when xob is XAttribute xattr:
                        oldValues[xattr] = bool.TryParse(xattr.Value, out var valid) ? valid : null;
                        break;
                }
            }
            else
            {
                throw new InvalidOperationException();
            }
        };

        model.Changed += (sender, e) =>
        {
            if (sender is XObject xob)
            {
                XElement? pxel = xob.Parent;
                switch (e.ObjectChange)
                {
                    case XObjectChange.Remove:
                        pxel = parentsOfRemoved[xob];
                        parentsOfRemoved.Remove(xob);
                        break;
                    case XObjectChange.Value when xob is XAttribute xattr:
                        var oldValue = oldValues.TryGetValue(xattr, out var validOld) ? validOld : null;
                        bool? newValue = bool.TryParse(xattr.Value, out var validNew) ? validNew : null;
                        oldValues.Remove(xattr);
                        if (newValue is null ^ oldValue is null)
                        {
                            this.ThrowPolicyException(MarkdownContextPolicyViolation.XAttributeBooleanToggle);
                            if (Enum.TryParse(xattr.Name.LocalName, ignoreCase: false, out StdMarkdownAttribute std))
                            {
                                if (oldValue == true)
                                {
                                    histo -= std;
                                }
                                else if (newValue == true)
                                {
                                    histo += std;
                                }
                            }
                            return;
                        }
                        else
                        {
                            if (newValue == oldValue)
                            {
                                return;
                            }
                            else
                            {   /* G T K */
                                // Toggle detected.
                            }
                        }
                        break;
                }
                switch (sender)
                {
                    case XElement xel:
                        OnXElementChanged(xel, pxel ?? throw new NullReferenceException(), e);
                        break;
                    case XAttribute xattr:
                        OnXAttributeChanged(xattr, pxel ?? throw new NullReferenceException(), e);
                        break;
                }
            }
            else
            {
                throw new InvalidOperationException();
            }
        };
        #region L o c a l F x
        void OnXElementChanged(XElement xel, XElement pxel, XObjectChangeEventArgs e)
        {
            changeCount++;
            switch (e.ObjectChange)
            {
                case XObjectChange.Add:
                case XObjectChange.Remove:
                    foreach (var attr in xel.Attributes())
                    {
                        OnXAttributeChanged(attr, pxel, e);
                    }
                    break;
            }
        }

        void OnXAttributeChanged(XAttribute xattr, XElement pxel, XObjectChangeEventArgs e)
        {
            changeCount++;
            if (Enum.TryParse(xattr.Name.LocalName, ignoreCase: false, out StdMarkdownAttribute std))
            {
                bool? newValue = bool.TryParse(xattr.Value, out var valid) ? valid : null;
                switch (e.ObjectChange)
                {
                    case XObjectChange.Add:
                        if(newValue != false)
                        { 
                            histo += std;
                        }
                        localUpdateAutocount();
                        break;
                    case XObjectChange.Remove:
                        if (newValue != false)
                        {
                            histo -= std;
                        }
                        localUpdateAutocount();
                        break;
                    case XObjectChange.Value:
                        switch (newValue)
                        {
                            case null:
                                /* N O O P */
                                break;
                            case true:
                                histo += std;
                                break;
                            case false:
                                histo -= std;
                                break;
                        }
                        break;
                }
                void localUpdateAutocount()
                {
                    // Count the actual model XBO objects
                    if (std == StdMarkdownAttribute.model)
                    {
                        var root = pxel.AncestorsAndSelf().Last();
                        if (root.Has<IMarkdownContext>())
                        {
                            root.SetStdAttributeValue(StdMarkdownAttribute.autocount, histo[StdMarkdownAttribute.model]);
                        }
                    }
                }
            }
        }
        #endregion L o c a l F x


        subtest_TrackLateral();
        subtest_TrackCurrentChild();
        subtest_TrackAddRemoveChild();

        #region S U B T E S T S
        void subtest_TrackLateral()
        {
            // Add
            model.SetStdAttributeValue(StdMarkdownAttribute.qmatch, true);

            actual = histo.ToString(Formatting.Indented);
            actual.ToClipboardExpected();
            { }
            expected = @" 
{
  ""qmatch"": 1
}"
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
            model.SetStdAttributeValue(StdMarkdownAttribute.qmatch, true);
            Assert.AreEqual(changeCountB4, changeCount);

            actual = histo.ToString(Formatting.Indented);
            actual.ToClipboardExpected();
            { }
            expected = @" 
{
  ""qmatch"": 1
}"
            ;

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting NO CHANGE."
            );

            // Remove
            model.RemoveDescendantAttributes(StdMarkdownAttribute.qmatch, includeSelf: true);
            actual = histo.ToString(Formatting.Indented);
            actual.ToClipboardExpected();
            { }
            expected = @" 
{}"
            ;

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting empty histogram."
            );
        }

        void subtest_TrackCurrentChild()
        {
            var xel = new XElement(nameof(StdMarkdownElement.xitem));
            model.Add(xel);

            // Add
            xel.SetStdAttributeValue(StdMarkdownAttribute.qmatch, true);

            actual = histo.ToString(Formatting.Indented);
            actual.ToClipboardExpected();
            { }
            expected = @" 
{
  ""qmatch"": 1
}"
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
            xel.SetStdAttributeValue(StdMarkdownAttribute.qmatch, true);
            Assert.AreEqual(changeCountB4, changeCount);

            actual = histo.ToString(Formatting.Indented);
            actual.ToClipboardExpected();
            expected = @" 
{
  ""qmatch"": 1
}"
            ;

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting NO CHANGE."
            );

            // Remove from Model
            model.RemoveDescendantAttributes(StdMarkdownAttribute.qmatch);
            actual = histo.ToString(Formatting.Indented);
            actual.ToClipboardExpected();
            { }
            expected = @" 
{}"
            ;

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting empty histogram."
            );
        }

        void subtest_TrackAddRemoveChild()
        {
            var xel = new XElement(
                nameof(StdMarkdownElement.xitem),
                new XAttribute(nameof(StdMarkdownAttribute.qmatch), true));

            // Add
            model.Add(xel);

            actual = histo.ToString(Formatting.Indented);
            actual.ToClipboardExpected();
            { }
            expected = @" 
{
  ""qmatch"": 1
}"
            ;

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting histogram to match."
            );

            // Remove
            xel.Remove();
            actual = histo.ToString(Formatting.Indented);
            actual.ToClipboardExpected();
            { }
            expected = @" 
{}"
            ;

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting empty histogram."
            );

            // Add it back in again
            model.Add(xel);

            actual = histo.ToString(Formatting.Indented);
            actual.ToClipboardExpected();
            { }
            expected = @" 
{
  ""qmatch"": 1
}"
            ;

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting histogram to match."
            );

            xel.SetStdAttributeValue(StdMarkdownAttribute.qmatch, false);

            actual = histo.ToString(Formatting.Indented);
            actual.ToClipboardExpected();
            { }
            expected = @" 
{}"
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
                nameof(StdMarkdownElement.xitem),
                new XBoundAttribute(nameof(StdMarkdownAttribute.model), i1),
                new XAttribute(nameof(StdMarkdownAttribute.qmatch), true));

            model.Add(xel);

            actual = histo.ToString(Formatting.Indented);
            actual.ToClipboardExpected();
            { }
            expected = @" 
{
  ""model"": 1,
  ""qmatch"": 1
}"
            ;

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting histogram to match."
            );
        }
        #endregion S U B T E S T S
    }

    [TestMethod]
    public void Test_HisogrammerMDC()
    {
        string actual, expected;
        ModeledMarkdownContext<SelectableQFModel> mmdc = new(){ QueryFilterConfig = QueryFilterConfig.Query };
        XElement model = mmdc.Model;

        mmdc.LoadCanon(new List<SelectableQFModel>().PopulateForDemo(10));

        actual = model.ToString();

        actual.ToClipboardExpected();
        { } // <- FIRST TIME ONLY: Adjust the message.
        actual.ToClipboardAssert("Expecting result to match.");
        { }

        expected = @" 
<model mdc=""[MMDC]"" autocount=""10"" count=""10"" matches=""10"">
  <xitem text=""88827ddb-e438-444a-b554-2767063d5a16"" model=""[SelectableQFModel]"" sort=""0"" />
  <xitem text=""ce02062f-45f8-4069-ab41-155c843f3264"" model=""[SelectableQFModel]"" sort=""1"" />
  <xitem text=""3d7860d8-fdb1-471a-8d14-d7d445bc2fa4"" model=""[SelectableQFModel]"" sort=""2"" />
  <xitem text=""772b4648-fc88-47ce-8157-76dce27b0ed9"" model=""[SelectableQFModel]"" sort=""3"" />
  <xitem text=""563f68f3-893d-4888-aa77-2f45a4dcea2f"" model=""[SelectableQFModel]"" sort=""4"" />
  <xitem text=""bbf6ad4f-f15a-403e-913b-81535656ec87"" model=""[SelectableQFModel]"" sort=""5"" />
  <xitem text=""09b79f51-e06b-43f3-85ff-68e47ae0ebd2"" model=""[SelectableQFModel]"" sort=""6"" />
  <xitem text=""18f818b3-f4bf-411a-86e6-77f639275d22"" model=""[SelectableQFModel]"" sort=""7"" />
  <xitem text=""171c8d82-8c43-490b-bf0d-959bdb5221a0"" model=""[SelectableQFModel]"" sort=""8"" />
  <xitem text=""e8de5c18-c28d-453d-a9c6-81de8a831db6"" model=""[SelectableQFModel]"" sort=""9"" />
</model>";

        Assert.AreEqual(
            expected.NormalizeResult(),
            actual.NormalizeResult(),
            "Expecting result to match."
        );


        subtest_Name();

        #region S U B T E S T S
        void subtest_Name()
        {
        }
        #endregion S U B T E S T S
    }
}
