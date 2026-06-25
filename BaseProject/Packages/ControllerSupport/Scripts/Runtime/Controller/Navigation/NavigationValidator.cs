#if UNITY_EDITOR
using System.Collections.Generic;
using Base.UtilityPackage.Logging;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Base.ControllerSupport.Controller.Navigation
{
    /// <summary>
    /// Ensures every selectable under a <see cref="NavigableGroup"/> carries a
    /// <see cref="NavigableElement"/>. Selectables without one are skipped by the navigation wiring,
    /// which silently breaks gamepad flow, so the missing component is added automatically and the fix
    /// is logged. Runs in the editor during a rebuild.
    /// </summary>
    public static class NavigationValidator
    {
        /// <summary>Adds a NavigableElement to every selectable in the root that is missing one.</summary>
        public static void Validate(Transform root, List<Selectable> buffer)
        {
            if (root == null)
                return;

            buffer.Clear();
            root.GetComponentsInChildren(true, buffer);

            foreach (Selectable selectable in buffer)
            {
                if (selectable.GetComponent<NavigableElement>() != null)
                    continue;

                AddNavigableElement(selectable, root);
            }
        }

        private static void AddNavigableElement(Selectable selectable, Transform root)
        {
            Undo.AddComponent<NavigableElement>(selectable.gameObject);

            CustomLogger.Log($"Added a NavigableElement to selectable \"{selectable.name}\" under navigable group "
                + $"\"{root.name}\" so it joins gamepad navigation.", selectable);
        }
    }
}
#endif