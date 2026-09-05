using Base.AttributesPackage.Editor.Collections;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Base.AttributesPackage.Tests
{
    /// <summary>
    /// Covers the text a list row is titled with, which is also the text the search box filters on. A
    /// wrong answer here turns a readable column of names back into a column of indices, and makes
    /// filtering a list match nothing.
    /// </summary>
    public sealed class ElementLabelTests
    {
        private const int FirstIndex = 0;
        private const string GivenName = "Kitchen Lamp";
        private const int GivenAmount = 42;

        private ElementLabelProbe _probe;
        private SerializedObject _serialized;

        /// <summary>A fresh host per test, so one test's array cannot decide the next one's outcome.</summary>
        [SetUp]
        public void Prepare()
        {
            _probe = ScriptableObject.CreateInstance<ElementLabelProbe>();
            _serialized = new SerializedObject(_probe);
        }

        /// <summary>Neither the host nor its serialized view is saved, so both are released by hand.</summary>
        [TearDown]
        public void Cleanup()
        {
            _serialized?.Dispose();
            _serialized = null;

            if (_probe != null)
                Object.DestroyImmediate(_probe);

            _probe = null;
        }

        /// <summary>A string element is its own label, which is what makes a list of names readable.</summary>
        [Test]
        public void AStringElementLabelsItself()
        {
            SerializedProperty element = FirstElementOf(ElementLabelProbe.NamesField);
            element.stringValue = GivenName;

            Assert.That(ElementLabel.For(element, FirstIndex), Is.EqualTo(GivenName));
        }

        /// <summary>
        /// An empty string is not a label, so the row says so rather than showing a blank line the
        /// search box could never match.
        /// </summary>
        [Test]
        public void AnEmptyStringElementIsMarkedUnnamed()
        {
            SerializedProperty element = FirstElementOf(ElementLabelProbe.NamesField);
            element.stringValue = string.Empty;

            Assert.That(ElementLabel.For(element, FirstIndex), Does.Contain("unnamed"));
        }

        /// <summary>A number is turned into its text, so a list of numbers filters like anything else.</summary>
        [Test]
        public void AnIntegerElementIsConvertedToText()
        {
            SerializedProperty element = FirstElementOf(ElementLabelProbe.AmountsField);
            element.intValue = GivenAmount;

            Assert.That(ElementLabel.For(element, FirstIndex), Is.EqualTo(GivenAmount.ToString()));
        }

        /// <summary>
        /// A struct has no single value to show, so the first string on it is used. This is the case
        /// the whole helper exists for: a list of configs reading as a column of names.
        /// </summary>
        [Test]
        public void AStructIsLabeledByItsFirstStringField()
        {
            SerializedProperty element = FirstElementOf(ElementLabelProbe.LabeledField);
            element.FindPropertyRelative(LabeledEntry.TitleField).stringValue = GivenName;

            Assert.That(ElementLabel.For(element, FirstIndex), Is.EqualTo(GivenName));
        }

        /// <summary>
        /// A struct with no string anywhere has nothing to be named after, so it keeps its index
        /// instead of showing an empty row.
        /// </summary>
        [Test]
        public void AStructWithoutAStringKeepsItsIndex()
        {
            SerializedProperty element = FirstElementOf(ElementLabelProbe.UnlabeledField);

            Assert.That(ElementLabel.For(element, FirstIndex), Is.EqualTo($"Element {FirstIndex}"));
        }

        /// <summary>Grows the named array to one entry and hands back that entry.</summary>
        private SerializedProperty FirstElementOf(string fieldName)
        {
            SerializedProperty array = _serialized.FindProperty(fieldName);
            array.arraySize = 1;

            return array.GetArrayElementAtIndex(FirstIndex);
        }
    }
}