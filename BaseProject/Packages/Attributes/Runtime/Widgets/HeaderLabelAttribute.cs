using System;
using JetBrains.Annotations;

namespace Base.AttributesPackage
{
    /// <summary>
    /// Shows the value of a property or parameterless method as read-only text in the component header.
    /// For the one fact worth seeing while the component is collapsed: a version, a state, a count.
    /// </summary>
    /// <remarks>
    /// <see cref="MeansImplicitUseAttribute"/> tells Rider that a decorated member is read even though
    /// nothing in the codebase references it.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method)]
    [MeansImplicitUse(ImplicitUseKindFlags.Access)]
    public sealed class HeaderLabelAttribute : Attribute
    {
        /// <summary>Width used when none is set explicitly.</summary>
        public const float DefaultWidth = 80f;

        /// <summary>Width of the label in pixels.</summary>
        public float Width { get; set; } = DefaultWidth;
    }
}