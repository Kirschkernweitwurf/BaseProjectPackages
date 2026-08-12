using UnityEngine;

namespace Base.AttributePackage.Editor.Windows.AttributeSamples.Samples
{
    /// <summary>Values shown in the inspector that Unity does not serialize.</summary>
    [AttributeSample("Callbacks")]
    internal sealed class NativeMemberSample : ScriptableObject
    {
        [Tooltip("An ordinary serialized field, so the read-only members below have something to report "
            + "on. Edit it and watch them follow.")]
        public int itemCount = 3;

        [OnArraySizeChanged(nameof(OnResized))]
        [Tooltip("Calls a method when the element count changes, but not when an element is edited.")]
        public string[] slots = new string[2];

        [ShowNonSerialized]
        [Tooltip("Shows a private field Unity would never serialize, for state that only exists at "
            + "runtime.")]
        private string lastEvent = "Nothing yet";

        /// <summary>A computed property surfaced in the inspector, read-only by nature.</summary>
        [ShowNativeProperty]
        [Tooltip("Shows a property, which has no serialized value at all, as a read-only row.")]
        public string Summary => $"{itemCount} items, {slots.Length} slots";

        private void OnResized(int size) => lastEvent = $"slots resized to {size}";
    }
}