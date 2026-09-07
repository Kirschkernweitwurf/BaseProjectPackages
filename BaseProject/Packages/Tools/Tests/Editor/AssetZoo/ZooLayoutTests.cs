using System;
using Base.ToolsPackage.Editor.AssetZoo.Alignment;
using Base.ToolsPackage.Editor.AssetZoo.Config;
using Base.ToolsPackage.Editor.AssetZoo.Layout;
using NUnit.Framework;
using UnityEngine;

namespace Base.ToolsPackage.Editor.Tests.AssetZoo
{
    /// <summary>
    /// Where the zoo puts things. Both halves are arithmetic over a count and a size, so neither needs
    /// a prefab, a scene or an asset to answer.
    /// <para>
    /// A wrong answer here is not an exception, it is a zoo that looks nearly right: a row that wraps
    /// one item late, a ring whose items overlap, a prop sunk halfway into the floor. The settings are
    /// the shipped defaults, so what is pinned is what a first build actually produces.
    /// </para>
    /// </summary>
    public sealed class ZooLayoutTests
    {
        private const float DefaultRadius = 10f;
        private const int GridColumns = 5;
        private const float Step = 3f;
        private const float Tolerance = 0.0001f;

        private static readonly Vector3 UnitCell = Vector3.one;

        /// <summary>A row fills every column before the next one starts.</summary>
        [Test]
        public void AGridWrapsAfterTheLastColumn()
        {
            LayoutResult result = Grid(12);

            Assert.That(result.Positions[GridColumns - 1].x, Is.EqualTo(Step * (GridColumns - 1)));
            Assert.That(result.Positions[GridColumns], Is.EqualTo(new Vector3(0f, 0f, Step)));
        }

        /// <summary>Spacing sits between the cells, so the step is the cell plus the gap.</summary>
        [Test]
        public void AGridStepsByTheCellPlusTheSpacing()
        {
            LayoutResult result = Grid(2);

            Assert.That(result.Positions[0], Is.EqualTo(Vector3.zero));
            Assert.That(result.Positions[1], Is.EqualTo(new Vector3(Step, 0f, 0f)));
        }

        /// <summary>
        /// The reported depth grows a row at a time, which is what pushes the next category clear of
        /// this one rather than through it.
        /// </summary>
        [Test]
        public void AGridReportsOneRowOfDepthPerStartedRow()
        {
            Assert.That(Grid(3).TotalSize.z, Is.EqualTo(Step));
            Assert.That(Grid(12).TotalSize.z, Is.EqualTo(Step * 3f));
        }

        /// <summary>
        /// The width is the full grid, not the items in it. Three items in a five column grid still
        /// reserve five columns, so a half empty category does not leave the next one overlapping it.
        /// </summary>
        [Test]
        public void AGridReservesEveryColumnEvenWhenItIsNotFull()
            => Assert.That(Grid(3).TotalSize.x, Is.EqualTo(Step * GridColumns));

        /// <summary>A line walks the X axis and stays on it.</summary>
        [Test]
        public void ALineStepsAlongOneAxis()
        {
            LayoutResult result = Line(4);

            Assert.That(result.Positions[3], Is.EqualTo(new Vector3(Step * 3f, 0f, 0f)));
            Assert.That(result.TotalSize.x, Is.EqualTo(Step * 4f));
        }

        /// <summary>A ring no smaller than asked for, when the items fit inside it.</summary>
        [Test]
        public void ACircleKeepsTheRadiusItWasGiven()
        {
            LayoutResult result = Circle(8);

            Assert.That(Radius(result), Is.EqualTo(DefaultRadius).Within(Tolerance));
        }

        /// <summary>
        /// The one rule worth having here. Enough items and the asked for radius would sit them on top
        /// of each other, so the ring grows instead.
        /// </summary>
        [Test]
        public void ACircleGrowsRatherThanOverlapItems()
        {
            LayoutResult result = Circle(25);

            Assert.That(Radius(result), Is.GreaterThan(DefaultRadius));
        }

        /// <summary>Every item sits on the ring, whatever the radius turned out to be.</summary>
        [Test]
        public void EveryItemSitsOnTheRing()
        {
            LayoutResult result = Circle(25);
            float radius = Radius(result);

            foreach (Vector3 position in result.Positions)
                Assert.That(new Vector2(position.x, position.z).magnitude, Is.EqualTo(radius).Within(Tolerance));
        }

        /// <summary>An empty category lays out nothing rather than failing.</summary>
        [Test]
        public void AnEmptyCategoryPlacesNothing()
        {
            Assert.That(Grid(0).Positions, Is.Empty);
            Assert.That(Line(0).Positions, Is.Empty);
            Assert.That(Circle(0).Positions, Is.Empty);
        }

