using System.Collections.Generic;
using System.Text.RegularExpressions;
using Base.ToolsPackage.Editor.NamespaceConventionValidator;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace Base.ToolsPackage.Editor.Tests
{
    /// <summary>
    /// Covers the rules the validator reports on. The scanner reads the project through
    /// <c>IAssetIndex</c>, so each case gets a layout and a set of files written for it instead of
    /// asserting against whatever happens to be in the Assets folder that day.
    /// </summary>
    public sealed class NamespaceConventionScannerTests
    {
        private const string AsmdefPath = "Assets/Scripts/Game.asmdef";
        private const string AssemblyAttribute = "[assembly: System.Reflection.AssemblyTitle(\"x\")]";
        private const string BackingFieldFormat = "<{0}>k__BackingField";
        private const string GameAsmdef = "{\"name\":\"Game\",\"rootNamespace\":\"Game\"}";
        private const string MissingIndexMessage = "asset index";
        private const string MissingRootMessage = "does not exist";
        private const string Root = "Assets/Scripts";

        private NamespaceConventionConfig _config;

        /// <summary>A config per test, so one test's edit cannot decide the next one's outcome.</summary>
        [SetUp]
        public void Prepare()
        {
            _config = ScriptableObject.CreateInstance<NamespaceConventionConfig>();
            SetString(nameof(NamespaceConventionConfig.RootFolder), Root);
        }

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
                .WithFolder("Assets/Scripts/Player")
                .WithFile("Assets/Scripts/Player/Health.cs", Source("Player"));

            Assert.That(Scan(index), Is.Empty);
        }

        /// <summary>
        /// The one that catches a move. A namespace naming a folder the file is not in is what a file
        /// dragged elsewhere leaves behind.
        /// </summary>
        [Test]
        public void ANamespaceNamingAnotherFolderIsReported()
        {
            FakeAssetIndex index = new FakeAssetIndex()
                .WithFolder("Assets/Scripts/Player")
                .WithFile("Assets/Scripts/Player/Health.cs", Source("Enemies"));

            Assert.That(TypesFrom(Scan(index)), Contains.Item(ENamespaceViolationType.MismatchedNamespace));
        }

        /// <summary>
        /// A type in the global namespace collides with everything else that skipped its namespace, so
        /// a file that declares none is reported on its own terms rather than as a mismatch.
        /// </summary>
        [Test]
        public void AFileWithoutANamespaceIsReportedAsMissing()
        {
            FakeAssetIndex index = new FakeAssetIndex()
                .WithFolder("Assets/Scripts/Player")
                .WithFile("Assets/Scripts/Player/Health.cs", "internal sealed class Health { }");

            Assert.That(TypesFrom(Scan(index)), Contains.Item(ENamespaceViolationType.MissingNamespace));
        }

        /// <summary>A project that does not want the global namespace reported can turn that half off.</summary>
        [Test]
        public void AMissingNamespaceCanBeAllowed()
        {
            SetBool(nameof(NamespaceConventionConfig.RequireNamespace), false);

            FakeAssetIndex index = new FakeAssetIndex()
                .WithFolder("Assets/Scripts/Player")
                .WithFile("Assets/Scripts/Player/Health.cs", "internal sealed class Health { }");

            Assert.That(Scan(index), Is.Empty);
        }

        /// <summary>
        /// Flattening a package so a consumer writes one using instead of six is a deliberate call, so
        /// a namespace that stops short of its folder passes while it is allowed.
        /// </summary>
        [Test]
        public void AShorterNamespaceIsAllowedWhileTheRuleSaysSo()
        {
            FakeAssetIndex index = new FakeAssetIndex()
                .WithFolder("Assets/Scripts/Player/Stats")
                .WithFile("Assets/Scripts/Player/Stats/Health.cs", Source("Player"));

            Assert.That(Scan(index), Is.Empty);
        }

        /// <summary>Turning the allowance off makes the same file a mismatch, which is the whole point.</summary>
        [Test]
        public void AShorterNamespaceIsReportedWhenTheAllowanceIsOff()
        {
            SetBool(nameof(NamespaceConventionConfig.AllowShorterNamespace), false);

            FakeAssetIndex index = new FakeAssetIndex()
                .WithFolder("Assets/Scripts/Player/Stats")
                .WithFile("Assets/Scripts/Player/Stats/Health.cs", Source("Player"));

            Assert.That(TypesFrom(Scan(index)), Contains.Item(ENamespaceViolationType.MismatchedNamespace));
        }

        /// <summary>
        /// The root namespace is what lets the tool run on a game's own scripts, where no assembly
        /// definition names anything. It sits in front of the path measured from the root folder.
        /// </summary>
        [Test]
        public void TheRootNamespacePrefixesEverythingBelowTheRoot()
        {
            SetString(nameof(NamespaceConventionConfig.RootNamespace), "Game");

            FakeAssetIndex index = new FakeAssetIndex()
                .WithFolder("Assets/Scripts/Player")
                .WithFile("Assets/Scripts/Player/Health.cs", Source("Game.Player"));

            Assert.That(Scan(index), Is.Empty);
        }

        /// <summary>Without the prefix the same file reads as its folder alone.</summary>
        [Test]
        public void WithoutARootNamespaceTheFolderPathStandsOnItsOwn()
        {
            FakeAssetIndex index = new FakeAssetIndex()
                .WithFolder("Assets/Scripts/Player")
                .WithFile("Assets/Scripts/Player/Health.cs", Source("Game.Player"));

            Assert.That(TypesFrom(Scan(index)), Contains.Item(ENamespaceViolationType.MismatchedNamespace));
        }

        /// <summary>
        /// An assembly definition names the namespace of everything below it, so its root wins over the
        /// configured one rather than being stacked on top of it.
        /// </summary>
        [Test]
        public void AnAssemblyDefinitionOverridesTheConfiguredRoot()
        {
            SetString(nameof(NamespaceConventionConfig.RootNamespace), "Ignored");

            FakeAssetIndex index = new FakeAssetIndex()
                .WithFolder("Assets/Scripts/Player")
                .WithFile(AsmdefPath, GameAsmdef)
                .WithFile("Assets/Scripts/Player/Health.cs", Source("Game.Player"));

            Assert.That(Scan(index), Is.Empty);
        }

        /// <summary>
        /// An assembly attribute file holds no type to place, so it belongs in the global namespace and
        /// is skipped by name rather than reported every single scan.
        /// </summary>
        [Test]
        public void AnIgnoredFileNameIsSkipped()
        {
            FakeAssetIndex index = new FakeAssetIndex()
                .WithFolder("Assets/Scripts/Player")
                .WithFile("Assets/Scripts/Player/AssemblyInfo.cs", AssemblyAttribute);

            Assert.That(Scan(index), Is.Empty);
        }

        /// <summary>An ignored folder is skipped whole, which is what third party code is in there for.</summary>
        [Test]
        public void AnIgnoredFolderIsSkippedWhole()
        {
            FakeAssetIndex index = new FakeAssetIndex()
                .WithFolder("Assets/Scripts/ThirdParty/Vendor")
                .WithFile("Assets/Scripts/ThirdParty/Vendor/Thing.cs", Source("Vendor"));

            Assert.That(Scan(index), Is.Empty);
        }

        /// <summary>
        /// A generated file is rewritten on the next import, so its namespace is not something the
        /// project decides and reporting it would only ever be noise.
        /// </summary>
        [Test]
        public void AGeneratedFileIsSkipped()
        {
            FakeAssetIndex index = new FakeAssetIndex()
                .WithFolder("Assets/Scripts/Input")
                .WithFile("Assets/Scripts/Input/Actions.cs", "// <auto-generated>\nnamespace Wrong { }");

            Assert.That(Scan(index), Is.Empty);
        }

        /// <summary>
        /// A tool that writes the marker into what it produces is not itself generated, so the marker
        /// only counts while it sits in the header.
        /// </summary>
        [Test]
        public void AGeneratorCarryingTheMarkerFurtherDownIsStillChecked()
        {
            string padding = new('/', 500);

            FakeAssetIndex index = new FakeAssetIndex()
                .WithFolder("Assets/Scripts/Input")
                .WithFile("Assets/Scripts/Input/Generator.cs",
                    $"{Source("Wrong")}\n{padding}\n// <auto-generated>");

            Assert.That(TypesFrom(Scan(index)), Contains.Item(ENamespaceViolationType.MismatchedNamespace));
        }

        /// <summary>The file scoped form declares a namespace just as much as the block form does.</summary>
        [Test]
        public void AFileScopedNamespaceIsReadTheSameWay()
        {
            FakeAssetIndex index = new FakeAssetIndex()
                .WithFolder("Assets/Scripts/Player")
                .WithFile("Assets/Scripts/Player/Health.cs", "namespace Player;\n\ninternal sealed class Health { }");

            Assert.That(Scan(index), Is.Empty);
        }

        /// <summary>A root that does not exist is reported, rather than passing as a project with no faults.</summary>
        [Test]
        public void AMissingRootIsReported()
        {
            LogAssert.Expect(LogType.Warning, new Regex(MissingRootMessage));

            Assert.That(NamespaceConventionScanner.Scan(_config, new FakeAssetIndex()), Is.Empty);
        }

        /// <summary>Scanning without a project to read is a caller mistake, and it says so.</summary>
        [Test]
        public void ScanningWithoutAnIndexIsReported()
        {
            LogAssert.Expect(LogType.Error, new Regex(MissingIndexMessage));

            Assert.That(NamespaceConventionScanner.Scan(_config, null), Is.Empty);
        }

        private static string Source(string declared) => $"namespace {declared}\n{{\n}}";

        private static List<ENamespaceViolationType> TypesFrom(List<NamespaceViolation> violations)
        {
            List<ENamespaceViolationType> types = new();

            foreach (NamespaceViolation violation in violations)
                types.Add(violation.Type);

            return types;
        }

        private List<NamespaceViolation> Scan(FakeAssetIndex index)
            => NamespaceConventionScanner.Scan(_config, index);

        private void SetBool(string propertyName, bool value)
        {
            SerializedObject serialized = new(_config);

            serialized.FindProperty(string.Format(BackingFieldFormat, propertyName)).boolValue = value;
            serialized.ApplyModifiedProperties();
        }

        private void SetString(string propertyName, string value)
        {
            SerializedObject serialized = new(_config);

            serialized.FindProperty(string.Format(BackingFieldFormat, propertyName)).stringValue = value;
            serialized.ApplyModifiedProperties();
        }
    }
}