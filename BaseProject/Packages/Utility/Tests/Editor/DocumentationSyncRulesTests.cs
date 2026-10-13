using System.Collections.Generic;
using Base.UtilityPackage.Editor.Documentation;
using NUnit.Framework;

namespace Base.UtilityPackage.Tests
{
    /// <summary>
    /// Covers the decisions behind the docs mirror in <c>Docs/Packages</c>: folder names, link rewriting,
    /// which folders go away and the generated pages. Copying files is plain file system work and is not
    /// tested here.
    /// </summary>
    public sealed class DocumentationSyncRulesTests
    {
        private const string Repo = DocumentationSyncRules.RepositoryPackagesUrl;

        private static readonly Dictionary<string, string> Installed = new()
        {
            { "Tools", "Tools" },
            { "Utility", "Utility" },
            { "ControllerSupport", "Controller Support" }
        };

        /// <summary>The Base prefix is the same on every package, so the mirror leaves it out.</summary>
        [Test]
        public void TheBasePrefixIsDroppedFromTheFolderName()
        {
            Assert.That(DocumentationSyncRules.MirrorName("Base Utility"), Is.EqualTo("Utility"));
            Assert.That(DocumentationSyncRules.MirrorName("Base Controller Support"),
                Is.EqualTo("Controller Support"));
            Assert.That(DocumentationSyncRules.MirrorName("Other Package"), Is.EqualTo("Other Package"));
            Assert.That(DocumentationSyncRules.MirrorName("  "), Is.Empty);
        }

        /// <summary>The repository folder is read from the documentation URL every Base package sets.</summary>
        [Test]
        public void TheRepositoryFolderComesFromTheDocumentationUrl()
        {
            bool found = DocumentationSyncRules.TryGetRepositoryFolder(
                Repo + "ControllerSupport/Documentation~/index.md", out string folder);

            Assert.That(found, Is.True);
            Assert.That(folder, Is.EqualTo("ControllerSupport"));
            Assert.That(DocumentationSyncRules.TryGetRepositoryFolder("https://example.com/docs", out _), Is.False);
            Assert.That(DocumentationSyncRules.TryGetRepositoryFolder(null, out _), Is.False);
        }

        /// <summary>A link to an installed package's docs on GitHub opens the local copy instead.</summary>
        [Test]
        public void ALinkToAnInstalledPackageBecomesRelative()
        {
            string page = "See [ids](" + Repo + "Tools/Documentation~/Tools/Generate%20Unique%20Ids.md).";

            string rewritten = DocumentationSyncRules.RewriteLinks(page, "index.md", "Utility", Installed);

            Assert.That(rewritten, Is.EqualTo("See [ids](../Tools/Tools/Generate%20Unique%20Ids.md)."));
        }

        /// <summary>Every folder the page sits in adds one step up, and spaces in names are encoded.</summary>
        [Test]
        public void ANestedPageClimbsOutOfItsFoldersAndEncodesSpaces()
        {
            string page = "[pads](" + Repo + "ControllerSupport/Documentation~/Components/Pad.md#setup)";

            string rewritten = DocumentationSyncRules.RewriteLinks(page, "Tools/Custom Log Handler.md", "Utility",
                Installed);

            Assert.That(rewritten, Is.EqualTo("[pads](../../Controller%20Support/Components/Pad.md#setup)"));
        }

        /// <summary>A link to the docs folder itself opens the package's start page.</summary>
        [Test]
        public void ALinkToTheDocsFolderOpensItsStartPage()
        {
            string page = "[tools](" + Repo + "Tools/Documentation~/)";

            Assert.That(DocumentationSyncRules.RewriteLinks(page, "index.md", "Utility", Installed),
                Is.EqualTo("[tools](../Tools/index.md)"));
        }

        /// <summary>A package that is not installed has no local copy, so only the GitHub link works.</summary>
        [Test]
        public void ALinkToAMissingPackageStaysOnGitHub()
        {
            string page = "[audio](" + Repo + "Audio/Documentation~/index.md)";

            Assert.That(DocumentationSyncRules.RewriteLinks(page, "index.md", "Utility", Installed),
                Is.EqualTo(page));
        }

