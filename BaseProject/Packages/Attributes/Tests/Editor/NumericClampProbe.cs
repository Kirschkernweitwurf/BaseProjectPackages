using UnityEngine;

namespace Base.AttributesPackage.Tests
{
    /// <summary>
    /// One field per numeric shape the clamp handles, so a test can name a type rather than build a
    /// property for it.
    /// </summary>
    internal sealed class NumericClampProbe : ScriptableObject
    {
        /// <summary>Serialized name of the float field.</summary>
        internal const string DecimalField = nameof(decimalValue);

        /// <summary>Serialized name of the int field.</summary>
        internal const string NumberField = nameof(number);

        /// <summary>Serialized name of the string field, which no clamp applies to.</summary>
        internal const string TextField = nameof(text);

        /// <summary>Serialized name of the two component float vector.</summary>
        internal const string Vector2Field = nameof(vector2);

        /// <summary>Serialized name of the two component integer vector.</summary>
        internal const string Vector2IntField = nameof(vector2Int);

        /// <summary>Serialized name of the three component float vector.</summary>
        internal const string Vector3Field = nameof(vector3);

        /// <summary>Serialized name of the three component integer vector.</summary>
        internal const string Vector3IntField = nameof(vector3Int);

        [SerializeField] private int number;
        [SerializeField] private float decimalValue;
        [SerializeField] private Vector2 vector2;
        [SerializeField] private Vector3 vector3;
        [SerializeField] private Vector2Int vector2Int;
        [SerializeField] private Vector3Int vector3Int;
        [SerializeField] private string text;
    }
}