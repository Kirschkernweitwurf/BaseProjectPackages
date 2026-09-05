using System;
using Base.AttributesPackage.Editor.Core;
using NUnit.Framework;

namespace Base.AttributesPackage.Tests
{
    /// <summary>
    /// Covers how an attribute names itself in the inspector. Every message the package shows derives
    /// its name from the type rather than repeating it as a literal, which is the only reason renaming
    /// an attribute does not leave stale text behind in a help box somewhere.
    /// </summary>
    public sealed class AttributeNamesTests
    {
        private const string Requirement = "a string";

        /// <summary>The suffix is noise in a label, so it is dropped.</summary>
        [Test]
        public void TheAttributeSuffixIsTrimmed()
            => Assert.That(AttributeNames.Display<TagAttribute>(), Is.EqualTo("Tag"));

        /// <summary>
        /// Only a trailing suffix counts, or an attribute whose name merely contains the word would
        /// come out mangled.
        /// </summary>
        [Test]
        public void ANameWithoutTheSuffixIsLeftAlone()
            => Assert.That(AttributeNames.Display(typeof(NotSuffixed)), Is.EqualTo(nameof(NotSuffixed)));

        /// <summary>
        /// The generic and the type form have to agree, since diagnostics use whichever they happen to
        /// hold and the same attribute must not read two ways.
        /// </summary>
        [Test]
        public void BothFormsNameTheSameAttributeTheSameWay()
            => Assert.That(AttributeNames.Display<RequiredAttribute>(),
                Is.EqualTo(AttributeNames.Display(typeof(RequiredAttribute))));

        /// <summary>
        /// A usage hint reads as the attribute is written in code, brackets included, so somebody can
        /// copy it out of the message.
        /// </summary>
        [Test]
        public void AUsageHintReadsAsTheAttributeIsWritten()
            => Assert.That(AttributeNames.Usage<TitleAttribute>(Requirement),
                Is.EqualTo($"Use [Title] with {Requirement}."));

        /// <summary>An attribute whose name is nothing but the suffix has nothing left to show.</summary>
        [Test]
        public void ANameThatIsOnlyTheSuffixCollapsesToNothing()
            => Assert.That(AttributeNames.Display(typeof(Attribute)), Is.Empty);

        /// <summary>An attribute that does not end in the suffix, for the trimming test above.</summary>
        private sealed class NotSuffixed : Attribute { }
    }
}