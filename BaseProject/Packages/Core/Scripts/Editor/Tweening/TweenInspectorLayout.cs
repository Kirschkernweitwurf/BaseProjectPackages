using System;
using System.Reflection;
using Base.AttributePackage;
using Base.AttributePackage.Editor;
using Base.CorePackage.Tweening.Core.Data;
using Base.CorePackage.Tweening.Core.Data.Profiles;
using UnityEditor;

namespace Base.CorePackage.Editor.Tweening
{
    /// <summary>
    /// Shared inspector layout for tween components and tween profile assets. It draws the fields in a
    /// fixed order (profile, values, timing, references) and hides every field that a turned on asset
    /// already provides. Value and reference fields run through the attribute package pipeline, so
    /// attributes like <see cref="GetComponentAttribute"/> and <see cref="TweenValueAttribute"/> keep
    /// working here. Reference fields are always drawn last, separated by a space.
    /// </summary>
    /// <remarks>
    /// Layout fields are recognized by what they are, not by what they are called: the asset and
    /// settings fields by their type, the two toggles by <see cref="TweenProfileToggleAttribute"/> and
    /// <see cref="TweenSettingsToggleAttribute"/>. Renaming a serialized field therefore cannot break
    /// this layout.
    /// </remarks>
    internal static class TweenInspectorLayout
    {
        private const string MissingAssetWarning = "No asset assigned. The fields below are used instead.";
        private const string ProfileInfo = "Values, timing and loop behavior come from this profile.";

        /// <summary>
        /// Unity's built-in script reference. Not one of our own members, so there is nothing to
        /// derive this name from.
        /// </summary>
        private const string ScriptField = "m_Script";
        private const string SettingsAssetInfo = "Timing and loop behavior come from this asset.";

        /// <summary>
        /// Draws the full inspector for the given tween component or tween profile.
        /// </summary>
        /// <param name="editor">The attribute package editor whose object is inspected.</param>
        public static void Draw(AttributePackageEditor editor)
        {
            SerializedObject serializedObject = editor.serializedObject;
            Type type = serializedObject.targetObject.GetType();

            serializedObject.Update();

            DrawScript(serializedObject);

            bool usesProfile = DrawToggle(FindByRole(serializedObject, type, IsProfileToggle));

            if (usesProfile)
                DrawAsset(FindByRole(serializedObject, type, IsProfileAsset), ProfileInfo);

            DrawValueFields(editor, type, usesProfile);

            if (!usesProfile)
                DrawTiming(serializedObject, type);

            DrawReferenceFields(editor, type);

            serializedObject.ApplyModifiedProperties();
        }

        private static void DrawScript(SerializedObject serializedObject)
        {
            SerializedProperty script = serializedObject.FindProperty(ScriptField);

            if (script == null)
                return;

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.PropertyField(script);
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.Space();
        }

        private static bool DrawToggle(SerializedProperty toggle)
        {
            if (toggle == null)
                return false;

            EditorGUILayout.PropertyField(toggle);

            return !toggle.hasMultipleDifferentValues
                && toggle.boolValue;
        }

        private static void DrawAsset(SerializedProperty asset, string info)
        {
            if (asset == null)
                return;

            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(asset);
            EditorGUI.indentLevel--;

            if (asset.hasMultipleDifferentValues)
                return;

            if (asset.objectReferenceValue == null)
            {
                EditorGUILayout.HelpBox(MissingAssetWarning, MessageType.Warning);

                return;
            }

            EditorGUILayout.HelpBox(info, MessageType.None);
        }

        private static void DrawValueFields(AttributePackageEditor editor, Type type, bool usesProfile)
        {
            SerializedProperty property = editor.serializedObject.GetIterator();
            bool enterChildren = true;

            while (property.NextVisible(enterChildren))
            {
                enterChildren = false;

                if (IsLayoutField(type, property.name))
                    continue;

                if (IsReferenceField(type, property.name))
                    continue;

                if (usesProfile
                    && IsProfileValue(type, property.name))
                    continue;

                DrawMember(editor, property, type);
            }
        }

