# Prefab Overview

🪛 **Tool** · for everyone

**Tags:** #artist #designer #assets #project-health

**Menu:** `Tools > Base Packages > Unity Editor > Project Health > Prefab Overview`

**For:** Everyone working with prefabs

Shows every prefab in the project as a tree of base prefabs and their variants, and flags variants that look like a problem.

## How to use it

1. Press **Scan Project**.
2. Expand a prefab to see its variants. **Expand All** and **Collapse All** open or close the whole tree.
3. Use the filter to show everything, only variants, or only prefabs with issues.
4. Press **Open** on a row to open that prefab.

## What it flags

| Issue | Meaning |
| --- | --- |
| Redundant variant | The variant has no overrides at all. It is a copy of its base and can probably go. |
| Heavy overrides | The variant changes so much of its base that it may be better as its own prefab. |
| Deep chain | Variants of variants of variants. Hard to follow which level changes what. |
| Missing base | The prefab the variant is built on no longer exists. |

## Good to know

Counting overrides opens every variant, so a scan of a big project takes a moment.
