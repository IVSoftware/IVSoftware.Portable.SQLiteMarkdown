using IVSoftware.Portable.Collections.Preview;
using IVSoftware.Portable.Common.Exceptions;
using IVSoftware.Portable.SQLiteMarkdown.Common;
using IVSoftware.Portable.SQLiteMarkdown.Internal;
using IVSoftware.Portable.SQLiteMarkdown.Util;
using IVSoftware.Portable.Xml.Linq.XBoundObject;
using IVSoftware.WinOS.MSTest.Extensions;
using Newtonsoft.Json;
using System.Collections.ObjectModel;

namespace IVSoftware.Portable.SQLiteMarkdown.MSTest;

[TestClass]
public class TestClass_260401_OPCv2
{
    [TestMethod, DoNotParallelize]
    [Claim("00000000-0000-0000-0000-000000000000")]
    public void Test_Suppressible()
    {
        string actual, expected;
        var builder = new List<string>();
        using var te = this.TestableEpoch();

        #region I T E M    G E N
        IList<SelectableQFModel>? eph = null;
        // CREATE (no side effects)
        var i1 = eph.AddDynamic("Item01");
        var i2 = eph.AddDynamic("Item02");
        var i3 = eph.AddDynamic("Item03");
        #endregion I T E M    G E N

        var itemsSource = new SuppressibleObservableCollection<SelectableQFModel>();

        #region E V E N T S
        itemsSource.CollectionChanged += (sender, e) =>
        {
            builder.Add(e.ToString(ReferenceEquals(sender, itemsSource)));
        };
        #endregion E V E N T S

        subtest_None();
        subtest_Preview();

        #region S U B T E S T S

        void subtest_None()
        {
            itemsSource.Add(i1);
            itemsSource.Add(i2);
            itemsSource.Add(i3);

            actual = string.Join(Environment.NewLine, builder);
            actual.ToClipboardExpected();
            { }
            expected = @" 
NetProjection.Add     NewItems= 1 NewStartingIndex= 0 NotifyCollectionChangedEventArgs           
NetProjection.Add     NewItems= 1 NewStartingIndex= 1 NotifyCollectionChangedEventArgs           
NetProjection.Add     NewItems= 1 NewStartingIndex= 2 NotifyCollectionChangedEventArgs           ";

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting 3x Add events."
            );

            builder.Clear();
            itemsSource.RemoveAt(2);

            actual = string.Join(Environment.NewLine, builder);
            actual.ToClipboardExpected();
            { }
            expected = @" 
NetProjection.Remove  OldItems= 1 OldStartingIndex= 2 NotifyCollectionChangedEventArgs           "
            ;

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting 1x Remove events."
            );

            builder.Clear();
            itemsSource[1] = i3;

            actual = string.Join(Environment.NewLine, builder);
            actual.ToClipboardExpected();
            { }
            expected = @" 
NetProjection.Replace NewItems= 1 OldItems= 1 NewStartingIndex= 1 OldStartingIndex= 1 NotifyCollectionChangedEventArgs           "
            ;

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting 1x Replace events."
            );

            builder.Clear();
            itemsSource.Move(1, 0);

            actual = string.Join(Environment.NewLine, builder);
            actual.ToClipboardExpected();
            { }
            expected = @" 
NetProjection.Move    NewItems= 1 OldItems= 1 NewStartingIndex= 0 OldStartingIndex= 1 NotifyCollectionChangedEventArgs           "
            ;

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting 1x Move events."
            );

            actual = JsonConvert.SerializeObject(itemsSource, Formatting.Indented);
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
  }
]";

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting list reflects all changes."
            );


            builder.Clear();
            itemsSource.Clear();

            actual = string.Join(Environment.NewLine, builder);
            actual.ToClipboardExpected();
            { }
            expected = @" 
NetProjection.Reset   NotifyCollectionChangedEventArgs           "
            ;

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting 1x Reset events."
            );
        }
        void subtest_Preview()
        {
            builder.Clear();
            using (itemsSource.BeginCoalesce(SuppressionPhase.Preview))
            {
                itemsSource.Add(i1);
                itemsSource.Add(i2);
                itemsSource.Add(i3);
            }

            actual = string.Join(Environment.NewLine, builder);
            actual.ToClipboardExpected();
            { }
            expected = @" 
NetProjection.Reset   NotifyCollectionChangedEventArgs           "
            ;

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting 3x Add events."
            );

            builder.Clear();
            itemsSource.RemoveAt(2);

            actual = string.Join(Environment.NewLine, builder);
            actual.ToClipboardExpected();
            { }
            expected = @" 
NetProjection.Remove  OldItems= 1 OldStartingIndex= 2 NotifyCollectionChangedEventArgs           "
            ;

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting 1x Remove events."
            );

            builder.Clear();
            itemsSource[1] = i3;

            actual = string.Join(Environment.NewLine, builder);
            actual.ToClipboardExpected();
            { }
            expected = @" 
NetProjection.Replace NewItems= 1 OldItems= 1 NewStartingIndex= 1 OldStartingIndex= 1 NotifyCollectionChangedEventArgs           "
            ;

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting 1x Replace events."
            );

            builder.Clear();
            itemsSource.Move(1, 0);

            actual = string.Join(Environment.NewLine, builder);
            actual.ToClipboardExpected();
            { }
            expected = @" 