        private static void DrawReferenceFields(AttributePackageEditor editor, Type type)
        {
            SerializedProperty property = editor.serializedObject.GetIterator();
            bool enterChildren = true;
            bool drewSpace = false;

            while (property.NextVisible(enterChildren))
            {
                enterChildren = false;

                if (!IsReferenceField(type, property.name))
                    continue;

                if (!drewSpace)
                {
                    EditorGUILayout.Space();
                    drewSpace = true;
                }

                DrawMember(editor, property, type);
            }
        }

        private static void DrawTiming(SerializedObject serializedObject, Type type)
        {
            SerializedProperty settingsAsset = FindByRole(serializedObject, type, IsSettingsAsset);

            // Profiles and components both reach this point, but only they carry a settings asset.
            if (settingsAsset == null)
                return;

            EditorGUILayout.Space();

            if (DrawToggle(FindByRole(serializedObject, type, IsSettingsToggle)))
            {
                DrawAsset(settingsAsset, SettingsAssetInfo);

                return;
            }

            DrawExpanded(FindByRole(serializedObject, type, IsSettings));
            DrawExpanded(FindByRole(serializedObject, type, IsLoopSettings));
        }

        private static void DrawExpanded(SerializedProperty property)
        {
            if (property == null)
                return;

            EditorGUILayout.PropertyField(property, true);
        }

        private static void DrawMember(AttributePackageEditor editor, SerializedProperty property, Type type)
        {
            FieldInfo field = ReflectionCache.GetField(type, property.name);
            MemberRenderer.Draw(property.Copy(), field, editor);
        }

        /// <summary>
        /// Returns the first serialized property whose backing field matches the given role, or
        /// <c>null</c> when the inspected object has no such field.
        /// </summary>
        private static SerializedProperty FindByRole(SerializedObject serializedObject, Type type,
            Func<FieldInfo, bool> hasRole)
        {
            SerializedProperty property = serializedObject.GetIterator();
            bool enterChildren = true;

            while (property.NextVisible(enterChildren))
            {
                enterChildren = false;

                FieldInfo field = ReflectionCache.GetField(type, property.name);

                if (field != null
                    && hasRole(field))
                    return property.Copy();
            }

            return null;
        }

        private static bool IsLayoutField(Type type, string propertyName)
        {
            if (propertyName == ScriptField)
                return true;

            FieldInfo field = ReflectionCache.GetField(type, propertyName);

            if (field == null)
                return false;

            return IsProfileToggle(field)
                || IsProfileAsset(field)
                || IsSettingsToggle(field)
                || IsSettingsAsset(field)
                || IsSettings(field)
                || IsLoopSettings(field);
        }

        private static bool IsProfileToggle(FieldInfo field)
            => field.IsDefined(typeof(TweenProfileToggleAttribute), false);

        private static bool IsSettingsToggle(FieldInfo field)
            => field.IsDefined(typeof(TweenSettingsToggleAttribute), false);

        private static bool IsProfileAsset(FieldInfo field) => typeof(TweenProfileSo).IsAssignableFrom(field.FieldType);

        private static bool IsSettingsAsset(FieldInfo field) => field.FieldType == typeof(TweenSettingsSo);

        private static bool IsSettings(FieldInfo field) => field.FieldType == typeof(TweenSettings);

        private static bool IsLoopSettings(FieldInfo field) => field.FieldType == typeof(LoopSettings);

        private static bool IsReferenceField(Type type, string propertyName)
        {
            FieldInfo field = ReflectionCache.GetField(type, propertyName);

            if (field == null)
                return false;

            return field.IsDefined(typeof(GetComponentAttribute), false)
                || field.IsDefined(typeof(GetComponentInParentAttribute), false);
        }

        private static bool IsProfileValue(Type type, string propertyName)
        {
            FieldInfo field = ReflectionCache.GetField(type, propertyName);
            return field?.IsDefined(typeof(TweenValueAttribute), false) ?? false;
        }
    }
}