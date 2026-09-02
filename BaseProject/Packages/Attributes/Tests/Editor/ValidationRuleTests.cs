using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Base.AttributesPackage.Tests
{
    /// <summary>
    /// Covers the rules behind the validation attributes. They run outside the inspector, against a
    /// live object rather than a serialized property, which is what lets the overview window report a
    /// prefab nobody has open. Each rule has to stay silent on a field it does not own.
    /// </summary>
    public sealed class ValidationRuleTests
    {
        private readonly List<Object> _created = new();

        private ValidationProbe _probe;

        /// <summary>Every test starts from an empty probe.</summary>
        [SetUp]
        public void Build() => _probe = new ValidationProbe();

        /// <summary>Assets built in a test are not saved anywhere, so they are destroyed here.</summary>
        [TearDown]
        public void Release()
        {
            foreach (Object asset in _created)
            {
                if (asset != null)
                    Object.DestroyImmediate(asset);
            }

            _created.Clear();
        }

        /// <summary>An empty required reference is reported with the default reason.</summary>
        [Test]
        public void AnEmptyRequiredReferenceIsReported()
        {
            RequiredRule rule = new();

            Assert.That(rule.IsViolated(Field(nameof(ValidationProbe.RequiredAsset)), _probe, out string reason),
                Is.True);

            Assert.That(reason, Is.EqualTo(RequiredAttribute.DefaultReason));
        }

        /// <summary>A filled required reference is fine.</summary>
        [Test]
        public void AFilledRequiredReferenceIsFine()
        {
            RequiredRule rule = new();
            _probe.RequiredAsset = Asset();

            Assert.That(rule.IsViolated(Field(nameof(ValidationProbe.RequiredAsset)), _probe, out string _),
                Is.False);
        }

        /// <summary>A custom message replaces the default reason.</summary>
        [Test]
        public void ACustomMessageReplacesTheDefaultReason()
        {
            RequiredRule rule = new();

            rule.IsViolated(Field(nameof(ValidationProbe.RequiredWithMessage)), _probe, out string reason);

            Assert.That(reason, Is.EqualTo(ValidationProbe.CustomMessage));
        }

        /// <summary>A field without the attribute is none of the rule's business.</summary>
        [Test]
        public void AFieldWithoutTheAttributeIsIgnored()
        {
            RequiredRule rule = new();

            Assert.That(rule.IsViolated(Field(nameof(ValidationProbe.Unmarked)), _probe, out string _), Is.False);
        }

        /// <summary>
        /// A required number is not a reference, so the rule stays out of it rather than reporting a
        /// zero as missing.
        /// </summary>
        [Test]
        public void ARequiredValueThatIsNotAReferenceIsIgnored()
        {
            RequiredRule rule = new();

            Assert.That(rule.IsViolated(Field(nameof(ValidationProbe.RequiredNumber)), _probe, out string _),
                Is.False);
        }

        /// <summary>An empty string is reported.</summary>
        [Test]
        public void AnEmptyTextIsReported()
        {
            NotNullOrEmptyRule rule = new();

            Assert.That(rule.IsViolated(Field(nameof(ValidationProbe.RequiredText)), _probe, out string reason),
                Is.True);

            Assert.That(reason, Is.EqualTo(NotNullOrEmptyAttribute.DefaultReason));

            _probe.RequiredText = string.Empty;

            Assert.That(rule.IsViolated(Field(nameof(ValidationProbe.RequiredText)), _probe, out string _), Is.True);
        }

        /// <summary>A string carrying something is fine.</summary>
        [Test]
        public void ATextCarryingSomethingIsFine()
        {
            NotNullOrEmptyRule rule = new();
            _probe.RequiredText = "Something";

            Assert.That(rule.IsViolated(Field(nameof(ValidationProbe.RequiredText)), _probe, out string _), Is.False);
        }

        /// <summary>A list with nothing in it is reported.</summary>
        [Test]
        public void AnEmptyListIsReported()
        {
            NotNullOrEmptyRule rule = new();
            _probe.RequiredList = new List<string>();

            Assert.That(rule.IsViolated(Field(nameof(ValidationProbe.RequiredList)), _probe, out string _), Is.True);
        }

        /// <summary>A list holding something is fine.</summary>
        [Test]
        public void AListHoldingSomethingIsFine()
        {
            NotNullOrEmptyRule rule = new();
            _probe.RequiredList = new List<string> { "Entry" };

            Assert.That(rule.IsViolated(Field(nameof(ValidationProbe.RequiredList)), _probe, out string _), Is.False);
        }

        /// <summary>A repeated entry is reported, naming where it repeats.</summary>
        [Test]
        public void ARepeatedEntryIsReported()
        {
            UniqueRule rule = new();
            _probe.UniqueEntries = new List<string> { "Alpha", "Beta", "Alpha" };

            Assert.That(rule.IsViolated(Field(nameof(ValidationProbe.UniqueEntries)), _probe, out string reason),
                Is.True);

            Assert.That(reason, Does.Contain("0"));
            Assert.That(reason, Does.Contain("2"));
        }

        /// <summary>A list where everything differs is fine.</summary>
        [Test]
        public void AListWithoutRepeatsIsFine()
        {
            UniqueRule rule = new();
            _probe.UniqueEntries = new List<string> { "Alpha", "Beta" };

            Assert.That(rule.IsViolated(Field(nameof(ValidationProbe.UniqueEntries)), _probe, out string _), Is.False);
        }

        /// <summary>Something that is not a list is none of the unique rule's business.</summary>
        [Test]
        public void SomethingThatIsNotAListIsIgnored()
        {
            UniqueRule rule = new();
            _probe.UniqueText = "Alpha";

            Assert.That(rule.IsViolated(Field(nameof(ValidationProbe.UniqueText)), _probe, out string _), Is.False);
        }

        /// <summary>
        /// A conditional reference is only required while its condition holds, so a configuration that
        /// never uses the field does not report an error for it.
        /// </summary>
        [Test]
        public void AConditionalReferenceIsQuietWhileItsConditionIsOff()
        {
            RequiredIfRule rule = new();
            _probe.NeedsAsset = false;

            Assert.That(rule.IsViolated(Field(nameof(ValidationProbe.ConditionalAsset)), _probe, out string _),
                Is.False);
        }

        /// <summary>Once the condition holds, an empty conditional reference is reported.</summary>
        [Test]
        public void AConditionalReferenceIsReportedOnceItsConditionHolds()
        {
            RequiredIfRule rule = new();
            _probe.NeedsAsset = true;

            Assert.That(rule.IsViolated(Field(nameof(ValidationProbe.ConditionalAsset)), _probe, out string reason),
                Is.True);

            Assert.That(reason, Is.EqualTo(RequiredIfAttribute.DefaultReason));
        }

        /// <summary>A filled conditional reference is fine even while its condition holds.</summary>
        [Test]
        public void AFilledConditionalReferenceIsFine()
        {
            RequiredIfRule rule = new();
            _probe.NeedsAsset = true;
            _probe.ConditionalAsset = Asset();

            Assert.That(rule.IsViolated(Field(nameof(ValidationProbe.ConditionalAsset)), _probe, out string _),
                Is.False);
        }

        /// <summary>An object of the wrong type is reported, naming the type it should have been.</summary>
        [Test]
        public void AnObjectOfTheWrongTypeIsReported()
        {
            MustImplementRule rule = new();
            _probe.WrongType = Asset();

            Assert.That(rule.IsViolated(Field(nameof(ValidationProbe.WrongType)), _probe, out string reason),
                Is.True);

            Assert.That(reason, Does.Contain(nameof(Texture2D)));
        }

        /// <summary>An object that satisfies the required type is fine.</summary>
        [Test]
        public void AnObjectOfTheRightTypeIsFine()
        {
            MustImplementRule rule = new();
            _probe.RightType = Asset();

            Assert.That(rule.IsViolated(Field(nameof(ValidationProbe.RightType)), _probe, out string _), Is.False);
        }

        /// <summary>
        /// An empty reference is the job of the required rule, so the type rule leaves it alone rather
        /// than reporting the same field twice.
        /// </summary>
        [Test]
        public void AnEmptyReferenceIsNotATypeViolation()
        {
            MustImplementRule rule = new();

            Assert.That(rule.IsViolated(Field(nameof(ValidationProbe.WrongType)), _probe, out string _), Is.False);
        }

        private static FieldInfo Field(string name) => typeof(ValidationProbe).GetField(name);

        private GameObject Asset()
        {
            GameObject created = new(nameof(ValidationProbe));

            _created.Add(created);

            return created;
        }
    }
}