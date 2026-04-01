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



