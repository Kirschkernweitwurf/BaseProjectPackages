# Attributes Package

Every attribute in `Base.AttributePackage`, grouped the same way the Attributes window groups them. There are **100**. Each one has a live page in that window with a working sample, the source behind it, what it needs and the other ways it can be written, so this table is an index rather than a manual.

Attributes target serialized fields unless the table says otherwise.

> [!NOTE]
> Open `Tools > Base Packages > Unity Editor > Project Health > Attributes` and search for any name below.

## Overview

| Group | Attributes |
| --- | --- |
| **Layout** (15) | `[Foldout]`, `[GUIColor]`, `[HideMonoScript]`, `[Horizontal]`, `[HorizontalLine]`, `[Indent]`, `[InfoBox]`, `[InlineProperty]`, `[Label]`, `[Prefix]`, `[PropertyOrder]`, `[StartExpanded]`, `[Suffix]`, `[Tab]`, `[Title]` |
| **Conditions** (10) | `[DisableIf]`, `[DisableInPlayMode]`, `[EnableIf]`, `[EnableInPlayMode]`, `[HideIf]`, `[HideInPlayMode]`, `[ReadOnly]`, `[ShowIf]`, `[ShowIfEnum]`, `[ShowInPlayMode]` |
| **Validation** (14) | `[ArraySize]`, `[AssetOnly]`, `[Max]`, `[MaxLength]`, `[MinMax]`, `[MustImplement]`, `[NotNullOrEmpty]`, `[NotZero]`, `[PowerOfTwo]`, `[Required]`, `[RequiredIf]`, `[SceneObjectOnly]`, `[Unique]`, `[ValidateInput]` |
| **References** (7) | `[Child]`, `[GetComponent]`, `[GetComponentInParent]`, `[GetInScene]`, `[GetPrefabWithComponent]`, `[GetScriptableObject]`, `[RequiredGet]` |
| **Pickers** (17) | `[AnimatorParam]`, `[AnimatorState]`, `[AssetDropdown]`, `[AudioMixerGroup]`, `[AudioMixerParameter]`, `[ComponentPicker]`, `[Dropdown]`, `[FilePath]`, `[FolderPath]`, `[Layer]`, `[ResourcesAsset]`, `[SceneName]`, `[SceneViewPicker]`, `[ShaderKeyword]`, `[ShaderParam]`, `[SortingLayer]`, `[Tag]` |
| **Widgets** (28) | `[Button]`, `[ClearButton]`, `[ColorPalette]`, `[CopyButton]`, `[CurveRange]`, `[Date]`, `[DrawLabel]`, `[DrawLine]`, `[DrawWireDisc]`, `[EnumToggleButtons]`, `[Expandable]`, `[HeaderButton]`, `[HeaderDraw]`, `[HeaderLabel]`, `[InlineButton]`, `[MinMaxSlider]`, `[OpenAsset]`, `[Percentage]`, `[PositionHandle]`, `[PreviewObject]`, `[ProgressBar]`, `[RadiusHandle]`, `[Rate]`, `[RotationHandle]`, `[ScaleHandle]`, `[ShowAssetPreview]`, `[Slider]`, `[Time]` |
| **Collections** (3) | `[ListDrawerSettings]`, `[Table]`, `[TableColumn]` |
| **Callbacks** (5) | `[OnArraySizeChanged]`, `[OnCollectionChanged]`, `[OnValueChanged]`, `[ShowNativeProperty]`, `[ShowNonSerialized]` |
| **Serialization** (1) | `[ReferencePicker]` |

## Layout

Headings, boxes, spacing and grouping. Nothing here changes a value; all of it changes how a component reads.

