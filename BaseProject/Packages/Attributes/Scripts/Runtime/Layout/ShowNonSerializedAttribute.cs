using System;

namespace Base.AttributePackage.Layout
{
    /// <summary>
    /// Shows a non-serialized field as a read-only value in the inspector.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class ShowNonSerializedAttribute : Attribute { }
}