using System;

namespace Base.AttributePackage.Editor.Windows.AttributeSamples
{
    /// <summary>
    /// Marks a ScriptableObject as a source of samples. Every field and method in it that carries a
    /// package attribute becomes one entry in the samples window.
    /// </summary>
    /// <remarks>
    /// Discovery is by attribute rather than by a list somewhere, so adding a sample is adding a field
    /// and nothing else. A list would be a second place to forget.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class)]
    internal sealed class AttributeSampleAttribute : Attribute
    {
        /// <summary>Group the samples in this type are listed under.</summary>
        public string Category { get; }

        /// <summary>Creates the attribute.</summary>
        /// <param name="category">Group the samples in this type are listed under.</param>
        public AttributeSampleAttribute(string category) => Category = category;
    }
}