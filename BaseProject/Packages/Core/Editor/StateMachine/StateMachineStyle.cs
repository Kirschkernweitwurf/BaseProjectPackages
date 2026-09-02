using Base.EditorUIPackage.Editor;
using UnityEngine.UIElements;

namespace Base.CorePackage.Editor.StateMachine
{
    /// <summary>
    /// The USS class names the monitor applies from code, and the one place that finds and attaches its
    /// style sheet. Everything visual lives in the sheet, so the look can be changed without touching the
    /// views, and the light editor theme is a class on the root rather than a second sheet.
    /// </summary>
    internal static class StateMachineStyle
    {
        /// <summary>A state node the machine is currently in. Modifier on the node.</summary>
        internal const string ActiveNodeClass = "sm-node--active";
        /// <summary>The pseudo node standing for transitions that leave from any state.</summary>
        internal const string AnyNodeClass = "sm-node--any";
        /// <summary>The surface the state nodes and their edges are laid out on.</summary>
        internal const string CanvasClass = "sm-canvas";
        /// <summary>A small inline pill carrying one value, such as a transition count.</summary>
        internal const string ChipClass = "sm-chip";
        /// <summary>A chip reporting a healthy value. Modifier on the chip.</summary>
        internal const string ChipGoodClass = "sm-chip--good";
        /// <summary>The label of the transition that last fired. Modifier on the edge label.</summary>
        internal const string EdgeLabelActiveClass = "sm-edge-label--active";
        /// <summary>The label sitting on a transition edge.</summary>
        internal const string EdgeLabelClass = "sm-edge-label";
        /// <summary>The explanatory paragraph of the empty state.</summary>
        internal const string EmptyBodyClass = "sm-empty__body";
        /// <summary>The panel shown when there is no state machine to monitor.</summary>
        internal const string EmptyClass = "sm-empty";
        /// <summary>The glyph at the center of the empty state.</summary>
        internal const string EmptyGlyphClass = "sm-empty__glyph";
        /// <summary>The ring drawn around the empty state glyph.</summary>
        internal const string EmptyRingClass = "sm-empty__ring";
        /// <summary>The headline of the empty state.</summary>
        internal const string EmptyTitleClass = "sm-empty__title";
        /// <summary>The name half of a labeled field row.</summary>
        internal const string FieldLabelClass = "sm-field__label";
        /// <summary>One labeled field, holding a label and a value side by side.</summary>
        internal const string FieldRowClass = "sm-field";
        /// <summary>The value half of a labeled field row.</summary>
        internal const string FieldValueClass = "sm-field__value";
        /// <summary>The state the machine starts in. Modifier on the node.</summary>
        internal const string InitialNodeClass = "sm-node--initial";
        /// <summary>
        /// Set on the root while the light theme is active, which is how the sheet switches its palette
        /// without a second sheet.
        /// </summary>
        internal const string LightClass = "sm-light";
        /// <summary>One machine in the list on the left.</summary>
        internal const string MachineRowClass = "sm-machine";
        /// <summary>The machine whose graph is being shown. Modifier on the machine row.</summary>
        internal const string MachineRowSelectedClass = "sm-machine--selected";
        /// <summary>The current state name shown on a machine row.</summary>
        internal const string MachineStateClass = "sm-machine__state";
        /// <summary>The machine name shown on a machine row.</summary>
        internal const string MachineTitleClass = "sm-machine__title";
        /// <summary>One state, drawn as a box on the canvas.</summary>
        internal const string NodeClass = "sm-node";
        /// <summary>The state name inside a node.</summary>
        internal const string NodeLabelClass = "sm-node__label";
        /// <summary>The scrolling content of a pane.</summary>
        internal const string PaneBodyClass = "sm-pane__body";
        /// <summary>One of the window sections, each with a header and a body.</summary>
        internal const string PaneClass = "sm-pane";
        /// <summary>The bar across the top of a pane.</summary>
        internal const string PaneHeaderClass = "sm-pane__header";
        /// <summary>A secondary line under a pane header, for context rather than data.</summary>
        internal const string PaneNoteClass = "sm-pane__note";
        /// <summary>The title text in a pane header.</summary>
        internal const string PaneTitleClass = "sm-pane__title";
        /// <summary>The window root. Everything the sheet styles sits under it.</summary>
        internal const string RootClass = "sm-root";
        /// <summary>A generic row inside a pane body.</summary>
        internal const string RowClass = "sm-row";
        /// <summary>A titled block grouping related rows inside a pane.</summary>
        internal const string SectionClass = "sm-section";
        /// <summary>The status strip along the bottom of the window.</summary>
        internal const string StatusClass = "sm-status";
        /// <summary>The message inside the status strip.</summary>
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
            // to. It follows the theme's effective editor theme rather than Unity's, so the preview on the
            // settings page shows the right one.
            root.EnableInClassList(LightClass, !EditorThemeProvider.IsDarkMode);

            EditorUssTheme.Apply(root, CreatePainter());

            return EditorStyleSheets.Apply(root, SheetGuid);
        }

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
            .Background(PaneClass, color: () => EditorPalette.Card)
            .Background(PaneHeaderClass, color: () => EditorTableStyles.HeaderColor)
            .Border(PaneHeaderClass, color: () => EditorPalette.Separator)
            .Text(PaneTitleClass, color: () => EditorPalette.DimText)
            .Text(PaneNoteClass, color: () => EditorPalette.DimText)
            .Background(MachineRowSelectedClass, color: () => EditorPalette.SelectionFill)
            .Border(MachineRowSelectedClass, color: () => EditorPalette.Accent)
            .Text(MachineTitleClass, color: () => EditorPalette.Text)
            .Text(MachineStateClass, color: () => EditorPalette.Success)
            .Background(ChipClass, color: () => EditorPalette.KeyCap)
            .Text(ChipClass, color: () => EditorPalette.DimText)
            .Text(ChipGoodClass, color: () => EditorPalette.Success)
            .Background(StatusClass, color: () => EditorTableStyles.HeaderColor)
            .Text(StatusTextClass, color: () => EditorPalette.DimText)
            .Text(EmptyTitleClass, color: () => EditorPalette.DimText)
            .Text(EmptyBodyClass, color: () => EditorPalette.DimText)
            .Text(FieldLabelClass, color: () => EditorPalette.DimText)
            .Text(FieldValueClass, color: () => EditorPalette.Text);
    }
}