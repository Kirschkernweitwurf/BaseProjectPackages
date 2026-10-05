# Reserialize Assets

🔧 **Tool** · programmers only

**Tags:** #programmer #assets

**Menu:** `Tools > Base Packages > Assets > Reserialize Assets`

Makes a renamed field actually stick. When you rename a serialized field and add `[FormerlySerializedAs]`, Unity quietly moves the old value into the new field every time it loads the asset. It never writes the file back, so the prefab on disk still holds the old name and the day you drop the attribute the value disappears. This tool rewrites the files so the new name is really there.

## How to use it

1. Select the folders you care about in the Project window and press **Add Selected Folders**.
2. Tick which kinds to include: prefabs, scenes, ScriptableObjects.
3. Press **Count Matching Assets** to see the blast radius before anything happens.
4. Press **Reserialize** and confirm.
5. Review the diff, then commit.

## The two buttons

| Button | What it does |
| --- | --- |
| Count Matching Assets | Runs the search and nothing else. Safe to press at any time. |
| Reserialize | Rewrites every match, after a confirm dialog showing the count. |

## Notes

- **Commit first.** The rewrite uses the serializer of the editor you are running, so the diff is bigger than your rename. Formatting Unity does not consider load-bearing gets refreshed along the way.
- **Keep the scope small.** Running on the whole project works and is sometimes what you want, but it touches assets that have nothing to do with your rename and buries the change you were actually after.
- An empty folder list means the whole project. The window says so, so you cannot do it by accident.
- Meta files are rewritten alongside the asset, which is what a field rename needs.
- This only fixes assets in this project. If the renamed field lives in a shared package, every project using it needs its own run before you can safely delete the `[FormerlySerializedAs]` attribute.
