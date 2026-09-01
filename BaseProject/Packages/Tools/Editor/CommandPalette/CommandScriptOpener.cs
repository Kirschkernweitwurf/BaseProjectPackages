using Base.ToolPackage.Editor.MenuManagerWindows;
using UnityEditor;

namespace Base.ToolPackage.Editor.CommandPalette
{
    /// <summary>
    /// Opens the script that declares a command. The lookup walks the asset database, so it only
    /// runs for the single entry the user asked about and its results are cached.
    /// </summary>
    internal static class CommandScriptOpener
    {
        private static readonly MenuScriptLookup Scripts = new();

        /// <summary>Whether the entry could have a script to open.</summary>
        /// <param name="entry">The entry to check.</param>
        /// <returns><c>true</c> when the declaring type is known.</returns>
        internal static bool CanOpen(CommandEntry entry) => entry.Owner != null;

        /// <summary>Opens the declaring script in the external editor and pings it.</summary>
        /// <param name="entry">The entry whose script is opened.</param>
        internal static void Open(CommandEntry entry)
        {
            if (!CanOpen(entry))
                return;

            MonoScript script = Scripts.Resolve(entry.Owner);

            if (script == null)
                return;

            AssetDatabase.OpenAsset(script);
            EditorGUIUtility.PingObject(script);
        }
    }
}