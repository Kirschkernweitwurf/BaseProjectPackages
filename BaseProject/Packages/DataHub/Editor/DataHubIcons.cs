using System;
using Base.EditorUIPackage.Editor;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Base.DataHubPackage.Editor
{
    /// <summary>
    /// The built-in editor icons the Data Hub draws, by name in one place. Unity picks the dark skin
    /// variant of each on its own.
    /// </summary>
    internal static class DataHubIcons
    {
        private const string DeleteName = "TreeEditor.Trash";
        private const string FolderName = "Folder Icon";
        private const string MinusName = "Toolbar Minus";
        private const string NewName = "Toolbar Plus";
        private const string PageName = "CustomTool";
        private const string PingName = "Project";
        private const string RefreshName = "Refresh";

        /// <summary>Deletes the selection.</summary>
        internal static Texture Delete => EditorIcons.Named(DeleteName);

        /// <summary>A group in the type tree.</summary>
        internal static Texture Folder => EditorIcons.Named(FolderName);

        /// <summary>Removes one.</summary>
        internal static Texture Minus => EditorIcons.Named(MinusName);

        /// <summary>Creates an asset or adds one.</summary>
        internal static Texture New => EditorIcons.Named(NewName);

        /// <summary>A custom page in the type tree.</summary>
        internal static Texture Page => EditorIcons.Named(PageName);

        /// <summary>Shows an asset in the Project window.</summary>
        internal static Texture Ping => EditorIcons.Named(PingName);

        /// <summary>Reads every asset again.</summary>
        internal static Texture Refresh => EditorIcons.Named(RefreshName);

        /// <summary>The icon of an asset, as the Project window shows it.</summary>
        /// <param name="asset">The asset.</param>
        /// <returns>The icon, or null for a missing asset.</returns>
        internal static Texture Of(Object asset) => asset == null
            ? null
            : AssetPreview.GetMiniThumbnail(asset);

        /// <summary>The icon of a type, the custom script icon when it has one.</summary>
        /// <param name="type">The type.</param>
        /// <returns>The icon.</returns>
        internal static Texture Of(Type type)
        {
            Texture icon = AssetPreview.GetMiniTypeThumbnail(type);

            return icon == null
                ? EditorIcons.Script
                : icon;
        }
    }
}