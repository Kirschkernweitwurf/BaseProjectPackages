# Base Localization Package

Editor tooling for syncing Unity String Table Collections with Google Sheets.

It wraps Unity's built-in Google Sheets integration and adds one-click Pull and Push for a single collection or all of them at once, from menu items or a dedicated window. Every sync validates its settings up front, so a misconfigured collection cannot leave the set half synced.

## Requirements

- Unity `6000.3` or newer
- `com.unity.localization` `1.5.11`
- A String Table Collection with a configured Google Sheets extension (Sheets Service Provider and Spreadsheet Id set)
- `Base.UtilityPackage` for logging and `[DynamicMenuItem]`
- Assemblies: `Base.LocalizationPackage.Editor` and the optional `Base.LocalizationPackage.Settings`

## What it does

- **Pull** overwrites local String Tables with the sheet data.
- **Push** overwrites the sheet with local String Table data, after a confirmation dialog.
- Works on a single collection or on all of them in one go.
- Discovers every String Table Collection that has a Google Sheets extension.
- Validates all extensions before running, so a misconfigured one is caught before anything is written.
- Groups results into succeeded and skipped, with a reason for each skip.

Push is destructive to the sheet and Pull is destructive to local data, so pick the direction with care.

## Usage

### Menu items

Under `Tools/Base Packages/Assets/Localization/`: **Pull All String Tables**, **Push All String Tables** and **Open Sync Window**.

### Sync window

Lists every collection that has a Google Sheets extension. Pull or Push all of them, or each one on its own. **Refresh** rescans after adding or changing collections.

### From code

```csharp
using Base.LocalizationPackage;

GoogleSheetsSync.SyncAll(ESyncDirection.Pull);

SyncResult result = GoogleSheetsSync.Sync(collection, ESyncDirection.Push);

if (!result.Success)
    Debug.LogWarning(result.Message);
```

`GetCollections` scans the Asset Database, so cache its result instead of calling it repeatedly.

## Runtime component

The editor tooling is the bulk of this package. It also ships one runtime component, `LanguageSetting`, which stores an index into a curated list of locales and applies it. It lives in the optional `Base.LocalizationPackage.Settings` assembly, gated behind a version define on `com.baseprojectpackages.settings`, so the sync tooling works fine without the Settings package installed.

Place it earlier in the scene than any component that reads localized strings during startup, so the locale is set before the first string is resolved.