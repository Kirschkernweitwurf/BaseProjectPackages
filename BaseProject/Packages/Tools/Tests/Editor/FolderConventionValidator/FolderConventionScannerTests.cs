using System.Collections.Generic;
using System.Text.RegularExpressions;
using Base.ToolsPackage.Editor.FolderConventionValidator;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace Base.ToolsPackage.Editor.Tests
{
    /// <summary>
    /// Covers the rules the validator reports on. Every one of them used to be unreachable, because
    /// the scanner read the live project and a test could only assert against whatever was in the
    /// Assets folder that day. Behind <c>IAssetIndex</c> each case gets a layout written for it.
    /// </summary>
    public sealed class FolderConventionScannerTests
    {
        private const string BackingFieldFormat = "<{0}>k__BackingField";
        private const string Root = "Assets";

        private FolderConventionConfig _config;

        /// <summary>A config per test, so one test's edit cannot decide the next one's outcome.</summary>
        [SetUp]
        public void Prepare() => _config = ScriptableObject.CreateInstance<FolderConventionConfig>();

        /// <summary>The config is never saved, so it has to be destroyed by hand.</summary>
        [TearDown]
        public void Cleanup()
        {
            if (_config != null)
                Object.DestroyImmediate(_config);

            _config = null;
        }

        /// <summary>A layout that follows every rule reports nothing, or the tool cries wolf.</summary>
        [Test]
        public void ACleanLayoutReportsNothing()
        {
            FakeAssetIndex index = new FakeAssetIndex()
                .WithFolder("Assets/Art")
                .WithFolder("Assets/Art/Textures");

            Assert.That(Scan(index), Is.Empty);
        }

        /// <summary>A folder that breaks the configured casing is what the tool is for.</summary>
        [Test]
        public void AFolderBreakingTheNamingStyleIsReported()
        {
            FakeAssetIndex index = new FakeAssetIndex().WithFolder("Assets/art");

            Assert.That(TypesFrom(Scan(index)), Contains.Item(EFolderViolationType.NamingStyle));
        }

        /// <summary>A name on the exception list is allowed to break the style, which is its whole point.</summary>
        [Test]
        public void AnAllowedExceptionMayBreakTheStyle()
        {
            FakeAssetIndex index = new FakeAssetIndex().WithFolder("Assets/_Project");

            Assert.That(Scan(index), Is.Empty);
        }

        /// <summary>A forbidden name is reported as forbidden, not merely as badly cased.</summary>
        [Test]
        public void AForbiddenNameIsReportedAsForbidden()
        {
            FakeAssetIndex index = new FakeAssetIndex().WithFolder("Assets/Temp");

            Assert.That(TypesFrom(Scan(index)), Contains.Item(EFolderViolationType.ForbiddenName));
        }

        /// <summary>
        /// An ignored folder is skipped along with everything inside it, so a third party package does
        /// not fill the report with names its author chose.
        /// </summary>
        [Test]
        public void AnIgnoredFolderHidesItsContentsToo()
        {
            FakeAssetIndex index = new FakeAssetIndex()
                .WithFolder("Assets/Plugins")
                .WithFolder("Assets/Plugins/some_vendor");

            Assert.That(Scan(index), Is.Empty);
        }

        /// <summary>A required folder that is missing is reported, and the window can create it.</summary>
        [Test]
        public void AMissingRequiredFolderIsReportedAsFixable()
        {
            SetRequiredFolders("Assets/Art");

            FakeAssetIndex index = new FakeAssetIndex().WithFolder("Assets");
            List<FolderViolation> violations = Scan(index);

            Assert.That(TypesFrom(violations), Contains.Item(EFolderViolationType.MissingFolder));
            Assert.That(violations[0].IsFixable, Is.True);
        }

        /// <summary>A required folder that exists is not reported.</summary>
        [Test]
        public void ARequiredFolderThatExistsIsNotReported()
        {
            SetRequiredFolders("Assets/Art");

            FakeAssetIndex index = new FakeAssetIndex().WithFolder("Assets/Art");

            Assert.That(Scan(index), Is.Empty);
        }

        /// <summary>An asset sitting straight in the root instead of in a subfolder is reported.</summary>
        [Test]
        public void AnAssetLooseInTheRootIsReported()
        {
            FakeAssetIndex index = new FakeAssetIndex()
                .WithFolder(Root)
                .WithAsset("Assets/Loose.png");

            Assert.That(TypesFrom(Scan(index)), Contains.Item(EFolderViolationType.LooseAsset));
        }

        /// <summary>An asset filed in a subfolder is where it belongs, however deep it sits.</summary>
        [Test]
        public void AnAssetInsideASubfolderIsNotLoose()
        {
            FakeAssetIndex index = new FakeAssetIndex()
                .WithFolder("Assets/Art")
                .WithAsset("Assets/Art/Tile.png");

            Assert.That(Scan(index), Is.Empty);
        }

        /// <summary>
        /// Only the first level past the limit is reported. Reporting every folder below it would bury
        /// the one place the nesting actually went wrong.
        /// </summary>
        [Test]
        public void OnlyTheFirstFolderPastTheDepthLimitIsReported()
        {
            SetMaxDepth(2);

            FakeAssetIndex index = new FakeAssetIndex().WithFolder("Assets/One/Two/Three/Four");
            List<FolderViolation> violations = Scan(index);

            Assert.That(violations, Has.Count.EqualTo(1));
            Assert.That(violations[0].Path, Is.EqualTo("Assets/One/Two/Three"));
        }

        /// <summary>A root that does not exist is a misconfiguration, so it is reported rather than scanned.</summary>
        [Test]
        public void AMissingRootIsReportedAndNothingIsScanned()
        {
            LogAssert.Expect(LogType.Warning, new Regex("does not exist"));

            Assert.That(Scan(new FakeAssetIndex()), Is.Empty);
        }

        /// <summary>Scanning without a project to read is a bug in the caller, not an empty result.</summary>
        [Test]
        public void ScanningWithoutAnIndexIsReported()
        {
            LogAssert.Expect(LogType.Error, new Regex("asset index"));

            Assert.That(FolderConventionScanner.Scan(_config, null), Is.Empty);
        }

        /// <summary>The violation types of a scan, so a test names a rule rather than an index.</summary>
        private static List<EFolderViolationType> TypesFrom(IReadOnlyList<FolderViolation> violations)
        {
            List<EFolderViolationType> types = new();

            foreach (FolderViolation violation in violations)
                types.Add(violation.Type);

            return types;
        }

        /// <summary>The serialized name an auto property's backing field is compiled into.</summary>
        private static string BackingFieldFor(string propertyName) => string.Format(BackingFieldFormat, propertyName);

        /// <summary>Runs the scanner against the given layout.</summary>
        private List<FolderViolation> Scan(FakeAssetIndex index) => FolderConventionScanner.Scan(_config, index);

        /// <summary>
        /// The config exposes its rules read only, so a test edits the serialized backing field the
        /// auto property is compiled into rather than the property itself.
        /// </summary>
        private void SetMaxDepth(int depth)
        {
            SerializedObject serialized = new(_config);

            serialized.FindProperty(BackingFieldFor(nameof(FolderConventionConfig.MaxDepth))).intValue = depth;
            serialized.ApplyModifiedProperties();
            serialized.Dispose();
        }

        /// <summary>Replaces the required folder list with the given entries.</summary>
        private void SetRequiredFolders(params string[] folders)
        {
            SerializedObject serialized = new(_config);
            SerializedProperty list =
                serialized.FindProperty(BackingFieldFor(nameof(FolderConventionConfig.RequiredFolders)));

            list.arraySize = folders.Length;

            for (int index = 0; index < folders.Length; index++)
                list.GetArrayElementAtIndex(index).stringValue = folders[index];

            serialized.ApplyModifiedProperties();
            serialized.Dispose();
        }
    }
}