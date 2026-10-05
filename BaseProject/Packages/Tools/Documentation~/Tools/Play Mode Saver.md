# Play Mode Saver

🪛 **Tool** · for everyone

**Menu:** `Tools > Base Packages > Unity Editor > Play Mode Saver`

Keeps the tweaks you made while the game was running. Normally everything you change in Play mode is thrown away when you stop. This tool snapshots the components you picked and lets you apply them afterwards.

## How to use it

1. Enter Play mode.
2. Tune your values in the Inspector as usual.
3. Mark what you want to keep:
   - Right click a component header and choose **Save Play Mode Changes**, or
   - Right click a GameObject in the hierarchy and choose **Save Play Mode Changes** to mark all of its components.
4. Changed your mind? Right click the component and choose **Forget Play Mode Changes**.
5. Exit Play mode. The marked components are captured automatically.
6. Open the window. Each captured entry can be **applied** or **discarded**.

## The three lists

| List | When it is used |
| --- | --- |
| Marked | During Play mode. What you flagged so far. |
| Captured | After Play mode. Waiting for you to apply or discard. |
| History | What was applied, discarded or failed, with timestamps. |

## Notes

- The captured list is cleared when the next Play session starts, so apply what you want before pressing Play again.
- Object references are restored where possible. Scene objects, prefab internals and assets are handled separately. Anything that cannot be found is reported as unresolvable.
- Prefab instances are supported. The window shows which prefab an entry belongs to.
