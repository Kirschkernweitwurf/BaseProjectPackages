using UnityEditor;
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

        private const string StyleSheetFilter = "StateMachineMonitor t:StyleSheet";

        /// <summary>Applies the root classes and attaches the style sheet.</summary>
        /// <param name="root">The root element of the window.</param>
        internal static void Apply(VisualElement root)
        {
            root.AddToClassList(RootClass);

            if (!EditorGUIUtility.isProSkin)
                root.AddToClassList(LightClass);

            foreach (string guid in AssetDatabase.FindAssets(StyleSheetFilter))
            {
                StyleSheet sheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(AssetDatabase.GUIDToAssetPath(guid));

                if (sheet == null)
                    continue;

                root.styleSheets.Add(sheet);

                return;
            }
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
    }
}