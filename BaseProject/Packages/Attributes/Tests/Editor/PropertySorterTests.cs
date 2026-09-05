using System.Collections.Generic;
using Base.AttributesPackage.Editor.Core;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Base.AttributesPackage.Tests
{
    /// <summary>
    /// Covers the order fields end up in. This is the most visible thing the package does, and the
    /// sort has to hold three promises at once: a run stays together, a field cannot leave the section
    /// it was written in, and everything that did not ask to move stays where it was.
    /// </summary>
    public sealed class PropertySorterTests
    {
        private readonly List<Object> _hosts = new();
        private readonly List<SerializedObject> _serializedObjects = new();

        /// <summary>
        /// None of the hosts is saved, so each is destroyed by hand. The serialized views are released
        /// here rather than where they were made, because the properties handed out of them stay in use
        /// for the length of a test and a collected view takes its properties down with it.
        /// </summary>
        [TearDown]
        public void Cleanup()
        {
            foreach (SerializedObject serialized in _serializedObjects)
                serialized.Dispose();

            _serializedObjects.Clear();

            foreach (Object host in _hosts)
            {
                if (host != null)
                    Object.DestroyImmediate(host);
            }

            _hosts.Clear();
        }

        /// <summary>
        /// A negative order pins a field to the top and a positive one pushes it down, with unmarked
        /// fields counting as zero in between. That is what makes pinning a matter of one number.
        /// </summary>
        [Test]
        public void AnOrderPinsAFieldAboveAndBelowTheUnmarkedOnes()
        {
            PropertyOrderProbe probe = Create<PropertyOrderProbe>();
            List<SerializedProperty> properties = PropertiesOf(probe,
                PropertyOrderProbe.PlainOneField,
                PropertyOrderProbe.PushedField,
                PropertyOrderProbe.PlainTwoField,
                PropertyOrderProbe.PinnedField);

            PropertySorter.Sort(properties, typeof(PropertyOrderProbe));

            Assert.That(NamesOf(properties), Is.EqualTo(new List<string>
            {
                PropertyOrderProbe.PinnedField,
                PropertyOrderProbe.PlainOneField,
                PropertyOrderProbe.PlainTwoField,
                PropertyOrderProbe.PushedField
            }));
        }

        /// <summary>
        /// The two unmarked fields keep the order they were declared in, since an unstable sort would
        /// reshuffle every field that shares an order, which is most of them.
        /// </summary>
        [Test]
        public void FieldsSharingAnOrderKeepTheOrderTheyWereWrittenIn()
        {
            PropertyOrderProbe probe = Create<PropertyOrderProbe>();
            List<SerializedProperty> properties = PropertiesOf(probe,
                PropertyOrderProbe.PlainOneField,
                PropertyOrderProbe.PushedField,
                PropertyOrderProbe.PlainTwoField,
                PropertyOrderProbe.PinnedField);

            PropertySorter.Sort(properties, typeof(PropertyOrderProbe));
            List<string> names = NamesOf(properties);

            Assert.That(names.IndexOf(PropertyOrderProbe.PlainOneField),
                Is.LessThan(names.IndexOf(PropertyOrderProbe.PlainTwoField)));
        }

        /// <summary>
        /// A foldout is a run of consecutive fields, so pinning one member pins the whole run rather
        /// than tearing that member out of the group it belongs to.
        /// </summary>
        [Test]
        public void PinningOneMemberOfARunMovesTheWholeRun()
        {
            PropertyRunProbe probe = Create<PropertyRunProbe>();
            List<SerializedProperty> properties = PropertiesOf(probe,
                PropertyRunProbe.LeadingField,
                PropertyRunProbe.GroupedOneField,
                PropertyRunProbe.GroupedTwoField);

            PropertySorter.Sort(properties, typeof(PropertyRunProbe));

            Assert.That(NamesOf(properties), Is.EqualTo(new List<string>
            {
                PropertyRunProbe.GroupedOneField,
                PropertyRunProbe.GroupedTwoField,
                PropertyRunProbe.LeadingField
            }));
        }

        /// <summary>
        /// Sorting happens inside a section, so a pinned field in the second one moves up within it and
        /// does not climb past the heading into the first.
        /// </summary>
        [Test]
        public void APinnedFieldCannotLeaveItsSection()
        {
            PropertySectionProbe probe = Create<PropertySectionProbe>();
            List<SerializedProperty> properties = PropertiesOf(probe,
                PropertySectionProbe.FirstTitleField,
                PropertySectionProbe.FirstBodyField,
                PropertySectionProbe.SecondTitleField,
                PropertySectionProbe.SecondBodyField,
                PropertySectionProbe.SecondPinnedField);

            PropertySorter.Sort(properties, typeof(PropertySectionProbe));

            Assert.That(NamesOf(properties), Is.EqualTo(new List<string>
            {
                PropertySectionProbe.FirstTitleField,
                PropertySectionProbe.FirstBodyField,
                PropertySectionProbe.SecondTitleField,
                PropertySectionProbe.SecondPinnedField,
                PropertySectionProbe.SecondBodyField
            }));
        }

        /// <summary>
        /// The heading is drawn from the field that carries it, so that field stays at the top of its
        /// section whatever sorts underneath it.
        /// </summary>
        [Test]
        public void TheFieldCarryingATitleStaysAtTheTopOfItsSection()
        {
            PropertySectionProbe probe = Create<PropertySectionProbe>();
            List<SerializedProperty> properties = PropertiesOf(probe,
                PropertySectionProbe.SecondTitleField,
                PropertySectionProbe.SecondBodyField,
                PropertySectionProbe.SecondPinnedField);

            PropertySorter.Sort(properties, typeof(PropertySectionProbe));

            Assert.That(NamesOf(properties)[0], Is.EqualTo(PropertySectionProbe.SecondTitleField));
        }

        /// <summary>
        /// Nothing is touched unless a field asked for it, so an object nobody marked draws in the
        /// order it was written.
        /// </summary>
        [Test]
        public void NothingMovesWhenNoFieldAsked()
        {
            PropertySectionProbe probe = Create<PropertySectionProbe>();
            List<string> declared = new()
            {
                PropertySectionProbe.FirstTitleField,
                PropertySectionProbe.FirstBodyField,
                PropertySectionProbe.SecondTitleField,
                PropertySectionProbe.SecondBodyField
            };

            List<SerializedProperty> properties = PropertiesOf(probe, declared.ToArray());
            PropertySorter.Sort(properties, typeof(PropertySectionProbe));

            Assert.That(NamesOf(properties), Is.EqualTo(declared));
        }

        /// <summary>The serialized names of the properties, in the order they now sit in.</summary>
        private static List<string> NamesOf(IReadOnlyList<SerializedProperty> properties)
        {
            List<string> names = new();

            foreach (SerializedProperty property in properties)
                names.Add(property.name);

            return names;
        }

        /// <summary>The named properties of the host, in the order they were asked for.</summary>
        private List<SerializedProperty> PropertiesOf(Object host, params string[] names)
        {
            SerializedObject serialized = new(host);
            _serializedObjects.Add(serialized);

            List<SerializedProperty> properties = new();

            foreach (string name in names)
                properties.Add(serialized.FindProperty(name));

            return properties;
        }

        /// <summary>Creates a host and remembers it so the teardown can clean it up.</summary>
        private T Create<T>() where T : ScriptableObject
        {
            T host = ScriptableObject.CreateInstance<T>();
            _hosts.Add(host);

            return host;
        }
    }
}