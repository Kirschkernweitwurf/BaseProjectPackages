using System;
using UnityEngine;

namespace Base.AttributesPackage.Samples
{
    /// <summary>A method called when a collection changes size.</summary>
    [AttributeSample(typeof(OnArraySizeChangedAttribute), EAttributeCategory.Callbacks,
        Description = "Calls a method when the element count changes, but not when an element is edited, for the setup "
            + "that depends on how many there are rather than what they hold.",
        Requirements = "The method has to be on the same object and take the new size as an int.",
        Variations = new[]
        {
            "Use OnCollectionChanged instead when the old contents are still needed."
        })]
    internal sealed class OnArraySizeChangedSample : ScriptableObject
    {
        [OnArraySizeChanged(nameof(OnResized))]
        [Tooltip("Add or remove an element and the log below records it.")]
        public string[] slots = new string[2];

        [ShowNonSerialized]
        [NonSerialized] public string log = "Nothing yet";

        private void OnResized(int size) => log = $"resized to {size}";
    }
}