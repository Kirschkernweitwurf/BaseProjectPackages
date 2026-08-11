using System;
using UnityEngine;

namespace Base.AttributePackage.Editor.Windows.AttributeTroubleshoot.Showcase
{
    /// <summary>
    /// Nested block used by the showcase to confirm the pipeline descends into plain serializable types
    /// and still honors the attributes on their fields.
    /// </summary>
    [Serializable]
    internal sealed class ShowcaseNestedSettings
    {
        /// <summary>Shows that a title works inside a nested type, not only on the object itself.</summary>
        [Title("Nested block", "#83DBC6")]
        public string label = "Nested";

        /// <summary>Shows that validation reaches this depth. Clear it to see the error box.</summary>
        [Required] public Material anchor;

        /// <summary>Drives the conditional field below it, within this nested block.</summary>
        public bool useOverride;

        /// <summary>
        /// Visible only while the toggle above is on, which proves conditions resolve against the nested
        /// object rather than against the asset that owns it.
        /// </summary>
        [ShowIf(nameof(useOverride))] public float overrideValue = 1f;

        /// <summary>Shows a widget attribute inside a nested type.</summary>
        [MinMaxSlider(0f, 10f)] public Vector2 nestedRange = new(2f, 8f);
    }
}