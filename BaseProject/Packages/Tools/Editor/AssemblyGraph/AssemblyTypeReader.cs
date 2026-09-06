using System;
using System.Collections.Generic;
using System.Reflection;

namespace Base.ToolsPackage.Editor.AssemblyGraph
{
    /// <summary>
    /// Reads the types of an assembly without letting one unloadable type abort the scan. An
    /// assembly that only half loads still answers for the half that did.
    /// </summary>
    internal static class AssemblyTypeReader
    {
        private static readonly Type[] NoTypes = Array.Empty<Type>();

        /// <summary>Returns every type the assembly could load.</summary>
        /// <param name="assembly">Assembly to read.</param>
        /// <returns>The loaded types, which may be fewer than the assembly declares.</returns>
        internal static Type[] Read(Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException exception)
            {
                return CollectLoaded(exception);
            }
            catch (Exception)
            {
                return NoTypes;
            }
        }

        private static Type[] CollectLoaded(ReflectionTypeLoadException exception)
        {
            if (exception.Types == null)
                return NoTypes;

            List<Type> loaded = new(exception.Types.Length);

            foreach (Type type in exception.Types)
            {
                if (type != null)
                    loaded.Add(type);
            }

            return loaded.ToArray();
        }
    }
}