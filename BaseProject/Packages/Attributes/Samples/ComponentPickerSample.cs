using UnityEngine;

namespace Base.AttributePackage.Samples
{
    /// <summary>A component picked from a dropped GameObject.</summary>
    [AttributeSample(typeof(ComponentPickerAttribute), EAttributeCategory.Pickers,
        Description = "Accepts a dropped GameObject and stores one of its components, with a badge that opens a list "
            + "of the siblings when more than one matches. Dropping a GameObject on a plain component field picks the "
            + "first match silently, which is the wrong one often enough to be worth this.",
        Requirements = "Drop a GameObject carrying more than one collider to see the picker offer a choice.",
        Variations = new[]
        {
            "Nothing to configure."
        })]
    internal sealed class ComponentPickerSample : ScriptableObject
    {
        [ComponentPicker]
        [Tooltip("Drop a GameObject and pick which of its colliders to store.")]
        public Collider component;
    }
}