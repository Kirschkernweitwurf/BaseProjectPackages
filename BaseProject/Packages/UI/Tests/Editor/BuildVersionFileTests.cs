using System.IO;
using Base.UIPackage.Utility;
using NUnit.Framework;

namespace Base.UIPackage.Tests
{
    /// <summary>
    /// The first coverage this package has. The version file is written once per build and read once
    /// per launch, so everything it gets wrong ships: a build number that stops counting, a label that
    /// reads as brackets around nothing, or an exception on the first build of a project that has never
    /// made one.
    /// </summary>
    public sealed class BuildVersionFileTests
    {
        private const string ExistingHeader = "Built on 2026-09-06";
        private const string FileName = "version.txt";
        private const string FolderName = "BuildVersionFileTests";
        private const string NestedFolderName = "Nested";
        private const string Version = "1.2.3";

        private string _root;

        /// <summary>A folder of its own per test, so nothing a test writes reaches the next one.</summary>
        [SetUp]
        public void Prepare()
        {
            _root = Path.Combine(Path.GetTempPath(), FolderName + Path.GetRandomFileName());
            Directory.CreateDirectory(_root);
        }

        /// <summary>The folder is real, so it has to be removed again.</summary>
        [TearDown]
        public void Cleanup()
        {
            if (Directory.Exists(_root))
                Directory.Delete(_root, true);

            _root = null;
        }

        /// <summary>
        /// A project that has never built has no file, and the label reads it on the first launch. Every
        /// caller indexes into the result, so it has to be the full length rather than empty.
        /// </summary>
        [Test]
        public void AMissingFileReadsAsAFullSetOfEmptyLines()
        {
            string[] lines = BuildVersionFile.Read(Path.Combine(_root, FileName));

            Assert.That(lines, Has.Length.EqualTo(BuildVersionFile.LineCount));
            Assert.That(lines[BuildVersionFile.VersionLineIndex], Is.Null);
            Assert.That(lines[BuildVersionFile.BuildNumberLineIndex], Is.Null);
        }

        /// <summary>A file somebody truncated is padded rather than read as far as it goes.</summary>
        [Test]
        public void AShortFileIsPaddedToTheFullLength()
        {
            string path = Write(ExistingHeader);

            string[] lines = BuildVersionFile.Read(path);

            Assert.That(lines, Has.Length.EqualTo(BuildVersionFile.LineCount));
            Assert.That(lines[0], Is.EqualTo(ExistingHeader));
        }

        /// <summary>A file with extra lines is cut back, so the indexes keep meaning what they say.</summary>
        [Test]
        public void ALongFileIsCutBackToTheFullLength()
        {
            string path = Write(ExistingHeader, Version, "7", "extra", "more");

            string[] lines = BuildVersionFile.Read(path);

            Assert.That(lines, Has.Length.EqualTo(BuildVersionFile.LineCount));
            Assert.That(lines[BuildVersionFile.BuildNumberLineIndex], Is.EqualTo("7"));
        }

        /// <summary>What was written comes back, or the count restarts on every build.</summary>
        [Test]
        public void WhatIsWrittenReadsBackTheSame()
        {
            string path = Path.Combine(_root, FileName);
            string[] written =
            {
                ExistingHeader,
                Version,
                "7"
            };

            BuildVersionFile.Write(path, written);

            Assert.That(BuildVersionFile.Read(path), Is.EqualTo(written));
        }

        /// <summary>
        /// The streaming assets folder does not exist in a project that never put anything in it, and
        /// the first build is exactly when it is written to.
        /// </summary>
        [Test]
        public void WritingCreatesAFolderThatDoesNotExistYet()
        {
            string path = Path.Combine(_root, NestedFolderName, FileName);

            BuildVersionFile.Write(path, new string[BuildVersionFile.LineCount]);

            Assert.That(File.Exists(path), Is.True);
        }

        /// <summary>The version of the build being made replaces whatever the last one left.</summary>
        [Test]
        public void AdvancingRecordsTheVersionOfThisBuild()
        {
            string[] lines = BuildVersionFile.Advance(new string[BuildVersionFile.LineCount], Version);

            Assert.That(lines[BuildVersionFile.VersionLineIndex], Is.EqualTo(Version));
        }

