# Zoo Config

**Create:** `Assets > Create > Scriptable Objects > Base > Asset Zoo > New Config`

**Default file name:** `ZC_ZooConfig`

Describes what the [🪛 Asset Zoo Builder](../tools/asset-zoo-builder.md) should lay out and how it should look. Make one config per library or per review, for example one for props and one for characters.

## Layout

How prefabs are arranged in space.

| Field | Meaning |
| --- | --- |
| **Type** | `Grid`, `Line` or `Circle`. Grid is a normal grid. Line puts everything in one row and wraps when it runs out of space. Circle places items in a ring. |
| **Alignment** | `Ground` puts the lowest point of the prefab on the floor. `Pivot` uses the prefab's own pivot. `Center` centers the bounding box on the slot. |
| **Category Direction** | Which way categories are stacked: Forward, Backward, Left or Right. |
| **Spacing** | Gap between items inside a category. |
| **Category Spacing** | Gap between categories. |
| **Grid Columns** | Number of columns, for Grid layout. |
| **Circle Radius** | Minimum radius, for Circle layout. It grows automatically if the items do not fit. |

Alignment only affects how things are displayed. The prefab assets are never changed.

## Labels

Floating text above the prefabs.

| Field | Meaning |
| --- | --- |
| **Show Category Labels** | Show a label above each group. |
| **Category Label Height** | How far above the items the category label sits. |
| **Category Font Size** | Font size for category labels. |
| **Show Item Labels** | Show a label above each prefab. |
| **Item Label Height** | How far above the prefab the item label sits. |
| **Item Font Size** | Font size for item labels. |
| **Item Color** | Color of item labels. Category labels use the color set per category. |

## Generation

Fills the categories automatically by scanning a folder.

| Field | Meaning |
| --- | --- |
| **Search Folder** | The folder to scan. Defaults to `Assets`. |
| **Prefixes** | Name prefixes to look for. Defaults to `P` and `SM`. |
| **Separator** | The character between name parts. Default is `_`. |
| **Search Depth** | How many subfolder levels to include. `0` is the folder itself, `-1` is unlimited. |
| **Ignore Prefix Case** | Match prefixes regardless of upper or lower case. |
| **Merge With Existing** | On: keep the categories you already have and only add new assets. Off: replace everything. |
| **Colorize Categories** | Give every generated category its own label color, based on its name. |

### Expected naming

`Prefix_Group_Name`. For example `P_Garden_Rock_01` and `SM_Garden_Rock_01` both go into the group **Garden**.

## Content

**Categories** is the actual list. Each category has:

| Field | Meaning |
| --- | --- |
| **Name** | Shown as the label above the group. |
| **Label Color** | Color of that category label. |
| **Entries** | The prefabs in this group. |

You can build this list by hand, or press **Auto Generate Categories** in the builder window and then adjust.

## Notes

- Changing the config does not rebuild the zoo. Press **Build Zoo** again.
- Duplicate prefabs inside a category are ignored.
