using System.Collections.Generic;
using System.IO;
using Base.UtilityPackage.Editor.Documentation;
using NUnit.Framework;

namespace Base.UtilityPackage.Tests
{
    /// <summary>
    /// Covers how the documentation window finds a docs folder and its start page, and orders what it
    /// found. Opening a file is the operating system's job and is not tested here.
    /// </summary>
    public sealed class DocumentationSourceTests
    {
        private const string MarkedStartPage = "---\ntype: Start Page\npackage: Tweening\n---\n# Tweening Package";
        private const string Overview = "---\ntype: Overview\npackage: Tweening\n---\n# Tweening Components";
        private const string StartPageName = "Tweening Package.md";

        private string _root;

        [SetUp]
        public void SetUp()
        {
            _root = Path.Combine(Path.GetTempPath(), "DocumentationSourceTests_" + Path.GetRandomFileName());
            Directory.CreateDirectory(_root);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_root))
                Directory.Delete(_root, true);
        }

        /// <summary>The start page is the page marked as one, whatever its file name.</summary>
        [Test]
        public void TheStartPageIsFoundByItsType()
        {
            string docs = CreateDocs();
            File.WriteAllText(Path.Combine(docs, "Tweening Components.md"), Overview);
            File.WriteAllText(Path.Combine(docs, StartPageName), MarkedStartPage);

            bool found = DocumentationSource.TryFindFolder(_root, DocumentationSource.PackageDocsFolder,
                out string folder, out string startPage);

            Assert.That(found, Is.True);
            Assert.That(folder, Is.EqualTo(docs));
            Assert.That(startPage, Is.EqualTo(StartPageName));
        }

        /// <summary>Older packages and projects only have an index.md, and must keep working.</summary>
        [Test]
        public void AnIndexWithoutTypeIsTheFallback()
        {
            string docs = CreateDocs();
            File.WriteAllText(Path.Combine(docs, DocumentationSource.FallbackStartPage), "# Docs");

            bool found = DocumentationSource.TryFindFolder(_root, DocumentationSource.PackageDocsFolder,
                out _, out string startPage);

            Assert.That(found, Is.True);
            Assert.That(startPage, Is.EqualTo(DocumentationSource.FallbackStartPage));
        }

        /// <summary>A marked page wins over a leftover index.md.</summary>
        [Test]
        public void AMarkedPageWinsOverTheIndex()
        {
            string docs = CreateDocs();
            File.WriteAllText(Path.Combine(docs, DocumentationSource.FallbackStartPage), "# Old");
            File.WriteAllText(Path.Combine(docs, StartPageName), MarkedStartPage);

            DocumentationSource.TryFindStartPage(docs, out string startPage);

            Assert.That(startPage, Is.EqualTo(StartPageName));
        }

        /// <summary>Only the root of the docs folder holds the start page, not a section folder.</summary>
        [Test]
        public void AMarkedPageInASubfolderDoesNotCount()
        {
            string docs = CreateDocs();
            string section = Path.Combine(docs, "Components");
            Directory.CreateDirectory(section);
            File.WriteAllText(Path.Combine(section, StartPageName), MarkedStartPage);

            Assert.That(DocumentationSource.TryFindStartPage(docs, out string startPage), Is.False);
            Assert.That(startPage, Is.Null);
        }

        /// <summary>A docs folder without a start page has nothing to open, so it is left out.</summary>
        [Test]
        public void AFolderWithoutAStartPageIsSkipped()
        {
            string docs = CreateDocs();
            File.WriteAllText(Path.Combine(docs, "Tweening Components.md"), Overview);

            bool found = DocumentationSource.TryFindFolder(_root, DocumentationSource.PackageDocsFolder,
                out string folder, out string startPage);

            Assert.That(found, Is.False);
            Assert.That(folder, Is.Null);
            Assert.That(startPage, Is.Null);
        }

        /// <summary>A package without a resolved path must not throw.</summary>
        [Test]
        public void AnEmptyRootFindsNothing()
        {
            Assert.That(DocumentationSource.TryFindFolder(string.Empty, DocumentationSource.ProjectDocsFolder,
                out _, out _), Is.False);
        }

        /// <summary>Only the type in the frontmatter counts, written the way Obsidian may save it.</summary>
        [Test]
        public void OnlyTheFrontmatterTypeMarksAStartPage()
        {
            Assert.That(DocumentationSource.IsStartPage(MarkedStartPage), Is.True);
            Assert.That(DocumentationSource.IsStartPage("---\r\ntype: \"Start Page\"\r\n---\r\n# A"), Is.True);
            Assert.That(DocumentationSource.IsStartPage(Overview), Is.False);
            Assert.That(DocumentationSource.IsStartPage("# A\ntype: Start Page"), Is.False);
            Assert.That(DocumentationSource.IsStartPage("---\npackage: A\n---\ntype: Start Page"), Is.False);
            Assert.That(DocumentationSource.IsStartPage(null), Is.False);
        }

        /// <summary>A source made without a start page opens the fallback, never an empty path.</summary>
        [Test]
        public void ASourceWithoutStartPageUsesTheFallback()
        {
            DocumentationSource source = new("Tools", "1.0.0", _root, null, string.Empty);

            Assert.That(source.StartPage, Is.EqualTo(DocumentationSource.FallbackStartPage));
            Assert.That(source.StartPagePath, Is.EqualTo(Path.Combine(_root, DocumentationSource.FallbackStartPage)));
        }

        /// <summary>The project docs are what a team member looks for first; packages follow alphabetically.</summary>
        [Test]
        public void TheProjectComesFirstThenPackagesByName()
        {
            List<DocumentationSource> sources = new()
            {
                new DocumentationSource("Tools", "1.0.0", _root, null, string.Empty),
                new DocumentationSource("This project", string.Empty, _root, null, string.Empty),
                new DocumentationSource("attributes", "1.0.0", _root, null, string.Empty)
            };

            DocumentationSource.Sort(sources);

            Assert.That(sources[0].DisplayName, Is.EqualTo("This project"));
            Assert.That(sources[1].DisplayName, Is.EqualTo("attributes"));
            Assert.That(sources[2].DisplayName, Is.EqualTo("Tools"));
        }

        private string CreateDocs()
        {
            string docs = Path.Combine(_root, DocumentationSource.PackageDocsFolder);
            Directory.CreateDirectory(docs);

            return docs;
        }
    }
}