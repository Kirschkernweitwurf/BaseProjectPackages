using System;

namespace Base.ToolsPackage.Editor.MenuManagerModel
{
    /// <summary>Live scan result for one entry. Holds the delegates and defaults needed to register a menu.</summary>
    internal sealed class ResolvedMenu
    {
        /// <summary>Kind of the entry.</summary>
        internal EMenuEntryKind Kind { get; }

        /// <summary>Full default path used when the entry is first discovered.</summary>
        internal string DefaultPath { get; }

        /// <summary>Action invoked when a menu item is clicked. Null for asset entries.</summary>
        internal Action Execute { get; }

        /// <summary>Optional validate function for a menu item, or null.</summary>
        internal Func<bool> Validate { get; }

        /// <summary>Optional check mark state function for a menu item, or null.</summary>
        internal Func<bool> Checked { get; }

        /// <summary>ScriptableObject type for an asset entry, or null.</summary>
        internal Type AssetType { get; }

        /// <summary>Default asset file name for an asset entry, without extension.</summary>
        internal string DefaultFileName { get; }

        /// <summary>Type that declares the entry, used to locate its script on disk.</summary>
        internal Type DeclaringType { get; }

        private ResolvedMenu(EMenuEntryKind kind, string defaultPath, Action execute, Func<bool> validate,
            Func<bool> isChecked, Type assetType, string defaultFileName, Type declaringType)
        {
            Kind = kind;
            DefaultPath = defaultPath;
            Execute = execute;
            Validate = validate;
            Checked = isChecked;
            AssetType = assetType;
            DefaultFileName = defaultFileName;
            DeclaringType = declaringType;
        }

        /// <summary>Creates a resolved menu item.</summary>
        internal static ResolvedMenu MenuItem(string defaultPath, Action execute, Func<bool> validate,
            Func<bool> isChecked, Type declaringType) => new(EMenuEntryKind.MenuItem, defaultPath, execute, validate,
            isChecked, null, string.Empty,
            declaringType);

        /// <summary>Creates a resolved asset creation entry.</summary>
        internal static ResolvedMenu CreateAsset(string defaultPath, Type assetType, string defaultFileName) => new(
            EMenuEntryKind.CreateAsset, defaultPath, null, null, null, assetType, defaultFileName, assetType);
    }
}