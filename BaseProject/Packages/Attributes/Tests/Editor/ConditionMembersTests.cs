using System;
using NUnit.Framework;

namespace Base.AttributesPackage.Tests
{
    /// <summary>
    /// Covers how a condition reads named members off a live object. The rules run outside the
    /// inspector, so this is the only way they can tell whether a conditional requirement applies. A
    /// member that cannot be resolved counts as true, so a typo never silently switches a rule off.
    /// </summary>
    public sealed class ConditionMembersTests
    {
        private ConditionProbe _probe;

        /// <summary>Every test starts from a probe with everything switched off.</summary>
        [SetUp]
        public void Build() => _probe = new ConditionProbe();

        /// <summary>A public field is read.</summary>
        [Test]
        public void APublicFieldIsRead()
        {
            Assert.That(Evaluate(EConditionMode.All, nameof(ConditionProbe.PublicFlag)), Is.False);

            _probe.PublicFlag = true;

            Assert.That(Evaluate(EConditionMode.All, nameof(ConditionProbe.PublicFlag)), Is.True);
        }

        /// <summary>A private field is read too, so a condition can point at a serialized backing field.</summary>
        [Test]
        public void APrivateFieldIsRead()
        {
            Assert.That(Evaluate(EConditionMode.All, ConditionProbe.PrivateFlagName), Is.False);

            _probe.SetPrivateFlag(true);

            Assert.That(Evaluate(EConditionMode.All, ConditionProbe.PrivateFlagName), Is.True);
        }

        /// <summary>A property is read.</summary>
        [Test]
        public void APropertyIsRead()
        {
            _probe.PropertyFlag = true;

            Assert.That(Evaluate(EConditionMode.All, nameof(ConditionProbe.PropertyFlag)), Is.True);
        }

        /// <summary>A method is called.</summary>
        [Test]
        public void AMethodIsCalled()
        {
            _probe.PublicFlag = true;

            Assert.That(Evaluate(EConditionMode.All, nameof(ConditionProbe.MethodFlag)), Is.True);
        }

        /// <summary>Every member has to hold when they are combined with all.</summary>
        [Test]
        public void CombiningWithAllNeedsEveryMember()
        {
            _probe.PublicFlag = true;

            Assert.That(Evaluate(EConditionMode.All, nameof(ConditionProbe.PublicFlag),
                nameof(ConditionProbe.PropertyFlag)), Is.False);

            _probe.PropertyFlag = true;

            Assert.That(Evaluate(EConditionMode.All, nameof(ConditionProbe.PublicFlag),
                nameof(ConditionProbe.PropertyFlag)), Is.True);
        }

        /// <summary>One member is enough when they are combined with any.</summary>
        [Test]
        public void CombiningWithAnyNeedsOneMember()
        {
            Assert.That(Evaluate(EConditionMode.Any, nameof(ConditionProbe.PublicFlag),
                nameof(ConditionProbe.PropertyFlag)), Is.False);

            _probe.PropertyFlag = true;

            Assert.That(Evaluate(EConditionMode.Any, nameof(ConditionProbe.PublicFlag),
                nameof(ConditionProbe.PropertyFlag)), Is.True);
        }

        /// <summary>
        /// A member nobody can find counts as true, matching the inspector, so a renamed field never
        /// silently suppresses the rule that points at it.
        /// </summary>
        [Test]
        public void AMemberThatCannotBeFoundCountsAsTrue()
            => Assert.That(Evaluate(EConditionMode.All, "NoSuchMember"), Is.True);

        /// <summary>A member that is not a boolean cannot answer the question, so it counts as true.</summary>
        [Test]
        public void AMemberThatIsNotABooleanCountsAsTrue()
            => Assert.That(Evaluate(EConditionMode.All, nameof(ConditionProbe.Count)), Is.True);

        /// <summary>A method that needs an argument cannot be called, so it counts as true.</summary>
        [Test]
        public void AMethodThatNeedsAnArgumentCountsAsTrue()
            => Assert.That(Evaluate(EConditionMode.All, nameof(ConditionProbe.MethodWithArgument)), Is.True);

        /// <summary>No members at all means there is nothing to hold back the rule.</summary>
        [Test]
        public void NoMembersMeansTheConditionHolds()
        {
            Assert.That(ConditionMembers.Evaluate(_probe, EConditionMode.All, null), Is.True);
            Assert.That(ConditionMembers.Evaluate(_probe, EConditionMode.All, Array.Empty<string>()), Is.True);
        }

        /// <summary>No object to read from means there is nothing to hold back the rule either.</summary>
        [Test]
        public void NoObjectMeansTheConditionHolds()
            => Assert.That(ConditionMembers.Evaluate(null, EConditionMode.All,
                new[] { nameof(ConditionProbe.PublicFlag) }), Is.True);

        private bool Evaluate(EConditionMode mode, params string[] members)
            => ConditionMembers.Evaluate(_probe, mode, members);
    }
}