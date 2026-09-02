using UnityEngine;

namespace Base.AttributesPackage.Samples
{
    /// <summary>A field visible only while playing.</summary>
    [AttributeSample(typeof(ShowInPlayModeAttribute), EAttributeCategory.Conditions,
        Description = "Shows the field only while the editor is playing, for runtime state that means nothing while "
            + "stopped.",
        Requirements = "Enter play mode to see the field appear.",
        Variations = new[]
        {
            "HideInPlayMode is the inverse, for setup that cannot be changed once running."
        })]
    internal sealed class ShowInPlayModeSample : ScriptableObject
    {
        [ShowInPlayMode]
        [Tooltip("Only visible while the editor is playing.")]
        public string playModeOnly = "Play mode only";
    }
}