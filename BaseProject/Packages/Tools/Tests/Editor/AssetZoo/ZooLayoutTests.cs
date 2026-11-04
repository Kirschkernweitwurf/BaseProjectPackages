using System.Collections.Generic;
using Base.ToolsPackage.Editor.AssetZoo.Alignment;
using Base.ToolsPackage.Editor.AssetZoo.Config;
using Base.ToolsPackage.Editor.AssetZoo.Generation;
using Base.ToolsPackage.Editor.AssetZoo.Layout;
using Base.ToolsPackage.Editor.AssetZoo.Overlay;
using NUnit.Framework;
using UnityEngine;

namespace Base.ToolsPackage.Editor.Tests.AssetZoo
{
    /// <summary>
    /// Where the zoo puts things. All of it is arithmetic over footprints, so none of it needs a prefab, a scene or
    /// an asset to answer.
    /// <para>
    /// A wrong answer here is not an exception, it is a zoo that looks nearly right: a row that wraps one item late,
    /// a mug lost in a cell made for a building, a prop sunk halfway into the floor. The settings are the shipped
    /// defaults, so what is pinned is what a first build actually produces.
    /// </para>
    /// </summary>
    public sealed class ZooLayoutTests
    {
        private const float Gap = 0.6f;
        private const float GroupGap = 3f;
        private const int PerRow = 6;
        private const float Tolerance = 0.0001f;

        private static readonly Vector2 Cup = new(0.1f, 0.1f);
        private static readonly Vector2 House = new(10f, 8f);

        /// <summary>The whole point: a small asset next to a big one takes only its own room.</summary>
        [Test]
        public void ASmallAssetTakesOnlyItsOwnRoomNextToABigOne()
        {
            LayoutResult result = Rows(House, Cup);

            Assert.That(result.Positions[1].x, Is.EqualTo(House.x + Gap + Cup.x * 0.5f).Within(Tolerance));
        }

        /// <summary>A row stops at its set length and the next one starts behind it.</summary>
        [Test]
        public void ARowWrapsAfterItsLength()
        {
            LayoutResult result = Rows(Many(PerRow + 1, Vector2.one));

            Assert.That(result.Positions[PerRow - 1].y, Is.EqualTo(0.5f).Within(Tolerance));
            Assert.That(result.Positions[PerRow].x, Is.EqualTo(0.5f).Within(Tolerance));
            Assert.That(result.Positions[PerRow].y, Is.EqualTo(1f + Gap + 0.5f).Within(Tolerance));
        }

        /// <summary>Everything in a row lines up on its front edge, so short depths do not float mid row.</summary>
        [Test]
        public void ARowLinesUpOnItsFrontEdge()
        {
            LayoutResult result = Rows(House, Cup);

            float houseFront = result.Positions[0].y - House.y * 0.5f;
            float cupFront = result.Positions[1].y - Cup.y * 0.5f;

            Assert.That(cupFront, Is.EqualTo(houseFront).Within(Tolerance));
        }

        /// <summary>The next row clears the deepest asset of the row before it.</summary>
        [Test]
        public void TheNextRowClearsTheDeepestAsset()
        {
            List<Vector2> footprints = Many(PerRow - 1, Cup);
            footprints.Add(House);
            footprints.Add(Cup);

            LayoutResult result = Rows(footprints);

            Assert.That(result.Positions[PerRow].y, Is.EqualTo(House.y + Gap + Cup.y * 0.5f).Within(Tolerance));
        }

        /// <summary>The size has no gap after the last asset, or a zone would stick out on one side.</summary>
        [Test]
        public void TheSizeEndsAtTheLastAsset()
        {
            LayoutResult result = Rows(Vector2.one, Vector2.one);

            Assert.That(result.Size.x, Is.EqualTo(2f + Gap).Within(Tolerance));
            Assert.That(result.Size.y, Is.EqualTo(1f).Within(Tolerance));
        }

        /// <summary>An empty group takes no floor.</summary>
        [Test]
        public void NothingTakesNoFloor() => Assert.That(Rows().Size, Is.EqualTo(Vector2.zero));

        /// <summary>A line ignores the row length.</summary>
        [Test]
        public void ALineNeverWraps()
        {
            LayoutResult result = new RowLayoutStrategy(true).Layout(Many(PerRow * 3, Vector2.one),
                new LayoutSettings());

            Assert.That(result.Positions[PerRow * 3 - 1].y, Is.EqualTo(0.5f).Within(Tolerance));
        }

