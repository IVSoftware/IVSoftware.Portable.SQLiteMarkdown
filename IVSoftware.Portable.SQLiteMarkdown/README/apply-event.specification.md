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

### Canonical Implementation


### Minimal Example

When changes are occurring, the typical consumer of `INotifyCollectionChanging` wants a list of items being changed _before_ they're changed, and this is especially true of a `Clear()` resulting in an `action: Reset`. The classic example is when a collection opts in to `INotifyPropertyChanged` messages on the items it contains, where the normal reset event args instance doesn't carry the infomation needed to unsubscribe.










