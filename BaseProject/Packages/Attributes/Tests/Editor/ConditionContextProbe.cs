using UnityEngine;

namespace Base.AttributesPackage.Tests
{
    /// <summary>
    /// Carries one member of every shape the editor side of a condition can be pointed at: a
    /// serialized bool it reads straight off the property, a serialized member that is not a bool, a
    /// bool that is not serialized at all, and an enum.
    /// </summary>
    internal sealed class ConditionContextProbe : ScriptableObject
    {
        /// <summary>Serialized name of the enum field, so a test can point a condition at it.</summary>
        internal const string MoodField = nameof(mood);

        /// <summary>Serialized name of the member that is not a bool.</summary>
        internal const string NumberField = nameof(number);

        /// <summary>Serialized name of the bool a condition reads off the property.</summary>
        internal const string SerializedFlagField = nameof(serializedFlag);

        /// <summary>Serialized name of the field standing in for the member being drawn.</summary>
        internal const string TargetField = nameof(target);

        [SerializeField] private int target;
        [SerializeField] private bool serializedFlag;
        [SerializeField] private int number;
        [SerializeField] private EProbeMood mood;

        /// <summary>A bool that is not serialized, so it can only be reached by reflection.</summary>
        internal bool UnserializedFlag { get; set; }
    }
}