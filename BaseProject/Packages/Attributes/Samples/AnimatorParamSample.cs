using UnityEngine;

namespace Base.AttributesPackage.Samples
{
    /// <summary>A parameter name picked from an Animator.</summary>
    [AttributeSample(typeof(AnimatorParamAttribute), EAttributeCategory.Pickers,
        Description = "Lists the parameters on an assigned Animator, so a trigger name cannot be misspelled and "
            + "quietly do nothing.",
        Requirements = "Assign the Animator field first, and the Animator needs a controller with parameters on it. An "
            + "asset can only reference assets, so the Animator has to come from a prefab.",
        Variations = new[]
        {
            "A second argument narrows the list to one parameter type.",
            "The field can be a string to store the name, or an int to store the hash."
        })]
    internal sealed class AnimatorParamSample : ScriptableObject
    {
        [Tooltip("Assign a prefab Animator here first. The picker below reads from it.")]
        public Animator animator;

        [AnimatorParam(nameof(animator))]
        [Tooltip("Lists the parameters on the Animator above.")]
        public string animatorParameter;
    }
}