| Attribute | What it does |
| --- | --- |
| `[Foldout("Bounds")]` | Puts consecutive fields that share a name into one collapsible group. The run ends where the name changes, so there is no closing marker to forget. |
| `[GUIColor(EColor.Lime)]` | Tints the whole field row, for the one value on a component that has to stand out from the rest. |
| `[HideMonoScript]` | Hides the read-only script row at the top of the inspector, for a component whose type is obvious and whose first real field is what the reader wants to see. |
| `[Horizontal("size")]` | Puts consecutive fields that share a name on one row, for values that are read together and mean little apart. |
| `[HorizontalLine(EColor.Red)]` | Draws a line above the field. Use it to split two groups of fields when neither of them is worth a heading of its own. |
| `[Indent(2)]` | Pushes the field one step to the right, which reads as belonging to the field above it. Cheaper than a foldout when there are only one or two fields to subordinate. |
| `[InfoBox("text")]` | Puts a box of text beside the field, for saying something the field name cannot. Use it for the note you would otherwise leave in a comment nobody editing the component ever sees. |
| `[InlineProperty]` | Draws a nested serializable type on the field own row instead of behind a foldout, for the small pairs where the foldout costs more room than it saves. |
| `[Label("Shown name")]` | Replaces the label of the field without renaming the field, for the case where the good code name and the good inspector name are not the same one. |
| `[Prefix("Speed")]` | Puts a small label in front of the value, for a qualifier that reads better before the number than after it: an axis, a currency, a comparison. |
| `[PropertyOrder(-1)]` | Moves a field in the inspector without moving it in the file, so a field can be pulled to the top for the reader without changing the order the data is serialized in. |
| `[StartExpanded]` | Opens the field the first time it is seen. Only the first draw is forced, so folding it up afterwards sticks. |
| `[Suffix(SuffixAttribute.Second)]` | Puts a small label after the value, almost always a unit. The constants cover the common units, so the same unit is spelled the same way on every field in the project that uses it. |
| `[Tab("General", "Settings")]` | Puts fields under named tabs inside one group, so a component with several modes shows one of them at a time instead of all of them at once. |
| `[Title("Stats", EColor.Cyan)]` | Puts a heading above the field, so a long component reads as a few named parts instead of one flat list. Optionally collapsible, which folds every field under it until the next heading. |

## Conditions

Showing, hiding and greying a field based on another one, so a component only offers what currently applies.

| Attribute | What it does |
| --- | --- |
| `[DisableIf(nameof(flag))]` | Greys the field out while the members it names are true. The inverse of enable-if. |
| `[DisableInPlayMode]` | Locks the field while the editor is playing, for setup that is read once at startup and would either be ignored or break something if it changed mid-run. |
| `[EnableIf(nameof(flag))]` | Leaves the field visible but only editable while the members it names are true. Greying out rather than hiding keeps the field where the reader last saw it, which is usually the friendlier of the two. |
| `[EnableInPlayMode]` | Leaves the field greyed out until the editor is playing, for values worth tuning live but meaningless to set beforehand. |
| `[HideIf(nameof(flag))]` | Hides the field while the members it names are true. The same check as show-if, negated, for when the readable way to say it is the negative one. |
| `[HideInPlayMode]` | Hides the field while the editor is playing, for setup that is read once at startup and cannot usefully be changed afterwards. |
| `[ReadOnly]` | Shows the field and never lets it be edited, for a value the component works out for itself and only reports. |
| `[ShowIf(nameof(flag))]` | Shows the field only while the members it names are true, so a mode nobody is using takes up no room at all. |
| `[ShowIfEnum(nameof(mode), EMode.A)]` | Shows the field only while an enum member equals one of the given values, which is how most conditions read once a component has more than two modes. |
| `[ShowInPlayMode]` | Shows the field only while the editor is playing, for runtime state that means nothing while stopped. |

## Validation

Fields that say so when they are wrong, while the object is being set up rather than at runtime.

