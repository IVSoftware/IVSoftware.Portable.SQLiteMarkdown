# Get Breaking Changes

This note captures the workflow that turned the published V1 package into a
usable witness for V2 public-contract regression analysis.

## Intent

The goal is not to guess what V1 exposed.

The goal is to:

- run the actual V1 package in isolation
- export its public contract from a trusted witness project
- embed that exported contract into the modern MSTest project
- compare V1 against the current assembly with `GetBreakingChanges`
- use the diff as a run-edit-debug guide for compatibility work

## The Witness

The project
`IVSoftware.Portable.SQLiteMarkdown.MSTest.V1.csproj`
is the witness.

Why this project matters:

- it references NuGet package `IVSoftware.Portable.SQLiteMarkdown` version
  `1.0.1`
- it runs outside the moving local source tree
- it gives us a stable view of what was actually published

Two tests are especially useful:

- `Test_V1Capabilities()`
  This is the richer surface witness. It helps answer
  "what did V1 visibly expose?"
- `Test_ToPublicContract()`
  This is the contract witness. It proves `ToPublicContract()` is stable on
  the published V1 assembly and provides the export path below.

## The Vehicle

Inside `Test_ToPublicContract()` there is a commented-out file export:

```csharp
#if false
// EmbeddedResource
File.WriteAllText(@"Version=1.0.1.xml", contractOrig);
#endif
```

That export is the bridge from witness to comparison fixture.

Process:

1. Temporarily enable the export in the V1 witness test.
2. Run the witness test against the published V1 package.
3. Save the emitted `Version=1.0.1.xml`.
4. Add that file to the modern MSTest project as an embedded resource.
5. Turn the export back off.

This converts "what V1 looked like" into a durable test fixture that the V2
test suite can consume repeatedly.

## The Modern Consumer

The modern comparison happens in
`IVSoftware.Portable.SQLiteMarkdown.MSTest\TestClass_SQLiteMarkdown.NuGet_V2.cs`
inside `Test_GetBreakingChanges()`.

The important flow is:

1. Read embedded resource `Version=1.0.1.xml`
2. Generate `contractCurrent` from the current local assembly
3. Call `IsContractValid(...)`
4. If invalid, call `GetBreakingChanges(...)`
5. Use the diff to drive targeted compatibility work

This gives us a practical loop:

- V1 witness establishes truth
- V2 comparison localizes drift
- agent + user iterate on the smallest meaningful fix

## Advisory Pathology

When the diff is copied to the clipboard and pasted into a test as `expected`,
that pasted value is diagnostic, not normative.

Use comments like:

```csharp
// CODEX: DO NOT ASSERT OR PRESERVE THIS EXPECTED VALUE.
// CODEX: Treat as advisory pathology snapshot only.
// - Manual clipboard capture
// - Diagnostic only
// - Used to observe reduction/change in breakage shape
```

Meaning:

- the pasted XML is not a golden contract
- it is a snapshot of current pathology
- it helps measure whether breakage is shrinking or changing shape

## What Worked Well

This run-debug-edit loop worked especially well when we:

- started from the witness instead of speculating
- preferred the narrowest build/test path first
- treated namespace drift separately from true API removal
- used compatibility wrappers when the old type still had value
- stopped and accepted a deliberate breaking change when it fixed a real
  design hazard

## Important Example

One deliberate remaining break was
`ObservableQueryFilterSource<T>.Clear(bool)`.

Why it was allowed to remain broken:

- the old shape made a list-like object silently participate in the inherited
  `MarkdownContext` regressive clear state machine
- this was a "silent killer" because `Clear()` looked list-like but was not a
  deterministic terminal clear
- V2 policy now prefers an explicit parameterless "no surprises" clear for
  list-like surfaces

So the workflow is not "eliminate every diff at any cost."

The workflow is:

- establish the published truth
- inspect every diff
- preserve compatibility where it is cheap and honest
- keep deliberate breaks when they fix harmful semantics

## Recommended Future Use

For future sessions:

1. Run the V1 witness first if contract intent is unclear.
2. Use the embedded `Version=1.0.1.xml` as the stable baseline.
3. Let `Test_GetBreakingChanges()` identify the smallest current mismatch set.
4. Fix one compatibility cluster at a time.
5. If a diff reflects an intentional policy correction, document it and stop
   trying to erase it.

This keeps the process grounded, reproducible, and friendly to
agent-assisted run-edit-debug work.
