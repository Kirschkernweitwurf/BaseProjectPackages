# Confirmation Service

🗞️ **Component** · programmers only

The connection between the confirm buttons and the popup. It is scene setup, not something you interact with directly.

## Where to put it

Once per scene, on a GameObject that lives for the whole scene, for example your UI manager object.

## Fields

| Field | Required | What it does |
| --- | --- | --- |
| Confirmation Menu Identifier | Yes | The menu identifier asset of your [🗞️ Confirmation Menu](Confirmation%20Menu.md). |

## Good to know

- Without this component in the scene, confirm buttons do nothing.
- Only one popup can be shown at a time. A second request while one is open is ignored.
- If the identifier points at a menu that is not a Confirmation Menu, you get an error in the Console on play. That usually means the wrong identifier was dragged in.
