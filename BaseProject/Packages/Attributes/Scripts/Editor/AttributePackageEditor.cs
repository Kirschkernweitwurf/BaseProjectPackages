using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Base.AttributePackage.Editor
{
    /// <summary>
    /// Base inspector for the attribute package. Handles the serialized script field, foldout and
    /// tab grouping, then delegates each member to <see cref="MemberRenderer"/> and the handler
    /// pipeline. Read-only native members and buttons are drawn by their renderers.
    /// Derive concrete editors targeting MonoBehaviour and ScriptableObject.
    /// </summary>
    public abstract class AttributePackageEditor : UnityEditor.Editor
    {
        private const string KeySeparator = ".";
        private const string ScriptPropertyPath = "m_Script";
        private const string TabKeyPrefix = "TAB";

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawFields();
            serializedObject.ApplyModifiedProperties();
            NativeMemberRenderer.Draw(this);
            ButtonRenderer.Draw(this);
        }

        private static void DrawScriptField(SerializedProperty scriptProperty)
        {
            bool previousState = GUI.enabled;
            GUI.enabled = false;
            EditorGUILayout.PropertyField(scriptProperty, true);
            GUI.enabled = previousState;
        }

        private void DrawFields()
        {
            List<SerializedProperty> properties = CollectProperties();
            Type type = target.GetType();

            int index = 0;
            string activeFoldout = null;
            bool activeExpanded = true;

            while (index < properties.Count)
            {
                SerializedProperty property = properties[index];
                FieldInfo field = ReflectionCache.GetField(type, property.name);

                if (field?.GetCustomAttribute<TabAttribute>() != null)
                {
                    activeFoldout = null;
                    index = DrawTabGroup(properties, index);
                    continue;
                }

                string foldoutName = field?.GetCustomAttribute<FoldoutAttribute>()?.Name;

                if (foldoutName != activeFoldout)
                {
                    activeFoldout = foldoutName;
                    if (foldoutName != null)
                        activeExpanded = DrawFoldoutHeader(foldoutName);
                }

                if (foldoutName != null && !activeExpanded)
                {
                    index++;
                    continue;
                }

                if (foldoutName != null)
                    EditorGUI.indentLevel++;

                MemberRenderer.Draw(property, field, this);

                if (foldoutName != null)
                    EditorGUI.indentLevel--;

                index++;
            }
        }

        private List<SerializedProperty> CollectProperties()
        {
            List<SerializedProperty> properties = new();
            SerializedProperty iterator = serializedObject.GetIterator();
            bool enterChildren = true;

            while (iterator.NextVisible(enterChildren))
            {
                enterChildren = false;

                if (iterator.propertyPath == ScriptPropertyPath)
                {
                    DrawScriptField(iterator);
                    continue;
                }

                properties.Add(iterator.Copy());
            }

            return properties;
        }

        private int DrawTabGroup(List<SerializedProperty> properties, int startIndex)
        {
            Type type = target.GetType();
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

            string key = type.FullName + KeySeparator + TabKeyPrefix + KeySeparator + group;
            int stored = Mathf.Clamp(EditorPrefs.GetInt(key, 0), 0, tabOrder.Count - 1);

            int selected = GUILayout.Toolbar(stored, tabOrder.ToArray());
            if (selected != stored)
                EditorPrefs.SetInt(key, selected);

            string selectedTab = tabOrder[selected];

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            for (int i = 0; i < members.Count; i++)
            {
                if (memberTabs[i] == selectedTab)
                    MemberRenderer.Draw(members[i], fields[i], this);
            }

            EditorGUILayout.EndVertical();

            return index;
        }

        private bool DrawFoldoutHeader(string foldoutName)
        {
            string key = target.GetType().FullName + KeySeparator + foldoutName;
            bool expanded = EditorGUILayout.Foldout(EditorPrefs.GetBool(key, true), foldoutName, true);
            EditorPrefs.SetBool(key, expanded);
            return expanded;
        }
    }
}