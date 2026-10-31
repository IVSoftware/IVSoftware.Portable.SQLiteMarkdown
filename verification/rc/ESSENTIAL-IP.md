# SQLiteMarkdown RC — Essential IP

Agreed October 28, 2026. Gabriel implements the RC under YTW. Local source,
verification and packaging are authorized; Git writes and NuGet publication are not.

## Release boundary

Publish the useful query/filter improvements beyond 2.0.0-beta and a new
SQLitePrismaConnection façade. Do not carry the proposed XML-modeled query/filter
collection into this package. Temporal affinity belongs entirely to its independent
NuGet. The already-published ObservableQueryFilterSource remains a compatibility
commitment, not an invitation to expand concrete collection ownership.

The canonical repository is D:/Codex/IVSoftware/SQLite/IVSoftware.Portable.SQLiteMarkdown.
The checked-out 261027.A-prisma-origin and local master shared b6b019a at onboarding.
The October 24 isolated candidate was an experiment assembled from beta and selected
modern source; it was not the bleeding-edge branch. Its useful extraction is being
reconciled in place, with pre-integration source bytes preserved and hash-manifested
in pre-integration/. No handwritten assertions or snapshots are changed.

## Ownership

Record requests CRUD through its assigned DBPI. DBPI composes the SQLite façade;
neither inherits a native SQLite connection. Query materialization assigns that
same DBPI to each returned persistent record before returning the result set.
Aggregate queries preserve each row's originating DBPI discriminately. These
record/database contracts belong to PrismaRuntime and its applications, not SQLite.
This RC adds the reusable façade; application adoption is a separate increment.

## Lifetime mechanism

The façade presents synchronous and asynchronous CRUD. Clients do not contemplate
tokens. Each synchronous call contains its native connection in a using scope and
never participates in DisposableHost. Async work is admitted and owns a token
before its first await; completion disposes that token, never the shared connection.
UI callers may leave update tasks unawaited while the owner remembers their work.
Consecutive edits use last-write semantics; this is not a per-edit custody journal.

WDT acquires its own token. Every client push/pop kicks the dog; WDT's token changes
are ignored to prevent circularity. Keep-alive is configurable, clamped to 1–10
seconds, default 10. Timer expiry relinquishes only WDT's token. Active work remains
protected even beyond expiry. Acquisition, expiry, final disposal and closing handoff
are coordinated under a lock; asynchronous close completion is tracked before reuse.

## Exclusive release invariant

Track async participation so an operation requiring exclusive file access can stop
admission, await accepted work, and close all managed connections before proceeding.
Explicit release bypasses keep-alive and is awaitable and terminal for that façade.
The application is annihilating DBPI, not merely a file. Surviving records are
reassigned to their destination DBPI during eligible root absorption; ordinary
annihilation retires records with their DBPI. No normal stale-record workflow or
additional per-edit history is introduced to justify the mechanism.

The move owner establishes durable destination completion before retiring the
persistent source. SQLite establishes release, not eligibility, synchronization,
cloud deletion, UI projection, or ownership reassignment. ESP must respect stopped
admission; reconciling incoming content is not a prerequisite to retiring a source.
Independent native connections are outside this façade's release guarantee.

## Execution and reporting

Lifetime accounting is distinct from execution coordination. One execution boundary
orders façade async requests and coordinates façade synchronous access. FullMutex
connections preserve SQLite's connection-thread safety; contention with external
connections remains SQLite locking behavior, with a bounded busy timeout. There is
no UI-thread dependency or requirement for a new ESP host.

Common's ThrowHard/ThrowSoft mechanism provides diagnostics even for Release feeds.
Routine failures do not escape to production users as exception conditions. The
façade returns unsuccessful CRUD results (zero/false/empty rows), reports failures
through Common, and never represents a failed release as permission to delete.
The application owns warning the user and deliberate termination for a genuine
imminent-corruption condition. No new automatic crash policy is invented here.

## Acceptance and exclusions

SQLiteMarkdown.RC.slnx is the focused developer-facing release solution. It includes
the library, linked untouched handwritten checks, and separate Nemesis peer tests.
Verify parser workflows and beta compatibility, async CRUD including unawaited
updates, concurrent operations, WDT restart/expiry, active work beyond timeout,
explicit release and physical file deletion. Use synthetic stores only. Portable
tests need no STA; use the established disposable STA helpers if a Windows-only
scenario later requires them.

Preserve constructor MarkdownContext(Type, bool), CollectionSyncAuthority, and the
published concrete void Clear(bool all = false). Interface collection clearing stays
terminal. Do not silently adopt the isolated candidate's alternative Clear contract.
Legacy mixed suites remain preserved in their original solution and are not counted
as passing this release merely because a focused subset passes. Build/package
evidence is local, not publication or an Android/iOS runtime verification.

## Bounded singularization correction

The author explicitly permits small suffix-rule improvements without loading a
dictionary. Preserve the published helpers, but correct the generic `es` removal
that changed apples to appl. Sibilant suffixes still remove es; ordinary plurals
remove only s, and terminal ss remains intact. This is a lightweight heuristic,
not a comprehensive English inflection engine or a new dependency.
