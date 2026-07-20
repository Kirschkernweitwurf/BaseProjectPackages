#if UNITY_EDITOR
using System;

namespace Base.ToolPackage.Editor.MenuManagerWindow
{
    /// <summary>Live scan result for one entry. Holds the delegates and defaults needed to register a menu.</summary>
    public sealed class ResolvedMenu
    {
        /// <summary>Kind of the entry.</summary>
        public EMenuEntryKind Kind { get; }

        /// <summary>Full default path used when the entry is first discovered.</summary>
        public string DefaultPath { get; }

        /// <summary>Action invoked when a menu item is clicked. Null for asset entries.</summary>
        public Action Execute { get; }

        /// <summary>Optional validate function for a menu item, or null.</summary>
        public Func<bool> Validate { get; }

        /// <summary>ScriptableObject type for an asset entry, or null.</summary>
        public Type AssetType { get; }

        /// <summary>Default asset file name for an asset entry, without extension.</summary>
        public string DefaultFileName { get; }

        /// <summary>Type that declares the entry, used to locate its script on disk.</summary>
        public Type DeclaringType { get; }

        private ResolvedMenu(EMenuEntryKind kind, string defaultPath, Action execute, Func<bool> validate,
            Type assetType, string defaultFileName, Type declaringType)
        {
            Kind = kind;
            DefaultPath = defaultPath;
            Execute = execute;
            Validate = validate;
            AssetType = assetType;
            DefaultFileName = defaultFileName;
            DeclaringType = declaringType;
        }

        /// <summary>Creates a resolved menu item.</summary>
        public static ResolvedMenu MenuItem(string defaultPath, Action execute, Func<bool> validate, Type declaringType)
            => new(EMenuEntryKind.MenuItem, defaultPath, execute, validate, null, string.Empty, declaringType);

        /// <summary>Creates a resolved asset creation entry.</summary>
        public static ResolvedMenu CreateAsset(string defaultPath, Type assetType, string defaultFileName)
            => new(EMenuEntryKind.CreateAsset, defaultPath, null, null, assetType, defaultFileName, assetType);
    }
}
#endif