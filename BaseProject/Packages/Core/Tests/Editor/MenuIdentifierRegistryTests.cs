using System.Collections.Generic;
using Base.CorePackage.MenuManaging.Identifier;
using NUnit.Framework;
using UnityEngine;

namespace Base.CorePackage.Tests
{
    /// <summary>
    /// Covers the lookup the runtime does to turn a menu name back into the asset it belongs to, and
    /// the comparison the generator uses to decide whether the registry asset has to be rewritten.
    /// </summary>
    public sealed class MenuIdentifierRegistryTests
    {
        private const string MissingName = "MID_Missing";
        private const string PauseName = "MID_Pause";
        private const string SettingsName = "MID_Settings";

        private readonly List<Object> _created = new();

        private MenuIdentifierRegistry _registry;
        private MenuIdentifier _pause;
        private MenuIdentifier _settings;

        /// <summary>Builds a registry holding two identifiers.</summary>
        [SetUp]
        public void Build()
        {
            _registry = Create<MenuIdentifierRegistry>(null);
            _pause = Create<MenuIdentifier>(PauseName);
            _settings = Create<MenuIdentifier>(SettingsName);

            _registry.SetEntries(new[]
            {
                _pause,
                _settings
            });
        }

        /// <summary>Assets created in a test are not saved anywhere, so they are destroyed here.</summary>
        [TearDown]
        public void Release()
        {
            foreach (Object asset in _created)
            {
                if (asset != null)
                    Object.DestroyImmediate(asset);
            }

            _created.Clear();
        }

        /// <summary>A registered identifier is found under its asset name.</summary>
        [Test]
        public void ARegisteredIdentifierIsFound()
        {
            Assert.That(_registry.TryGet(PauseName, out MenuIdentifier found), Is.True);
            Assert.That(found, Is.SameAs(_pause));
        }

        /// <summary>A name that was never registered finds nothing.</summary>
        [Test]
        public void AnUnknownNameFindsNothing()
        {
            Assert.That(_registry.TryGet(MissingName, out MenuIdentifier found), Is.False);
            Assert.That(found, Is.Null);
        }

        /// <summary>The lookup goes by exact name, so a different case is a different menu.</summary>
        [Test]
        public void TheLookupIsCaseSensitive()
            => Assert.That(_registry.TryGet(PauseName.ToLowerInvariant(), out MenuIdentifier _), Is.False);

        /// <summary>A registry that was never filled finds nothing instead of throwing.</summary>
        [Test]
        public void AnEmptyRegistryFindsNothing()
        {
            MenuIdentifierRegistry empty = Create<MenuIdentifierRegistry>(null);

            Assert.That(empty.TryGet(PauseName, out MenuIdentifier _), Is.False);
        }

        /// <summary>A gap left by a deleted asset is skipped rather than walked into.</summary>
        [Test]
        public void AMissingEntryIsSkipped()
        {
            _registry.SetEntries(new[]
            {
                null,
                _settings
            });

            Assert.That(_registry.TryGet(SettingsName, out MenuIdentifier found), Is.True);
            Assert.That(found, Is.SameAs(_settings));
        }

        /// <summary>The same entries in the same order count as unchanged.</summary>
        [Test]
        public void TheSameEntriesInTheSameOrderAreEqual() => Assert.That(_registry.EntriesEqual(new[]
        {
            _pause,
            _settings
        }), Is.True);

        /// <summary>A different order is a change, since the asset stores the order.</summary>
        [Test]
        public void ADifferentOrderIsNotEqual() => Assert.That(_registry.EntriesEqual(new[]
        {
            _settings,
            _pause
        }), Is.False);

        /// <summary>A different count is a change.</summary>
        [Test]
        public void ADifferentCountIsNotEqual() => Assert.That(_registry.EntriesEqual(new[]
        {
            _pause
        }), Is.False);

        /// <summary>Nothing compared against nothing counts as unchanged.</summary>
        [Test]
        public void NoEntriesOnBothSidesAreEqual()
        {
            MenuIdentifierRegistry empty = Create<MenuIdentifierRegistry>(null);

            Assert.That(empty.EntriesEqual(null), Is.True);
        }

        private T Create<T>(string assetName) where T : ScriptableObject
        {
            T asset = ScriptableObject.CreateInstance<T>();

            if (!string.IsNullOrEmpty(assetName))
                asset.name = assetName;

            _created.Add(asset);

            return asset;
        }
    }
}