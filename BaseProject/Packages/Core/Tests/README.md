# Core package tests

## Making them appear in the Test Runner

Tests that live inside a package are not collected by default. The consuming project has to name the
package in `Packages/manifest.json`:

```json
{
  "dependencies": { },
  "testables": [ "com.baseprojectpackages.core" ]
}
```

Without that entry the assembly still compiles and the tests still exist, but the Test Runner window
will not list them, which looks exactly like having no tests at all.

## What is covered

These are edit mode tests over the parts of the package whose correctness is not visible from the
outside. A system that is wrong in one of these ways still runs, which is why they exist.

- **`SeededRandomTests`** covers the promise the whole seeded generator exists for: the same seed
  replays the same run, and every helper built on top of it stays inside the range it advertises.
- **`NoiseTests`** covers the two things layered noise is easy to get wrong: the output leaving the
  range it promises once octaves and shaping are stacked on, and a seed that does not actually move
  the pattern.
- **`WeightedTableTests`** covers what a weighted draw is supposed to guarantee: weight decides how
  often an entry comes up, a weight of zero takes it out entirely, and an empty table reports that
  instead of handing back a value.
- **`StateMachineLifecycleTests`** covers what a machine does around a run rather than inside one:
  entering and leaving states, the clock it keeps, the shape it reports to tooling, and whether it
  lets go of itself when it stops.
- **`StateMachineTransitionTests`** covers which transition a machine picks. Evaluation order is the
  part of a state machine that is invisible from the outside and the part a stuck machine almost
  always turns on.

`StateMachineProbe` is the context the state machine tests run over. It records which states were
entered, ticked and left, so a test can state what the machine did rather than inspect what it holds.