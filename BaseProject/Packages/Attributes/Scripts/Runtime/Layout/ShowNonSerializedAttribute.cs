using System;
using JetBrains.Annotations;

namespace Base.AttributePackage
{
    /// <summary>
    /// Shows a non-serialized field as a read-only value in the inspector.
    /// </summary>
    /// <remarks>
    /// <see cref="MeansImplicitUseAttribute"/> tells Rider that a decorated field is read even though
    /// nothing in the codebase references it.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Field)]
    [MeansImplicitUse(ImplicitUseKindFlags.Access)]
    public sealed class ShowNonSerializedAttribute : Attribute { }
}