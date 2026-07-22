# Base Localization Package

Editor tooling for syncing Unity String Table Collections with Google Sheets.

It wraps Unity's built-in Google Sheets integration and adds one-click Pull and
Push for a single collection or all of them at once, reachable from menu items or
a dedicated sync window. Every sync validates its settings up front, reports
progress and logs a clear per-collection result.

## Features

- **Pull** overwrites local String Tables with the sheet data.
- **Push** overwrites the sheet with local String Table data.
- Works on a single collection or on all collections in one go.
- Discovers every String Table Collection that has a Google Sheets extension.
- Validates all extensions before running so a misconfigured one cannot leave a
  collection half synced.
- Progress bar while syncing plus a confirmation dialog before any Push.
- Groups results into succeeded and skipped, with a reason for each skip.

## Requirements

- Unity 6000.3 or newer.
- `com.unity.localization` 1.5.11.
- A String Table Collection with a configured Google Sheets extension
  (Sheets Service Provider and Spreadsheet Id set).

This package is Editor only.

## Installation

Add it to your project through the Package Manager or by referencing it in
`manifest.json`:

```json
"com.baseprojectpackages.localization": "1.0.7"
```

## Usage

### Menu items

Under `Tools/Base Packages/Assets/Localization/`:

- **Pull All String Tables** pulls every collection from its sheet.
- **Push All String Tables** pushes every collection to its sheet.
- **Open Sync Window** opens the sync window described below.

### Sync window

The window lists every collection that has a Google Sheets extension. From there
you can Pull or Push all collections. You can also Pull and Push each collection
on its own. Use **Refresh** to rescan after adding or changing collections.

### From code

```csharp
using Base.LocalizationPackage;

// Sync every collection.
GoogleSheetsSync.SyncAll(ESyncDirection.Pull);

// Sync one collection and check the result.
SyncResult result = GoogleSheetsSync.Sync(collection, ESyncDirection.Push);
if (!result.Success)
    Debug.LogWarning(result.Message);
```

`GetCollections` scans the Asset Database, so cache its result instead of calling
it repeatedly.

## API

| Type | Purpose |
| --- | --- |
| `GoogleSheetsSync` | Static entry point for `GetCollections`, `Sync` and `SyncAll`. |
| `ESyncDirection` | `Pull` or `Push`. |
| `SyncResult` | Read-only result of a sync, exposing `Success` and `Message`. |
| `LocalizationSyncWindow` | The Editor window, openable via `Open`. |
| `LocalizationMenu` | Registers the menu items. |

## Notes

Push is destructive to the sheet and Pull is destructive to local data, so pick
the direction with care. A Push always asks for confirmation first.
