using Base.EditorUiPackage;
using UnityEngine.UIElements;

namespace Base.ToolPackage.Editor.AudioRules.Window
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
        internal const string AddClass = "ar-add";
        internal const string BadgeClass = "ar-badge";
        internal const string BadgeZeroClass = "ar-badge--zero";
        internal const string CardClass = "ar-card";
        internal const string CellBadClass = "ar-cell--bad";
        internal const string CellClass = "ar-cell";
        internal const string CellDimClass = "ar-cell--dim";
        internal const string CellGoodClass = "ar-cell--good";
        internal const string CellNumberClass = "ar-cell--number";
        internal const string CellTargetClass = "ar-cell--target";
        internal const string ChipBadClass = "ar-chip--bad";
        internal const string ChipClass = "ar-chip";
        internal const string ChipGoodClass = "ar-chip--good";
        internal const string ChipWarnClass = "ar-chip--warn";
        internal const string ClipCellClass = "ar-clip-cell";
        internal const string ClipNameClass = "ar-clip-name";
        internal const string ConnClass = "ar-conn";
        internal const string DetailClass = "ar-detail";
        internal const string DetailPathClass = "ar-detail--path";
        internal const string DetailPlaceholderClass = "ar-detail--placeholder";
        internal const string DetailReasonClass = "ar-detail--reason";
        internal const string DetailRuleClass = "ar-detail--rule";
        internal const string DetailTitleClass = "ar-detail--title";
        internal const string EmptyBodyClass = "ar-empty__body";
        internal const string EmptyClass = "ar-empty";
        internal const string EmptyGlyphClass = "ar-empty__glyph";
        internal const string EmptyGlyphNoteClass = "ar-empty__glyph--note";
        internal const string EmptyRingClass = "ar-empty__ring";
        internal const string EmptyTitleClass = "ar-empty__title";
        internal const string FooterClass = "ar-footer";
        internal const string GhostClass = "ar-ghost";
        internal const string GoodClass = "ar-good";
        internal const string GridCellClass = "ar-grid-cell";
        internal const string GridCellDimClass = "ar-grid-cell--dim";
        internal const string GridCellHeadClass = "ar-grid-cell--head";
        internal const string GridRowChangedClass = "ar-grid-row--changed";
        internal const string GridRowClass = "ar-grid-row";
        internal const string GridRowHeadClass = "ar-grid-row--head";
        internal const string GridRowMutedClass = "ar-grid-row--muted";
        internal const string MatchClass = "ar-match";
        internal const string MetaRowClass = "ar-metarow";
        internal const string NotesClass = "ar-notes";
        internal const string PaneBodyClass = "ar-pane__body";
        internal const string PaneClass = "ar-pane";
        internal const string PaneHeaderClass = "ar-pane__header";
        internal const string PaneNoteClass = "ar-pane__note";
        internal const string PaneTitleClass = "ar-pane__title";
        internal const string PillClass = "ar-pill";
        internal const string PillsClass = "ar-pills";
        internal const string PlayClass = "ar-play";
        internal const string PrimaryClass = "ar-primary";
        internal const string ProgressClass = "ar-progress";
        internal const string ProgressFillClass = "ar-progress__fill";
        internal const string RowClass = "ar-row";
        internal const string RuleLabelClass = "ar-rule-label";
        internal const string RuleRowClass = "ar-rule-row";
        internal const string RuleRowOffClass = "ar-rule-row--off";
        internal const string RuleSetClass = "ar-ruleset";
        internal const string RuleTargetClass = "ar-rule-target";
        internal const string SearchClass = "ar-search";
        internal const string SectionClass = "ar-section";
        internal const string SectionRuleClass = "ar-section__rule";
        internal const string SectionTitleClass = "ar-section__title";
        internal const string SettingToggleClass = "ar-setting-toggle";
        internal const string StatusClass = "ar-status";
        internal const string StatusTextClass = "ar-status__text";
        internal const string TargetClass = "ar-target";
        internal const string ToolButtonClass = "ar-tool-button";
        internal const string ToolButtonDangerClass = "ar-tool-button--danger";
        internal const string ToolbarClass = "ar-toolbar";
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
            // theme's effective skin rather than Unity's, so the settings page preview shows the
            // skin it is previewing rather than the one the editor happens to run.
            root.EnableInClassList(LightClass, !EditorThemeProvider.IsDarkSkin);

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
            .Background(PaneClass, () => EditorPalette.Card)
            .Background(PaneHeaderClass, () => EditorTableStyles.HeaderColor)
            .Border(PaneHeaderClass, () => EditorPalette.Separator)
            .Background(CardClass, () => EditorPalette.Card)
            .Background(StatusClass, () => EditorTableStyles.HeaderColor)
            .Border(StatusClass, () => EditorPalette.Separator)
            .Background(FooterClass, () => EditorTableStyles.HeaderColor)
            .Border(FooterClass, () => EditorPalette.Separator)
            .Background(ProgressClass, () => EditorPalette.KeyCap)
            .Background(ProgressFillClass, () => EditorPalette.Accent)
            .Background(PrimaryClass, () => EditorPalette.Accent)
            .Background(SectionRuleClass, () => EditorPalette.Hover)
            .Background(RuleTargetClass, () => EditorPalette.SelectionFill)
            .Background(ChipClass, () => EditorPalette.KeyCap)
            .Background(BadgeClass, () => EditorPalette.KeyCap)
            .Background(PlayClass, () => EditorPalette.KeyCap)
            .Background(ToolButtonClass, () => EditorPalette.KeyCap)
            .Border(GridRowClass, () => EditorPalette.Hover)
            .Border(GridRowHeadClass, () => EditorPalette.Separator)
            .Border(GridRowChangedClass, () => EditorPalette.Warning)
            .Border(EmptyRingClass, () => EditorPalette.DimText)
            .Text(PaneTitleClass, () => EditorPalette.DimText)
            .Text(PaneNoteClass, () => EditorPalette.DimText)
            .Text(SectionTitleClass, () => EditorPalette.DimText)
            .Text(StatusTextClass, () => EditorPalette.DimText)
            .Text(CellClass, () => EditorPalette.Text)
            .Text(CellNumberClass, () => EditorPalette.DimText)
            .Text(CellDimClass, () => EditorPalette.DimText)
            .Text(CellTargetClass, () => EditorPalette.Warning)
            .Text(CellGoodClass, () => EditorPalette.Success)
            .Text(CellBadClass, () => EditorPalette.Danger)
            .Text(GridCellClass, () => EditorPalette.Text)
            .Text(GridCellHeadClass, () => EditorPalette.DimText)
            .Text(GridCellDimClass, () => EditorPalette.DimText)
            .Text(DetailClass, () => EditorPalette.DimText)
            .Text(DetailTitleClass, () => EditorPalette.Text)
            .Text(DetailRuleClass, () => EditorPalette.Text)
            .Text(DetailReasonClass, () => EditorPalette.DimText)
            .Text(DetailPathClass, () => EditorPalette.DimText)
            .Text(ChipClass, () => EditorPalette.DimText)
            .Text(BadgeClass, () => EditorPalette.DimText)
            .Text(PlayClass, () => EditorPalette.DimText)
            .Text(ToolButtonClass, () => EditorPalette.DimText)
            .Text(RuleLabelClass, () => EditorPalette.Text)
            .Text(RuleTargetClass, () => EditorPalette.Accent)
            .Text(ConnClass, () => EditorPalette.DimText)
            .Text(GhostClass, () => EditorPalette.DimText)
            .Text(EmptyGlyphClass, () => EditorPalette.DimText)
            .Text(EmptyTitleClass, () => EditorPalette.Text)
            .Text(EmptyBodyClass, () => EditorPalette.DimText)
            .Text(ChipGoodClass, () => EditorPalette.Success)
            .Text(ChipWarnClass, () => EditorPalette.Warning)
            .Text(ChipBadClass, () => EditorPalette.Danger)
            .Text(PillClass, () => EditorPalette.Danger);
    }
}