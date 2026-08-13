using UnityEngine;

namespace Base.AttributePackage.Samples
{
    /// <summary>A state name picked from an Animator controller.</summary>
    [AttributeSample(typeof(AnimatorStateAttribute), EAttributeCategory.Pickers,
        Description = "Lists the states on the controller of an assigned Animator, prefixed by their layer.",
        Requirements = "Assign the Animator field first, and the Animator needs a controller. An asset can only "
            + "reference assets, so the Animator has to come from a prefab.",
        Variations = new[]
        {
            "Nothing to configure beyond the source field."
        })]
    internal sealed class AnimatorStateSample : ScriptableObject
    {
        [Tooltip("Assign a prefab Animator here first. The picker below reads from it.")]
        public Animator animator;

        [AnimatorState(nameof(animator))]
        [Tooltip("Lists the states on the controller of the Animator above.")]
        public string animatorState;
    }
}