using System.Reflection;
using Base.AttributesPackage.Editor;
using Base.AttributesPackage.Editor.Core;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Base.AttributesPackage.Tests
{
    /// <summary>
    /// Covers the bounds read off a field and, from them, whether the add and remove controls draw at
    /// all. A wrong answer here either strands a list nobody can edit or leaves buttons on a field
    /// whose indices carry meaning, and neither is visible until somebody tries.
    /// </summary>
    public sealed class ArraySizeLimitsTests
    {
        private const int EqualBound = 3;
        private const int FixedCount = 4;
        private const BindingFlags Flags = BindingFlags.Instance | BindingFlags.NonPublic;
        private const int Highest = 5;
        private const int Lowest = 2;

        private ArraySizeProbe _probe;
        private SerializedObject _serialized;

        /// <summary>A probe and one serialized view, kept alive for the length of the test.</summary>
        [SetUp]
        public void Prepare()
        {
            _probe = ScriptableObject.CreateInstance<ArraySizeProbe>();
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

        /// <summary>An exact count is both the floor and the ceiling.</summary>
        [Test]
        public void AnExactCountBoundsBothEnds()
        {
            Assert.That(TryGet(ArraySizeProbe.FixedField, out int minimum, out int maximum), Is.True);
            Assert.That(minimum, Is.EqualTo(FixedCount));
            Assert.That(maximum, Is.EqualTo(FixedCount));
        }

        /// <summary>A range is read as it was written.</summary>
        [Test]
        public void ARangeIsReadAtBothEnds()
        {
            TryGet(ArraySizeProbe.RangedField, out int minimum, out int maximum);

            Assert.That(minimum, Is.EqualTo(Lowest));
            Assert.That(maximum, Is.EqualTo(Highest));
        }

        /// <summary>A floor with no ceiling is still a bound, so it is reported rather than dismissed.</summary>
        [Test]
        public void AFloorWithNoCeilingIsStillABound()
        {
            Assert.That(TryGet(ArraySizeProbe.MinimumOnlyField, out int minimum, out int _), Is.True);
            Assert.That(minimum, Is.EqualTo(Lowest));
        }

        /// <summary>A field nobody bounded has no bounds to read.</summary>
        [Test]
        public void AnUnmarkedArrayHasNoBounds()
            => Assert.That(TryGet(ArraySizeProbe.PlainField, out int _, out int _), Is.False);

        /// <summary>
        /// Unity reports a string as an array of characters, so the attribute has to be ignored there or
        /// every bounded string in the project would report a count it does not have.
        /// </summary>
        [Test]
        public void AStringIsNotTreatedAsAnArray()
            => Assert.That(TryGet(ArraySizeProbe.TextField, out int _, out int _), Is.False);

        /// <summary>The attribute on something that is not a collection has nothing to bound.</summary>
        [Test]
        public void AFieldThatIsNotACollectionHasNoBounds()
            => Assert.That(TryGet(ArraySizeProbe.NumberField, out int _, out int _), Is.False);

        /// <summary>An exact count leaves nothing to add or remove, so the controls come off.</summary>
        [Test]
        public void AnExactCountLocksTheSize()
            => Assert.That(CanResize(ArraySizeProbe.FixedField), Is.False);

        /// <summary>
        /// A floor equal to its ceiling is an exact count written the long way, and has to lock the same.
        /// </summary>
        [Test]
        public void EqualBoundsLockTheSizeToo()
        {
            TryGet(ArraySizeProbe.EqualBoundsField, out int minimum, out int maximum);

            Assert.That(minimum, Is.EqualTo(EqualBound));
            Assert.That(maximum, Is.EqualTo(EqualBound));
            Assert.That(CanResize(ArraySizeProbe.EqualBoundsField), Is.False);
        }

        /// <summary>A range still leaves room to move within it, so the controls stay.</summary>
        [Test]
        public void ARangeStillAllowsResizing()
            => Assert.That(CanResize(ArraySizeProbe.RangedField), Is.True);

        /// <summary>A floor with no ceiling can always grow, so the controls stay.</summary>
        [Test]
        public void AFloorWithNoCeilingStillAllowsResizing()
            => Assert.That(CanResize(ArraySizeProbe.MinimumOnlyField), Is.True);

        /// <summary>A field nobody bounded is free, which is the common case.</summary>
        [Test]
        public void AnUnmarkedArrayAllowsResizing()
            => Assert.That(CanResize(ArraySizeProbe.PlainField), Is.True);

        /// <summary>Reads the bounds of the named field.</summary>
        private bool TryGet(string fieldName, out int minimum, out int maximum)
            => ArraySizeLimits.TryGet(Context(fieldName), out minimum, out maximum);

        /// <summary>Whether the named field may still be added to or removed from.</summary>
        private bool CanResize(string fieldName) => ArraySizeLimits.CanResize(Context(fieldName));

        /// <summary>Builds a context standing on the named field of the probe.</summary>
        private MemberContext Context(string fieldName)
        {
            FieldInfo field = typeof(ArraySizeProbe).GetField(fieldName, Flags);

            return new MemberContext(_serialized.FindProperty(fieldName), field, _probe,
                typeof(ArraySizeProbe), _probe, null, null);
        }
    }
}