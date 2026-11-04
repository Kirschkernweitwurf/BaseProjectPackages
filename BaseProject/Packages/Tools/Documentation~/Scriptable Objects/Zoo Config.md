---
type: Scriptable Object
package: Tools
audience: Everyone
tags:
  - artist
  - assets
summary: One saved zoo for the 🪛 Asset Zoo Builder, with its folders, groups and look.
---
# Zoo Config

💌 **Scriptable Object** · for everyone

**Tags:** #artist #assets

**Create:** the **New…** button in the [🪛 Asset Zoo Builder](../Tools/Asset%20Zoo%20Builder.md), or `Assets > Create > Scriptable Objects > Base > Asset Zoo > New Zoo Config`

**Default file name:** `ZC_ZooConfig`

One saved zoo: what it shows, how that is sorted and how it looks. Make one per library or per review. It is edited in the [🪛 Asset Zoo Builder](../Tools/Asset%20Zoo%20Builder.md). Double-clicking it opens it there.

## Layout

How the art stands. All of this updates a standing zoo while you change it.

| Field | Meaning |
| --- | --- |
| **Type** | `Rows` are rows of a set length, each asset taking only the room it needs. `Line` is one long row. `Circle` is a ring. |
| **Alignment** | `Floor` stands the bottom of what you see on the floor. `Pivot` uses the prefab's own pivot. `Center` puts the middle of what you see on its place. |
| **Order** | `Small in Front` puts big things behind small ones, so nothing hides. `As Listed` follows the list. |
| **Groups** | `Blocks` keeps the zoo roughly square, like a city map. `Row` and `Column` line the groups up. |
| **Spacing** | Gap between two assets, in meters. |
| **Category Spacing** | Gap between two groups, in meters. |
| **Items Per Row** | How many assets stand in one row. |
| **Circle Radius** | Smallest radius of a ring. It grows when the assets would not fit. |

The prefab assets are never changed.

## Show

What stands around the art.

| Field | Meaning |
| --- | --- |
| **Has Group Names** | Draw group names and zones in the Scene view. |
| **Has Item Names** | Draw asset names in the Scene view. |
| **Has Sizes** | Add the size and triangle count to every name. Hovering a name always shows them. |
| **Has Ruler** | Draw a ruler at the start of every group. |
| **Scale Figure** | Stands at the start of every group. A person works best. |
| **Has Floor** | Build a floor that fits the zoo, so the art has ground and shadows to stand on. |
| **Floor Material** | The material of the floor and of the boards decals land on. Empty uses the render pipeline's default. |

## Generation

Where the art comes from.

| Field | Meaning |
| --- | --- |
| **Sources** | The folders, prefabs and models the zoo shows. |
| **Group By** | `Folder` makes one group per folder. A folder that only sorts by file type, like `Models Cafe`, counts as its parent. `Name` uses the first word of the name behind its prefix. |
| **Has Models With Prefab** | Also show a model when a prefab already shows it. |
| **Search Depth** | How many subfolder levels to include. `0` is the folder itself, `-1` is all of them. |
| **Prefixes** | For `Name`: prefixes that are always stripped. |
| **Can Detect Prefixes** | For `Name`: also recognize prefixes that are not listed. |
| **Min Prefix Occurrences** | For `Name`: how many assets have to share a word before it counts as a prefix. |
| **Max Prefix Length** | For `Name`: the longest a prefix can be. |
| **Separator** | For `Name`: the character between name parts. Default is `_`. |
| **Is Prefix Case Ignored** | For `Name`: match prefixes regardless of upper or lower case. |

## Content

| Field | Meaning |
| --- | --- |
| **Zoo Scene** | The scene the zoo is built into. It opens by itself on build. Empty uses the open scene. |
| **Categories** | The groups, in the order they stand in. |

Each group has:

| Field | Meaning |
| --- | --- |
| **Name** | Shown on its zone in the Scene view. |
| **Label Color** | Color of its zone and of the dot next to its name. |
| **Entries** | The prefabs in this group. |
| **Is Hidden** | Leaves the whole group out of the zoo. |
| **Hidden Entries** | Assets of this group left out of the zoo. |

## Notes

- A scan only adds. What you moved, reordered or hid stays that way.
- Only **Start Over…** in the builder forgets your changes.