| Attribute | What it does |
| --- | --- |
| `[ArraySize(n)]` | Locks the element count of a collection, so the add and remove buttons disappear and the size cannot drift, for a list that mirrors something fixed like the sides of a die. |
| `[AssetOnly]` | Refuses anything that lives in a scene, so a reference stored on an asset cannot be pointed at an object that will not exist the next time the asset is loaded. |
| `[Max(max)]` | Clamps the value to a maximum, for a value with a natural ceiling and no meaningful floor. |
| `[MaxLength(n)]` | Trims the string to a maximum character count after editing, for an identifier that has to fit somewhere with a fixed width. |
| `[MinMax(min, max)]` | Clamps the value into a range without drawing a slider, for a number that is typed rather than dragged. |
| `[MustImplement(typeof(IThing))]` | Accepts an object only when it, or a component on it, implements the named interfaces. Unity cannot type a field as an interface and keep it serializable, and this is the closest thing to it that still serializes. |
| `[NotNullOrEmpty]` | Requires a string with something in it, or a collection with at least one element. Empty is a different failure from null and this catches both. |
| `[NotZero(step)]` | Steps the value away from zero when it is set to zero, for a divisor or a scale where zero is not a value but a bug. |
| `[PowerOfTwo]` | Snaps the value to the nearest power of two, for the sizes the hardware wants that way. |
| `[Required]` | Marks a reference as one that has to be filled in, and shows an error under the field while it is empty. A missing reference then shows up while the object is being set up rather than as a null at runtime. |
| `[RequiredIf(nameof(flag))]` | Requires the field only while the members it names are true, for a reference that matters in one mode and is meaningless in the other. |
| `[SceneObjectOnly]` | Refuses a project asset, so a reference that has to point at something in the open scene cannot be filled with a prefab that only looks right. **Components only.** |
| `[Unique]` | Requires every entry of a collection to differ, and names the first pair that does not. Empty entries are ignored, so a half-filled list is not reported as broken while it is being written. |
| `[ValidateInput(nameof(Method))]` | Runs a method of your own and reports what it returns, for the checks no general attribute could know about. |

## References

References that fill themselves from the project or the hierarchy rather than waiting to be dragged in.

| Attribute | What it does |
| --- | --- |
| `[Child("Muzzle")]` | Fills itself with a component of the field type from the children while the field is empty. For the muzzle, the visual root or whatever else a component owns one level down. **Components only.** |
| `[GetComponent]` | Fills itself with a component of the field type from the same GameObject while the field is empty. The reference that would otherwise be dragged onto itself every time a prefab is made. **Components only.** |
| `[GetComponentInParent]` | Fills itself with a component of the field type from an ancestor while the field is empty, skipping the GameObject it is on. For the reference a child holds to whatever owns it. **Components only.** |
| `[GetInScene]` | Fills itself with the first component of the field type found anywhere in the open scene. The widest of the getters, and the one to reach for last: first found is arbitrary the moment a second one exists. **Components only.** |
| `[GetPrefabWithComponent(typeof(T))]` | Fills itself with the first prefab in the project that carries the given component, and assigns the prefab root rather than the component. The weakest of the auto-getters: first found is arbitrary the moment a second matching prefab exists. |
| `[GetScriptableObject]` | Fills itself with the first asset of the field type found in the project, for the single settings object a component always points at. |
| `[RequiredGet]` | Fills itself the way the getters do, and reports an error when nothing was found. The two halves belong together often enough that doing both with one attribute is worth it. **Components only.** |

## Pickers

Fields that offer a list of valid answers instead of free text, so a name cannot be spelled wrong.

