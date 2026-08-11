using System;
using System.Collections.Generic;
using System.Reflection;
using Base.UtilityPackage.Editor;
using UnityEditor;
using UnityEngine;

namespace Base.AttributePackage.Editor
{
    /// <summary>
    /// Base inspector for the attribute package. Handles the serialized script field, foldout and
    /// collapsible title grouping, then delegates each member to <see cref="MemberRenderer"/> and the
    /// handler pipeline. Tab groups are drawn by <see cref="TabGroupRenderer"/>, read-only native
    /// members and buttons by their renderers. Header buttons are not drawn from here: Unity does not
    /// call OnHeaderGUI for component editors, so <see cref="HeaderItemInjector"/> registers them with
    /// the header itself. Derive concrete editors targeting MonoBehaviour and ScriptableObject.
    /// </summary>
    public abstract class AttributePackageEditor : UnityEditor.Editor
    {
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

                // The title is opened before the tab check, so a field that carries both still gets its
                // section header and the tab group ends up inside it rather than beside it.
                DrawTitle(type, field);

                if (ReflectionCache.GetAttribute<TabAttribute>(field) != null)
                {
                    index = DrawTabGroup(properties, index, type);
                    continue;
                }

                index++;

                if (_inTitleSection && !_titleExpanded)
                    continue;

                if (!UpdateFoldout(field))
                    continue;

                DrawIndented(property, field);
            }
        }

        // A tab group ends any foldout run above it but stays inside the enclosing title section, so a
        // collapsed section hides its tabs instead of leaving them floating on their own.
        private int DrawTabGroup(List<SerializedProperty> properties, int index, Type type)
        {
            _activeFoldout = null;

            if (_inTitleSection && !_titleExpanded)
                return TabGroupRenderer.Skip(properties, index, type);

            int indent = _inTitleSection
                ? 1
                : 0;

            EditorGUI.indentLevel += indent;
            int next = TabGroupRenderer.Draw(properties, index, this);
            EditorGUI.indentLevel -= indent;

            return next;
        }

        // Called once before the field loop. A tab group no longer resets it, because that is what made
        // tabs escape the title section they were declared in.
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

                if (iterator.propertyPath == EditorConstants.ScriptPropertyName)
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