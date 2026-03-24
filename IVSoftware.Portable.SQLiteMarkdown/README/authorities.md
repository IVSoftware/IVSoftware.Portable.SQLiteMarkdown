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

As policy, the `ObservableNetCollection` and the `CanonicalSupersetInternal` are set to the same reference.
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

### Discrete `ObservableCollection<T>`

In this topology, the view model exposes a property that is a discrete `ObservableCollection<T>` and binds it in the role of items source. (In other words, this would be a typical arrangement for an app prior to integrating MDC.) The view model then injects that reference into MDC using `MDC.SetNetObservableCollection(ObservableCollection<T>, option)` where, the `option` specifies whether the MDC is allowed to make direct changes to the collection directly.

View <-> ItemsSource = `ObservableCollection<T>` when `MDC.SetNetObservableCollection(ItemsSource, option)`.

#### _Altering the `CanonicalSuperset`_

**UI Flow**

1. Interactive changes to the visible surface invoke `ItemsSource` (the ONP), raising `CollectionChanged` on the ONP.
2. The MDC internal handler obtains `Projection` authority, and updates the `Model` based on the BCL `NotifyCollectionChangedEventArgs`.
3. If filter aware, the PMSS is synchronized inline.
3. The `ModelChanged` event is raised. This is a before-and-after diff with respect to ONP (which might already be reduced).

___









- The MDC itself (`INotifyCollectionChanged` via `ModeledMarkdownContext<T>`)
- An `ObservableCollection<T>` (or more likely an `ObservablePreviewCollection<T>`) injected as the ONP



In the first case, the `IList` interface can be invoked be invoked

___

## Authority Scopes

This section details what takes place in each authority scope, what causes one to be entered, and references unit tests by the reference guid for each stated policy. Some may be familiar with the MFC paradigm of Dialog Data Exchange (DDX) where there is a component of directionality, and this documentation will borrow the acronym and use it loosely to indicate when one agent of change needs to interact with others in a manner that does not create circularity.

The "net" of ONP implies that there are "other" backing stores that contribute to the current population. Here's a listing of the _local_ epoch-driven sources.

**CanonicalSuperset** (or CSS which is unambiguous in this context)
This collection represents the stack that is pushed when the system goes into Filtering mode. The ONP - the collection being viewed - is obviously not canonical, but at the same time must be revertable to the full unfiltered set. CSS _never_ raises `ModelChanged` on its own.

**Model** Represents a hierarchy based on the `FullPath` property (if available) or the `Id` property (required - maps to the PK of the model). The model tracks CSS and always has an identical population of items. Whenever IME text changes settle, the `fmatch` attribute updates to reflect whether the item matches the new filter. When predicate sources toggle, the `pmatch` attribute updates to reflect whether the item matches the new predicate. Whenever `fmatch` or `pmatch` changes, the `ismatch` attribute updates to reflect whether the item satisfies both conditions.


**PredicateMatchSubset** (PMSS) represents the selection of items from the Model where `ismatch`? == true. When PMSS collection changes, it raises ModelChanged. This includes model initialization in response to LoadCanon.

___
_There is also a wildcard situation where a remote synchronization might, for example, permanently remove an item from contention._
___

The quiescent state of MDC has no authority.

___

### Projection Authority - Policies

The Projection authority represents the state that is most like a normal observable collection bound to a typical platform-specific collection view. When UI actions like [Add] and [Delete] are available on the selection, the user has the ability to modify the canonical backing stores permanently and directly.

___
In the first case, when the `IList` interface is invoked.

Δ `CanonicalSuperset` (CSS) raises its own `CollectionChanged` 
-> Model
-> PredicateMatchSubset
-> ModelChanged
-> `MDC.CollectionChanged` 

The question now is whether ModelChanged should be applied to ONP which is one of:
- Null (Inherited option)
- ObservableOnly
- AllowDirectChanges
___
In the second case, the platform-specific collection view is already listening to `MDC.CollectionChanged`.

___
These persistent insert or remove operations are straightforward when the full list is shown, but when the ONP is already filtered heuristics are required to map new items in terms of the likely intent with respect to items that are currently hidden. That is, when those items are made visible again, the ordering should feel intuitive when placed alongside any new items.

___
{45E71CC4-7F5D-467E-9758-2B3DD7D55F00}
**Permanent ONP changes must propagate to the CSS.**

This policy assumes a data binding between the platform-specific collection view and the ONP collection itself. In other words, adding or removing items from the UI will raise `CollectionChanged` events that the MDC will be listening for.

### Circularity

Δ ONP -> CollectionChanged  -> CSS: 

The handler must immediately attempt to gain Projection authority and then apply the change ledger (the received event) to the CSS.

Δ CSS !-> ModelChanged -> ONP

The `ModelChanged` event is raised when either the CSS _or_ the PMSS changes. In this case, these changes are driven by the projection, not the model itself. ONP is *always updated* when `ModelChanged` is raised, so CSS and PMSS must be responsible for _not_ raising the event when Projection has the authority.
___

{B96F0C73-300B-4EBB-BCBF-02833A61493C}
**An existing item removed from CSS must be removed from PMSS.**

Δ CSS -> ModelChanged -> PMSS

This only happens after a permanent live deletion. The item is gone, never to return.
___

{BEBFB589-1ABD-4B19-85A9-DD25334FC3B7}
**A new item added to the CSS must be added to the Model (and be considered a match for the duration of the epoch).**

Δ CSS -> ModelChanged -> Model

This only happens after a permanent live addition, and regardless of filters or predicates *must be granted match status* to avoid having the new item "disappear on commit" in the case of a mismatch. So, until a new filter text change settles, or until predicate sources are toggled, the MDC assumes that "The user made the new item and they want to see it in the collection".

___

{77B4F3BA-8090-4957-992A-40338ADD22CA}
**The `<model count="n">` attribute must always reflect the CSS count. 

Δ CSS -> CollectionChanged -> `<model count="n">`

___

{D0A3F2E2-1CA3-447C-A6B6-CF3A9EBCDD15}

**The `<model matches="n">` attribute must always reflect the PMSS count. 


Δ PMSS -> CollectionChanged -> `<model matches="n">`










