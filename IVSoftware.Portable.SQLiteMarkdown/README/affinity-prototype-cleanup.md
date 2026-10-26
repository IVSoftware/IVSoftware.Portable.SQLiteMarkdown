# Affinity prototype cleanup — October 24, 2026

SQLiteMarkdown's shipping library no longer contains its internal temporal/prioritized
affinity prototype, public AffinitySlot, AffinityPolicy, AffinityException, or unused
ApplyAffinities hook. Query/filter path resolution uses the existing Collections
IPathPlaceable contract. No Affinity package dependency or type forwarder was added.

## Preserved work and its owner

The standalone IVSoftware.Portable.Affinity solution owns the maintained temporal
specification, mental models, and Nemesis review under its docs directory. Original
temporal models, the field/emitter prototypes, and the two handwritten temporal test
classes were transferred to reference/SQLiteMarkdownPrototype without rewriting their
snapshots. Supporting sources were copied for context. All 13 reference files passed
preservation hash verification. The reference source is outside compiled projects;
it is not included in the package or counted as passing tests.

SQLiteMarkdown's broader query/filter tests retain only the legacy prioritized-model
fixtures they use. The reusable public contracts remain in IVSoftware.Portable.Affinity.
This transfer implements no scheduler and adds no public Affinity members.

## Published compatibility evidence

Direct NuGet.org package metadata inspection found no type name containing Affinity or
Temporal in either the published 1.2.0 assembly (81 type definitions) or 2.0.0-beta
assembly (83 type definitions). Neither contains ApplyAffinities. The removed affinity
surface was absent from both releases; this conclusion does not certify every
historical/private build. Unlisting alone does not erase compatibility obligations.

Published package SHA-256 hashes inspected:
- 1.2.0: EF4CD4B818B2AD8A49739392E3E82061D8A81A4FB9144D41785EE52D18C64ADC
- 2.0.0-beta: 1FBA9A5BE100B10705027FADFB4C54BF5B8F68A9DEF261B2AFDDCE8C41000204

Private/local build consumers of Common.AffinitySlot must reference the Affinity
candidate and change the namespace. Existing published packages are not modified.

## Verification

The cleaned SQLiteMarkdown library builds in Debug with warnings visible; its rebuilt
DLL has no Affinity/Temporal types. The Affinity solution builds with zero warnings and
errors, and its existing direct-executable Nemesis suite passes 3/3 with zero skipped.
The archived handwritten temporal tests are not part of this test total.

SQLiteMarkdown's full handwritten test project remains unverified: the attempted
build encountered stale XML Collections/support project references, missing collection
types, and unsigned-build friend-access key mismatches. No snapshots were changed to
work around these blockers. No Git/GitHub operations or publication occurred.
