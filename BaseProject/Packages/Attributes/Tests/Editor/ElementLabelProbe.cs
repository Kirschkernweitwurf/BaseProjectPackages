using UnityEngine;

namespace Base.AttributesPackage.Tests
{
    /// <summary>
    /// Holds one array per element shape the list label has to deal with: a leaf that is its own
    /// label, a leaf that has to be converted to one, an element whose label sits on a child, and one
    /// that has no label anywhere.
    /// </summary>
    internal sealed class ElementLabelProbe : ScriptableObject
    {
        /// <summary>Serialized name of the integer array, so a test can reach it without a literal.</summary>
        internal const string AmountsField = nameof(amounts);

        /// <summary>Serialized name of the labeled struct array.</summary>
        internal const string LabeledField = nameof(labeled);

        /// <summary>Serialized name of the string array.</summary>
        internal const string NamesField = nameof(names);

        /// <summary>Serialized name of the unlabeled struct array.</summary>
        internal const string UnlabeledField = nameof(unlabeled);

        [SerializeField] private string[] names;
        [SerializeField] private int[] amounts;
        [SerializeField] private LabeledEntry[] labeled;
        [SerializeField] private UnlabeledEntry[] unlabeled;
    }
}