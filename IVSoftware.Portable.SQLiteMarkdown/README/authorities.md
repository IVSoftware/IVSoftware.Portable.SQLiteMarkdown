# [<](../../README.md)

## Authorities

Authority epochs refer to the provenance of any cylclical FSM state that has been entered.

### UI Thread

Changes of state occur, nearly all of the time, by user actions on a visible projection surface. The UI surface that interacts with a `MarkdownContext` (MDC) typically presents a list, grid, or tree-style view. This element is referred to as the `ObservableNetProjection` (ONP) and is intended for binding to the platform-specific control directly. The UI elements that can also include:

- **Input Method Entry (IME)** - A textbox-style control for query search terms that can also change state to serve as a filter to refine a recordset once received.

1. Query standby mode anticipated what could be a long running (think 'cloud') query on a datasource. This requires a manual Commit.
2. Filter mode on a recordset that it already showing. There are fast queries on a local, internal SQLite database, that serve to remodel the filtered collection whenever text changes settle on a watchdog timer (WDT).

- **[X]** - A state-aware Clear that goes in phases until it reaches a terminal all-clear state.
1. Clears the IME text.
2. If filtering, goes back to Query standby mode.  

- **Predicates** - In the form of check boxes (e.g., IsChecked) that might interact with radio buttons (e.g., ShowChecked, ShowUnchecked, ShowAll).

___

### Synchronization thread

In most scenarios, changes to the UI initiated by a non UI thread are rare but are shown by this example.

1. Local-first databases are cloud synced.
2. Remote user deletes a record that is currently displayed in the ONP.
3. The ONP updates, removing the item after determining that there is no active edit lock on the item.

___

### Asynchronous Operations

The platform-agnostic MDC performs sequential background work under the jurisdiction of its semaphore, and is directly awaitable. 

___

## Authority Scopes

This section details what takes place in each authority scope, what causes one to be entered, and references unit tests by the reference guid for each stated policy. Some may be familiar with the MFC paradigm of Dialog Data Exchange (DDX) where there is a component of directionality, and this documentation will borrow the acronym and use it loosely to indicate when one agent of change needs to interact with others in a manner that does not create circularity.

The quiescent state of MDC has no authority.

___

### Projection

This represents the state that is most like a normal observable collection bound to a typical platform-specific collection view. When UI actions like [Add] and [Delete] are available on the selection, the user has the ability to modify the canonical backing stores permanently and directly.

Permanent insert or remove operations are straightforward when the full list is shown, but when the ONP is already filtered heuristics are required to map new items in terms of the likely intent with respect to items that are currently hidden. That is, when those items are maded visible again, the ordering should feel intuitive when placed alongside any new items.



