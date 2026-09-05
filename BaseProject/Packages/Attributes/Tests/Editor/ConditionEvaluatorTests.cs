using System;
using Base.AttributesPackage.Editor;
using Base.AttributesPackage.Editor.Core;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Base.AttributesPackage.Tests
{
    /// <summary>
    /// Covers the editor half of a condition, which is what every conditional attribute in an
    /// inspector runs through. The runtime half is covered separately and behaves differently on
    /// purpose: this one reads a serialized sibling off the property so a field reacts to a value
    /// still being typed, rather than to the last one that was applied.
    /// </summary>
    public sealed class ConditionEvaluatorTests
    {
        private const string MissingMember = "NoSuchMember";

        private ConditionContextProbe _probe;
        private UnityEditor.Editor _editor;
        private SerializedProperty _property;

        /// <summary>A probe with a live editor, since a condition resolves siblings through it.</summary>
        [SetUp]
        public void Prepare()
        {
            _probe = ScriptableObject.CreateInstance<ConditionContextProbe>();
            _editor = UnityEditor.Editor.CreateEditor(_probe);
            _property = _editor.serializedObject.FindProperty(ConditionContextProbe.TargetField);
        }

        /// <summary>Neither the probe nor its editor is saved, so both are destroyed by hand.</summary>
        [TearDown]
        public void Cleanup()
        {
            _property = null;

            if (_editor != null)
                Object.DestroyImmediate(_editor);

            if (_probe != null)
                Object.DestroyImmediate(_probe);

            _editor = null;
            _probe = null;
        }

        /// <summary>
        /// The reason this path exists at all. A tick the user just made lives in the serialized copy
        /// and has not reached the object yet, and the field it controls has to react to it now rather
        /// than one repaint later.
        /// </summary>
        [Test]
        public void AnEditThatHasNotBeenAppliedIsAlreadyVisible()
        {
            SerializedProperty flag = _editor.serializedObject.FindProperty(
                ConditionContextProbe.SerializedFlagField);

            flag.boolValue = true;

            Assert.That(Resolve(ConditionContextProbe.SerializedFlagField), Is.True);
        }

        /// <summary>A serialized bool that is off keeps the condition off.</summary>
        [Test]
        public void ASerializedFlagThatIsOffHoldsTheConditionBack()
            => Assert.That(Resolve(ConditionContextProbe.SerializedFlagField), Is.False);

        /// <summary>
        /// A member with no serialized counterpart is read by reflection, so a condition can point at
        /// a plain property the inspector never shows.
        /// </summary>
        [Test]
        public void AMemberWithNoSerializedFormFallsBackToReflection()
        {
            _probe.UnserializedFlag = true;

            Assert.That(Resolve(nameof(ConditionContextProbe.UnserializedFlag)), Is.True);
        }

        /// <summary>
        /// A member that cannot answer the question counts as true, so a renamed field never silently
        /// hides the thing that points at it.
        /// </summary>
        [Test]
        public void AMemberThatCannotBeFoundCountsAsTrue()
            => Assert.That(Resolve(MissingMember), Is.True);

        /// <summary>A serialized member that is not a bool cannot answer either, so it counts as true.</summary>
        [Test]
        public void AMemberThatIsNotABooleanCountsAsTrue()
            => Assert.That(Resolve(ConditionContextProbe.NumberField), Is.True);

        /// <summary>Every member has to hold when they are combined with all.</summary>
        [Test]
        public void CombiningWithAllNeedsEveryMember()
        {
            _probe.UnserializedFlag = true;

            Assert.That(ResolveAll(EConditionMode.All, ConditionContextProbe.SerializedFlagField,
                nameof(ConditionContextProbe.UnserializedFlag)), Is.False);
        }

        /// <summary>One member is enough when they are combined with any.</summary>
        [Test]
        public void CombiningWithAnyNeedsOneMember()
        {
            _probe.UnserializedFlag = true;

            Assert.That(ResolveAll(EConditionMode.Any, ConditionContextProbe.SerializedFlagField,
                nameof(ConditionContextProbe.UnserializedFlag)), Is.True);
        }

        /// <summary>
        /// An attribute written without arguments has nothing to test, so it must not hide or disable
        /// the member it sits on.
        /// </summary>
        [Test]
        public void NoMembersMeansTheConditionHolds()
        {
            Assert.That(ConditionEvaluator.ResolveAll(Context(), EConditionMode.All, null), Is.True);
            Assert.That(ConditionEvaluator.ResolveAll(Context(), EConditionMode.All, Array.Empty<string>()),
                Is.True);
        }

        /// <summary>An enum member is handed back as its value, which is what a switch compares against.</summary>
        [Test]
        public void AnEnumMemberIsResolvedToItsValue()
        {
            SerializedProperty mood = _editor.serializedObject.FindProperty(ConditionContextProbe.MoodField);
            mood.enumValueIndex = (int)EProbeMood.Angry;
            _editor.serializedObject.ApplyModifiedProperties();

            Assert.That(ConditionEvaluator.ResolveEnum(Context(), ConditionContextProbe.MoodField),
                Is.EqualTo(EProbeMood.Angry));
        }

        /// <summary>
        /// An enum member nobody can find resolves to nothing rather than to a default value, so a
        /// comparison against it fails loudly instead of matching the first entry.
        /// </summary>
        [Test]
        public void AnEnumMemberThatCannotBeFoundResolvesToNothing()
            => Assert.That(ConditionEvaluator.ResolveEnum(Context(), MissingMember), Is.Null);

        /// <summary>Resolves one member with the modes combined by all.</summary>
        private bool Resolve(string member) => ResolveAll(EConditionMode.All, member);

        /// <summary>Resolves the given members in the given mode.</summary>
        private bool ResolveAll(EConditionMode mode, params string[] members)
            => ConditionEvaluator.ResolveAll(Context(), mode, members);

        /// <summary>Builds a context standing on the probe's target field.</summary>
        private MemberContext Context()
            => new(_property, null, _probe, typeof(ConditionContextProbe), _probe, _editor, null);
    }
}