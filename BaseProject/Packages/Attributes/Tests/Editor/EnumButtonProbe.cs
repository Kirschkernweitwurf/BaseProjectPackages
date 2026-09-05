using UnityEngine;

namespace Base.AttributesPackage.Tests
{
    /// <summary>
    /// One serialized field per enum shape the layout has to handle: a plain enum, a flags enum, and a
    /// flags enum with nothing to draw.
    /// </summary>
    internal sealed class EnumButtonProbe : ScriptableObject
    {
        /// <summary>Serialized name of the flags enum field.</summary>
        internal const string FlagsField = nameof(flags);

        /// <summary>Serialized name of the plain enum field.</summary>
        internal const string MoodField = nameof(mood);

        /// <summary>Serialized name of the flags enum that offers only its zero member.</summary>
        internal const string ZeroOnlyField = nameof(zeroOnly);

        [SerializeField] private EProbeMood mood;
        [SerializeField] private EProbeFlags flags;
        [SerializeField] private EProbeZeroOnly zeroOnly;
    }
}