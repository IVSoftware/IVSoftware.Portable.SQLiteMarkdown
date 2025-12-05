# Expression Parsing

This cross-platform library supports expression-based filtering and search for SQLite-backed collections. Input text is parsed into SQL using structured rules for substrings and tags with atomicity for "quoted phrases".

### Designed for a common shape

This isn’t a full SQL parser or a precision query engine — and it doesn’t try to be. It won’t replace hand-crafted queries when those are needed.

Instead, it’s a lightweight utility that captures the **gestalt** of a dataset in a useful, pragmatic way — tuned for the **probable and almost-certain** UI shape:

- A **platform-specific list view** (WinForms, MAUI, WPF, etc.)
- Driven by a **shared navigation search bar**
- Where one input field controls both what’s shown and how it’s refined

It works well in situations where you want to support both *browsing* and *filtering* without re-engineering your data layer or your UI, and all it asks in return is that you decorate your target properties using the small set of [MarkdownTerm] attributes available in this package.


| Mode            | Description                                                                 |
|------------------|-----------------------------------------------------------------------------|
| **FILTER**        | Filter an in-memory list (e.g., app settings, enums, cached results)       |
| **QUERY**         | Query a remote source (e.g., cloud database, REST API)                     |
| **QUERY → FILTER**| Query a remote source, then refine results using SQLite-backed filtering   |

---
# Demo App included in this Repo

This library is platform-agnostic. At the same time, it drops any pretense of not knowing there's a UI out there with a "probable almost-certain shape" to it. That predicted UI is unapologetically supported with states and bindable properties.

