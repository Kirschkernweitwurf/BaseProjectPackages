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
        private const string ScriptPropertyPath = "m_Script";

        private string _activeFoldout;
        private bool _foldoutExpanded = true;
        private bool _inTitleSection;
        private bool _titleExpanded = true;

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
            ResetGroupState();

            int index = 0;
            while (index < properties.Count)
            {
                SerializedProperty property = properties[index];
                FieldInfo field = ReflectionCache.GetField(type, property.name);

                if (ReflectionCache.GetAttribute<TabAttribute>(field) != null)
                {
                    ResetGroupState();
                    index = TabGroupRenderer.Draw(properties, index, this);
                    continue;
                }

                index++;
                DrawTitle(type, field);

                if (_inTitleSection && !_titleExpanded)
                    continue;

                if (!UpdateFoldout(field))
                    continue;

                DrawIndented(property, field);
            }
        }

        private void ResetGroupState()
        {
            _activeFoldout = null;
            _inTitleSection = false;
            _titleExpanded = true;
        }

        // A titled field opens a new section. Only a collapsible title can fold the fields below it.
        private void DrawTitle(Type type, FieldInfo field)
        {
            TitleAttribute title = ReflectionCache.GetAttribute<TitleAttribute>(field);
            if (title == null)
                return;

            _inTitleSection = title.Foldout;
            _titleExpanded = true;

            if (_inTitleSection)
                _titleExpanded = TitleRenderer.DrawCollapsible(type, title);
        }

        // Returns false while the field belongs to a collapsed foldout.
        private bool UpdateFoldout(FieldInfo field)
        {
            string foldoutName = ReflectionCache.GetAttribute<FoldoutAttribute>(field)?.Name;

            if (foldoutName != _activeFoldout)
            {
                _activeFoldout = foldoutName;

                if (foldoutName != null)
                    _foldoutExpanded = DrawFoldoutHeader(foldoutName);
            }

            return foldoutName == null || _foldoutExpanded;
        }

        private void DrawIndented(SerializedProperty property, FieldInfo field)
        {
            int indent = 0;

            if (_activeFoldout != null)
                indent++;

            if (_inTitleSection)
                indent++;

            EditorGUI.indentLevel += indent;
            MemberRenderer.Draw(property, field, this);
            EditorGUI.indentLevel -= indent;
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
            string key = StateKey.For(target.GetType(), foldoutName);
            bool stored = EditorPrefs.GetBool(key, true);
            bool expanded = EditorGUILayout.Foldout(stored, foldoutName, true);
            if (expanded != stored)
                EditorPrefs.SetBool(key, expanded);

            return expanded;
        }
    }
}