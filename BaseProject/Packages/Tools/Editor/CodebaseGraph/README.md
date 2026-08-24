# Codebase Graph

Static analysis over the compiled IL of every project assembly. It builds a dependency graph at three levels, namespaces, types and members, and reports what it can prove about it. The question it exists to answer honestly is whether a piece of code is still reachable.

The Tools package README covers what the tool does from the outside. This file is the detail: how it decides, and where it is known to be wrong.

## Why IL rather than source

A source scan sees what is written. An IL scan sees what the compiler produced, which is what actually runs. That difference is the whole tool: a member reached only through a lambda inside a getter, a const the compiler inlined into thirty call sites, a generic instantiation that only exists after substitution. Reading Mono.Cecil over the compiled assemblies means none of those need a special case in a parser.

The cost is that the scan reads what the editor compiles. Code live only in a player build has no caller anywhere in the code that exists, which is a real blind spot and not a fixable one.

## What is looked for beyond the IL

A code scan cannot see a reference wired in the inspector, so several of those are resolved anyway:

- `Invoke` and `SendMessage` by name, from IL string literals
- UnityEvent targets, from asset YAML
- Animation events
- Types stored by `SerializeReference`
- Consts the compiler inlined

This is what keeps working code from being reported dead. It is also the part most likely to miss something, which is what the test suite is for.

## Findings

Ranked high to low:

- Dead members and types
- Serialized fields nothing reads
- Fields written and never read
- Members that could be private, internal or readonly
- Mutable static state
- Type and namespace cycles, with the cheapest edge to cut named
- Very large types
- Types that are load bearing and concrete at once

## Dismissals

Per finding, stored in `ProjectSettings/CodebaseGraphDismissed.json` so they are committed with the project.

A dismissal id embeds the signature it was written for. That is deliberate: a rename brings the finding back rather than carrying the dismissal along, and the now-stale entry is listed for review instead of being silently kept. A dismissal is a judgment about a specific piece of code, and once that code changes the judgment is no longer known to hold.

`[CodebaseGraphIgnore]` from the Utility package is the permanent version, for a finding that is wrong for a reason the scan cannot see and never will. It silences every finding on what it marks, forever. Where the member really is used and the scan simply cannot see the caller, `[UsedImplicitly]` says that more precisely and leaves the findings that are about design rather than about use still reporting.

## Export and baseline

**Export findings** writes the whole report as Markdown, dismissal block included. **Export scope** writes one namespace or assembly on its own with its boundary first, small enough to hand to somebody working on that part alone.

**New only** compares against `ProjectSettings/CodebaseGraphBaseline.json` and shows what this scan found and the last one did not.

## Tests

`Tests/Editor/CodebaseGraph` covers the liveness judgment, which is what everything else rests on. Eleven shapes that are alive and must not be reported, six that are dead and must be. A tool reporting nothing would pass the first half, which is why the second half exists. See `Tools/Tests/README.md`.

## Known gaps

Written down here rather than left in a chat log. Each of these is a decision made on purpose, not something nobody noticed.

### Reported but not actionable

**Members of `AssetNamingRule` and `AssetNamingRuleSet` report as unused public API.** They sit one namespace below the window that owns them, so the rule that clears window-owned types does not reach them. Every cheap fix tried so far breaks something real:

- Any rule counting namespace levels treats `NamingConventions.Data` and `Editor.Handlers` identically, and the second is a genuine extension point.
- "Every caller lives inside ancestor N" is true of `CompactHelpBox` as well, which a consumer writing a handler really may call.

This needs reachability from entry points. Two ids, stable across scans, cheap to dismiss in the meantime.

**`Setting<T>` reports as hard to change safely.** The exclusion for types meant to be depended on deliberately does not include "implements an interface", because an empty marker says nothing and would exempt a concrete workhorse like `EventBus`, which is exactly what the finding is for.

If this is ever built, the test is neither "implements an interface" nor "how many implementers". It is whether any incoming edge is typed as the interface. `Setting<T>` passes that, `EventBus` fails it, because everything reaches `EventBus` concretely.

### Measurement hidden rather than fixed

**`typeof` in a static table counts as coupling.** A lookup table naming thirty types reaches into thirty namespaces and does one job. The size floor on the very large type finding hides this rather than fixing it. The real fix separates a type reference from a dependency, so an `ldtoken` counts for liveness and not for coupling, which is four files and a new edge kind.

### Not started

- Overload line resolution. A member with several overloads can resolve to the wrong line.
- Hashed dismissal ids. Would shorten the block at the cost of a lookup step.
- `dismiss-section <finding>`. One verb silencing a whole finding category. Would have saved twenty four near identical lines in a single review round.

## Numbers to carry forward

From one full review of 205 findings against the source:

- **Factually wrong: 7.** Three members reached only from a lambda inside a getter, one lookup table called a god class, two markers the tool reported against itself, and one member behind a define that was off. Six are fixed. The seventh is the player-build blind spot above, which is structural.
- **True but not worth acting on: roughly two thirds.** A different problem with a different fix. Accuracy work will not move it, and ranking work will not move accuracy.

Tracking these as one number points the work in the wrong direction. The next run against a codebase this tool has not seen is the real test of whether 7 in 205 holds.