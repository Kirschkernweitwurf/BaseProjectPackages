using NUnit.Framework;

namespace Base.AttributesPackage.Tests
{
    /// <summary>
    /// Covers when an array counts as locked. The add and remove buttons switch themselves off on a
    /// fixed field, so getting this wrong either strands a list nobody can edit or leaves buttons on a
    /// field whose indices carry meaning.
    /// </summary>
    public sealed class ArraySizeAttributeTests
    {
        /// <summary>An exact count locks the array.</summary>
        [Test]
        public void AnExactCountLocksTheArray()
        {
            ArraySizeAttribute attribute = new(4);

            Assert.That(attribute.Size, Is.EqualTo(4));
            Assert.That(attribute.IsFixed, Is.True);
        }

        /// <summary>A count of nothing is still an exact count, so it locks the array too.</summary>
        [Test]
        public void ACountOfNothingStillLocksTheArray() => Assert.That(new ArraySizeAttribute(0).IsFixed, Is.True);

        /// <summary>An attribute with no bounds at all leaves the array open.</summary>
        [Test]
        public void NoBoundsLeavesTheArrayOpen()
        {
            ArraySizeAttribute attribute = new();

            Assert.That(attribute.Size, Is.EqualTo(ArraySizeAttribute.Unbounded));
            Assert.That(attribute.Min, Is.EqualTo(ArraySizeAttribute.Unbounded));
            Assert.That(attribute.Max, Is.EqualTo(ArraySizeAttribute.Unbounded));
            Assert.That(attribute.IsFixed, Is.False);
        }

        /// <summary>A range that allows more than one count leaves the array open.</summary>
        [Test]
        public void AWideRangeLeavesTheArrayOpen()
        {
            ArraySizeAttribute attribute = new()
            {
                Min = 2,
                Max = 5
            };

            Assert.That(attribute.IsFixed, Is.False);
        }

        /// <summary>A range that allows exactly one count is the same as an exact count.</summary>
        [Test]
        public void ARangeThatAllowsOneCountLocksTheArray()
        {
            ArraySizeAttribute attribute = new()
            {
                Min = 3,
                Max = 3
            };

            Assert.That(attribute.IsFixed, Is.True);
        }

        /// <summary>A lower bound on its own leaves the array open at the top.</summary>
        [Test]
        public void ALowerBoundAloneLeavesTheArrayOpen()
        {
            ArraySizeAttribute attribute = new()
            {
                Min = 3
            };

            Assert.That(attribute.IsFixed, Is.False);
        }

        /// <summary>An upper bound on its own leaves the array open at the bottom.</summary>
        [Test]
        public void AnUpperBoundAloneLeavesTheArrayOpen()
        {
            ArraySizeAttribute attribute = new()
            {
                Max = 3
            };

            Assert.That(attribute.IsFixed, Is.False);
        }
    }
}