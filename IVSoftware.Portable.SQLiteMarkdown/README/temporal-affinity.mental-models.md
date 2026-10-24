# Temporal affinity — mental models

Captured from the author's examples in the LHAND Tap-Grab-Hover chat,
October 21, 2026. Read these alongside the
[working specification](temporal-affinity.specification.md) and internal
`ITemporalAffinity` contract. These are intended user experiences and future
hands-on acceptance scenarios, not claims of completed implementation.

The examples explain the settled rules. They do not introduce new scheduling
policies, override declared constraints, or settle the deferred editor/display
decisions listed in the specification.

## "I'm working on it" / "I'm taking a break"

Play and Pause describe the person's engagement, not whether the wall clock runs.
While eligible work is executing, Play consumes its Remaining balance. Pause
preserves that balance; the passage of time does not perform the work for you.

For free ASAP, a break moves the projected finish later. Before a fixed deadline,
the same break reduces Available and can turn the warning RED. Duration remains
the original allocation in either case.

This is an aspirational estimate of progress: Play assumes effort is proceeding
according to plan. It is not proof of actual completion.

## "Pack carry-on starts at 9:03"

A root pin is at 9:03. Pack carry-on is indented beneath it in child-below affinity
and has a 25-minute allocation. Its initial placement is 9:03–9:28, not Now–Now+25.
Before 9:03, its Remaining holds at 25 minutes even if Play is selected. At its
eligible start, Play consumes the balance; Pause preserves it and, when no closing
constraint intervenes, pushes its projected end later.

The pin has minute resolution. The initial placement does not promise that the
projected end remains 9:28 through a pause. Tapping the displayed balance opens
Duration editing; the allocation does not need another permanent card legend.

## "Make the plane" / "I'll skip Starbucks"

Flight is fixed at 9:03. Packing and travel preparations are child-above ASAP work,
so their sequence must fit before that start. The clock approaches 9:03 whether
or not the person is ready to leave.

"I'm supposed to be driving but I'm not even in the car" describes paused effort
with shrinking Available, not completed driving. A RED warning makes the shortfall
visible. It does not prohibit trying to catch up.

"I'll have to skip stopping at Starbucks on the way" removes an unfinished demand
from the plan by checking it off. Recompute the other work against the released
capacity. The scheduler does not silently shorten the drive, move the flight,
or invent progress. How the application records skipped versus completed work
remains a separate decision.

## "I took a nap on a two-hour flight"

Flight starts at 9:03 and reserves two hours. Reading and presentation work assigned
below it are intended to fit within 9:03–11:03. Together they require two hours.
A paused nap leaves less available flight time than unfinished work.

Two visible responses express different intent:

- **RED — "I present when I land":** retain the closing constraint and warn of the
  shortfall. The person may hurry, change the estimate, or accept partial completion.
- **BLUE — "I can finish afterward":** explicitly choose spill-over. Continue the
  unfinished presentation in the independent ASAP pool after the reserved block,
  honoring other applicable constraints.

Spill-over does not create a second real presentation or lose work already done.
The policy glyph lets the person reopen the choice. Until chosen, preserve the
constraint and warning; scarcity alone does not block participation.

## "An hour to finish the marathon, but only 50 minutes left"

The work estimate says 60 minutes Remaining; the deadline permits 50 minutes
Available. This is scarcity, not an invalid booking. RED means hurry up: the
person could run faster than the estimate. Execution and subsequent scheduling
remain meaningful. Neither Remaining nor Duration is reduced merely to make
the warning disappear.

This example is the guard against accidentally treating every shortage as an
unresolvable conflict.

## "Two meetings in the same room at the same time"

Both pinned reservations require the same execution capacity over overlapping
intervals. They cannot both be honored as declared. This is a structural conflict,
unlike too much agenda for a valid meeting reservation.

Expose the conflict and block affected participation and the dependent following
ASAP sequence inside the constrained block until resolved. Do not present those
placements as valid, hide the booking, or silently select which meeting wins.
Unrelated blocks and independent ASAP remain valid. Any arbitration must be
an explicit applicable policy, not an accidental side effect.

The room illustrates declared capacity. It does not imply that unrelated calendars
or different rooms are automatically in conflict, or authorize resource-model work.

## "Five minutes of work, a pin five minutes from now"

An independent five-minute float can initially fit before the pin. Pause rather
than work, and the next reconciliation immediately has less than five minutes
before the boundary. Split the valid placement across it; do not wait until the
boundary to discover the continuation.

A point pin reserves no elapsed interval. A pin with Duration reserves its whole
block, so the continuation resumes after its end. This is independent ASAP flowing
around an obstruction, not child-above work ignoring its own parent's deadline.

## "A hobby project will take five days — when could I finish?"

One independent ASAP project requires five days of effort. Its projected finish
comes from accumulating eligible working time around reservations. A distant
finish does not make that free ASAP invalid.

Weekday and work-hour constraints can be assigned through the separate editor,
and an ASAP can reference more than one. These constrain eligible time; do not
invent sleep or working hours the person has not declared. Recover the existing
editor's composition semantics before calculating with multiple constraints.

## "Two time records, one real presentation"

An interrupted task may appear in multiple ephemeral placements. These are views
of the same real record, not separately counted tasks. Progress on one placement
changes the shared Remaining balance; content and identity remain attached to
the real item. Recompute the other placements without duplicating completed work.

## "Maybe I can't give you that schedule, but I won't silently fail"

WYSIWYG is the governing promise. The person must be able to understand the
relationship between the visible plan, warnings, conflicts, choices, and outcome.
Show scarcity as scarcity and contradictory reservations as conflicts. A heuristic
may estimate placement under a declared policy; it may not conceal missing context
or silently waive an impossible reservation.

Use these stories during hands-on implementation to test whether the backend
delivers the intended experience. Where a story exposes an unresolved detail,
settle that detail explicitly rather than making the prototype the new contract.
