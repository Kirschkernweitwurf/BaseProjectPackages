# Base Editor UI Package

The shared look of the Base editor windows. Editor only, no dependencies, so any tool assembly can reference it without dragging anything else in.

Skin-aware color picking, the row stripe, hover and selection tints, the hairline separator, rounded nine-sliced backgrounds for cards, pills and buttons, badge measuring and drawing, and the style-cache lifecycle every window otherwise reimplements.

What does **not** live here is anything one window alone understands. A badge color for "group has no menu", a chip color for "creates an asset", the width of one particular button: those stay in the window's own style class. This package is the shared floor, not a replacement for per-window styling.

## Requirements

- Unity `6000.3` or newer
- No dependencies, Base or otherwise
- One assembly: `Base.EditorUiPackage`, Editor platform only

## Contents

| Type | What it holds |
|---|---|
| `EditorPalette` | Every color a window can name in general terms, resolved against the active skin |
| `EditorMetrics` | Row and header height, pill and badge height, corner radii, indent step, gaps, separator and divider thickness, hover lift and press drop |
| `EditorStyleSet` | Base class for a window's style cache |
| `EditorStyleUtility` | The small helpers a window needs while building its own styles |
| `EditorTextureCache` | Generates and owns the flat and rounded background textures |
| `EditorRows` | Striped rows, hover and selection tints, separators, badges, indent guides |
| `EditorIcons` | The built-in editor icons the Base windows report findings with |
| `EditorFonts` | A monospaced font for source blocks |
| `ESortOrder` | The three states a sortable column header cycles through |

## The parts worth knowing

`EditorPalette` exposes `Accent`, `Text`, `DimText`, `Background`, `Field`, `Card`, `Border`, `Stripe`, `Hover`, `Selection`, `SelectionFill`, `Separator`, `Divider`, `KeyCap`, `Focus`, `Warning`, `Success` and `Danger`, all resolved for the active skin. Two building blocks sit underneath: `Pick(pro, personal)` returns the value for the current skin, and `Tint(proAlpha, personalAlpha)` returns a neutral overlay that lightens on the dark skin and darkens on the light one, which is how nearly every subtle background tint in an editor window is built.

`EditorStyleSet` handles the two rules that are easy to get wrong: styles have to be built inside a GUI call because `EditorStyles` is not valid before that, and they have to be rebuilt when the user switches skin. Call `EnsureBuilt` from `OnGUI` and `Dispose` from `OnDisable`, and implement `Build`.

```csharp
internal sealed class MyWindowStyles : EditorStyleSet
{
    internal GUIStyle Row { get; private set; }

    protected override void Build()
    {
        Row = new GUIStyle(EditorStyles.label);
        EditorStyleUtility.PinTextColor(Row, EditorPalette.Text);
    }
}
```

`EditorStyleUtility.PinTextColor` pins a style's text color across normal, hover, active and focused. Without it, plain labels light up like buttons when the mouse passes over them. `Shade` brightens on hover and darkens on press for hand-drawn buttons, `MutedTextColor` takes the muted gray from the skin rather than guessing it, and `UniformPadding` and `HorizontalPadding` build `RectOffset` values.

`EditorTextureCache` textures are hidden and not saved, so the owner has to `Release` them. A rounded texture is a `2 * radius + 1` square whose single center pixel stretches, so corners keep their true size at any target rectangle. Set the style's `border` to the same radius.

`EditorFonts.Monospaced()` is created once per domain and deliberately never destroyed: destroying it left every `GUIStyle` built from it pointing at nothing, which Unity reports as a deleted invalid font reference on the next reload.

## Who uses it

The Attributes, Controller Support, Core, Services and Tools packages all build their windows on this. The Base Package Installer deliberately does not: it is the tool that installs this package, so in a fresh project it has to compile before any Base package exists.