using Base.CoreDebugPackage.DebugMenu.CheatConsole;

namespace Base.CoreDebugPackage.Tests
{
    /// <summary>
    /// Declares one command of each kind the registry looks for, and one method that carries no
    /// attribute at all, so a scan that picks up too much is as visible as one that picks up too
    /// little.
    /// </summary>
    /// <remarks>
    /// The methods are public because the registry reaches them through reflection, the same way it
    /// reaches a real cheat command.
    /// </remarks>
    public sealed class CheatCommandRegistryProbe
    {
        /// <summary>The name the instance command is registered under.</summary>
        public const string InstanceCommand = "probe_instance";

        /// <summary>The name the static command is registered under.</summary>
        public const string StaticCommand = "probe_static";

        /// <summary>A command found by scanning an object.</summary>
        [CheatCommand(InstanceCommand)]
        public void Instance()
        {
        }

        /// <summary>A command found by scanning an assembly.</summary>
        [CheatCommand(StaticCommand)]
        public static void Static()
        {
        }

        /// <summary>A method with no attribute, which no scan should return.</summary>
        public void NotACommand()
        {
        }
    }
}