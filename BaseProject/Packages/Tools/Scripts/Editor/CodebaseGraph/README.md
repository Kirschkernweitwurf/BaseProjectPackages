# Codebase Graph, known gaps

Written down here rather than left in a chat log. Each of these is a decision that was made on
purpose, not something nobody noticed.

## Reported but not actionable

**Members of `AssetNamingRule` and `AssetNamingRuleSet` report as unused public API.** They sit one
namespace below the window that owns them, so the rule that clears window-owned types does not reach
them. Every cheap fix tried so far breaks something real:

- Any rule counting namespace levels treats `NamingConventions.Data` and `Editor.Handlers`
  identically, and the second is a genuine extension point.
- "Every caller lives inside ancestor N" is true of `CompactHelpBox` as well, which a consumer
  writing a handler really may call.

This needs reachability from entry points. Two ids, stable across scans, cheap to dismiss.

**`Setting<T>` reports as hard to change safely.** The exclusion for types meant to be depended on
deliberately does not include "implements an interface", because an empty marker says nothing and
would exempt a concrete workhorse like `EventBus`, which is exactly what the finding is for.

If this is ever built, the test is neither "implements an interface" nor "how many implementers". It
is whether any incoming edge is typed as the interface. `Setting<T>` passes that, `EventBus` fails it,
because everything reaches `EventBus` concretely.

## Measurement hidden rather than fixed

**`typeof` in a static table counts as coupling.** A lookup table naming thirty types reaches into
thirty namespaces and does one job. The size floor on the very large type finding hides this rather
than fixing it. The real fix separates a type reference from a dependency, so an `ldtoken` counts for
liveness and not for coupling, which is four files and a new edge kind.

## Not started

- Overload line resolution. A member with several overloads can resolve to the wrong line.
- Hashed dismissal ids. Would shorten the block at the cost of a lookup step.
- `dismiss-section <finding>`. One verb that silences a whole finding category. Would have saved
  twenty four near identical lines in a single review round.

## Numbers to carry forward

From one full review of 205 findings against the source:

- **Factually wrong: 7.** Three members reached only from a lambda inside a getter, one lookup table
  called a god class, two markers the tool reported against itself, and one member behind a define
  that was off. Six are fixed. The seventh is structurally invisible: the scan reads what the editor
  compiles, so code live only in a player build has no caller anywhere in the code that exists.
- **True but not worth acting on: roughly two thirds.** A different problem with a different fix.
  Accuracy work will not move it, and ranking work will not move accuracy.

Tracking these as one number points the work in the wrong direction. The next run against a codebase
this tool has not seen is the real test of whether 7 in 205 holds.