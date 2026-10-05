# Asset Zoo Builder

**Menu:** `Tools > Base Packages > Assets > Asset Zoo > Open Zoo Builder`

Lays out prefabs in the scene in neat rows so you can look at your whole art library at once. Good for reviews, scale checks and screenshots.

## How to use it

1. Create a config: `Assets > Create > Scriptable Objects > Base > Asset Zoo > New Config`. See[💌 ZooConfig](../scriptable-objects/zoo-config.md).
2. Open the window and drop the config into the **Config** field.
3. Optional: drop a Transform into **Parent** to build the zoo under a specific object.
4. Set the search folder under **Generation** in the config.
5. Press **Auto Generate Categories** to fill the categories from asset names.
6. Press **Build Zoo**.

The whole config is editable right inside the window, so you can tweak spacing and rebuild without leaving it.

## Buttons

| Button | What it does |
| --- | --- |
| Auto Generate Categories | Scans the search folder and groups matching assets into categories. |
| Build Zoo | Spawns the prefabs in the scene. |
| Clear Zoo | Removes the built zoo again. |
| Select Zoo Root | Selects the root object of the built zoo. |
| Select Zoo Parent | Selects the parent you assigned. |

## Naming convention for auto generate

Assets are expected to be named `Prefix_Group_Name`. For example `P_Garden_Rock_01` and `SM_Garden_Rock_01` both land in the group **Garden**. Prefixes and the separator are configurable.

## Notes

- The last used config is remembered, so the next session is just open and build.
- The zoo root carries a [🏷️ ZooRootMarker](../components/zoo-root-marker.md) so the builder can find its own object again. Do not remove it by hand.
- Nothing is written to the prefabs. The zoo is a throwaway scene object.
