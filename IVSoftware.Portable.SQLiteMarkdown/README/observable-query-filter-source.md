## Observable Query Filter Source

This class is a functional drop-in replacement for ObservableCollection&lt;T&gt; and supports `INotifyCollectionChanged`, but it does not inherit from it. Instead, it wraps two collections - one consisting of all items (unfiltered collection) and one consisting of a subset of the full recordset. The routing between there two collections is managed by state variables.

On one hand, this portable class is completely decoupled from any platform-specific UI controls or views. At the same time, it acknowledges the probable and almost-certain existence of 'some' UI and unapologetically supports the most likely configurations of a UI with these state variables.

Although it's essentially a query-then-filter state machine, it can be freely used to "just query" or "just filter". The basic premise is that the developer has an arbitrary data connection and will make a first-pass query of it to provide a baseline recordset to OQFS. Then, with the full dataset displayed in the platform-specific collection view, the same input text field is repurposed and in as state where subsequent text entry filters the items in the view instead of initiating a new query.

## How It Works

`ObservableQueryFilterSource<T>` manages a seamless **query-then-filter** workflow using a built-in state machine. It allows flexible integration with any UI while letting you keep full control over when and how remote queries are executed.

### Step 1: Model Setup

Filtering is driven by lightweight attribute decoration on your model properties:

```csharp
public class Contact
{
    [PrimaryKey]
    public string Id { get; set; }

    [QueryLikeTerm]
    public string Name { get; set; }

    [QueryLikeTerm]
    public string Email { get; set; }
}
```

> For full details on supported syntax and query translation, see [Expression Parsing & SQL Translation](./SQLiteMarkdown.md).

---

### Step 2: Query and Filter Lifecycle

Internally, the class tracks two distinct state models:

- `SearchEntryState` describes the current state of the search input text
- `FilteringState` describes the readiness and activity of in-memory filtering

These expose signals you can use for UI feedback (e.g., icons, colors, messages).

#### Initial Entry

- The user types a query into the search field.
- Once typing has paused, the control raises the `InputTextSettled` event.
- This is a neutral notification: it does not execute a query or change state by itself.
- You choose whether to respond to this event (e.g., trigger a remote query on Enter only).

#### Query Execution

- If you decide to proceed (e.g., from `InputTextSettled` or ReturnKey handler), you:
  - Parse the query using the input text
  - Apply the resulting SQL to your own database
  - Pass the results to `ReplaceItems(resultSet)`

#### FilteringState.Armed

- After `ReplaceItems()` is called:
  - The internal backing store is populated
  - Filtering is now armed but not yet active
- This is indicated by `FilteringState.Armed`
- You may reflect this in the UI by showing a funnel icon or accent color

#### FilteringState.Active

- After a result set has been loaded and the user modifies the input:
  - Filtering is triggered after a debounce interval
  - The list is filtered in memory using the parsed expression
  - `FilteringState` transitions to `Active`

Filtering works implicitly — no manual action is required (IJW) — no manual trigger is required. The debounce delay can be customized, but filtering is always asynchronous and deliberate.

#### FilteringState.Ineligible

- Filtering is skipped if:
  - It is globally disabled
  - Fewer than two unfiltered records exist
- In this case, filtering does not activate even if input changes

---

### Step 3: Clear Button Behavior

The search field supports a two-stage clear interaction pattern:

- First click on the clear `[x]` button:
  - Clears the text field
  - Leaves the current results list in place
  - Filtering is disarmed, but the query results remain visible

- Second click (with the field already empty):
  - Clears the result set completely
  - Resets both state machines
  - `SearchEntryState` returns to `Cleared`
  - `FilteringState` becomes `Ineligible` or idle
  - System is ready for a new query cycle

This avoids accidental result loss while giving the user clear control over when to reset the search context.

---

### Benefits

- Fully decoupled from UI frameworks — works with MAUI, WPF, Blazor, etc.
- Compatible with any data backend — SQLite, MySQL, Entity Framework, REST API
- No subclassing required — filtering logic is declared with simple attributes
- Query and filter expression are stored as part of internal state
- Seamless integration with app navigation — persistent search state per collection view
- UI can observe state transitions (e.g., filtering armed vs active) for visual feedback
- Debounced filtering with developer-defined timing

___
 
___

### Awaitable Semantics

`ObservableQueryFilterSource<T>` is directly awaitable. Awaiting an instance pauses execution while the system is considered *busy*—this begins with the first character typed (regardless of whether it leads to a queryable state) and continues until input has settled.

If input settling triggers a query, the system remains busy until that query completes and the result set is loaded. This allows you to coordinate with async flows:

```csharp
await items;
```

> **Note:** If you use `ReplaceItemsAsync()`, it automatically completes the busy cycle. You do not need to call `.Release()` manually.

___

### Spinning Busy from User Code

If your query or processing logic takes time (e.g., calling a remote API), you can manually indicate that the system is busy using a token:

```csharp
using (items.DHostBusy.GetToken())
{
    var results = await MyRemoteQueryAsync();
    await items.ReplaceItemsAsync(results);
}
```

This ensures:
- `items.Busy == true` while your operation is active
- The `await items;` mechanism waits until both input settles **and** your async logic completes
- State transitions (`SearchEntryState`, `FilteringState`) remain correct and observable

This is the preferred way to keep UI or consumers in sync with ongoing user-driven queries.
___


### Common Pattern: Shared NavSearchBar

In many apps, a shared `NavSearchBar` (NSB) is used to support multiple collection views, each activated through navigation.

Each view typically owns its own view model, and that view model contains an instance of `ObservableQueryFilterSource<T>` (OQFS) for its dataset.

While the view model could track the active expression and filtering state explicitly, `OQFS` already maintains the current input expression and state internally. This allows the NSB to be easily reconfigured when returning to a given view — no need for separate state plumbing.

> **Note:** NavSearchBar is not provided directly; this pattern assumes a shared search input routed through the view model’s InputText. You can also browse this repo for tests like Test_DemoFlow() to see this pattern in action.

This pattern supports:

- A one-to-many relationship between a shared NSB and multiple views
- Automatic restoration of the correct input expression for each dataset
- Consistent and predictable UX across navigable data contexts

