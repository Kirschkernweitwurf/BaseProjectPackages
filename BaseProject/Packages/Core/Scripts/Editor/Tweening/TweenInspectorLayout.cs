using System;
using System.Reflection;
using Base.CorePackage.Tweening.Core.Data;
using UnityEditor;
using Object = UnityEngine.Object;

namespace Base.CorePackage.Editor.Tweening
{
    /// <summary>
    /// Shared inspector layout for tween components and tween profile assets. It draws the fields
    /// in a fixed order (profile, values, timing) and hides every field that a turned on asset
    /// already provides, so each visible field is one that actually applies.
    /// </summary>
    internal static class TweenInspectorLayout
    {
        private const BindingFlags FieldFlags = BindingFlags.Instance
            | BindingFlags.Public
            | BindingFlags.NonPublic
            | BindingFlags.DeclaredOnly;

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
        /// <param name="serializedObject">The object being inspected.</param>
        public static void Draw(SerializedObject serializedObject)
        {
            serializedObject.Update();

            DrawScript(serializedObject);

            bool usesProfile = DrawToggle(serializedObject, UseProfileField);

            if (usesProfile)
                DrawAsset(serializedObject, ProfileField, ProfileInfo);

            DrawFields(serializedObject, usesProfile);

            if (!usesProfile)
                DrawTiming(serializedObject);

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

        private static void DrawFields(SerializedObject serializedObject, bool usesProfile)
        {
            SerializedProperty property = serializedObject.GetIterator();
            bool enterChildren = true;

            while (property.NextVisible(enterChildren))
            {
                enterChildren = false;

                if (IsLayoutField(property.name))
                    continue;

                if (usesProfile && IsProfileValue(serializedObject.targetObject, property.name))
                    continue;

                EditorGUILayout.PropertyField(property, true);
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

        private static bool IsLayoutField(string propertyName) => propertyName == LoopSettingsField
            || propertyName == ProfileField
            || propertyName == ScriptField
            || propertyName == SettingsAssetField
            || propertyName == TweenSettingsField
            || propertyName == UseProfileField
            || propertyName == UseSettingsAssetField;

        private static bool IsProfileValue(Object target, string propertyName)
        {
            for (Type type = target.GetType(); type != null; type = type.BaseType)
            {
                FieldInfo field = type.GetField(propertyName, FieldFlags);

                if (field != null)
                    return field.IsDefined(typeof(TweenValueAttribute), false);
            }

            return false;
        }
    }
}