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
## ObservableQueryFilterSource

Drop-in replacement for ObservableCollection&lt;T&gt; with built-in support for both Query and Query-then-Filter workflows. It exposes a declarative interface for managing collection state while tracking query/filter intent via an internal FSM (QueryFilterStateTracker). Though UI-agnostic, the class anticipates integration with a navigation search bar, where queries are externally applied and subsequent in-memory filtering is handled via an embedded SQLite store. This enables persistent introspection of the original query, filtered/unfiltered results, and search metadata—all without any UI dependencies.

[ObservableQueryFilterSource](./IVSoftware.Portable.SQLiteMarkdown/ReadMe/observable-query-filter-source.md)
___

## SelfIndexing Class

The `SelfIndexing` class enables automatic generation of SQL search terms from property values using simple attribute annotations. It tracks changes in data, defers processing intelligently, and maintains up-to-date searchable terms (`LikeTerm`, `ContainsTerm`, `TagMatchTerm`) for fast, expression-based querying over markdown-bound SQLite objects.

To use it, inherit from `SelfIndexed`, apply `[PrimaryKey]` to your ID property, and annotate other properties with `[SelfIndexed(...)]` to control how they contribute to indexing and persistence.

[SelfIndexing](./IVSoftware.Portable.SQLiteMarkdown/ReadMe/selfindexing-class.md)
