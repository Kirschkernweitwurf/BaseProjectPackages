# Controller Support

Full gamepad support for uGUI menus: reliable navigation wiring, focus that never gets lost, stick scrolling, device-aware button prompts and priority-stacked rumble. Drop it in, mark your selectables, press play.

## Features

- **Explicit navigation wiring** built from on-screen positions, with optional edge wrapping
- **Focus watchdog** that restores a valid selection whenever the gamepad loses it
- **Priority system** so the right group wins focus when several menus are open
- **Last-selected memory** per group, so reopening a menu feels natural
- **Stick scrolling** for ScrollRects and automatic scroll-into-view for selected elements
- **Input device tracking** that flips between mouse/keyboard and gamepad on real actuation
- **Prompt glyphs** per device family, as sprites or inline TextMeshPro tags
- **Curve-based rumble** with priority stacking, so competing haptics resolve instead of fighting
- **Navigation Groups window** for jumping to, inspecting and rebuilding groups per group, per scene or project wide

## Core Concepts

### NavigableElement

Marker component for a `Selectable` that should be part of gamepad navigation. Only marked selectables get wired. Anything without it is treated as a gap and fixed on demand during a rebuild (a `NavigableElement` is added, its selectable is assigned and the fix is logged).

### NavigableGroup

A self-contained navigation context. Put it on a menu root and it collects all `NavigableElement`s beneath it, wires explicit four-way navigation between them by proximity and exposes a default focus target.

| Setting | Effect |
| --- | --- |
| Default Element | Selected when the group gains focus and nothing is remembered |
| Priority | Higher priority groups win focus restoration when several are active |
| Auto Activate | Group activates itself in `OnEnable` |
| Remember Last Selected | Focus returns to the last used element instead of the default |
| Wrap | Navigation loops around the edges of the group |

Rebuild via the inspector buttons or the Navigation Groups window. Both go through `NavigationRebuildService`, which adds missing elements, rewires and saves in one step. `NavigableGroup.Rebuild()` itself only rewires, so runtime code can call it without any editor dependency.

### FocusWatchdog

Global service that keeps the UI alive for gamepad users. When the current selection becomes null or inactive, it restores focus to the highest priority active group. With an `InputDeviceTracker` present, it only guards focus while the gamepad is the active device, so mouse users can deselect freely.

Ties on priority go to the most recently activated group.

### MenuNavigationModule

The single seam between the menu layer and this package. Attach it to a menu and assign a group: the group activates when the menu opens and deactivates when it closes. It warns at startup when the group's Auto Activate or priority disagrees with the menu. Navigation stays menu-agnostic everywhere else.

## Scrolling

- **GamepadScrollRect**: drives a `ScrollRect` with a Vector2 action (typically the right stick). Uses unscaled time, so it works while menus pause the game. Configurable speed, dead zone and vertical inversion.
- **ScrollIntoView**: keeps the selected child of a `ScrollRect` visible inside the viewport, with configurable edge padding. uGUI does not do this on its own, so long lists need it.

## Input Prompts

- **InputDeviceTracker**: service that tracks the active device family (`MouseKeyboard` or `Gamepad`) based on real actuation, ignoring noise like resting sticks. Raises `OnDeviceChanged`.
- **InputGlyphSet**: ScriptableObject mapping input actions to glyphs for one device family. Create it from the asset menu (`Scriptable Objects/Base/Input/New Glyph Set`), one set per device.
- **InputGlyphProvider**: service that resolves the right glyph for the active device. `TryGetSprite` for images, `TryGetTmpSpriteTag` for inline TextMeshPro tags. Raises `OnActiveDeviceChanged` so labels can refresh.

## Haptics

- **RumblePatternData**: the curves and timing of one haptic. Both motors are authored over normalized time, so the same shape stretches to any duration. Serializable on its own, so it can sit inline on a component or be built in code.
- **RumblePattern**: asset wrapper around `RumblePatternData`, for haptics shared across the project. Create it from the asset menu (`Scriptable Objects/Base/Input/New Rumble Pattern`). In play mode the inspector has Preview and Stop buttons, so a curve can be tuned against the real pad instead of by guesswork.
- **RumbleRequest**: one live playback, with its own clock.
- **RumbleConfig**: asset holding the project defaults (rumble on, main intensity). The `RumbleService` and the settings components all point at the same asset, so a default is authored once instead of retyped per component. Create it from the asset menu (`Scriptable Objects/Base/Input/New Rumble Config`).
- **RumbleService**: owns the motors. Requests stack in a `PriorityTracker<RumbleRequest>`. Reads its starting state from the config, then a settings component overrides it with the player's choice.

| Setting | Effect |
| --- | --- |
| Duration | Length of one pass over the curves |
| Loop | Repeats until stopped explicitly |
| Use Unscaled Time | Keeps playing while the game is paused |
| Low Frequency | Heavy, rolling motor over normalized time |
| High Frequency | Light, buzzing motor over normalized time |

