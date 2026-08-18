using Base.AttributePackage;
using UnityEngine;
using UnityEngine.UI;

namespace Base.ControllerSupportPackage.Controller.Navigation
{
    /// <summary>
    /// Marks a sibling <see cref="Selectable"/> as a deliberate navigation target. Only selectables that
    /// carry this component are wired into a <see cref="NavigableGroup"/>'s explicit navigation. Any
    /// selectable without it counts as a navigation gap and gets one added during a rebuild.
    /// </summary>
    [RequireComponent(typeof(Selectable))]
    public sealed class NavigableElement : MonoBehaviour
    {
        /// <summary>Serialized name of the selectable field, for editor tooling.</summary>
        public const string SelectableFieldName = nameof(selectable);

        [Tooltip("The selectable this element makes navigable. Auto-assigned from the same GameObject.")]
        [GetComponent]
        [Required]
        [SerializeField] private Selectable selectable;

        /// <summary>The sibling selectable this element makes navigable.</summary>
        public Selectable Selectable => selectable;

        /// <summary>True when the element can currently receive focus.</summary>
        public bool IsNavigable() => selectable != null
            && selectable.IsInteractable()
            && gameObject.activeInHierarchy;
    }
}