        /// <summary>No position is ever negative, on a ring either, so the block after it cannot run into it.</summary>
        [Test]
        public void ARingStaysInsideItsOwnSize()
        {
            LayoutResult result = Circle(Many(8, Vector2.one));

            foreach (Vector2 position in result.Positions)
            {
                Assert.That(position.x, Is.InRange(0f, result.Size.x));
                Assert.That(position.y, Is.InRange(0f, result.Size.y));
            }
        }

        /// <summary>A ring no smaller than asked for, when the assets fit inside it.</summary>
        [Test]
        public void ARingKeepsTheRadiusItWasGiven()
            => Assert.That(RingRadius(Circle(Many(4, Vector2.one))), Is.EqualTo(new LayoutSettings().CircleRadius)
                .Within(Tolerance));

        /// <summary>A ring grows when its assets would otherwise run into each other.</summary>
        [Test]
        public void ARingGrowsWhenItsAssetsWouldNotFit()
        {
            const int Count = 200;

            float radius = RingRadius(Circle(Many(Count, Vector2.one)));
            float needed = Count * (1f + Gap) / (2f * Mathf.PI);

            Assert.That(radius, Is.EqualTo(needed).Within(Tolerance));
        }

        /// <summary>A single asset has no ring to stand on and sits in the middle of its own room.</summary>
        [Test]
        public void ASingleAssetOnARingStandsInTheMiddle()
        {
            LayoutResult result = Circle(Many(1, Vector2.one));

            Assert.That(result.Positions[0], Is.EqualTo(new Vector2(0.5f, 0.5f)));
        }

        /// <summary>The bug this rework started from: a library stretched into a strip kilometers long.</summary>
        [Test]
        public void BlocksStayRoughlySquare()
        {
            LayoutResult result = ZooGroupLayout.Arrange(Many(50, new Vector2(10f, 10f)),
                EGroupArrangement.Blocks, GroupGap);

            Assert.That(result.Size.x / result.Size.y, Is.InRange(0.5f, 2.5f));
        }

        /// <summary>A block wider than the whole zoo still gets a row of its own.</summary>
        [Test]
        public void AHugeBlockStillGetsARow()
        {
            List<Vector2> blocks = Many(3, Vector2.one);
            blocks.Insert(1, new Vector2(1000f, 1f));

            LayoutResult result = ZooGroupLayout.Arrange(blocks, EGroupArrangement.Blocks, GroupGap);

            Assert.That(result.Positions[1].x, Is.EqualTo(0f));
        }

        /// <summary>Blocks in one row start clear of each other.</summary>
        [Test]
        public void BlocksInARowClearEachOther()
        {
            LayoutResult result = ZooGroupLayout.Arrange(Many(2, Vector2.one), EGroupArrangement.Row, GroupGap);

            Assert.That(result.Positions[1], Is.EqualTo(new Vector2(1f + GroupGap, 0f)));
        }

        /// <summary>A column starts every block on a new row.</summary>
        [Test]
        public void BlocksInAColumnStandBehindEachOther()
        {
            LayoutResult result = ZooGroupLayout.Arrange(Many(2, Vector2.one), EGroupArrangement.Column, GroupGap);

            Assert.That(result.Positions[1], Is.EqualTo(new Vector2(0f, 1f + GroupGap)));
        }

        /// <summary>Standing on the floor puts the lowest point at zero, wherever the pivot is.</summary>
        [Test]
        public void GroundAlignmentRestsTheLowestPointOnTheFloor()
        {
            Bounds bounds = new(new Vector3(1f, 2f, 3f), new Vector3(2f, 2f, 2f));

            Vector3 offset = new GroundAlignment().GetOffset(bounds);

            Assert.That(bounds.min.y + offset.y, Is.EqualTo(0f).Within(Tolerance));
            Assert.That(offset.x, Is.EqualTo(-1f).Within(Tolerance));
            Assert.That(offset.z, Is.EqualTo(-3f).Within(Tolerance));
        }

        /// <summary>The middle stands on the place, so floating things can be compared at their centers.</summary>
        [Test]
        public void CenterAlignmentPutsTheMiddleOnThePlace()
        {
            Bounds bounds = new(new Vector3(1f, 2f, 3f), Vector3.one);

            Assert.That(new CenterAlignment().GetOffset(bounds), Is.EqualTo(-bounds.center));
        }

        /// <summary>The pivot is left alone, because that is what this mode is for.</summary>
        [Test]
        public void PivotAlignmentLeavesThePivotAlone()
            => Assert.That(new PivotAlignment().GetOffset(new Bounds(Vector3.one, Vector3.one)),
                Is.EqualTo(Vector3.zero));

