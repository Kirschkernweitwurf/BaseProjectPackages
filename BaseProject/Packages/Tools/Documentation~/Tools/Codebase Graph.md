# Codebase Graph

🔧 **Tool** · programmers only

**Menu:** `Tools > Base Packages > Code > Health > Codebase Graph`

Reads the compiled code of every assembly in the project and shows what uses what, as a node graph. It answers one question: is this code still reachable.

## What it is good for

- Finding code nothing calls any more, so you can delete it.
- Finding serialized fields you set in the inspector that no code ever reads.
- Spotting members that could be private, internal or readonly.
- Seeing circles between types or namespaces and which arrow is cheapest to cut.
- Handing one namespace to somebody or to an AI, as a single readable file.

## How to use it

1. Open the window and press **Rescan**. It takes around twenty seconds on a large project and does not run on its own.
2. You start at namespaces. Double click one to see its types, double click a type to see its members. **Back** goes up a level.
3. Click anything to see what was found on it and what to do about it, in the panel on the right.
4. Use the finding dropdown to show one kind of problem at a time.
5. Work through the list on the left. Fix what is real, dismiss what is not.

## Reading the graph

Nodes are shaped by what they are.

- Wide and round is a namespace, square is a type, narrow is a member.
- A light border means an interface.
- The letter in the circle is the kind: `C` class, `I` interface, `M` method, `F` field and so on.
- The stripe down the left and the text color show who can use it. Green is public, amber is protected, blue is internal, grey is private.
- A red row or badge is something found. A teal one is something you dismissed.
- A green **Clear** on a list row means nothing was found there at all.

The legend in the top right lists all of it. Click it to fold it away.

Lines start faint and light up when you click a node. Use the **Lines** dropdown if you want them all drawn or none.

**Layout: Dependencies** places things by what uses what, which is what you want when reading the structure. **Layout: Grouped by name** puts each package back together and sorts it alphabetically, which is what you want when hunting for something.

## What it finds

| Finding | Meaning |
| --- | --- |
| Never used members | Nothing calls it or reads it. |
| Unreferenced types | No other type mentions it. |
| Serialized, never read | You can set it in the inspector but no code reads it. |
| Written, never read | Code assigns it and nothing reads it back. |
| Could be private | Only its own type uses it. |
| Could be internal | Only its own assembly uses it. |
| Could be readonly | Only the constructor sets it. |
| Mutable static state | A static field that keeps its value between play sessions. |
| Very large types | Reaches into many namespaces or has a great many members. |
| Hard to change safely | Lots of code depends on it and there is no interface in front. |
| Type cycles | Types that depend on each other in a circle. |
| Namespace cycles | Namespaces that use each other in a circle. |
| Unused public API | Nothing here uses it, but it is public in a package. |
| Unused interface members | Implemented, but never called through the interface. |
| Unimplemented interface members | Declared, implemented by nobody. |

Some findings offer **Apply fix**, which edits the source for you. It refuses whenever anything is unclear, so it does nothing rather than the wrong thing. Commit first.

## Dismissing

Not every finding is a problem. Dismissing hides one without changing any code.

- **Dismiss this** on a finding card hides that one finding. This is the one you usually want.
- **Dismiss everything here** hides everything on that entry, including things found later.
- **Dismiss with contents** also hides everything inside a namespace or type.
- **Bring back** undoes it.

The **Dismissed** button in the toolbar opens the full list, where you can restore anything.

A dismissal remembers the exact signature it was written for. Rename the member and the finding comes back and the old entry is listed as stale so you can look at it again. The list separates two cases: the thing it pointed at is gone and the finding no longer fires. The second usually means you fixed it.

Dismissals live in `ProjectSettings/CodebaseGraphDismissed.json`.

## Only showing what is new

Turn on **New only** above the list to see just what this scan found and the last one did not. It is empty until you have scanned twice, because there is nothing to compare against yet.

## Exporting

**Export findings** writes the whole report as a Markdown file. It is ordered by how much attention each thing deserves, opens with the twenty worst and ends with a block of dismissals you can edit and hand back through **Update dismissals**.

**Export scope** writes one namespace or assembly on its own. Open a namespace first or pick an assembly in the toolbar. That file starts with what the scope uses and what uses it, then lists every type with its public members, then the findings inside it. It is small enough to hand to somebody working on that part alone.

## What it cannot see

The tool reads compiled code, so a few things are invisible to it. Most are handled anyway, by reading strings and asset files:

- `Invoke` and `SendMessage` by name
- Methods a UnityEvent calls, set in the inspector
- Animation events
- Types stored by `SerializeReference`
- Consts, whose values the compiler copies into every call site

Reflection is the one that remains. Check for it before you delete something.

Put `[CodebaseGraphIgnore]` on a type that should never be reported, such as a test fixture or generated output. Generated files, sample code and test assemblies are skipped already.

## Notes

- Node colors come from the first two parts of the namespace, so one package shares a color family.
- A namespace with more than 150 types only draws the first 150. The heading says so. Narrow the filter to see the rest.
- The scan is thrown away whenever Unity recompiles, so you press **Rescan** after code changes.
- The tool's own rules about what counts as reachable are covered by a test suite in the package.
