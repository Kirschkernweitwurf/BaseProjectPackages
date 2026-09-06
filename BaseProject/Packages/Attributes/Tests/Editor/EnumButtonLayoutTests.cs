using System;
using Base.AttributesPackage.Editor.Core;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Base.AttributesPackage.Tests
{
    /// <summary>
    /// Covers what a set of enum buttons is built from. A flags enum and a plain one are drawn by the
    /// same drawer but indexed differently, so a label mapped to the wrong slot writes the wrong value
    /// when the button is pressed.
    /// </summary>
    public sealed class EnumButtonLayoutTests
    {
        private const string NicifiedSecond = "Second Thing";

        private EnumButtonProbe _probe;
        private SerializedObject _serialized;

        /// <summary>A probe and one serialized view, kept alive for the length of the test.</summary>
        [SetUp]
        public void Prepare()
        {
            _probe = ScriptableObject.CreateInstance<EnumButtonProbe>();
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

        /// <summary>A plain enum takes its labels from Unity, so they read as the inspector shows them.</summary>
        [Test]
        public void APlainEnumTakesUnityDisplayNames()
        {
            EnumButtonLayout layout = Build(typeof(EProbeMood), EnumButtonProbe.MoodField);

            Assert.That(layout.IsFlags, Is.False);
            Assert.That(layout.Labels, Is.EqualTo(new[]
            {
                nameof(EProbeMood.Calm),
                nameof(EProbeMood.Angry)
            }));
        }

        /// <summary>
        /// A plain enum maps by Unity's value index, so it carries no bits of its own. The drawer
        /// branches on this being null.
        /// </summary>
        [Test]
        public void APlainEnumCarriesNoBits()
            => Assert.That(Build(typeof(EProbeMood), EnumButtonProbe.MoodField).Values, Is.Null);

        /// <summary>A flags enum says so, which is what makes the buttons multi-select.</summary>
        [Test]
        public void AFlagsEnumIsRecognized()
            => Assert.That(Build(typeof(EProbeFlags), EnumButtonProbe.FlagsField).IsFlags, Is.True);

        /// <summary>
        /// The zero member is the absence of every flag rather than one of them, so it gets no button
        /// of its own.
        /// </summary>
        [Test]
        public void TheZeroMemberOfAFlagsEnumGetsNoButton()
        {
            EnumButtonLayout layout = Build(typeof(EProbeFlags), EnumButtonProbe.FlagsField);

            Assert.That(layout.Labels, Has.Length.EqualTo(3));
            Assert.That(layout.Labels, Has.No.Member(nameof(EProbeFlags.None)));
        }

        /// <summary>
        /// Every label lines up with the bit it writes. A slot out of step here toggles a flag the
        /// button does not name.
        /// </summary>
        [Test]
        public void EveryFlagLabelLinesUpWithItsBit()
        {
            EnumButtonLayout layout = Build(typeof(EProbeFlags), EnumButtonProbe.FlagsField);

            Assert.That(layout.Values, Is.EqualTo(new[]
            {
                (int)EProbeFlags.First,
                (int)EProbeFlags.SecondThing,
                (int)EProbeFlags.Third
            }));
        }

        /// <summary>A flag written in one word is spaced out, matching how Unity labels a plain enum.</summary>
        [Test]
        public void AFlagLabelIsSpacedOut() => Assert.That(
            Build(typeof(EProbeFlags), EnumButtonProbe.FlagsField).Labels,
            Contains.Item(NicifiedSecond));

        /// <summary>
        /// An enum with nothing to draw yields no layout, which is the drawer's signal to fall back to
        /// the ordinary popup rather than draw an empty row.
        /// </summary>
        [Test]
        public void AnEnumWithNothingToDrawYieldsNoLayout()
            => Assert.That(Build(typeof(EProbeZeroOnly), EnumButtonProbe.ZeroOnlyField), Is.Null);

        /// <summary>Builds the layout for the named field of the probe.</summary>
        private EnumButtonLayout Build(Type enumType, string fieldName)
            => EnumButtonLayout.Build(enumType, _serialized.FindProperty(fieldName));
    }
}