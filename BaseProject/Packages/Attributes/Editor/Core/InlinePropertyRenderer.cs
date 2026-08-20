using System;
using System.Collections.Generic;
using Base.UtilityPackage.Editor;
using UnityEditor;
using UnityEngine;

namespace Base.AttributePackage.Editor.Drawers
{
    /// <summary>
    /// Draws the children of an <see cref="InlinePropertyAttribute"/> member on the field's own row.
    /// </summary>
    /// <remarks>
    /// Only leaf children fit on a row. A child that is itself a nested object, an array, or anything
    /// else whose height is not one line cannot honestly be squeezed into a cell, so a type containing
    /// one falls back to the ordinary foldout rather than drawing something misleading.
    /// </remarks>
    internal static class InlinePropertyRenderer
    {
        private const float CellGap = 4f;
        private const float LabelPadding = 6f;
        private const float MaximumLabelShare = 0.7f;

        // Reused between rows so the renderer allocates one list per repaint instead of one per row.
        private static readonly List<SerializedProperty> Children = new();

        /// <summary>Draws the member inline, or reports that it cannot be.</summary>
        /// <param name="context">The member currently being drawn.</param>
        /// <param name="nestedType">The nested type behind the member.</param>
        /// <returns>True when the member was drawn inline.</returns>
        public static bool TryDraw(in MemberContext context, Type nestedType)
        {
            InlinePropertyAttribute attribute = context.GetAttribute<InlinePropertyAttribute>();
            if (attribute == null || nestedType == null)
                return false;

            if (!Collect(context.Property))
                return false;

            Rect row = EditorGUILayout.GetControlRect(true, EditorGUIUtility.singleLineHeight);
            Rect content = EditorGUI.PrefixLabel(row, context.EffectiveLabel);

            // The prefix label already consumed the indent, and each child draws its own label inside
            // its cell, so both the ambient indent and the inspector's label width are set aside here.
            float labelWidth = EditorGUIUtility.labelWidth;

            using (new NoIndentScope())
                DrawCells(content);

            EditorGUIUtility.labelWidth = labelWidth;

            return true;
        }

        // Each child is measured against its own label, so the value starts where the text ends. There
        // is no setting for this: a fixed width cannot fit both "Min" and "Maximalistic", and any number
        // chosen for one of them is wrong for the other.
        private static void DrawCells(Rect content)
        {
            float width = (content.width - CellGap * (Children.Count - 1)) / Children.Count;

            for (int i = 0; i < Children.Count; i++)
            {
                Rect cell = new(content.x + i * (width + CellGap), content.y, width, content.height);
                GUIContent label = ScratchContent.For(Children[i].displayName);

                EditorGUIUtility.labelWidth = Mathf.Min(EditorStyles.label.CalcSize(label).x + LabelPadding,
                    width * MaximumLabelShare);

                EditorGUI.PropertyField(cell, Children[i], label);
            }
        }

        // Returns false when the type holds something a single row cannot show.
        private static bool Collect(SerializedProperty property)
        {
            Children.Clear();

            SerializedProperty iterator = property.Copy();
            SerializedProperty end = property.GetEndProperty();
            bool enterChildren = true;

            while (iterator.NextVisible(enterChildren) && !SerializedProperty.EqualContents(iterator, end))
            {
                enterChildren = false;

                if (!IsLeaf(iterator))
                {
                    Children.Clear();
                    return false;
                }

                Children.Add(iterator.Copy());
            }

            return Children.Count > 0;
        }

        private static bool IsLeaf(SerializedProperty property)
        {
            if (property.isArray && property.propertyType != SerializedPropertyType.String)
                return false;

            if (property.propertyType == SerializedPropertyType.Generic)
                return false;

            return !property.hasVisibleChildren;
        }
    }
}