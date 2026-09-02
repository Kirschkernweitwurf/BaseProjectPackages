using System;
using UnityEngine;

namespace Base.AttributesPackage.Samples
{
    /// <summary>A method called when a field is edited.</summary>
    [AttributeSample(typeof(OnValueChangedAttribute), EAttributeCategory.Callbacks,
        Description = "Calls a method whenever the field is edited in the inspector, for the recalculation that has to "
            + "follow a value rather than wait for play mode.",
        Requirements = "The method has to be on the same object and take no parameters.",
        Variations = new[]
        {
            "Nothing to configure beyond the method name."
        })]
    internal sealed class OnValueChangedSample : ScriptableObject
    {
        [OnValueChanged(nameof(OnSpeedChanged))]
        [Tooltip("Edit this and the log below records it.")]
        public float speed = 5f;

        [ShowNonSerialized]
        [NonSerialized] public string log = "Nothing yet";

        private void OnSpeedChanged() => log = $"speed is now {speed}";
    }
}