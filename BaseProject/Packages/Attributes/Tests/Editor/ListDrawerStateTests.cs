using Base.AttributesPackage.Editor.Collections;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Base.AttributesPackage.Tests
{
    /// <summary>
    /// Covers the one thing this state is for: a filter belongs to one field on one object, so it is
    /// keyed per instance and per property path. Keying it any wider makes one search box drive lists
    /// nobody typed into.
    /// </summary>
    public sealed class ListDrawerStateTests
    {
        private const string GivenSearch = "lamp";

        private ElementLabelProbe _first;
        private ElementLabelProbe _second;
        private SerializedObject _firstSerialized;
        private SerializedObject _secondSerialized;

        /// <summary>Two hosts of the same type, so a per-instance key can be told from a wider one.</summary>
        [SetUp]
        public void Prepare()
        {
            _first = ScriptableObject.CreateInstance<ElementLabelProbe>();
            _second = ScriptableObject.CreateInstance<ElementLabelProbe>();
            _firstSerialized = new SerializedObject(_first);
            _secondSerialized = new SerializedObject(_second);
        }

        /// <summary>Hands back both hosts.</summary>
        [TearDown]
        public void Cleanup()
        {
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

        /// <summary>The same field on the same object keeps the filter that was typed into it.</summary>
        [Test]
        public void TheSameFieldKeepsItsFilter()
        {
            ListDrawerState.For(NamesOf(_firstSerialized)).Search = GivenSearch;

            Assert.That(ListDrawerState.For(NamesOf(_firstSerialized)).Search, Is.EqualTo(GivenSearch));
        }

        /// <summary>
        /// A filter is something one person typed into one field, so the same field on another object
        /// must not inherit it.
        /// </summary>
        [Test]
        public void AnotherObjectDoesNotInheritTheFilter()
        {
            ListDrawerState.For(NamesOf(_firstSerialized)).Search = GivenSearch;

            Assert.That(ListDrawerState.For(NamesOf(_secondSerialized)).Search, Is.Empty);
        }

        /// <summary>Two fields on one object filter separately, or one search box would drive both.</summary>
        [Test]
        public void TwoFieldsOnOneObjectFilterSeparately()
        {
            ListDrawerState.For(NamesOf(_firstSerialized)).Search = GivenSearch;

            Assert.That(ListDrawerState.For(AmountsOf(_firstSerialized)).Search, Is.Empty);
        }

        /// <summary>The string array of the given host.</summary>
        private static SerializedProperty NamesOf(SerializedObject host)
            => host.FindProperty(ElementLabelProbe.NamesField);

        /// <summary>The integer array of the given host.</summary>
        private static SerializedProperty AmountsOf(SerializedObject host)
            => host.FindProperty(ElementLabelProbe.AmountsField);
    }
}