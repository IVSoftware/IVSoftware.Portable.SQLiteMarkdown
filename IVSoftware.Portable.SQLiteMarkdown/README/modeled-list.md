# [<](../../README.md)

# Modeled Observable Collection

`ModeledObservableCollection<T>` is a direct subclass of `ObservableCollection<T>` that has the ability to present itself as a hierarchal `XElement` model, based on a designated `[FullPath]` attribute.

When its `XElement? Model { get; set; }` property is not `null`:

- Changes to the collection are tracked continuously and maintained in the model.
- Changed made to the model have the reciprocal ability to act upon the collection.

The mechanism for controlling the direction is the `ModelDataExchange` or MDX, a disposable element that reverses the authority for a reference-counted epoch that begins with an outer `using` block for `BeginMDX()`.

When its property is set to `null` this synchronization is disabled, and the overhead ceases. Regardless, this (or any) `IList` can me modeled using the extension provided in this library.

```
/// <summary>
/// Builds a canonical XElement model and returns its string form.
/// </summary>
/// <remarks>
/// - Enumerates items and places each by full path into the model tree.
/// - Sets modeling metadata and order; attaches bound model reference.
/// - Optionally applies preview via resolved delegate.
/// - Throws if path or placement result is invalid.
/// </remarks>
public static string ToString(this IList @this, out XElement model){...}
```
___

## ModelDataExchange (MDX)

The MDX has two functions:

- Control the update direction.
- Determine whether changes are immediate or deferred.

The second function carries a kind of superpower - the ability to accumulate intermediate collection change events and algorithmically coalesce them. The work product is either a single, BCL-compatible multi event, or a non-compatible `Add` event where the `NewItems` consist of an event playlist. 

Rehydrating `collectionBefore` to `collectionAfter` is performed by calling the `Apply` extension, which accepts either `NotifyCollectionChangedEventArgs` (from the the BCL) or  `NotifyCollectionChangingEventArgs` (from this library).

```
/// <summary>
/// Applies a normalized collection change to a list target.
/// </summary>
/// <remarks>
/// - Mirrors model application semantics for IList-backed projections.
/// - Uses normalized action and payload for consistent mutation behavior.
/// - Serves as the projection-side execution counterpart to model updates.
/// </remarks>
public static void Apply(this IList list, EventArgs eUnk)
```
___

## The Basis of Modeling

The ability to model an arbitrary object (and its containing collection) requires knowing its full path which is:

- Discovered, heuristically, using reflection.
- Invoked as a cached compiled delegate thereafter.








