# Resolution Setting

🔖 **Component** · for everyone

Saves the screen resolution.

Key: `Resolution`

## Inspector fields

None. Drop it on a GameObject and it works.

## What gets saved

A text label like `1920x1080`. Because it is a label and not a position in a list, a saved resolution stays correct even if the player changes monitors, as long as the new monitor supports it.

The default is whatever the screen is already showing the first time the game runs.

## Order matters

Put this **after** [🔖 Full Screen Mode Setting](<Full Screen Mode Setting.md>) in the Hierarchy. The resolution is applied using whatever full screen mode is currently active, so the mode has to be set first.

## Which UI to use

[🔖 Resolution Choice Element](<Resolution Choice Element.md>). It fills its own option list with the resolutions the player's monitor actually supports, so you never type them by hand.

Set its Setting Key to `Resolution`.

## Good to know

- If a saved resolution is not supported any more, the picker falls back to the first option in the list.
- Testing this in the Editor is unreliable. Build the game to check it properly.

## Setup checklist

- Sits below FullScreenModeSetting in the Hierarchy.
- A ResolutionChoiceElement with key `Resolution`.
