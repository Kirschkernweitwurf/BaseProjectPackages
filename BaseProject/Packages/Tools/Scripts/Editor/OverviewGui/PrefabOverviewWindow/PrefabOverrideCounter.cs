using System;
using Base.UtilityPackage.Logging;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Base.ToolPackage.Editor.OverviewGui.PrefabOverviewWindow
{
    /// <summary>
    /// Counts how far a prefab variant drifted away from its base prefab.
    /// </summary>
    public static class PrefabOverrideCounter
    {
        // A variant always stores its own name as a modification, which says nothing about actual drift.
        private const string NameProperty = "m_Name";

        /// <summary>
        /// Loads the variant into a temporary scene and counts its overrides against the base prefab.
        /// </summary>
        /// <param name="assetPath">Project relative path of the variant asset.</param>
        /// <returns>The counted overrides, or empty counts when the asset cannot be read.</returns>
        public static PrefabOverrideCounts Count(string assetPath)
        {
            GameObject contents = null;

            try
            {
                contents = PrefabUtility.LoadPrefabContents(assetPath);

                if (contents == null)
                    return default(PrefabOverrideCounts);

                // Only the contents of a variant behave like an instance of another prefab.
                if (!PrefabUtility.IsPartOfPrefabInstance(contents))
                    return default(PrefabOverrideCounts);

                return new PrefabOverrideCounts(CountModifiedProperties(contents),
                    PrefabUtility.GetAddedComponents(contents).Count,
                    PrefabUtility.GetRemovedComponents(contents).Count,
                    PrefabUtility.GetAddedGameObjects(contents).Count);
            }
            catch (Exception exception)
            {
                CustomLogger.LogWarning($"Overrides of {assetPath} could not be read: {exception.Message}", null);
                return default(PrefabOverrideCounts);
            }
            finally
            {
                if (contents != null)
                    PrefabUtility.UnloadPrefabContents(contents);
            }
        }

        private static int CountModifiedProperties(GameObject contents)
        {
            PropertyModification[] modifications = PrefabUtility.GetPropertyModifications(contents);

            if (modifications == null)
                return 0;

            Object baseRoot = PrefabUtility.GetCorrespondingObjectFromSource(contents);
            int count = 0;

            foreach (PropertyModification modification in modifications)
            {
                if (modification == null)
                    continue;

                if (IsRootName(modification, baseRoot))
                    continue;

                count++;
            }

            return count;
        }

        private static bool IsRootName(PropertyModification modification, Object baseRoot)
            => modification.propertyPath == NameProperty
                && modification.target == baseRoot;
    }
}