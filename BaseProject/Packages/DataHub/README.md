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

- **Type tree** on the left: every registered table and page, nested by path, with the asset count of each table.
- **Table** in the middle: one row per asset, one column per serialized field. Nested serializable structs are flattened into one column per field, with the struct name above them. Auto-properties with `[field: SerializeField]` get clean titles. Sort by clicking a header, resize by dragging its edge, search names and values, filter to problems only.
- **Inline editing** through `SerializedProperty`, so Undo, dirty marking and saving behave like the inspector. Editing a cell of a selected row writes the same value to every selected row.
- **Inspector** on the right: the normal inspector of the selected rows, for fields a cell cannot edit, like `[SerializeReference]` lists.
- **Check column**: a red badge with the problem count, the problems in its tooltip.
- **New, Duplicate, Rename, Delete** in the toolbar and on right click.

## Extension points

All in `Base.DataHubPackage.Editor.Extensions`. Each implementation needs a parameterless constructor.

| Interface | Purpose |
|---|---|
| `IDataTable` | A data type listed as a table, at a path in the tree. An abstract type lists every derived type with the shared columns. |
| `IDataColumn` | A computed, read-only column for a type and its derived types. |
| `IDataValidator` | A project check. Problems show as a red badge. |
| `IDataCreateRule` | Folder, start name and collection list for new assets of a type. The most specific rule wins. |
| `IDataHubPage` | A whole custom page in the tree, drawn with GUILayout. |

```csharp
internal sealed class GiftTable : IDataTable
{
    public string Path => "Cards/Gift";
    public Type DataType => typeof(GiftSo);
    public int Order => 1;
}
```

## Asset actions

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
- Lists, `[SerializeReference]` fields and Unity structs like `Vector3` show as read-only text in the table. Edit them in the inspector panel.