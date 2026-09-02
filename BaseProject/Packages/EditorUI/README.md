# Base Editor UI

The shared look of the Base editor windows. Editor only, no dependencies, so any tool assembly can reference it without dragging anything else in.

Every color, corner radius, row height and gap comes from a **theme** the project owns. Assign one under `Project Settings > Base Tools > Editor UI Theme`, drag a slider, and every open Base window redraws with it. With no theme assigned the package draws with its built-in look, which is exactly what it drew with before themes existed.

What does **not** live here is anything one window alone understands. A badge color for "group has no menu", a chip color for "creates an asset", the width of one particular button: those stay in the window's own style class. This package is the shared floor, not a replacement for per-window styling.

## Requirements

- Unity `6000.3` or newer
- No dependencies, Base or otherwise
- One assembly: `Base.EditorUIPackage.Editor`, Editor platform only

## Contents

| Type | What it holds |
|---|---|
| `EditorPalette` | Every color a window can name in general terms, resolved against the active skin and theme |
| `EditorMetrics` | Row and header height, pill and badge height, corner radii, indent step, gaps, separator and divider thickness, hover lift and press drop |
| `EditorTableStyles` | The whole look of a list window: card, striped rows, badges, ping button, toolbar, empty state |
| `EditorStyleSet` | Base class for a window's style cache |
| `EditorSkinWatch` | The same staleness check for a cache that cannot inherit, because it is static |
| `EditorStyleUtility` | The small helpers a window needs while building its own styles |
| `EditorTextureCache` | Generates and owns the flat and rounded background textures |
| `EditorRows` | Striped rows, hover and selection tints, separators, badges, sort arrows, indent guides |
| `EditorColumnDividers` | The draggable lines between the columns of a table |
| `EditorIcons` | The built-in editor icons the Base windows report findings with |
| `EditorFonts` | A monospaced font for source blocks |
| `EditorStyleSheets` | Loads and attaches a USS sheet by asset GUID, for UI Toolkit windows |
| `ESortOrder` | The three states a sortable column header cycles through |
| `EEditorDividerAction` | What a divider did during one event |

Under `Editor/Theme`:

| Type | What it holds |
|---|---|
| `EditorTheme` | The asset: colors for both skins, layout metrics, list window metrics |
| `EditorThemeColors` | The 21 colors of one editor skin |
| `EditorThemeMetrics` | The 18 spacings, sizes and corner radii every window lays out by |
| `EditorThemeTable` | The 21 numbers and opacities a list window is built from |
| `EditorThemeDefaults` | The built-in look, and the only file here allowed to spell numbers out |
| `EditorThemeSettings` | Which theme the project uses, stored in `ProjectSettings` by asset GUID |
| `EditorThemeProvider` | Resolves and caches the active look, and raises a revision when it moves |
| `EditorThemeSettingsProvider` | The settings page, with the live preview |
| `EditorThemeGui` | The editable body of a theme, shared by the page and the asset inspector |
| `EditorThemePreview` | A miniature list window drawn from the real styles |
| `EditorThemeAssetFactory` | Creates a theme asset and makes it active |
| `EditorThemeInspector` | The inspector of a theme asset |

## One namespace for the whole package

Every type here declares `Base.EditorUIPackage.Editor`, including the ones under `Theme` and `Uss`. Same
reason as the Attributes package: a window pulling in a palette, some metrics, a row helper and a
style set should need one using line, not four. The folders are marked as non-namespace-providers
(Rider: **folder > Properties > Namespace provider: off**), so a folder-to-namespace scan reporting
these is reporting the convention.

## Theming

A theme is a `ScriptableObject` in your project, so it can be version controlled, shared and switched. Which one is active is stored in `ProjectSettings/BaseEditorTheme.asset` by GUID, so the choice travels with the project rather than with the machine and survives the asset being renamed or moved.

1. Open `Project Settings > Base Tools > Editor UI Theme`
2. Hit **Create Theme Asset** and pick a place for it
3. Edit anything under **Dark Skin Colors**, **Light Skin Colors**, **Layout** or **List Windows**

The preview at the top of the page is a real list window drawn from the real styles, so what you see there is what the Service Locator, Event Bus, Todo and attribute windows will look like. **Use Built-in Look** puts the project back on the defaults without deleting the asset, and **Reset To Built-in Look** puts the asset itself back.

