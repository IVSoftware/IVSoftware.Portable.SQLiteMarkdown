# [<](../../README.md)

## `NotifyCollectionChangingEventArgs`

This class is an event change ledger with semantics for applying it to an `IList`.

### `IsBclCompatible`

When true, this event is isomorphic - translatable to a standard `INotifyCollectionChanged` notification without loss, reinterpretation, or structural expansion.
___

## `Diff` Extension

The `ilistBefore.Diff(ilistAfter)` method creates an `NotifyCollectionChangingEventArgs` by comparing the argument to the receiver. Idiomatically, in code, the returned event is typically referred to `ePre`, whereas any BCL `NotifyCollectionChangedEventArgs` produced in the flow are referred to as `ePost` to avoid ambiguity.
___

### The `Apply()` Method Extension for `IList`

Once the `Diff` method has been run to obtain the delta between two collections, it can be passed to the `Apply()` extension:

### Minimal Example

The baseline example is:

- The `ePre` is `IsBclCompatible`.
- The `IList` is `INotifyCollectionChanged` and is not `INotifyCollectionChanging`

When a `IsBclCompatible` event is applied, the target collection raises its `CollectionChanged` event.

### Applying a Non-Compatible Event

When a projection surface is displaying a predicate-matched subset of the canonical recordset, and that filter is removed, the `Diff` between the filtered version and the full version it typically not `IsBclCompatible` and the `ePre` produced is a playlist of mixed `Add` and `Replace` events.

These events are therefore played back one at a time, and will produce a corresponding `CollectionChanged` event for each one. In spite of the churn, this isn't really a problem because the collection (i.e., the UI control that is bound to this collection) turns out all right in the end. However, many of these same controls will respond more efficiently to a single Reset event on a non-empty collection.

___

_MENTAL MODEL - Reducing churn for a collection that implements filtering capabilities is **clearly preferred**._
___

### Applying a Range Event

Range events can produce the same kind of churn. 

Collections that support `IRangeable` often produce `IsBclCompatible` for actions like `AddRange`, but can also feature multi moves or removes that may be jagged or discontiguouous and are therefore escalated to a non-compatible event and Batch semantics.

There is, however, a major distinction to be made. When range methods are exposed, the mental model is, "This will minimize churn or eliminate it altogether". 

___

_MENTAL MODEL - Reducing churn for a collection that implements range capabilities is a **mandate**._

___

### Internal Flow for the `Apply()` Method Extension for `IList`

Once the `Diff` method has been run to obtain the delta between two collections, it can be passed to the `Apply()` extension:

```
myList.Apply(ePre);
```

The `Diff` method has provided a value for `ePre.IsBclCompatible`, indicating whether the event is isomorphic - that is, translatable to a standard `INotifyCollectionChanged` notification without loss, reinterpretation, or structural expansion. Even when this property is `true` it comes with a shading in terms of the `Apply()` method because (for example) a multi-term `Add` needs to be replayed one item at a time. Given that a filtered list _prefers_ to avoid this churn, and a ranged collection _demands_ it, the internal strategy is to attempt a cast to `INotifyCollectionChangedSuppressable` which is available in this library.

```
internal interface INotifyCollectionChangedSuppressible
{
    /// <summary>
    /// Increments the ref count for the suppression epoch.
    /// </summary>
    /// <remarks>
    /// When the ref count returns to zero, disposal raises a final event
    /// with a coalesced <see cref="NotifyCollectionChangingEventArgs"/> instance.
    /// </remarks>
    IDisposable BeginSuppressNotify();

    /// <summary>
    /// Sets an internal flag indicating that the final emission for the current
    /// suppression epoch should include a void marker.
    /// </summary>
    /// <remarks>
    /// This method does not terminate the suppression scope or affect the reference
    /// count. Disposal proceeds normally via the <see cref="IDisposable"/> tokens
    /// returned by <see cref="BeginSuppressNotify"/>. Instead, it alters the semantics
    /// of the final emission, signaling that the coalesced result should be disregarded.
    /// </remarks>
    void CancelSuppressNotify();
}
```

A canonical implementation will cast `ePre` to a BCL `NotifyCollectionChangedEventArgs` instance and raise it as `OnCollectionChanged(ePost)`, and an `ePre` where `IsBclCompatible` == `false` will simply cast to `action: Reset`. Most UI controls bound to this collection will repopulate the projection surface, effectively syncing to the new reality of the collection (even - or especially - if this new reality comes from replaying a Batch event list).

Another indication that `ePre` is an (incompatible) batch is that the `NewItems` property is populated with an `IList<EventArgs>`.

___

## Preview Semantics

Collections may also implement `INotifyCollectionChanging` as a separate concern.

