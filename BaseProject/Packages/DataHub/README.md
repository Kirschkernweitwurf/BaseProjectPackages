# Base Data Hub

One editor window for every data asset of a project. Editor only.

**Team docs:** how to use the window is in [`Documentation~/Data Hub Package.md`](Documentation~/Data%20Hub%20Package.md). In Unity, open `Help > Base Packages Documentation`.

The package knows nothing about any game. A project declares what shows up through small interfaces, each found automatically through `TypeCache`, so adding a table, column, check, create rule or page is one new class and no change to this package.

## Requirements

- Unity `6000.3` or newer
- `Base.EditorUIPackage.Editor` for the look of the window
- `Base.UtilityPackage` for `CustomLogger` and `[DynamicMenuItem]`
- `Base.AttributesPackage` for the validation attributes the check column runs
- `Base.ToolsPackage.Editor.NamingConventions` for the names of new and renamed assets
- One assembly: `Base.DataHubPackage.Editor`, Editor platform only

## The window

`Tools > Base Packages > Assets > Data Hub`

- **Type tree** on the left: every registered table and page, nested by path and foldable, with the asset count of each table.
- **Table** in the middle: one row per asset, one column per serialized field. Nested serializable structs are flattened into one column per field, with the struct name above them. Auto-properties with `[field: SerializeField]` get clean titles. Sort by clicking a header, resize by dragging its edge, search names and values, filter to problems only. The name and check columns stay in place while the rest scrolls sideways.
- **Inline editing** through `SerializedProperty`, so Undo, dirty marking and saving behave like the inspector. Editing a cell of a selected row writes the same value to every selected row.
- **Side panel** on the right, with three tabs:
  - **Inspector:** the normal inspector of the selected rows, for fields a cell cannot edit, like `[SerializeReference]` lists, and the `[Button]` methods of Base Attributes.
  - **Contents:** every asset list of the selected asset as one line per entry with its count, like a deck holding a card three times. Change counts with minus, plus or by typing, remove an entry, pick one to add N copies, or drop rows from the table on the list.
  - **Used In:** every list in the project that can hold the selected rows, with how often they are in it. Plus, minus or a typed count changes it for every selected row at once.
- **Actions** above the table and on right click: the parameterless `[Button]` methods the selected assets share, run on each of them in one undo step.
- **Drag** a row by its name to drop the selected assets on a list, any object field or the Project window.
- **Check column**: a quiet check mark for a fine row, a red count for a broken one and an amber count for warnings, with the messages in its tooltip.
- **Tooltips** are one short line each. A column title shows the field's own `[Tooltip]` when it has one, otherwise what kind of value it holds, plus a hint only where clicking the cell is not enough.
- **New, Duplicate, Rename, Delete** above the table and on right click, also as Ctrl+D, Delete and F2. Ctrl+A selects every row, Ctrl+F jumps to the search.

## Extension points

All in `Base.DataHubPackage.Editor.Extensions`. Each implementation needs a parameterless constructor.

| Interface | Purpose |
|---|---|
| `IDataTable` | A data type listed as a table, at a path in the tree. An abstract type lists every derived type with the shared columns. |
| `IDataColumn` | A computed, read-only column for a type and its derived types. |
| `IDataValidator` | A project check. Problems show as a red badge. |
| `IDataCreateRule` | Folder, start name and collection list for new assets of a type. The most specific rule wins. |
| `IDataHubPage` | A whole custom page in the tree, drawn with GUILayout. |
| `IDataSummary` | A live summary of an asset at the top of its Contents tab, drawn with GUILayout inside a card the hub provides. |
| `IDataUsageCheck` | Assets of a type that nothing uses yet get an amber warning. Collections do not count as use, other data assets and saved scenes do. |

```csharp
internal sealed class GiftTable : IDataTable
{
    public string Path => "Cards/Gift";
    public Type DataType => typeof(GiftSo);
    public int Order => 1;
}
```

## Asset actions

- **Show in Data Hub** in the Project window's context menu opens the table of the selected asset with its row selected.
- **Bulk edit** on right click of a number cell or column title: add, multiply or set for many rows, previewed first, rounded for whole numbers and kept within `[Min]` and `[Range]`.
- **Replace Everywhere** on right click of a row: swaps the asset for another in every list and field of the data assets, previewed per spot.
- **Sheet** menu: copy the shown rows to the clipboard or a CSV file and import edits back, matched by name and title, with a preview of every change.

- **New** creates the asset in the folder of its create rule, names it through the naming conventions, runs the same reset as the inspector, and adds it to the collection list of the rule. Without a rule it lands next to the selected asset.
- **Duplicate** copies next to the source with the next free number and adds the copy to the collection.
- **Rename** shows the final name, after the naming conventions, before it is applied.
- **Delete** removes every reference from the project's ScriptableObject assets first (list elements are removed, single fields set to none), then moves the files to the trash. Scenes and prefabs are not cleaned.

## Validation

Each row is checked against:

1. The validation attributes of Base Attributes, such as `[Required]` and `[NotNullOrEmpty]`, through `ReferenceValidation.Collect`.
2. Every `IDataValidator` whose type matches.
3. Its collection: an asset whose create rule names a collection but is not in it gets "Not in ...". Right click, **Add to Collection** fixes it.

## Limits

- Built on IMGUI and drawn row by row only where visible. Fine for hundreds of assets per type.
- Creating an asset cannot be undone with Undo, only deleted again. Edits can.
- Lists, `[SerializeReference]` fields and Unity structs like `Vector3` show as read-only text in the table. Edit asset lists in the Contents tab, the rest in the Inspector tab.
- Contents and Used In only edit top-level lists of asset references. Lists typed as plain `ScriptableObject` or `Object` are left out, since they hold engine data, not designer data.