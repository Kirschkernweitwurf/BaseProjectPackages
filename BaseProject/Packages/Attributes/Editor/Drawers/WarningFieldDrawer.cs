using Base.AttributePackage.Editor.Handlers;
using Base.UtilityPackage.Editor;
using UnityEditor;
using UnityEngine;

namespace Base.AttributePackage.Editor.Drawers
{
    /// <summary>
    /// Shared base for drawers that fall back to the plain field and explain the problem in a compact
    /// warning below it, instead of drawing a dropdown that cannot be filled. Derived drawers decide
    /// which property types they support, what the warning says and how the field itself is drawn.
    /// </summary>
    internal abstract class WarningFieldDrawer : PropertyDrawer
    {
        private const float WarningSpacing = 2f;

        /// <summary>Message shown when the attribute sits on an unsupported field type.</summary>
        protected abstract string UsageMessage { get; }

        /// <summary>
        /// Extra height above and below the field's own line, for a drawer that paints outside it.
        /// </summary>
        /// <remarks>
        /// Zero for almost every drawer. A field that draws an outline or a shadow needs the room
        /// reserved, or it paints over whatever the inspector put next to it.
        /// </remarks>
        protected virtual float VerticalPadding => 0f;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (!IsSupported(property))
                return EditorGUIUtility.singleLineHeight;

            float height = EditorGUIUtility.singleLineHeight + VerticalPadding * 2f;

            return Evaluate(property) == null
                ? height
                : height + WarningSpacing + CompactHelpBox.Height;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (!IsSupported(property))
            {
                LabeledField.Hint(position, label, UsageMessage);
                return;
            }

            string warning = Evaluate(property);

            // The field sits inside its padding, so a drawer that paints past its own line has that room
            // above and below rather than taking it from the fields either side.
            Rect fieldRect = new(position.x, position.y + VerticalPadding, position.width,
                EditorGUIUtility.singleLineHeight);

            EditorGUI.BeginProperty(fieldRect, label, property);
            DrawField(fieldRect, property, label, warning == null);
            EditorGUI.EndProperty();

            if (warning == null)
                return;

            Rect warningRect = new(position.x, fieldRect.yMax + VerticalPadding + WarningSpacing,
                position.width, CompactHelpBox.Height);

            CompactHelpBox.Draw(warningRect, warning, EInfoBoxType.Warning);
        }

        /// <summary>Whether the drawer can handle the given property type.</summary>
        protected abstract bool IsSupported(SerializedProperty property);

        /// <summary>
        /// Returns the warning text, or null when everything the drawer needs is present. Always runs
        /// before <see cref="DrawField"/>, so it can prepare the data the field needs.
        /// </summary>
        protected abstract string Evaluate(SerializedProperty property);

        /// <summary>Draws the field itself. Complete is false while a warning is shown.</summary>
        protected abstract void DrawField(Rect rect, SerializedProperty property, GUIContent label, bool complete);
    }
}