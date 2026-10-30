---
type: Component
package: Controller Support
audience: Programmers
tags:
  - programmer
  - gamepad
  - ui
summary: Global safety net that keeps the focus on the screen on top, for gamepad and keyboard players.
---
# Focus Watchdog

🗞️ **Component** · programmers only

**Tags:** #programmer #gamepad #ui

**Namespace:** `Base.ControllerSupport.Controller.Focus`

**Base class:** `GameServiceBehaviour` (registered in the `ServiceLocator`)

## What it does

Global safety net that keeps the focus on the screen on top. The highest priority active `NavigableGroup` owns the focus, and:

- A selection on a group behind it is moved out, so keys and buttons never act on a screen the player can't reach. While the player steers it moves to the top group; while the player points it is cleared.
- While the player steers, a selection that becomes null or inactive returns to the top group.
- A group that closes with the selection inside hands it on at once, so a press in the same frame can't reach a screen that is fading out.

Without it, closing a menu or disabling a button leaves the UI dead for a gamepad user, and a menu opened by mouse leaves the keyboard on the button behind it.

## Setup

Add one `FocusWatchdog` to your service scene. There are no settings.

An `EventSystem` must exist in the scene. If none is found, a warning is logged once.

## How focus is chosen

- Active groups are compared by **Priority**. Highest wins.
- On a tie, the most recently activated group wins.
- Destroyed groups that never deregistered are cleaned up along the way.

Registering a group also promotes focus right away, so opening a higher priority menu takes focus immediately instead of waiting for the current selection to break. A group that holds its focus (`NavigableGroup.IsFocusHeld`) still owns it: the selection behind it is cleared and no element is picked.

Keep the group order in line with the menus: give each menu's group the menu's priority (the [Menu Navigation Module](Menu%20Navigation%20Module.md) warns otherwise), and never deactivate and activate a group just to rewire it, which would make it the newest.

## Steering and pointing

The watchdog asks an `ISteeringSource` service whether the player steers right now. Register one when keyboard players should keep a focus like gamepad players, since the device tracker only knows gamepads. Without one, it guards the focus while an `InputDeviceTracker` reports the gamepad, and always without a tracker.

While the player points, mouse users can click into empty space and deselect freely.

## API

| Member | Description |
| --- | --- |
| `RegisterGroup(NavigableGroup)` | Marks the group as active and promotes focus if it now wins. |
| `DeregisterGroup(NavigableGroup)` | Removes the group from the active set and moves a selection inside it on. |

You normally do not call these yourself. `NavigableGroup.Activate()` and `Deactivate()` do it.

## Troubleshooting

| Symptom | Likely cause |
| --- | --- |
| "No EventSystem exists in the scene" | Missing EventSystem. Add one. |
| Focus jumps to the wrong menu | The group's priority differs from its menu's, or a group was activated again after a newer one. |
| Keyboard keeps no focus | No `ISteeringSource` is registered, so only a gamepad counts as steering. |
| Focus never restores | No group is active, or all their elements are non-interactable. |
| "Group has no valid element to focus" | Default Element is missing, disabled, or not interactable. |