        /// <summary>Each layout name builds the arrangement it is named after.</summary>
        /// <param name="type">
        /// The layout the settings ask for, as its underlying byte. The enum is internal to the package
        /// and a public test method cannot name it in its signature.
        /// </param>
        /// <param name="expected">The strategy that should answer.</param>
        [TestCase((byte)ELayoutType.Circle, typeof(CircleLayoutStrategy))]
        [TestCase((byte)ELayoutType.Grid, typeof(GridLayoutStrategy))]
        [TestCase((byte)ELayoutType.Line, typeof(LineLayoutStrategy))]
        public void EachLayoutNameBuildsItsOwnStrategy(byte type, Type expected)
            => Assert.That(LayoutStrategyFactory.Create((ELayoutType)type), Is.TypeOf(expected));

        /// <summary>The bottom of the box rests on the slot, so a prop stands on the floor.</summary>
        [Test]
        public void GroundAlignmentPutsTheBottomOnTheSlot()
        {
            Bounds bounds = new(new Vector3(0f, 2f, 0f), new Vector3(1f, 4f, 1f));
            Vector3 offset = new GroundAlignment().GetOffset(bounds);

            Assert.That(offset.y, Is.EqualTo(-bounds.min.y));
            Assert.That(bounds.min.y + offset.y, Is.EqualTo(0f));
        }

        /// <summary>Centre alignment puts the middle of the box on the slot instead.</summary>
        [Test]
        public void CenterAlignmentPutsTheMiddleOnTheSlot()
        {
            Bounds bounds = new(new Vector3(1f, 2f, 3f), Vector3.one);

            Assert.That(new CenterAlignment().GetOffset(bounds), Is.EqualTo(-bounds.center));
        }

        /// <summary>
        /// Ground alignment centres the two flat axes as well, so only the height is treated
        /// differently from centring.
        /// </summary>
        [Test]
        public void GroundAlignmentStillCentersTheFlatAxes()
        {
            Bounds bounds = new(new Vector3(1f, 2f, 3f), new Vector3(1f, 4f, 1f));
            Vector3 offset = new GroundAlignment().GetOffset(bounds);

            Assert.That(offset.x, Is.EqualTo(-bounds.center.x));
            Assert.That(offset.z, Is.EqualTo(-bounds.center.z));
        }

        /// <summary>Pivot alignment moves nothing, which is what honouring the authored pivot means.</summary>
        [Test]
        public void PivotAlignmentMovesNothing()
            => Assert.That(new PivotAlignment().GetOffset(new Bounds(Vector3.one, Vector3.one)),
                Is.EqualTo(Vector3.zero));

        /// <summary>Each alignment name builds the strategy it is named after.</summary>
        /// <param name="mode">
        /// The alignment the settings ask for, as its underlying byte, for the same reason.
        /// </param>
        /// <param name="expected">The strategy that should answer.</param>
        [TestCase((byte)EAlignmentMode.Center, typeof(CenterAlignment))]
        [TestCase((byte)EAlignmentMode.Ground, typeof(GroundAlignment))]
        [TestCase((byte)EAlignmentMode.Pivot, typeof(PivotAlignment))]
        public void EachAlignmentNameBuildsItsOwnStrategy(byte mode, Type expected)
            => Assert.That(AlignmentStrategyFactory.Create((EAlignmentMode)mode), Is.TypeOf(expected));

        /// <summary>The radius the ring ended up with, read back from where an item was put.</summary>
        /// <param name="result">A circle layout of at least one item.</param>
        /// <returns>The distance from the origin to the first item.</returns>
        private static float Radius(LayoutResult result)
            => new Vector2(result.Positions[0].x, result.Positions[0].z).magnitude;

        /// <summary>Lays out the given number of unit cells with the shipped settings.</summary>
        /// <param name="itemCount">How many items to place.</param>
        /// <returns>The result.</returns>
        private static LayoutResult Grid(int itemCount)
            => new GridLayoutStrategy().Layout(itemCount, UnitCell, new LayoutSettings());

        /// <summary>Lays out the given number of unit cells in a line.</summary>
        /// <param name="itemCount">How many items to place.</param>
        /// <returns>The result.</returns>
        private static LayoutResult Line(int itemCount)
            => new LineLayoutStrategy().Layout(itemCount, UnitCell, new LayoutSettings());

        /// <summary>Lays out the given number of unit cells on a ring.</summary>
        /// <param name="itemCount">How many items to place.</param>
        /// <returns>The result.</returns>
        private static LayoutResult Circle(int itemCount)
            => new CircleLayoutStrategy().Layout(itemCount, UnitCell, new LayoutSettings());
    }
}