using System.Diagnostics;
using System.Xml.Linq;

namespace IVSoftware.Portable.SQLiteMarkdown.XBO;

/// <summary>
/// Nemesis-generated adapter for the handwritten suite's debugger-only assertion-authoring output.
/// </summary>
/// <remarks>
/// - Authorship: Nemesis; follows the existing TestClipboardExtensions guard.
/// - Changes no assertions or snapshots; unattended tests do not publish authoring text to the clipboard.
/// </remarks>
internal static class Nemesis_ClipboardAdapter
{
    public static void ToClipboardExpected(this string text)
    {
        if (Debugger.IsAttached) IVSoftware.WinOS.MSTest.Extensions.Extensions.ToClipboardExpected(text);
    }

    public static void ToClipboardExpected(this XElement element)
    {
        if (Debugger.IsAttached) IVSoftware.WinOS.MSTest.Extensions.Extensions.ToClipboardExpected(element);
    }
}
