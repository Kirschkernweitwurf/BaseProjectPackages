# Focus Watchdog

**Namespace:** `Base.ControllerSupport.Controller.Focus`

**Base class:** `GameServiceBehaviour` (registered in the `ServiceLocator`)

## What it does

Global safety net that keeps a valid selection while a gamepad is in use. If the current selection becomes null or inactive, it restores focus to the highest priority active `NavigableGroup`.

Without it, closing a menu or disabling a button leaves the UI dead for a gamepad user, with no way back in.

## Setup

Add one `FocusWatchdog` to your service scene. There are no settings.

An `EventSystem` must exist in the scene. If none is found, a warning is logged once.

## How focus is chosen

- Active groups are compared by **Priority**. Highest wins.
- On a tie, the most recently activated group wins.
- Destroyed groups that never deregistered are cleaned up along the way.

Registering a group also promotes focus right away, so opening a higher priority menu takes focus immediately instead of waiting for the current selection to break.

## Mouse and keyboard

If an `InputDeviceTracker` is available, focus is only guarded while the gamepad is the active device. Mouse users can click into empty space and deselect freely.

Without a tracker, the watchdog always guards focus.

## API

| Member | Description |
| --- | --- |
| `RegisterGroup(NavigableGroup)` | Marks the group as active and promotes focus if it now wins. |
| `DeregisterGroup(NavigableGroup)` | Removes the group from the active set. |

You normally do not call these yourself. `NavigableGroup.Activate()` and `Deactivate()` do it.

## Troubleshooting

| Symptom | Likely cause |
| --- | --- |
| "No EventSystem exists in the scene" | Missing EventSystem. Add one. |
| Focus jumps to the wrong menu | Two active groups with the same priority. Give the front menu a higher one. |
| Focus never restores | No group is active, or all their elements are non-interactable. |
| "Group has no valid element to focus" | Default Element is missing, disabled, or not interactable. |
