# Base UI Package

Reusable UI building blocks for Unity. The package bundles click-driven button
components, an awaitable confirmation dialog and a set of small utility components
so the same UI systems can be dropped into any project without rewriting them.

All types live under the `Base.UIPackage` namespace, split into `Buttons`,
`Confirmation` and `Utility`.

## Dependencies

This package builds on the other Base packages and expects them to be present:

- **Base.CorePackage** for the `ServiceLocator`, the `MenuManager` and menu
  identifiers, the `SceneLoadingManager` and `GameServiceBehaviour`.
- **Base.AttributePackage** for the inspector attributes (`[Required]`,
  `[GetComponent]`, `[NotNullOrEmpty]`, `[SceneName]`).
- **Base.UtilityPackage** for `CustomLogger` and the `Platform` helper.
- **TextMeshPro** and Unity's built-in **UI (uGUI)**.

## Buttons

Every button derives from `CustomButton`, an abstract `MonoBehaviour` that
requires a `Button` component and wires its own `OnClick` handler on `Awake`.
Subclasses only implement the behavior they need.

- **CustomButton** — abstract base that hooks and unhooks the `Button.onClick`
  listener and exposes an abstract `OnClick`.
- **OpenMenuButton** — opens a target menu through the `MenuManager`, with an
  optional parent menu that stays registered as its owner.
- **CloseMenuButton** — closes a target menu if it is currently open.
- **PauseMenuButton** — toggles the pause menu and swaps the button icon
  between a play sprite and a pause sprite, staying in sync with
  `PauseMenu.OnPauseStateChanged`.
- **LoadSceneButton** — unloads all scenes and additively loads a chosen scene
  asynchronously through the `SceneLoadingManager`.
- **OpenLinkOnClick** — opens a URL in the default browser.

## Confirmation

An asynchronous confirmation flow for actions that need the player to agree
before they run, such as quitting or leaving a scene.

- **ConfirmationRequest** — a readonly struct holding the message and the
  optional confirm and cancel labels.
- **ConfirmationMenu** — the menu that shows the message, the two buttons and
  falls back to default labels when none are given.
- **ConfirmationService** — a `GameServiceBehaviour` that exposes an awaitable
  `ShowConfirmationAsync`. Only one confirmation runs at a time and concurrent
  requests are denied.
- **BaseConfirmationButton** — abstract button that shows the confirmation box
  and calls `OnConfirm` or `OnCancel` based on the answer.
- **ConfirmedLoadSceneButton** — loads a scene once the player confirms.
- **ConfirmedQuitButton** — quits the build or stops the editor once the player
  confirms.

## Utility

Small standalone components for common UI and build needs.

- **Billboard** — rotates an object to face the main camera at runtime, with an
  option to lock rotation to the Y axis so it stays upright.
- **EditorBillboard** — faces the viewing camera in both play mode and the scene
  view, useful for authoring in the editor.
- **WorldCanvasWrapper** — assigns the main camera as the world camera of a
  world-space `Canvas` on `Awake`.
- **FpsCounter** — shows the current frames per second in a `TMP_Text`, hidden
  in release builds unless explicitly enabled.
- **BuildVersion** — displays the version and build number read from a
  `version.txt` in the StreamingAssets folder, with an option to hide it in
  release builds.
- **BuildVersionProcessor** — an `IPreprocessBuildWithReport` step that writes
  the current version and increments the build number before every build.

## Assets

The package ships a few ready-made prefabs and images to get started quickly:

- `Prefabs/Buttons/BasicImageButton`
- `Prefabs/Buttons/BasicTextButton`
- `Images/BG_60`

## Installation

Add the package to your project through the Unity Package Manager, either from a
Git URL or as a local package, and make sure the Base packages listed under
[Dependencies](#dependencies) are installed as well.