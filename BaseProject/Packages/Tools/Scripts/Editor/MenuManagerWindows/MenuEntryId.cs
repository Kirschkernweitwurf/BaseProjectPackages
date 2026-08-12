using System;

namespace Base.ToolPackage.Editor.MenuManagerWindows
{
    /// <summary>
    /// Builds the stable ids that identify a managed entry across recompiles and across renames of
    /// its menu path. Every producer and consumer of an id goes through here, so the format is
    /// written down once.
    /// </summary>
    internal static class MenuEntryId
    {
        /// <summary>Prefix of every asset creation id.</summary>
        public const string CreateAssetPrefix = "CA:";

        private const char MemberSeparator = '.';

        /// <summary>Prefix of every menu item id.</summary>
        public const string MenuItemPrefix = "MI:";

        /// <summary>Builds the id of an asset creation entry.</summary>
        /// <param name="type">The ScriptableObject type behind the entry.</param>
        /// <returns>The stable id of the entry.</returns>
        public static string ForCreateAsset(Type type) => CreateAssetPrefix + type.FullName;

        /// <summary>Builds the id of a menu item entry.</summary>
        /// <param name="owner">The type that declares the method.</param>
        /// <param name="methodName">The name of the decorated method.</param>
        /// <returns>The stable id of the entry.</returns>
        public static string ForMenuItem(Type owner, string methodName)
            => MenuItemPrefix + owner.FullName + MemberSeparator + methodName;
    }
}