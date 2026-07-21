using UnityEngine;
using UnityEngine.UI;

namespace Base.ControllerSupport.Controller.Navigation
{
    /// <summary>
    /// Marks a sibling <see cref="Selectable"/> as a deliberate navigation target. Only selectables that
    /// carry this component are wired into a <see cref="NavigableGroup"/>'s explicit navigation. Any
    /// selectable without it is reported by <see cref="NavigationValidator"/> as a navigation gap.
    /// </summary>
    [RequireComponent(typeof(Selectable))]
    public sealed class NavigableElement : MonoBehaviour
    {
        private Selectable _selectable;

        /// <summary>The sibling selectable this element makes navigable. Resolved lazily for edit mode.</summary>
        public Selectable Selectable => _selectable != null
            ? _selectable
            : _selectable = GetComponent<Selectable>();

        /// <summary>True when the element can currently receive focus.</summary>
        public bool IsNavigable() => Selectable != null && Selectable.IsInteractable() && gameObject.activeInHierarchy;
    }
}