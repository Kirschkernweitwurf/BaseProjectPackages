using UnityEngine;

namespace Base.AttributesPackage.Samples
{
    /// <summary>The script row hidden.</summary>
    [HideMonoScript]
    [AttributeSample(typeof(HideMonoScriptAttribute), EAttributeCategory.Layout,
        Description = "Hides the read-only script row at the top of the inspector, for a component whose type is "
            + "obvious and whose first real field is what the reader wants to see.",
        Requirements = "Goes on the class, not on a field.",
        Variations = new[]
        {
            "Nothing to configure. It is either on the type or it is not."
        })]
    internal sealed class HideMonoScriptSample : ScriptableObject
    {
        [Tooltip("The first thing in this inspector, since the script row above it is hidden.")]
        public string first = "No script row above me";
    }
}