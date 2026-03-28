using IVSoftware.Portable.Common.Attributes;
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
            changeCount++;
            if (sender is XObject xob)
            {
                bool? oldValue = null;
                XElement? pxel = xob.Parent;
                switch (e.ObjectChange)
                {
                    case XObjectChange.Remove:
                        pxel = parentsOfRemoved[xob];
                        parentsOfRemoved.Remove(xob);
                        break;
                    case XObjectChange.Value when xob is XAttribute xattr:
                        oldValue = oldValues.TryGetValue(xattr, out var valid) ? valid : null;
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
            if (Enum.TryParse(xattr.Name.LocalName, ignoreCase: false, out StdMarkdownAttribute std))
            {
                bool? @bool = null;
                if(bool.TryParse(xattr.Value, out var @explicit))
                {
                    @bool = @explicit;
                }
                switch (e.ObjectChange)
                {
                    case XObjectChange.Add:
                        if(@bool != false)
                        { 
                            histo += std;
                        }
                        localUpdateAutocount();
                        break;
                    case XObjectChange.Remove:
                        if (@bool != false)
                        {
                            histo -= std;
                        }
                        localUpdateAutocount();
                        break;
                    case XObjectChange.Value:
                        switch (@bool)
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

            // Idempotent set
            changeCountB4 = changeCount;
            model.SetStdAttributeValue(StdMarkdownAttribute.qmatch, true);
            Assert.AreEqual(changeCountB4 + 1, changeCount);

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

            // Causes no change
            xel.SetStdAttributeValue(StdMarkdownAttribute.qmatch, true);

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
}
