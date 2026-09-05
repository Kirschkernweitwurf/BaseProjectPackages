using System.IO;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace Base.MemoryProfilerPackage.Tests
{
    /// <summary>
    /// Covers where a snapshot is written. The path is configured project relative so the asset can be
    /// committed, but a snapshot is taken against the file system, so anything that resolves it wrong
    /// writes captures somewhere nobody looks or fails to write them at all.
    /// </summary>
    public sealed class MemoryProfilerRunnerTests
    {
        private const string AbsolutePath = "/tmp/BaseMemoryCaptures";
        private const string MissingConfigMessage = "config";
        private const string RelativePath = "./MemoryCaptures";
        private const string WindowsAbsolutePath = @"C:\BaseMemoryCaptures";

        private MemoryProfilerConfigSo _config;

        /// <summary>A config per test, so one test's path cannot decide the next one's outcome.</summary>
        [SetUp]
        public void Prepare() => _config = ScriptableObject.CreateInstance<MemoryProfilerConfigSo>();

        /// <summary>The config is never saved, so it has to be destroyed by hand.</summary>
        [TearDown]
        public void Cleanup()
        {
            if (_config != null)
                Object.DestroyImmediate(_config);

            _config = null;
        }

        /// <summary>
        /// A relative path is resolved against the project folder, not the Assets folder inside it,
        /// which is what makes the default land next to Assets rather than in it.
        /// </summary>
        [Test]
        public void ARelativePathIsResolvedAgainstTheProjectFolder()
        {
            SetStoragePath(RelativePath);

            string projectFolder = Directory.GetParent(Application.dataPath).FullName;
            string resolved = MemoryProfilerRunner.ResolveStorageDirectory(_config);

            Assert.That(resolved, Does.StartWith(projectFolder));
        }

        /// <summary>Whatever is resolved is absolute, since a snapshot is written by full path.</summary>
        [Test]
        public void TheResolvedPathIsAlwaysAbsolute()
        {
            SetStoragePath(RelativePath);

            Assert.That(Path.IsPathRooted(MemoryProfilerRunner.ResolveStorageDirectory(_config)), Is.True);
        }

        /// <summary>
        /// A path that is already absolute is left as it is. Somebody pointing captures at a scratch
        /// drive means that drive, not a folder of the same name inside the project.
        /// </summary>
        [Test]
        public void AnAbsolutePathIsLeftAsItIs()
        {
            SetStoragePath(RootedPath());

            Assert.That(MemoryProfilerRunner.ResolveStorageDirectory(_config), Is.EqualTo(RootedPath()));
        }

        /// <summary>
        /// An empty path falls back to the default rather than resolving to the project folder itself,
        /// which would scatter captures across the repository root.
        /// </summary>
        [Test]
        public void AnEmptyPathFallsBackToTheDefault()
        {
            SetStoragePath(string.Empty);

            string expected = MemoryProfilerConfigSo.DefaultStoragePath.TrimStart('.', '/', '\\');

            Assert.That(MemoryProfilerRunner.ResolveStorageDirectory(_config), Does.Contain(expected));
        }

        /// <summary>
        /// Resolving without a config is said out loud and still answers with the default, because a
        /// capture already in flight has to be written somewhere.
        /// </summary>
        [Test]
        public void ResolvingWithoutAConfigIsReportedAndFallsBack()
        {
            LogAssert.Expect(LogType.Error, new Regex(MissingConfigMessage));

            string resolved = MemoryProfilerRunner.ResolveStorageDirectory(null);

            Assert.That(Path.IsPathRooted(resolved), Is.True);
        }

        /// <summary>An absolute path in the notation this machine uses.</summary>
        private static string RootedPath() => Application.platform == RuntimePlatform.WindowsEditor
            ? WindowsAbsolutePath
            : AbsolutePath;

        /// <summary>Writes the storage path through the serialized field the config exposes.</summary>
        private void SetStoragePath(string path)
        {
            SerializedObject serialized = new(_config);

            serialized.FindProperty(MemoryProfilerConfigSo.SnapshotStoragePathField).stringValue = path;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            serialized.Dispose();
        }
    }
}