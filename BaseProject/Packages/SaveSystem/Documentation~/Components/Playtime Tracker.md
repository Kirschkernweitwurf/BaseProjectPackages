# Playtime Tracker

🗞️ **Component** · programmers only

**Tags:** #programmer #saving #ui

Optional. Counts how long the player has been playing and stores the total in every save, so a load menu can show "12h 40m".

Put it on any GameObject that lives for the whole game, for example next to the [🗞️ Save Manager](Save%20Manager.md).

## Inspector settings

None. It starts counting on scene start.

## Behaviour

- Counts real time, so it keeps running even if the game is slowed down.
- `Pause()` and `Resume()` can be called from a `Button` OnClick or from code, for example when the player opens a menu you do not want to count.
- When a save is loaded, a programmer seeds the tracker with the play time from that save, so the counter continues instead of restarting.

## Notes

- Only one is needed for the whole game.
- Without this component saves still work, the play time is simply not recorded.
