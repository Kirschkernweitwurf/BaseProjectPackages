using UnityEngine;

namespace Base.AttributesPackage.Tests
{
    /// <summary>
    /// Two titled sections, with a pinned field in the second. Used to check that an ordered field
    /// cannot climb out of the section it was written in, and that the field carrying the title stays
    /// at the top of its own section.
    /// </summary>
    internal sealed class PropertySectionProbe : ScriptableObject
    {
        /// <summary>Serialized name of the field that opens the first section.</summary>
        internal const string FirstTitleField = nameof(firstTitle);

        /// <summary>Serialized name of the plain field in the first section.</summary>
        internal const string FirstBodyField = nameof(firstBody);

        /// <summary>Serialized name of the field that opens the second section.</summary>
        internal const string SecondTitleField = nameof(secondTitle);

        /// <summary>Serialized name of the plain field in the second section.</summary>
        internal const string SecondBodyField = nameof(secondBody);

        /// <summary>Serialized name of the pinned field in the second section.</summary>
        internal const string SecondPinnedField = nameof(secondPinned);

        [SerializeField] [Title("First")] private int firstTitle;
        [SerializeField] private int firstBody;
        [SerializeField] [Title("Second")] private int secondTitle;
        [SerializeField] private int secondBody;
        [SerializeField] [PropertyOrder(-10)] private int secondPinned;
    }
}