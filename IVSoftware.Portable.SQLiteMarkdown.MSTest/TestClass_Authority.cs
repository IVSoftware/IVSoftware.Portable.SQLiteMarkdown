using IVSoftware.Portable.Collections;
using IVSoftware.Portable.Common.Attributes;
using IVSoftware.Portable.Disposable;
using IVSoftware.Portable.SQLiteMarkdown.Common;
using IVSoftware.Portable.SQLiteMarkdown.Util;
using IVSoftware.Portable.StateMachine;
using IVSoftware.Portable.SQLiteMarkdown.Internal;
using IVSoftware.WinOS.MSTest.Extensions;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using Newtonsoft.Json;

namespace IVSoftware.Portable.SQLiteMarkdown.MSTest;

[TestClass]
[PublishedContract(type: typeof(AuthorityEpochProvider))]
public class TestClass_Authority
{
    List<string> builder = new();
    class ObservableNetProjection<T> : ObservablePreviewCollection<T>
    {
        [Probationary]
        public IModeledMarkdownContext? MMDC { get; set; }
#if false
        public IModeledMarkdownContext? MMDC
        {
            get => _mmdc;
            set
            {
                if (!Equals(_mmdc, value))
                {
                    if(_mmdc is not null)
                    {
                        _mmdc.ModelChanged -= localOnCollectionChangedForward;
                    }
                    _mmdc = value;
                    if (_mmdc is not null)
                    {
                        _mmdc.ModelChanged += localOnCollectionChangedForward;
                    }
                }

                void localOnCollectionChangedForward(object? sender, NotifyCollectionChangedEventArgs e)
                {
                    // OnCollectionChanged(e);
                }
            }
        }
        IModeledMarkdownContext? _mmdc = default;
#endif
    }
    class TestableMMDC : ModeledMarkdownContext<SelectableQFModel>
    {

    }

    [TestMethod, DoNotParallelize]
    public void Test_ApplyToList()
    {
        string actual, expected;
        IList<SelectableQFModel>? eph = null;
        using var te = this.TestableEpoch();
        var srce = new ObservableNetProjection<SelectableQFModel>();
        var dest = new ObservableCollection<SelectableQFModel>();
        var authorityEpoch = new AuthorityEpochProvider();
        var builder = new List<string>();

        #region L o c a l F x				
        using var local = srce.WithOnDispose(
            onInit: (sender, e) =>
            {
                srce.CollectionChanged += localOnCollectionChanged;
            },
            onDispose: (sender, e) =>
            {
                srce.CollectionChanged -= localOnCollectionChanged;
            });
        void localOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            builder.Add(e.ToString(true, authorityEpoch.Authority));
            dest.Apply(e);
        }
        bool EqualsSrceAndDest()
        {
            return srce
                .Select(_ => _.GetFullPath())
                .SequenceEqual(dest.Select(_ => _.GetFullPath()));
        }
        #endregion L o c a l F x

        subtest_WithProjectionAuthority();
        subtest_WithoutProjectionAuthority();

