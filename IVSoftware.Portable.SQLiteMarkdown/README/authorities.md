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

## Authority Epochs

At rest, MDC has no authority. Authority is established only within an active epoch, and determines how changes propagate between the view (ONP), the canonical superset (CSS), and the model.

The system operates over three coordinated structures:
- **CanonicalSuperset (CSS)** — the authoritative recordset for the current epoch.
- **Model** — a structural representation of CSS, tracking match state and hierarchy.
- **PredicateMatchSubset (PMSS)** — the currently visible subset based on filter and predicate evaluation.

Authority within an epoch governs:
- where mutations are applied first (ONP vs CSS),
- whether intermediate changes are suppressed or propagated,
- and what final `INotifyCollectionChanged` shape is emitted.

To avoid circularity, the active authority must suppress feedback from downstream structures. For example, when the UI (projection) initiates a change, it is applied to CSS, but resulting model updates must not re-trigger changes back onto the originating projection.

The flows described in the following sections (UI, Commit, Settle, Predicate) are expressions of this authority under different epoch types.

___

## Topologies

The MDC is platform-agnostic and does not require a UI surface, but it is designed to interoperate with one.

In most applications, a view is already bound to a collection (e.g., `ItemsSource = {Binding Items}`), where `Items` is typically an `ObservableCollection<T>`. That established binding is the starting point.

This section examines how MDC integrates into that existing shape. Each topology reflects a different way of satisfying the `ItemsSource` contract (`IList` + `INotifyCollectionChanged`), and explores the tradeoffs between preserving the familiar `ObservableCollection<T>` pattern versus treating the collection as a routed projection over a canonical model.

From the perspective of the view binding, this takes one of the following shapes:

___

[Claim("{FA7BB019-E8BA-4B81-A4DF-34528279044A}")]
### Discrete `ObservableCollection<T>`

[Claim("{FA7BB019-E8BA-4B81-A4DF-34528279044A}")]
### Discrete `ObservableCollection<T>`

In this topology, the view model exposes a property that is a discrete `ObservableCollection<T>` and binds it as the `ItemsSource`. This reflects the typical arrangement prior to integrating MDC.

MDC is introduced by injecting that existing instance via `MDC.SetNetObservableCollection(ObservableCollection<T>, option)`, where `option` determines whether MDC may apply direct mutations to the collection.

This approach preserves established behaviors—including override points such as `InsertItem`, `SetItem`, and `ClearItems` - and provides a low-friction entry point for adopting MDC without restructuring the view model.From the perspective of the view, interaction remains anchored to the `ObservableCollection<T>`:

View <-> ItemsSource = `ObservableCollection<T>`  
when `MDC.SetNetObservableCollection(ItemsSource, option)`

The view continues to interact exclusively with the `ObservableCollection<T>` instance, even after it has been registered with MDC.

#### _Altering the `CanonicalSuperset`_

[Claim("{179C424C-B39D-444E-8AB0-AD567551742F}")]
**Projection Authority (UI)**
1. Interactive changes to the visible surface invoke `ItemsSource` (the ONP), raising `CollectionChanged` on the ONP.
2. The MDC internal handler obtains `Projection` authority, and updates the `Model` based on the BCL `NotifyCollectionChangedEventArgs`.
3. If filter aware, the PMSS is synchronized inline.
4. The `ModelChanged` event is raised. This is a before-and-after diff with respect to ONP (which might already be reduced).
5. Holding `Projection` authority means that _no attempt_ will be made to modify eithr ONP or CSS. 

[Claim("{DC169D72-BE19-4A83-8106-EA702664DE8B}")]
**Commit Authority**

[Claim("{CAD5D55D-80DC-46E6-BAE3-46C69A99F8B0}")]
**Settle Authority**

[Claim("{6E400ED2-537A-40F5-B3FF-ED39CA223680}")]
**Predicate Authority**

___

[Claim("{34FC2036-8748-4D91-8DB7-E57934D0A351}")]
### Pure Implementer<T>

Many platform-specific collection views rely on `IList` and `INotifyCollectionChanged` for the `ItemsSource` contract, but do not require `ObservableCollection<T>` specifically. This topology implements that contract directly, eliminating the standalone `ObservableCollection<T>` intermediate layer.

**Pro**  
Rather than copying items into a canonical backing store when entering `IsFiltering` (and restoring them when exiting), the collection routes between canonical and filtered views. This avoids push/pop synchronization, reduces churn, and reflects the current state without reconstruction.

**Con**  
This approach does not inherit from `ObservableCollection<T>`, so override points such as `InsertItem`, `SetItem`, and `ClearItems` are not available. Custom mutation logic must be implemented explicitly rather than relying on BCL hooks.

From the perspective of the view, interaction is anchored to the contract rather than a concrete type:

View <-> ItemsSource = MDC
where: `MDC : IList, INotifyCollectionChanged`  
when ItemsSource is not `ObservableCollection<T>`

The view continues to interact with the collection through standard BCL contracts, without requiring `ObservableCollection<T>` as an intermediate.

#### _Altering the `CanonicalSuperset`_

[Claim("{179C424C-B39D-444E-8AB0-AD567551742F}")]
**Projection Authority (UI)**

[Claim("{DC169D72-BE19-4A83-8106-EA702664DE8B}")]
**Commit Authority**

[Claim("{CAD5D55D-80DC-46E6-BAE3-46C69A99F8B0}")]
**Settle Authority**

[Claim("{6E400ED2-537A-40F5-B3FF-ED39CA223680}")]
**Predicate Authority**