        /// <summary>A pivot off to one side needs room on both sides of it, or it reaches into its neighbor.</summary>
        [Test]
        public void AnOffCenterPivotReservesRoomOnBothSides()
        {
            Bounds bounds = new(new Vector3(2f, 0f, 0f), new Vector3(2f, 1f, 1f));

            Assert.That(new PivotAlignment().GetFootprint(bounds).x, Is.EqualTo(6f).Within(Tolerance));
        }

        /// <summary>Each layout type picks the strategy it is named for.</summary>
        [Test]
        public void TheLayoutFactoryMatchesTheEnum()
        {
            Assert.That(LayoutStrategyFactory.Create(ELayoutType.Rows), Is.TypeOf<RowLayoutStrategy>());
            Assert.That(LayoutStrategyFactory.Create(ELayoutType.Line), Is.TypeOf<RowLayoutStrategy>());
            Assert.That(LayoutStrategyFactory.Create(ELayoutType.Circle), Is.TypeOf<CircleLayoutStrategy>());
        }

        /// <summary>Each alignment mode picks the strategy it is named for.</summary>
        [Test]
        public void TheAlignmentFactoryMatchesTheEnum()
        {
            Assert.That(AlignmentStrategyFactory.Create(EAlignmentMode.Ground), Is.TypeOf<GroundAlignment>());
            Assert.That(AlignmentStrategyFactory.Create(EAlignmentMode.Pivot), Is.TypeOf<PivotAlignment>());
            Assert.That(AlignmentStrategyFactory.Create(EAlignmentMode.Center), Is.TypeOf<CenterAlignment>());
        }

        /// <summary>The prefab and its model land in one group, the way an artist sorted them.</summary>
        /// <param name="path">The asset's path.</param>
        /// <param name="expected">The group it should land in.</param>
        [TestCase("Assets/Art/Environment/Cafe/P_Cafe_Chair_01.prefab", "Cafe")]
        [TestCase("Assets/Art/Environment/Cafe/Models Cafe/Cafe_Chair_01.fbx", "Cafe")]
        [TestCase("Assets/Art/Characters/Prefabs/P_Date.prefab", "Characters")]
        [TestCase("Assets/Art/Props/Meshes/FBX/Mug.fbx", "Props")]
        [TestCase("Assets/P_Thing.prefab", "Assets")]
        public void AFolderThatOnlySortsByFileTypeIsSkipped(string path, string expected)
            => Assert.That(ZooFolderGrouping.GetGroupName(path), Is.EqualTo(expected));

        /// <summary>Small things read in centimeters, big things in meters.</summary>
        /// <param name="x">The width.</param>
        /// <param name="y">The height.</param>
        /// <param name="z">The depth.</param>
        /// <param name="expected">How it should read.</param>
        [TestCase(0.13f, 0.1f, 0.09f, "13 x 10 x 9 cm")]
        [TestCase(0.87f, 0.86f, 0.52f, "87 x 86 x 52 cm")]
        [TestCase(2.1f, 0.84f, 1.7f, "2.10 x 0.84 x 1.70 m")]
        public void ASizeReadsInTheUnitThatFits(float x, float y, float z, string expected)
            => Assert.That(ZooItemText.FormatSize(new Vector3(x, y, z)), Is.EqualTo(expected));

        /// <summary>Triangles go short once they run into the thousands.</summary>
        /// <param name="triangles">The number of triangles.</param>
        /// <param name="expected">How it should read.</param>
        [TestCase(592, "592 tris")]
        [TestCase(2022, "2k tris")]
        [TestCase(32392, "32.4k tris")]
        public void TrianglesReadShort(int triangles, string expected)
            => Assert.That(ZooItemText.FormatTriangles(triangles), Is.EqualTo(expected));

        /// <summary>The color of a group never changes between scans, or the map would keep shifting.</summary>
        [Test]
        public void AGroupKeepsItsColor()
            => Assert.That(ZooAutoGenerator.GetCategoryColor("Cafe"),
                Is.EqualTo(ZooAutoGenerator.GetCategoryColor("cafe")));

        private static LayoutResult Rows(params Vector2[] footprints) => Rows(new List<Vector2>(footprints));

        private static LayoutResult Rows(List<Vector2> footprints)
            => new RowLayoutStrategy(false).Layout(footprints, new LayoutSettings());

        private static LayoutResult Circle(List<Vector2> footprints)
            => new CircleLayoutStrategy().Layout(footprints, new LayoutSettings());

        private static List<Vector2> Many(int count, Vector2 footprint)
        {
            List<Vector2> footprints = new();

            for (int i = 0; i < count; i++)
                footprints.Add(footprint);

            return footprints;
        }

        private static float RingRadius(LayoutResult result)
        {
            Vector2 middle = result.Size * 0.5f;

            return (result.Positions[0] - middle).magnitude;
        }
    }
}