# [<](../../README.md)

## `NotifyCollectionChangingEventArgs`

This class is an event change ledger with semantics for applying it to an `IList`.

### `IsBclCompatible`

___

## `Diff` Extension

This method creates an `NotifyCollectionChangingEventArgs` by comparing two lists. Idiomatically, in code, this event is typically represented as `ePre`.

___

## Apply

This method applies a `NotifyCollectionChangingEventArgs` to an `IList`. 

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

### The `Apply` Method Extension for `IList`

Once the `Diff` method has been run to obtain the delta between two collections, it can be passed to the `Apply()` extension:

```
myList.Apply(ePre);

```

The `Diff` method has provided a value for `IsBclCompatible`.

_TRUE_
- The event is converted to a single call is made to the corresponding front-end method.
- No attempt is made to suppress the resulting `CollectionChanged` event.

Any event that is not `IsBclCompatible` be designated an `action: Reset`.