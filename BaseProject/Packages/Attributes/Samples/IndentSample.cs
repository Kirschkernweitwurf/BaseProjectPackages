using UnityEngine;

namespace Base.AttributePackage.Samples
{
    /// <summary>A field pushed one step to the right.</summary>
    [AttributeSample(typeof(IndentAttribute), EAttributeCategory.Layout,
        Description = "Pushes the field one step to the right, which reads as belonging to the field "
            + "above it. Cheaper than a foldout when there are only one or two fields to subordinate.",
        Requirements = "Nothing.",
        Variations = new[]
        {
            "Indent() for one step.",
            "Indent(2) for two.",
            "A negative number pulls the field back to the left instead, out of an indent it inherited."
        })]
    internal sealed class IndentSample : ScriptableObject
    {
        [Tooltip("The field the indented ones below belong to.")]
        public bool useOverride = true;

        [Indent]
        [Tooltip("One step in.")]
        public float first = 1f;

        [Indent(2)]
        [Tooltip("Two steps in.")]
        public float second = 2f;

        [Indent(-1)]
        [Tooltip("One step out, which only shows inside a section that already indents its fields.")]
        public float pulledBack = 3f;
    }
}