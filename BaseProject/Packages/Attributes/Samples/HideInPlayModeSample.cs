using UnityEngine;

namespace Base.AttributesPackage.Samples
{
    /// <summary>A field hidden while playing.</summary>
    [AttributeSample(typeof(HideInPlayModeAttribute), EAttributeCategory.Conditions,
        Description = "Hides the field while the editor is playing, for setup that is read once at startup and cannot "
            + "usefully be changed afterwards.",
        Requirements = "Enter play mode to see the field disappear.",
        Variations = new[]
        {
            "ShowInPlayMode is the inverse."
        })]
    internal sealed class HideInPlayModeSample : ScriptableObject
    {
        [HideInPlayMode]
        [Tooltip("Disappears while the editor is playing.")]
        public string editModeOnly = "Edit mode only";
    }
}