using System;
using UnityEngine;

namespace Base.AttributePackage.Samples
{
    /// <summary>An interface field filled by picking an implementation.</summary>
    [AttributeSample(typeof(ReferencePickerAttribute), EAttributeCategory.Serialization,
        Description = "Offers every implementation of the field type and stores the one that is picked by reference, "
            + "which is how an interface field is filled in from the inspector at all.",
        Requirements = "The field needs SerializeReference on it as well, and the implementations have to be marked "
            + "Serializable.",
        Variations = new[]
        {
            "Works on interfaces and on abstract base classes.",
            "The picked implementation is edited in place under the field."
        })]
    internal sealed class ReferencePickerSample : ScriptableObject
    {
        [SerializeReference]
        [ReferencePicker]
        [Tooltip("Pick an implementation and its own fields appear under this one.")]
        public IAbility ability;

        /// <summary>Something for the picker to offer implementations of.</summary>
        public interface IAbility { }

        /// <summary>One implementation, so the picker has an entry.</summary>
        [Serializable]
        public sealed class DashAbility : IAbility
        {
            /// <summary>How far the dash travels.</summary>
            public float distance = 4f;
        }

        /// <summary>A second implementation, so the picker has a choice to offer.</summary>
        [Serializable]
        public sealed class HealAbility : IAbility
        {
            /// <summary>How much the heal restores.</summary>
            public int amount = 25;
        }
    }
}