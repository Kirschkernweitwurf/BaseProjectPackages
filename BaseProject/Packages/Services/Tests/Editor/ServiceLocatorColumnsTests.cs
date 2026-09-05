using Base.ServicesPackage.Editor;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Base.ServicesPackage.Tests
{
    /// <summary>
    /// Covers where the columns of the service window land. The header and every row below it ask the
    /// same object for their rectangles, so an error here does not misplace one cell, it shears the
    /// whole table apart from its own headings.
    /// </summary>
    /// <remarks>
    /// The widths are read from the editor preferences, which hold whatever this machine last dragged
    /// to. Each test writes known ones and puts the originals back, so the outcome does not depend on
    /// how somebody left the window.
    /// </remarks>
    public sealed class ServiceLocatorColumnsTests
    {
        private const float BadgeWidth = 60f;
        private const float NarrowWidth = 300f;
        private const float RowHeight = 20f;
        private const float SavedInstance = 200f;
        private const float SavedService = 150f;
        private const float WideWidth = 1200f;

        private float _originalInstance;
        private float _originalService;

        /// <summary>Remembers what this machine had and writes the widths the tests reason about.</summary>
        [SetUp]
        public void Prepare()
        {
            _originalService = EditorPrefs.GetFloat(ServiceLocatorColumns.ServiceWidthKey, SavedService);
            _originalInstance = EditorPrefs.GetFloat(ServiceLocatorColumns.InstanceWidthKey, SavedInstance);

            EditorPrefs.SetFloat(ServiceLocatorColumns.ServiceWidthKey, SavedService);
            EditorPrefs.SetFloat(ServiceLocatorColumns.InstanceWidthKey, SavedInstance);
        }

        /// <summary>Puts back what this machine had, so a test run leaves no window resized.</summary>
        [TearDown]
        public void Cleanup()
        {
            EditorPrefs.SetFloat(ServiceLocatorColumns.ServiceWidthKey, _originalService);
            EditorPrefs.SetFloat(ServiceLocatorColumns.InstanceWidthKey, _originalInstance);
        }

        /// <summary>The columns run left to right in the order the window reads them.</summary>
        [Test]
        public void TheColumnsRunLeftToRightInOrder()
        {
            ServiceLocatorColumns columns = Laid(WideWidth);
            Rect row = Row(WideWidth);

            Assert.That(columns.Service(row).x, Is.LessThan(columns.Instance(row).x));
            Assert.That(columns.Instance(row).x, Is.LessThan(columns.Location(row).x));
            Assert.That(columns.Location(row).x, Is.LessThan(columns.State(row).x));
            Assert.That(columns.State(row).x, Is.LessThan(columns.Ping(row).x));
        }

        /// <summary>
        /// No column runs into the one after it. Text that overlaps the next heading is the failure
        /// this whole object exists to prevent.
        /// </summary>
        [Test]
        public void NoColumnRunsIntoTheNextOne()
        {
            ServiceLocatorColumns columns = Laid(WideWidth);
            Rect row = Row(WideWidth);

            Assert.That(columns.Service(row).xMax, Is.LessThanOrEqualTo(columns.Instance(row).x));
            Assert.That(columns.Instance(row).xMax, Is.LessThanOrEqualTo(columns.Location(row).x));
            Assert.That(columns.Location(row).xMax, Is.LessThanOrEqualTo(columns.State(row).x));
        }

        /// <summary>
        /// The whole point of one object serving every row: two rows of the same width get the same x
        /// positions, so the headings sit over their own columns.
        /// </summary>
        [Test]
        public void TwoRowsOfOneWidthShareTheirColumnPositions()
        {
            ServiceLocatorColumns columns = Laid(WideWidth);

            Rect header = new(0f, 0f, WideWidth, RowHeight);
            Rect body = new(0f, RowHeight * 5f, WideWidth, RowHeight);

            Assert.That(columns.Service(body).x, Is.EqualTo(columns.Service(header).x));
            Assert.That(columns.Location(body).x, Is.EqualTo(columns.Location(header).x));
        }

        /// <summary>The ping button is pinned to the right rather than following the text columns.</summary>
        [Test]
        public void ThePingButtonIsPinnedToTheRight()
        {
            Rect narrow = Row(NarrowWidth);
            Rect wide = Row(WideWidth);

            float narrowInset = narrow.xMax - Laid(NarrowWidth).Ping(narrow).xMax;
            float wideInset = wide.xMax - Laid(WideWidth).Ping(wide).xMax;

            Assert.That(narrowInset, Is.EqualTo(wideInset));
        }

        /// <summary>Every column stays inside the row it was laid out in.</summary>
        [Test]
        public void NoColumnLeavesTheRow()
        {
            ServiceLocatorColumns columns = Laid(WideWidth);
            Rect row = Row(WideWidth);

            Assert.That(columns.Service(row).x, Is.GreaterThanOrEqualTo(row.x));
            Assert.That(columns.Ping(row).xMax, Is.LessThanOrEqualTo(row.xMax));
        }

        /// <summary>Given room, the columns are the widths that were dragged to.</summary>
        [Test]
        public void WithRoomToSpareTheDraggedWidthsAreUsed()
        {
            ServiceLocatorColumns columns = Laid(WideWidth);
            Rect row = Row(WideWidth);

            Assert.That(columns.Instance(row).width, Is.GreaterThan(columns.Service(row).width));
        }

        /// <summary>
        /// When the window is too narrow for everything, room is taken from Instance before Service.
        /// The wider of the two here is Instance, so it having become the narrower one is the proof.
        /// </summary>
        [Test]
        public void RoomIsTakenFromInstanceBeforeService()
        {
            ServiceLocatorColumns columns = Laid(NarrowWidth);
            Rect row = Row(NarrowWidth);

            Assert.That(columns.Instance(row).width, Is.LessThanOrEqualTo(columns.Service(row).width));
        }

        /// <summary>
        /// A narrow window clamps what is drawn without touching what was dragged to, so widening the
        /// window again gives the columns back rather than leaving them squashed for good.
        /// </summary>
        [Test]
        public void SquashingTheWindowDoesNotLoseTheDraggedWidths()
        {
            ServiceLocatorColumns columns = new();
            columns.Recalculate(Row(NarrowWidth), BadgeWidth);
            columns.Recalculate(Row(WideWidth), BadgeWidth);

            Rect row = Row(WideWidth);

            Assert.That(columns.Instance(row).width, Is.GreaterThan(columns.Service(row).width));
        }

        /// <summary>A row of the given width, laid out at the origin.</summary>
        private static Rect Row(float width) => new(0f, 0f, width, RowHeight);

        /// <summary>Columns already recalculated for a row of the given width.</summary>
        private static ServiceLocatorColumns Laid(float width)
        {
            ServiceLocatorColumns columns = new();
            columns.Recalculate(Row(width), BadgeWidth);

            return columns;
        }
    }
}