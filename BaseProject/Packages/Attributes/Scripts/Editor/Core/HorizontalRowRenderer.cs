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
    /// has to be given an exact width and the handler pipeline inside it lays out with the ambient label
    /// width. Setting that width per cell is what keeps a three-field row readable instead of three
    /// labels crushed against three tiny controls.
    /// <para>
    /// Decorations are lifted above the row rather than drawn inside a cell. An info box or a separator
    /// spans the inspector, so leaving one in a cell makes that cell taller than its neighbors and
    /// pushes its own field a row out of line with them. Above the row is also where a full-width box
    /// belongs, whichever of the fields it happens to be written on.
    /// </para>
    /// </remarks>
    internal static class HorizontalRowRenderer
    {
        private const float IndentStep = 15f;
        private const float LabelPadding = 6f;
        private const float MaximumLabelShare = 0.6f;
        private const float MinimumCellWidth = 40f;
        private const float MinimumLabelWidth = 24f;
        private const float NoLabelWidth = 0.01f;
        private const float ViewPadding = 22f;

        // Reused between rows so drawing one does not allocate three lists per repaint.
        private static readonly List<SerializedProperty> Members = new();

        private static readonly List<FieldInfo> Fields = new();

        private static readonly List<HorizontalAttribute> Settings = new();

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

            int index = Collect(properties, startIndex, type, group);
            if (Members.Count == 0)
                return index;

            for (int i = 0; i < Members.Count; i++)
                MemberRenderer.DrawDecorations(Members[i], Fields[i], editor);

            float spacing = EditorGUIUtility.standardVerticalSpacing;
            float available = AvailableWidth() - spacing * (Members.Count - 1);
            float total = TotalWeight();

            EditorGUILayout.BeginHorizontal();

            for (int i = 0; i < Members.Count; i++)
            {
                float width = Mathf.Max(available * (Settings[i].Weight / total), MinimumCellWidth);

                DrawCell(i, width, editor);

                if (i + 1 < Members.Count)
                    GUILayout.Space(spacing);
            }

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

        private static HorizontalAttribute AttributeAt(List<SerializedProperty> properties, int index, Type type)
            => ReflectionCache.GetAttribute<HorizontalAttribute>(
                ReflectionCache.GetField(type, properties[index].name));

        // The row is laid out from the inspector width rather than from a rect, because the width of the
        // enclosing block is not known during the layout pass. The indent and the scrollbar are taken
        // off, which is what stops the last cell running past the right edge inside a title section.
        private static float AvailableWidth()
            => Mathf.Max(EditorGUIUtility.currentViewWidth - EditorGUI.indentLevel * IndentStep - ViewPadding,
                MinimumCellWidth);

        private static int Collect(List<SerializedProperty> properties, int startIndex, Type type, string group)
        {
            Members.Clear();
            Fields.Clear();
            Settings.Clear();

            int index = startIndex;

            while (index < properties.Count)
            {
                FieldInfo field = ReflectionCache.GetField(type, properties[index].name);
                HorizontalAttribute attribute = ReflectionCache.GetAttribute<HorizontalAttribute>(field);

                if (attribute == null || attribute.Group != group)
                    break;

                Members.Add(properties[index]);
                Fields.Add(field);
                Settings.Add(attribute);

                index++;
            }

            return index;
        }

        // The label width Unity uses is measured from the left edge of the inspector, not from the
        // field, so an indented control gets one step less than it was given. Adding the indent back is
        // what stops the text clipping into the value beside it.
        private static float MeasureLabel(int index, float width)
        {
            GUIContent label = ScratchContent.For(LabelText(index));
            float measured = EditorStyles.label.CalcSize(label).x + LabelPadding;
            float indent = EditorGUI.indentLevel * IndentStep;

            return Mathf.Clamp(measured, MinimumLabelWidth, width * MaximumLabelShare) + indent;
        }

        // A computed label is measured against the name instead, since resolving it needs the member
        // context the cell has not built yet and the two are usually about the same length anyway.
        private static string LabelText(int index)
        {
            LabelAttribute label = ReflectionCache.GetAttribute<LabelAttribute>(Fields[index]);

            return label != null && !ValueResolver.IsMemberReference(label.Text)
                ? label.Text
                : ObjectNames.NicifyVariableName(Members[index].name);
        }

        private static float TotalWeight()
        {
            float total = 0f;

            foreach (HorizontalAttribute attribute in Settings)
                total += Mathf.Max(attribute.Weight, 0.01f);

            return Mathf.Max(total, 0.01f);
        }

        // The ambient label width belongs to a full-width row and leaves most of a cell empty. Each cell
        // is measured against its own label instead, so the value starts where the text ends rather than
        // at a column the row never had. Capped, because a cell whose label eats it has no room left for
        // the thing the label is naming.
        private static void DrawCell(int index, float width, AttributePackageEditor editor)
        {
            float labelWidth = EditorGUIUtility.labelWidth;

            EditorGUIUtility.labelWidth = Settings[index].ShowLabel
                ? MeasureLabel(index, width)
                : NoLabelWidth;

            EditorGUILayout.BeginVertical(GUILayout.Width(width));

            // Every cell's decorations were already drawn above the row, so none of them run again here.
            MemberRenderer.DrawWithoutDecorations(Members[index], Fields[index], editor,
                Settings[index].ShowLabel);

            EditorGUILayout.EndVertical();

            EditorGUIUtility.labelWidth = labelWidth;
        }
    }
}