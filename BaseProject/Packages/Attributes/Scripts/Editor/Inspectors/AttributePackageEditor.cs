using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;

namespace Base.AttributePackage.Editor
{
    /// <summary>
    /// Base inspector for the attribute package. Handles the serialized script field, foldout and
    /// collapsible title grouping, then delegates each member to <see cref="MemberRenderer"/> and the
    /// handler pipeline. Tab groups are drawn by <see cref="TabGroupRenderer"/>, read-only native
    /// members and buttons by their renderers. Derive concrete editors targeting MonoBehaviour and
    /// ScriptableObject.
    /// </summary>
    public abstract class AttributePackageEditor : UnityEditor.Editor
    {
        private const string KeySeparator = ".";
        private const string ScriptPropertyPath = "m_Script";

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
            using (new EditorGUI.DisabledScope(true))
                EditorGUILayout.PropertyField(scriptProperty, true);
        }

        private void DrawFields()
        {
            List<SerializedProperty> properties = CollectProperties(out SerializedProperty script);
            if (script != null)
                DrawScriptField(script);

            Type type = target.GetType();

            int index = 0;
            string activeFoldout = null;
            bool activeExpanded = true;
            string activeTitleSection = null;
            bool titleSectionExpanded = true;

            while (index < properties.Count)
            {
                SerializedProperty property = properties[index];
                FieldInfo field = ReflectionCache.GetField(type, property.name);

                if (ReflectionCache.GetAttribute<TabAttribute>(field) != null)
                {
                    activeFoldout = null;
                    activeTitleSection = null;
                    index = TabGroupRenderer.Draw(properties, index, this);
                    continue;
                }

                TitleAttribute title = ReflectionCache.GetAttribute<TitleAttribute>(field);
                if (title != null)
                {
                    if (title.Foldout)
                    {
                        activeTitleSection = title.Title;
                        titleSectionExpanded = TitleRenderer.DrawCollapsible(type, title);
                    }
                    else
                    {
                        activeTitleSection = null;
                        titleSectionExpanded = true;
                    }
                }

                if (activeTitleSection != null && !titleSectionExpanded)
                {
                    index++;
                    continue;
                }

                string foldoutName = ReflectionCache.GetAttribute<FoldoutAttribute>(field)?.Name;

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

                bool sectionIndent = activeTitleSection != null;

                if (foldoutName != null)
                    EditorGUI.indentLevel++;

                if (sectionIndent)
                    EditorGUI.indentLevel++;

                MemberRenderer.Draw(property, field, this);

                if (sectionIndent)
                    EditorGUI.indentLevel--;

                if (foldoutName != null)
                    EditorGUI.indentLevel--;

                index++;
            }
        }

        private List<SerializedProperty> CollectProperties(out SerializedProperty script)
        {
            script = null;
            List<SerializedProperty> properties = new();

            SerializedProperty iterator = serializedObject.GetIterator();
            bool enterChildren = true;

            while (iterator.NextVisible(enterChildren))
            {
                enterChildren = false;

                if (iterator.propertyPath == ScriptPropertyPath)
                    script = iterator.Copy();
                else
                    properties.Add(iterator.Copy());
            }

            return properties;
        }

        private bool DrawFoldoutHeader(string foldoutName)
        {
            string key = target.GetType().FullName + KeySeparator + foldoutName;
            bool stored = EditorPrefs.GetBool(key, true);
            bool expanded = EditorGUILayout.Foldout(stored, foldoutName, true);
            if (expanded != stored)
                EditorPrefs.SetBool(key, expanded);

            return expanded;
        }
    }
}