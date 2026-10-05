# Unused Audio Clips

🪛 **Tool** · for everyone

**Menu path:** `Tools > Base Packages > Assets > Audio > Unused Audio Clips`

## What it does

Finds audio clips in the project that nothing uses, so you can clean them up.

A clip counts as **used** if it is referenced by:

- any scene in the Build Settings
- any prefab in the project
- any Audio Container asset

Everything else is reported as unused.

## How to use it

1. Open the window from the menu. It scans right away.
2. Wait for the scan to finish. It opens every build scene one by one, so this takes a moment on large projects.
3. Look through the list of found clips.
4. Use **Select All in Project** to highlight them in the Project window, or **Delete All** to remove them.
5. Press **Rescan** any time to run the scan again.

## Buttons

| Button | What it does |
| --- | --- |
| Scan / Rescan | Runs the scan again from scratch |
| Select All in Project | Selects every found clip in the Project window |
| Delete All | Deletes every found clip after a confirmation prompt |

## Folders it looks at

| Folder | Purpose |
| --- | --- |
| `Assets/Audio` | Where clips are searched for |
| `Assets/ScriptableObjects/AudioContainer` | Where Audio Containers are searched for |

If either folder is missing, the window logs a warning and skips that part of the scan.

## Good to know

- You are asked to save your open scenes before the scan starts, because the tool opens scenes during the search. Your original scene setup is restored afterwards.
- Clips referenced only from code, from Addressables, or from Resources loading are **not** detected. Check anything suspicious before deleting.
- **Delete All cannot be undone.** Commit your work first.
