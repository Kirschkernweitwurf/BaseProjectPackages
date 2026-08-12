using UnityEditor;
using UnityEngine;

namespace Base.AttributePackage.Editor.Collections
{
    /// <summary>
    /// Shared metrics, labels and small controls for the list and table renderers, so both look and
    /// behave the same and neither carries a raw number of its own.
    /// </summary>
    internal static class CollectionGui
    {
        /// <summary>Label of the add button.</summary>
        public const string AddLabel = "+";

        /// <summary>Width of a square row button.</summary>
        public const float ButtonWidth = 22f;

        /// <summary>Cancel label of the delete confirmation dialog.</summary>
        public const string ConfirmCancel = "Cancel";

        /// <summary>Accept label of the delete confirmation dialog.</summary>
        public const string ConfirmDelete = "Delete";

        /// <summary>Horizontal gap between two controls in a row.</summary>
        public const float Gap = 4f;

        /// <summary>Label of the remove button.</summary>
        public const string RemoveLabel = "\u2715";

        /// <summary>Vertical gap left between two rows so a list does not read as one solid block.</summary>
        public const float RowGap = 3f;

        /// <summary>How far a striped row is tinted from the inspector background.</summary>
        private const float StripeStrength = 0.045f;

        /// <summary>
        /// Tints every other row of a list. The stripe is a shift from the background rather than a
        /// fixed color, so the same constant works on both editor skins.
        /// </summary>
        /// <param name="rect">The row to tint.</param>
        /// <param name="index">Position of the row in the list.</param>
        public static void DrawStripe(Rect rect, int index)
        {
            if (index % 2 != 0 || Event.current.type != EventType.Repaint)
                return;

            EditorGUI.DrawRect(rect, EditorGUIUtility.isProSkin
                ? new Color(1f, 1f, 1f, StripeStrength)
                : new Color(0f, 0f, 0f, StripeStrength));
        }

        /// <summary>Width of the reorder arrows, which need less room than a full button.</summary>
        public const float SmallButtonWidth = 18f;

        private const int GlyphFontSize = 11;
        private const int LabelFontSize = 10;

        /// <summary>Height of a single control line.</summary>
        public static float Line => EditorGUIUtility.singleLineHeight;

        /// <summary>Vertical gap between two rows.</summary>
        public static float Spacing => EditorGUIUtility.standardVerticalSpacing;

        /// <summary>
        /// Style for the arrow and cross glyphs. The mini button font renders them at label size, which
        /// makes an arrow taller than the row it reorders, so the glyphs get their own smaller size.
        /// </summary>
        public static GUIStyle GlyphButton => _glyphButton ??= new GUIStyle(EditorStyles.miniButton)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = GlyphFontSize,
            padding = new RectOffset(0, 0, 0, 0)
        };

        /// <summary>Style for the buttons that carry a word rather than a glyph.</summary>
        public static GUIStyle LabelButton => _labelButton ??= new GUIStyle(EditorStyles.miniButton)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = LabelFontSize,
            padding = new RectOffset(0, 0, 0, 0)
        };

        private static GUIStyle _glyphButton;
        private static GUIStyle _labelButton;

        /// <summary>Draws a small glyph button and returns whether it was clicked.</summary>
        /// <param name="rect">Where to draw it.</param>
        /// <param name="label">The glyph on the button.</param>
        /// <param name="enabled">Whether the button is clickable.</param>
        /// <returns>True when clicked.</returns>
        public static bool SmallButton(Rect rect, string label, bool enabled = true)
        {
            using (new EditorGUI.DisabledScope(!enabled))
                return GUI.Button(rect, label, GlyphButton);
        }

        /// <summary>
        /// Removes an element from a serialized array. Object reference elements are cleared first,
        /// because Unity's delete only nulls them on the first call and removes them on the second.
        /// </summary>
        /// <param name="array">The serialized array to remove from.</param>
        /// <param name="index">Index of the element to remove.</param>
        public static void DeleteElement(SerializedProperty array, int index)
        {
            SerializedProperty element = array.GetArrayElementAtIndex(index);

            if (element.propertyType == SerializedPropertyType.ObjectReference
                && element.objectReferenceValue != null)
                element.objectReferenceValue = null;

            int size = array.arraySize;
            array.DeleteArrayElementAtIndex(index);

            if (array.arraySize == size)
                array.DeleteArrayElementAtIndex(index);
        }

        /// <summary>Asks before removing a row, when the caller wants a confirmation.</summary>
        /// <param name="label">What is being removed, shown in the dialog.</param>
        /// <param name="required">Whether a confirmation is wanted at all.</param>
        /// <returns>True when the removal should go ahead.</returns>
        public static bool ConfirmRemoval(string label, bool required)
        {
            if (!required)
                return true;

            return EditorUtility.DisplayDialog(ConfirmDelete, $"Remove {label}?", ConfirmDelete, ConfirmCancel);
        }
    }
}