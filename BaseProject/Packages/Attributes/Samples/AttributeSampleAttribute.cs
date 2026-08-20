using System;

namespace Base.AttributePackage.Samples
{
    /// <summary>
    /// Marks a ScriptableObject as the sample for exactly one attribute, and carries everything the
    /// reference page shows about it.
    /// </summary>
    /// <remarks>
    /// One attribute per sample, which is what lets the page draw the whole object and print the whole
    /// class. A sample demonstrating several attributes could do neither: the preview would show fields
    /// the reader did not ask about, and the snippet would have to be cut back out of the file by
    /// guesswork.
    /// <para>
    /// The attribute is named by <c>typeof</c> rather than by string, so renaming it moves the sample
    /// with it and an attribute that no longer exists is a compile error rather than a missing page.
    /// </para>
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class)]
    internal sealed class AttributeSampleAttribute : Attribute
    {
        /// <summary>The attribute this sample demonstrates.</summary>
        public Type AttributeType { get; }

        /// <summary>The group the sample is listed under.</summary>
        public EAttributeCategory Category { get; }

        /// <summary>What the attribute does, in a sentence or two.</summary>
        public string Description { get; set; }

        /// <summary>What the reader has to set up before the sample does anything.</summary>
        public string Requirements { get; set; }

        /// <summary>Something worth knowing that is not another way of writing it.</summary>
        public string Info { get; set; }

        /// <summary>The other ways the attribute can be written, one line each.</summary>
        public string[] Variations { get; set; }

        /// <summary>Creates the sample marker.</summary>
        /// <param name="attributeType">The attribute this sample demonstrates.</param>
        /// <param name="category">The group the sample is listed under.</param>
        public AttributeSampleAttribute(Type attributeType, EAttributeCategory category)
        {
            AttributeType = attributeType;
            Category = category;
        }
    }
}