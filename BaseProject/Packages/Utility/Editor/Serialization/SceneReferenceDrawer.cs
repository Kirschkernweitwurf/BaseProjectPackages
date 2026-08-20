using Base.UtilityPackage.Serialization;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Base.UtilityPackage.Editor.Serialization
{
    /// <summary>
    /// Draws a <see cref="SceneReference"/> as an object field restricted to scene assets, and keeps the
    /// cached path, name and build index in sync. Runtime code cannot ask the asset database anything,
    /// so this drawer is the only place those values can be filled.
    /// </summary>
    [CustomPropertyDrawer(typeof(SceneReference), true)]
    public sealed class SceneReferenceDrawer : PropertyDrawer
    {
        private const string NotInBuildMessage = "This scene is not enabled in the build settings, so it "
            + "cannot be loaded by index and will fail to load in a player.";

        private const float WarningHeightLines = 2f;
        private const float WarningSpacing = 2f;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float line = EditorGUIUtility.singleLineHeight;

            return NeedsWarning(property)
                ? line + WarningSpacing + line * WarningHeightLines
                : line;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty asset = property.FindPropertyRelative(SceneReference.AssetField);
            if (asset == null)
            {
                EditorGUI.PropertyField(position, property, label, true);
                return;
            }

            Rect fieldRect = new(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);

            EditorGUI.BeginProperty(fieldRect, label, asset);

            Object assigned = EditorGUI.ObjectField(fieldRect, label, asset.objectReferenceValue,
                typeof(SceneAsset), false);

            if (assigned != asset.objectReferenceValue)
                asset.objectReferenceValue = assigned;

            // Re-synced every frame rather than only on change, because the build settings can change
            // while the same asset stays assigned.
            Sync(property, asset.objectReferenceValue);

            EditorGUI.EndProperty();

            if (!NeedsWarning(property))
                return;

            Rect warningRect = new(position.x, fieldRect.yMax + WarningSpacing, position.width,
                EditorGUIUtility.singleLineHeight * WarningHeightLines);

            EditorGUI.HelpBox(warningRect, NotInBuildMessage, MessageType.Warning);
        }

        private static void Sync(SerializedProperty property, Object asset)
        {
            SerializedProperty path = property.FindPropertyRelative(SceneReference.PathField);
            SerializedProperty sceneName = property.FindPropertyRelative(SceneReference.NameField);
            SerializedProperty buildIndex = property.FindPropertyRelative(SceneReference.BuildIndexField);

            if (path == null || sceneName == null || buildIndex == null)
                return;

            if (asset == null)
            {
                Write(path, string.Empty);
                Write(sceneName, string.Empty);

                if (buildIndex.intValue != SceneReference.NotInBuild)
                    buildIndex.intValue = SceneReference.NotInBuild;

                return;
            }

            string assetPath = AssetDatabase.GetAssetPath(asset);

            Write(path, assetPath);
            Write(sceneName, System.IO.Path.GetFileNameWithoutExtension(assetPath));

            int index = ResolveBuildIndex(assetPath);
            if (buildIndex.intValue != index)
                buildIndex.intValue = index;
        }

        // Assigning an unchanged value would still mark the object dirty, so every write is guarded.
        private static void Write(SerializedProperty property, string value)
        {
            if (property.stringValue != value)
                property.stringValue = value;
        }

        // Disabled scenes are skipped, because the runtime build index only counts enabled ones.
        private static int ResolveBuildIndex(string assetPath)
        {
            int index = 0;

            foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
            {
                if (!scene.enabled)
                    continue;

                if (scene.path == assetPath)
                    return index;

                index++;
            }

            return SceneReference.NotInBuild;
        }

        private static bool NeedsWarning(SerializedProperty property)
        {
            SerializedProperty asset = property.FindPropertyRelative(SceneReference.AssetField);
            SerializedProperty buildIndex = property.FindPropertyRelative(SceneReference.BuildIndexField);

            if (asset == null || buildIndex == null)
                return false;

            return asset.objectReferenceValue != null && buildIndex.intValue == SceneReference.NotInBuild;
        }
    }
}