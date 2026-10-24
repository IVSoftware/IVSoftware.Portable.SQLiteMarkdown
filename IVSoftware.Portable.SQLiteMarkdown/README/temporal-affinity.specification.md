# Temporal affinity — reconciled working specification

Agreed through CBQ with the author, October 21, 2026, in the LHAND Tap-Grab-Hover
chat. This reconciles the internal [ITemporalAffinity contract](../Interface.cs)
with the temporal-card discussion. It records intended behavior, not completed
scheduler functionality. Hands-on scenarios will validate the machinery.

The companion [mental models](temporal-affinity.mental-models.md) preserve the
author's concrete examples and connect the rules to intended user experiences.

This document governs interpretation of temporal comments. It adds no interface
members or runtime behavior. Presentation belongs to the consuming application.

## WYSIWYG and determinism

Visible relationships and declared constraints define the requested schedule.
The same ordered field, captured epoch, progress, constraints, and policy choices
must produce the same result. Distinguish estimated-work scarcity from contradictory
reservations. Expose both; never silently discard work, move a pin, relax constraints,
or imply that conflicting reservations can all be honored.

Use deterministic rules first. Where inputs leave a genuine choice, use explicit
heuristics or option policies. Missing query context is not proof of availability;
expose missing scheduling context when it prevents a valid result.

## Duration, Remaining, and Available

| Quantity | Meaning | Effect of time |
| --- | --- | --- |
| Duration | Original allocation of work or engagement; a pin's reserved block length. | Does not shrink with clock passage or execution. |
| Remaining | Unfulfilled balance of the allocation. | Play consumes eligible executing work; Pause preserves it. |
| AvailableTimeSpan | Working time permitted by placement, boundaries, and availability constraints. | Can shrink during Pause toward a fixed deadline. |

Scarcity is Available less than Remaining, not Remaining less than Duration.
Ordinary progress also makes Remaining less than Duration. Scarcity itself must
not reduce either work quantity or falsely record progress. Scarcity is a RED
hurry-up warning, not invalidity: the person may work faster, revise an estimate,
or accept partial completion. Play and subsequent scheduling may continue.
Free ASAP has no finite completion deadline and therefore no deadline scarcity.

Checking off work releases its unfinished scheduling requirement and recomputes
the affected field. Persisted distinctions between skipping and completing work
remain an application decision.

## Pins and hierarchy

- Temporal behavior is optional. Temporal children are ASAP; only roots may be fixed.
- Pinned times have minute resolution; do not display seconds as part of a pin.
  Existing fixed-time, fixed-date, and fixed-date-and-time modes remain distinct.
- Child-above ASAP schedules toward completion before its fixed ancestor's start.
- Child-below ASAP schedules forward from that fixed start, following preceding
  work and applicable constraints. A first child cannot execute before that start.
- A pin with Duration bounds its child-below epoch at start plus Duration. Without
  a reserved duration, that pin imposes no closing deadline on its child-below work.
- Before its eligible start, work retains its unfinished balance, including in Play.

Reconcile against one captured epoch snapshot under the existing emitter/field
ownership. Discardable pulses do not justify inconsistent per-card readings of Now.

## Play and Pause

Play means "I'm working on it." Eligible executing work consumes Remaining against
actual elapsed time. For a free ASAP sequence, projected completion can hold steady
while its active balance decreases.

Pause means "I'm taking a break." Remaining holds. Free ASAP placement and completion
move later; constrained Available can shrink into scarcity. Neither operation moves
the pin or fabricates effort. `OutOfTime` is distinct from human-confirmed completion;
do not silently check off or hide work when its countdown reaches zero.

## Independent ASAP and splitting

Independent ASAP occupies eligible unreserved time in canonical order. Delay alone
never makes it invalid. A point pin splits placement at its boundary but reserves
no elapsed interval. A pin with Duration reserves its block against independent
work, which resumes after the block ends. Children assigned to that block are its
own work, not independent work competing with its reservation.

`AffinitySlot` placements are ephemeral representations of one real item. They
share Duration, Remaining, identity, and completion accounting. Never duplicate
the record or count work twice. Projected completion follows the final slot,
including intervening gaps; first start plus Remaining is insufficient for splits.

Example: a five-minute float precedes a point pin five minutes away. While paused,
the portion that no longer fits is immediately placed beyond the boundary. If the
pin reserves a block, continuation begins after its end. BLUE can express this
valid split continuation.

A five-day hobby item similarly occupies available gaps. Assigned availability
calendars constrain eligible time. Do not invent sleep or working-hour reservations.

## Scarcity choices and structural conflict

Scarcity alone never blocks participation or invalidates following ASAP work.
For example, an estimated hour of marathon effort with 50 minutes Available is
a RED hurry-up warning; it is still valid to continue and attempt a faster finish.

Raise a contextual resolution bar with two choices:

- **Keep within pin:** retain the constraints and continue participating, with the
  RED warning while Available is below Remaining. The choice does not manufacture
  sufficient time. Checking off other work or revising the requirement can release
  capacity; actual work may also proceed faster than its estimate.
- **Spill into ASAP pool:** permit the unfinished continuation outside this block's
  closing constraint. Preserve the real item and completed effort, honor other
  applicable constraints, and recompute the block and independent placements.

Until answered, retain the constraint and warning with no automatic spill, but do
not block work merely because time is scarce. Show a clickable glyph for the chosen
policy so the user can reopen the options. Reversal recomputes placement without
undoing work already performed.

Example: a two-hour Flight contains two hours of reading and presentation work.
A paused nap preserves Remaining while Available shrinks. The shortfall becomes RED.
Explicit spill permits BLUE continuation after the flight, using the same real item.

A structural conflict is different: two pinned blocks reserving the same execution
capacity at overlapping times cannot both be honored as declared. Until resolved,
block the affected participation and dependent following ASAP sequence within the
constrained block. Preserve identity, progress, constraints, and diagnostic context;
do not hide the item or display dependent placements as valid. Independent ASAP and
unrelated blocks remain valid. A scarcity option does not by itself resolve conflicting
reservations; the conflicting declarations or an explicit applicable policy must be
resolved before the affected schedule is admitted as valid.

## Existing policies and implementation checkpoints

Preserve `FreeTimeMode`, `ScarceTimeMode`, and `OverlapTimeMode` as existing policy
concepts. Allocation must be deterministic and explicit; it cannot erase unfinished
work or silently waive an unresolved structural conflict. Scarcity policies alone
do not impose an execution block.

ASAP may have multiple availability constraints, such as weekdays and working hours.
Recover the existing editor's composition rules before implementation. Intersection,
union, precedence, and representation were not settled here; do not invent them.

Card direction: aligned start/end columns at upper right, a clock-sensitive balance
across both columns below, and pin/timer cues beside that display. Use existing IT
expander glyphs for hierarchy. Bias toward no legends. Duration appears in editing,
opened by tapping the balance. The exact constrained-display mapping of Remaining
and Available is still to be validated; do not mutate Remaining to display Available.
An ASAP-present play/pause strip is planned above the normal OnePage command bar.

The current prototype's Duration setter resets Remaining. Editing progressed work
needs an explicit preservation/reset decision before implementation. This document
does not silently change that behavior. Unfinished scheduler states and derived
properties mean source comments and existing tests are not proof of implementation.

Hands-on checks: start-gated execution; paused scarcity without blocking; overlapping
pins blocking their affected sequence within one constrained block; recovery by
checking off work; spill and reversal; point-pin and block splits; shared slot
progress; and deterministic availability-constraint scheduling.
