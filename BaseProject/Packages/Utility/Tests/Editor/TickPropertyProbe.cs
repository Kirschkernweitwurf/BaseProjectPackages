using UnityEngine;

namespace Base.UtilityPackage.Tests
{
    /// <summary>
    /// One field per shape a date row can be pointed at: the two that hold ticks, the integer that
    /// cannot, and two that are not tick counts at all.
    /// </summary>
    internal sealed class TickPropertyProbe : ScriptableObject
    {
        /// <summary>Serialized name of the tick field inside the wrapper.</summary>
        internal const string InnerTicksField = nameof(TickWrapper.ticks);

        /// <summary>Serialized name of the field that is a number but too small for ticks.</summary>
        internal const string NarrowField = nameof(narrow);

        /// <summary>Serialized name of the bare tick count.</summary>
        internal const string TicksField = nameof(ticks);

        /// <summary>Serialized name of the struct that holds a tick count.</summary>
        internal const string WrapperField = nameof(wrapper);

        /// <summary>Serialized name of the struct that holds no tick count.</summary>
        internal const string WrongWrapperField = nameof(wrongWrapper);

        [SerializeField] private long ticks;
        [SerializeField] private int narrow;
        [SerializeField] private TickWrapper wrapper;
        [SerializeField] private EmptyWrapper wrongWrapper;
    }
}