| Attribute | What it does |
| --- | --- |
| `[AnimatorParam(nameof(animator))]` | Lists the parameters on an assigned Animator, so a trigger name cannot be misspelled and quietly do nothing. |
| `[AnimatorState(nameof(animator))]` | Lists the states on the controller of an assigned Animator, prefixed by their layer. |
| `[AssetDropdown("t:Prefab")]` | Lists every asset of the field type as a dropdown instead of opening the object picker. Worth it only while the set is small and clearly named, since a flat list of four hundred materials is worse than the picker it replaces. |
| `[AudioMixerGroup(nameof(mixer))]` | Restricts a mixer group reference to the groups of one mixer, so a group from an unrelated mixer cannot be assigned. |
| `[AudioMixerParameter(nameof(mixer))]` | Lists the parameters exposed on an assigned AudioMixer, so a volume parameter cannot be named wrong. |
| `[ComponentPicker]` | Accepts a dropped GameObject and stores one of its components, with a badge that opens a list of the siblings when more than one matches. Dropping a GameObject on a plain component field picks the first match silently, which is the wrong one often enough to be worth this. |
| `[Dropdown(nameof(Member))]` | Offers the values a member returns, so the list of valid answers lives in code next to whatever reads it rather than in a comment. |
| `[FilePath("json")]` | Turns a string into a file path with a browse button, optionally narrowed to one extension so the picker cannot return something the field could not read. |
| `[FolderPath]` | Turns a string into a folder path with a browse button. Project relative by default, so the value survives moving the project. |
| `[Layer]` | Shows a dropdown of the project layers and stores the layer index, so the number in the field always matches a layer that exists. |
| `[ResourcesAsset(typeof(T))]` | Picks an asset that lives under a Resources folder. The field stores the load path because that is the only handle Resources.Load takes, but what is being chosen is the asset, not a path somebody typed. |
| `[SceneName]` | Shows a dropdown of the scenes in the build settings. Stored as a string, so a scene left out of the build is spotted here rather than at load time. |
| `[SceneViewPicker]` | Adds a crosshair button beside an object reference. Pressing it arms the Scene view, and the next click there assigns whatever was hit, which beats hunting for the right object in a deep hierarchy. **Components only.** |
| `[ShaderKeyword(nameof(material))]` | Lists the keywords declared by the shader of an assigned material. |
| `[ShaderParam(nameof(material))]` | Lists the shader properties of an assigned material, optionally narrowed to one type, so a tint field cannot end up pointing at a texture property. |
| `[SortingLayer]` | Shows a dropdown of the sorting layers, stored by name, for anything that has to be drawn in front of or behind something else. |
| `[Tag]` | Shows a dropdown of the tags the project has. The value stays a string, but it is picked from a list instead of typed, so it cannot be spelled wrong. |

## Widgets

Controls that replace or extend the plain field, plus the scene view handles.

| Attribute | What it does |
| --- | --- |
| `[Button("Label")]` | Turns a method into a button drawn under the fields, which saves writing a small editor script for every one-off action. |
| `[ClearButton]` | Puts a small button on the field row that resets it to none or empty, for a reference you clear more often than you reassign. |
| `[ColorPalette(nameof(Member))]` | Restricts a color to a set of swatches provided by a member, so a tint cannot quietly drift off the palette the rest of the project uses. |
| `[CopyButton]` | Puts a small button on the field row that copies the value to the clipboard, for an identifier you paste somewhere else. |
| `[CurveRange(minX, minY, maxX, maxY)]` | Locks an animation curve to a range and optionally tints it, so a curve that something reads as normalized cannot wander outside the box it is read in. |
| `[Date]` | Draws a point in time as year, month and day with a calendar picker, optionally with a time of day. For dates you author, like an event start. |
| `[DrawLabel("text")]` | Draws a text label in the Scene view at the position the field holds, for naming what a point in space is while looking at the level. **Components only.** |
| `[DrawLine(EColor.Lime)]` | Draws a line in the Scene view from the object to the position the field holds, so a target or an offset is visible rather than inferred from three numbers. **Components only.** |
| `[DrawWireDisc(EColor.Orange)]` | Draws a ring in the Scene view for a radius, showing it without offering to change it. The read-only half of the radius handle, for a value something else owns. **Components only.** |
| `[EnumToggleButtons]` | Draws an enum as a row of toolbar buttons instead of a dropdown, which saves a click and shows every option at once. A flags enum becomes a row of multi-select toggles. |
| `[Expandable]` | Opens the referenced asset inline under the field, so a settings object can be edited without leaving the component that points at it. |
| `[HeaderButton("Docs")]` | Puts a button in the component title bar, for the action that belongs to the component as a whole rather than to any one field. **Components only.** |
| `[HeaderDraw(Width = 70f)]` | Hands a rectangle in the component title bar to a method of your own, for the case where neither a button nor a label is what the header should hold. **Components only.** |
| `[HeaderLabel]` | Puts a short read-only value in the component title bar, for the one number that says at a glance whether the component is set up. **Components only.** |
| `[InlineButton(nameof(Method))]` | Puts a button on the field own row that calls a method, for the small action that belongs to that one value. |
| `[MinMaxSlider(min, max)]` | Turns a Vector2 into a single slider with two handles, for a range whose two ends belong together. X is the low end and Y the high one. |
| `[OpenAsset("Edit")]` | Adds a button that opens the referenced asset in whatever editor owns it, for a reference you follow more often than you change. |
| `[Percentage(true)]` | Shows a value between zero and one as a percentage, so the inspector says 75 while the field still stores 0.75 for the code that reads it. |
| `[PositionHandle]` | Draws a move handle in the Scene view for a Vector3 field, so a position stored on a component is dragged rather than typed. **Components only.** |
| `[PreviewObject(96f)]` | Draws the assigned object as a large preview under the field, for a reference picked by eye rather than by name. |
| `[ProgressBar(max)]` | Draws the value as a filled bar instead of a number, for anything read at a glance rather than typed exactly. |
| `[RadiusHandle(EColor.Cyan)]` | Draws a ring in the Scene view whose radius is the field, for a range or a trigger size that is easier to judge against the level than as a number. **Components only.** |
| `[Rate(1, 5)]` | Shows a small integer as a row of stars, for a value picked by feel rather than measured. |
| `[RotationHandle]` | Draws a rotation handle in the Scene view for a Quaternion or Vector3 field. **Components only.** |
| `[ScaleHandle]` | Draws a scale handle in the Scene view for a Vector3 field. **Components only.** |
| `[ShowAssetPreview(64)]` | Draws a small thumbnail of the assigned asset under the field. The lighter version of the preview attribute, for when a reminder of what is assigned is enough. |
| `[Slider(0f, nameof(Max))]` | A slider whose bounds can be read from other members. Unity's own Range takes only constants, so a limit that depends on the setup has to be duplicated as a magic number or given up on. This takes a member name for either end, and can clamp the stored value to it. |
| `[Time]` | Draws a duration as day, hour, minute, second and millisecond fields. For lengths of time you type, like a cooldown or a round timer. |

