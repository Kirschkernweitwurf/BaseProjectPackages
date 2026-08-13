using UnityEngine;

namespace Base.AttributePackage.Samples
{
    /// <summary>Consecutive fields placed on one row.</summary>
    [AttributeSample(typeof(HorizontalAttribute), EAttributeCategory.Layout,
        Description = "Puts consecutive fields that share a name on one row, for values that are read together and "
            + "mean little apart.",
        Requirements = "Nothing.",
        Variations = new[]
        {
            "Weight decides how much of the row each field takes. Left out, the row is split evenly."
        })]
    internal sealed class HorizontalSample : ScriptableObject
    {
        [Horizontal("size")]
        [Tooltip("Shares a row with the field below, evenly.")]
        public int columns = 4;

        [Horizontal("size")]
        public int rows = 3;

        [Horizontal("weighted", Weight = 3f)]
        [Tooltip("Takes three quarters of its row, since the field below asks for one.")]
        public string label = "Takes most of the row";

        [Horizontal("weighted", Weight = 1f)]
        public int count = 1;
    }
}