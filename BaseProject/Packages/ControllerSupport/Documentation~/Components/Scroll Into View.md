# Scroll Into View

🔖 **Component** · for everyone

**Tags:** #designer #gamepad #ui

**Namespace:** `Base.ControllerSupport.Controller.Scrolling`

**Requires:** `ScrollRect` on the same GameObject

## What it does

Keeps the selected element inside the visible part of a `ScrollRect`. uGUI does not do this for gamepad navigation, so without it a long list scrolls the selection out of sight and the player is navigating blind.

## Settings

| Field | Description | Default |
| --- | --- | --- |
| Padding | Pixels kept between the selected element and the viewport edge. | 16  |

## Behavior

- Runs in `LateUpdate` and only reacts when the selection actually changes.
- Ignores selections that are not children of the ScrollRect's content.
- Rebuilds the layout of this content only, not every canvas in the scene, so it stays cheap.
- Scrolls the minimum distance needed, on both axes if the ScrollRect allows it.
- Clears the ScrollRect velocity first, so an ongoing inertia scroll does not fight the correction.

## Setup

1. Add it to the GameObject that has the `ScrollRect`.
2. Make sure **Content** is assigned. A warning is logged in `Awake` if it is not.
3. Put your selectable items under that content object.

## Notes

Raise **Padding** when list items sit tight against the viewport edge and look cut off.

Use a **Viewport** on the ScrollRect. Without one, the ScrollRect's own RectTransform is used as the visible area, which is usually less accurate.
