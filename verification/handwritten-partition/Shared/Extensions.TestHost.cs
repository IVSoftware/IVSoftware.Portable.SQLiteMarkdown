using System.Reflection;

namespace IVSoftware.Portable.SQLiteMarkdown.MSTest;

/// <summary>
/// Keeps the handwritten tag-spacing control test-local without widening production visibility.
/// </summary>
/// <remarks>
/// Nemesis-authored host adapter. The existing tests set and restore the same
/// internal property; only the access route differs from their old friend assembly.
/// </remarks>
internal static class Extensions
{
    static PropertyInfo TagSpacing => typeof(IVSoftware.Portable.SQLiteMarkdown.Extensions)
        .GetProperty("InsertSpaceBetweenTags", BindingFlags.Static | BindingFlags.NonPublic)!;

    internal static bool InsertSpaceBetweenTags
    {
        get => (bool)TagSpacing.GetValue(null)!;
        set => TagSpacing.SetValue(null, value);
    }
}
