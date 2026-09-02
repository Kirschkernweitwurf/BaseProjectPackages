# Tools package tests

## Making them appear in the Test Runner

Tests that live inside a package are not collected by default. The consuming project has to name the
package in `Packages/manifest.json`:

```json
{
  "dependencies": { },
  "testables": [ "com.baseprojectpackages.tools" ]
}
```

Without that entry the assembly still compiles and the tests still exist, but the Test Runner window
will not list them, which looks exactly like having no tests at all.

## What the liveness tests cover

Everything the Codebase Graph says rests on one judgment: whether a member is reachable. Most of the
ways that judgment can be wrong are invisible, because a member reached only through generated
machinery or through a string looks identical to a dead one.

The fixture holds both halves. Eleven shapes that are alive and must not be reported, and six that are
dead and must be. A tool that reported nothing would pass the first half, which is why the second half
exists.

## What is deliberately not covered

Three shapes need real assets whose GUIDs only exist once Unity has imported them, so there is nothing
that can be committed to assert them: `SerializeReference` node types, UnityEvent targets and
animation events.