        #region S U B T E S T S
        void subtest_WithProjectionAuthority()
        {
            var authority = authorityEpoch.BeginAuthority(ModeledCollectionChangeAuthority.Projection);
            // CREATE (no side effects)
            var i1 = eph.AddDynamic("Item01");
            var i2 = eph.AddDynamic("Item02");
            var i3 = eph.AddDynamic("Item03");

            // ADD
            srce.Add(i1);
            srce.Add(i2);
            srce.Add(i3);
            Assert.IsTrue(EqualsSrceAndDest());

            // INSERT (middle)
            var i4 = eph.AddDynamic("Item04");
            srce.Insert(1, i4);
            Assert.IsTrue(EqualsSrceAndDest());

            actual = JsonConvert.SerializeObject(dest, Formatting.Indented);
            actual.ToClipboardExpected();
            { }
            expected = @" 
[
  {
    ""Id"": ""312d1c21-0000-0000-0000-000000000000"",
    ""Description"": ""Item01"",
    ""Keywords"": ""[]"",
    ""KeywordsDisplay"": """",
    ""Tags"": ""[]"",
    ""IsChecked"": false,
    ""Selection"": 0,
    ""IsEditing"": false,
    ""PrimaryKey"": ""312d1c21-0000-0000-0000-000000000000"",
    ""QueryTerm"": ""item01"",
    ""FilterTerm"": ""item01"",
    ""TagMatchTerm"": """",
    ""Properties"": ""{\r\n  \""Description\"": \""Item01\"",\r\n  \""Tags\"": \""[]\""\r\n}""
  },
  {
    ""Id"": ""312d1c21-0000-0000-0000-000000000003"",
    ""Description"": ""Item04"",
    ""Keywords"": ""[]"",
    ""KeywordsDisplay"": """",
    ""Tags"": ""[]"",
    ""IsChecked"": false,
    ""Selection"": 0,
    ""IsEditing"": false,
    ""PrimaryKey"": ""312d1c21-0000-0000-0000-000000000003"",
    ""QueryTerm"": ""item04"",
    ""FilterTerm"": ""item04"",
    ""TagMatchTerm"": """",
    ""Properties"": ""{\r\n  \""Description\"": \""Item04\"",\r\n  \""Tags\"": \""[]\""\r\n}""
  },
  {
    ""Id"": ""312d1c21-0000-0000-0000-000000000001"",
    ""Description"": ""Item02"",
    ""Keywords"": ""[]"",
    ""KeywordsDisplay"": """",
    ""Tags"": ""[]"",
    ""IsChecked"": false,
    ""Selection"": 0,
    ""IsEditing"": false,
    ""PrimaryKey"": ""312d1c21-0000-0000-0000-000000000001"",
    ""QueryTerm"": ""item02"",
    ""FilterTerm"": ""item02"",
    ""TagMatchTerm"": """",
    ""Properties"": ""{\r\n  \""Description\"": \""Item02\"",\r\n  \""Tags\"": \""[]\""\r\n}""
  },
  {
    ""Id"": ""312d1c21-0000-0000-0000-000000000002"",
    ""Description"": ""Item03"",
    ""Keywords"": ""[]"",
    ""KeywordsDisplay"": """",
    ""Tags"": ""[]"",
    ""IsChecked"": false,
    ""Selection"": 0,
    ""IsEditing"": false,
    ""PrimaryKey"": ""312d1c21-0000-0000-0000-000000000002"",
    ""QueryTerm"": ""item03"",
    ""FilterTerm"": ""item03"",
    ""TagMatchTerm"": """",
    ""Properties"": ""{\r\n  \""Description\"": \""Item03\"",\r\n  \""Tags\"": \""[]\""\r\n}""
  }
]";

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting json shows Item04 @ Index 1."
            );

            // REPLACE (index-based)
            var i2b = eph.AddDynamic("Item02B");
            srce[2] = i2b;
            Assert.IsTrue(EqualsSrceAndDest());

            actual = JsonConvert.SerializeObject(dest, Formatting.Indented);
            actual.ToClipboardExpected();
            { } // <- FIRST TIME ONLY: Adjust the message.
            actual.ToClipboardAssert("Expecting replacement @ Index 2.");
            { }
            expected = @" 
[
  {
    ""Id"": ""312d1c21-0000-0000-0000-000000000000"",
    ""Description"": ""Item01"",
    ""Keywords"": ""[]"",
    ""KeywordsDisplay"": """",
    ""Tags"": ""[]"",
    ""IsChecked"": false,
    ""Selection"": 0,
    ""IsEditing"": false,
    ""PrimaryKey"": ""312d1c21-0000-0000-0000-000000000000"",
    ""QueryTerm"": ""item01"",
    ""FilterTerm"": ""item01"",
    ""TagMatchTerm"": """",
    ""Properties"": ""{\r\n  \""Description\"": \""Item01\"",\r\n  \""Tags\"": \""[]\""\r\n}""
  },
  {
    ""Id"": ""312d1c21-0000-0000-0000-000000000003"",
    ""Description"": ""Item04"",
    ""Keywords"": ""[]"",
    ""KeywordsDisplay"": """",
    ""Tags"": ""[]"",
    ""IsChecked"": false,
    ""Selection"": 0,
    ""IsEditing"": false,
    ""PrimaryKey"": ""312d1c21-0000-0000-0000-000000000003"",
    ""QueryTerm"": ""item04"",
    ""FilterTerm"": ""item04"",
    ""TagMatchTerm"": """",
    ""Properties"": ""{\r\n  \""Description\"": \""Item04\"",\r\n  \""Tags\"": \""[]\""\r\n}""
  },
  {
    ""Id"": ""312d1c21-0000-0000-0000-000000000004"",
    ""Description"": ""Item02B"",
    ""Keywords"": ""[]"",
    ""KeywordsDisplay"": """",
    ""Tags"": ""[]"",
    ""IsChecked"": false,
    ""Selection"": 0,
    ""IsEditing"": false,
    ""PrimaryKey"": ""312d1c21-0000-0000-0000-000000000004"",
    ""QueryTerm"": ""item02b"",
    ""FilterTerm"": ""item02b"",
    ""TagMatchTerm"": """",
    ""Properties"": ""{\r\n  \""Description\"": \""Item02B\"",\r\n  \""Tags\"": \""[]\""\r\n}""
  },
  {
    ""Id"": ""312d1c21-0000-0000-0000-000000000002"",
    ""Description"": ""Item03"",
    ""Keywords"": ""[]"",
    ""KeywordsDisplay"": """",
    ""Tags"": ""[]"",
    ""IsChecked"": false,
    ""Selection"": 0,
    ""IsEditing"": false,
    ""PrimaryKey"": ""312d1c21-0000-0000-0000-000000000002"",
    ""QueryTerm"": ""item03"",
    ""FilterTerm"": ""item03"",
    ""TagMatchTerm"": """",
    ""Properties"": ""{\r\n  \""Description\"": \""Item03\"",\r\n  \""Tags\"": \""[]\""\r\n}""
  }
]";

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting replacement @ Index 2."
            );

            // MOVE
            srce.Move(3, 0);
            Assert.IsTrue(EqualsSrceAndDest());

            actual = JsonConvert.SerializeObject(dest, Formatting.Indented);
            actual.ToClipboardExpected();
            { }
            expected = @" 
