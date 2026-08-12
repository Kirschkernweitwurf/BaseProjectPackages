using System;
using System.Collections.Generic;
using Base.UtilityPackage.Collections;
using Base.UtilityPackage.Serialization;
using UnityEngine;

namespace Base.AttributePackage.Editor.Windows.AttributeSamples.Samples
{
    /// <summary>The types Unity cannot serialize on its own.</summary>
    [AttributeSample("Serialization")]
    internal sealed class SerializationSample : ScriptableObject
    {
        /// <summary>Something for the reference picker to offer implementations of.</summary>
        public interface IAbility
        {
            /// <summary>Name shown in the picker.</summary>
            string DisplayName { get; }
        }

        /// <summary>One implementation, so the picker has an entry.</summary>
        [Serializable]
        public sealed class DashAbility : IAbility
        {
            /// <summary>How far the dash travels.</summary>
            [Tooltip("An ordinary field on the picked implementation, to show the picker edits it in place.")]
            public float distance = 4f;

            /// <inheritdoc/>
            public string DisplayName => nameof(DashAbility);
        }

        [InfoBox("Unity stores none of these by itself. Add two rows with the same key to see the "
            + "duplicate warning.")]
        [Tooltip("A dictionary Unity can save. Add two rows with the same key to see the duplicate warning.")]
        public SerializableDictionary<string, int> counts = new();

        [Tooltip("A set Unity can save, with the same duplicate warning.")]
        public SerializableHashSet<string> names = new();

        [SerializeReference] [ReferencePicker] public IAbility ability;

        [Tooltip("An object field restricted to things implementing an interface.")]
        public InterfaceReference<IAbility> abilityObject = new();

        [Tooltip("Serializes a Type, narrowed to one base, with a searchable picker.")]
        public TypeReferenceOfBase<IAbility> abilityType = new();

        [Tooltip("References a scene by asset rather than by name, so renaming the file keeps it intact.")]
        public SceneReference scene = new();

        [InlineProperty] public Range range = new();

        /// <summary>Two numbers, small enough that a foldout costs more than it shows.</summary>
        [Serializable]
        public sealed class Range
        {
            /// <summary>Low end.</summary>
            [Tooltip("Low end of the inline range.")]
            public float min = 1f;

            /// <summary>High end.</summary>
            [Tooltip("High end of the inline range.")]
            public float max = 5f;
        }
    }
}