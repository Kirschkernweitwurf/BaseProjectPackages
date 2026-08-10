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

## What LivenessTests covers

Everything the Codebase Graph says rests on one judgement: whether a member is reachable. Most of the
ways that judgement can be wrong are invisible, because a member reached only through generated
machinery or through a string looks identical to a dead one.

The fixture holds both halves. Eleven shapes that are alive and must not be reported, and six that are
dead and must be. A tool that reported nothing would pass the first half, which is why the second half
exists.

Three shapes are deliberately not covered: SerializeReference node types, UnityEvent targets and
animation events. All three need real assets whose guids only exist once Unity has imported them, so
there is nothing that can be committed to assert them.