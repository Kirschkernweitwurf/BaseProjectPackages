# Namespace Conventions

🔧 **Tool** · programmers only

**Tags:** #programmer #project-health

**Menu:** `Tools > Base Packages > Unity Editor > Project Health > Namespace Conventions`

**Create:** `Create > Scriptable Objects > Base > Namespace Convention > New Config` (default file name `NCC_NamespaceConventions`)

Checks that every script's namespace matches the folder it sits in, and lists the ones that do not. The rules live in a config asset, so every project can have its own.

## How to use it

1. Assign a config at the top, or press **Create Config**.
2. Open the rules section and adjust the settings.
3. Scan.
4. Click **Go to** on a row to open the script.

## The config

| Field | What it does |
| --- | --- |
| Root Folder | Folder the scan starts at. Namespaces are measured from here down. |
| Ignored Folders | Folder names that are skipped, including everything inside them. |
| Ignored File Names | File names that are skipped, for files without a type in them. |
| Root Namespace | Put in front of the folder path for scripts no assembly definition owns. Empty measures from the root alone. |
| Allow Shorter Namespace | Allows a namespace that stops short of its folder, the way packages are often flattened. |
| Require Namespace | Also lists files that declare no namespace at all. |
| Ignore Generated Files | Ignores files written by code generators. |

## Good to know

Where an assembly definition owns a folder, its root namespace is the starting point, not the folder name.
