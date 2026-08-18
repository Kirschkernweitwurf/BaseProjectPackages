# Base Editor UI Package

The shared look of the Base editor windows. Editor only, no dependencies, so any
tool assembly can reference it without dragging anything else in.

Six style classes across four packages had each grown their own copy of the same
things: pro-skin color picking, the accent blue, the row stripe, the hover tint,
the hairline separator, the hover-lift and press-drop constants, the trick for
pinning a label's text color across all four states, and a nine-sliced rounded
rectangle generator. Those live here now.

What does **not** live here is anything one window alone understands. A badge
color for "group has no menu", a chip color for "creates an asset", the width of
one particular button: those stay in the window's own style class. This package
is the shared floor, not a replacement for per-window styling.

## Requirements

- Unity `6000.3` or newer
- No package dependencies

## Contents

### `EditorPalette`

Every color a window can name in general terms: `Accent`, `Text`, `DimText`,
`Background`, `Field`, `Card`, `Border`, `Stripe`, `Hover`, `Selection`,
`SelectionFill`, `Separator`, `Divider`, `KeyCap`, `Focus`, `Warning`, `Success`
and `Danger`. All resolve against the current skin.

Two building blocks sit underneath:

- `Pick(pro, personal)` returns the value for the active skin.
- `Tint(proAlpha, personalAlpha)` returns a neutral overlay that lightens on the
  dark skin and darkens on the light one, which is how nearly every subtle
  background tint in an editor window is built.

### `EditorMetrics`

Row height, header height, pill and badge height, corner radii, indent step, gap
sizes, separator and divider thickness, divider grab width, hover lift and press
drop.

### `EditorStyleUtility`

- `PinTextColor` pins a style's text color across normal, hover, active and
  focused. Without it, plain labels light up like buttons when the mouse passes
  over them.
- `Shade` brightens on hover and darkens on press, for hand-drawn buttons.
- `MutedTextColor` takes the muted gray from the skin rather than guessing it.
- `UniformPadding` and `HorizontalPadding` build `RectOffset` values.

### `EditorTextureCache`

Generates flat and nine-sliced rounded textures and owns them. Textures are
hidden and not saved, so the owner has to `Release` them. A rounded texture is a
`2 * radius + 1` square whose single center pixel stretches, so corners keep
their true size at any target rectangle. Set the style's `border` to the same
radius.

### `EditorStyleSet`

Base class for a window's style cache. Handles the two rules that are easy to get
wrong: styles have to be built inside a GUI call because `EditorStyles` is not
valid before that, and they have to be rebuilt when the user switches skin. Call
`EnsureBuilt` from `OnGUI` and `Dispose` from `OnDisable`, and implement `Build`.

### `EditorIcons`

The built-in editor icons the Base windows report findings with: `Error`,
`Warning`, `Success`, `Script` and `GameObject`, plus `Named` for anything else.
Resolved on first access, because `IconContent` is only valid inside a GUI
callback.

### `EditorFonts`

`Monospaced()` returns an OS monospaced font for source blocks. Created once per
domain and deliberately never destroyed: destroying it left every `GUIStyle`
built from it pointing at nothing, which Unity reports as a deleted invalid font
reference on the next reload.

### `EditorRows`

`DrawRowBackground` with striping, hover and selection precedence,
`DrawSeparator`, `MeasureBadge`, `DrawBadge` and `DrawIndentGuides`.

## Migration status

Windows are moved onto this layer one at a time, keeping each window's own style
class and its public members intact so no window code changes.

- Done: the Tool package's Menu Manager, Command Palette and Overview GUI, the
  Controller Support package's Navigation Groups window, and the Attribute
  package's Attribute Explorer, Troubleshoot and Required Reference windows.
- Not migrating: the Base Package Installer. It is the tool that installs this
  package, so in a fresh project it has to compile before any Base package
  exists. Its `InstallerTheme`, `InstallerStyles` and `TableColumnLayout` stay
  duplicated on purpose. With the installer excluded, a shared column layout
  would have had a single consumer, so it did not join this package either.