using System;

namespace Base.AttributePackage.Editor.Windows.AttributeSamples
{
    /// <summary>
    /// One attribute as it is demonstrated by one member of one sample.
    /// </summary>
    /// <remarks>
    /// The list is per attribute rather than per sample file, because looking something up starts from
    /// the attribute's name and nothing else. Which file it happens to live in is an implementation
    /// detail of the samples, not something a reader should have to guess at.
    /// </remarks>
    internal readonly struct AttributeSampleEntry
    {
        /// <summary>Name shown in the list, without the Attribute suffix.</summary>
        public readonly string Title;

        /// <summary>Group the entry is listed under.</summary>
        public readonly string Category;

        /// <summary>The sample type to instantiate and draw.</summary>
        public readonly Type SampleType;

        /// <summary>Name of the field or method that carries the attribute.</summary>
        public readonly string MemberName;

        /// <summary>One line explaining the attribute, taken from the member's tooltip.</summary>
        public readonly string Description;

        /// <summary>Creates an entry.</summary>
        /// <param name="title">Name shown in the list.</param>
        /// <param name="category">Group the entry is listed under.</param>
        /// <param name="sampleType">The sample type to draw.</param>
        /// <param name="memberName">The member carrying the attribute.</param>
        /// <param name="description">One line explaining the attribute.</param>
        public AttributeSampleEntry(string title, string category, Type sampleType, string memberName,
            string description)
        {
            Title = title;
            Category = category;
            SampleType = sampleType;
            MemberName = memberName;
            Description = description;
        }
    }
}