![Search demo with and without [app] tag](https://raw.githubusercontent.com/IVSoftware/IVSoftware.Portable.SQLiteMarkdown/master/IVSoftware.Portable.SQLiteMarkdown/README/img/demo-screenshot-term.png)

![Search demo with [app] tag](https://raw.githubusercontent.com/IVSoftware/IVSoftware.Portable.SQLiteMarkdown/master/IVSoftware.Portable.SQLiteMarkdown/README/img/demo-screenshot-tag.png)


The included demo is WinForms but don't be misled by that. There are no platform dependencies in the core package. None.

___

# Expression Syntax Documentation

This README outlines the operators and rules for parsing expressions in a custom search language. The operators follow a standard order of operations and offer flexible syntax for various logical expressions.

### Table of Contents

1. [Operators](#operators)
2. [Escaped Operators](#escaped-operators)
3. [Logical Operators](#logical-operators)
   - [AND](#and-operator)
   - [OR](#or-operator)
   - [NOT](#not-operator)
4. [Atomic Term Operators](#atomic-term-operators)
   - [TAG](#tag)
   - [Single Quotes](#single-quotes)
   - [Double Quotes](#double-quotes)

___

## Operators

Operators adhere to the standard order of operations, with **PARENTHESES** providing the functionality to override the default precedence.

### Escaped Operators

The following characters can be escaped to be treated as literals in expressions:

- `\&`, `\|`, `\!`, `\(`, `\)`, `\'`, `\"`, `""`, `''`

___

## Logical Operators

### AND Operator

The **AND** operation can be represented in several ways:

- A single `&` character.
- A single space ` `.
- Any consecutive string of `&` and space characters.

**Examples:**

- `"Term1 Term2"` is interpreted as `"Term1&Term2"`.
- `"Term1 & Term2"` is interpreted as `"Term1&Term2"`.

___

### OR Operator

The **OR** operation can be represented as:

- A single `|` character.
- Any consecutive string of `|` characters and spaces.

**Examples:**

- `"Term1 | Term2"` is interpreted as `"Term1|Term2"`.
- `"Term1||Term2"` is interpreted as `"Term1|Term2"`.

___

### NOT Operator

The **NOT** operation can be represented as:

- A single `!` character.
- Any consecutive string of `!` characters.

**Examples:**

- `"Term1 !Term2"` is interpreted as `"Term1&!Term2"`.
- `"Term1 !(Term2 | Term3)"` is interpreted as `"Term1&!(Term2|Term3)"`.

### Linting Requirements

Expressions must be formatted to avoid consecutive identical operators or conflicting operators, such as `&|`.

___

## Atomic Term Operators

### TAG

Expressions that include tokens enclosed in square brackets (`[` and `]`) are treated as **tags**.

#### Special Rule

While a user is entering a search term, the parser is invoked any time the input delay settles. During this process:

- **Unmatched square brackets** (either an opening `[` without a closing `]`, or vice versa) are considered to be literal characters and are included in **LIKE** and **FILTER** expressions without interpretation as tags.

___

### Single Quotes

Single quotes (`'`) are used to define atomic (exact match) terms.

- Quotes must appear in **pairs** to be interpreted as atomic delimiters.
- If an expression contains a **single** quote, it is treated as a **literal** and included in the search term.
- Consecutive single quotes (`''`) are interpreted as an **escaped quote**.
- During **incremental input** (e.g. debounced typing), a **trailing unpaired** quote is always treated as a **literal** to preserve user intent mid-expression.

___

### Double Quotes

Double quotes (`"`) follow the same rules as single quotes.

- Must appear in **pairs** to define atomic terms.
- A lone double quote is treated as a **literal** character.
- `""` is interpreted as an **escaped** double quote.
- During **incremental input**, a **trailing unpaired** quote is always treated as **literal**.
___

By following these rules, expressions can be parsed flexibly and safely—even while a user is still typing.

___

## Expression to SQL Translation

Once your model is decorated with markdown attributes like `[QueryLikeTerm]`, parsed expressions are converted into SQL using the attribute definitions. Each term is matched across **all decorated fields**, joined with `OR`.

For example:

```csharp
public class PetProfile
{
    [QueryLikeTerm]
    public string Name { get; set; }

    [QueryLikeTerm]
    public string Species { get; set; }
}
```
Input like `"pet"` will match:

```sql
(Name LIKE '%pet%' OR Species LIKE '%pet%')
```

Here are more examples:

| Input Expression       | SQL Translation (simplified)                                                                     | Description                         |
|------------------------|--------------------------------------------------------------------------------------------------|-------------------------------------|
| `cat dog`              | `(Name LIKE '%cat%' OR Species LIKE '%cat%') AND (Name LIKE '%dog%' OR Species LIKE '%dog%')`    | Implicit AND                        |
| `cat & dog`            | `(Name LIKE '%cat%' OR Species LIKE '%cat%') AND (Name LIKE '%dog%' OR Species LIKE '%dog%')`    | Explicit AND                        |
| `cat &&& dog`          | `(Name LIKE '%cat%' OR Species LIKE '%cat%') AND (Name LIKE '%dog%' OR Species LIKE '%dog%')`    | Redundant AND syntax normalized     |
| `cat | dog`            | `(Name LIKE '%cat%' OR Species LIKE '%cat%') OR (Name LIKE '%dog%' OR Species LIKE '%dog%')`     | OR operator                         |
| `cat || dog`           | `(Name LIKE '%cat%' OR Species LIKE '%cat%') OR (Name LIKE '%dog%' OR Species LIKE '%dog%')`     | Redundant OR syntax normalized      |
| `cat !dog`             | `(Name LIKE '%cat%' OR Species LIKE '%cat%') AND NOT (Name LIKE '%dog%' OR Species LIKE '%dog%')`| AND with NOT                        |
| `!cat`                 | `NOT (Name LIKE '%cat%' OR Species LIKE '%cat%')`                                                | Single NOT                          |
| `\!cat`                | `(Name LIKE '%!cat%' OR Species LIKE '%!cat%')`                                                  | Escaped NOT — treated as literal    |
| `!(cat | dog)`         | `NOT ((Name LIKE '%cat%' OR Species LIKE '%cat%') OR (Name LIKE '%dog%' OR Species LIKE '%dog%'))` | Negated group                     |
| `'exact phrase'`       | `(Name LIKE '%exact phrase%' OR Species LIKE '%exact phrase%')`                                  | Exact match using single quotes     |
| `"exact phrase"`       | `(Name LIKE '%exact phrase%' OR Species LIKE '%exact phrase%')`                                  | Exact match using double quotes     |
| `\"Hello\"`          | `(Name LIKE '%""Hello""%' OR Species LIKE '%""Hello""%')`                                          | Literal quotes via escaping         |

> Matching is case-insensitive.

---

## Split Contracts – Query Templates for Expression Parsing

So let’s be clear. We’ve used a class to generate a SQL expression. When we perform the actual query, does the data type receiving the recordset need to be the same type?  **It does not!**

That’s the idea behind **Split Contracts** — you can separate the type used to **build the query** from the type used to **receive the results**. The query model is just a template. It defines how to interpret the input expression, not how the data is stored or shaped.

This lets you create purpose-specific templates that filter the same table in different ways. Want to search just by `Name`? Or only `Species`? Or maybe apply a strict tag match? Define a few small query classes and switch between them on the fly — even bind them to a dropdown in the UI.

```csharp
class SearchByName     { [QueryLikeTerm] public string Name { get; set; } }
class SearchBySpecies  { [QueryLikeTerm] public string Species { get; set; } }
class SearchByTag      { [TagMatchTerm]  public string Tags { get; set; } }
```

Each of these can use the same search input — but produce different SQL depending on the fields and attributes involved.

> Think of Split Contracts as little search adapters: they don't hold the data, they shape the search.

This pattern lets you:

- Apply different query behaviors for different views, roles, or modes
- Avoid annotating your core data models with filter-specific concerns
- Cleanly separate indexing logic from data logic

> Query templates are lightweight and composable — think of them as named filter contracts for how a user’s input should be interpreted.

---
## SelfIndexing Class

The `SelfIndexing` class enables automatic generation of SQL search terms from property values using simple attribute annotations. It tracks changes in data, throttles processing intelligently, and maintains up-to-date searchable properties (`QueryTerm`, `FilterTerm`, `TagMatchTerm`) for fast, expression-based querying over markdown-bound SQLite objects.

One easy way to take advantage of this scheme is to inherit from the `SelfIndexed` base class, apply `[PrimaryKey]` to your ID property, and annotate other properties with e.g. `[SelfIndexed(IndexingMode.QueryOrFilter)]` to control how they contribute to indexing and persistence.

> **Note:**  
> These indexing attributes — `[QueryLikeTerm]`, `[FilterLikeTerm]`, and `[TagMatchTerm]` — typically map directly to individual SQL clauses.  
> However, when used via `[SelfIndexed]` in a class derived from `SelfIndexed`, those values are **aggregated** into unified properties like `QueryTerm`, `FilterTerm`, and `TagMatchTerm`.  
> This makes `SelfIndexing` especially well-suited for filtering and full-text search scenarios, where a consolidated expression better reflects user intent.

[SelfIndexing](./IVSoftware.Portable.SQLiteMarkdown/README/selfindexing-class.md)
___

## ObservableQueryFilterSource

Drop-in replacement for ObservableCollection&lt;T&gt; with built-in support for both Query and Query-then-Filter workflows. It exposes a declarative interface for managing collection state while tracking query/filter intent via an internal FSM (QueryFilterStateTracker). Though UI-agnostic, the class anticipates integration with a navigation search bar, where queries are externally applied and subsequent in-memory filtering is handled via an embedded SQLite store. This enables persistent introspection of the original query, filtered/unfiltered results, and search metadata—all without any UI dependencies.

[ObservableQueryFilterSource](./IVSoftware.Portable.SQLiteMarkdown/README/observable-query-filter-source.md)
