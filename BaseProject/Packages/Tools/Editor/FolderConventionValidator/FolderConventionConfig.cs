using System.Collections.Generic;
using Base.AttributesPackage;
using Base.UtilityPackage.Menus;
using UnityEngine;

namespace Base.ToolsPackage.Editor.FolderConventionValidator
{
    /// <summary>
    /// Rules the project folder layout has to follow. Read by the
    /// <see cref="FolderConventionScanner"/>, edited as a project asset.
    /// </summary>
    [DynamicCreateAssetMenu("Scriptable Objects/Base/Folder Convention/New Config", "FCC_FolderConventions")]
    internal sealed class FolderConventionConfig : ScriptableObject
    {
        private const int DefaultMaxDepth = 6;
        private const string DefaultRoot = "Assets";
        private const int MaxAllowedDepth = 32;
        private const int MinAllowedDepth = 1;

        /// <summary>Folder the scan starts at. Everything below it is validated.</summary>
        [field: Title("Scope")]
        [field: Tooltip("Folder the scan starts at. Everything below it is validated.")]
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

        /// <summary>Style every folder name has to match.</summary>
        [field: Title("Naming")]
        [field: Tooltip("Style every folder name has to match.")]
        [field: SerializeField]
        public EFolderNamingStyle NamingStyle { get; private set; } = EFolderNamingStyle.PascalCase;

        /// <summary>Folder names that may break the naming style, for example a project root.</summary>
        [field: Tooltip("Folder names that may break the naming style, for example a project root.")]
        [field: Unique]
        [field: SerializeField]
        public List<string> AllowedNameExceptions { get; private set; } = new()
        {
            "_Project"
        };

        /// <summary>Folder names that are never allowed, whatever the naming style says.</summary>
        [field: Tooltip("Folder names that are never allowed, whatever the naming style says.")]
        [field: Unique]
        [field: SerializeField]
        public List<string> ForbiddenNames { get; private set; } = new()
        {
            "New Folder",
            "Temp",
            "Test"
        };

        /// <summary>How many folder levels are allowed below the root.</summary>
        [field: Title("Structure")]
        [field: Tooltip("How many folder levels are allowed below the root.")]
        [field: MinMax(MinAllowedDepth, MaxAllowedDepth)]
        [field: SerializeField]
        public int MaxDepth { get; private set; } = DefaultMaxDepth;

        /// <summary>Folders that have to exist. Missing ones can be created from the window.</summary>
        [field: Tooltip("Folders that have to exist. Missing ones can be created from the window.")]
        [field: Unique]
        [field: SerializeField]
        public List<string> RequiredFolders { get; private set; } = new();

        /// <summary>Allows assets to sit directly in the root folder instead of a subfolder.</summary>
        [field: Tooltip("Allows assets to sit directly in the root folder instead of a subfolder.")]
        [field: SerializeField]
        public bool AllowLooseAssetsInRoot { get; private set; }
    }
}