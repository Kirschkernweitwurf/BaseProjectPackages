# Gamepad Scroll Rect

🔖 **Component** · for everyone

**Tags:** #designer #gamepad #ui

**Namespace:** `Base.ControllerSupport.Controller.Scrolling`

**Requires:** `ScrollRect` on the same GameObject

## What it does

Lets a stick scroll a `ScrollRect` directly, usually the right stick. Useful for long text, logs and lists where you want to read ahead without moving the selection.

It uses unscaled time, so it keeps working while a menu pauses the game.

## Settings

| Field | Description | Default |
| --- | --- | --- |
| Scroll Action | Vector2 input action that drives scrolling, e.g. the right stick. Required. | —   |
| Scroll Speed | Speed in normalized units per second. | 1   |
| Dead Zone | Stick magnitude below this is ignored. | 0.15 |
| Invert Vertical | Flips the vertical axis. | off |

## Behavior

- The action is enabled in `OnEnable` and disabled in `OnDisable`.
- Vertical input only applies when the ScrollRect has **Vertical** enabled, same for horizontal.
- The position is clamped, so it stops cleanly at both ends.

## Notes

Use a `Vector2` action, not a button or axis. A pass-through action bound to the right stick is the normal choice.

Pair it with `ScrollIntoView` if the list also contains selectable elements.
