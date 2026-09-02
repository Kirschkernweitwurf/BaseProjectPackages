# Base Controller Support

Full gamepad support for uGUI menus: navigation wiring built from on-screen positions, focus that never gets lost, stick scrolling, device-aware button prompts and priority-stacked rumble.

## Requirements

- Unity `6000.3` or newer
- `com.unity.inputsystem` `1.19.0` and TextMeshPro
- `Base.CorePackage` for the menu layer
- `Base.ServicesPackage` for `ServiceLocator`, `GameServiceBehaviour`, `EPriority` and `PriorityTracker`
- `Base.UtilityPackage` for `CustomLogger`, `PersistentKey` and the dynamic menu attributes
- `Base.AttributesPackage` for the inspector attributes
- `Base.EditorUIPackage.Editor` for the Navigation Groups window
- `Base.SettingsPackage`, optional, only for the two rumble setting components
- Assemblies: `Base.ControllerSupportPackage`, `Base.ControllerSupportPackage.Editor` and the optional `Base.ControllerSupportPackage.Settings`

## Quick start

1. Add `FocusWatchdog`, `InputDeviceTracker`, `InputGlyphProvider` and `RumbleService` to your service scene.
2. Put a `NavigableGroup` on each menu root and assign its Default Element.
3. Add `NavigableElement` to your selectables, or just hit Rebuild and let the validator add them.
4. Bridge menus with `MenuNavigationModule`.
5. For long lists, add `ScrollIntoView` and optionally `GamepadScrollRect` to the ScrollRect.
6. Author one `InputGlyphSet` per device family and assign both to the `InputGlyphProvider`.
7. Create one `RumbleConfig`, assign it to the `RumbleService`, and author a `RumblePattern` per haptic.

## Navigation

### NavigableElement

Marker component for a `Selectable` that should be part of gamepad navigation. Only marked selectables get wired. Anything without it is a gap, fixed on demand during a rebuild: the component is added, its selectable assigned and the fix logged.

### NavigableGroup

A self-contained navigation context. Put it on a menu root and it collects every `NavigableElement` beneath it, wires explicit four-way navigation between them by proximity and exposes a default focus target.

| Setting | Effect |
| --- | --- |
| Default Element | Selected when the group gains focus and nothing is remembered |
| Priority | Higher priority groups win focus restoration when several are active |
| Auto Activate | Group activates itself in `OnEnable` |
| Remember Last Selected | Focus returns to the last used element instead of the default |
| Wrap | Navigation loops around the edges of the group |

Rebuild from the inspector buttons or the Navigation Groups window. Both go through `NavigationRebuildService`, which adds missing elements, rewires and saves in one step. `NavigableGroup.Rebuild()` itself only rewires, so runtime code can call it without any editor dependency.

Rebuilds only ever run when you trigger them, so wiring never changes silently.

### FocusWatchdog

Keeps the UI alive for gamepad users. When the current selection becomes null or inactive it restores focus to the highest priority active group, with ties going to the most recently activated one. With an `InputDeviceTracker` present it only guards focus while the gamepad is the active device, so mouse users can deselect freely.

### MenuNavigationModule

The single seam between the menu layer and this package. Attach it to a menu and assign a group: the group activates when the menu opens and deactivates when it closes. It warns at startup when the group's Auto Activate or priority disagrees with the menu. Navigation stays menu-agnostic everywhere else.

## Scrolling

- **GamepadScrollRect** drives a `ScrollRect` from a Vector2 action, typically the right stick. Unscaled time, so it keeps working while menus pause the game. Configurable speed, dead zone and vertical inversion.
- **ScrollIntoView** keeps the selected child of a `ScrollRect` inside the viewport, with configurable edge padding. uGUI does not do this for gamepad navigation, so long lists scroll the selection out of sight without it.

## Input prompts

`InputDeviceTracker` is the single source of truth for the active device family (`MouseKeyboard` or `Gamepad`), flipping only on real actuation so a resting stick does not count. It raises `OnDeviceChanged`.

`InputGlyphSet` maps input actions to glyphs for one device family; author one asset per device. `InputGlyphProvider` resolves the right glyph for the active device through `TryGetSprite` for images or `TryGetTmpSpriteTag` for inline TextMeshPro tags, and raises `OnActiveDeviceChanged` so labels can refresh themselves.

## Haptics

`RumblePatternData` holds the curves and timing of one haptic. Both motors are authored over normalized time, so the same shape stretches to any duration. It is serializable on its own, so a pattern can sit inline on a component, and `RumblePattern` is the asset wrapper for haptics shared across the project. In play mode its inspector has Preview and Stop buttons, so a curve can be tuned against the real pad instead of by guesswork.

`RumbleConfig` holds the project defaults (rumble on, main intensity). The `RumbleService` and the settings components all point at the same asset, so a default is authored once instead of retyped per component.

```csharp
if (ServiceLocator.TryGet(out RumbleService rumbleService))
    rumbleService.Play(hitPattern, this, EPriority.High);
```

A second `Play` from the same caller replaces the first, so retriggering restarts the pattern instead of stacking copies. Stop it with `rumbleService.Stop(this)`. For a one-off without an asset:

```csharp
rumbleService.PlayBurst(low: 0.6f, high: 0.2f, duration: 0.15f, caller: this);
```

### How stacking resolves

Requests stack in a `PriorityTracker<RumbleRequest>`. Only the highest priority one reaches the motors, and ties go to the most recent. Every request keeps advancing its own clock while it is outranked, so a preempted burst expires on schedule instead of firing late, and a looping ambient pattern that loses to a hit resumes mid-loop rather than restarting.

Non-looping requests remove themselves when they expire; looping ones run until `Stop`. The service stops the motors on focus loss, app pause, disable and destroy, and hands over cleanly when the pad is swapped or unplugged mid-pattern.

### Player settings

`RumbleService` owns the player-facing state directly: `SetRumbleEnabled` gates the motors, `SetMainIntensity` scales every request, and `OnRumbleEnabledChanged` lets UI follow along. `RumbleSettingKeys` holds the `PersistentKey`s both values persist under, so the component that writes a value, the service that reads it and the asset that seeds it cannot drift apart.

Persisting those values is the Settings package's job, but the two components that do it ship here, in `Runtime/Haptics/Settings`. That assembly only compiles when `com.baseprojectpackages.settings` is installed, so this package stays usable without it. Install the Settings package for a rumble toggle and strength slider in your options menu, and assign both components the same `RumbleConfig` the service uses.

## Navigation Groups window

`Tools > Base Packages > Unity Editor > Controller Navigation Groups`

Lists every `NavigableGroup` in the loaded scenes with its menu, scene, priority and element count. Per row you can jump to a group, rebuild it or fix a missing element. The toolbar rebuilds every group in the loaded scenes or across the whole project; the project rebuild opens every scene, rewires all groups, saves the scenes and rebuilds any prefabs containing groups too.

It also reports the menu rule violations that follow from a group sitting on a menu: a group on a menu has to leave Auto Activate off, since the menu is the thing that activates it.