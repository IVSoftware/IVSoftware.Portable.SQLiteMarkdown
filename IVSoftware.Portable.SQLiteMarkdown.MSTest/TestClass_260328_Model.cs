using IVSoftware.Portable.Common.Attributes;
using IVSoftware.Portable.SQLiteMarkdown.Internal;
using IVSoftware.Portable.SQLiteMarkdown.Util;
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

        Dictionary<XElement, XElement> parentsOfRemoved = new();
        EnumHistogrammer<StdMarkdownAttribute> histo = new(ZeroCountOption.Remove);

        var model = new XElement(nameof(StdMarkdownElement.model));
        model.Changing += (sender, e) =>
        {
            if (sender is XElement xel && e.ObjectChange == XObjectChange.Remove)
            {
                parentsOfRemoved[xel] = xel.Parent ?? throw new NullReferenceException();
            }
        };
        model.Changed += (sender, e) =>
        {
            switch (sender)
            {
                case XElement xel:
                    XElement? pxel =
                        e.ObjectChange == XObjectChange.Remove
                        ? parentsOfRemoved[xel]
                        : xel.Parent;
                    if (pxel is null)
                    {
                        throw new NullReferenceException();
                    }
                    else
                    {
                        OnXElementChanged(xel, pxel, e);
                    }
                    break;
                case XAttribute xattr:
                    OnXAttributeChanged(xattr, e);
                    break;
            }
        };
        #region L o c a l F x
        void OnXElementChanged(XElement xel, XElement pxel, XObjectChangeEventArgs e)
        {
        }

        void OnXAttributeChanged(XAttribute xattr, XObjectChangeEventArgs e)
        {
            if(Enum.TryParse(xattr.Name.LocalName, out StdMarkdownAttribute std))
            {
                switch (e.ObjectChange)
                {
                    case XObjectChange.Add:
                        histo += std;
                        break;
                    case XObjectChange.Remove:
                        histo -= std;
                        break;
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
            model.SetAttributeValue(StdMarkdownAttribute.qmatch, true);

            actual = histo.ToString(HistogrammerToStringOption.Json);
            actual.ToClipboardExpected();
            { }
            expected = @" 
{""qmatch"":1}";

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting histogram to match."
            );
            model.SetAttributeValue(StdMarkdownAttribute.qmatch, true);

            actual = histo.ToString(HistogrammerToStringOption.Json);
            actual.ToClipboardExpected();
            { }
            expected = @" 
{""qmatch"":1}";

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting NO CHANGE."
            );

            // Remove
            model.RemoveDescendantAttributes(StdMarkdownAttribute.qmatch, includeSelf: true);
            actual = histo.ToString(HistogrammerToStringOption.Json);
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
            xel.SetAttributeValue(StdMarkdownAttribute.qmatch, true);

            actual = histo.ToString(HistogrammerToStringOption.Json);
            actual.ToClipboardExpected();
            { }
            expected = @" 
{""qmatch"":1}";

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting histogram to match."
            );
            xel.SetAttributeValue(StdMarkdownAttribute.qmatch, true);

            actual = histo.ToString(HistogrammerToStringOption.Json);
            actual.ToClipboardExpected();
            { }
            expected = @" 
{""qmatch"":1}";

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting NO CHANGE."
            );

            // Remove from Model
            model.RemoveDescendantAttributes(StdMarkdownAttribute.qmatch);
            actual = histo.ToString(HistogrammerToStringOption.Json);
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
            model.Add(xel);
            { }
        }
        #endregion S U B T E S T S
    }
}
