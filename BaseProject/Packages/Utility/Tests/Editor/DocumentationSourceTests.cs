using System.Collections.Generic;
using System.IO;
using Base.UtilityPackage.Editor.Documentation;
using NUnit.Framework;

namespace Base.UtilityPackage.Tests
{
    /// <summary>
    /// Covers how the documentation window finds a docs folder and orders what it found. Opening a
    /// file is the operating system's job and is not tested here.
    /// </summary>
    public sealed class DocumentationSourceTests
    {
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

        /// <summary>A docs folder with an index.md is what the window lists.</summary>
        [Test]
        public void AFolderWithAStartPageIsFound()
        {
            string docs = Path.Combine(_root, DocumentationSource.PackageDocsFolder);
            Directory.CreateDirectory(docs);
            File.WriteAllText(Path.Combine(docs, DocumentationSource.StartPage), "# Docs");

            bool found = DocumentationSource.TryFindFolder(_root, DocumentationSource.PackageDocsFolder,
                out string folder);

            Assert.That(found, Is.True);
            Assert.That(folder, Is.EqualTo(docs));
        }

        /// <summary>A docs folder without an index.md has nothing to open, so it is left out.</summary>
        [Test]
        public void AFolderWithoutAStartPageIsSkipped()
        {
            Directory.CreateDirectory(Path.Combine(_root, DocumentationSource.PackageDocsFolder));

            bool found = DocumentationSource.TryFindFolder(_root, DocumentationSource.PackageDocsFolder,
                out string folder);

            Assert.That(found, Is.False);
            Assert.That(folder, Is.Null);
        }

        /// <summary>A package without a resolved path must not throw.</summary>
        [Test]
        public void AnEmptyRootFindsNothing()
        {
            Assert.That(DocumentationSource.TryFindFolder(string.Empty, DocumentationSource.ProjectDocsFolder,
                out _), Is.False);
        }

        /// <summary>The project docs are what a team member looks for first; packages follow alphabetically.</summary>
        [Test]
        public void TheProjectComesFirstThenPackagesByName()
        {
            List<DocumentationSource> sources = new()
            {
                new DocumentationSource("Tools", "1.0.0", _root, string.Empty),
                new DocumentationSource("This project", string.Empty, _root, string.Empty),
                new DocumentationSource("attributes", "1.0.0", _root, string.Empty)
            };

            DocumentationSource.Sort(sources);

            Assert.That(sources[0].DisplayName, Is.EqualTo("This project"));
            Assert.That(sources[1].DisplayName, Is.EqualTo("attributes"));
            Assert.That(sources[2].DisplayName, Is.EqualTo("Tools"));
        }
    }
}