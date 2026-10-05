# Auto Start Scene

🪛 **Tool** · for everyone

**Tags:** #artist #designer #sound #workflow #scenes

**Menu:** `Edit > Project Settings > Base Tools > Auto Start Scene`

Always enters Play mode from the same scene, no matter which scene you have open. Useful when the game needs a bootstrap or loading scene to start correctly.

## How to use it

1. Open Project Settings and go to **Base Tools > Auto Start Scene**.
2. Tick **Enable Auto Start**.
3. Drop a scene into the **Start Scene** field.

Now pressing Play always begins in that scene. When you stop, your original scene comes back.

## Defaults

If you leave the field empty, the first enabled scene in Build Settings is used. The panel tells you which one that is.

If there is no scene available at all, you get a warning to add one to Build Settings.

## Notes

- The setting is stored per user, so it does not affect anyone else on the team.
- Turning it off restores Unity's normal behaviour right away.
