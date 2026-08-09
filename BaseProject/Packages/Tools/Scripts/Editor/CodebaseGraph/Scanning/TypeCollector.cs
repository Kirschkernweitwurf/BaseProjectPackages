using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using Base.UtilityPackage;

namespace Base.ToolPackage.Editor.CodebaseGraph.Scanning
{
    /// <summary>
    /// Walks the scanned assemblies and splits their types into the ones that were written by hand and
    /// the ones the compiler produced for lambdas, iterators and async methods.
    /// </summary>
    public static class TypeCollector
    {
        /// <summary>Collects every type of the given assemblies. Nested types are already included.</summary>
        /// <param name="assemblies">Assemblies to walk.</param>
        /// <param name="declaredTypes">Receives the hand written types.</param>
        /// <param name="generatedTypes">Receives the compiler generated types.</param>
        public static void Collect(IReadOnlyList<Assembly> assemblies,
            List<Type> declaredTypes,
            List<Type> generatedTypes)
        {
            foreach (Assembly assembly in assemblies)
            {
                foreach (Type type in ReflectionUtility.GetLoadableTypes(assembly))
                {
                    if (type.IsGenericParameter)
                        continue;

                    if (IsGenerated(type))
                        generatedTypes.Add(type);
                    else
                        declaredTypes.Add(type);
                }
            }
        }

        private static bool IsGenerated(Type type)
        {
            // A nested type inside generated machinery is generated too, even when its own name looks
            // ordinary. That is how the array initializer blobs under PrivateImplementationDetails leak in.
            for (Type current = type; current != null; current = current.DeclaringType)
            {
                if (current.IsDefined(typeof(CompilerGeneratedAttribute), false))
                    return true;

                if (CompilerGeneratedNameResolver.IsGeneratedName(current.Name))
                    return true;
            }

            return false;
        }

        /// <summary>Walks outward through nested types until a hand written type is reached.</summary>
        /// <param name="type">Type to start at.</param>
        /// <returns>The owning hand written type, or null when there is none.</returns>
        public static Type FindDeclaringWrittenType(Type type)
        {
            Type current = type;

            while (current != null && IsGenerated(current))
                current = current.DeclaringType;

            return current;
        }
    }
}
