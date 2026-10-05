# Lighting Profile Applier

🔖 **Component** · for everyone

**Add Component:** Lighting Profile Applier

Applies a saved lighting setup as soon as the scene that holds this component is loaded.

This is the fix for a common problem: when you load several scenes at once, only the active scene's lighting is used. With a profile you decide which lighting wins, no matter which scene is active.

## Fields

| Field | What it is for |
| --- | --- |
| **Profile** | The Lighting Profile asset to apply. Required. |
| **Sun** | Optional. The directional light used as the sun. Leave empty to keep whatever Unity picked. |

## How to use it

1. Create a profile: `Assets > Create > Scriptable Objects > Base > Lighting Profile > New Profile`.
2. Set up the lighting in a scene the way you want it, then press **Capture** on the profile.
3. Add this component to any GameObject in the scene that should use it.
4. Drop the profile in.

## Notes

- The profile is applied in `Awake`, so it happens before anything is drawn.
- **Profile** is marked as required. If you forget it, you get an error in the Console instead of a silent failure.
- One applier per scene is enough. Two of them will fight over the settings.
