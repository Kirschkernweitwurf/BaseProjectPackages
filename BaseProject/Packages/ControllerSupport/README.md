# Controller Support

Full gamepad support for uGUI menus: reliable navigation wiring, focus that never gets lost, stick scrolling and device-aware button prompts. Drop it in, mark your selectables, press play.

## Features

- **Explicit navigation wiring** built from on-screen positions, with optional edge wrapping
- **Focus watchdog** that restores a valid selection whenever the gamepad loses it
- **Priority system** so the right group wins focus when several menus are open
- **Last-selected memory** per group, so reopening a menu feels natural
- **Stick scrolling** for ScrollRects and automatic scroll-into-view for selected elements
- **Input device tracking** that flips between mouse/keyboard and gamepad on real actuation
- **Prompt glyphs** per device family, as sprites or inline TextMeshPro tags
- **Navigation Groups window** for jumping to, inspecting and rebuilding groups per group, per scene or project wide

## Core Concepts

### NavigableElement

Marker component for a `Selectable` that should be part of gamepad navigation. Only marked selectables get wired. Anything without it is treated as a gap and fixed automatically during a rebuild (a `NavigableElement` is added and the fix is logged).

### NavigableGroup

A self-contained navigation context. Put it on a menu root and it collects all `NavigableElement`s beneath it, wires explicit four-way navigation between them by proximity and exposes a default focus target.

| Setting | Effect |
| --- | --- |
| Default Element | Selected when the group gains focus and nothing is remembered |
| Priority | Higher priority groups win focus restoration when several are active |
| Auto Activate | Group activates itself in `OnEnable` |
| Remember Last Selected | Focus returns to the last used element instead of the default |
| Wrap | Navigation loops around the edges of the group |

Rebuild via the inspector buttons, the context menu or `Rebuild()` in code.

### FocusWatchdog

Global service that keeps the UI alive for gamepad users. When the current selection becomes null or inactive, it restores focus to the highest priority active group. With an `InputDeviceTracker` present, it only guards focus while the gamepad is the active device, so mouse users can deselect freely.

Ties on priority go to the most recently activated group.

### MenuNavigationModule

The single seam between the menu layer and this package. Attach it to a menu and assign a group: the group activates when the menu opens and deactivates when it closes. Navigation stays menu-agnostic everywhere else.

## Scrolling

- **GamepadScrollRect**: drives a `ScrollRect` with a Vector2 action (typically the right stick). Uses unscaled time, so it works while menus pause the game. Configurable speed, dead zone and vertical inversion.
- **ScrollIntoView**: keeps the selected child of a `ScrollRect` visible inside the viewport, with configurable edge padding. uGUI does not do this on its own, so long lists need it.

## Input Prompts

- **InputDeviceTracker**: service that tracks the active device family (`MouseKeyboard` or `Gamepad`) based on real actuation, ignoring noise like resting sticks. Raises `OnDeviceChanged`.
- **InputGlyphSet**: ScriptableObject mapping input actions to glyphs for one device family. Create via the asset menu, one set per device.
- **InputGlyphProvider**: service that resolves the right glyph for the active device. `TryGetSprite` for images, `GetTmpSpriteTag` for inline TextMeshPro tags. Raises `OnActiveDeviceChanged` so labels can refresh.

## Quick Start

1. Add `FocusWatchdog`, `InputDeviceTracker` and `InputGlyphProvider` to your service scene.
2. Put a `NavigableGroup` on each menu root and assign its Default Element.
3. Add `NavigableElement` to your selectables or just hit Rebuild and let the validator add them.
4. Bridge menus with `MenuNavigationModule`.
5. For long lists, add `ScrollIntoView` (and optionally `GamepadScrollRect`) to the ScrollRect.
6. Author one `InputGlyphSet` per device family and assign both to the `InputGlyphProvider`.

## Editor Tooling

Rebuilds only ever run when you trigger them, so wiring never changes silently:

- **Navigation Groups window** (`Tools > Base Packages > Controller Support > Navigation Groups`): lists every group in the loaded scenes with its scene, priority and element count. Jump to any group, rebuild it individually, rebuild every group in the loaded scenes rebuild the entire project. The project rebuild walks all scenes and prefabs, rewires their groups and saves the results.
- **Inspector**: Rebuild and Rebuild Scene buttons on every `NavigableGroup`

## Dependencies

- Unity Input System
- Base Core Package (`ServiceLocator`, `GameServiceBehaviour`, menu managing, `EPriority`)
- Base Utility Package (`CustomLogger`)
- Base Tool Package (`DynamicCreateAssetMenu`)
- TextMeshPro (for inline glyph tags)