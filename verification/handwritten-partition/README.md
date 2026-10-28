# Handwritten verification separation — first executable increment

This workspace separates SQLite behavior from modeled-collection integration without
rewriting the original handwritten methods or their expected snapshots. It is a
verification bridge, not RC integration or permission to retire uncovered scenarios.

The three mixed source files contain **34 attributed test methods**. The source
partitioner copies their method text using Roslyn and records SHA-256 provenance in
`method-manifest.json`. `Verify-Preservation.ps1` checks the originals and the 30
executable copies. Changes belong in the authoritative source, followed by deliberate
regeneration; the copies are not competing handwritten owners.

| Disposition | Methods | Current evidence |
| --- | ---: | --- |
| SQLite-focused handwritten view | 29 | 29 passed against current master |
| XML handwritten integration view | 1 | `Test_Detect_QueryENB_or_QueryEN_when_IsFiltering` passed unchanged |
| Integration references awaiting scenario-level migration | 2 | `Test_QueryFilterFSMs`, `Test_DemoFlow`; preserved, not counted as passing |
| Mixed database-allocation test | 1 | Three new XML Nemesis checks pass; original SQLite configuration translation and parser observations remain pending |
| Historical affinity-fixture policy | 1 | `Test_PolicyException` retained in original source; ownership unresolved |

## What is preserved

- All three original files, their handwritten assertions, and their snapshots remain unchanged.
- Executable copies retain the exact method text. Header imports and fixture packaging are scaffolding.
- The diagnostic state formatter is copied into the test host. A small test-local adapter
  reaches the existing internal tag-spacing switch without changing production accessibility.
- The host redirects the original disposable database fixture to a unique test output directory.
  It does not delete the developer's historical LocalAppData database.
- Original clipboard helpers remain in use; no clipboard substitution was introduced.

The executable SQLite view still references XML test infrastructure and the current
production assembly. It does **not** establish that the isolated RC candidate can
compile or pass these methods without its former model dependency.

## Coverage ownership and remaining work

`Test_QueryFilterFSMs` has six meaningful subtests: external query returning zero,
one, and two records; clearing the awaiter; clearing query results; and clearing
filtered query results. Its historical prioritized fixture depends on retired
affinity/custody machinery. Preserve these intentions when replacing the fixture;
do not manufacture a private contract merely to restore compilation.

`Test_DemoFlow` has four meaningful subtests: initial query commit, animal filtering,
database append/requery, and the `color d` regression. It mixes the published SQLite
provider, a test-only search-bar host, model snapshots, and retired dump overloads.
Query/parser/commit assertions belong with SQLite; canonical identity, routing,
and model assertions belong with XML. Existing parser and live/predicate tests are
useful witnesses but are not claimed as complete replacements for this sequence.
The `.cs.reference` copies in the XML integration project remain visible reference
material and are not compiled. Their original counterparts remain untouched.

`Test_NoSpuriousFilterQueryDatabaseInstantiation` mixed three intentions. Its
model-owned allocation concern now has non-vacuous public-API coverage in
`Nemesis_QueryMirrorOwnershipTests`: repeated reads and ordinary mutations do not
create a database; opt-in copies and synchronizes the canon; disabling/re-enabling
rebuilds from the current canon. The historical test's optional private-interface
branch is not reproduced. Its SQLite configuration translation and parser/context
allocation observations still require separate disposition.

The XML Nemesis suite passes **137/137**, including the three new checks. This is
separate from the 29 + 1 handwritten methods; no pending reference is included.

## Build limits and review boundary

Both new handwritten projects and the XML Nemesis project build. The XML solution
build encountered locked lab output assemblies while the Collections lab was running.
The SQLite solution build encountered pre-existing references to a former XML project
location. Neither full solution is reported green. The successful XML test-project
builds used `BuildProjectReferences=false` after their dependencies had built during
this increment; they did not replace the running lab's files.

No production source was changed. No Git commit, remote write, or package publication
was performed. Published API decisions remain separate in `RC-DECISIONS.md`.

Suggested local checkpoint messages, when the developer chooses to commit:

- SQLite: `Separate executable handwritten SQLite verification and record migration gaps`
- XML: `Preserve handwritten filter-state coverage and verify query mirror ownership`
