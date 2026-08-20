using System;
using UnityEngine;

namespace Base.AttributePackage.Samples
{
    /// <summary>A string or collection that cannot be left empty.</summary>
    [AttributeSample(typeof(NotNullOrEmptyAttribute), EAttributeCategory.Validation,
        Description = "Requires a string with something in it, or a collection with at least one element. Empty is a "
            + "different failure from null and this catches both.",
        Requirements = "Clear the field to see the error appear.",
        Variations = new[]
        {
            "NotNullOrEmpty(message) writes a message of your own instead of the default one.",
            "Works on strings, arrays and lists."
        })]
    internal sealed class NotNullOrEmptySample : ScriptableObject
    {
        [NotNullOrEmpty("A profile needs a name.")]
        [Tooltip("Clear this and the message above appears.")]
        public string profileName = "Default";

        [NotNullOrEmpty]
        [Tooltip("Empty on purpose, so the error is visible. Add an element and it clears.")]
        public string[] stages = Array.Empty<string>();
    }
}