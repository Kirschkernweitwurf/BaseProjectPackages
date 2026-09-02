using UnityEngine;

namespace Base.AttributesPackage.Samples
{
    /// <summary>A field locked while playing.</summary>
    [AttributeSample(typeof(DisableInPlayModeAttribute), EAttributeCategory.Conditions,
        Description = "Locks the field while the editor is playing, for setup that is read once at "
            + "startup and would either be ignored or break something if it changed mid-run.",
        Requirements = "Enter play mode to see the field lock.",
        Variations = new[]
        {
            "EnableInPlayMode is the inverse, for a value only worth tuning while running."
        })]
    internal sealed class DisableInPlayModeSample : ScriptableObject
    {
        [DisableInPlayMode]
        public string lockedDuringPlay = "Editable now, locked once you press play";
    }
}