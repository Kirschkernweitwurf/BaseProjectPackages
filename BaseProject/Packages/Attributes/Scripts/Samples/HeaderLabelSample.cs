using UnityEngine;

namespace Base.AttributePackage.Samples
{
    /// <summary>A value shown in the component header.</summary>
    [AttributeSample(typeof(HeaderLabelAttribute), EAttributeCategory.Widgets,
        Description = "Puts a short read-only value in the component title bar, for the one number that says at a "
            + "glance whether the component is set up.",
        Requirements = "Drawn by the real Inspector, which is what owns the component title bar. Use the button below "
            + "to put this sample into your scene, then look at it in the Inspector.",
        Variations = new[]
        {
            "Goes on a property or a parameterless method.",
            "Width sets how much room it takes."
        })]
    internal sealed class HeaderLabelSample : MonoBehaviour
    {
        [Tooltip("The header label reads this field.")]
        public int waypointCount = 3;

        /// <summary>Shown in the component title bar.</summary>
        [HeaderLabel]
        public string Summary => $"{waypointCount} waypoints";
    }
}