using Base.EditorUIPackage.Editor;
using UnityEngine.UIElements;

namespace Base.ToolsPackage.Editor.AudioRules.Window
{
    /// <summary>
    /// The USS class names the audio rules window applies from code, and the one place that attaches
    /// its sheets and keeps its colors in step with the active theme.
    /// </summary>
    /// <remarks>
    /// The sheet routes every color through its own variables, and nothing in C# can write a USS
    /// custom property, so the themed colors arrive as inline styles through
    /// <see cref="EditorUssPainter"/> instead. What the sheet declares stays as the built-in look,
    /// which is what the window falls back to when no theme is assigned.
    /// </remarks>
    internal static class AudioRulesStyle
    {
        /// <summary>The button that adds a new rule.</summary>
        internal const string AddClass = "ar-add";
        /// <summary>A small count pill, such as how many clips a rule matched.</summary>
        internal const string BadgeClass = "ar-badge";
        /// <summary>A badge whose count is zero, drawn muted. Modifier on the badge.</summary>
        internal const string BadgeZeroClass = "ar-badge--zero";
        /// <summary>A panel grouping related controls.</summary>
        internal const string CardClass = "ar-card";
        /// <summary>A cell whose value breaks its rule. Modifier on the cell.</summary>
        internal const string CellBadClass = "ar-cell--bad";
        /// <summary>One cell of the clip table.</summary>
        internal const string CellClass = "ar-cell";
        /// <summary>A cell with nothing to say, drawn muted. Modifier on the cell.</summary>
        internal const string CellDimClass = "ar-cell--dim";
        /// <summary>A cell whose value satisfies its rule. Modifier on the cell.</summary>
        internal const string CellGoodClass = "ar-cell--good";
        /// <summary>A cell holding a number, so it can be right aligned. Modifier on the cell.</summary>
        internal const string CellNumberClass = "ar-cell--number";
        /// <summary>A cell showing what a rule wants the value to be. Modifier on the cell.</summary>
        internal const string CellTargetClass = "ar-cell--target";
        /// <summary>A chip reporting a value that breaks a rule. Modifier on the chip.</summary>
        internal const string ChipBadClass = "ar-chip--bad";
        /// <summary>A small inline pill carrying one value.</summary>
        internal const string ChipClass = "ar-chip";
        /// <summary>A chip reporting a value that satisfies a rule. Modifier on the chip.</summary>
        internal const string ChipGoodClass = "ar-chip--good";
        /// <summary>A chip reporting a value worth a look but not a failure. Modifier on the chip.</summary>
        internal const string ChipWarnClass = "ar-chip--warn";
        /// <summary>The cell holding the clip itself, which is wider than the value columns.</summary>
        internal const string ClipCellClass = "ar-clip-cell";
        /// <summary>The clip file name inside its cell.</summary>
        internal const string ClipNameClass = "ar-clip-name";
        /// <summary>The line drawn between a rule and the clips it matches.</summary>
        internal const string ConnClass = "ar-conn";
        /// <summary>The panel describing whatever is currently selected.</summary>
        internal const string DetailClass = "ar-detail";
        /// <summary>The asset path line in the detail panel.</summary>
        internal const string DetailPathClass = "ar-detail--path";
        /// <summary>The text the detail panel shows before anything is selected.</summary>
        internal const string DetailPlaceholderClass = "ar-detail--placeholder";
        /// <summary>The line explaining why a clip matched or failed.</summary>
        internal const string DetailReasonClass = "ar-detail--reason";
        /// <summary>The rule name shown in the detail panel.</summary>
        internal const string DetailRuleClass = "ar-detail--rule";
        /// <summary>The heading of the detail panel.</summary>
        internal const string DetailTitleClass = "ar-detail--title";
        /// <summary>The explanatory paragraph of an empty state.</summary>
        internal const string EmptyBodyClass = "ar-empty__body";
        /// <summary>The panel shown when there is nothing to list.</summary>
        internal const string EmptyClass = "ar-empty";
        /// <summary>The glyph at the center of an empty state.</summary>
        internal const string EmptyGlyphClass = "ar-empty__glyph";
        /// <summary>The musical note variant of the empty glyph. Modifier on the glyph.</summary>
        internal const string EmptyGlyphNoteClass = "ar-empty__glyph--note";
        /// <summary>The ring drawn around the empty state glyph.</summary>
        internal const string EmptyRingClass = "ar-empty__ring";
        /// <summary>The headline of an empty state.</summary>
        internal const string EmptyTitleClass = "ar-empty__title";
        /// <summary>The strip along the bottom of a pane.</summary>
        internal const string FooterClass = "ar-footer";
        /// <summary>A placeholder shown while a drag is in progress.</summary>
        internal const string GhostClass = "ar-ghost";
        /// <summary>Marks a value that satisfies its rule, wherever such a value is shown.</summary>
        internal const string GoodClass = "ar-good";
        /// <summary>One cell of the settings grid.</summary>
        internal const string GridCellClass = "ar-grid-cell";
        /// <summary>A grid cell carrying no override. Modifier on the grid cell.</summary>
        internal const string GridCellDimClass = "ar-grid-cell--dim";
        /// <summary>A grid cell in the header row. Modifier on the grid cell.</summary>
        internal const string GridCellHeadClass = "ar-grid-cell--head";
        /// <summary>A grid row with unsaved edits. Modifier on the grid row.</summary>
        internal const string GridRowChangedClass = "ar-grid-row--changed";
        /// <summary>One row of the settings grid.</summary>
        internal const string GridRowClass = "ar-grid-row";
        /// <summary>The header row of the settings grid. Modifier on the grid row.</summary>
        internal const string GridRowHeadClass = "ar-grid-row--head";
        /// <summary>A grid row that does not apply to the current selection. Modifier on the grid row.</summary>
        internal const string GridRowMutedClass = "ar-grid-row--muted";
        /// <summary>Marks a clip a rule matched.</summary>
        internal const string MatchClass = "ar-match";
        /// <summary>The row of secondary information under a title.</summary>
        internal const string MetaRowClass = "ar-metarow";
        /// <summary>Free text recorded against a rule.</summary>
        internal const string NotesClass = "ar-notes";
        /// <summary>The scrolling content of a pane.</summary>
        internal const string PaneBodyClass = "ar-pane__body";
        /// <summary>One of the window sections, each with a header and a body.</summary>
        internal const string PaneClass = "ar-pane";
        /// <summary>The bar across the top of a pane.</summary>
        internal const string PaneHeaderClass = "ar-pane__header";
        /// <summary>A secondary line under a pane header.</summary>
        internal const string PaneNoteClass = "ar-pane__note";
        /// <summary>The title text in a pane header.</summary>
        internal const string PaneTitleClass = "ar-pane__title";
        /// <summary>One pill in a pill group.</summary>
        internal const string PillClass = "ar-pill";
        /// <summary>A row of pills, used where several small values sit together.</summary>
        internal const string PillsClass = "ar-pills";
        /// <summary>The button that auditions a clip.</summary>
        internal const string PlayClass = "ar-play";
        /// <summary>The main action of a toolbar or footer, drawn with emphasis.</summary>
        internal const string PrimaryClass = "ar-primary";
        /// <summary>The track of the scan progress bar.</summary>
        internal const string ProgressClass = "ar-progress";
        /// <summary>The filled part of the scan progress bar.</summary>
        internal const string ProgressFillClass = "ar-progress__fill";
        /// <summary>A generic row inside a pane body.</summary>
        internal const string RowClass = "ar-row";
        /// <summary>The name of a rule.</summary>
        internal const string RuleLabelClass = "ar-rule-label";
        /// <summary>One rule in the rule list.</summary>
        internal const string RuleRowClass = "ar-rule-row";
        /// <summary>A rule that is switched off. Modifier on the rule row.</summary>
        internal const string RuleRowOffClass = "ar-rule-row--off";
        /// <summary>The container holding a group of related rules.</summary>
        internal const string RuleSetClass = "ar-ruleset";
        /// <summary>What a rule expects, shown beside its name.</summary>
        internal const string RuleTargetClass = "ar-rule-target";
        /// <summary>The search field filtering the clip list.</summary>
        internal const string SearchClass = "ar-search";
        /// <summary>A titled block grouping related rows.</summary>
        internal const string SectionClass = "ar-section";
        /// <summary>The hairline under a section title.</summary>
        internal const string SectionRuleClass = "ar-section__rule";
        /// <summary>The heading of a section.</summary>
        internal const string SectionTitleClass = "ar-section__title";
        /// <summary>A toggle switching one import setting on or off.</summary>
        internal const string SettingToggleClass = "ar-setting-toggle";
        /// <summary>The status strip along the bottom of the window.</summary>
        internal const string StatusClass = "ar-status";
        /// <summary>The message inside the status strip.</summary>
        internal const string StatusTextClass = "ar-status__text";
        /// <summary>The value a rule is aiming for, wherever it is shown on its own.</summary>
        internal const string TargetClass = "ar-target";
        /// <summary>The toolbar across the top of the window.</summary>
        internal const string ToolbarClass = "ar-toolbar";
        /// <summary>One button in the toolbar.</summary>
        internal const string ToolButtonClass = "ar-tool-button";
        /// <summary>A toolbar button whose action cannot be undone. Modifier on the tool button.</summary>
        internal const string ToolButtonDangerClass = "ar-tool-button--danger";
        /// <summary>Marks a value worth a look but not a failure, wherever such a value is shown.</summary>
        internal const string WarnClass = "ar-warn";

