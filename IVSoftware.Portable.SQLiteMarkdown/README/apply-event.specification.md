# [<](../../README.md)

## `NotifyCollectionChangingEventArgs`

This class is an event change ledger.


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

### Applying a Range Event

Collections that support `IRangeable` often produce `IsBclCompatible` for actions like `AddRange`, but can also feature multi moves or removes that may be jagged or discontiguouous and are therefore escalated to Batch semantics.

### Applying a Batch Event








