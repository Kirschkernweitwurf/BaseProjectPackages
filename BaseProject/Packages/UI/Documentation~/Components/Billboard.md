# Billboard

🔖 **Component** · for everyone

Turns an object so it always faces the camera. Use it for world-space health bars, name tags, speech bubbles or sprites that should never look flat.

## Where to put it

On the object that should turn, usually the root of a world-space canvas or a sprite.

## Fields

| Field | Required | What it does |
| --- | --- | --- |
| Lock Y Axis | No  | On: the object only turns left and right and stays upright. Off: it tilts to match the camera exactly. |

## Good to know

- Turn **Lock Y Axis** on for anything standing in the world, like name tags. It keeps text from tipping over when the camera looks down.
- The scene needs a camera tagged **MainCamera**. If there is none, the component switches itself off and writes a warning to the Console.
- This only works in Play mode. To see it while building the scene, use [🔖 Editor Billboard](Editor%20Billboard.md).