Each of the two editor skins carries its own finished colors rather than a shared color plus an opacity, so changing one skin never moves the other. What used to be written as a neutral overlay is simply white or black at a low opacity in the dark and light sets.

### How a change reaches a window

`EditorThemeProvider` caches the resolved look, because a list window reads several values per row per repaint. Changing anything calls `NotifyChanged`, which drops the cache, raises `Revision` by one and repaints every view.

A style cache notices by comparing the revision it built with. `EditorStyleSet` and `EditorSkinWatch` both do this already, so a window that uses either gets theme support with no code of its own:

```csharp
internal sealed class MyWindowStyles : EditorStyleSet
{
    internal GUIStyle Row { get; private set; }

    protected override void Build()
    {
        Row = EditorStyleUtility.PinTextColor(new GUIStyle(EditorStyles.label), EditorPalette.Text);
    }
}
```

Caches poll the revision rather than subscribing to an event on purpose. A static event outlives play mode while Domain Reload is off, and would keep handing frames to windows that closed long ago. An integer cannot.

A static style cache cannot inherit `EditorStyleSet`, so it holds an `EditorSkinWatch` instead:

```csharp
private static readonly EditorSkinWatch Watch = new();

private static void EnsureBuilt()
{
    if (!Watch.IsStale)
        return;

    // build the styles

    Watch.MarkFresh();
}
```

Asking and answering are two calls rather than one because rebuilding can fail. Reading an editor style while a dropdown owns the GUI throws, and a cache that marked itself fresh before that happened would stay half built for the rest of the session.

### Why these are properties and not constants

`EditorMetrics` and `EditorTableStyles` expose sizes as static properties. A constant is copied into every assembly that reads it at compile time, so a themed value would stay baked into whatever it happened to be when that assembly was last built. Nothing in a Base package may declare `const float X = EditorMetrics.Y;` for the same reason.

## The parts worth knowing

`EditorPalette` exposes `Accent`, `AccentText`, `Text`, `DimText`, `Background`, `Field`, `Card`, `Border`, `Secondary`, `SecondaryText`, `Stripe`, `Hover`, `Selection`, `SelectionFill`, `Separator`, `Divider`, `KeyCap`, `Focus`, `Warning`, `Success` and `Danger`, all resolved for the active skin and theme. Three building blocks sit underneath, for the colors a window defines itself: `Pick(pro, personal)` returns the value for the current skin, `Tint(proAlpha, personalAlpha)` returns a neutral overlay that lightens on the dark skin and darkens on the light one, and `WithAlpha(color, alpha)` fades a color, which is how a badge or a row tint is built.

`EditorTableStyles` is the whole look of a list window, and the thing a new list window should start from rather than copy. Subclass it and add only what is yours:

```csharp
internal sealed class MyListStyles : EditorTableStyles
{
    internal const float MinWindowWidth = 660f;

    internal static Color MyStateBadgeColor => BadgeFill(EditorPalette.Focus);
}
```

Its static members are reachable through the subclass name, so `MyListStyles.RowInset` and `MyListStyles.EmptyIconRect(area)` work without redeclaring anything. `BadgeFill` mixes any color at the theme's badge opacity, for a state the shared fills have no name for.

`EditorStyleUtility.PinTextColor` pins a style's text color across normal, hover, active and focused. Without it, plain labels light up like buttons when the mouse passes over them. `BuildFilledButton` draws the rounded accent or muted button, generating its three backgrounds into the caller's texture cache. `Shade` brightens on hover and darkens on press, `MutedTextColor` takes the muted gray from the skin rather than guessing it, and `UniformPadding` and `HorizontalPadding` build `RectOffset` values.

`EditorTextureCache` textures are hidden and not saved, so the owner has to `Release` them. A rounded texture is a `2 * radius + 1` square whose single center pixel stretches, so corners keep their true size at any target rectangle. Set the style's `border` to the same radius.

`EditorFonts.Monospaced()` is created once per domain and deliberately never destroyed: destroying it left every `GUIStyle` built from it pointing at nothing, which Unity reports as a deleted invalid font reference on the next reload.

## Who uses it

The Attributes, Controller Support, Core, Services and Tools packages all build their windows on this. The Base Package Installer deliberately does not: it is the tool that installs this package, so in a fresh project it has to compile before any Base package exists.