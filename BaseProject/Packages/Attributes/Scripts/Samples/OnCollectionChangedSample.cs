using System.Collections.Generic;
using System;
using UnityEngine;

namespace Base.AttributePackage.Samples
{
    /// <summary>Methods called before and after a collection changes.</summary>
    [AttributeSample(typeof(OnCollectionChangedAttribute), EAttributeCategory.Callbacks,
        Description = "Calls one method before the size changes and one after, so whatever is leaving can be released "
            + "while it is still there.",
        Requirements = "Both methods have to be on the same object and take the size as an int.",
        Variations = new[]
        {
            "Either method can be left out when only one half is needed."
        })]
    internal sealed class OnCollectionChangedSample : ScriptableObject
    {
        [OnCollectionChanged(nameof(BeforeChanged), nameof(AfterChanged))]
        [Tooltip("Add or remove an element and the log below records both halves.")]
        public List<string> items = new();

        [ShowNonSerialized]
        [NonSerialized] public string log = "Nothing yet";

        // The before half runs while the old contents are still there, which is what a collection that owns
        // something needs in order to release what is leaving.
        private void BeforeChanged(int size) => log = $"before: {size} items";

        private void AfterChanged(int size) => log = $"after: {size} items";
    }
}