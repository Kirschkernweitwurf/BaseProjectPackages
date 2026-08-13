using UnityEngine;

namespace Base.AttributePackage.Editor.Windows.AttributeSamples.Samples
{
    /// <summary>Controls that replace the plain field.</summary>
    [AttributeSample("Widgets")]
    internal sealed class WidgetsSample : ScriptableObject
    {
        /// <summary>Damage types, shown as a mask and as buttons.</summary>
        [System.Flags]
        public enum EElement : byte
        {
            /// <summary>Nothing selected.</summary>
            None = 0,

            /// <summary>Fire damage.</summary>
            Fire = 1,

            /// <summary>Ice damage.</summary>
            Ice = 2,

            /// <summary>Shock damage.</summary>
            Shock = 4
        }

        [InfoBox("The upper bound of the first slider is read from a field, not a constant.")]
        [Tooltip("The upper bound the slider below reads, to show a bound can be a member rather than a constant.")]
        public float maxSpeed = 20f;

        [Slider(0f, nameof(maxSpeed), AutoClamp = true)]
        [Suffix(SuffixAttribute.MetersPerSecond)]
        [Tooltip("A slider whose upper bound comes from another field, clamped so the value cannot sit outside it.")]
        public float speed = 8f;

        [MinMaxSlider(0f, 100f)] public Vector2 spawnRange = new(20f, 80f);

        [ProgressBar(100f, EColor.Green)] public float charge = 62f;

        [Percentage(true)] public float opacity = 0.75f;

        [Rate(1, 5)] public int difficulty = 3;

        [TextArea(2, 8)]
        [Tooltip("Unity's own text area, not one of ours. It already grows between two and eight lines "
            + "and scrolls past that, which is why this package has nothing of its own for it.")]
        public string notes = "Grows with what you type.";

    }
}