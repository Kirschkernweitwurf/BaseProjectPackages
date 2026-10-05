# Scriptable Objects

Asset files you create through `Assets > Create > Scriptable Objects > Base`.

| Asset | What it is for |
| --- | --- |
| [💌 LightingProfile](lighting-profile.md) | Stores a scene's lighting settings so you can reuse them anywhere. |
| [💌 ZooConfig](zoo-config.md) | Describes what the Asset Zoo Builder should lay out, and how. |
| Audio Rule Set (`ARS_`) | The rules for [Audio Rules](../tools/audio-rules.md). |
| Asset Naming Rule Set (`ANRS_`) | The rules for [Asset Naming Conventions](../tools/asset-naming-conventions.md). |
| Folder Convention Config (`FCC_`) | The rules for [Folder Conventions](../tools/folder-conventions.md). |
| Namespace Convention Config (`NCC_`) | The rules for [Namespace Conventions](../tools/namespace-conventions.md). |

The Unique Id Scriptable Object now lives in the [Utility package](https://github.com/Kirschkernweitwurf/BaseProjectPackages/blob/main/BaseProject/Packages/Utility/Documentation~/scriptable-objects/unique-id-scriptable-object.md).

## Where the menu paths come from

These assets use `[DynamicCreateAssetMenu]`, which means their place in the Create menu is data, not code. You can move them around in the [Create Asset Manager](../tools/create-asset-manager.md).

## Internal stores

The package also uses a few scriptable objects you never create by hand. They are listed here so you know what the files in `ProjectSettings` and `UserSettings` are.

| File | What it holds |
| --- | --- |
| `ProjectSettings/MenuManagerOverlay.asset` | Your project's menu layout. Shared with the team. |
| `MenuManagerRegistry.asset` | The menu layout shipped with the package. Read only. |
| `ProjectSettings/UnityConstantsOrderRegistry.asset` | The constants from the Order Manager. |
| `ProjectSettings/UniqueIdSettings.asset` | The on and off switch for ID validation. |
| `UserSettings/Base/ComponentClipboard.asset` | The component clipboard. Personal, not committed. |
| Play mode state store | The captured Play mode changes waiting to be applied. |
