using System;
using System.Collections.Generic;
using System.Reflection;
using Base.UtilityPackage;

namespace Base.AttributesPackage
{
    /// <summary>
    /// Discovers every <see cref="IValidationRule"/> implementation once and caches it. Add a new rule
    /// class with a public parameterless constructor, and it is included automatically.
    /// </summary>
    internal static class ValidationRules
    {
        /// <summary>All discovered rules.</summary>
        internal static IReadOnlyList<IValidationRule> All => _rules ??= Discover();

        // Reflection cache over loaded types. Those cannot change without a domain reload,
        // which clears this anyway, so carrying it across play sessions is correct.
        private static IValidationRule[] _rules;

        private static IValidationRule[] Discover()
        {
            List<IValidationRule> rules = new();

            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (IsFrameworkAssembly(assembly.GetName().Name))
                    continue;

                foreach (Type type in ReflectionUtility.GetLoadableTypes(assembly))
                {
                    if (type.IsAbstract || type.IsInterface)
                        continue;

                    if (!typeof(IValidationRule).IsAssignableFrom(type))
                        continue;

                    if (type.GetConstructor(Type.EmptyTypes) == null)
                        continue;

                    rules.Add((IValidationRule)Activator.CreateInstance(type));
                }
            }

            return rules.ToArray();
        }

        private static bool IsFrameworkAssembly(string name) => name.StartsWith("Unity")
            || name.StartsWith("System")
            || name.StartsWith("Mono.")
            || name.StartsWith("netstandard")
            || name == "mscorlib";
    }
}