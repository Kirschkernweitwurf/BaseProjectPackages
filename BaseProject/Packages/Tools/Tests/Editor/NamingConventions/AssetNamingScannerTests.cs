using System.Collections.Generic;
using System.Text.RegularExpressions;
using Base.ToolsPackage.Editor.NamingConventions.Data;
using Base.ToolsPackage.Editor.NamingConventions.Scanning;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Base.ToolsPackage.Editor.Tests
{
    /// <summary>
    /// Covers which assets a naming scan looks at in the first place. Everything downstream trusts
    /// this list, so a path wrongly included means the tool proposes renaming something it must not
    /// touch, and one wrongly excluded means a rule silently stops applying.
    /// </summary>
    public sealed class AssetNamingScannerTests
    {
        private const string PackageAsset = "Packages/com.example.thing/Runtime/Icon.png";
        private const string ProjectAsset = "Assets/Art/Tile.png";
        private const string ProjectScript = "Assets/Scripts/Player.cs";

        private AssetNamingRuleSet _ruleSet;

        /// <summary>
        /// A rule set per test. A fresh one seeds nothing, so the ignore list starts empty and only
        /// what a test sets is in play.
        /// </summary>
        [SetUp]
        public void Prepare() => _ruleSet = ScriptableObject.CreateInstance<AssetNamingRuleSet>();

        /// <summary>The rule set is never saved, so it has to be destroyed by hand.</summary>
        [TearDown]
        public void Cleanup()
        {
            if (_ruleSet != null)
                Object.DestroyImmediate(_ruleSet);

            _ruleSet = null;
        }

        /// <summary>An ordinary asset in the project is what the scan is for.</summary>
        [Test]
        public void AnAssetInTheProjectIsCollected()
        {
            FakeAssetIndex index = new FakeAssetIndex().WithAsset(ProjectAsset);

            Assert.That(Collect(index), Contains.Item(ProjectAsset));
        }

        /// <summary>
        /// A folder has no naming rule of its own here, and renaming one moves everything inside it,
        /// so folders are left out of the list entirely.
        /// </summary>
        [Test]
        public void AFolderIsNotCollected()
        {
            FakeAssetIndex index = new FakeAssetIndex().WithFolder("Assets/Art");

            Assert.That(Collect(index), Is.Empty);
        }

        /// <summary>
        /// Anything outside the project is not the project's to rename, so a path that starts
        /// somewhere else is skipped whatever it is.
        /// </summary>
        [Test]
        public void APathOutsideTheProjectIsNotCollected()
        {
            FakeAssetIndex index = new FakeAssetIndex().WithAsset("Library/ShaderCache/Thing.asset");

            Assert.That(Collect(index), Is.Empty);
        }

        /// <summary>
        /// Packages are somebody else's assets and cannot be renamed, so they are out unless the
        /// project says otherwise.
        /// </summary>
        [Test]
        public void APackageAssetIsSkippedByDefault()
        {
            FakeAssetIndex index = new FakeAssetIndex().WithAsset(PackageAsset);

            Assert.That(Collect(index), Is.Empty);
        }

        /// <summary>A project that owns its packages can opt them back in.</summary>
        [Test]
        public void APackageAssetIsCollectedWhenPackagesAreIncluded()
        {
            _ruleSet.IncludePackages = true;

            FakeAssetIndex index = new FakeAssetIndex().WithAsset(PackageAsset);

            Assert.That(Collect(index), Contains.Item(PackageAsset));
        }

        /// <summary>
        /// Renaming a script breaks the class inside it, so scripts stay out until somebody asks for
        /// them explicitly.
        /// </summary>
        [Test]
        public void AScriptIsSkippedByDefault()
        {
            FakeAssetIndex index = new FakeAssetIndex().WithAsset(ProjectScript);

            Assert.That(Collect(index), Is.Empty);
        }

        /// <summary>Scripts come back in when the project accepts what that costs.</summary>
        [Test]
        public void AScriptIsCollectedWhenScriptsAreIncluded()
        {
            _ruleSet.IncludeScripts = true;

            FakeAssetIndex index = new FakeAssetIndex().WithAsset(ProjectScript);

            Assert.That(Collect(index), Contains.Item(ProjectScript));
        }

        /// <summary>
        /// The exclusions stack rather than replace each other, so one asset getting in does not drag
        /// the rest of a mixed project in with it.
        /// </summary>
        [Test]
        public void OnlyTheAssetsInScopeSurviveAMixedProject()
        {
            FakeAssetIndex index = new FakeAssetIndex()
                .WithFolder("Assets/Art")
                .WithAsset(ProjectAsset)
                .WithAsset(ProjectScript)
                .WithAsset(PackageAsset);

            Assert.That(Collect(index), Is.EqualTo(new List<string>
            {
                ProjectAsset
            }));
        }

        /// <summary>Collecting without rules is a bug in the caller, not an empty project.</summary>
        [Test]
        public void CollectingWithoutARuleSetIsReported()
        {
            LogAssert.Expect(LogType.Error, new Regex(nameof(AssetNamingRuleSet)));

            Assert.That(AssetNamingScanner.CollectAssetPaths(null, new FakeAssetIndex()), Is.Empty);
        }

        /// <summary>Collecting without a project to read is likewise a bug in the caller.</summary>
        [Test]
        public void CollectingWithoutAnIndexIsReported()
        {
            LogAssert.Expect(LogType.Error, new Regex("asset index"));

            Assert.That(AssetNamingScanner.CollectAssetPaths(_ruleSet, null), Is.Empty);
        }

        /// <summary>Runs the collection against the given layout.</summary>
        private List<string> Collect(FakeAssetIndex index)
            => AssetNamingScanner.CollectAssetPaths(_ruleSet, index);
    }
}