using Base.AttributesPackage.Editor.Core;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Base.AttributesPackage.Tests
{
    /// <summary>
    /// Covers the one tracker that decides whether a default expanded state is applied. It is keyed by
    /// type and property path, the same way Unity stores the expanded flag itself, so folding a field
    /// on one object stays folded on the next object of that type instead of springing back open.
    /// </summary>
    public sealed class FirstDrawTests
    {
        private ElementLabelProbe _first;
        private ElementLabelProbe _second;
        private SerializedObject _firstSerialized;
        private SerializedObject _secondSerialized;

        /// <summary>
        /// Two hosts of the same type, so a per-type key can be told from a per-instance one. The set
        /// is static and outlives a test, so it is cleared before each one.
        /// </summary>
        [SetUp]
        public void Prepare()
        {
            _first = ScriptableObject.CreateInstance<ElementLabelProbe>();
            _second = ScriptableObject.CreateInstance<ElementLabelProbe>();
            _firstSerialized = new SerializedObject(_first);
            _secondSerialized = new SerializedObject(_second);

            FirstDraw.Forget(typeof(ElementLabelProbe));
        }

        /// <summary>Hands back both hosts and clears what this test left in the static set.</summary>
        [TearDown]
        public void Cleanup()
        {
            FirstDraw.Forget(typeof(ElementLabelProbe));

            _firstSerialized?.Dispose();
            _secondSerialized?.Dispose();
            _firstSerialized = null;
            _secondSerialized = null;

            if (_first != null)
                Object.DestroyImmediate(_first);

            if (_second != null)
                Object.DestroyImmediate(_second);

            _first = null;
            _second = null;
        }

        /// <summary>
        /// A default is applied once and not forced again, or the field could never be folded away.
        /// </summary>
        [Test]
        public void OnlyTheFirstDrawReportsItself()
        {
            Assert.That(FirstDraw.IsFirst(NamesOf(_firstSerialized)), Is.True);
            Assert.That(FirstDraw.IsFirst(NamesOf(_firstSerialized)), Is.False);
        }

        /// <summary>
        /// Keyed by type on purpose: Unity shares the expanded flag across every object of a type, so a
        /// per-instance key would force the field open again on the next object and undo the fold on
        /// the one before it.
        /// </summary>
        [Test]
        public void AnotherObjectOfTheSameTypeIsNotAFirstDraw()
        {
            FirstDraw.IsFirst(NamesOf(_firstSerialized));

            Assert.That(FirstDraw.IsFirst(NamesOf(_secondSerialized)), Is.False);
        }

        /// <summary>Two fields are tracked apart, so drawing one does not consume the other's default.</summary>
        [Test]
        public void TwoFieldsOnOneObjectAreTrackedSeparately()
        {
            FirstDraw.IsFirst(NamesOf(_firstSerialized));

            Assert.That(FirstDraw.IsFirst(AmountsOf(_firstSerialized)), Is.True);
        }

        /// <summary>
        /// Forgetting a type puts every field on it back to never drawn, which is how a sample page
        /// shows the defaults its attributes declare on every visit.
        /// </summary>
        [Test]
        public void ForgettingATypeMakesTheNextDrawTheFirstAgain()
        {
            FirstDraw.IsFirst(NamesOf(_firstSerialized));

            FirstDraw.Forget(typeof(ElementLabelProbe));

            Assert.That(FirstDraw.IsFirst(NamesOf(_firstSerialized)), Is.True);
        }

        /// <summary>Nothing to forget is not an error, so a missing type is simply ignored.</summary>
        [Test]
        public void ForgettingNothingIsHarmless()
            => Assert.DoesNotThrow(() => FirstDraw.Forget(null));

        /// <summary>The string array of the given host.</summary>
        private static SerializedProperty NamesOf(SerializedObject host)
            => host.FindProperty(ElementLabelProbe.NamesField);

        /// <summary>The integer array of the given host.</summary>
        private static SerializedProperty AmountsOf(SerializedObject host)
            => host.FindProperty(ElementLabelProbe.AmountsField);
    }
}