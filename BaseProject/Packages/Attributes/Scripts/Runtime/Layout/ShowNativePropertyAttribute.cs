using System;
using JetBrains.Annotations;

namespace Base.AttributePackage
{
    /// <summary>
    /// Shows the value of a readable property as a read-only value in the inspector.
    /// </summary>
    /// <remarks>
    /// <see cref="MeansImplicitUseAttribute"/> tells Rider that a decorated property is read even though
    /// nothing in the codebase references it.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Property)]
    [MeansImplicitUse(ImplicitUseKindFlags.Access)]
    public sealed class ShowNativePropertyAttribute : Attribute { }
}
