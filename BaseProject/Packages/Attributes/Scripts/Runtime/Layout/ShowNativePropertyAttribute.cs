using System;

namespace Base.AttributePackage.Layout
{
    /// <summary>
    /// Shows the value of a readable property as a read-only value in the inspector.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class ShowNativePropertyAttribute : Attribute { }
}