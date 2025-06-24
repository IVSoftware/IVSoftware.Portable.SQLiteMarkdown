## Observable Query Filter Source

This class is a drop-in replacement for ObservableCollection&lt;T&gt; and supports `INotifyCollectionChanged`, but it does not inherit from it. Instead, it wraps two collections - one of consisting of all items (unfiltered collection) and one consisting of a subset of the full recordset. The routing between there two collections is managed by state variables.

On one hand, this portable class is completely decoupled from any platform-specific UI controls or views. At the same time, it acknowledges the probable and almost-certain existence of 'some' UI and unapologetically supports the most likely configurations of a UI with these state variables.

Although it's essentially a query-then-filter state machine, it can be freely used to "just query" or "just filter". The basic premise is that the developer has an arbitrary data connection and will make a first-pass query of it to provide a baseline recordset to OQFS. Then, with the full dataset displayed in the platform-specific collection view, the same input text field is repurposed and in as state where subsequent text entry filters the items in the view instead of initiating a new query.

## How It Works

`ObservableQueryFilterSource<T>` manages a seamless **query-then-filter** workflow using a built-in state machine. It allows flexible integration with any UI while letting you keep full control over when and how remote queries are executed.

### Step 1: Model Decoration

Filtering behavior is driven by simple attribute annotations on your model properties — no subclassing required.

```csharp
public class Contact
{
    [PrimaryKey]
    public string Id { get; set; }

    [SqlLikeTerm]
    public string Name { get; set; }

    [SqlLikeTerm]
    public string Email { get; set; }
}
```

This enables SQL-style parsing for:
- Logical operators: AND, OR, NOT
- Tag expressions: [tag]
- Exact matches with quotes
- Escaped characters and unmatched bracket handling
- Partial or imbalanced input during typing

Additional attribute types like `[FilterContainsTerm]` and `[TagMatchTerm]` are available for alternative filtering strategies.

### Example Expressions

Once decorated, your model participates in expression parsing using a SQLite-Markdown-style language.

> In the example model above, both `Name` and `Email` are marked with `[SqlLikeTerm]`.  
> That means each search term is matched against **both** fields, joined with `OR`.

Here are some examples of input expressions and how they are translated:

| Input Expression       | SQL Translation (simplified)                                                                 | Description                         |
|------------------------|----------------------------------------------------------------------------------------------|-------------------------------------|
| `a b`                  | `(Name LIKE '%a%' OR Email LIKE '%a%') AND (Name LIKE '%b%' OR Email LIKE '%b%')`           | Implicit AND                        |
| `a | b`                | `(Name LIKE '%a%' OR Email LIKE '%a%') OR (Name LIKE '%b%' OR Email LIKE '%b%')`           | OR operator                         |
| `a !b`                 | `(Name LIKE '%a%' OR Email LIKE '%a%') AND NOT (Name LIKE '%b%' OR Email LIKE '%b%')`       | AND with NOT                        |
| `!a`                   | `NOT (Name LIKE '%a%' OR Email LIKE '%a%')`                                                 | Single NOT                          |
| `'exact phrase'`       | `(Name LIKE '%exact phrase%' OR Email LIKE '%exact phrase%')`                               | Exact match using single quotes     |
| `"exact phrase"`       | `(Name LIKE '%exact phrase%' OR Email LIKE '%exact phrase%')`                               | Exact match using double quotes     |
| `!(a | b)`             | `NOT ((Name LIKE '%a%' OR Email LIKE '%a%') OR (Name LIKE '%b%' OR Email LIKE '%b%'))`     | Negated group                       |
| `a & b`                | `(Name LIKE '%a%' OR Email LIKE '%a%') AND (Name LIKE '%b%' OR Email LIKE '%b%')`           | Explicit AND                        |
| `a &&& b`              | `(Name LIKE '%a%' OR Email LIKE '%a%') AND (Name LIKE '%b%' OR Email LIKE '%b%')`           | Redundant AND syntax normalized     |
| `a || b`               | `(Name LIKE '%a%' OR Email LIKE '%a%') OR (Name LIKE '%b%' OR Email LIKE '%b%')`           | Redundant OR syntax normalized      |
| `\!a`                  | `(Name LIKE '%!a%' OR Email LIKE '%!a%')`                                                   | Escaped NOT — treated as literal    |
| `\[bracket\]`          | `(Name LIKE '%[bracket]%' OR Email LIKE '%[bracket]%')`                                     | Escaped brackets                    |

> All matching is case-insensitive by default unless overridden with `StringCasing` options in the attribute.

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

Filtering is implicitly just works (IJW) — no manual trigger is required.  
The debounce delay can be customized, but filtering is always asynchronous and deliberate.

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

### Common Pattern: Shared NavSearchBar

In many apps, a shared `NavSearchBar` (NSB) is used to support multiple collection views, each activated through navigation.

Each view typically owns its own view model, and that view model contains an instance of `ObservableQueryFilterSource<T>` (OQFS) for its dataset.

While the view model could track the active expression and filtering state explicitly, `OQFS` already maintains the current input expression and state internally. This allows the NSB to be easily reconfigured when returning to a given view — no need for separate state plumbing.

This pattern supports:

- A one-to-many relationship between a shared NSB and multiple views
- Automatic restoration of the correct input expression for each dataset
- Consistent and predictable UX across navigable data contexts

