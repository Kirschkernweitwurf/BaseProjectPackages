using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Base.ControllerSupportPackage.Controller.Focus
{
    /// <summary>Tells whether confirm does something on an object, so a prompt never offers a dead button.</summary>
    public static class SubmitTarget
    {
        /// <summary>
        /// Whether <paramref name="target"/> takes confirm right now: it handles it and a button on it is not
        /// switched off, like a pile without cards.
        /// </summary>
        /// <param name="target">The object confirm would go to.</param>
        public static bool CanSubmit(GameObject target)
        {
            if (target == null
                || !ExecuteEvents.CanHandleEvent<ISubmitHandler>(target))
                return false;

            return !target.TryGetComponent(out Selectable selectable)
                || selectable.IsInteractable();
        }
    }
}