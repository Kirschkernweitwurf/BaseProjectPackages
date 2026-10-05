# FPS Counter

Shows the current framerate on screen. A debug tool, not a player-facing feature.

## Where to put it

On a GameObject in your UI, usually next to the text field it writes into.

## Fields

| Field | Required | What it does |
| --- | --- | --- |
| Show In Release Builds | No  | Off by default. Off means it hides itself in the finished game and only shows in the Editor and test builds. |
| Fps Text | Yes | The text field it writes into. |

## Good to know

- The text field must be **TextMeshPro**.
- The number updates twice a second so it stays readable instead of flickering.
- Leave **Show In Release Builds** off unless you have a reason. Nothing to clean up before shipping then.
