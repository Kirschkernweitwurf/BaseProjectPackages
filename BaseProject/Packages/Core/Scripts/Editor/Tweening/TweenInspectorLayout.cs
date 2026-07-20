using System;
using System.Reflection;
using Base.AttributePackage;
using Base.AttributePackage.Editor;
using Base.CorePackage.Tweening.Core.Data;
using UnityEditor;

namespace Base.CorePackage.Editor.Tweening
{
    /// <summary>
    /// Shared inspector layout for tween components and tween profile assets. It draws the fields in a
    /// fixed order (profile, values, timing, references) and hides every field that a turned on asset
    /// already provides. Value and reference fields run through the attribute package pipeline, so
    /// attributes like [GetComponent] and [TweenValue] keep working here. Reference fields are always
    /// drawn last, separated by a space.
    /// </summary>
    internal static class TweenInspectorLayout
    {
        private const string LoopSettingsField = "loopSettings";
        private const string MissingAssetWarning = "No asset assigned. The fields below are used instead.";
        private const string ProfileField = "profile";
        private const string ProfileInfo = "Values, timing and loop behavior come from this profile.";
        private const string ScriptField = "m_Script";
        private const string SettingsAssetField = "settingsAsset";
        private const string SettingsAssetInfo = "Timing and loop behavior come from this asset.";
        private const string TweenSettingsField = "tweenSettings";
        private const string UseProfileField = "useProfile";
        private const string UseSettingsAssetField = "useSettingsAsset";

        /// <summary>
        /// Draws the full inspector for the given tween component or tween profile.
        /// </summary>
        /// <param name="editor">The attribute package editor whose object is inspected.</param>
        public static void Draw(AttributePackageEditor editor)
        {
            SerializedObject serializedObject = editor.serializedObject;
            serializedObject.Update();

            DrawScript(serializedObject);

            bool usesProfile = DrawToggle(serializedObject, UseProfileField);

            if (usesProfile)
                DrawAsset(serializedObject, ProfileField, ProfileInfo);

            DrawValueFields(editor, usesProfile);

            if (!usesProfile)
                DrawTiming(serializedObject);

            DrawReferenceFields(editor);

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

        private static bool DrawToggle(SerializedObject serializedObject, string toggleName)
        {
            SerializedProperty toggle = serializedObject.FindProperty(toggleName);

            if (toggle == null)
                return false;

            EditorGUILayout.PropertyField(toggle);

            return !toggle.hasMultipleDifferentValues
                && toggle.boolValue;
        }

        private static void DrawAsset(SerializedObject serializedObject, string assetName, string info)
        {
            SerializedProperty asset = serializedObject.FindProperty(assetName);

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

        private static void DrawValueFields(AttributePackageEditor editor, bool usesProfile)
        {
            SerializedObject serializedObject = editor.serializedObject;
            Type type = serializedObject.targetObject.GetType();
            SerializedProperty property = serializedObject.GetIterator();
            bool enterChildren = true;

            while (property.NextVisible(enterChildren))
            {
                enterChildren = false;

                if (IsLayoutField(property.name))
                    continue;

                if (IsReferenceField(type, property.name))
                    continue;

                if (usesProfile && IsProfileValue(type, property.name))
                    continue;

                DrawMember(editor, property, type);
            }
        }

        private static void DrawReferenceFields(AttributePackageEditor editor)
        {
            SerializedObject serializedObject = editor.serializedObject;
            Type type = serializedObject.targetObject.GetType();
            SerializedProperty property = serializedObject.GetIterator();
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

        private static void DrawTiming(SerializedObject serializedObject)
        {
            if (serializedObject.FindProperty(SettingsAssetField) == null)
                return;

            EditorGUILayout.Space();

            if (DrawToggle(serializedObject, UseSettingsAssetField))
            {
                DrawAsset(serializedObject, SettingsAssetField, SettingsAssetInfo);

                return;
            }

            EditorGUILayout.PropertyField(serializedObject.FindProperty(TweenSettingsField), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty(LoopSettingsField), true);
        }

        private static void DrawMember(AttributePackageEditor editor, SerializedProperty property, Type type)
        {
            FieldInfo field = ReflectionCache.GetField(type, property.name);
            MemberRenderer.Draw(property.Copy(), field, editor);
        }

        private static bool IsLayoutField(string propertyName) => propertyName == LoopSettingsField
            || propertyName == ProfileField
            || propertyName == ScriptField
            || propertyName == SettingsAssetField
            || propertyName == TweenSettingsField
            || propertyName == UseProfileField
            || propertyName == UseSettingsAssetField;

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