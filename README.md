# Expression Parsing Documentation

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

Once your model is decorated with `[SqlLikeTerm]` or related attributes, parsed expressions are converted into SQL using the attribute definitions. Each term is matched across **all decorated fields**, joined with `OR`.

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

| Input Expression       | SQL Translation (simplified)                                                                 | Description                         |
|------------------------|----------------------------------------------------------------------------------------------|-------------------------------------|
| `cat dog`              | `(Name LIKE '%cat%' OR Species LIKE '%cat%') AND (Name LIKE '%dog%' OR Species LIKE '%dog%')`   | Implicit AND                        |
| `cat | dog`            | `(Name LIKE '%cat%' OR Species LIKE '%cat%') OR (Name LIKE '%dog%' OR Species LIKE '%dog%')`    | OR operator                         |
| `cat !dog`             | `(Name LIKE '%cat%' OR Species LIKE '%cat%') AND NOT (Name LIKE '%dog%' OR Species LIKE '%dog%')`| AND with NOT                        |
| `!cat`                 | `NOT (Name LIKE '%cat%' OR Species LIKE '%cat%')`                                              | Single NOT                          |
| `'exact phrase'`       | `(Name LIKE '%exact phrase%' OR Species LIKE '%exact phrase%')`                               | Exact match using single quotes     |
| `"exact phrase"`       | `(Name LIKE '%exact phrase%' OR Species LIKE '%exact phrase%')`                               | Exact match using double quotes     |
| `!(cat | dog)`         | `NOT ((Name LIKE '%cat%' OR Species LIKE '%cat%') OR (Name LIKE '%dog%' OR Species LIKE '%dog%'))` | Negated group                     |
| `cat & dog`            | `(Name LIKE '%cat%' OR Species LIKE '%cat%') AND (Name LIKE '%dog%' OR Species LIKE '%dog%')`   | Explicit AND                        |
| `cat &&& dog`          | `(Name LIKE '%cat%' OR Species LIKE '%cat%') AND (Name LIKE '%dog%' OR Species LIKE '%dog%')`   | Redundant AND syntax normalized     |
| `cat || dog`           | `(Name LIKE '%cat%' OR Species LIKE '%cat%') OR (Name LIKE '%dog%' OR Species LIKE '%dog%')`    | Redundant OR syntax normalized      |
| `\!cat`                | `(Name LIKE '%!cat%' OR Species LIKE '%!cat%')`                                               | Escaped NOT — treated as literal    |
| `\[bracket\]`          | `(Name LIKE '%[bracket]%' OR Species LIKE '%[bracket]%')`                                     | Escaped brackets                    |

> Matching is case-insensitive by default unless configured via `StringCasing`.

---

## Query Templates for Expression Parsing

In many scenarios, you already have a dataset — for example:

- An `ObservableCollection<T>` that will be filtered in-memory, or
- A SQLite table or IQueryable source populated with records of type `T`

But you may want to control **how the search expression is parsed and translated into SQL** — independently of how your data is shaped or persisted.

To support this, the parser treats `T` as a **query template**, not necessarily the literal payload type.

You can define a proxy class with:

- The same property names as your real data
- The same `[Table]` mapping (optional, but recommended for SQLite)
- Different `[MarkdownTerm]` attributes to control which fields participate in the expression

```csharp
[Table("Contact")]
public class ContactStrictIndex
{
    [SqlLikeTerm]
    public string Name { get; set; }
}

[Table("Contact")]
public class ContactWideIndex
{
    [SqlLikeTerm]
    public string Name { get; set; }

    [SqlLikeTerm]
    public string Email { get; set; }
}
```

You can then parse the same input using different templates:

```csharp
var strictSql = "foo bar".ParseSqlMarkdown<ContactStrictIndex>();
var wideSql = "foo bar".ParseSqlMarkdown<ContactWideIndex>();
```

This pattern allows you to:

- Apply different query behaviors for different views, roles, or modes
- Avoid annotating your core domain model with filter-specific concerns
- Cleanly separate indexing logic from data logic

> Query templates are lightweight and composable — think of them as named filter contracts for how a user’s input should be interpreted.

---

## ObservableQueryFilterSource

Drop-in replacement for ObservableCollection&lt;T&gt; with built-in support for both Query and Query-then-Filter workflows. It exposes a declarative interface for managing collection state while tracking query/filter intent via an internal FSM (QueryFilterStateTracker). Though UI-agnostic, the class anticipates integration with a navigation search bar, where queries are externally applied and subsequent in-memory filtering is handled via an embedded SQLite store. This enables persistent introspection of the original query, filtered/unfiltered results, and search metadata—all without any UI dependencies.

[ObservableQueryFilterSource](./IVSoftware.Portable.SQLiteMarkdown/ReadMe/observable-query-filter-source.md)
___

## SelfIndexing Class

The `SelfIndexing` class enables automatic generation of SQL search terms from property values using simple attribute annotations. It tracks changes in data, defers processing intelligently, and maintains up-to-date searchable terms (`LikeTerm`, `ContainsTerm`, `TagMatchTerm`) for fast, expression-based querying over markdown-bound SQLite objects.

To use it, inherit from `SelfIndexed`, apply `[PrimaryKey]` to your ID property, and annotate other properties with `[SelfIndexed(...)]` to control how they contribute to indexing and persistence.

[SelfIndexing](./IVSoftware.Portable.SQLiteMarkdown/ReadMe/selfindexing-class.md)
