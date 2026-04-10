using System.Reflection;
using Utility.Logging;

namespace Systems.CheatConsole
{
    /// <summary>
    /// Immutable data describing a cheat command and how to invoke it.
    /// </summary>
    public sealed class CheatCommandInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CheatCommandInfo"/> class.
        /// </summary>
        /// <param name="attribute">The attribute that defines this cheat command.</param>
        /// <param name="method">The method to invoke when this command is executed.</param>
        /// <param name="target">
        /// The object instance on which to invoke the method (null for static methods).
        /// </param>
        public CheatCommandInfo(CheatCommandAttribute attribute, MethodInfo method, object target)
        {
            if (method == null)
            {
                CustomLogger.LogError("Cheat command method cannot be null.", null);
                return;
            }

            if (attribute == null)
            {
                CustomLogger.LogError("Cheat command attribute cannot be null.", null);
                return;
            }

            Attribute = attribute;
            Method = method;
            Target = target;
        }

        /// <summary>
        /// Gets the cheat command attribute metadata.
        /// </summary>
        public CheatCommandAttribute Attribute { get; }

        /// <summary>
        /// Gets the method that will be invoked when the command is executed.
        /// </summary>
        public MethodInfo Method { get; }

        /// <summary>
        /// Gets the target instance on which the method is invoked, or null if the method is static.
        /// </summary>
        public object Target { get; }
    }
}