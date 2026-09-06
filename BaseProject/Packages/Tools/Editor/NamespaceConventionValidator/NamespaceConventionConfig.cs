using System.Collections.Generic;
using Base.AttributesPackage;
using Base.UtilityPackage.Menus;
using UnityEngine;

namespace Base.ToolsPackage.Editor.NamespaceConventionValidator
{
    /// <summary>
    /// Rules the namespaces below a folder have to follow. Read by the
    /// <see cref="NamespaceConventionScanner"/>, edited as a project asset.
    /// </summary>
    /// <remarks>
    /// The root is the point the namespaces are measured from, which is what lets the same tool check
    /// a game's own scripts and a package. Pointing it at <c>Assets/Scripts</c> with a root namespace
    /// of <c>Game</c> makes <c>Assets/Scripts/Player/Health.cs</c> read as <c>Game.Player</c>, and the
    /// folder the scripts happen to live in stops leaking into every namespace.
    /// </remarks>
    [DynamicCreateAssetMenu("Scriptable Objects/Base/Namespace Convention/New Config", "NCC_NamespaceConventions")]
    internal sealed class NamespaceConventionConfig : ScriptableObject
    {
        private const string DefaultRoot = "Assets";

        /// <summary>Folder the scan starts at. Namespaces are measured from here down.</summary>
        [field: Title("Scope")]
        [field: Tooltip("Folder the scan starts at. Namespaces are measured from here down.")]
        [field: NotNullOrEmpty]
        [field: FolderPath]
        [field: SerializeField]
        public string RootFolder { get; private set; } = DefaultRoot;

        /// <summary>Folder names that are skipped, including everything inside them.</summary>
        [field: Tooltip("Folder names that are skipped, including everything inside them.")]
        [field: Unique]
        [field: SerializeField]
        public List<string> IgnoredFolders { get; private set; } = new()
        {
            "Plugins",
            "StreamingAssets",
            "ThirdParty"
        };

        /// <summary>File names that are skipped, for files that hold no type to place.</summary>
        [field: Tooltip("File names that are skipped, for files that hold no type to place.")]
        [field: Unique]
        [field: SerializeField]
        public List<string> IgnoredFileNames { get; private set; } = new()
        {
            "AssemblyInfo.cs"
        };

        /// <summary>
        /// Prefix put in front of the folder path for scripts no assembly definition owns. Leave it
        /// empty to measure from the root folder alone.
        /// </summary>
        [field: Title("Naming")]
        [field: Tooltip("Prefix for scripts no assembly definition owns. Empty measures from the root alone.")]
        [field: SerializeField]
        public string RootNamespace { get; private set; } = string.Empty;

        /// <summary>
        /// Allows a namespace that stops short of its folder, which is how a package is flattened so a
        /// consumer writes one using instead of six.
        /// </summary>
        [field: Tooltip("Allows a namespace that stops short of its folder, which is how a package is flattened.")]
        [field: SerializeField]
        public bool AllowShorterNamespace { get; private set; } = true;

        /// <summary>Reports a file that declares no namespace at all, so its types land in the global one.</summary>
        [field: Title("Reporting")]
        [field: Tooltip("Reports a file that declares no namespace at all.")]
        [field: SerializeField]
        public bool RequireNamespace { get; private set; } = true;

        /// <summary>Skips files written by a generator, since their namespace is not ours to decide.</summary>
        [field: Tooltip("Skips files written by a generator, since their namespace is not ours to decide.")]
        [field: SerializeField]
        public bool IgnoreGeneratedFiles { get; private set; } = true;
    }
}