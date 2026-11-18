using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Base.TextPackage.Editor
{
    /// <summary>
    /// Where a text box lives in the project: the prefab or scene that holds it and its path in there. A box inside
    /// a nested prefab points at that prefab, since that is where its size is set.
    /// </summary>
    public sealed class TextBoxLocation
    {
        private const string KeySeparator = ":";
        private const string OpenSceneCancel = "Cancel";
        private const string OpenSceneFormat = "The text box lives in the scene '{0}'. Open it?";
        private const string OpenSceneOk = "Open Scene";
        private const string OpenSceneTitle = "Show Text Box";
        private const char PathSeparator = '/';

        /// <summary>Creates the location.</summary>
        /// <param name="asset">The prefab or scene asset that holds the box.</param>
        /// <param name="path">The path of the box in there, from its root object, like "Menu/Content/Title".</param>
        internal TextBoxLocation(Object asset, string path)
        {
            Asset = asset;
            Path = path;
        }

        /// <summary>The prefab or scene asset that holds the box.</summary>
        public Object Asset { get; }

        /// <summary>The path of the box from the root object of <see cref="Asset"/>, like "Menu/Title".</summary>
        public string Path { get; }

        /// <summary>
        /// The asset, the objects above the box and the box itself, like "P_Menu", "Content", "Title".
        /// </summary>
        internal string[] Crumbs
        {
            get
            {
                // A prefab's root carries the prefab's name, so it would show up twice.
                int skipped = Asset is GameObject
                    ? 1
                    : 0;

                return Parts().Skip(skipped).Prepend(Asset.name).ToArray();
            }
        }

        /// <summary>A key that stays the same while the asset moves, for remembering a box between checks.</summary>
        internal string Key => AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(Asset)) + KeySeparator + Path;

        /// <summary>The name of the box object.</summary>
        internal string Name => Parts().Last();

        /// <summary>
        /// Opens the prefab or scene and selects the box, so it can be resized right away. A scene that is not open
        /// asks first, because opening it closes the scenes open now. Returns the box or null when it was not
        /// opened.
        /// </summary>
        internal GameObject Show()
        {
            if (Asset is SceneAsset)
                return ShowInScene();

            PrefabStage stage = PrefabStageUtility.OpenPrefab(AssetDatabase.GetAssetPath(Asset));

            if (stage == null)
                return null;

            return Select(stage.prefabContentsRoot.transform, string.Join(PathSeparator, Parts().Skip(1)));
        }

        private static GameObject Select(Transform root, string relativePath)
        {
            Transform target = string.IsNullOrEmpty(relativePath)
                ? root
                : root.Find(relativePath);

            if (target == null)
                return null;

            Selection.activeGameObject = target.gameObject;
            EditorGUIUtility.PingObject(target.gameObject);

            return target.gameObject;
        }

        private string[] Parts() => Path.Split(PathSeparator);

        private GameObject ShowInScene()
        {
            string assetPath = AssetDatabase.GetAssetPath(Asset);
            Scene scene = SceneManager.GetSceneByPath(assetPath);

            if (!scene.isLoaded)
            {
                if (!EditorUtility.DisplayDialog(OpenSceneTitle, string.Format(OpenSceneFormat, Asset.name),
                        OpenSceneOk, OpenSceneCancel)
                    || !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                {
                    EditorGUIUtility.PingObject(Asset);
                    return null;
                }

                scene = EditorSceneManager.OpenScene(assetPath);
            }

            // A prefab open in prefab mode hides the scene, so the selection would not show.
            StageUtility.GoToMainStage();

            string[] parts = Parts();
            GameObject root = scene.GetRootGameObjects().FirstOrDefault(candidate => candidate.name == parts[0]);

            if (root == null)
                return null;

            return Select(root.transform, string.Join(PathSeparator, parts.Skip(1)));
        }
    }
}