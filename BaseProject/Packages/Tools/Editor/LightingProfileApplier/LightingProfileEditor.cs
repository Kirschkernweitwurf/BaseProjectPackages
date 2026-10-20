using System.IO;
using Base.AttributesPackage.Editor.Inspectors;
using Base.ToolsPackage.LightingProfileApplier;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

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
        private const string NotBakedMessage = "The reflections of this scene are not baked yet, so the game "
            + "shows none. Bake the lighting once and save the scene.";
        private const string SceneLabel = "Captured From";
        private const string UnknownScene = "Not captured yet";

        /// <inheritdoc/>
        public override void OnInspectorGUI()
        {
            LightingProfile profile = (LightingProfile)target;

            DrawSceneLabel(profile);
            base.OnInspectorGUI();

            if (IsMissingBakedReflection(profile))
                EditorGUILayout.HelpBox(NotBakedMessage, MessageType.Warning);

            EditorGUILayout.Space();

            if (GUILayout.Button("Capture From Open Scene"))
                LightingProfileSync.CaptureActiveScene(profile);

            if (GUILayout.Button("Preview In Open Scene"))
                profile.Apply();
        }

        private static void DrawSceneLabel(LightingProfile profile)
        {
            string scenePath = AssetDatabase.GUIDToAssetPath(profile.SceneGuid);
            string sceneName = string.IsNullOrEmpty(scenePath)
                ? UnknownScene
                : Path.GetFileNameWithoutExtension(scenePath);

            EditorGUILayout.LabelField(SceneLabel, sceneName);
        }

        /// <summary>
        /// A skybox scene that was never baked only has a reflection Unity made on the fly, which the profile
        /// cannot keep.
        /// </summary>
        private static bool IsMissingBakedReflection(LightingProfile profile)
            => profile.HasAmbientProbe
                && profile.EnvironmentReflection == null
                && profile.ReflectionMode == DefaultReflectionMode.Skybox
                && profile.Skybox != null;
    }
}