NetProjection.Move    NewItems= 1 OldItems= 1 NewStartingIndex= 0 OldStartingIndex= 1 NotifyCollectionChangedEventArgs           "
            ;

            actual = JsonConvert.SerializeObject(itemsSource, Formatting.Indented);
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
  }
]";

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting list reflects all changes."
            );
        }
        #endregion S U B T E S T S
    }

    private class SuppressibleObservableCollection<T>
        : ObservableCollection<T>
        , INotifyCollectionChangedSuppressible
    {
        public NotifyCollectionChangeScope EventScope { get; }
        protected override void InsertItem(int index, T item)
        {
            switch (Phase)
            {
                case SuppressionPhase.None:
                case SuppressionPhase.Commit:
                    base.InsertItem(index, item);
                    break;
                case SuppressionPhase.Preview:
                    var ePre = new NotifyCollectionChangingEventArgs(
                        action: NotifyCollectionChangeAction.Add,
                        scope: EventScope,
                        newItems: new[] { item },
                        newStartingIndex: index);
                    if(!DHostCoalesce.TryAppend(ePre))
                    {
                        base.InsertItem(index, item);
                    }
                    break;
                default:
                    this.ThrowFramework<NotSupportedException>($"The {Phase.ToFullKey()} case is not supported.");
                    break;
            }
        }
        protected override void SetItem(int index, T item)
        {
            switch (Phase)
            {
                case SuppressionPhase.None:
                case SuppressionPhase.Commit:
                    base.SetItem(index, item);
                    break;
                case SuppressionPhase.Preview:
                    var ePre = new NotifyCollectionChangingEventArgs(
                        action: NotifyCollectionChangeAction.Replace,
                        scope: EventScope,
                        newItems: new[] { item },
                        oldItems: new[] { this[index] },
                        newStartingIndex: index,
                        oldStartingIndex: index);
                    if (!DHostCoalesce.TryAppend(ePre))
                    {
                        throw new NotImplementedException("ToDo");
                    }
                    break;
                default:
                    this.ThrowFramework<NotSupportedException>($"The {Phase.ToFullKey()} case is not supported.");
                    break;
            }
        }
        protected override void RemoveItem(int index)
        {
            switch (Phase)
            {
                case SuppressionPhase.None:
                case SuppressionPhase.Commit:
                    base.RemoveItem(index);
                    break;
                case SuppressionPhase.Preview:
                    var item = this[index];

                    var ePre = new NotifyCollectionChangingEventArgs(
                        action: NotifyCollectionChangeAction.Remove,
                        scope: EventScope,
                        oldItems: new[] { item },
                        oldStartingIndex: index);
                    if (!DHostCoalesce.TryAppend(ePre))
                    {
                        throw new NotImplementedException("ToDo");
                    }
                    break;
                default:
                    this.ThrowFramework<NotSupportedException>($"The {Phase.ToFullKey()} case is not supported.");
                    break;
            }
        }
        protected override void MoveItem(int oldIndex, int newIndex)
        {
            switch (Phase)
            {
                case SuppressionPhase.None:
                case SuppressionPhase.Commit:
                    base.MoveItem(oldIndex, newIndex);
                    break;
                case SuppressionPhase.Preview:
                    var item = this[oldIndex];

                    var ePre = new NotifyCollectionChangingEventArgs(
                        action: NotifyCollectionChangeAction.Move,
                        scope: EventScope,
                        newItems: new[] { item },
                        oldItems: new[] { item },
                        newStartingIndex: newIndex,
                        oldStartingIndex: oldIndex);
                    if (!DHostCoalesce.TryAppend(ePre))
                    {
                        throw new NotImplementedException("ToDo");
                    }
                    break;
                default:
                    this.ThrowFramework<NotSupportedException>($"The {Phase.ToFullKey()} case is not supported.");
                    break;
            }
        }
        protected override void ClearItems()
        {
            switch (Phase)
            {
                case SuppressionPhase.None:
                case SuppressionPhase.Commit:
                    base.ClearItems();
                    break;
                case SuppressionPhase.Preview:
                    var snapshot = this.ToArray();

                    var ePre = new NotifyCollectionChangingEventArgs(
                        action: NotifyCollectionChangeAction.Reset,
                        scope: EventScope,
                        oldItems: snapshot,
                        oldStartingIndex: -1);
                    if (!DHostCoalesce.TryAppend(ePre))
                    {
                        throw new NotImplementedException("ToDo");
                    }
                    break;
                default:
                    this.ThrowFramework<NotSupportedException>($"The {Phase.ToFullKey()} case is not supported.");
                    break;
            }
        }

        public IDisposable BeginCoalesce(SuppressionPhase phase) => DHostCoalesce.GetToken(phase, this);

        public void CancelCoalesce() => DHostCoalesce.CancelSuppressNotify();
        public SuppressionPhase Phase => DHostCoalesce.Phase;

        public DHostSuppress DHostCoalesce
        {
            get
            {
                if (_dhostCoalesce is null)
                {
                    _dhostCoalesce = new DHostSuppress();
                    _dhostCoalesce.FinalDispose += (sender, e) => OnFinalCoalesce((CoalescingFinalDisposeEventArgs)e);
                }
                return _dhostCoalesce;
            }
        }
        DHostSuppress? _dhostCoalesce = null;

        private void OnFinalCoalesce(CoalescingFinalDisposeEventArgs e)
        {
            OnCollectionChanged((e.Coalesced));
        }
    }
}
