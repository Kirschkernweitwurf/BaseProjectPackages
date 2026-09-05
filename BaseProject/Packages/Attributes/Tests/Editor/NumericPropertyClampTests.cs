using Base.AttributesPackage.Editor.Core;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Base.AttributesPackage.Tests
{
    /// <summary>
    /// Covers the clamp both the min and the max handler share, which is what makes them agree. It
    /// runs on every repaint of the field it guards, so it also has to leave a value that is already
    /// inside the range untouched rather than rewriting it.
    /// </summary>
    public sealed class NumericPropertyClampTests
    {
        private const float Ceiling = 10f;
        private const float Floor = 2f;
        private const float Tolerance = 0.0001f;

        private NumericClampProbe _probe;
        private SerializedObject _serialized;

        /// <summary>A probe and one serialized view, kept alive for the length of the test.</summary>
        [SetUp]
        public void Prepare()
        {
            _probe = ScriptableObject.CreateInstance<NumericClampProbe>();
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

        /// <summary>An integer below the floor is lifted to it.</summary>
        [Test]
        public void AnIntegerBelowTheFloorIsLifted()
        {
            SerializedProperty property = Clamped(NumericClampProbe.NumberField, 0);

            Assert.That(property.intValue, Is.EqualTo((int)Floor));
        }

        /// <summary>An integer above the ceiling is pulled down to it.</summary>
        [Test]
        public void AnIntegerAboveTheCeilingIsPulledDown()
        {
            SerializedProperty property = Clamped(NumericClampProbe.NumberField, 99);

            Assert.That(property.intValue, Is.EqualTo((int)Ceiling));
        }

        /// <summary>An integer already inside the range is left as it is.</summary>
        [Test]
        public void AnIntegerInsideTheRangeIsLeftAlone()
        {
            SerializedProperty property = Clamped(NumericClampProbe.NumberField, 5);

            Assert.That(property.intValue, Is.EqualTo(5));
        }

        /// <summary>
        /// A bound that is not whole rounds rather than truncating, so a floor of 2.6 does not admit a
        /// 2 that is below it.
        /// </summary>
        [Test]
        public void AnIntegerBoundIsRounded()
        {
            SerializedProperty property = _serialized.FindProperty(NumericClampProbe.NumberField);
            property.intValue = 0;
            NumericPropertyClamp.Apply(property, 2.6f, Ceiling);

            Assert.That(property.intValue, Is.EqualTo(3));
        }

        /// <summary>A float below the floor is lifted to it.</summary>
        [Test]
        public void AFloatBelowTheFloorIsLifted()
        {
            SerializedProperty property = _serialized.FindProperty(NumericClampProbe.DecimalField);
            property.floatValue = -5f;
            NumericPropertyClamp.Apply(property, Floor, Ceiling);

            Assert.That(property.floatValue, Is.EqualTo(Floor).Within(Tolerance));
        }

        /// <summary>A float already inside the range keeps the value it had.</summary>
        [Test]
        public void AFloatInsideTheRangeIsLeftAlone()
        {
            SerializedProperty property = _serialized.FindProperty(NumericClampProbe.DecimalField);
            property.floatValue = 4.25f;
            NumericPropertyClamp.Apply(property, Floor, Ceiling);

            Assert.That(property.floatValue, Is.EqualTo(4.25f).Within(Tolerance));
        }

        /// <summary>
        /// Only one side needs a bound, so the other is passed an infinity and has to leave its end of
        /// the range open.
        /// </summary>
        [Test]
        public void AnUnboundedSideDoesNotClamp()
        {
            SerializedProperty property = _serialized.FindProperty(NumericClampProbe.DecimalField);
            property.floatValue = 1000f;
            NumericPropertyClamp.Apply(property, Floor, float.PositiveInfinity);

            Assert.That(property.floatValue, Is.EqualTo(1000f).Within(Tolerance));
        }

        /// <summary>
        /// A vector is clamped one component at a time, the way Unity's own min attribute treats one,
        /// rather than by its length.
        /// </summary>
        [Test]
        public void AVectorIsClampedComponentWise()
        {
            SerializedProperty property = _serialized.FindProperty(NumericClampProbe.Vector2Field);
            property.vector2Value = new Vector2(-1f, 99f);
            NumericPropertyClamp.Apply(property, Floor, Ceiling);

            Assert.That(property.vector2Value, Is.EqualTo(new Vector2(Floor, Ceiling)));
        }

        /// <summary>The third component is clamped along with the other two.</summary>
        [Test]
        public void EveryComponentOfAThreeAxisVectorIsClamped()
        {
            SerializedProperty property = _serialized.FindProperty(NumericClampProbe.Vector3Field);
            property.vector3Value = new Vector3(-1f, 5f, 99f);
            NumericPropertyClamp.Apply(property, Floor, Ceiling);

            Assert.That(property.vector3Value, Is.EqualTo(new Vector3(Floor, 5f, Ceiling)));
        }

        /// <summary>An integer vector clamps component wise too, and rounds like a plain integer.</summary>
        [Test]
        public void AnIntegerVectorIsClampedComponentWise()
        {
            SerializedProperty property = _serialized.FindProperty(NumericClampProbe.Vector2IntField);
            property.vector2IntValue = new Vector2Int(-1, 99);
            NumericPropertyClamp.Apply(property, Floor, Ceiling);

            Assert.That(property.vector2IntValue, Is.EqualTo(new Vector2Int((int)Floor, (int)Ceiling)));
        }

        /// <summary>The third component of an integer vector is clamped as well.</summary>
        [Test]
        public void EveryComponentOfAThreeAxisIntegerVectorIsClamped()
        {
            SerializedProperty property = _serialized.FindProperty(NumericClampProbe.Vector3IntField);
            property.vector3IntValue = new Vector3Int(-1, 5, 99);
            NumericPropertyClamp.Apply(property, Floor, Ceiling);

            Assert.That(property.vector3IntValue, Is.EqualTo(new Vector3Int((int)Floor, 5, (int)Ceiling)));
        }

        /// <summary>
        /// A type with no numbers in it is passed over rather than throwing, so the attribute on the
        /// wrong field is harmless instead of breaking the whole inspector.
        /// </summary>
        [Test]
        public void ATypeWithNothingToClampIsPassedOver()
        {
            SerializedProperty property = _serialized.FindProperty(NumericClampProbe.TextField);
            property.stringValue = "unchanged";

            Assert.That(() => NumericPropertyClamp.Apply(property, Floor, Ceiling), Throws.Nothing);
            Assert.That(property.stringValue, Is.EqualTo("unchanged"));
        }

        /// <summary>Sets the named integer property and clamps it into the shared range.</summary>
        private SerializedProperty Clamped(string fieldName, int value)
        {
            SerializedProperty property = _serialized.FindProperty(fieldName);
            property.intValue = value;
            NumericPropertyClamp.Apply(property, Floor, Ceiling);

            return property;
        }
    }
}