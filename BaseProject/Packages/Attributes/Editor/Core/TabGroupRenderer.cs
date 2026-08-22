using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Base.AttributePackage.Editor.Drawers
{
    /// <summary>
    /// Draws a run of consecutive <see cref="TabAttribute"/> members as a tab bar with the selected
    /// tab's members below it. The selection is stored per owner type and group in
    /// <see cref="EditorPrefs"/>, as is the open state when the group asks for a foldout.
    /// </summary>
    internal static class TabGroupRenderer
    {
        private const float BarSpacing = 8f;
        private const float BlockOverlap = 3f;
        private const float ContentInset = 4f;
        private const string FoldoutKeyPrefix = "TABFOLD";
        private const float GroupSpacing = 6f;
        private const float IndentStep = 15f;
        private const string TabKeyPrefix = "TAB";
        private const float ViewPadding = 24f;

        private static GUIStyle FoldoutStyle => _foldoutStyle ??= new GUIStyle(EditorStyles.foldout)
        {
            fontStyle = FontStyle.Bold
        };

        // A copy of the box style with no top padding, so the bar sits flush against the top edge of
        // the block. The original is shared by every help box in the editor and must not be edited in
        // place.
        // No padding and no side margins, so the block is exactly as wide as the bar above it. Anything
        // the style adds on one side would show as the block reaching past the bar it belongs to.
        private static GUIStyle ContentStyle => _contentStyle ??= new GUIStyle(EditorStyles.helpBox)
        {
            padding = new RectOffset(0, 0, 0, 0),

            // No margins either. A margin on one side only would inset the block without inseting the
            // bar that fills it, and the two would stop being the same width.
            margin = new RectOffset(0, 0, 0, EditorStyles.helpBox.margin.bottom),

            // The help box style paints its background beyond its own rect, which is fine for a box that
            // stands alone and is exactly what made this one bleed past the fields beside it.
            overflow = new RectOffset(0, 0, 0, 0)
        };

        private static GUIStyle _contentStyle;
        private static GUIStyle _foldoutStyle;

        /// <summary>
        /// Draws the tab group starting at the given index and returns the index of the first member
        /// after the group.
        /// </summary>
        /// <param name="properties">All properties of the inspected object.</param>
        /// <param name="startIndex">Index of the first member of the group.</param>
        /// <param name="editor">The editor drawing the group.</param>
        /// <returns>The index of the first member after the group.</returns>
        public static int Draw(List<SerializedProperty> properties, int startIndex, UnityEditor.Editor editor)
        {
            Type type = editor.target.GetType();
            TabAttribute first = AttributeAt(properties, startIndex, type);
            string group = first.Group;

            List<SerializedProperty> members = new();
            List<FieldInfo> fields = new();
            List<string> memberTabs = new();
            List<string> tabOrder = new();

            int index = Collect(properties, startIndex, type, group, members, fields, memberTabs, tabOrder);

            GUILayout.Space(GroupSpacing);

            if (!DrawFoldout(type, first, tabOrder))
            {
                GUILayout.Space(GroupSpacing);
                return index;
            }

            // The bar's space is reserved now and the bar itself is drawn at the very end, so the block
            // is painted first and the bar lands on top of it. Drawn the other way round, the block's
            // background covers the bottom edge of the bar it belongs to.
            string[] tabs = tabOrder.ToArray();
            string key = StateKey.For(type, TabKeyPrefix, group);
            int stored = Mathf.Clamp(EditorPrefs.GetInt(key, 0), 0, tabs.Length - 1);

            int rows = WrappedToolbar.Rows(tabs, AvailableWidth());
            Rect bar = GUILayoutUtility.GetRect(0f, rows * EditorGUIUtility.singleLineHeight,
                EditorStyles.toolbarButton, GUILayout.ExpandWidth(true));

            string selectedTab = tabs[stored];

            // The layout's gap between two controls is taken back, and then a little more. Both the bar
            // and the block have rounded corners, so meeting exactly on an edge leaves four notches
            // where the two curves pull away from each other. Tucking the block up under the bar puts
            // its corners behind the bar's, and the pair reads as one shape.
            GUILayout.Space(-EditorGUIUtility.standardVerticalSpacing - BlockOverlap);

            EditorGUILayout.BeginVertical(ContentStyle);

            // The block's top is tucked behind the bar, so the first field starts below that overlap
            // rather than at the block's own edge.
            GUILayout.Space(BarSpacing + BlockOverlap);

            // The fields are inset by hand rather than by the box style, so the block's own width stays
            // exactly the bar's and only the contents move in from its edges.
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(ContentInset);
            EditorGUILayout.BeginVertical();

            for (int i = 0; i < members.Count; i++)
            {
                if (memberTabs[i] == selectedTab)
                    MemberRenderer.Draw(members[i], fields[i], editor);
            }

            EditorGUILayout.EndVertical();
            GUILayout.Space(ContentInset);
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(ContentInset);
            EditorGUILayout.EndVertical();

            int picked = WrappedToolbar.DrawAt(bar, stored, tabs);

            if (picked != stored)
            {
                EditorPrefs.SetInt(key, picked);
                GUI.changed = true;
            }

            GUILayout.Space(GroupSpacing);

            return index;
        }

        /// <summary>
        /// Advances past the tab group without drawing it, for when the section around it is collapsed.
        /// </summary>
        /// <param name="properties">All properties of the inspected object.</param>
        /// <param name="startIndex">Index of the first member of the group.</param>
        /// <param name="type">The inspected type.</param>
        /// <returns>The index of the first member after the group.</returns>
        public static int Skip(List<SerializedProperty> properties, int startIndex, Type type)
        {
            string group = AttributeAt(properties, startIndex, type).Group;
            int index = startIndex;

            while (index < properties.Count)
            {
                TabAttribute tab = AttributeAt(properties, index, type);
                if (tab == null || tab.Group != group)
                    break;

                index++;
            }

            return index;
        }

        private static TabAttribute AttributeAt(List<SerializedProperty> properties, int index, Type type)
            => ReflectionCache.GetAttribute<TabAttribute>(ReflectionCache.GetField(type, properties[index].name));

        // Returns whether the group body should be drawn. A group without the foldout setting always is.
        private static bool DrawFoldout(Type type, TabAttribute attribute, List<string> tabOrder)
        {
            if (!attribute.Foldout)
                return true;

            // A group name is optional, so the first tab names the header when there is none.
            string header = string.IsNullOrEmpty(attribute.Group)
                ? tabOrder[0]
                : attribute.Group;

            string key = StateKey.For(type, FoldoutKeyPrefix, header);
            bool stored = EditorPrefs.GetBool(key, attribute.DefaultExpanded);
            bool expanded = EditorGUILayout.Foldout(stored, header, true, FoldoutStyle);

            if (expanded != stored)
                EditorPrefs.SetBool(key, expanded);

            return expanded;
        }

        private static int Collect(List<SerializedProperty> properties, int startIndex, Type type, string group,
            List<SerializedProperty> members, List<FieldInfo> fields, List<string> memberTabs, List<string> tabOrder)
        {
            int index = startIndex;

            while (index < properties.Count)
            {
                FieldInfo field = ReflectionCache.GetField(type, properties[index].name);
                TabAttribute tab = ReflectionCache.GetAttribute<TabAttribute>(field);
                if (tab == null || tab.Group != group)
                    break;

                members.Add(properties[index]);
                fields.Add(field);
                memberTabs.Add(tab.Name);

                if (!tabOrder.Contains(tab.Name))
                    tabOrder.Add(tab.Name);

                index++;
            }

            return index;
        }

        // The layout width of the enclosing block is not known during the layout pass, so the view width
        // less the current indent is the closest honest estimate.
        // The bar is drawn inside the block, so the width it wraps against is the block's content width.
        // The block has no padding, so that is the view less the indent and the scrollbar.
        private static float AvailableWidth()
            => EditorGUIUtility.currentViewWidth - EditorGUI.indentLevel * IndentStep - ViewPadding;
    }
}