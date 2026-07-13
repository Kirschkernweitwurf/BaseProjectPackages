using System;
using System.Collections.Generic;
using System.Reflection;

namespace Base.AttributePackage.Validation
{
    /// <summary>
    /// Discovers every <see cref="IValidationRule"/> implementation once and caches it. Add a new rule
    /// class with a public parameterless constructor and it is included automatically.
    /// </summary>
    public static class ValidationRules
    {
        private static IValidationRule[] _rules;

        /// <summary>All discovered rules.</summary>
        public static IReadOnlyList<IValidationRule> All => _rules ??= Discover();

        private static IValidationRule[] Discover()
        {
            List<IValidationRule> rules = new();

            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (IsFrameworkAssembly(assembly.GetName().Name))
                    continue;

                foreach (Type type in SafeGetTypes(assembly))
                {
                    if (type == null || type.IsAbstract || type.IsInterface)
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

        private static IEnumerable<Type> SafeGetTypes(Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException exception)
            {
                return exception.Types;
            }
        }

        private static bool IsFrameworkAssembly(string name)
            => name.StartsWith("Unity")
                || name.StartsWith("System")
                || name.StartsWith("Mono.")
                || name.StartsWith("netstandard")
                || name == "mscorlib";
    }
}
