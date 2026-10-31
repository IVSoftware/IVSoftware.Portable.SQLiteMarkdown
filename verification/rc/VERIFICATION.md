# SQLiteMarkdown 2.0.0-rc — local verification

October 28, 2026. Source and local package preparation are complete for the focused
release solution, SQLiteMarkdown.RC.slnx. This is not a published or signed release.

| Check | Debug | Release |
| --- | --- | --- |
| Focused solution build | Passed | Passed |
| Linked unchanged handwritten checks | 14 passed, 0 failed/skipped | 14 passed, 0 failed/skipped |
| Nemesis peer checks | 20 passed, 0 failed/skipped | 20 passed, 0 failed/skipped |

Full compilation reports 29 existing nullable/obsolete/unused warnings. Warnings
remain visible; project policy is unchanged. Verification invocations pass
`/p:SignAssembly=false /p:TreatWarningsAsErrors=false /p:WarningsAsErrors=`.
Restore used only the installed NuGet cache with audit disabled. Direct executable
MSTest runners were used with TESTINGPLATFORM_TELEMETRY_OPTOUT=1; no VSTest/socket
runner, network retrieval or elevated shell was used.

## Earned behavior

- Unawaited consecutive updates settle to the final submitted value before release.
- Repeated release requests join one task; successful release allows physical file deletion.
- WDT restarts on activity, closes idle connections, and permits later reopening.
- A native exclusive lock holds an accepted write beyond WDT expiry. The connection
  stays alive, explicit release remains pending, and the accepted update survives
  after the lock is removed. Requests after retirement are not admitted.
- Explicit release bypasses the ten-second keep-alive.
- Concurrent façade sync/async requests preserve all independently identified rows.
- A real SQL failure reaches Common's feed, settles without faulting the caller,
  and does not leak participation or prevent release.
- Parser/query-only/filter-only/query-then-filter checks execute real SQLite SQL.
  Key predicates compose with OR expressions; scoped limits revert; string parsing
  does not instantiate filtering storage. Incremental trailing OR remains executable.
- Published constructor, concrete stateful Clear(bool), interface terminal clearing,
  synchronization-authority type, and two restored utility helpers are retained.
- Bounded dictionary-free suffix correction covers apples, cases, boxes, classes,
  churches, brushes, dogs and terminal ss; no comprehensive inflection guarantee.

## Public API and package

compare-beta-api.ps1 compares exported types and declared public constructor/method/
accessor/field signatures against the cached 2.0.0-beta DLL, using isolated load
contexts without executing package code. All 391 baseline entries are present;
the RC has 501 entries. This signature comparison is not a complete semantic or
attribute/default-value compatibility proof. Separate tests establish the critical
Clear and constructor behavior. beta-api-comparison.json records the result.

inspect-package.py verifies that the local Release nupkg contains the built
netstandard2.0 assembly, README and icon, with no signing keys. The assembly contains
SQLitePrismaConnection and no proposed modeled-collection or temporal-affinity
implementation; the dependency manifest has no Collections or Affinity package.
package-final.json records the package hash, contents and dependencies. All 54
preserved pre-integration source files match their archive manifest hashes.

The generated nupkg is at:
`IVSoftware.Portable.SQLiteMarkdown/bin/Release/IVSoftware.Portable.SQLiteMarkdown.2.0.0-rc.nupkg`.
It is unsigned verification output. Normal signed publication and application
adoption require their separate developer workflow. No NuGet publication occurred.

## Scope limits and preservation

The original solution and mixed legacy suites remain intact. They cover unpublished
modeled/affinity fixture behavior and are not included in the 34-test RC count.
Their snapshots and handwritten methods were not edited, disabled or regenerated.
The focused handwritten project links the preserved 14-check subset; its existing
debugger-only clipboard adapter is separate, visibly Nemesis-authored scaffolding.
The broader frozen suite's obsolete public-contract witness is not rewritten to
bless the RC. pre-integration/ preserves overwritten production-source provenance.

The façade guarantees release only for its owned async participation. Independent
native connections or another façade sharing sqlite-net's pooled connection for
the same path/configuration are outside that ownership contract. Compose one owner
per database. DBPI/Record integration, durable moves, ESP policy and UI updates were
not implemented in this package increment. netstandard2.0 and shared APIs preserve
the portability direction; all execution here was on Windows, not Android or iOS.

Local Git status inspection was available; diff/check returned a work-tree error
in this session. No checkout, index, refs, commits or remotes were changed. Existing
unrelated Gabriel.Reference changes were preserved.

## Developer handoff

Suggested SQLiteMarkdown commit:
`Prepare compatible SQLiteMarkdown RC with owned SQLite connection lifetime`

Suggested Gabriel.Reference commit (only this increment's two documents):
`Record SQLite connection release invariant and RC package ownership`

Primary evidence: build-8.log, build-9-release.log, handwritten-final.log,
peer-final.log, handwritten-release.log, peer-release.log,
beta-api-comparison.json and package-final.json.