        private const string LightClass = "ar-light";
        private const string RootClass = "ar-root";

        /// <summary>The GUID of the window's style sheet, from its meta file.</summary>
        private const string SheetGuid = "d8b09094dba6c8843918cdbc7ca28d4b";

        /// <summary>
        /// Marks the root, attaches the shared and the window's own sheets, and paints the tree from
        /// the active theme, repainting it whenever that theme changes.
        /// </summary>
        /// <param name="root">The root element of the window.</param>
        /// <returns>False when the window's own sheet is missing, so the caller can report it.</returns>
        internal static bool Apply(VisualElement root)
        {
            if (root == null)
                return false;

            root.AddToClassList(RootClass);

            // The sheet carries a light variant of its own colors as the fallback. It follows the
            // theme's effective editor theme rather than Unity's, so the settings page preview shows the
            // editor theme it is previewing rather than the one the editor happens to run.
            root.EnableInClassList(LightClass, !EditorThemeProvider.IsDarkMode);

            EditorUssTheme.Apply(root, CreatePainter());

            return EditorStyleSheets.Apply(root, SheetGuid);
        }

        /// <summary>
        /// Maps the classes whose color means the same here as anywhere else in the Base windows onto
        /// the palette.
        /// </summary>
        /// <remarks>
        /// Registered base class first and its modifiers after, because the painter applies the list
        /// in order and an element carrying both has to end on the modifier. Hover and selected
        /// states stay with the sheet: a painter writes inline styles, which no pseudo state can
        /// override. Unity's own collection and column header classes stay there too.
        /// </remarks>
        /// <returns>The painter the window is drawn with.</returns>
        private static EditorUssPainter CreatePainter() => new EditorUssPainter()
            .Background(PaneClass, color: () => EditorPalette.Card)
            .Background(PaneHeaderClass, color: () => EditorTableStyles.HeaderColor)
            .Border(PaneHeaderClass, color: () => EditorPalette.Separator)
            .Background(CardClass, color: () => EditorPalette.Card)
            .Background(StatusClass, color: () => EditorTableStyles.HeaderColor)
            .Border(StatusClass, color: () => EditorPalette.Separator)
            .Background(FooterClass, color: () => EditorTableStyles.HeaderColor)
            .Border(FooterClass, color: () => EditorPalette.Separator)
            .Background(ProgressClass, color: () => EditorPalette.KeyCap)
            .Background(ProgressFillClass, color: () => EditorPalette.Accent)
            .Background(PrimaryClass, color: () => EditorPalette.Accent)
            .Background(SectionRuleClass, color: () => EditorPalette.Hover)
            .Background(RuleTargetClass, color: () => EditorPalette.SelectionFill)
            .Background(ChipClass, color: () => EditorPalette.KeyCap)
            .Background(BadgeClass, color: () => EditorPalette.KeyCap)
            .Background(PlayClass, color: () => EditorPalette.KeyCap)
            .Background(ToolButtonClass, color: () => EditorPalette.KeyCap)
            .Border(GridRowClass, color: () => EditorPalette.Hover)
            .Border(GridRowHeadClass, color: () => EditorPalette.Separator)
            .Border(GridRowChangedClass, color: () => EditorPalette.Warning)
            .Border(EmptyRingClass, color: () => EditorPalette.DimText)
            .Text(PaneTitleClass, color: () => EditorPalette.DimText)
            .Text(PaneNoteClass, color: () => EditorPalette.DimText)
            .Text(SectionTitleClass, color: () => EditorPalette.DimText)
            .Text(StatusTextClass, color: () => EditorPalette.DimText)
            .Text(CellClass, color: () => EditorPalette.Text)
            .Text(CellNumberClass, color: () => EditorPalette.DimText)
            .Text(CellDimClass, color: () => EditorPalette.DimText)
            .Text(CellTargetClass, color: () => EditorPalette.Warning)
            .Text(CellGoodClass, color: () => EditorPalette.Success)
            .Text(CellBadClass, color: () => EditorPalette.Danger)
            .Text(GridCellClass, color: () => EditorPalette.Text)
            .Text(GridCellHeadClass, color: () => EditorPalette.DimText)
            .Text(GridCellDimClass, color: () => EditorPalette.DimText)
            .Text(DetailClass, color: () => EditorPalette.DimText)
            .Text(DetailTitleClass, color: () => EditorPalette.Text)
            .Text(DetailRuleClass, color: () => EditorPalette.Text)
            .Text(DetailReasonClass, color: () => EditorPalette.DimText)
            .Text(DetailPathClass, color: () => EditorPalette.DimText)
            .Text(ChipClass, color: () => EditorPalette.DimText)
            .Text(BadgeClass, color: () => EditorPalette.DimText)
            .Text(PlayClass, color: () => EditorPalette.DimText)
            .Text(ToolButtonClass, color: () => EditorPalette.DimText)
            .Text(RuleLabelClass, color: () => EditorPalette.Text)
            .Text(RuleTargetClass, color: () => EditorPalette.Accent)
            .Text(ConnClass, color: () => EditorPalette.DimText)
            .Text(GhostClass, color: () => EditorPalette.DimText)
            .Text(EmptyGlyphClass, color: () => EditorPalette.DimText)
            .Text(EmptyTitleClass, color: () => EditorPalette.Text)
            .Text(EmptyBodyClass, color: () => EditorPalette.DimText)
            .Text(ChipGoodClass, color: () => EditorPalette.Success)
            .Text(ChipWarnClass, color: () => EditorPalette.Warning)
            .Text(ChipBadClass, color: () => EditorPalette.Danger)
            .Text(PillClass, color: () => EditorPalette.Danger);
    }
}