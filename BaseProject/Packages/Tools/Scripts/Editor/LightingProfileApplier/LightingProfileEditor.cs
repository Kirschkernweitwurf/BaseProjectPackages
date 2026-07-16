using UnityEditor;
using UnityEngine;

namespace Base.LightingProfiles.Editor
{
    /// <summary>
    /// Adds capture and preview buttons to the lighting profile inspector.
    /// </summary>
    [CustomEditor(typeof(LightingProfile))]
    public class LightingProfileEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
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