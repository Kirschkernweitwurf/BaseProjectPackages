# Navigable Element

🔖 **Component** · for everyone

**Tags:** #designer #gamepad #ui

**Namespace:** `Base.ControllerSupport.Controller.Navigation`

**Requires:** `Selectable` on the same GameObject

## What it does

Marks a button, slider, toggle or any other `Selectable` as a real gamepad target. Only selectables that carry this component get wired into a `NavigableGroup`.

It is a pure marker. There are no settings.

## Why it exists

Not every selectable in a UI should be reachable with a stick. Making the marker explicit means the wiring is a deliberate choice instead of a guess.

## Adding it

Two options:

1. Add the component by hand.
2. Hit **Rebuild** on the `NavigableGroup` (or use the Navigation Groups window). Any selectable below the group that is missing the component gets one added, and the fix is logged.

## API

| Member | Description |
| --- | --- |
| `Selectable` | The sibling selectable this element wraps. Resolved lazily, so it also works in edit mode. |
| `IsNavigable()` | True when the selectable exists, is interactable, and its GameObject is active in the hierarchy. |

Non-navigable elements are skipped during wiring, so a disabled button never becomes a dead end.