```
internal interface INotifyCollectionChanging
{
    public event EventHandler<NotifyCollectionChangingEventArgs>? CollectionChanging;

    /// <summary>
    /// Defines the extent to which a preview handler may interact with a pending
    /// collection change proposal.
    /// </summary>
    /// <remarks>
    /// This enumeration constrains what a handler is permitted to do during the
    /// preview (Changing) phase. It does not describe the change itself, but rather
    /// the allowed level of participation in shaping or rejecting it.
    ///
    /// - ReadOnly   : Observe only. No modification or cancellation is permitted.
    /// - CancelOnly : The proposal may be rejected but not altered.
    /// - FullControl: The proposal may be rewritten or rejected entirely.
    ///
    /// These flags are enforced by the preview pipeline. Handlers opting into
    /// higher scopes assume responsibility for producing a valid and internally
    /// consistent change contract.
    /// </remarks>
    NotifyCollectionChangeScope EventScope { get; }
}
```

### Canonical Implementation

The core overridable surface of `ObservableCollection<T>` (i.e., where behavior is actually shaped) is small and intentional:

```
protected override void InsertItem(int index, T item);
protected override void SetItem(int index, T item);
protected override void RemoveItem(int index);
protected override void MoveItem(int oldIndex, int newIndex);
protected override void ClearItems();
```

In each override, the `CollectionChanging` event is raised with the proposed change, which is always single except in the case of `ClearItems`. Typically, this is wrapped by `protected virtual OnCollectionChanging(ePre)`. The move here is usually straightforward - Look at the `Cancel` property before allowing the base class method to be invoked.

If the collection implements `ICollectionChangeSuppressible` or `IRangeable : ICollectionChangeSuppressible`, it requires the extra step of checking the suppression flag. For simplicity, this flag can be implemented using the `DHostCoalescingCollectionChange` reference counter class.

```
DHostCoalescingCollectionChange provider { get; } = new();
```

Then each overridden method will first generate an appropriate preview event `ePre` before invoking `provider.TryAppend(ePre)`;

- If the return value is false, execute normally (as described above).
- However, the override should early return if the value is `true`.

Eventually, disposal of the token(s) will raise `DHostCoalescingCollectionChange.FinalDispose(sender, e)` where `e.Coalesced` contains a meta event for reconstructing the intended transformation. What this means is that the `NewItems` property of this change event is itself a collection of the accumulated single events. Essentially, this is an event "playlist" that will produce the intended transformation.

___

### Example #1 - Reverting a Filtered Subset

Suppose a minimal collection holds {1A, 2B, 3A, 4B, 5A } and then is filtered for "A" to produce {1A, 3A, 5A }. The Canonical->Filtered transition requires two `action: Remove` events and is a valid BCL event for a multiple remove.

Now, consider the inverse playlist.

Index 0: Idempotent
Index 1: Replace 3A with 2B
Index 2: Replace 5A with 3A
Index 3: Add 4B
Index 4: Add 5A.



___

### Example #2 - Property Changed Notifications for Collection Items

When changes are occurring, the typical consumer of `INotifyCollectionChanging` wants a list of items being changed _before_ they're changed, and this is especially true of a `Clear()` resulting in an `action: Reset`.

The classic example is when a collection opts in to `INotifyPropertyChanged` messages on the items it contains, where the normal reset event args instance doesn't carry the infomation needed to unsubscribe. One _could_ do this in the overrides of `ObservableCollection<T>`. In fact, if that's the only objective then that offers a pristine approach. Generally, however, the inclusion of `INotifyCollectionChanging` implies an ability to _cancel_ proposed changes. 

```
/// <summary>
/// Defines the extent to which a preview handler may interact with a pending
/// collection change proposal.
/// </summary>
/// <remarks>
/// This enumeration constrains what a handler is permitted to do during the
/// preview (Changing) phase. It does not describe the change itself, but rather
/// the allowed level of participation in shaping or rejecting it.
///
/// - ReadOnly   : Observe only. No modification or cancellation is permitted.
/// - CancelOnly : The proposal may be rejected but not altered.
/// - FullControl: The proposal may be rewritten or rejected entirely.
///
/// These flags are enforced by the preview pipeline. Handlers opting into
/// higher scopes assume responsibility for producing a valid and internally
/// consistent change contract.
/// </remarks>
[Flags]
internal enum NotifyCollectionChangeScope
{
    /// <summary>
    /// Observe the proposal without modifying or canceling it.
    /// </summary>
    ReadOnly = 0x0,

    /// <summary>
    /// Allows the proposal to be canceled but not modified.
    /// </summary>
    CancelOnly = 0x1,

    /// <summary>
    /// Allows full control over the proposal, including rewriting or canceling it.
    /// </summary>
    FullControl = 0x3,
}
```