using UnityEngine;

namespace Base.AttributesPackage.Samples
{
    /// <summary>A field editable only while playing.</summary>
    [AttributeSample(typeof(EnableInPlayModeAttribute), EAttributeCategory.Conditions,
        Description = "Leaves the field greyed out until the editor is playing, for values worth tuning live but "
            + "meaningless to set beforehand.",
        Requirements = "Enter play mode to make the field editable.",
        Variations = new[]
        {
            "DisableInPlayMode is the inverse."
        })]
    internal sealed class EnableInPlayModeSample : ScriptableObject
    {
        [EnableInPlayMode]
        [Tooltip("Editable only during play.")]
        public float tunable = 1f;
    }
}