        /// <summary>Counting is the point. A stored number goes up by one, once per build.</summary>
        /// <param name="stored">The number the file held.</param>
        /// <param name="expected">The number the build has to get.</param>
        [TestCase("0", "1")]
        [TestCase("7", "8")]
        [TestCase("41", "42")]
        public void AdvancingCountsTheStoredNumberUp(string stored, string expected)
        {
            string[] lines = new string[BuildVersionFile.LineCount];
            lines[BuildVersionFile.BuildNumberLineIndex] = stored;

            BuildVersionFile.Advance(lines, Version);

            Assert.That(lines[BuildVersionFile.BuildNumberLineIndex], Is.EqualTo(expected));
        }

        /// <summary>
        /// A number that cannot be read is a file that was never written or that somebody edited, and
        /// neither is a reason to stop a build. The count starts over instead.
        /// </summary>
        /// <param name="stored">The unreadable value the file held.</param>
        [TestCase(null)]
        [TestCase("")]
        [TestCase("seven")]
        public void AnUnreadableNumberStartsTheCountOver(string stored)
        {
            string[] lines = new string[BuildVersionFile.LineCount];
            lines[BuildVersionFile.BuildNumberLineIndex] = stored;

            BuildVersionFile.Advance(lines, Version);

            Assert.That(lines[BuildVersionFile.BuildNumberLineIndex],
                Is.EqualTo(BuildVersionFile.FirstBuildNumber.ToString()));
        }

        /// <summary>
        /// The first line is not ours. Whatever a project keeps up there has to survive a build, or the
        /// build pipeline quietly eats it.
        /// </summary>
        [Test]
        public void AdvancingLeavesTheFirstLineAlone()
        {
            string[] lines = new string[BuildVersionFile.LineCount];
            lines[0] = ExistingHeader;

            BuildVersionFile.Advance(lines, Version);

            Assert.That(lines[0], Is.EqualTo(ExistingHeader));
        }

        /// <summary>A project that has never built shows nothing, not brackets around nothing.</summary>
        [Test]
        public void AnEmptyFileFormatsAsNothingAtAll()
            => Assert.That(BuildVersionFile.Format(new string[BuildVersionFile.LineCount]), Is.Empty);

        /// <summary>The label a player sees, built from both values.</summary>
        [Test]
        public void AFilledFileFormatsAsTheVersionAndTheBuildNumber()
        {
            string[] lines = new string[BuildVersionFile.LineCount];
            lines[BuildVersionFile.VersionLineIndex] = Version;
            lines[BuildVersionFile.BuildNumberLineIndex] = "7";

            Assert.That(BuildVersionFile.Format(lines), Is.EqualTo($"{Version} [7]"));
        }

        /// <summary>
        /// One value on its own is still worth showing. This is what a hand edited file looks like, and
        /// showing the half that is there beats showing nothing at all.
        /// </summary>
        [Test]
        public void AFileHoldingOnlyOneValueStillFormats()
        {
            string[] lines = new string[BuildVersionFile.LineCount];
            lines[BuildVersionFile.VersionLineIndex] = Version;

            Assert.That(BuildVersionFile.Format(lines), Does.Contain(Version));
        }

        /// <summary>
        /// A build reads, counts and writes in one pass, so the round trip is what actually has to hold
        /// rather than any one of the three.
        /// </summary>
        [Test]
        public void ABuildCountsOnFromWhereTheLastOneStopped()
        {
            string path = Path.Combine(_root, FileName);

            BuildVersionFile.Write(path, BuildVersionFile.Advance(BuildVersionFile.Read(path), Version));
            BuildVersionFile.Write(path, BuildVersionFile.Advance(BuildVersionFile.Read(path), Version));

            Assert.That(BuildVersionFile.Format(BuildVersionFile.Read(path)), Is.EqualTo($"{Version} [2]"));
        }

        /// <summary>Writes the given lines into the test folder and hands back the path.</summary>
        /// <param name="lines">The lines to write.</param>
        /// <returns>The path of the written file.</returns>
        private string Write(params string[] lines)
        {
            string path = Path.Combine(_root, FileName);
            File.WriteAllLines(path, lines);

            return path;
        }
    }
}