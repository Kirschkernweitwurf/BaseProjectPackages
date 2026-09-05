using Base.AttributesPackage.Editor;
using NUnit.Framework;

namespace Base.AttributesPackage.Tests
{
    /// <summary>
    /// Covers what turns a fixed attribute argument into a live one. Every title, info box, label,
    /// validation message and button caption in the package goes through here, so a reference that
    /// silently fails shows the raw <c>$Member</c> text in the inspector instead of a value.
    /// </summary>
    public sealed class ValueResolverTests
    {
        private const string Literal = "Plain text";
        private const string MissingReference = "$NoSuchMember";
        private const string Prefix = "$";

        private ValueResolverProbe _probe;

        /// <summary>A probe per test, since a resolved member reads whatever it currently holds.</summary>
        [SetUp]
        public void Prepare() => _probe = new ValueResolverProbe();

        /// <summary>The prefix is the whole convention, so it is what tells the two forms apart.</summary>
        [Test]
        public void OnlyAPrefixedValueIsAReference()
        {
            Assert.That(ValueResolver.IsMemberReference(Prefix + nameof(ValueResolverProbe.PublicText)), Is.True);
            Assert.That(ValueResolver.IsMemberReference(nameof(ValueResolverProbe.PublicText)), Is.False);
        }

        /// <summary>Nothing at all is not a reference, so it is not looked up.</summary>
        [Test]
        public void NothingIsNotAReference()
        {
            Assert.That(ValueResolver.IsMemberReference(null), Is.False);
            Assert.That(ValueResolver.IsMemberReference(string.Empty), Is.False);
        }

        /// <summary>The name is the argument without its prefix.</summary>
        [Test]
        public void TheNameIsTheArgumentWithoutThePrefix()
            => Assert.That(ValueResolver.MemberName(Prefix + nameof(ValueResolverProbe.PublicText)),
                Is.EqualTo(nameof(ValueResolverProbe.PublicText)));

        /// <summary>A literal has no prefix to strip, so it comes back whole.</summary>
        [Test]
        public void ALiteralKeepsItsWholeName()
            => Assert.That(ValueResolver.MemberName(Literal), Is.EqualTo(Literal));

        /// <summary>Text that names nothing is used as it was typed, which is the common case.</summary>
        [Test]
        public void ALiteralIsUsedAsWritten()
            => Assert.That(Resolve(Literal), Is.EqualTo(Literal));

        /// <summary>A field is read.</summary>
        [Test]
        public void AFieldReferenceIsRead()
            => Assert.That(Resolve(Prefix + nameof(ValueResolverProbe.PublicText)),
                Is.EqualTo(ValueResolverProbe.PublicValue));

        /// <summary>
        /// A private field is read too, so a reference can point at the serialized backing field it
        /// sits next to rather than needing a property put in front of it.
        /// </summary>
        [Test]
        public void APrivateFieldReferenceIsRead()
            => Assert.That(Resolve(Prefix + ValueResolverProbe.PrivateTextName),
                Is.EqualTo(ValueResolverProbe.PrivateValue));

        /// <summary>A property is read.</summary>
        [Test]
        public void APropertyReferenceIsRead()
            => Assert.That(Resolve(Prefix + nameof(ValueResolverProbe.TextProperty)),
                Is.EqualTo(ValueResolverProbe.PropertyValue));

        /// <summary>
        /// A method is called, which is what lets a caption say what it is about to do rather than
        /// naming a value that already exists.
        /// </summary>
        [Test]
        public void AMethodReferenceIsCalled()
            => Assert.That(Resolve(Prefix + nameof(ValueResolverProbe.TextMethod)),
                Is.EqualTo(ValueResolverProbe.MethodValue));

        /// <summary>
        /// A member holding nothing shows nothing, rather than the word a null reference would print
        /// if it were converted anyway.
        /// </summary>
        [Test]
        public void AMemberHoldingNothingResolvesToNothing()
            => Assert.That(Resolve(Prefix + nameof(ValueResolverProbe.MissingText)), Is.Empty);

        /// <summary>
        /// A reference nobody can resolve falls back to the argument as typed, prefix included. It
        /// reads as wrong in the inspector on purpose, which is how a typo gets noticed.
        /// </summary>
        [Test]
        public void AReferenceThatCannotBeResolvedFallsBackToTheArgument()
            => Assert.That(Resolve(MissingReference), Is.EqualTo(MissingReference));

        /// <summary>Nothing to read from means nothing was read.</summary>
        [Test]
        public void ReadingWithoutATypeOrAnOwnerFails()
        {
            Assert.That(ValueResolver.TryRead(null, _probe, nameof(ValueResolverProbe.PublicText), out object _),
                Is.False);

            Assert.That(ValueResolver.TryRead(typeof(ValueResolverProbe), null,
                nameof(ValueResolverProbe.PublicText), out object _), Is.False);
        }

        /// <summary>A member with no name is not a member.</summary>
        [Test]
        public void ReadingWithoutANameFails()
            => Assert.That(ValueResolver.TryRead(typeof(ValueResolverProbe), _probe, string.Empty, out object _),
                Is.False);

        /// <summary>A method that returns nothing has no value to show, so it is refused.</summary>
        [Test]
        public void AMethodThatReturnsNothingIsRefused()
            => Assert.That(Read(nameof(ValueResolverProbe.VoidMethod)), Is.False);

        /// <summary>
        /// A method that needs an argument cannot be called, since the attribute carries only a name.
        /// </summary>
        [Test]
        public void AMethodThatNeedsAnArgumentIsRefused()
            => Assert.That(Read(nameof(ValueResolverProbe.MethodWithArgument)), Is.False);

        /// <summary>Resolves an attribute argument against the probe.</summary>
        private string Resolve(string value) => ValueResolver.Text(Context(), value);

        /// <summary>Whether the named member could be read off the probe.</summary>
        private bool Read(string member)
            => ValueResolver.TryRead(typeof(ValueResolverProbe), _probe, member, out object _);

        /// <summary>
        /// A context carrying only the owner, since resolving a text argument reads nothing else off
        /// it. This is also the shape it has inside a nested serializable type.
        /// </summary>
        private MemberContext Context()
            => new(null, null, null, typeof(ValueResolverProbe), _probe, null, null);
    }
}