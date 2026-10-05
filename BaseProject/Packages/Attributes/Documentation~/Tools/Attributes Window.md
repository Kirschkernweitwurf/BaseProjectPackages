# Attributes Window

🪛 **Tool** · for everyone

**Tags:** #designer #inspector

**Menu:** `Tools > Base Packages > Unity Editor > Project Health > Attributes`

## What it does

One window for everything about the attribute package. It is the documentation for the 98 attributes and the thing that tells you when one of them is not doing what it reads like.

Three tabs:

| Tab | What it is for | For whom |
| --- | --- | --- |
| **Reference** | One page per attribute, with a live sample and the code behind it | Everyone |
| **Showcase** | Every attribute at once on one object | Everyone |
| **Troubleshoot** | Attribute usages in the project that cannot work as written | Programmers |

## Reference

A searchable list on the left, one page on the right.

Each page has:

- **The name and what it does**, in a sentence
- **Requirements**, meaning what you have to set up before the sample does anything. Most say `Nothing`. The ones reading an Animator or a Material say so, because a preview that looks broken until an unstated field is filled in is the fastest way to make a reference look wrong
- **Live**, the sample drawn through the real inspector. Edit it freely, nothing is saved
- **Variations**, the other ways the attribute can be written
- **Good to know**, a fact about the attribute that is not another way of writing it
- **Source**, the whole sample class, with buttons to copy the attribute, copy the snippet or open the file

The samples are one class per attribute. That is what lets the page draw the whole object and print the whole class: everything in a sample is part of the answer, including the bool a condition watches and the property a dropdown reads its options from. The snippet is paste-and-compile as a result, rather than a fragment cut out of a larger file.

### Attributes that need a GameObject

Seventeen attributes are about a GameObject and its hierarchy, so their samples are components rather than assets. Their pages carry a **Create in scene** button.

Press it and a temporary object carrying the sample appears in the open scene and is selected. That is the only way to see two families of attribute:

- **scene handles** draw in the Scene view for the selected object, which an embedded inspector never is
- **header controls** are drawn by the real Inspector's component title bar, which an embedded inspector does not draw at all

The object it creates is never saved with the scene, and `Ctrl+Z` removes it.

### Keyboard

| Key | Does |
| --- | --- |
| `Ctrl+F` | Jump to the search box |
| `Up` / `Down` | Walk the list. Up from the top row goes back to the search box |
| `Right` on a closed category | Open it |
| `Right` on anything open | Move into the page beside the list |
| `Left` | Back to the list, or close the category |
| `Enter` on a category page | Open the highlighted attribute |

## Showcase

A throwaway asset carrying one of every attribute, drawn through the real inspector. Edit anything, nothing is saved.

Use it to see several attributes against each other. Use **Reference** when you want one of them explained.

## Troubleshoot

Every drawer in this package fails quietly on purpose: an attribute that cannot resolve what it points at falls back to the plain field, so a typo never breaks a whole inspector. That is right at runtime and useless for finding mistakes, because the fallback is only visible while the affected object happens to be selected.

Press **Scan**. The window walks every component, ScriptableObject and serializable type in the project.

### What it reports

| Severity | Means |
| --- | --- |
| **Error** | The attribute does nothing at all |
| **Warning** | It works, but not the way it reads |

Typical findings:

- a condition member that does not exist or is not a `bool`, which makes the condition true forever
- `[Dropdown]` pointing at something that is not enumerable
- `[AnimatorParam]`, `[AnimatorState]`, `[AudioMixerParameter]` or `[ShaderParam]` whose source field is missing or of the wrong type
- an attribute on a field type its drawer cannot handle, for example `[Required]` on an `int`
- `[GetComponent]`, `[Child]` or `[GetComponentInParent]` on a non-component type, on a `GameObject` field, or on a ScriptableObject, which has no hierarchy to search
- `[Button]` or `[HeaderButton]` on a method the renderer has to skip
- `[OnValueChanged]`, `[InlineButton]` or `[ValidateInput]` whose target method is gone or has changed shape
- `[ReferencePicker]` without `[SerializeReference]`

### How to use it

1. Press **Scan**.
2. The bar under the buttons says how many errors and warnings, across how many types.
3. Findings are grouped by the type that owns them. **Click a group header** to open that script.
4. **Errors only** hides the warnings.
5. The **search field** filters by type, member, attribute or message.
6. **Copy report** puts the report on the clipboard, respecting the errors-only filter.

The scan is manual rather than continuous: walking every type in the project is not cheap enough to run on a timer. A domain reload clears the result rather than showing a stale one.

### Demo types

A healthy project produces an empty report, which is the right outcome and a useless way to learn what the window looks like. Toggle **Demo types** and the scan runs over a set of types that are broken on purpose, one per family of mistake.

> [!NOTE]
> Those types are never included in a project scan, and the toggle clears itself whenever you change tabs, so a demo report can never be mistaken for the state of your project.

They are also the test fixture. If a check stops working, its demo type stops being reported.

## Related

See the [Attributes Package](../index.md) overview for the full list of attributes this window documents.
