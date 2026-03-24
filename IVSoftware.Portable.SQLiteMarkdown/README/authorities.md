# [<](../../README.md)

## Authorities

Authority epochs refer to the provenance of any cyclical FSM state that has been entered.

## UI Thread

Changes of state _usually_ occur because of user actions on a visible projection surface. The UI surface that interacts with a `MarkdownContext` (MDC) typically presents a list, grid, or tree-style view. This element is referred to as the `ObservableNetProjection` (ONP) and is intended for binding to the platform-specific control directly. The UI elements that can also include:

- **Input Method Entry (IME)** - A textbox-style control for query search terms that can also change state to serve as a filter to refine a recordset once received.

1. Query standby mode anticipated what could be a long running (think 'cloud') query on a datasource. This requires a manual Commit.
2. Filter mode on a recordset that it already showing. There are fast queries on a local, internal SQLite database, that serve to remodel the filtered collection whenever text changes settle on a watchdog timer (WDT).

- **[X]** - A state-aware Clear that goes in phases until it reaches a terminal all-clear state.
1. Clears the IME text.
2. If filtering, goes back to Query standby mode.  

- **Predicates** - In the form of check boxes (e.g., IsChecked) that might interact with radio buttons (e.g., ShowChecked, ShowUnchecked, ShowAll).

___

## Synchronization thread

Changes to the UI initiated by a non UI thread are less common but can be shown by this example.

1. Local-first databases are cloud synced.
2. Remote user deletes a record that is currently displayed in the ONP.
3. The ONP updates, removing the item after determining that there is no active edit lock on the item.

___

## Asynchronous Operations

The platform-agnostic MDC performs sequential background work under the jurisdiction of its semaphore, and is directly awaitable. 

___

## Canon

The MDC exposes a `LoadCanon(IEnumerable recordset)` method which establishes a baseline model for any subsequent operations that occur inside the recordset's **Epoch**.
___

## Epoch

In Query states, a recordset that is captured as a result of a `Commit()` becomes replaces any existing items and becomes the new canon.
___

## Model

An `XElement` root model tracks changes to the canonical superset. These changes come from one of these sources.

- _Load Canon_ - Existing items are replaced by the new canonical recordset, and the `Model` is reinitialized from it.
- _Add-Remove UI Surface_ - When existing items are permanently removed (i.e., from a master database) interactively.
- _Remote Sync_ - For example, when an item removal is detected in a remote sync operation.

If the item type supports a `FullPath` property, the model is hierarchal and supports a `Depth` property for the item.

The canonical sort order is captured so that ephemeral sorts (e.g. column header clicks) can be reverted.

### Filtering (when Filter flag is present in Config)

The `qmatch` attribute is set based on the internal `FilterQueryDatabase` returning a matching PK for a filter expression.

The `pmatch` attribute is set based on toggles of active predicates.

The `ismatch` attribute is an AND of the other two if either are present, but otherwise defaults to "effectively true".
___

### Query Only

An MDC configured as `QueryFilterConfig.Query` does not advance to a Filter state. The visible surface always shows all of the items, albeit the order and depth can change. These change might be ephemeral (e.g., a column header sort) or permanent (e.g. a drag-drop).
___

### Filter Only

An MDC configured as `QueryFilterConfig.Filter` typically receives an initial `LoadCanon()` and filters on the same items for its lifetime.

___

### Query and Filter

An MDC configured as `QueryFilterConfig.QueryAndFilter` will advance to a filtering state when `LoadCanon()` injects a collection of two or more items.

___

## Topologies

The MDC is designed to interact with a platform-specific collection view, but the functionality of the MDC (which is platform agnostic) does not require one.

Assume that the MDC has a two-way binding to a view. When a user makes a permanent change to the visible surface (i.e. adds or removes an item) this change is pushed onto the `IList` interface. When the model initiates a change (i.e. applies a new filter or predicate) this is broadcast an a `CollectionChanged` event.

From the perspective of the view binding, this takes one of the following shapes:

___

[Claim("{FA7BB019-E8BA-4B81-A4DF-34528279044A}")]
### Discrete `ObservableCollection<T>`

In this topology, the view model exposes a property that is a discrete `ObservableCollection<T>` and binds it in the role of items source. (In other words, this would be a typical arrangement for an app prior to integrating MDC.) The view model then injects that reference into MDC using `MDC.SetNetObservableCollection(ObservableCollection<T>, option)` where, the `option` specifies whether the MDC is allowed to make direct changes to the collection directly.

View <-> ItemsSource = `ObservableCollection<T>` when `MDC.SetNetObservableCollection(ItemsSource, option)`.

#### _Altering the `CanonicalSuperset`_

[Claim("{179C424C-B39D-444E-8AB0-AD567551742F}")]
**UI Flow (Default)**

1. Interactive changes to the visible surface invoke `ItemsSource` (the ONP), raising `CollectionChanged` on the ONP.
2. The MDC internal handler obtains `Projection` authority, and updates the `Model` based on the BCL `NotifyCollectionChangedEventArgs`.
3. If filter aware, the PMSS is synchronized inline.
4. The `ModelChanged` event is raised. This is a before-and-after diff with respect to ONP (which might already be reduced).
5. Holding `Projection` authority means that _no attempt_ will be made to modify eithr ONP or CSS. 

**Commit Flow**
[Claim("{DC169D72-BE19-4A83-8106-EA702664DE8B}")]

**Settle Flow**
[Claim("{CAD5D55D-80DC-46E6-BAE3-46C69A99F8B0}")]

**Predicate Flow**
[Claim("{6E400ED2-537A-40F5-B3FF-ED39CA223680}")]

___

[Claim("{34FC2036-8748-4D91-8DB7-E57934D0A351}")]
### Pure Implementer<T>

In this topology, the view model exposes an instance of `ModeledMarkdownContext<T>` performs the two-way binding directly to it.

Platform-specific collection views typically rely on `IList` and `INotifyCollectionChanged` for the `ItemsSource` contract, and don't really care whether the implementation is (or inherits) `ObservableCollection<T>`.



