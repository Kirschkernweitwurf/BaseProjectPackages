using UnityEngine;

namespace Base.AttributePackage.Samples
{
    /// <summary>An animation curve locked to a box.</summary>
    [AttributeSample(typeof(CurveRangeAttribute), EAttributeCategory.Widgets,
        Description = "Locks an animation curve to a range and optionally tints it, so a curve that something reads as "
            + "normalized cannot wander outside the box it is read in.",
        Requirements = "The field has to be an AnimationCurve.",
        Variations = new[]
        {
            "CurveRange(min, max) for a square box.",
            "CurveRange(minX, minY, maxX, maxY) for a box with different bounds per axis.",
            "A color argument tints the curve."
        })]
    internal sealed class CurveRangeSample : ScriptableObject
    {
        [CurveRange(0f, 0f, 1f, 1f, EColor.Magenta)]
        [Tooltip("Locked to the unit square. Try to drag a key outside it.")]
        public AnimationCurve curve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
    }
}