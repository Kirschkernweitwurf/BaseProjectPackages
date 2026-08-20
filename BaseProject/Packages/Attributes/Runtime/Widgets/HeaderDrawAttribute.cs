using System;
using JetBrains.Annotations;

namespace Base.AttributePackage
{
    /// <summary>
    /// Hands the decorated method a rect in the component header and lets it draw whatever it likes. The
    /// escape hatch for anything the button and the label do not cover.
    /// </summary>
    /// <remarks>
    /// The method has to take a single <c>Rect</c> and return nothing. It runs during the header's own
    /// GUI pass, so it may use the immediate mode GUI directly.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Method)]
    [MeansImplicitUse(ImplicitUseKindFlags.Access)]
    public sealed class HeaderDrawAttribute : Attribute
    {
        /// <summary>Width used when none is set explicitly.</summary>
        public const float DefaultWidth = 60f;

        /// <summary>Width of the rect handed to the method.</summary>
        public float Width { get; set; } = DefaultWidth;
    }
}