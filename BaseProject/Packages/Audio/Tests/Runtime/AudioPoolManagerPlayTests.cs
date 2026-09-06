using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Base.AudioPackage.Pool;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Base.AudioPackage.PlayTests
{
    /// <summary>
    /// The pool manager builds its pools in <c>Awake</c>, so nothing it does is reachable until frames
    /// are running. What it gets wrong is quiet: a type whose prefab was never assigned has no pool, and
    /// every sound of that type is silent with nothing in the console unless the manager says so itself.
    /// </summary>
    public sealed class AudioPoolManagerPlayTests
    {
        private const string ManagerName = "AudioPoolManager";
        private const string ParentName = "PoolParent";
        private const int PrewarmCount = 2;

        private readonly List<GameObject> _hosts = new();

        /// <summary>Hands back everything the test put in the scene.</summary>
        [TearDown]
        public void Cleanup()
        {
            foreach (GameObject host in _hosts)
            {
                if (host != null)
                    Object.DestroyImmediate(host);
            }

            _hosts.Clear();
        }

        /// <summary>
        /// Every type in the enum has a prefab field, so a fully wired manager has to answer for all of
        /// them. A type without a pool is a type that plays nothing.
        /// </summary>
        [UnityTest]
        public IEnumerator EveryAudioTypeGetsAPool()
        {
            AudioPoolManager manager = CreateManager(out Transform _, skipUi: false);

            yield return null;

            foreach (EAudioType type in Enum.GetValues(typeof(EAudioType)))
                Assert.That(manager.GetAudioSource(type), Is.Not.Null, $"{type} has no pool");
        }

        /// <summary>
        /// Prewarming is the whole point: the sources exist before the first sound plays rather than
        /// being built during it. Four types at two each is eight objects under the parent.
        /// </summary>
        [UnityTest]
        public IEnumerator EveryPoolPrewarmsTheRequestedNumberOfSources()
        {
            CreateManager(out Transform parent, skipUi: false);

            yield return null;

            int types = Enum.GetValues(typeof(EAudioType)).Length;

            Assert.That(parent.childCount, Is.EqualTo(types * PrewarmCount));
        }

        /// <summary>Prewarmed sources wait switched off, otherwise they are objects nobody owns.</summary>
        [UnityTest]
        public IEnumerator PrewarmedSourcesWaitDeactivated()
        {
            CreateManager(out Transform parent, skipUi: false);

            yield return null;

            Assert.That(AudioPlayTestObjects.CountActiveChildren(parent), Is.EqualTo(0));
        }

        /// <summary>
        /// The parent is optional, so leaving it empty has to fall back to the manager rather than
        /// dropping every pooled source loose in the scene root.
        /// </summary>
        [UnityTest]
        public IEnumerator AnEmptyParentFallsBackToTheManagerItself()
        {
            AudioPoolManager manager = CreateManager(out Transform _, skipUi: false, useParent: false);

            yield return null;

            Assert.That(manager.transform.childCount, Is.EqualTo(Enum.GetValues(typeof(EAudioType)).Length
                * PrewarmCount));
        }

        /// <summary>
        /// A prefab nobody assigned is the mistake this manager exists to catch. It says which type it
        /// happened to, once at startup, instead of leaving that type quietly silent.
        /// </summary>
        [UnityTest]
        public IEnumerator AMissingPrefabIsReportedAndLeavesThatTypeSilent()
        {
            LogAssert.Expect(LogType.Error, new Regex(nameof(EAudioType)));

            AudioPoolManager manager = CreateManager(out Transform _, skipUi: true);

            yield return null;

            Assert.That(manager.GetAudioSource(EAudioType.Ui), Is.Null);
            Assert.That(manager.GetAudioSource(EAudioType.Music), Is.Not.Null);
        }

        /// <summary>
        /// A released source is the one the next take gets. Building a second while the first sits free
        /// is the leak the pool exists to prevent.
        /// </summary>
        [UnityTest]
        public IEnumerator AReleasedSourceIsHandedOutAgain()
        {
            AudioPoolManager manager = CreateManager(out Transform parent, skipUi: false);

            yield return null;

            AudioSource taken = manager.GetAudioSource(EAudioType.Sfx2D);
            int childrenAfterTake = parent.childCount;

            manager.ReleaseAudioSource(EAudioType.Sfx2D, taken);

            Assert.That(taken.gameObject.activeSelf, Is.False);
            Assert.That(manager.GetAudioSource(EAudioType.Sfx2D), Is.Not.Null);
            Assert.That(parent.childCount, Is.EqualTo(childrenAfterTake));
        }

        /// <summary>
        /// Clearing hands every source back at once, and whoever was tracking them has to hear about it.
        /// Without the event the manager would keep handing out sources the pool already reclaimed.
        /// </summary>
        [UnityTest]
        public IEnumerator ClearingEveryPoolRaisesTheEvent()
        {
            AudioPoolManager manager = CreateManager(out Transform parent, skipUi: false);

            yield return null;

            int raised = 0;
            manager.PoolsCleared += () => raised++;

            manager.GetAudioSource(EAudioType.Sfx2D);
            manager.GetAudioSource(EAudioType.Music);
            manager.ClearPools();

            Assert.That(raised, Is.EqualTo(1));
            Assert.That(AudioPlayTestObjects.CountActiveChildren(parent), Is.EqualTo(0));
        }

        /// <summary>Clearing one type raises it as well, since its sources went back too.</summary>
        [UnityTest]
        public IEnumerator ClearingOnePoolRaisesTheEvent()
        {
            AudioPoolManager manager = CreateManager(out Transform parent, skipUi: false);

            yield return null;

            int raised = 0;
            manager.PoolsCleared += () => raised++;

            manager.GetAudioSource(EAudioType.Sfx2D);
            manager.ClearPool(EAudioType.Sfx2D);

            Assert.That(raised, Is.EqualTo(1));
            Assert.That(AudioPlayTestObjects.CountActiveChildren(parent), Is.EqualTo(0));
        }

        /// <summary>
        /// Clearing a type that has no pool is a caller mistake rather than a lifecycle event, so it is
        /// reported and nothing is raised. Raising anyway would tell listeners to drop sources that are
        /// still playing.
        /// </summary>
        [UnityTest]
        public IEnumerator ClearingATypeWithoutAPoolIsReportedAndRaisesNothing()
        {
            LogAssert.Expect(LogType.Error, new Regex(nameof(EAudioType)));

            AudioPoolManager manager = CreateManager(out Transform _, skipUi: true);

            yield return null;

            int raised = 0;
            manager.PoolsCleared += () => raised++;

            LogAssert.Expect(LogType.Warning, new Regex(nameof(EAudioType)));
            manager.ClearPool(EAudioType.Ui);

            Assert.That(raised, Is.EqualTo(0));
        }

        /// <summary>
        /// Builds a manager on an object that is still switched off, fills its serialized fields, and
        /// switches it on, so <c>Awake</c> runs against a manager that is already wired.
        /// </summary>
        /// <param name="parent">The transform the pools parent their sources to.</param>
        /// <param name="skipUi">Whether to leave the interface prefab unassigned.</param>
        /// <param name="useParent">Whether to assign the parent at all.</param>
        /// <returns>The running manager.</returns>
        private AudioPoolManager CreateManager(out Transform parent, bool skipUi, bool useParent = true)
        {
            GameObject host = new(ManagerName);
            host.SetActive(false);
            _hosts.Add(host);

            AudioPoolManager manager = host.AddComponent<AudioPoolManager>();

            GameObject parentHost = new(ParentName);
            _hosts.Add(parentHost);
            parent = parentHost.transform;

            AudioPlayTestObjects.SetField(manager, AudioPoolManager.PrewarmCountField, PrewarmCount);

            if (useParent)
                AudioPlayTestObjects.SetField(manager, AudioPoolManager.PoolParentField, parent);

            AudioPlayTestObjects.SetField(manager, AudioPoolManager.AudioSource2DPrefabField,
                AudioPlayTestObjects.CreatePrefab(nameof(EAudioType.Sfx2D), _hosts));

            AudioPlayTestObjects.SetField(manager, AudioPoolManager.AudioSource3DPrefabField,
                AudioPlayTestObjects.CreatePrefab(nameof(EAudioType.Sfx3D), _hosts));

            AudioPlayTestObjects.SetField(manager, AudioPoolManager.AudioSourceMusicPrefabField,
                AudioPlayTestObjects.CreatePrefab(nameof(EAudioType.Music), _hosts));

            if (!skipUi)
                AudioPlayTestObjects.SetField(manager, AudioPoolManager.AudioSourceUiPrefabField,
                    AudioPlayTestObjects.CreatePrefab(nameof(EAudioType.Ui), _hosts));

            host.SetActive(true);

            return manager;
        }
    }
}