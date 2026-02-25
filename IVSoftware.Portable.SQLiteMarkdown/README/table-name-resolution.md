# [<](../../README.md)

## Contract Table Resolution Matrix

| Base Class `[Table]` | Derived Class `[Table]` | Walk Finds Base `[Table]`? | Effective Table Name | Advisory / Conflict Behavior |
|----------------------|--------------------------|-----------------------------|----------------------|------------------------------|
| Yes (`[Table("items")]`) | None | Yes | `items` | Advisory: Derived type implicitly uses base table. |
| Yes (`[Table("items")]`) | Same (`[Table("items")]`) | Yes | `items` | No issue. Explicit agreement. |
| Yes (`[Table("items")]`) | Different (`[Table("itemsA")]`) | Yes | `items` | Conflict: **Base class wins**. Behavior governed by `ContractErrorLevel` (ThrowSoft / ThrowHard / Advisory). |
| No | Yes (`[Table("itemsA")]`) | No | `itemsA` | No conflict. Derived attribute defines table. |
| No | None | No | `Type.Name` | No conflict. SQLite default mapping applies. |
| No `[Table]` in any base walk | Yes (`[Table("itemsA")]`) | No | `itemsA` | Explicit attribute on current type defines table. |
| No `[Table]` in any base walk | None | No | `Mapper.GetMapping(type).TableName` (typically `Type.Name`) | Default SQLite mapping used. |

---

### Resolution Rule Summary

1. Walk base types from least derived to most derived.
2. The first `[Table]` discovered in the inheritance chain establishes the **canonical contract table name**.
3. If the current type declares an explicit `[Table]`:
   - If it agrees with the canonical base table name, no issue.
   - If it conflicts, **base class wins** to avoid spurious table creation.
4. If no `[Table]` is found in the base walk:
   - Use the current type’s `[Table]` if present.
   - Otherwise fall back to SQLite’s default table name (typically the type name).

---

### Core Principle

> TO AVOID SPURIOUS TABLE CREATION — BASE CLASS WINS.

The rationale is stability of the contract database across an inheritance tree.  
Explicit attributes on subclasses cannot silently fork schema identity.