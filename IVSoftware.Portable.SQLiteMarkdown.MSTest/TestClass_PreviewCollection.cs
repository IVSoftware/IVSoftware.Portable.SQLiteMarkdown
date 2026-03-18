using IVSoftware.Portable.SQLiteMarkdown.Common;
using IVSoftware.Portable.SQLiteMarkdown.Internal;
using IVSoftware.Portable.SQLiteMarkdown.Util;

namespace IVSoftware.Portable.SQLiteMarkdown.MSTest;

[TestClass]
public class TestClass_PreviewCollection
{
    [TestMethod, DoNotParallelize]
    public void Test_IListBasics()
    {
        using var te = this.TestableEpoch();

        var builder = new List<string>();
        var opc = new ObservablePreviewCollection<SelectableQFModel>();

        opc.CollectionChanged += (sender, e) =>
        {
            builder.Add(e.ToString(ReferenceEquals(sender, opc)));
        };

        opc.AddDynamic("Brown Dog", "[canine][color]", false, new() { "loyal", "friend", "furry" });

        { }
    }
}
