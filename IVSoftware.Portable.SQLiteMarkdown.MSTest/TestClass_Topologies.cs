using IVSoftware.Portable.Common.Attributes;
using IVSoftware.Portable.SQLiteMarkdown.Common;
using IVSoftware.Portable.SQLiteMarkdown.Internal;
using IVSoftware.Portable.SQLiteMarkdown.Util;
using IVSoftware.Portable.Xml.Linq.XBoundObject;
using IVSoftware.WinOS.MSTest.Extensions;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Xml.Linq;

namespace IVSoftware.Portable.SQLiteMarkdown.MSTest
{
    [TestClass, DoNotParallelize]
    public class TestClass_Topologies
    {
        [TestMethod]
        public void Test_TopologyBasics()
        {
            string actual, expected;
            using var te = this.TestableEpoch();

            ModeledMarkdownContext<SelectableQFModel> mmdc = new();
            XElement model = mmdc.Model;

            #region L o c a l F x
            // Get a dynamic item using a dummy list.
            List<SelectableQFModel> Ephemeral() => new List<SelectableQFModel>();
            string SerializeTopology() => mmdc.SerializeTopology<SelectableQFModel>();
            #endregion L o c a l F x


            subtest_SerializeTopo();
            subtest_DefaultTopo();

            #region S U B T E S T S
            // Test the custom JSON serializer itself!
            void subtest_SerializeTopo()
            {
                actual = SerializeTopology();
                actual.ToClipboardExpected();
                { } // <- FIRST TIME ONLY: Adjust the message.
                actual.ToClipboardAssert("Expecting json serialization to match.");
                { }
                expected = @" 
{
  ""Model"": {
    ""model"": {
      ""@mdc"": ""[ModeledMarkdownContext]""
    }
  },
  ""IsFiltering"": false,
  ""ObservableNetProjection"": null,
  ""CanonicalSuperset"": [],
  ""PredicateMatchSubset"": [],
  ""Count"": 0,
  ""IsReadOnly"": false,
  ""ProjectionTopology"": ""None"",
  ""ProjectionOption"": ""ObservableOnly"",
  ""ReplaceItemsEventingOptions"": ""StructuralReplaceEvent""
}";

                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting json serialization to match."
                );
            }

            void subtest_DefaultTopo()
            {
                actual = SerializeTopology();
                actual.ToClipboardExpected();
                { }
                expected = @" 
{
  ""Model"": {
    ""model"": {
      ""@mdc"": ""[ModeledMarkdownContext]""
    }
  },
  ""IsFiltering"": false,
  ""ObservableNetProjection"": null,
  ""CanonicalSuperset"": [],
  ""PredicateMatchSubset"": [],
  ""Count"": 0,
  ""IsReadOnly"": false,
  ""ProjectionTopology"": ""None"",
  ""ProjectionOption"": ""ObservableOnly"",
  ""ReplaceItemsEventingOptions"": ""StructuralReplaceEvent""
}"
                ;

                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting json serialization to match."
                );

                mmdc.Add(Ephemeral().AddDynamic("Cats", "[]", false));
                { }

                actual = SerializeTopology();
                actual.ToClipboardAssert("Expecting json serialization to match.");
                { }
                expected = @" 
{
  ""Model"": {
    ""model"": {
      ""@mdc"": ""[ModeledMarkdownContext]""
    }
  },
  ""IsFiltering"": false,
  ""ObservableNetCollection"": null,
  ""CanonicalSuperset"": [
    {
      ""Id"": ""312d1c21-0000-0000-0000-000000000000"",
      ""Description"": ""Cats"",
      ""Keywords"": ""[]"",
      ""KeywordsDisplay"": """",
      ""Tags"": ""[]"",
      ""IsChecked"": false,
      ""Selection"": 0,
      ""IsEditing"": false,
      ""PrimaryKey"": ""312d1c21-0000-0000-0000-000000000000"",
      ""QueryTerm"": ""cats"",
      ""FilterTerm"": ""cats"",
      ""TagMatchTerm"": """",
      ""Properties"": ""{\r\n  \""Description\"": \""Cats\"",\r\n  \""Tags\"": \""[]\""\r\n}""
    }
  ],
  ""PredicateMatchSubset"": [],
  ""Count"": 1,
  ""IsReadOnly"": false,
  ""ProjectionTopology"": ""None"",
  ""ProjectionOption"": ""Inherited"",
  ""ReplaceItemsEventingOptions"": 0,
  ""ObservableNetProjection"": null
}";

                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting json serialization to match."
                );

                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting json serialization to match."
                );
            }
            #endregion S U B T E S T S
        }

        /// <summary>
        /// QueryFilter Router that inherits MMDC
        /// </summary>
        [TestMethod, Probationary("While I think through what 'Topology' means.")]
        public void Test_Topologies()
        {
            subtest_InheritObservableCollection();
            subtest_InheritMMDC_ObservableOnly();
            subtest_InheritMMDC_AllowDirectChanges();

            #region S U B T E S T S
            void subtest_InheritObservableCollection()
            {
                var oqf = new InheritObservableCollection.OBQFC<SelectableQFModel>();
            }
            void subtest_InheritMMDC_ObservableOnly()
            {
                var oqf = new InheritModeledMarkdownContext.ObservableOnly.OBQFC<SelectableQFModel>();
            }

            void subtest_InheritMMDC_AllowDirectChanges()
            {
                var oqf = new InheritModeledMarkdownContext.AllowDirectUpdates.OBQFC<SelectableQFModel>();
            }
            

            #endregion S U B T E S T S
        }
    }
    namespace InheritObservableCollection
    {
        class OBQFC<T>
            : ObservableCollection<T>
            , INotifyCollectionChanged
            where T : new()
        {
            public OBQFC()
            {
                _mmdc.ProjectionOption = NetProjectionOption.AllowDirectChanges;
            }
            protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
            {
                base.OnCollectionChanged(e);
            }

            ModeledMarkdownContext<T> _mmdc = new ModeledMarkdownContext<T>();
        }
    }

    namespace InheritModeledMarkdownContext.ObservableOnly
    {
        class OBQFC<T>
            : ModeledMarkdownContext<T>
            , IEnumerable<T>
            , INotifyCollectionChanged
            where T : new()
        {
            public OBQFC()
            {
                ProjectionOption = NetProjectionOption.AllowDirectChanges;
            }
            public virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
            {
                CollectionChanged?.Invoke(this, e);
            }
            public event NotifyCollectionChangedEventHandler? CollectionChanged;
        }
    }

    namespace InheritModeledMarkdownContext.AllowDirectUpdates
    {
        class OBQFC<T>
            : ModeledMarkdownContext<T>
            , INotifyCollectionChanged
            where T : new()
        {
            public OBQFC()
            {
                ProjectionOption = NetProjectionOption.AllowDirectChanges;
            }
            public virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
            {
                CollectionChanged?.Invoke(this, e);
            }
            public event NotifyCollectionChangedEventHandler? CollectionChanged;
        }
    }
}
