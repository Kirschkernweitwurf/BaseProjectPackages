# Base Tweening Package

A data driven tween system with runtime factories, ready-made components and authoring assets. Drop a component on a transform, renderer, graphic or text and drive its value from a reusable profile asset, or build tweens in code and never touch a component.

It was split out of the Core package because it was roughly forty percent of it while almost nothing depended on it: a project that wants menus or audio should not have to compile fifty tween files, and a project that only wants tweens should not have to install the rest of Core.

## Requirements

- Unity `6000.3` or newer
- `com.unity.ugui` `2.0.0` and TextMeshPro, for the graphic and text tweens
- `Base.ServicePackage` for `GameServiceBehaviour` and the shutdown pipeline
- `Base.AttributePackage` for the inspector attributes the components use
- `Base.UtilityPackage` for `CustomLogger`, `[DynamicCreateAssetMenu]` and `IMenuResettable`
- Assemblies: `Base.TweeningPackage` and `Base.TweeningPackage.Editor`

## The runtime core

`TweenRunner` is the central update service that advances every active tween and broadcasts its lifecycle events. `Tween<T>` interpolates from a start value to a target over time, capturing the start value lazily after any configured delay so an external change in the meantime is not tweened from a stale reading. `TweenBase` carries the lifecycle and the completion events, and `TweenSequence` runs several tweens in order or in parallel.

`TweenFX` is the shortcut for code-driven work: factory methods that create, start and return a configured tween for the common Unity components.

`TweenLerpUtility` holds unclamped interpolation for the supported types. Unclamped on purpose, so overshooting easings actually overshoot.

## Components

One component per property, in two flavors:

- **Fixed**, such as `FadeTween`: interpolates between two authored values.
- **Captured**, such as `FadeToTween`: interpolates from whatever the value was at `Awake` to a target.

`RotationByTween` is the one delta-relative component, tweening by an offset from the rotation at the moment the tween is created.

| Area | Components |
|---|---|
| Transform | `PositionTween`, `PositionToTween`, `RotationTween`, `RotationByTween`, `ScaleTween`, `ScaleToTween` |
| UI | `FadeTween`, `FadeToTween`, `GraphicColorTween`, `GraphicColorToTween`, `ImageColorTween`, `ImageColorToTween`, `ImageFillAmountTween`, `ImageFillAmountToTween`, `TmpAlphaTween`, `TmpAlphaToTween`, `TmpColorTween`, `TmpColorToTween` |
| Renderer | `SpriteRendererColorTween`, `SpriteRendererColorToTween` |

`TweenGroup` plays several of them as one sequence or in parallel, forward or reversed, which is what menu enter and exit animations are built from. It implements `IMenuResettable`, so a menu closing resets it to its baseline and the menu opens fresh next time. `UIEventTrigger` drives a group from hover and click, and `TweenController<T>` is a base for tracking and killing the tweens belonging to one target, such as a card or a unit.

## Authoring assets

A tween reads its setup from the first source that is turned on: a profile asset, then a shared settings asset, then its own inline fields. A custom inspector hides whatever an assigned asset already provides, so the fields on screen are always the ones that are actually in effect.

- `FloatTweenProfileSo`, `ColorTweenProfileSo` and `Vector3TweenProfileSo` bundle the values, timing and loop behavior of a tween, so many components share one authored setup.
- `TweenSettingsSo` is timing and looping only, for tuning a whole family of tweens from one place, for example "UI Snappy" or "Card Flip".

Which fields the inspector hides is decided by `[TweenValue]`, `[TweenProfileToggle]` and `[TweenSettingsToggle]` on the fields rather than by field name, so renaming a field cannot break the layout.

`EEasingType` covers the usual set (see [easings.net](https://easings.net/)), `ELoopType` and `LoopSettings` the looping, where a loop count is the number of extra plays *after* the first and each direction of a ping-pong counts as one.

## Who uses it

The Core package uses it for menu open and close animations, the debug menu and `TweenGroupObjectPool`. The Settings package uses it for the selection indicator. Nothing in this package reaches back into either.

## Install

Install through the Git Package Manager window. Installing by hand means installing `Base.UtilityPackage`, `Base.AttributePackage` and `Base.ServicePackage` first, in that order; UPM cannot resolve Base package dependencies for Git installs.