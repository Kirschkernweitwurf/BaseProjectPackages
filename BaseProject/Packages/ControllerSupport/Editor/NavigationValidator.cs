using System.Collections.Generic;
using Base.ControllerSupportPackage.Controller.Navigation;
using Base.UtilityPackage.Logging;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Base.ControllerSupportPackage.Editor
{
    /// <summary>
    /// Ensures every <see cref="Selectable"/> under a <see cref="NavigableGroup"/> carries a
    /// <see cref="NavigableElement"/>. Selectables without one are skipped by the navigation wiring,
    /// which silently breaks gamepad flow, so the missing component is added and the fix is logged.
    /// Runs as the first step of a rebuild, so newly added elements are wired in the same pass.
    /// </summary>
    internal static class NavigationValidator
    {
        private static readonly List<Selectable> Selectables = new();

        /// <summary>
        /// Adds a <see cref="NavigableElement"/> to every selectable below the root that is missing one
        /// and returns how many were added.
        /// </summary>
        internal static int AddMissingElements(Transform root)
        {
            if (root == null)
            {
                CustomLogger.LogWarning("Validation was called without a root, nothing to check.", null);
                return 0;
            }

            Selectables.Clear();
            root.GetComponentsInChildren(true, Selectables);

            int added = 0;

            foreach (Selectable selectable in Selectables)
            {
                if (selectable.GetComponent<NavigableElement>() != null)
                    continue;

                AddNavigableElement(selectable, root);
                added++;
            }

            return added;
        }

        private static void AddNavigableElement(Selectable selectable, Transform root)
        {
            NavigableElement element = Undo.AddComponent<NavigableElement>(selectable.gameObject);

            // Assigned here rather than left to the inspector, so elements added in bulk are wired
            // even when their GameObject is never selected.
            SerializedObject serializedElement = new(element);
            serializedElement.FindProperty(NavigableElement.SelectableFieldName).objectReferenceValue = selectable;
            serializedElement.ApplyModifiedProperties();

            CustomLogger.Log($"Added a {nameof(NavigableElement)} to selectable \"{selectable.name}\" under "
                + $"navigable group \"{root.name}\" so it joins gamepad navigation.", selectable);
        }
    }
}