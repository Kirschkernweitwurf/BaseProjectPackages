using UnityEngine;

namespace Base.AttributesPackage.Editor.Windows.AttributeExplorer.Troubleshoot.Samples
{
    /// <summary>
    /// Attributes sitting on field types their drawers cannot handle, so the samples tab can show what an
    /// attribute that quietly does nothing looks like.
    /// </summary>
    [TroubleshootSample]
    internal sealed class SampleFieldTypeIssues
    {
        /// <summary>An int can never be null, so the requirement can never fire.</summary>
        [Required] public int requiredOnNumber;

        /// <summary>Tags are strings, so an int cannot hold one.</summary>
        [Tag] public int tagOnNumber;

        /// <summary>A two-handled slider needs a Vector2 to store both ends.</summary>
        [MinMaxSlider(0f, 10f)] public float sliderOnFloat;

        /// <summary>A mask field needs an enum to read the flags from.</summary>
        /// <summary>An inline inspector needs an asset reference to draw.</summary>
        [Expandable] public string expandableOnString;

        /// <summary>A curve range needs an AnimationCurve.</summary>
        [CurveRange(0f, 1f)] public float curveOnFloat;

        /// <summary>Snapping to a power of two needs an int.</summary>
        [PowerOfTwo] public string powerOfTwoOnString;

        /// <summary>An asset preview needs an object reference to preview.</summary>
        [ShowAssetPreview] public Vector3 previewOnVector;
    }
}