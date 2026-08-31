using Base.EditorUiPackage;
using UnityEngine.UIElements;

namespace Base.CorePackage.Editor.StateMachine
{
    /// <summary>
    /// The USS class names the monitor applies from code, and the one place that finds and attaches its
    /// style sheet. Everything visual lives in the sheet, so the look can be changed without touching the
    /// views, and the light skin is a class on the root rather than a second sheet.
    /// </summary>
    internal static class StateMachineStyle
    {
        internal const string ActiveNodeClass = "sm-node--active";
        internal const string AnyNodeClass = "sm-node--any";
        internal const string CanvasClass = "sm-canvas";
        internal const string ChipClass = "sm-chip";
        internal const string ChipGoodClass = "sm-chip--good";
        internal const string EdgeLabelClass = "sm-edge-label";
        internal const string EdgeLabelActiveClass = "sm-edge-label--active";
        internal const string EmptyBodyClass = "sm-empty__body";
        internal const string EmptyClass = "sm-empty";
        internal const string EmptyGlyphClass = "sm-empty__glyph";
        internal const string EmptyRingClass = "sm-empty__ring";
        internal const string EmptyTitleClass = "sm-empty__title";
        internal const string FieldLabelClass = "sm-field__label";
        internal const string FieldRowClass = "sm-field";
        internal const string FieldValueClass = "sm-field__value";
        internal const string InitialNodeClass = "sm-node--initial";
        internal const string LightClass = "sm-light";
        internal const string MachineRowClass = "sm-machine";
        internal const string MachineRowSelectedClass = "sm-machine--selected";
        internal const string MachineStateClass = "sm-machine__state";
        internal const string MachineTitleClass = "sm-machine__title";
        internal const string NodeClass = "sm-node";
        internal const string NodeLabelClass = "sm-node__label";
        internal const string PaneBodyClass = "sm-pane__body";
        internal const string PaneClass = "sm-pane";
        internal const string PaneHeaderClass = "sm-pane__header";
        internal const string PaneNoteClass = "sm-pane__note";
        internal const string PaneTitleClass = "sm-pane__title";
        internal const string RootClass = "sm-root";
        internal const string RowClass = "sm-row";
        internal const string SectionClass = "sm-section";
        internal const string StatusClass = "sm-status";
        internal const string StatusTextClass = "sm-status__text";

        /// <summary>The GUID of the monitor's style sheet, from its meta file.</summary>
        private const string SheetGuid = "58438bffd19f6c9419859e00b5d779da";

        /// <summary>
        /// Applies the root classes, attaches the shared and the monitor's own style sheets, and keeps
        /// the whole tree in step with the active theme.
        /// </summary>
        /// <param name="root">The root element of the window.</param>
        /// <returns>False when the monitor's own sheet is missing, so the caller can report it.</returns>
        internal static bool Apply(VisualElement root)
        {
            if (root == null)
                return false;

            root.AddToClassList(RootClass);

            // The sheet keeps a light variant of its own colors, which is what the window falls back
            // to. It follows the theme's effective skin rather than Unity's, so the preview on the
            // settings page shows the right one.
            root.EnableInClassList(LightClass, !EditorThemeProvider.IsDarkSkin);

            EditorUssTheme.Apply(root, CreatePainter());

            return EditorStyleSheets.Apply(root, SheetGuid);
        }

        /// <summary>
        /// Maps the classes whose color means the same thing here as anywhere else in the Base windows
        /// onto the palette.
        /// </summary>
        /// <remarks>
        /// Deliberately short. What is registered is the window furniture: panes, headers, hairlines,
        /// text and the accent. The canvas, the node fills and the edge colors stay with the sheet,
        /// because a state machine graph is the only thing that has a meaning for them and there is no
        /// palette name that fits.
        /// </remarks>
        /// <returns>The painter the monitor is drawn with.</returns>
        private static EditorUssPainter CreatePainter() => new EditorUssPainter()
            .Background(PaneClass, () => EditorPalette.Card)
            .Background(PaneHeaderClass, () => EditorTableStyles.HeaderColor)
            .Border(PaneHeaderClass, () => EditorPalette.Separator)
            .Text(PaneTitleClass, () => EditorPalette.DimText)
            .Text(PaneNoteClass, () => EditorPalette.DimText)
            .Background(MachineRowSelectedClass, () => EditorPalette.SelectionFill)
            .Border(MachineRowSelectedClass, () => EditorPalette.Accent)
            .Text(MachineTitleClass, () => EditorPalette.Text)
            .Text(MachineStateClass, () => EditorPalette.Success)
            .Background(ChipClass, () => EditorPalette.KeyCap)
            .Text(ChipClass, () => EditorPalette.DimText)
            .Text(ChipGoodClass, () => EditorPalette.Success)
            .Background(StatusClass, () => EditorTableStyles.HeaderColor)
            .Text(StatusTextClass, () => EditorPalette.DimText)
            .Text(EmptyTitleClass, () => EditorPalette.DimText)
            .Text(EmptyBodyClass, () => EditorPalette.DimText)
            .Text(FieldLabelClass, () => EditorPalette.DimText)
            .Text(FieldValueClass, () => EditorPalette.Text);

        /// <summary>Builds one of the small rounded labels used in the headers and the status bar.</summary>
        /// <param name="text">What the chip says.</param>
        /// <param name="variant">Extra class controlling the color, or null for the neutral one.</param>
        /// <returns>The chip, ready to be added.</returns>
        internal static Label Chip(string text, string variant)
        {
            Label chip = new(text);

            chip.AddToClassList(ChipClass);

            if (!string.IsNullOrEmpty(variant))
                chip.AddToClassList(variant);

            return chip;
        }
    }
}