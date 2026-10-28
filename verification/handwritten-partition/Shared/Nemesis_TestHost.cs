using System.Reflection;

namespace IVSoftware.Portable.SQLiteMarkdown.MSTest;

/// <summary>
/// Nemesis-authored test-host routing isolates the original disposable database fixture.
/// </summary>
/// <remarks>
/// - Authorship: Nemesis; contains no assertion substitutions or production changes.
/// - Only the fixture's database path is redirected, before class initialization.
/// </remarks>
[TestClass]
public sealed class Nemesis_TestHost
{
    [AssemblyInitialize]
    public static void Initialize(TestContext context)
    {
        var fixture = typeof(TestClass_TestClass_SQLiteMarkdown).GetNestedType("DatabaseDisposableHost", BindingFlags.NonPublic)!;
        var path = fixture.GetField("_databasePathForTest", BindingFlags.Static | BindingFlags.NonPublic)!;
        string directory = Path.Combine(AppContext.BaseDirectory, "TestData", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        path.SetValue(null, Path.Combine(directory, "disposable.db"));
    }
}
