# SQLiteMarkdown 2.0.0-rc — local verification

October 28, 2026. Source and local package preparation are complete for the focused
release solution, SQLiteMarkdown.RC.slnx. This is not a published or signed release.

**Current decision, October 31:** keep SQLiteMarkdown project-linked for a bounded
SasQuatch MAUI OG and Prisma adoption increment. The acceptance bar is **ready to
adopt**. The developer accepted deferring NuGet RC publication; see the
[adoption review](#october-31-ready-to-adopt-review) below.

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

## October 31 ready-to-adopt review

Gabriel reviewed the candidate against the ecosystem North Star, the upcoming
ESP client cutover, OG's DBPI/connection paths, and Prisma's registry and record
stores. The developer accepted keeping the active consumers project-linked while
one bounded adoption increment earns the connection contract. No implementation,
test, snapshot, package version, README prose, or release-note change was made by
this review. No package was published.

### Fresh evidence

| Check | Debug | Release |
| --- | --- | --- |
| Focused RC solution | Passed; 29 warnings, 0 errors | Passed; 29 warnings, 0 errors |
| Preserved handwritten subset | 14 passed, 0 failed/skipped | 14 passed, 0 failed/skipped |
| Nemesis peer suite | 20 passed, 0 failed/skipped | 20 passed, 0 failed/skipped |

Both suites used their direct executable MSTest runners and locally restored
dependencies. Release verification disabled assembly signing on the command line;
it does not establish a signed publication artifact. All execution was on Windows.
The earlier Android consumer BOK establishes compilation, not an Android or iOS
runtime witness for the new connection lifetime contract.

The preservation verifier also passed: all three authoritative handwritten files
and all 30 executable copied methods still match their recorded hashes.

The broader 29-method Core.Handwritten.Tests partition was separately restored and
built. It currently fails compilation with 11 errors involving retired
SQLiteMarkdown.Internal/Obsolete and Collections Events/Internal/IModeledCollection/
IRangeable test-host dependencies. Its tests were not executed or counted in the
34-test result. This is a verification-host migration gap; the build failure alone
does not demonstrate a parser defect. Frozen witnesses were preserved.

### Adoption findings and next proof

- **Transaction-scoped work is the main API adoption gap.** SQLitePrismaConnection
  exposes individual CRUD/Execute operations, and each synchronous call opens its
  own using-scoped connection. Prisma's QuickStore/CustodyJournal saves record,
  Content, and custody changes in one transaction; category operations also retain
  atomic work. Replacing those paths with separate facade calls would lose that
  boundary. Agree a reusable owned transaction execution contract and demonstrate
  successful commit plus rollback of the complete unit before migrating those
  paths. Domain records, custody policy, and move eligibility stay above SQLite.
- **Release must be proved at the consuming owner.** OG still uses raw and pooled
  native connections. Prisma's facade adoption currently covers its registry;
  QuickStore and LocalCategoryArray still use independent native connections.
  Prove one active connection owner for each participating database, drain accepted
  work, and preserve data during immutable-offering or retirement handoff. The
  package's isolated release result cannot certify independently owned handles.
  Explicit ReleaseAsync is terminal for that instance; ordinary repeated uploads
  need a deliberate snapshot/resume boundary that permits subsequent local edits.
- **Carry forward the relevant broader regression intentions.** The focused suite
  gives useful lifetime, failure-reporting, parser, identity, and Clear witnesses.
  Reconcile the relevant preserved query/filter/indexing scenarios with live
  verification during adoption; explicitly retain or disposition the mixed model
  and affinity fixtures. Do not replace frozen assertions just to obtain green.

The bounded adoption proof should cover the actual OG DBPI and Prisma ownership
seams with synthetic stores: atomic record/Content/custody preservation, accepted
work during handoff, rejection after retirement, distinct database scopes, and
recoverable failure without false durability or cloud receipts. SQLite owns its
database operation/lifetime mechanism; the clients own application of records and
ESP owns admitted-store synchronization. This gate does not establish live cloud
synchronization or authorize a broad storage rewrite.

### Release notes and README

The csproj's 2.0.0-rc notes already describe the facade, internal participation,
keep-alive, terminal release, excluded prototypes, and bounded singularization.
The README already covers the substantive new capability and the preserved Clear
contract. Its packaged text matches the repository copy exactly. Preserve the
polished expression/query/filter material; no substantive addition is needed now.

Before publication, qualify the release-note diagnostic event as
`IVSoftware.Portable.Common.Exceptions.Throw.BeginThrowOrAdvise`. Also verify the
two new repository-relative README links to SQLiteMarkdown.RC.slnx and Essential
IP in the NuGet presentation, using publication-safe absolute links if needed.
Neither referenced file is embedded in the nupkg. NuGet's
[README preview guidance](https://learn.microsoft.com/en-us/nuget/nuget-org/package-readme-on-nuget-org#preview-your-readme)
applies when the separate publication workflow is authorized. An eventual larger
adoption guide should be a small end section or a linked sub-read, preserving the
existing README body.

Fresh review evidence is under
`D:/Codex/IVSoftware/Quatch/artifacts/sqlitemarkdown-rc-review-20261031/`:
focused-debug-build.log, focused-release-build.log, handwritten-debug.log,
peer-debug.log, handwritten-release.log, peer-release.log,
broader-handwritten-build.log, package-document-review.json, and review-result.json.

Suggested documentation commit:
`Record SQLiteMarkdown adoption review and defer RC pending client proof`
