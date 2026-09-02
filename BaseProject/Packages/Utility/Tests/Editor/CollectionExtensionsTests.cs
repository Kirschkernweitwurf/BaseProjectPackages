using System.Collections.Generic;
using System.Text.RegularExpressions;
using Base.UtilityPackage.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using CollectionExtensions = Base.UtilityPackage.Collections.CollectionExtensions;

namespace Base.UtilityPackage.Tests
{
    /// <summary>
    /// Covers the two helpers a caller reaches for without thinking: wrapping one item as a sequence,
    /// and picking one out of a list. Neither may throw when the list turns out to hold nothing.
    /// </summary>
    public sealed class CollectionExtensionsTests
    {
        private const int DrawCount = 100;
        private const string FirstItem = "Alpha";
        private const string SecondItem = "Beta";
        private const string ThirdItem = "Gamma";

        /// <summary>A single item reads back as a sequence of exactly that item.</summary>
        [Test]
        public void ASingleItemBecomesASequenceOfOne()
        {
            List<string> wrapped = new(CollectionExtensions.Single(FirstItem));

            Assert.That(wrapped.Count, Is.EqualTo(1));
            Assert.That(wrapped[0], Is.EqualTo(FirstItem));
        }

        /// <summary>A picked element comes out of the list it was asked for.</summary>
        [Test]
        public void ARandomElementComesFromTheList()
        {
            List<string> items = new() { FirstItem, SecondItem, ThirdItem };
            List<string> picks = new();

            for (int index = 0; index < DrawCount; index++)
                picks.Add(items.GetRandomElement());

            Assert.That(picks, Is.All.Matches<string>(items.Contains));
        }

        /// <summary>An array binds to the same helper a list does.</summary>
        [Test]
        public void AnArrayPicksTheSameWay()
        {
            string[] items = { FirstItem };

            Assert.That(items.GetRandomElement(), Is.EqualTo(FirstItem));
        }

        /// <summary>A missing list is reported rather than picked from.</summary>
        [Test]
        public void AMissingListIsReported()
        {
            List<string> items = null;

            LogAssert.Expect(LogType.Warning, new Regex(nameof(CollectionExtensions.GetRandomElement)));

            Assert.That(items.GetRandomElement(), Is.Null);
        }

        /// <summary>An empty list has nothing to pick from and is reported.</summary>
        [Test]
        public void AnEmptyListIsReported()
        {
            LogAssert.Expect(LogType.Warning, new Regex(nameof(CollectionExtensions.GetRandomElement)));

            Assert.That(new List<string>().GetRandomElement(), Is.Null);
        }
    }
}