[
  {
    ""Id"": ""312d1c21-0000-0000-0000-000000000002"",
    ""Description"": ""Item03"",
    ""Keywords"": ""[]"",
    ""KeywordsDisplay"": """",
    ""Tags"": ""[]"",
    ""IsChecked"": false,
    ""Selection"": 0,
    ""IsEditing"": false,
    ""PrimaryKey"": ""312d1c21-0000-0000-0000-000000000002"",
    ""QueryTerm"": ""item03"",
    ""FilterTerm"": ""item03"",
    ""TagMatchTerm"": """",
    ""Properties"": ""{\r\n  \""Description\"": \""Item03\"",\r\n  \""Tags\"": \""[]\""\r\n}""
  },
  {
    ""Id"": ""312d1c21-0000-0000-0000-000000000000"",
    ""Description"": ""Item01"",
    ""Keywords"": ""[]"",
    ""KeywordsDisplay"": """",
    ""Tags"": ""[]"",
    ""IsChecked"": false,
    ""Selection"": 0,
    ""IsEditing"": false,
    ""PrimaryKey"": ""312d1c21-0000-0000-0000-000000000000"",
    ""QueryTerm"": ""item01"",
    ""FilterTerm"": ""item01"",
    ""TagMatchTerm"": """",
    ""Properties"": ""{\r\n  \""Description\"": \""Item01\"",\r\n  \""Tags\"": \""[]\""\r\n}""
  },
  {
    ""Id"": ""312d1c21-0000-0000-0000-000000000003"",
    ""Description"": ""Item04"",
    ""Keywords"": ""[]"",
    ""KeywordsDisplay"": """",
    ""Tags"": ""[]"",
    ""IsChecked"": false,
    ""Selection"": 0,
    ""IsEditing"": false,
    ""PrimaryKey"": ""312d1c21-0000-0000-0000-000000000003"",
    ""QueryTerm"": ""item04"",
    ""FilterTerm"": ""item04"",
    ""TagMatchTerm"": """",
    ""Properties"": ""{\r\n  \""Description\"": \""Item04\"",\r\n  \""Tags\"": \""[]\""\r\n}""
  },
  {
    ""Id"": ""312d1c21-0000-0000-0000-000000000004"",
    ""Description"": ""Item02B"",
    ""Keywords"": ""[]"",
    ""KeywordsDisplay"": """",
    ""Tags"": ""[]"",
    ""IsChecked"": false,
    ""Selection"": 0,
    ""IsEditing"": false,
    ""PrimaryKey"": ""312d1c21-0000-0000-0000-000000000004"",
    ""QueryTerm"": ""item02b"",
    ""FilterTerm"": ""item02b"",
    ""TagMatchTerm"": """",
    ""Properties"": ""{\r\n  \""Description\"": \""Item02B\"",\r\n  \""Tags\"": \""[]\""\r\n}""
  }
]";

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting Item03 moved to Index 0"
            );

            // REMOVE (by index for determinism)
            builder.Clear();
            srce.RemoveAt(1);
            Assert.IsTrue(EqualsSrceAndDest());

            actual = string.Join(Environment.NewLine, builder);
            actual.ToClipboardExpected();
            { } // <- FIRST TIME ONLY: Adjust the message.
            actual.ToClipboardAssert("Expecting builder content to match.");
            { }

            // RESET
            srce.Clear();
            Assert.IsTrue(EqualsSrceAndDest());
        }
        void subtest_WithoutProjectionAuthority()
        {
        }
        #endregion S U B T E S T S
    }

    [TestMethod, DoNotParallelize]
    public void Test_GravityAbstract()
    {
        string actual, expected;
        using var te = this.TestableEpoch();

        // Δ Always: ONP -> CSS with Projection authority (If it can be obtained). 
        subtest_Name();

        #region S U B T E S T S
        void subtest_Name()
        {
        }
        #endregion S U B T E S T S
    }

    [TestMethod, DoNotParallelize, Ignore]
    public void Test_Reset()
    {
        string actual, expected;
        using var te = this.TestableEpoch();

        ObservableNetProjection<SelectableQFModel> onp = new();

        // [Careful]
        // - This isn't a subscription to MMDC at all.
        // - In particular, it doesn't make the ONP event show up first in builder.
        onp.CollectionChanged += (sender, e) =>
        {
            builder.Add(e.ToString(ReferenceEquals(sender, onp)));
        };
        var mmdc = new ModeledMarkdownContext<SelectableQFModel>
        {
            ObservableNetProjection = onp,
        };

        #region E V E N T S
        // This is the order that matters:
        // FIRST - subscribe the MMDC direct
        mmdc.ModelChanged += (sender, e) =>
        {
            builder.Add($"{mmdc.Authority.ToString().PadRight(10)} {e.ToString(ReferenceEquals(sender, onp))}" );
        };
        // SECOND - subscribe the MMDC in the Composed collection
        onp.MMDC = mmdc; // Cross-linked ONP will now forware the ModelChanged event.
        #endregion E V E N T S

        subtest_ResetWhenEmpty();
        subtest_ResetNonEmpty();

        #region S U B T E S T S
        void subtest_ResetWhenEmpty()
        {
            builder.Clear();

            // Clear, when already empty.
            mmdc.Clear(all: true);

            actual = string.Join(Environment.NewLine, builder);
            actual.ToClipboardExpected();
            { }
            expected = @"         
Other.Reset   NotifyCollectionChangedEventArgs           
NetProjection.Reset   NotifyCollectionChangedEventArgs   "
            ;

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting events received in order of subscription."
            );
        }

        void subtest_ResetNonEmpty()
        {
            onp.PopulateForDemo(1);
            Assert.AreEqual(1, mmdc.CanonicalCount);

            actual = onp.MMDC.Model.ToString();
            actual.ToClipboardExpected();
            { }
            expected = @" 
<model autocount=""1"" count=""1"" matches=""1"" mmdc=""[MMDC]"">
  <xitem text=""312d1c21-0000-0000-0000-000000000000"" model=""[SelectableQFModel]"" sort=""0"" />
</model>"
            ;
            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting that an arbitrary item is present in order to test the clear."
            );
            builder.Clear();
            // Clear, when non-empty.
            mmdc.Clear(all: true);

            actual = string.Join(Environment.NewLine, builder);
            actual.ToClipboardExpected();
            { }
            expected = @" 
