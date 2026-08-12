using UnityEngine;

namespace Base.AttributePackage.Editor.Windows.AttributeSamples.Samples
{
    /// <summary>Small labels and tints attached to a field.</summary>
    [AttributeSample("Layout")]
    internal sealed class AffixSample : ScriptableObject
    {
        [Prefix("Speed")]
        [Tooltip("Puts a label in front of the field, for a unit or a qualifier that reads better before "
            + "the number than after it.")]
        public float prefixed = 3.5f;

        [Suffix(SuffixAttribute.MetersPerSecond)]
        [Tooltip("Puts a label after the field. The constants cover the common units so the same unit is "
            + "always spelled the same way.")]
        public float suffixed = 7f;

        [GUIColor(EColor.Lime)]
        [Tooltip("Tints the whole field, for the one value on a component that needs to stand out.")]
        public string tinted = "Lime tinted";

        [StartExpanded]
        [Tooltip("Opens the first time you see it. Fold it up and it stays folded, because only the "
            + "first draw is forced.")]
        public string[] expanded =
        {
            "first",
            "second"
        };

        [PropertyOrder(-1)]
        [Tooltip("Declared last, drawn first. Moves a field in the inspector without moving it in the "
            + "file, so the serialized data layout does not change for a cosmetic reason.")]
        public string pinnedToTop = "Declared last, drawn first";
    }
}