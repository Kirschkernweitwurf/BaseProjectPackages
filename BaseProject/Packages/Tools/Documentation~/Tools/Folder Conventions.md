# Folder Conventions

🪛 **Tool** · for everyone

**Menu:** `Tools > Base Packages > Unity Editor > Project Health > Folder Conventions`

Checks the folder layout of the project against your own rules and lists everything that breaks them. The rules live in a `FolderConventionConfig` asset, so every project can have its own layout.

## How to use it

1. Assign a config at the top, or press **Create Config** if there is none yet.
2. Open the **Rules** foldout and set up the conventions. The list updates as you edit.
3. Press **Scan**.
4. Click a row to ping the folder in the Project window, or use **Go to**.
5. Missing folders show a **Create** button that builds the folder and all missing parents.

## What it checks

| Check | Description |
| --- | --- |
| Missing folder | A folder from the required list does not exist. Fixable with one click. |
| Naming style | A folder name does not match the style you picked, for example `art_stuff` when you want PascalCase. |
| Forbidden name | The name is on your blocklist, for example "New Folder" or "Temp". |
| Exceeded depth | The folder is nested deeper than the limit. Only the first level past the limit is reported. |
| Loose asset | A file sits directly in the root folder instead of a subfolder. |

## The config

| Field | Description |
| --- | --- |
| Root Folder | Where the scan starts. Everything below it is validated. Default is `Assets`. |
| Ignored Folders | Folder names that are skipped, including everything inside them. |
| Naming Style | PascalCase, camelCase, snake_case, kebab-case or Any. |
| Allowed Name Exceptions | Names that may break the naming style, for example a project root like `_Project`. |
| Forbidden Names | Names that are never allowed, whatever the naming style says. |
| Max Depth | How many folder levels are allowed below the root. |
| Required Folders | Folders that have to exist. Missing ones can be created from the window. |
| Allow Loose Assets In Root | Turns the loose asset check off. |

Paths may be written with or without the `Assets` prefix and with either slash direction. `Art/Textures` and `Assets\Art\Textures` both resolve to `Assets/Art/Textures`.

## Notes

Folders themselves are never reported as loose assets, only files, and only one level deep. A file inside `Assets/Art` is fine even when `Assets` itself has to stay clean.

If your project keeps a few files in the root on purpose, either tick **Allow Loose Assets In Root** or point the root at your real project folder, for example `Assets/_Project`.

The config is a normal project asset, so it can be checked into version control and shared with the team.
