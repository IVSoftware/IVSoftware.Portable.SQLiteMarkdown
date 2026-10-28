# Production reconciliation decisions — no production fixes in this increment

This report distinguishes published compatibility decisions from verification-host
limitations. None of the build limitations in the adjacent README is presented as
a new RC implementation defect. The three new mirror checks found no failure.

## First blocking decision: Clear semantics

The published provider exposes `new void Clear(bool all = false)`. A concrete
no-argument call clears search state according to that established protocol;
explicit `IList.Clear` and `ICollection<T>.Clear` request terminal clearing.

The isolated candidate instead exposes `new void Clear()` forwarding to
`Clear(true)` and a `Clear(bool all)` returning `FilteringState`. This changes both
the no-argument behavior and the bool overload's signature. The recommendation is
to preserve the published provider's contract and verify concrete and interface
calls independently. Do not import the candidate's alternative accidentally.

## Other published surface checks before integration

- Retain or intentionally address the published `MarkdownContext(Type, bool)`
  constructor. An unused historical parameter is not itself authority to remove it.
- Reconcile the published `CollectionSyncAuthority` name with current master and
  the isolated candidate; a source rename can be a public API break.
- Review table-resolution precedence and diagnostics against published usage.
- Keep affinity/custody and general modeled-collection implementation out of the
  SQLite RC according to the agreed owner map. The current unpublished adapter
  and its mixed tests require a deliberate separation, not a blind whole-file move.

The recovered trailing-pipe regressions remain useful RC witnesses. This increment
did not freshly reproduce the historical exception or claim a new parser fix.
