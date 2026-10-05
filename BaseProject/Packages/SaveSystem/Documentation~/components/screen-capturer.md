# Screen Capturer

Optional. Takes a screenshot when the game is saved and stores it in the slot as a thumbnail.

Put it on any GameObject that lives for the whole game, for example next to the [🔧 Save Manager](save-manager.md).

## Inspector settings

None. It works as soon as it is in the scene.

## Behaviour

- The screenshot is taken at the end of the frame, so the UI you see is included.
- It is scaled down to about 480 pixels wide, which keeps save files small.
- Without this component saves still work, they just have no thumbnail.

## Showing the thumbnail in a menu

The image is stored as PNG bytes in the slot. A programmer loads it and turns it into a texture for your `Image` component. You just provide the slot for it.

## Notes

- Only one is needed for the whole game.
- If your save button sits on a pause screen, the screenshot will show that pause screen. Hide the menu for a frame first if you want a clean shot.
