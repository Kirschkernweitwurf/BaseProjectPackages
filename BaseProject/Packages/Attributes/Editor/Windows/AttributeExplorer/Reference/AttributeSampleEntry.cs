using System;
using Base.AttributePackage.Samples;

namespace Base.AttributePackage.Editor.Drawers.Windows.AttributeExplorer.Reference
{
    /// <summary>
    /// One attribute and the sample that demonstrates it.
    /// </summary>
    internal readonly struct AttributeSampleEntry
    {
        private static readonly string[] NoVariations = Array.Empty<string>();

        /// <summary>Name shown in the list, without the Attribute suffix.</summary>
        internal string Title { get; }

        /// <summary>The group the entry is listed under.</summary>
        internal EAttributeCategory Category { get; }

        /// <summary>The group name as it is shown.</summary>
        internal string CategoryName { get; }

        /// <summary>The sample type to instantiate and draw.</summary>
        internal Type SampleType { get; }

        /// <summary>What the attribute does.</summary>
        internal string Description { get; }

        /// <summary>What has to be set up before the sample does anything.</summary>
        internal string Requirements { get; }

        /// <summary>Something worth knowing that is not another way of writing it.</summary>
        internal string Info { get; }

        /// <summary>The other ways the attribute can be written.</summary>
        internal string[] Variations { get; }

        /// <summary>Creates an entry.</summary>
        /// <param name="title">Name shown in the list.</param>
        /// <param name="category">The group the entry is listed under.</param>
        /// <param name="categoryName">The group name as it is shown.</param>
        /// <param name="sampleType">The sample type to draw.</param>
        /// <param name="description">What the attribute does.</param>
        /// <param name="requirements">What has to be set up first.</param>
        /// <param name="variations">The other ways the attribute can be written.</param>
        /// <param name="info">Something worth knowing that is not a variation.</param>
        internal AttributeSampleEntry(string title, EAttributeCategory category, string categoryName,
            Type sampleType, string description, string requirements, string info, string[] variations)
        {
            Title = title;
            Category = category;
            CategoryName = categoryName;
            SampleType = sampleType;
            Description = description ?? string.Empty;
            Requirements = requirements ?? string.Empty;
            Info = info ?? string.Empty;
            Variations = variations ?? NoVariations;
        }
    }
}