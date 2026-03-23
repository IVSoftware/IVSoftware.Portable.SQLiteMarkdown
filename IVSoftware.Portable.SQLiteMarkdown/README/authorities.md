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
