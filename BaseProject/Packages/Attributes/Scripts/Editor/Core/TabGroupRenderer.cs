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
    /// <see cref="EditorPrefs"/>.
    /// </summary>
    public static class TabGroupRenderer
    {
        private const string KeySeparator = ".";
        private const string TabKeyPrefix = "TAB";

        /// <summary>
        /// Draws the tab group starting at the given index and returns the index of the first member
        /// after the group.
        /// </summary>
        public static int Draw(List<SerializedProperty> properties, int startIndex, AttributePackageEditor editor)
        {
            Type type = editor.target.GetType();
            string group = ReflectionCache.GetField(type, properties[startIndex].name)
                .GetCustomAttribute<TabAttribute>()
                .Group;

            List<SerializedProperty> members = new();
            List<FieldInfo> fields = new();
            List<string> memberTabs = new();
            List<string> tabOrder = new();

            int index = startIndex;
            while (index < properties.Count)
            {
                FieldInfo field = ReflectionCache.GetField(type, properties[index].name);
                TabAttribute tab = field?.GetCustomAttribute<TabAttribute>();
                if (tab == null || tab.Group != group)
                    break;

                members.Add(properties[index]);
                fields.Add(field);
                memberTabs.Add(tab.Name);
                if (!tabOrder.Contains(tab.Name))
                    tabOrder.Add(tab.Name);

                index++;
            }

            string selectedTab = DrawTabBar(type, group, tabOrder);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            for (int i = 0; i < members.Count; i++)
            {
                if (memberTabs[i] == selectedTab)
                    MemberRenderer.Draw(members[i], fields[i], editor);
            }

            EditorGUILayout.EndVertical();

            return index;
        }

        private static string DrawTabBar(Type type, string group, List<string> tabOrder)
        {
            string key = type.FullName + KeySeparator + TabKeyPrefix + KeySeparator + group;
            int stored = Mathf.Clamp(EditorPrefs.GetInt(key, 0), 0, tabOrder.Count - 1);

            int selected = GUILayout.Toolbar(stored, tabOrder.ToArray());
            if (selected != stored)
                EditorPrefs.SetInt(key, selected);

            return tabOrder[selected];
        }
    }
}