using Base.TweeningPackage.Core.Data;
using NUnit.Framework;

namespace Base.TweeningPackage.Tests
{
    /// <summary>
    /// Covers the loop settings and the copy that keeps a runtime change out of the shared asset it
    /// came from.
    /// </summary>
    public sealed class LoopSettingsTests
    {
        private const int InfiniteLoops = -1;

        /// <summary>Fresh settings play once and stop.</summary>
        [Test]
        public void FreshSettingsPlayOnce()
        {
            LoopSettings settings = new();

            Assert.That(settings.LoopCount, Is.EqualTo(0));
            Assert.That(settings.LoopType, Is.EqualTo(ELoopType.None));
        }

        /// <summary>Settings keep what they were built with.</summary>
        [Test]
        public void SettingsKeepWhatTheyWereBuiltWith()
        {
            LoopSettings settings = new(3, ELoopType.PingPong);

            Assert.That(settings.LoopCount, Is.EqualTo(3));
            Assert.That(settings.LoopType, Is.EqualTo(ELoopType.PingPong));
        }

        /// <summary>Each value can be changed on its own.</summary>
        [Test]
        public void EachValueCanBeChangedOnItsOwn()
        {
            LoopSettings settings = new();

            settings.SetLoopCount(InfiniteLoops);
            settings.SetLoopType(ELoopType.Restart);

            Assert.That(settings.LoopCount, Is.EqualTo(InfiniteLoops));
            Assert.That(settings.LoopType, Is.EqualTo(ELoopType.Restart));
        }

        /// <summary>A copy carries the same values.</summary>
        [Test]
        public void ACopyCarriesTheSameValues()
        {
            LoopSettings original = new(2, ELoopType.Continue);
            LoopSettings copy = original.Copy();

            Assert.That(copy.LoopCount, Is.EqualTo(original.LoopCount));
            Assert.That(copy.LoopType, Is.EqualTo(original.LoopType));
        }

        /// <summary>A copy is its own object, so changing it leaves the asset alone.</summary>
        [Test]
        public void ACopyIsIndependent()
        {
            LoopSettings original = new(2, ELoopType.Continue);
            LoopSettings copy = original.Copy();

            copy.SetLoopCount(InfiniteLoops);
            copy.SetLoopType(ELoopType.PingPong);

            Assert.That(original.LoopCount, Is.EqualTo(2));
            Assert.That(original.LoopType, Is.EqualTo(ELoopType.Continue));
        }
    }
}