# Asset Naming Conventions

**Menu:** `Tools > Base Packages > Assets > Asset Naming Conventions`

Checks the file names of your assets against your own rules and renames the ones that break them, right in the window. The rules live in an `AssetNamingRuleSet` asset, so every project can have its own conventions. Renaming an asset keeps its GUID, so all references survive.

## How to use it

1. Press **Create Rule Set** if there is none yet.
2. Press **Auto-Detect** to read the conventions your assets already follow.
3. Open the **Rules** section and correct what the detection got wrong. Everything stays editable.
4. Press **Scan**.
5. Fix names in **Scan Results**: edit a suggestion, then press **Rename** or Enter, or press **Rename All** for the whole list.
6. Assets that should never be renamed get **Dismiss**. They move to **Dismissed** and can be restored any time.

## The window

| Section | Description |
| --- | --- |
| Rules | Scan options, the ignore list and the rule table. |
| Dismissed | Assets you took out of the scan. Each row has **Go To** and **Restore**. |
| Scan Results | Everything that breaks a rule, grouped by folder or by rule. |
| History | Every rename, dismiss and restore the tool applied, newest first. Clearable. |

All four sections sit in one scroll view and can be collapsed. Each has its own color.

The toolbar holds **Scan**, **Auto-Detect**, **Rename All**, the grouping popup, a rule filter and a search field. The search filters the results and the dismissed list at the same time.

## What it checks

| Check | Description |
| --- | --- |
| Missing prefix | The name does not start with any of the allowed prefixes, for example `P_`. |
| Missing suffix | The name does not end with any of the allowed suffixes, for example `_Data`. |
| Casing | The name does not match the casing you picked, for example `kitchen_lamp` when you want PascalCase. |
| Space or dash | Asset names never contain a space or a dash. This is always checked. |
| Number length | The number at the end is not as long as the rule asks for, for example `_1` when you want `_01`. |
| Missing underscore | The number at the end is glued to the name, for example `Lamp01` instead of `Lamp_01`. |
| Pattern | The name does not match the regular expression of the rule. |

## The rule table

| Column | Description |
| --- | --- |
| On  | Turns the rule off without deleting it. |
| Rule | Name of the rule. Only shown in this window. |
| Asset Type | What kind of asset the rule checks. `Prefab` and `Model` come from the file ending, everything else from the asset type. |
| Type Name | The value behind the popup. Pick **Custom** to type a full type name like `UnityEngine.Texture2D`. Empty means every asset. |
| Path Contains | Only check assets whose path contains this text, for example `/Art/`. Empty checks everywhere. |
| Casing | How the name must be cased. |
| Prefixes | The name must start with one of these, comma separated. The first one is used for fixes. |
| Suffixes | The name must end with one of these, comma separated. The first one is used for fixes. |
| Pattern | Optional regular expression, for shapes the other columns cannot express, like a word count or a length limit. When set, nothing else is checked. Most rules leave it empty. |
| Digits | Length of the number at the end. 0 allows any length. |

Rules are checked in order and the first one that applies to an asset wins. Column widths can be dragged and are remembered per machine.

### Casing styles

| Style | Example |
| --- | --- |
| PascalCase | `KitchenLamp` |
| camelCase | `kitchenLamp` |
| UPPER_SNAKE_CASE | `KITCHEN_LAMP` |
| lower_snake_case | `kitchen_lamp` |
| Pascal_Snake_Case | `Kitchen_Lamp` |
| Any | No casing check |

Use **Pascal_Snake_Case** when a name carries a category, for example `SM_Kitchen_Lamp` where `Kitchen` is the prop pack and `Lamp` is the asset. PascalCase would collapse it into `SM_KitchenLamp`.

## Numbers at the end

A trailing number is split off before the name is checked, with or without its underscore. So `P_Street_Lamp01` is checked as `P_Street_Lamp` plus the number `01`, and the suggestion becomes `P_Street_Lamp_01`. **Digits** decides how long the number has to be: 2 turns `_1` into `_01`, 3 turns it into `_001`, and 0 accepts any length.

## Scan options

| Option | Description |
| --- | --- |
| Scan packages | Also check assets in the `Packages` folder. |
| Scan scripts | Also check `.cs` files. Off by default, because renaming a script breaks its class name. |
| Ignored Path Fragments | Assets whose path contains one of these are skipped by every rule, for example `/TextMesh Pro/`. |

## Auto-Detect

Groups every asset by its kind and writes one rule per group. A prefix, suffix, casing or number length only becomes part of a rule when a clear majority of the group agrees, and a group needs at least five assets to be considered at all. If prefabs use both `P_` and `SM_`, both end up allowed instead of one being reported as wrong.

The result overwrites the current rules, so it asks first. Treat it as a starting point and correct it in the table.

## Suggestions

A prefix or suffix the name already carries is kept when the rule allows it. With the prefixes `P_`, `S_` and `SM_`, the name `SM_Kitchen01` is fixed to `SM_Kitchen_01` and does not switch to `P_`. Only a name without any allowed prefix gets the first one from the list.

## Notes

Renaming uses `AssetDatabase.RenameAsset`, which keeps the GUID, so scenes, prefabs and code references keep working. Scripts are the exception, which is why they are excluded by default.

The rule set is a normal project asset, so it can be checked into version control and shared with the team. Dismissed assets are stored by GUID in `ProjectSettings/AssetNamingDismissed.json` and survive renames. The history is stored in `ProjectSettings/AssetNamingHistory.json` and keeps the last 200 entries. Both files can be committed as well.