## Collections

Lists and arrays drawn as something better than a stack of foldouts.

| Attribute | What it does |
| --- | --- |
| `[ListDrawerSettings(Searchable = true)]` | Configures how a list is drawn: whether it can be searched, whether removing a row asks first, and whether the rows are tinted. The list itself stays Unity's own, so it reorders, selects and resizes exactly like a list without the attribute. |
| `[Table]` | Draws a list as a grid with one row per element and one column per field, for elements small enough that a stack of foldouts costs more room than it saves. |
| `[TableColumn(2f)]` | Sets how wide one column of a table is relative to the others, and can give it a header that differs from the field name. |

## Callbacks

Methods that run when a value changes, and members shown in the inspector that Unity never serializes.

| Attribute | What it does |
| --- | --- |
| `[OnArraySizeChanged(nameof(Method))]` | Calls a method when the element count changes, but not when an element is edited, for the setup that depends on how many there are rather than what they hold. |
| `[OnCollectionChanged(nameof(Before), nameof(After))]` | Calls one method before the size changes and one after, so whatever is leaving can be released while it is still there. |
| `[OnValueChanged(nameof(Method))]` | Calls a method whenever the field is edited in the inspector, for the recalculation that has to follow a value rather than wait for play mode. |
| `[ShowNativeProperty]` | Shows a property, which has no serialized value at all, as a read-only row, for the summary a component can work out about itself. |
| `[ShowNonSerialized]` | Shows a member Unity would never serialize, for runtime state that would otherwise be invisible without attaching a debugger. Read-only by nature: there is no serialized value behind it to write to. |

## Serialization

The types Unity cannot store on its own.

| Attribute | What it does |
| --- | --- |
| `[ReferencePicker]` | Offers every implementation of the field type and stores the one that is picked by reference, which is how an interface field is filled in from the inspector at all. |

## Reading these docs

Every page says near the top who it is for. The emoji in front of a link says the same:

| Area | Everyone | Programmers only |
| --- | --- | --- |
| Components 🗳️ | 🔖 | 🗞️ |
| Scriptable Objects 📬 | 💌 | ✉️ |
| Tools 🧰 | 🪛 | 🔧 |

Menu paths are defaults. They can be moved with the Menu Item Manager in the Tools package, so your project may differ.

Something wrong or missing? [Open an issue](https://github.com/Kirschkernweitwurf/BaseProjectPackages/issues). Half a sentence is enough.
