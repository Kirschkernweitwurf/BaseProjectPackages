using Base.UtilityPackage.Editor.Serialization;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Base.UtilityPackage.Tests
{
    /// <summary>
    /// Covers what a date or duration row decides it was pointed at. Every such drawer resolves
    /// through here rather than deciding for itself, so a shape wrongly accepted writes a tick count
    /// into a field that cannot hold one, and a shape wrongly refused turns the row into a hint.
    /// </summary>
    public sealed class TickPropertyTests
    {
        private TickPropertyProbe _probe;
        private SerializedObject _serialized;

        /// <summary>A probe and one serialized view, kept alive for the length of the test.</summary>
        [SetUp]
        public void Prepare()
        {
            _probe = ScriptableObject.CreateInstance<TickPropertyProbe>();
            _serialized = new SerializedObject(_probe);
        }

        /// <summary>Neither is saved, so both are released by hand.</summary>
        [TearDown]
        public void Cleanup()
        {
            _serialized?.Dispose();
            _serialized = null;

            if (_probe != null)
                Object.DestroyImmediate(_probe);

            _probe = null;
        }

        /// <summary>A bare tick count is already the property to edit, so it is handed straight back.</summary>
        [Test]
        public void ABareTickCountIsItsOwnProperty()
        {
            SerializedProperty property = Property(TickPropertyProbe.TicksField);

            Assert.That(Resolve(property), Is.SameAs(property));
        }

        /// <summary>
        /// A number too small for a tick count is refused rather than widened, because writing back
        /// through it would drop the top half of what was typed without saying so.
        /// </summary>
        [Test]
        public void ANumberTooSmallForTicksIsRefused()
            => Assert.That(Resolve(Property(TickPropertyProbe.NarrowField)), Is.Null);

        /// <summary>
        /// A wrapper is unwrapped to the tick count inside it, which is what lets one row serve both a
        /// bare count and a serializable date.
        /// </summary>
        [Test]
        public void AWrapperIsUnwrappedToTheCountInsideIt()
        {
            SerializedProperty resolved = Resolve(Property(TickPropertyProbe.WrapperField));

            Assert.That(resolved, Is.Not.Null);
            Assert.That(resolved.name, Is.EqualTo(TickPropertyProbe.InnerTicksField));
        }

        /// <summary>A struct with no tick count in it has nothing to unwrap to.</summary>
        [Test]
        public void AWrapperWithoutACountResolvesToNothing()
            => Assert.That(Resolve(Property(TickPropertyProbe.WrongWrapperField)), Is.Null);

        /// <summary>
        /// The name the count is looked up under comes from the caller, so a wrapper that keeps it
        /// elsewhere is not silently matched on the wrong field.
        /// </summary>
        [Test]
        public void TheCountIsLookedUpUnderTheNameTheCallerGave() => Assert.That(
            TickProperty.Resolve(Property(TickPropertyProbe.WrapperField), "notTheTickField"),
            Is.Null);

        /// <summary>Nothing in means nothing out, not a crash.</summary>
        [Test]
        public void NothingResolvesToNothing() => Assert.That(Resolve(null), Is.Null);

        /// <summary>Resolves the tick count of the given property.</summary>
        private static SerializedProperty Resolve(SerializedProperty property)
            => TickProperty.Resolve(property, TickPropertyProbe.InnerTicksField);

        /// <summary>The named property of the probe.</summary>
        private SerializedProperty Property(string fieldName) => _serialized.FindProperty(fieldName);
    }
}