Other.Reset   NotifyCollectionChangedEventArgs           
NetProjection.Reset   NotifyCollectionChangedEventArgs           "
            ;

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting events received in order of subscription."
            );
        }
        #endregion S U B T E S T S
    }

    [TestMethod, DoNotParallelize, Ignore]
    public void Test_Commit()
    {
        string actual, expected;
        using var te = this.TestableEpoch();
        List<SelectableQFModel> recordset = new();

        ObservableNetProjection<SelectableQFModel> onp = new();
        var mmdc = new ModeledMarkdownContext<SelectableQFModel>
        {
            ObservableNetProjection = onp,
        };

        // [Careful]
        // - This isn't a subscription to MMDC at all.
        // - In particular, it doesn't make the ONP event show up first in builder.
        onp.CollectionChanged += (sender, e) =>
        {
            builder.Add($"{mmdc.Authority.ToString().PadRight(10)} {e.ToString(ReferenceEquals(sender, onp))}");
        };

        #region E V E N T S
        // This is the order that matters:
        // FIRST - subscribe the MMDC direct
        mmdc.ModelChanged += (sender, e) =>
        {
            builder.Add($"{mmdc.Authority.ToString().PadRight(10)} {e.ToString(ReferenceEquals(sender, onp))}");
        };
        // SECOND - subscribe the MMDC in the Composed collection
        onp.MMDC = mmdc; // Cross-linked ONP will now forware the ModelChanged event.
        #endregion E V E N T S

        #region S U B T E S T S

        subtest_CanonizeOne();
        void subtest_CanonizeOne()
        {
            recordset.PopulateForDemo(1);

            builder.Clear();
            mmdc.LoadCanon(recordset);

            actual = string.Join(Environment.NewLine, builder);
            actual.ToClipboardExpected();
            { }
            expected = @" 
Commit     NotifyCollectionChangedEventArgs           Other         Reset   
Commit     NotifyCollectionChangedEventArgs           NetProjection Reset   
Commit     NotifyCollectionChangedEventArgs           NetProjection Reset   
Commit     NotifyCollectionChangedEventArgs           Other         Add     NewItems= 1 
Commit     NotifyCollectionChangedEventArgs           NetProjection Add     NewItems= 1"
            ;
            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting 2 events x 2 subscribers (FOUR)"
            );
            Assert.AreEqual(4, builder.Count, "TEMPORARY LIMIT");

            actual = onp.MMDC.Model.ToString();
            actual.ToClipboardExpected();
            { }
            expected = @" 
<model autocount=""1"" count=""1"" matches=""1"" mmdc=""[MMDC]"">
  <xitem text=""312d1c21-0000-0000-0000-000000000000"" model=""[SelectableQFModel]"" sort=""0"" />
</model>"
            ;
            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting that an arbitrary item is present in order to test the clear."
            );
        }
        #endregion
    }

    [TestMethod, DoNotParallelize, Ignore]
    public void Test_Projection()
    {
        string actual, expected;
        using var te = this.TestableEpoch();

        ObservableNetProjection<SelectableQFModel> onp = new();

        // [Careful]
        // - This isn't a subscription to MMDC at all.
        // - In particular, it doesn't make the ONP event show up first in builder.
        onp.CollectionChanged += (sender, e) =>
        {
            builder.Add(e.ToString(ReferenceEquals(sender, onp)));
        };
        var mmdc = new ModeledMarkdownContext<SelectableQFModel>
        {
            ObservableNetProjection = onp,
        };

        #region E V E N T S
        // This is the order that matters:
        // FIRST - subscribe the MMDC direct
        mmdc.ModelChanged += (sender, e) =>
        {
            builder.Add(e.ToString(ReferenceEquals(sender, onp)));
        };
        // SECOND - subscribe the MMDC in the Composed collection
        onp.MMDC = mmdc; // Cross-linked ONP will now forware the ModelChanged event.
        #endregion E V E N T S

        #region S U B T E S T S
        #endregion
    }

    [TestMethod, DoNotParallelize, Ignore]
    public void Test_Settle()
    {
        string actual, expected;
        using var te = this.TestableEpoch();

        ObservableNetProjection<SelectableQFModel> onp = new();

        // [Careful]
        // - This isn't a subscription to MMDC at all.
        // - In particular, it doesn't make the ONP event show up first in builder.
        onp.CollectionChanged += (sender, e) =>
        {
            builder.Add(e.ToString(ReferenceEquals(sender, onp)));
        };
        var mmdc = new ModeledMarkdownContext<SelectableQFModel>
        {
            ObservableNetProjection = onp,
        };

        #region E V E N T S
        // This is the order that matters:
        // FIRST - subscribe the MMDC direct
        mmdc.ModelChanged += (sender, e) =>
        {
            builder.Add(e.ToString(ReferenceEquals(sender, onp)));
        };
        // SECOND - subscribe the MMDC in the Composed collection
        onp.MMDC = mmdc; // Cross-linked ONP will now forware the ModelChanged event.
        #endregion E V E N T S

        #region S U B T E S T S
        #endregion
    }

    [TestMethod, DoNotParallelize, Ignore]
    public void Test_Predicate()
    {
        string actual, expected;
        using var te = this.TestableEpoch();

        ObservableNetProjection<SelectableQFModel> onp = new();

        // [Careful]
        // - This isn't a subscription to MMDC at all.
        // - In particular, it doesn't make the ONP event show up first in builder.
        onp.CollectionChanged += (sender, e) =>
        {
            builder.Add(e.ToString(ReferenceEquals(sender, onp)));
        };
        var mmdc = new ModeledMarkdownContext<SelectableQFModel>
        {
            ObservableNetProjection = onp,
        };

        #region E V E N T S
        // This is the order that matters:
        // FIRST - subscribe the MMDC direct
        mmdc.ModelChanged += (sender, e) =>
        {
            builder.Add(e.ToString(ReferenceEquals(sender, onp)));
        };
        // SECOND - subscribe the MMDC in the Composed collection
        onp.MMDC = mmdc; // Cross-linked ONP will now forware the ModelChanged event.
        #endregion E V E N T S

        #region S U B T E S T S
        #endregion
    }
}
