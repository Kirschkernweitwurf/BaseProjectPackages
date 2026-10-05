# Static Reset Checker

🔧 **Tool** · programmers only

**Menu:** `Tools > Base Packages > Code > Health > Static Reset Checker`

Finds static fields that are never reset when Play mode starts. This matters when **Reset Domain** is turned off in the Editor settings, because static values then survive between play sessions and cause bugs that only appear on the second run.

## How it works

The scanner reads the source files as text. It looks for `static` declarations and checks whether that field name shows up inside a reset method in the same file. If it does not, the field is reported.

## How to use it

1. Set the **Root Folder** to scan, usually `Assets`.
2. Press **Scan**.
3. Findings are grouped by file. Click a finding to open the script at that line.

## Options

| Option | Meaning |
| --- | --- |
| Reset Attributes | Which attributes mark a reset method. Default: `InitializeOnEnterPlayMode`, `RuntimeInitializeOnLoadMethod`. |
| Ignore Marker | Comment text that silences a field. Default: `reset-ignore`. |
| Include Events | Also check static events. |
| Include Auto Properties | Also check static auto properties. |
| Skip Editor Folders | Ignore anything inside an Editor folder. |
| Expand Helpers | Also follow methods called from a reset method. |
| Ignore Readonly | Skip `readonly` fields, which usually cannot change. |
| Log To Console | Print the findings to the Console as well. |

## Silencing a false positive

Add the ignore marker as a comment on the field line:

```csharp
private static int _cachedCount; // reset-ignore
```