### How stacking resolves

Only the highest priority request reaches the motors, and ties go to the most recent one. Every request keeps advancing its own clock while it is outranked, so a preempted burst expires on schedule instead of waiting in the stack and firing late. A looping ambient pattern that loses to a hit therefore resumes mid-loop rather than restarting.

Non-looping requests remove themselves when they expire. Looping ones run until `Stop`.

The service stops the motors on focus loss, app pause, disable and destroy, and hands over cleanly when the pad is swapped or unplugged mid-pattern.

### Playing a rumble

```csharp
if (ServiceLocator.TryGet(out RumbleService rumbleService))
    rumbleService.Play(hitPattern, this, EPriority.High);
```

A second `Play` from the same caller replaces the first, so retriggering restarts the pattern instead of stacking copies of it. Stop it again with `rumbleService.Stop(this)`.

For a one-off without an asset:

```csharp
rumbleService.PlayBurst(low: 0.6f, high: 0.2f, duration: 0.15f, caller: this);
```

### Settings

`RumbleService` owns the player-facing state directly. `SetRumbleEnabled` gates the motors, `SetMainIntensity` scales every request and `OnRumbleEnabledChanged` lets UI follow the state. Defaults come from the `RumbleConfig` asset and `RumbleSettingKeys` holds the `PersistentKey`s both values persist under, so the component that writes a value, the service that reads it and the asset that seeds it cannot drift apart.

Persisting those values is the Settings package's job, but the two components that do it ship here, in `Scripts/Runtime/Haptics/Settings`. That assembly (`Base.ControllerSupportPackage.Settings`) only compiles when `com.baseprojectpackages.settings` is installed, so this package stays usable without it. Install the Settings package if you want a rumble toggle and strength slider in your options menu, and assign both components the same `RumbleConfig` the service uses.

## Quick Start

1. Add `FocusWatchdog`, `InputDeviceTracker`, `InputGlyphProvider` and `RumbleService` to your service scene.
2. Put a `NavigableGroup` on each menu root and assign its Default Element.
3. Add `NavigableElement` to your selectables or just hit Rebuild and let the validator add them.
4. Bridge menus with `MenuNavigationModule`.
5. For long lists, add `ScrollIntoView` (and optionally `GamepadScrollRect`) to the ScrollRect.
6. Author one `InputGlyphSet` per device family and assign both to the `InputGlyphProvider`.
7. Create one `RumbleConfig` and assign it to the `RumbleService`.
8. Author a `RumblePattern` per haptic and tune it with the Preview button in play mode.

## Editor Tooling

Rebuilds only ever run when you trigger them, so wiring never changes silently.

- **Navigation Groups window** (`Tools > Base Packages > Unity Editor > Controller Navigation Groups`): lists every group in the loaded scenes with its scene, priority and element count. Per row you can go to a group, rebuild it or fix a missing element. The toolbar rebuilds every group in the loaded scenes or the whole project. The project rebuild opens every scene, rewires all groups and saves the scenes, and rebuilds any prefabs that contain groups too.
- **Inspector**: Rebuild and Rebuild Scene buttons on every `NavigableGroup`.

## Upgrading to 1.5.0

New `Haptics` folder. Create a `RumbleConfig` asset and add a `RumbleService` to your service scene next to `FocusWatchdog` and `InputDeviceTracker`, with the config assigned. Nothing existing changes.

## Upgrading to 1.3.0

`NavigableElement`, `GamepadScrollRect` and `ScrollIntoView` now hold their sibling component in a serialized `[GetComponent]` field instead of resolving it at runtime. Existing scenes and prefabs need those fields filled once. Open them, or run the GetComponent batch assigner from the Attributes package, or simply hit Rebuild Project in the Navigation Groups window.

## Dependencies

- Unity Input System
- Base Service Package (`ServiceLocator`, `GameServiceBehaviour`, `EPriority`, `PriorityTracker`)
- Base Core Package (menu managing)
- Base Utility Package (`CustomLogger`, `DynamicCreateAssetMenu`, `DynamicMenuItem`, `PersistentKey`)
- Base Attribute Package (`[Required]`, `[GetComponent]`, `[Child]`, `[Title]`, `[CurveRange]`, `[Percentage]`, `[Button]`)
- Base Editor UI Package (the shared look of the Navigation Groups window)
- TextMeshPro (for inline glyph tags)
- Base Settings Package, optional. Only needed for `RumbleEnabledSetting` and `RumbleIntensitySetting`; without it that assembly is skipped.

Assemblies are `Base.ControllerSupportPackage`, `Base.ControllerSupportPackage.Editor` and the optional `Base.ControllerSupportPackage.Settings`.