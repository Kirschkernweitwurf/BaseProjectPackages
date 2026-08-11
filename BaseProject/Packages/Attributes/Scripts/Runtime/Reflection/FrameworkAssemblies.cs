using System;
using System.Collections.Generic;

namespace Base.AttributePackage
{
    /// <summary>
    /// Decides whether a type or an assembly belongs to Unity or the .NET framework. Shared by the
    /// validation scanner, the rule discovery and the editor member pipeline, so all three agree on
    /// where user code ends. Results are cached per type and cleared on domain reload.
    /// </summary>
    internal static class FrameworkAssemblies
    {
        private const string CoreLibrary = "mscorlib";
        private const string MonoPrefix = "Mono.";
        private const string NetStandardPrefix = "netstandard";
        private const string SystemPrefix = "System";
        private const string UnityPrefix = "Unity";

        private static readonly Dictionary<Type, bool> Results = new();

        /// <summary>Returns true when the type is declared in a Unity or framework assembly.</summary>
        public static bool Contains(Type type)
        {
            if (Results.TryGetValue(type, out bool cached))
                return cached;

            bool result = Contains(type.Assembly.GetName().Name);
            Results[type] = result;
            return result;
        }

        /// <summary>Returns true when the assembly name belongs to Unity or the framework.</summary>
        public static bool Contains(string assemblyName) => assemblyName.StartsWith(UnityPrefix)
            || assemblyName.StartsWith(SystemPrefix)
            || assemblyName.StartsWith(MonoPrefix)
            || assemblyName.StartsWith(NetStandardPrefix)
            || assemblyName == CoreLibrary;
    }
}