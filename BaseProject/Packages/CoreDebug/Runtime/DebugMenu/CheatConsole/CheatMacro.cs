using System.Collections.Generic;
using Base.UtilityPackage.Menus;
using UnityEngine;

namespace Base.CoreDebugPackage.DebugMenu.CheatConsole
{
    /// <summary>
    /// A cheat command anyone can set up in the editor: typing <see cref="Command"/> in the cheat console runs
    /// its lines one after another, as if they were typed. Lets every team member keep their own test setup, for
    /// example "Jonny" turning on two cheats at once. Picked up from any <c>Resources/CheatMacros</c> folder.
    /// </summary>
    [DynamicCreateAssetMenu("Scriptable Objects/Base/Debug/New Cheat Macro", "CM_CheatMacro")]
    public sealed class CheatMacro : ScriptableObject
    {
        [Tooltip("What to type in the cheat console. Any name, for example your own. Empty uses the asset name.")]
        [SerializeField] private string command;

        [Tooltip("Shown next to the command in help.")]
        [SerializeField] private string description;

        [Tooltip("Cheat console lines, run top to bottom. Each one is a command with its arguments, exactly as you "
            + "would type it. A line may call another macro.")]
        [SerializeField] private List<string> lines = new();

        /// <summary>The name typed in the console to run this macro.</summary>
        public string Command => string.IsNullOrWhiteSpace(command)
            ? name
            : command.Trim();

        /// <summary>Shown next to the command in help.</summary>
        public string Description => description;

        /// <summary>The console lines, in the order they run.</summary>
        public IReadOnlyList<string> Lines => lines;
    }
}