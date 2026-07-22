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

        private static readonly Dictionary<string, string[]> TabBars = new();

        /// <summary>
        /// Draws the tab group starting at the given index and returns the index of the first member
        /// after the group.
        /// </summary>
        public static int Draw(List<SerializedProperty> properties, int startIndex, AttributePackageEditor editor)
        {
            Type type = editor.target.GetType();
            string group = ReflectionCache
                .GetAttribute<TabAttribute>(ReflectionCache.GetField(type, properties[startIndex].name))
                .Group;

            List<SerializedProperty> members = new();
            List<FieldInfo> fields = new();
            List<string> memberTabs = new();
            List<string> tabOrder = new();

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

            string selectedTab = DrawTabBar(type, group, ResolveTabBar(type, group, tabOrder));

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            for (int i = 0; i < members.Count; i++)
            {
                if (memberTabs[i] == selectedTab)
                    MemberRenderer.Draw(members[i], fields[i], editor);
            }

            EditorGUILayout.EndVertical();

            return index;
        }

        private static string DrawTabBar(Type type, string group, string[] tabOrder)
        {
            string key = type.FullName + KeySeparator + TabKeyPrefix + KeySeparator + group;
            int stored = Mathf.Clamp(EditorPrefs.GetInt(key, 0), 0, tabOrder.Length - 1);

            int selected = GUILayout.Toolbar(stored, tabOrder);
            if (selected != stored)
                EditorPrefs.SetInt(key, selected);

            return tabOrder[selected];
        }

        private static string[] ResolveTabBar(Type type, string group, List<string> tabOrder)
        {
            string key = type.FullName + KeySeparator + group;
            if (TabBars.TryGetValue(key, out string[] cached) && cached.Length == tabOrder.Count)
                return cached;

            string[] result = tabOrder.ToArray();
            TabBars[key] = result;
            return result;
        }
    }
}