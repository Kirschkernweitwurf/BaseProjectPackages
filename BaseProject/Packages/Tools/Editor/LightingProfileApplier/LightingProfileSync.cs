using System.IO;
using Base.ToolsPackage.LightingProfileApplier;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Applier = Base.ToolsPackage.LightingProfileApplier.LightingProfileApplier;

namespace Base.ToolsPackage.Editor.LightingProfileApplier
{
    /// <summary>
    /// Keeps the profile of every synced <see cref="Applier"/> equal to its scene's Lighting window. Artists
    /// light and bake as usual and the game picks it up on the next save, with no capture step to forget. A
    /// scene copied together with its applier gets its own profile instead of overwriting the original's.
    /// </summary>
    [InitializeOnLoad]
    public static class LightingProfileSync
    {
        private const string FilePrefix = "LP_";

        static LightingProfileSync()
        {
            EditorSceneManager.sceneSaving += (scene, _) => SyncScene(scene);
            Lightmapping.bakeCompleted += () => SyncScene(SceneManager.GetActiveScene());
        }

        /// <summary>
        /// Captures the lighting of the scene into the profiles of its synced appliers and creates a profile
        /// where one is missing or belongs to another scene. Only the active scene is synced, because the
        /// render settings always show the active scene's lighting.
        /// </summary>
        /// <param name="scene">The scene to sync.</param>
        public static void SyncScene(Scene scene)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode
                || scene != SceneManager.GetActiveScene())
                return;

            // A scene saved for the first time has no GUID yet. Its next save syncs it.
            string sceneGuid = AssetDatabase.AssetPathToGUID(scene.path);

            if (string.IsNullOrEmpty(sceneGuid))
                return;

            foreach (GameObject root in scene.GetRootGameObjects())
            {
                foreach (Applier applier in root.GetComponentsInChildren<Applier>(includeInactive: true))
                {
                    if (applier.IsSyncedWithScene)
                        SyncApplier(applier, scene, sceneGuid);
                }
            }
        }

        /// <summary>
        /// Copies the active scene's lighting into the profile and marks the profile as belonging to it.
        /// </summary>
        /// <param name="profile">The profile to capture into.</param>
        internal static void CaptureActiveScene(LightingProfile profile)
        {
            Undo.RecordObject(profile, "Capture Lighting Profile");
            profile.Capture();

            if (!EditorUtility.IsPersistent(profile.EnvironmentReflection))
                profile.ForgetEnvironmentReflection();

            profile.SceneGuid = AssetDatabase.AssetPathToGUID(SceneManager.GetActiveScene().path);
            EditorUtility.SetDirty(profile);
            AssetDatabase.SaveAssetIfDirty(profile);
        }

        private static void SyncApplier(Applier applier, Scene scene, string sceneGuid)
        {
            LightingProfile profile = applier.Profile;

            if (profile == null
                || IsOwnedByOtherScene(profile, sceneGuid))
                profile = CreateProfile(scene);

            CaptureActiveScene(profile);
            AssignReferences(applier, profile);
        }

        private static bool IsOwnedByOtherScene(LightingProfile profile, string sceneGuid)
            => !string.IsNullOrEmpty(profile.SceneGuid)
                && profile.SceneGuid != sceneGuid;

        private static LightingProfile CreateProfile(Scene scene)
        {
            // Unity keeps a scene's lighting data in a folder named after the scene, so the profile goes there.
            string sceneFolder = Path.GetDirectoryName(scene.path);
            string folder = $"{sceneFolder}/{scene.name}";

            if (!AssetDatabase.IsValidFolder(folder))
                AssetDatabase.CreateFolder(sceneFolder, scene.name);

            LightingProfile profile = ScriptableObject.CreateInstance<LightingProfile>();
            string path = AssetDatabase.GenerateUniqueAssetPath($"{folder}/{FilePrefix}{scene.name}.asset");
            AssetDatabase.CreateAsset(profile, path);

            return profile;
        }

        private static void AssignReferences(Applier applier, LightingProfile profile)
        {
            if (applier.Profile == profile
                && applier.Sun == RenderSettings.sun)
                return;

            Undo.RecordObject(applier, "Sync Lighting Profile");
            applier.Assign(profile, RenderSettings.sun);
            EditorUtility.SetDirty(applier);
        }
    }
}