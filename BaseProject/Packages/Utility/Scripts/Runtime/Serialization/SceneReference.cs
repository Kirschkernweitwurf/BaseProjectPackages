using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Base.UtilityPackage.Serialization
{
    /// <summary>
    /// References a scene by asset instead of by name. Moving or renaming the scene file keeps the
    /// reference intact, which a plain name string cannot do. The path, name and build index are cached
    /// alongside the asset so runtime code never needs the editor-only scene asset type.
    /// </summary>
    /// <remarks>
    /// The cached values are written by the inspector drawer, which is the only place that can ask the
    /// asset database anything. A scene added to the build settings after the field was last touched
    /// keeps a stale build index until the inspector draws it again, so prefer <see cref="Path"/> or
    /// <see cref="Name"/> for loading and treat <see cref="BuildIndex"/> as a convenience.
    /// </remarks>
    [Serializable]
    public sealed class SceneReference
    {
        /// <summary>Name of the serialized asset field. Used by the inspector drawer.</summary>
        public const string AssetField = nameof(sceneAsset);

        /// <summary>Name of the serialized build index field. Used by the inspector drawer.</summary>
        public const string BuildIndexField = nameof(buildIndex);

        /// <summary>Name of the serialized name field. Used by the inspector drawer.</summary>
        public const string NameField = nameof(sceneName);

        /// <summary>Name of the serialized path field. Used by the inspector drawer.</summary>
        public const string PathField = nameof(scenePath);

        /// <summary>Build index used when the scene is not in the build settings.</summary>
        public const int NotInBuild = -1;

        // Typed as Object rather than SceneAsset because SceneAsset lives in UnityEditor and cannot be
        // named from a runtime assembly. The drawer restricts what can be dropped in.
        [SerializeField] private Object sceneAsset;
        [SerializeField] private string scenePath;
        [SerializeField] private string sceneName;
        [SerializeField] private int buildIndex = NotInBuild;

        /// <summary>Project-relative path of the scene, for use with the scene manager.</summary>
        public string Path => scenePath;

        /// <summary>File name of the scene without the extension.</summary>
        public string Name => sceneName;

        /// <summary>Index in the build settings, or <see cref="NotInBuild"/>.</summary>
        public int BuildIndex => buildIndex;

        /// <summary>True when the scene is listed and enabled in the build settings.</summary>
        public bool IsInBuild => buildIndex >= 0;

        /// <summary>True when a scene asset is assigned.</summary>
        public bool IsAssigned => !string.IsNullOrEmpty(scenePath);

        /// <summary>Returns the scene name, for logs and inspectors.</summary>
        /// <returns>The scene name, or an empty string.</returns>
        public override string ToString() => sceneName ?? string.Empty;
    }
}
