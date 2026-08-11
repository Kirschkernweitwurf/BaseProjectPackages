using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Base.AttributePackage.Editor
{
    /// <summary>
    /// Draws a run of consecutive <see cref="HorizontalAttribute"/> members side by side on one row, at
    /// the relative widths they ask for.
    /// </summary>
    /// <remarks>
    /// The cells are laid out by hand rather than through a horizontal layout group, because each cell
    /// has to be given an exact width and the handler pipeline inside it lays out with the ambient
    /// label width. Setting that width per cell is what keeps a three-field row readable instead of
    /// three labels crushed against three tiny controls.
    /// </remarks>
    internal static class HorizontalRowRenderer
    {
        private const float CellGap = 4f;
        private const float LabelShare = 0.45f;
        private const float MinimumLabelWidth = 24f;

        /// <summary>
        /// Draws the row starting at the given index and returns the index of the first member after it.
        /// </summary>
        /// <param name="properties">All properties of the inspected object.</param>
        /// <param name="startIndex">Index of the first member of the row.</param>
        /// <param name="editor">The editor drawing the row.</param>
        /// <returns>The index of the first member after the row.</returns>
        public static int Draw(List<SerializedProperty> properties, int startIndex, AttributePackageEditor editor)
        {
            Type type = editor.target.GetType();
            string group = AttributeAt(properties, startIndex, type).Group;

            List<SerializedProperty> members = new();
            List<FieldInfo> fields = new();
            List<HorizontalAttribute> settings = new();

            int index = Collect(properties, startIndex, type, group, members, fields, settings);

            float total = TotalWeight(settings);
            float available = EditorGUIUtility.currentViewWidth;

            EditorGUILayout.BeginHorizontal();

            for (int i = 0; i < members.Count; i++)
                DrawCell(members[i], fields[i], settings[i], available * (settings[i].Weight / total), editor);

            EditorGUILayout.EndHorizontal();

            return index;
        }

        /// <summary>Advances past the row without drawing it, for when the section around it is closed.</summary>
        /// <param name="properties">All properties of the inspected object.</param>
        /// <param name="startIndex">Index of the first member of the row.</param>
        /// <param name="type">The inspected type.</param>
        /// <returns>The index of the first member after the row.</returns>
        public static int Skip(List<SerializedProperty> properties, int startIndex, Type type)
        {
            string group = AttributeAt(properties, startIndex, type).Group;
            int index = startIndex;

            while (index < properties.Count)
            {
                HorizontalAttribute attribute = AttributeAt(properties, index, type);
                if (attribute == null || attribute.Group != group)
                    break;

                index++;
            }

            return index;
        }

        private static HorizontalAttribute AttributeAt(List<SerializedProperty> properties, int index,
            Type type)
            => ReflectionCache.GetAttribute<HorizontalAttribute>(
                ReflectionCache.GetField(type, properties[index].name));

        private static int Collect(List<SerializedProperty> properties, int startIndex, Type type,
            string group, List<SerializedProperty> members, List<FieldInfo> fields,
            List<HorizontalAttribute> settings)
        {
            int index = startIndex;

            while (index < properties.Count)
            {
                FieldInfo field = ReflectionCache.GetField(type, properties[index].name);
                HorizontalAttribute attribute = ReflectionCache.GetAttribute<HorizontalAttribute>(field);

                if (attribute == null || attribute.Group != group)
                    break;

                members.Add(properties[index]);
                fields.Add(field);
                settings.Add(attribute);

                index++;
            }

            return index;
        }

        private static float TotalWeight(List<HorizontalAttribute> settings)
        {
            float total = 0f;

            foreach (HorizontalAttribute attribute in settings)
                total += Mathf.Max(attribute.Weight, 0.01f);

            return Mathf.Max(total, 0.01f);
        }

        // The ambient label width is the full inspector's, which in a narrow cell leaves no room for the
        // value. Each cell gets its own share instead, and a cell that hides its label gets none.
        private static void DrawCell(SerializedProperty property, FieldInfo field, HorizontalAttribute settings,
            float width, AttributePackageEditor editor)
        {
            float labelWidth = EditorGUIUtility.labelWidth;

            EditorGUIUtility.labelWidth = settings.ShowLabel
                ? Mathf.Max(width * LabelShare, MinimumLabelWidth)
                : 0.01f;

            EditorGUILayout.BeginVertical(GUILayout.Width(width - CellGap));
            MemberRenderer.Draw(property, field, editor, settings.ShowLabel);
            EditorGUILayout.EndVertical();

            EditorGUIUtility.labelWidth = labelWidth;
        }
    }
}