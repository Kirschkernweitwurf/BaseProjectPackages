# Assembly Graph

🔧 **Tool** · programmers only

**Menu:** `Tools > Base Packages > Unity Editor > Project Health > Assembly Graph`

Shows every assembly definition in the project as a node graph, with arrows for the references between them.

## What it is good for

- Seeing at a glance how the code is wired together.
- Spotting circular or surprising dependencies.
- Finding references an assembly no longer needs, which slows down compile times.

## How to use it

1. Open the window. The graph builds itself.
2. Use the toolbar toggles to show or hide packages, Unity packages and library assemblies.
3. Type in the search field to highlight assemblies by name.
4. Turn on **Only Issues** to hide everything that is healthy.
5. Click a node to focus it. The graph then shows only that node and what it touches. Use **Clear Focus** to go back.

## Cleaning up

Nodes with unused references offer a cleanup action. It removes those references from the asmdef file.

Unity and library assemblies are protected and cannot be edited.

## Notes

- Node colors come from the root namespace, so related assemblies share a color.
- The layout puts assemblies with no dependencies at the top and builds downward.
