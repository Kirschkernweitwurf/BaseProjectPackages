using UnityEngine;

namespace Base.AttributesPackage.Samples
{
    /// <summary>A reference restricted to objects implementing an interface.</summary>
    [AttributeSample(typeof(MustImplementAttribute), EAttributeCategory.Validation,
        Description = "Accepts an object only when it, or a component on it, implements the named interfaces. Unity "
            + "cannot type a field as an interface and keep it serializable, and this is the closest thing to it that "
            + "still serializes.",
        Requirements = "Try dragging in a GameObject without the interface to see it rejected.",
        Variations = new[]
        {
            "Several interfaces can be named, and all of them are required.",
            "Use InterfaceReference from the utility package when you want the field typed as the interface instead."
        })]
    internal sealed class MustImplementSample : ScriptableObject
    {
        [MustImplement(typeof(IMarker))]
        [Tooltip("Only accepts objects carrying a component that implements the interface.")]
        public GameObject mustImplement;

        /// <summary>Something for the constraint above to ask for.</summary>
        public interface IMarker { }
    }
}