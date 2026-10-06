using System.Diagnostics;
using System.Xml.Linq;
using ClipboardHelpers = IVSoftware.WinOS.MSTest.Extensions.Extensions;

namespace IVSoftware.Portable.SQLiteMarkdown.MSTest;

/// <summary>
/// Keeps assertion-authoring clipboard output in debugger sessions without making
/// unattended test runs depend on access to the Windows clipboard.
/// </summary>
internal static class TestClipboardExtensions
{
    public static void ToClipboard(this string text, bool log = false)
    {
        if (Debugger.IsAttached) ClipboardHelpers.ToClipboard(text, log);
    }

    public static void ToClipboard(this XElement element)
    {
        if (Debugger.IsAttached) ClipboardHelpers.ToClipboard(element);
    }

    public static void ToClipboardExpected(this string text)
    {
        if (Debugger.IsAttached) ClipboardHelpers.ToClipboardExpected(text);
    }

    public static void ToClipboardExpected(this XElement element)
    {
        if (Debugger.IsAttached) ClipboardHelpers.ToClipboardExpected(element);
    }

    public static void ToClipboardAssert(this string text, string? message = null)
    {
        if (Debugger.IsAttached) ClipboardHelpers.ToClipboardAssert(text, message);
    }

    public static void ToClipboardAssert(this XElement element, string? message = null)
    {
        if (Debugger.IsAttached) ClipboardHelpers.ToClipboardAssert(element, message);
    }
}
