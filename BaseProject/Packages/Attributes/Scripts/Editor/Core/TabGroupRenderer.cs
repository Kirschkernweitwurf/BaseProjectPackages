using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Base.AttributePackage.Editor
{
    /// <summary>
    /// Draws a run of consecutive <see cref="TabAttribute"/> members as a tab bar with the selected
    /// tab's members below it. The selection is stored per owner type and group in
    /// <see cref="EditorPrefs"/>, as is the open state when the group asks for a foldout.
    /// </summary>
    internal static class TabGroupRenderer
    {
        private const string FoldoutKeyPrefix = "TABFOLD";
        private const float GroupSpacing = 6f;
        private const float IndentStep = 15f;
        private const string TabKeyPrefix = "TAB";
        private const float ViewPadding = 24f;

        private static GUIStyle FoldoutStyle => _foldoutStyle ??= new GUIStyle(EditorStyles.foldout)
        {
            fontStyle = FontStyle.Bold
        };

        private static GUIStyle _foldoutStyle;

        /// <summary>
        /// Draws the tab group starting at the given index and returns the index of the first member
        /// after the group.
        /// </summary>
        /// <param name="properties">All properties of the inspected object.</param>
        /// <param name="startIndex">Index of the first member of the group.</param>
        /// <param name="editor">The editor drawing the group.</param>
        /// <returns>The index of the first member after the group.</returns>
        public static int Draw(List<SerializedProperty> properties, int startIndex, AttributePackageEditor editor)
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

            string selectedTab = DrawTabBar(type, group, tabOrder.ToArray());

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            for (int i = 0; i < members.Count; i++)
            {
                if (memberTabs[i] == selectedTab)
                    MemberRenderer.Draw(members[i], fields[i], editor);
            }

            EditorGUILayout.EndVertical();
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
            => ReflectionCache.GetAttribute<TabAttribute>(
                ReflectionCache.GetField(type, properties[index].name));

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

        private static string DrawTabBar(Type type, string group, string[] tabOrder)
        {
            string key = StateKey.For(type, TabKeyPrefix, group);
            int stored = Mathf.Clamp(EditorPrefs.GetInt(key, 0), 0, tabOrder.Length - 1);

            // The bar wraps rather than truncating, because a narrow inspector otherwise turns a row of
            // readable tabs into a row of stubs that cannot be told apart.
            int selected = WrappedToolbar.Draw(stored, tabOrder, AvailableWidth());
            if (selected != stored)
                EditorPrefs.SetInt(key, selected);

            return tabOrder[selected];
        }

        // The layout width of the enclosing block is not known during the layout pass, so the view width
        // less the current indent is the closest honest estimate.
        private static float AvailableWidth()
            => EditorGUIUtility.currentViewWidth - EditorGUI.indentLevel * IndentStep - ViewPadding;
    }
}