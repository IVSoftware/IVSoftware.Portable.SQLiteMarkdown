using IVSoftware.Portable.SQLiteMarkdown.MSTest.Models;
using IVSoftware.Portable.SQLiteMarkdown.Util;
using IVSoftware.Portable.Xml.Linq.XBoundObject;
using SQLite;
using System.Collections.ObjectModel;

namespace IVSoftware.Portable.SQLiteMarkdown.MSTest
{
    [TestClass]
    public class TestClass_CollectionPreview
    {
        [TestMethod]
        public void TestMethod_AddRange()
        {
            Switcheroo.ObservableNetProjectionWithComposition<ItemCardModel>
                opc = new();
            IModeledMarkdownContext
                mdc = opc.Model.To<IModeledMarkdownContext>();
            { }
            // F I L T E R    M O D E    ! ! ! !
            opc.QueryFilterConfig = QueryFilterConfig.Filter;

            #region M D C    B O O T S T R A P
            Assert.AreSame(
                opc,
                mdc.ObservableNetProjection,
                "Expecting opc is injected in the factory getter.");

            Assert.IsTrue(opc.IsFiltering, "ALWAYS TRUE in filter mode.");

            Type ct = mdc.ContractType;
            TableMapping mapping = ct.GetSQLiteMapping();
            Assert.AreEqual(
                $"{nameof(ItemCardModel)}",
                mapping.TableName,
                "Expecting correct table name mapping."
            );
            #endregion M D C    B O O T S T R A P

            opc.AddDynamic("Brown Dog", "[canine][color]", false, new() { "loyal", "friend", "furry" });
        }
    }
}

