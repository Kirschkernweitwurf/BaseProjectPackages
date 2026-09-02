using Base.AttributesPackage.Editor.Inspectors;
using Base.ToolsPackage.LightingProfileApplier;
using UnityEditor;
using UnityEngine;

namespace Base.ToolsPackage.Editor.LightingProfileApplier
{
    /// <summary>
    /// Adds capture and preview buttons to the lighting profile inspector. Derives from
    /// <see cref="AttributesPackageEditor"/> so the attribute handler pipeline (ShowIf and friends)
    /// stays active for the profile fields.
    /// </summary>
    [CustomEditor(typeof(LightingProfile))]
    internal class LightingProfileEditor : AttributesPackageEditor
    {
        /// <inheritdoc/>
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EditorGUILayout.Space();

            LightingProfile profile = (LightingProfile)target;

            if (GUILayout.Button("Capture From Open Scene"))
                CaptureFrom(profile);

            if (GUILayout.Button("Preview In Open Scene"))
                profile.Apply();
        }

        private static void CaptureFrom(LightingProfile profile)
        {
            Undo.RecordObject(profile, "Capture Lighting Profile");
            profile.Capture();
            EditorUtility.SetDirty(profile);
            AssetDatabase.SaveAssetIfDirty(profile);
        }
    }
}