        /// <summary>Links inside the package's own docs already work in the mirror and are left alone.</summary>
        [Test]
        public void LinksInsideTheOwnDocsAreUnchanged()
        {
            const string page = "[log](Tools/Custom%20Log%20Handler.md) [up](../index.md#top) [web](https://x.y)";

            Assert.That(DocumentationSyncRules.RewriteLinks(page, "Tools/Page.md", "Utility", Installed),
                Is.EqualTo(page));
        }

        /// <summary>A README next to the docs folder is not mirrored, so the link points to GitHub.</summary>
        [Test]
        public void ALinkOutOfTheDocsFolderPointsToGitHub()
        {
            const string page = "See the [README](../README.md).";

            Assert.That(DocumentationSyncRules.RewriteLinks(page, "index.md", "Utility", Installed),
                Is.EqualTo("See the [README](" + Repo + "Utility/README.md)."));
        }

        /// <summary>A relative link into another installed package's docs ends up at its local copy.</summary>
        [Test]
        public void ARelativeLinkIntoAnotherPackageBecomesTheLocalCopy()
        {
            const string page = "[tools](../../Tools/Documentation~/index.md)";

            Assert.That(DocumentationSyncRules.RewriteLinks(page, "index.md", "Utility", Installed),
                Is.EqualTo("[tools](../Tools/index.md)"));
        }

        /// <summary>Folders of packages that were removed go away; installed ones stay, whatever their case.</summary>
        [Test]
        public void OnlyFoldersOfRemovedPackagesAreDeleted()
        {
            List<string> stale = DocumentationSyncRules.FoldersToDelete(
                new[] { "Utility", "audio", "Tools", "Old Package" },
                new[] { "utility", "Tools", "Controller Support" });

            Assert.That(stale, Is.EqualTo(new[] { "audio", "Old Package" }));
        }

        /// <summary>Nothing is deleted when the folder is empty.</summary>
        [Test]
        public void NoFoldersMeansNothingToDelete()
        {
            Assert.That(DocumentationSyncRules.FoldersToDelete(null, new[] { "Utility" }), Is.Empty);
        }

        /// <summary>The generated start page links every package, sorted, with spaces encoded.</summary>
        [Test]
        public void TheIndexLinksEveryPackageInOrder()
        {
            List<DocumentationSource> packages = new()
            {
                new DocumentationSource("Base Utility", "1.8.0", "/u", string.Empty),
                new DocumentationSource("Base Controller Support", "2.0.1", "/c", string.Empty)
            };

            string index = DocumentationSyncRules.BuildIndex(packages);

            int controller = index.IndexOf("| [Controller Support](Controller%20Support/index.md) | 2.0.1 |",
                System.StringComparison.Ordinal);
            int utility = index.IndexOf("| [Utility](Utility/index.md) | 1.8.0 |", System.StringComparison.Ordinal);

            Assert.That(controller, Is.GreaterThan(0));
            Assert.That(utility, Is.GreaterThan(controller));
            Assert.That(index, Does.Contain("Do not edit by hand"));
            Assert.That(index, Does.Not.EndWith("\n"));
        }

        /// <summary>The ignore line is added once and never twice.</summary>
        [Test]
        public void TheIgnoreLineIsAddedOnce()
        {
            string first = DocumentationSyncRules.EnsureIgnoreLine("Library\r\nTemp", out bool added);
            string second = DocumentationSyncRules.EnsureIgnoreLine(first, out bool addedAgain);

            Assert.That(added, Is.True);
            Assert.That(first, Is.EqualTo("Library\r\nTemp\r\nDocs/Packages\r\n"));
            Assert.That(addedAgain, Is.False);
            Assert.That(second, Is.EqualTo(first));
        }

        /// <summary>A line someone already wrote with slashes around it counts as present.</summary>
        [Test]
        public void AnExistingIgnoreLineWithSlashesCounts()
        {
            const string content = "Library\n/Docs/Packages/\n";

            Assert.That(DocumentationSyncRules.EnsureIgnoreLine(content, out bool added), Is.EqualTo(content));
            Assert.That(added, Is.False);
        }
    }
}