using System;
using System.Collections.Generic;
using UnityEngine;

namespace Base.UtilityPackage
{
    /// <summary>
    /// Decides whether a type or an assembly belongs to Unity or to the .NET framework, which is the
    /// line between code a project owns and code it only consumes. Anything walking types has to draw
    /// that line somewhere, and drawing it in one place is what keeps the answers consistent.
    /// </summary>
    /// <remarks>
    /// The judgement is made on the assembly name alone. That is coarse, but it is the only signal
    /// available without loading metadata, and the alternative of listing assemblies by hand goes out
    /// of date with every Unity version.
    /// </remarks>
    public static class FrameworkAssemblies
    {
        private const string CoreLibrary = "mscorlib";
        private const string MonoPrefix = "Mono.";
        private const string NetStandardPrefix = "netstandard";
        private const string SystemPrefix = "System";
        private const string UnityPrefix = "Unity";

        private static readonly Dictionary<Type, bool> Results = new();

        /// <summary>Returns true when the type is declared in a Unity or framework assembly.</summary>
        /// <param name="type">The type to judge. A null type counts as framework, since it is not project code.</param>
        /// <returns>True when the type is not project code.</returns>
        public static bool Contains(Type type)
        {
            if (type == null)
                return true;

            if (Results.TryGetValue(type, out bool cached))
                return cached;

            bool result = Contains(type.Assembly.GetName().Name);
            Results[type] = result;

            return result;
        }

        /// <summary>Returns true when the assembly name belongs to Unity or the framework.</summary>
        /// <param name="assemblyName">The simple assembly name, without version or culture.</param>
        /// <returns>True when the assembly is not project code.</returns>
        public static bool Contains(string assemblyName)
        {
            if (string.IsNullOrEmpty(assemblyName))
                return true;

            return assemblyName.StartsWith(UnityPrefix, StringComparison.Ordinal)
                || assemblyName.StartsWith(SystemPrefix, StringComparison.Ordinal)
                || assemblyName.StartsWith(MonoPrefix, StringComparison.Ordinal)
                || assemblyName.StartsWith(NetStandardPrefix, StringComparison.Ordinal)
                || assemblyName == CoreLibrary;
        }

        // The answer for a given type never changes, so the cache is only reset to keep it from
        // growing across play sessions. Domain reload is off, which means nothing else would clear it.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Results.Clear();
    }
}