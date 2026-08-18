# Base Tweening Package

A data driven tween system with runtime factories, ready-made components and
authoring assets. Drop a component on a transform, renderer, graphic or text and
drive its value from a reusable profile asset.

It was split out of the Core package because it was roughly forty percent of it
while almost nothing depended on it: a project that wants menus or audio should
not have to compile fifty tween files, and a project that only wants tweens
should not have to install the rest of Core.

## Requirements

- Unity `6000.3` or newer
- TextMeshPro, for the text tweens

### Related Base packages

- `Base.ServicePackage` for `GameServiceBehaviour` and the shutdown pipeline
- `Base.AttributePackage` for the inspector attributes the components use
- `Base.UtilityPackage` for `CustomLogger`, `[DynamicCreateAssetMenu]` and
  `IMenuResettable`

## Systems

- `TweenFX` provides high-level factory methods for common components.
- `Tween`, `TweenBase`, `TweenSequence` and `TweenRunner` form the runtime core.
- Component tweens cover transforms, renderers, images and TextMeshPro. Each
  comes in three flavors: `FadeTween` (fixed), `FadeToTween` (captured start)
  and `FadeByTween` (delta relative).
- `TweenGroup` plays several tweens as one sequence or in parallel, and resets
  itself through `IMenuResettable` when an owning menu closes.
- Profile assets (`FloatTweenProfileSo`, `ColorTweenProfileSo`,
  `Vector3TweenProfileSo` and `TweenSettingsSo`) let many components share one
  authored setup. A custom inspector hides fields that an assigned profile
  already provides.

## Who uses it

The Core package uses it for menu open and close animations, the debug menu and
`TweenGroupObjectPool`. The Settings package uses it for the selection
indicator. Nothing in this package reaches back into Core.

## Install

Install through the Git Package Manager window or add the Git URL directly.
Install `Base.UtilityPackage`, `Base.AttributePackage` and `Base.ServicePackage`
first; UPM cannot resolve Base package dependencies for Git installs, so the
order is manual.

## Upgrading from Core

Namespaces changed from `Base.CorePackage.Tweening.*` to `Base.TweeningPackage.*`
and the editor namespace from `Base.CorePackage.Editor.Tweening` to
`Base.TweeningPackage.Editor`. Script GUIDs are unchanged, so scene and prefab
references survive. The four profile assets carry `[DynamicCreateAssetMenu]`, and
their `MenuEntry` ids are derived from the type name, so those four entries have
to be re-added in the Menu Manager once.