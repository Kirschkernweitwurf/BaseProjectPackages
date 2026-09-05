using UnityEngine;

namespace Base.AttributesPackage.Tests
{
    /// <summary>
    /// Fields in the order they are declared, which is the order Unity would draw them in. Each one
    /// carries the marking one ordering case needs, so a test can name a field rather than an index.
    /// </summary>
    internal sealed class PropertyOrderProbe : ScriptableObject
    {
        /// <summary>Serialized name of the field pinned above everything else.</summary>
        internal const string PinnedField = nameof(pinned);

        /// <summary>Serialized name of the first unmarked field.</summary>
        internal const string PlainOneField = nameof(plainOne);

        /// <summary>Serialized name of the second unmarked field.</summary>
        internal const string PlainTwoField = nameof(plainTwo);

        /// <summary>Serialized name of the field pushed below everything else.</summary>
        internal const string PushedField = nameof(pushed);

        [SerializeField] private int plainOne;
        [SerializeField] [PropertyOrder(10)] private int pushed;
        [SerializeField] private int plainTwo;
        [SerializeField] [PropertyOrder(-10)] private int pinned;
    }
}