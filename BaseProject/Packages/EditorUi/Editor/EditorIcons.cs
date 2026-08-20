using UnityEditor;
using UnityEngine;

namespace Base.EditorUiPackage
{
    /// <summary>
    /// The built-in editor icons the Base windows report findings with, resolved once and cached.
    /// </summary>
    /// <remarks>
    /// <see cref="EditorGUIUtility.IconContent(string)"/> is only valid inside a GUI callback, so
    /// every icon here is resolved on first access rather than in a static constructor.
    /// </remarks>
    public static class EditorIcons
    {
        private const string ErrorName = "console.erroricon.sml";
        private const string GameObjectName = "GameObject Icon";
        private const string ScriptName = "cs Script Icon";
        private const string SuccessName = "TestPassed";
        private const string WarningName = "console.warnicon.sml";

        /// <summary>Red icon for a finding that breaks something.</summary>
        public static Texture Error => Get(ref _error, ErrorName);

        /// <summary>Default icon for a game object in a group header.</summary>
        public static Texture GameObject => Get(ref _gameObject, GameObjectName);

        /// <summary>Script icon for a type in a group header.</summary>
        public static Texture Script => Get(ref _script, ScriptName);

        /// <summary>Green check shown when a scan found nothing to report.</summary>
        public static Texture Success => Get(ref _success, SuccessName);

        /// <summary>Yellow icon for a finding that only changes behavior.</summary>
        public static Texture Warning => Get(ref _warning, WarningName);

        private static Texture _error;
        private static Texture _gameObject;
        private static Texture _script;
        private static Texture _success;
        private static Texture _warning;

        /// <summary>
        /// Resolves any built-in editor icon by name, for the ones this class does not name itself.
        /// </summary>
        /// <param name="iconName">The built-in icon name.</param>
        /// <returns>The icon texture, or null outside a GUI callback.</returns>
        public static Texture Named(string iconName) => EditorGUIUtility.IconContent(iconName).image;

        private static Texture Get(ref Texture cached, string iconName)
        {
            if (cached == null)
                cached = Named(iconName);

            return cached;
        }
    }
}