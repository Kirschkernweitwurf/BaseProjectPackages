using UnityEngine;

namespace Base.AttributesPackage.Tests
{
    /// <summary>
    /// A foldout of two fields, only the second of which is pinned, followed by a plain field. Used to
    /// check that a run moves as one thing rather than letting the pinned member leave it behind.
    /// </summary>
    internal sealed class PropertyRunProbe : ScriptableObject
    {
        /// <summary>Serialized name of the first member of the foldout.</summary>
        internal const string GroupedOneField = nameof(groupedOne);

        /// <summary>Serialized name of the pinned member of the foldout.</summary>
        internal const string GroupedTwoField = nameof(groupedTwo);

        /// <summary>Serialized name of the field declared before the foldout.</summary>
        internal const string LeadingField = nameof(leading);

        private const string GroupName = "Group";

        [SerializeField] private int leading;
        [SerializeField] [Foldout(GroupName)] private int groupedOne;
        [SerializeField] [Foldout(GroupName)] [PropertyOrder(-10)] private int groupedTwo;
    }
}