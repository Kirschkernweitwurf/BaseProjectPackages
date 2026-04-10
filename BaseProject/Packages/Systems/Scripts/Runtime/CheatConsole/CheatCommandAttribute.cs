using System;

namespace Systems.CheatConsole
{
    /// <summary>
    /// Marks a method as a cheat command that can be invoked via the cheat console.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public sealed class CheatCommandAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CheatCommandAttribute"/> class.
        /// </summary>
        /// <param name="command">
        /// The command string used to trigger this cheat (e.g. "god", "give_gold").
        /// </param>
        public CheatCommandAttribute(string command) => Command = command;

        /// <summary>
        /// Gets the command string that will be typed into the console to execute this cheat.
        /// </summary>
        public string Command { get; }

        /// <summary>
        /// Gets or sets the human-readable description of this cheat command.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the usage string showing how to call this command, including parameters.
        /// </summary>
        public string Usage { get; set; }
    }
}