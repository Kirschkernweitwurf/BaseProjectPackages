# Base Text

Localized text styles for TextMeshPro: one style per kind of text box and language, balanced line breaks, hyphenation, readable fonts and text size options for accessibility. The Text Fit editor window renders every localized text in every language in its real box and lists what does not fit.

**Team docs:** how to use everything in the editor is in [`Documentation~/Text Package.md`](Documentation~/Text%20Package.md). In Unity, open `Help > Base Packages Documentation`.

Code only. The package ships no fonts, text styles or hyphenation patterns. The project brings its own, see [What the project provides](#what-the-project-provides).

## Requirements

- Unity `6000.3` or newer
- `com.unity.localization` `1.5.11`
- `com.unity.ugui` `2.0.0` and TextMeshPro
- `Base.AttributesPackage` for `[Required]` and `[GetComponent]`
- `Base.UtilityPackage` for `CustomLogger`, `[DynamicMenuItem]` and `[DynamicCreateAssetMenu]`
- `Base.EditorUIPackage` for the skin of the Text Fit window, editor only
- Assemblies: `Base.TextPackage` and `Base.TextPackage.Editor`

## What the project provides

| What | Why |
|---|---|
| An Asset Table Collection named `Text Styles` | Each entry is one kind of text box with a `TextStyle` per locale. A `LocalizedTextBox` picks its entry in the Inspector. The editor tools look the collection up by this exact name |
| Reading settings that call `ReadableText` | The package has no settings of its own. See [Readable fonts and text size](#readable-fonts-and-text-size) |
| A `ReadableFontSet` asset | The readable font choices and the wider spacing. The Text Fit worst case uses the first one it finds |
| Hyphenation patterns | Only for boxes with `isHyphenated`. See [Hyphenation](#hyphenation) |
| A Pseudo-Locale with `WordExpander` | Optional. Gives the Text Fit window its Tight page |

## Text styles

| Type | What it does |
|---|---|
| `TextStyle` | ScriptableObject for one kind of box in one language: font, material, size, auto size minimum, line spacing, max lines, readable minimum, ellipsis and `ETextRole`. Created with `Scriptable Objects/Base/Text/New Text Style` |
| `LocalizedTextStyle` | `LocalizedAsset<TextStyle>`, the per-locale reference a box holds |
| `LocalizedTextBox` | Applies the style of the current locale to its `TMP_Text`, with the readable font, spacing and text size on top. Balances lines, hyphenates, raises `Fitted` after every fit and warns about overflow in the editor and development builds |
| `ETextRole` | `Panel` grows its box with the text. `Frame` keeps its box and shrinks the text down to the readable minimum |
| `TextSizeGroup` | Gives every active `LocalizedTextBox` below it with the same style the smallest size any of them needs |
| `TextLocale` | The locale styled texts are prepared for: the selected one or the one an editor check measures |
| `TextOverflow` | How far rendered letters run past their box, read from the glyph outlines. The overflow warning and Text Fit both use it |

`LocalizedTextBox` also exposes `ShownSizeGrowth`, `TextWidthGrowth`, `LargeViewScale` and `ReadableViewScale` for components that follow its text. Code that always shows a box bigger on hover or focus calls `SetLargeView(scale)`, so a frame may shrink below its readable minimum by that much.

## Line balancing

| Type | What it does |
|---|---|
| `LineBalancer` | Internal. Narrows a wrapped text through its margins to the narrowest width that keeps its line count and size, like CSS `text-wrap: balance`. The box and its layout stay as they are |
| `ELineFit` | `Balance` evens out every line. `Pretty` keeps lines full and only fixes a short last line, like CSS `text-wrap: pretty` |
| `BrokenWords` | Finds words a text broke across two lines without a hyphen. The balancer avoids such layouts and Text Fit reports them |

A `LocalizedTextBox` balances styles with `MaxLines` above 1, unless the box may cut its text.

## Hyphenation

`TextHyphenator` is a TextMeshPro `ITextPreprocessor`. A box with `isHyphenated` marks long words with soft hyphens right before TextMeshPro reads the text, so the text other code sets and reads stays untouched. Rich text tags stay whole. `LiangHyphenator` is the pure C# Knuth-Liang implementation behind it. Words under 7 letters stay whole and 3 letters stay on each side of a break.

The package ships no patterns. The project copies the plain text pattern files of [hyph-utf8](https://github.com/hyphenation/tex-hyphen) into a `Resources/Hyphenation` folder:

```
Resources/Hyphenation/hyph-en-us.pat.txt   patterns
Resources/Hyphenation/hyph-en-us.hyp.txt   exceptions, optional
```

Unity imports `.txt` files as `TextAsset`. `TextHyphenator` loads them with `Resources.Load` from `Hyphenation/hyph-<name>.pat` and `Hyphenation/hyph-<name>.hyp`. A missing exceptions file is normal. Lines that start with `%` are comments.

The `<name>` comes from a fixed map in `TextHyphenator`, keyed by the locale code:

| Locale code | Pattern name |
|---|---|
| `en` | `en-us` |
| `de` | `de-1996` |
| `es` | `es` |
| `fr` | `fr` |
| `pt-BR` | `pt` |
| `ru` | `ru` |

Every other locale passes through without hyphens, like Japanese and Chinese, which break anywhere on their own. A new language needs a new entry in that map. Each pattern file has its own license, so ship their notices with the game.

## Readable fonts and text size

`ReadableText` holds the player's reading settings. Every `LocalizedTextBox` and every zoom component listens to `ReadableText.Changed` and applies them again at once.

| Type | What it does |
|---|---|
| `ReadableText` | Static. The readable font, the extra spacing and the text size scale, from 1 to `LargestScale` (1.5). Raises `Changed` when one changes |
| `ReadableFontSet` | ScriptableObject: the font choices in menu order (the first is the designed look), the position of the widest font for the Text Fit worst case and the wider spacing. Created with `Scriptable Objects/Base/Text/New Readable Font Set` |
| `ReadableFont` | One font choice: the locales it has letters for and its swaps |
| `ReadableFontSwap` | Replaces one designed font, optionally only with one material, by a readable font scaled to the same lowercase height and line height |
| `TextSpacing` | Extra room between letters, words, lines and paragraphs, in hundredths of the font size |

The project's settings code passes the player's choices in:

```csharp
// The font setting stores the position in the font set, so 0 is the designed look.
ReadableText.SetFont(fontSet.GetFont(fontIndex));

ReadableText.SetSpacing(isWideSpacing
    ? fontSet.WideSpacing
    : TextSpacing.None);

// 1 is as designed, ReadableText.LargestScale is the largest text size.
ReadableText.SetScale(textScale);
```

Call `ReadableText.Clear()` when the settings go away at the end of a play session. It drops the choices without raising `Changed`. The statics reset on their own with domain reload off.

`ReadableText.GetScaledSize` adds the style's `MinReadableSize` times the extra scale to every size. So the smallest text reaches the full scale while titles grow less and the hierarchy stays. Wider spacing only reaches paragraphs: panel styles with `MaxLines` above 2. A readable font only replaces fonts it has a swap for and only in locales it has letters for.

## Text size components

| Type | What it does |
|---|---|
| `ITextZoom` | A part of the screen that zooms with the text size. Texts inside keep their designed size within it and may shrink as far as the zoom grows them back |
| `ScaleWithTextSize` | `ITextZoom` that scales its transform around its pivot, up to `maxZoom` |
| `ScaleCanvasWithTextSize` | `ITextZoom` that shrinks a `CanvasScaler`'s reference resolution, so a whole screen canvas zooms |
| `ScaleGridWithTextSize` | `ITextZoom` that scales a `GridLayoutGroup` and lowers its fixed column or row count, so a row keeps about its width |
| `ScaleWithText` | Scales icons beside one `LocalizedTextBox` by its `ShownSizeGrowth`, the size the text really shows |
| `WidenWithTextSize` | `ILayoutElement` that widens a fixed-width box by `TextWidthGrowth`, capped at 1.5 times and an optional max width |
| `GrowWithLabel` | `ILayoutElement` that lets the width of a button's own layout group win over its designed width |
| `GrowWithText` | `ILayoutElement` that gives a row the height its text needs, never less than designed |
| `KeepRowInPlaceOnTextChange` | Keeps the selected row of a `ScrollRect` at its spot when the reading settings change |

## Pseudo-localization

| Type | What it does |
|---|---|
| `WordExpander` | `IPseudoLocalizationMethod` that makes a text longer with filler words after a space, by length rules, like a translation does. Unity's `Expander` glues random characters to the last word instead. The same text always gets the same filler, so checks stay stable |

## Text Fit window

`Tools > Base Packages > Assets > Localization > Text Fit`, a `[DynamicMenuItem]` the Menu Item Manager can move. It loads every prefab and scene with a text into preview scenes and renders every text in every locale, pseudo-locales included, in its real box. The team docs page explains the window itself.

Built in, `UiTextSource` covers every `LocalizeStringEvent` and every field marked with `[ShownIn]` without any setup. Texts that code builds need a source of their own:

| Type | What it does |
|---|---|
| `ITextSampleSource` | One part of the project whose texts the check renders. Every non-abstract implementation with a parameterless constructor, of any access, in any editor assembly is found through `TypeCache` |
| `TextFitStage` | The preview scenes. A source puts prefabs or scenes on it (`Instantiate`, `Open`), finds components (`FindAll`) and claims its boxes (`Box`), so they no longer show as unchecked |
| `TextBox` | A claimed `TMP_Text` with its location and Text Styles key. The optional `prepare` callback runs after every new text, for a box that code sizes to its text |
| `TextSample` | One text in one locale with its box, a source label, the Smart String arguments, the context asset and the string table entry. The optional `arrange` callback puts the box's surroundings in the state the text is shown in |
| `SmartTestArguments` | Worst-case values for Smart String placeholders: every plural form plus a wide number |
| `TextFitLocales` | The locales of a check, the English fallback and the pseudo-locale helpers `ShownText` and `Stretch` |
| `TextStyleTable` | Reads the `Text Styles` collection straight from the project, in any locale |
| `ShownInAttribute` | Runtime. Marks a `LocalizedString` field or a list of them as shown in a `TMP_Text` or `LocalizeStringEvent` field of the same component |
| `SpawnedInAttribute` | Runtime. Marks a prefab field as placed under the `Transform` or list of them that another field of the same component holds, so the entry is measured with the room the list gives it |

A source has three members:

1. `Area` names its group in the report, like `"Items"`.
2. `Load(TextFitStage stage)` runs once per check. It puts the prefabs on the stage and claims their boxes with `stage.Box(text)`.
3. `Collect(Locale locale)` returns a `TextSample` per text and box in that locale. `TextFitLocales.ShownText` falls back to English and `Stretch` lengthens the text for a pseudo-locale.

For a build pipeline, `TextFitBatch.Run` runs the check from the command line and exits with 1 when a text of a real language does not fit. Pseudo-locale problems are warnings.

```
-batchmode -quit -executeMethod Base.TextPackage.Editor.TextFitBatch.Run
```

`TextFitWatcher` raises `Changed` after an asset, prefab or scene was imported, moved or deleted, for other windows that want to check again. The list of ignored boxes lives in `ProjectSettings/TextFitIgnoreList.asset`.