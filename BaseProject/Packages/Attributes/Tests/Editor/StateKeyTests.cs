using Base.AttributesPackage.Editor.Core;
using NUnit.Framework;

namespace Base.AttributesPackage.Tests
{
    /// <summary>
    /// Covers the keys every piece of per-field editor state is filed under. Two fields that compose
    /// the same key share a foldout, a filter and a first draw flag, which reads as one field moving
    /// on its own when the other is touched.
    /// </summary>
    public sealed class StateKeyTests
    {
        private const int Component = 1;
        private const int InstanceId = 4242;
        private const string Name = "isExpanded";
        private const string Path = "items";

        /// <summary>
        /// A type key is built from the full name, not the short one, so two types of the same name in
        /// different namespaces do not share state.
        /// </summary>
        [Test]
        public void ATypeKeyUsesTheFullTypeName() => Assert.That(StateKey.For(typeof(StateKeyTests), Name),
            Does.StartWith(typeof(StateKeyTests).FullName));

        /// <summary>Two names on one type stay apart.</summary>
        [Test]
        public void TwoNamesOnOneTypeDoNotCollide() => Assert.That(StateKey.For(typeof(StateKeyTests), Name),
            Is.Not.EqualTo(StateKey.For(typeof(StateKeyTests), Path)));

        /// <summary>Two types sharing a name stay apart.</summary>
        [Test]
        public void TwoTypesSharingANameDoNotCollide() => Assert.That(StateKey.For(typeof(StateKeyTests), Name),
            Is.Not.EqualTo(StateKey.For(typeof(StateKey), Name)));

        /// <summary>
        /// A category sits between the type and the name, so a title and a foldout of the same name on
        /// one type are two separate pieces of state.
        /// </summary>
        [Test]
        public void ACategoryKeepsTwoUsesOfOneNameApart() => Assert.That(
            StateKey.For(typeof(StateKeyTests), "titles", Name),
            Is.Not.EqualTo(StateKey.For(typeof(StateKeyTests), "foldouts", Name)));

        /// <summary>An instance key carries the id, so the same field on two objects is two states.</summary>
        [Test]
        public void AnInstanceKeyCarriesTheInstanceId()
        {
            Assert.That(StateKey.For(InstanceId, Path), Does.Contain(InstanceId.ToString()));
            Assert.That(StateKey.For(InstanceId, Path), Is.Not.EqualTo(StateKey.For(InstanceId + 1, Path)));
        }

        /// <summary>
        /// A vector's components are addressed separately, or dragging one axis would move the state of
        /// the next one along with it.
        /// </summary>
        [Test]
        public void TwoComponentsOfOneFieldDoNotCollide() => Assert.That(StateKey.For(Path, Component),
            Is.Not.EqualTo(StateKey.For(Path, Component + 1)));

        /// <summary>
        /// The type key and the instance key are built from different things, so a state filed per type
        /// is never read back as one filed per instance.
        /// </summary>
        [Test]
        public void ATypeKeyAndAnInstanceKeyAreNotTheSame() => Assert.That(StateKey.For(typeof(StateKeyTests), Path),
            Is.Not.EqualTo(StateKey.For(InstanceId, Path)));
    }
}