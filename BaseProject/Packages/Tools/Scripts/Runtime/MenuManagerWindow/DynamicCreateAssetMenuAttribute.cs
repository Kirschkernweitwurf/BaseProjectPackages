using System;

namespace Base.ToolPackage.MenuManagerWindow
{
    /// <summary>
    /// Marks a ScriptableObject type as a data driven asset creation entry. Path and priority are managed in the Menu
    /// Manager window.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class DynamicCreateAssetMenuAttribute : Attribute
    {
        /// <summary>Path relative to "Assets/Create/" used until it is changed in the window, for example "Gameplay/Card".</summary>
        public string DefaultPath { get; }

        /// <summary>Default file name for the created asset, without extension.</summary>
        public string FileName { get; }

        /// <summary>Creates the attribute.</summary>
        public DynamicCreateAssetMenuAttribute(string defaultPath = "", string fileName = "")
        {
            DefaultPath = defaultPath;
            FileName = fileName;
        }
    }
}