using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Base.MemoryProfilerPackage.Tests
{
    /// <summary>
    /// The first coverage this package has. The config decides whether anything gets captured at all
    /// and how often, so its defaults and its one clamp are what a wrong value would ride in on.
    /// </summary>
    public sealed class MemoryProfilerConfigSoTests
    {
        private const float BelowMinimum = 0f;

        private MemoryProfilerConfigSo _config;

        /// <summary>A fresh asset per test, so one test's edit cannot decide the next one's outcome.</summary>
        [SetUp]
        public void Prepare() => _config = ScriptableObject.CreateInstance<MemoryProfilerConfigSo>();

        /// <summary>The instance is never saved, so it has to be destroyed by hand.</summary>
        [TearDown]
        public void Cleanup()
        {
            if (_config != null)
                Object.DestroyImmediate(_config);

            _config = null;
        }

        /// <summary>
        /// Capturing writes snapshots to disk on a timer, so a newly created config has to stay off
        /// until somebody turns it on rather than starting to profile the moment it exists.
        /// </summary>
        [Test]
        public void ANewConfigCapturesNothingUntilItIsEnabled()
            => Assert.That(_config.IsEnabled, Is.False);

        /// <summary>Both capture triggers are on, so enabling the config alone is enough to get snapshots.</summary>
        [Test]
        public void BothCaptureTriggersAreOnByDefault()
        {
            Assert.That(_config.CaptureOnInterval, Is.True);
            Assert.That(_config.CaptureOnSceneLoad, Is.True);
        }

        /// <summary>
        /// The default path mirrors the Memory Profiler preference, so snapshots land where the
        /// window already looks for them without anything being configured.
        /// </summary>
        [Test]
        public void AFreshConfigPointsAtTheDefaultStoragePath()
            => Assert.That(_config.SnapshotStoragePath, Is.EqualTo(MemoryProfilerConfigSo.DefaultStoragePath));

        /// <summary>
        /// A prefix is what separates one snapshot file from the next, so it cannot start out empty.
        /// </summary>
        [Test]
        public void AFreshConfigHasAFileNamePrefix()
            => Assert.That(_config.FileNamePrefix, Is.Not.Empty);

        /// <summary>
        /// An interval of zero would capture every frame and stall the editor, so editing the field
        /// below the floor has to be pulled back up rather than accepted.
        /// </summary>
        [Test]
        public void AnIntervalBelowTheFloorIsClampedWhenTheAssetIsEdited()
        {
            SerializedObject serialized = new(_config);

            serialized.FindProperty(MemoryProfilerConfigSo.IntervalSecondsField).floatValue = BelowMinimum;
            serialized.ApplyModifiedProperties();

            Assert.That(_config.IntervalSeconds, Is.EqualTo(MemoryProfilerConfigSo.MinIntervalSeconds));
        }

        /// <summary>
        /// The runner loads the asset by this path, so the folder it is filed under and the name the
        /// path is built from have to stay in step.
        /// </summary>
        [Test]
        public void TheResourcePathStartsWithTheFolderTheAssetLivesIn()
        {
            Assert.That(MemoryProfilerConfigSo.ResourcePath,
                Does.StartWith(MemoryProfilerConfigSo.ResourceSubFolder));

            Assert.That(MemoryProfilerConfigSo.ResourcePath, Does.EndWith(MemoryProfilerConfigSo.ConfigName));
        }
    }
}