using System;
using System.Collections.Generic;
using System.Reflection;
using Base.UtilityPackage;
using Base.UtilityPackage.Logging;

namespace Base.SaveSystemPackage.Serialization
{
    /// <summary>
    /// Finds every <see cref="ISaveMigration"/> in the loaded assemblies and creates one instance of
    /// each.
    /// <para>
    /// A migration is code, not an asset, so nothing can point at it from the inspector. Without this
    /// a project would have to replace the whole composition just to register one step, which is the
    /// kind of friction that ends with the step never being written.
    /// </para>
    /// </summary>
    public static class SaveMigrationDiscovery
    {
        // The engine and framework assemblies cannot contain a project's migrations, and walking their
        // types is by far the most expensive part of this scan.
        private static readonly string[] SkippedAssemblyPrefixes =
        {
            "Mono.",
            "mscorlib",
            "netstandard",
            "nunit.",
            "System",
            "Unity.",
            "UnityEditor",
            "UnityEngine"
        };

        /// <summary>Creates one instance of every migration that can be constructed.</summary>
        /// <returns>The discovered migrations, in no particular order.</returns>
        public static IReadOnlyList<ISaveMigration> Discover()
        {
            List<ISaveMigration> migrations = new();

            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (IsSkipped(assembly))
                    continue;

                foreach (Type type in ReflectionUtility.GetLoadableTypes(assembly))
                    TryAdd(type, migrations);
            }

            return migrations;
        }

        private static bool IsSkipped(Assembly assembly)
        {
            // A dynamic assembly holds no project code and throws rather than listing its types.
            if (assembly.IsDynamic)
                return true;

            string name = assembly.GetName().Name;

            foreach (string prefix in SkippedAssemblyPrefixes)
            {
                if (name.StartsWith(prefix, StringComparison.Ordinal))
                    return true;
            }

            return false;
        }

        private static void TryAdd(Type type, ICollection<ISaveMigration> migrations)
        {
            if (type.IsAbstract
                || type.IsGenericTypeDefinition
                || !typeof(ISaveMigration).IsAssignableFrom(type))
                return;

            // The discovery is what creates the instance, so a migration cannot ask for anything.
            if (type.GetConstructor(Type.EmptyTypes) == null)
            {
                CustomLogger.LogError($"'{type.Name}' implements {nameof(ISaveMigration)} but has no public "
                    + "parameterless constructor, so it cannot be discovered and will never run.", null);

                return;
            }

            try
            {
                migrations.Add((ISaveMigration)Activator.CreateInstance(type));
            }
            catch (Exception exception)
            {
                CustomLogger.LogError($"Could not create the save migration '{type.Name}': {exception.Message}",
                    null);
            }
        }
    }
}