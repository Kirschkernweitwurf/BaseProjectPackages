using System.Collections.Generic;
using Base.ToolsPackage.Editor.StaticResetChecker;
using NUnit.Framework;

namespace Base.ToolsPackage.Editor.Tests
{
    /// <summary>
    /// Covers what the checker reports on. With Enter Play Mode options on, a static that nothing
    /// clears keeps its value into the next session, so a miss here is a bug that only shows up on the
    /// second run and a false positive is noise in a report nobody then reads.
    /// </summary>
    public sealed class StaticResetScannerTests
    {
        private const string AbsolutePath = "C:/Project/Assets/Probe.cs";
        private const string AssetPath = "Assets/Probe.cs";

        /// <summary>A plain static with nothing clearing it is the case the tool exists for.</summary>
        [Test]
        public void AStaticFieldWithNoResetIsReported()
        {
            List<Finding> findings = Scan("static int counter;");

            Assert.That(NamesOf(findings), Contains.Item("counter"));
        }

        /// <summary>
        /// A static event is the one that bites hardest, because handlers from the previous session
        /// survive and fire into objects that no longer exist.
        /// </summary>
        [Test]
        public void AStaticEventWithNoResetIsReported()
        {
            List<Finding> findings = Scan("static event System.Action Changed;");

            Assert.That(NamesOf(findings), Contains.Item("Changed"));
            Assert.That(findings[0].Kind, Does.Contain("event"));
        }

        /// <summary>Clearing the field in a method that runs on entering play mode is the fix, so it counts.</summary>
        [Test]
        public void AFieldClearedInAResetMethodIsNotReported()
        {
            List<Finding> findings = Scan("static int counter;",
                "[RuntimeInitializeOnLoadMethod]",
                "static void Reset() { counter = 0; }");

            Assert.That(findings, Is.Empty);
        }

        /// <summary>
        /// A method without one of the configured attributes is just a method, or every field assigned
        /// anywhere would count as reset.
        /// </summary>
        [Test]
        public void AFieldClearedInAnOrdinaryMethodIsStillReported()
        {
            List<Finding> findings = Scan("static int counter;",
                "static void Reset() { counter = 0; }");

            Assert.That(NamesOf(findings), Contains.Item("counter"));
        }

        /// <summary>Events can be switched off for a project that handles them another way.</summary>
        [Test]
        public void EventsAreSkippedWhenTheyAreSwitchedOff()
        {
            ScanOptions options = new()
            {
                IncludeEvents = false
            };

            Assert.That(Scan(options, "static event System.Action Changed;"), Is.Empty);
        }

        /// <summary>
        /// A readonly static cannot be reassigned, so it is passed over by default even though what it
        /// points at can still hold state.
        /// </summary>
        [Test]
        public void AReadonlyStaticIsPassedOverByDefault()
            => Assert.That(Scan("static readonly List<int> Items = new();"), Is.Empty);

        /// <summary>Turning that off brings it back, since a mutable collection behind it still leaks.</summary>
        [Test]
        public void AReadonlyStaticIsReportedWhenItIsNotIgnored()
        {
            ScanOptions options = new()
            {
                IgnoreReadonly = false
            };

            Assert.That(NamesOf(Scan(options, "static readonly List<int> Items = new();")),
                Contains.Item("Items"));
        }

        /// <summary>
        /// The marker exists for a static cleared somewhere the scan cannot follow, so it silences one
        /// line rather than the whole file.
        /// </summary>
        [Test]
        public void TheIgnoreMarkerSilencesOneField()
        {
            List<Finding> findings = Scan("static int counted; // reset-ignore",
                "static int uncounted;");

            Assert.That(NamesOf(findings), Contains.Item("uncounted"));
            Assert.That(NamesOf(findings), Has.No.Member("counted"));
        }

        /// <summary>
        /// A static method holds no state, so it is not a field. Reporting one would put every helper
        /// class in the project into the report.
        /// </summary>
        [Test]
        public void AStaticMethodIsNotAField()
            => Assert.That(Scan("static void DoWork() { }"), Is.Empty);

        /// <summary>
        /// The word appearing inside a string is not a declaration. The scanner blanks strings before
        /// it looks, and this is what that is for.
        /// </summary>
        [Test]
        public void TheWordInsideAStringIsNotADeclaration()
            => Assert.That(Scan("void Log() { Print(\"static int counter;\"); }"), Is.Empty);

        /// <summary>Two fields declared together are two leaks, not one.</summary>
        [Test]
        public void SeveralFieldsOnOneLineAreEachReported()
        {
            List<Finding> findings = Scan("static int first, second;");

            Assert.That(NamesOf(findings), Contains.Item("first"));
            Assert.That(NamesOf(findings), Contains.Item("second"));
        }

        /// <summary>The finding points at the line the declaration sits on, so the window can open it.</summary>
        [Test]
        public void AFindingCarriesTheLineItWasFoundOn()
        {
            List<Finding> findings = Scan("int placeholder;",
                "static int counter;");

            Assert.That(findings[0].Line, Is.EqualTo(4));
        }

        /// <summary>The names a scan reported, so a test names a field rather than an index.</summary>
        private static List<string> NamesOf(IReadOnlyList<Finding> findings)
        {
            List<string> names = new();

            foreach (Finding finding in findings)
                names.Add(finding.Name);

            return names;
        }

        /// <summary>Scans the given class body with the default options.</summary>
        private static List<Finding> Scan(params string[] bodyLines) => Scan(new ScanOptions(), bodyLines);

        /// <summary>Wraps the given lines in a class and scans it.</summary>
        private static List<Finding> Scan(ScanOptions options, params string[] bodyLines)
        {
            List<Finding> results = new();
            List<string> lines = new()
            {
                "class Probe",
                "{"
            };

            lines.AddRange(bodyLines);
            lines.Add("}");

            StaticResetScanner.ScanFile(string.Join("\n", lines), AssetPath, AbsolutePath, options, results);

            return results;
        }
    }
}