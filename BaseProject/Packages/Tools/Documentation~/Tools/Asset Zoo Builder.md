---
type: Tool
package: Tools
audience: Everyone
tags:
  - artist
  - assets
summary: Stands your art in the Scene view at true size, sorted by folder, so you can look at a whole library at once.
menu: Tools > Base Packages > Assets > Asset Zoo Builder
---
# Asset Zoo Builder

🪛 **Tool** · for everyone

**Tags:** #artist #assets

**Menu:** `Tools > Base Packages > Assets > Asset Zoo Builder`

Stands your art in the Scene view at true size, sorted by folder, so you can look at a whole library at once. Good for checking a new model in engine, comparing scale, reviews and screenshots.

## How to use it

1. Open the window. It finds the project's saved zoo by itself. A project without one asks you to drop a folder of art on the window.
2. Drop folders from the Project window on **What to Show**, or press **Add Folder…**.
3. Press **Build Zoo**. The Scene view flies to your art.

Made a new model? Press **Rebuild Zoo**. It looks for new art in your folders, marks it **New** in the Scene view and flies to it.

## In the Scene view

| What | What it does |
| --- | --- |
| Zones and group names | Show the blocks like a map. Click a name to fly there. |
| Asset names | Appear as you get closer and never overlap. |
| Hover a name | Shows a wire box, the real size, triangles and materials. |
| Click a name | Selects the asset and finds it in the Project window. |
| Double-click a name | Opens the prefab. |
| Yellow dot | Something to look at: an empty material slot, a broken shader, the default material, or a root scale other than 1. |
| Ruler | Stands at the start of every group, in meters. |

The labels live only in the Scene view, so Game view shots stay clean. The Gizmos button of the Scene view turns them off.

## The window

| Section | What it does |
| --- | --- |
| Saved Zoo | Remembers the folders, groups and look. Double-click one in the Project window to open it here. |
| What to Show | The folders. Removing one takes its assets out of the zoo. **Group By** sorts by folder or by the first word of the name. |
| Groups | Your art as pictures. See below. |
| Look | Updates the zoo while you drag. |
| More | The scene to build into, how groups stand next to each other, the floor material, and the rules for grouping by name. **Rescan** looks for new art without building. **Start Over…** forgets all your changes. |

## Groups

- The eye leaves a group or asset out of the zoo at once. A rescan keeps it out, so this is the way to get rid of something.
- The camera flies to a group. It looks from high enough that nothing tall is in the way.
- Click a thumbnail to find it in the Project window, double-click to fly to it.
- Drag thumbnails between groups, and prefabs in from the Project window.
- Drag a row to change the order of the groups.
- Plus adds an empty group of your own. A scan leaves it alone.

## What is left out

A scan only keeps art. It leaves out prefabs with nothing to see, like UI screens and managers, and models that already have a prefab, so everything shows up once. **Models With Prefab** under **More** brings those models back. The window says what it left out.

Things that only exist as particles or lights are not shown either.

## Notes

- A new zoo starts next to what is already in the scene, so hand placed art stays where it is.
- Drag the zoo root wherever you like, or under another object. A rebuild stays there.
- A decal shows nothing without a surface, so each one brings a small board.
- The footer warns when the open scene is part of the game, because saving it would ship the zoo. Setting **Zoo Scene** under **More** opens the right scene on every build.
- Nothing is written to the prefabs.