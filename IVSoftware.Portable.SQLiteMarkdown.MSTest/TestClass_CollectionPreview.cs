using IVSoftware.Portable.SQLiteMarkdown.MSTest.Models;
using IVSoftware.Portable.SQLiteMarkdown.Util;
using IVSoftware.Portable.Xml.Linq.XBoundObject;
using IVSoftware.WinOS.MSTest.Extensions;
using Newtonsoft.Json;
using SQLite;
using System.Collections.ObjectModel;

namespace IVSoftware.Portable.SQLiteMarkdown.MSTest
{
    [TestClass]
    public class TestClass_CollectionPreview
    {
        [TestMethod, DoNotParallelize]
        public void TestMethod_AddRange()
        {
            using var te = this.TestableEpoch();

            string actual, expected;

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
            { }

            actual = JsonConvert.SerializeObject(opc, Newtonsoft.Json.Formatting.Indented);
            actual.ToClipboardExpected();
            { }
            expected = @" 
[
  {
    ""Id"": ""312d1c21-0000-0000-0000-000000000000"",
    ""Description"": ""Brown Dog"",
    ""Keywords"": ""[\""loyal\"",\""friend\"",\""furry\""]"",
    ""KeywordsDisplay"": ""\""loyal\"",\""friend\"",\""furry\"""",
    ""Tags"": ""[canine] [color]"",
    ""IsChecked"": false,
    ""Selection"": 0,
    ""IsEditing"": false,
    ""PrimaryKey"": ""312d1c21-0000-0000-0000-000000000000"",
    ""QueryTerm"": ""brown~dog~loyal~friend~furry~[canine]~[color]"",
    ""FilterTerm"": ""brown~dog~loyal~friend~furry~[canine]~[color]"",
    ""TagMatchTerm"": ""[canine] [color]"",
    ""Properties"": ""{\r\n  \""Description\"": \""Brown Dog\"",\r\n  \""Tags\"": \""[canine] [color]\"",\r\n  \""Keywords\"": \""[\\\""loyal\\\"",\\\""friend\\\"",\\\""furry\\\""]\""\r\n}""
  }
]";

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting model to track."
            );
        }
    }
}

