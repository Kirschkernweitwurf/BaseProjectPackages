using System.Collections.Generic;
using Base.ToolsPackage.Editor.ComponentClipboard;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Base.ToolsPackage.Editor.Tests
{
    /// <summary>
    /// What a paste is going to do, worked out before anything is changed.
    /// <para>
    /// The plan is what the window previews and what the paste then carries out, so the two can never
    /// disagree. Getting it wrong is quiet in the worst way: a component is overwritten that should
    /// have been left alone, or a duplicate is added next to the one that should have been overwritten,
    /// and neither says anything.
    /// </para>
    /// </summary>
    /// <remarks>
    /// The two component types are Unity's own rather than probes written here. A component type has to
    /// come from an assembly Unity can resolve a script for, and one declared inside a test assembly is
    /// not, so adding it hands back nothing. Colliders suit the job: two of them fit on one object,
    /// which is what the pairing rules need, and neither does anything on its own.
    /// </remarks>
    public sealed class ComponentPastePlanTests
    {
        private const string HostName = "Clipboard Host";
        private const string OtherHostName = "Other Host";

        private readonly List<GameObject> _hosts = new();

        /// <summary>Hands back the objects the test built.</summary>
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

        /// <summary>Nothing to paste onto means nothing to do, rather than something to go wrong.</summary>
        [Test]
        public void PastingOntoNothingPlansNothing()
            => Assert.That(ComponentOperations.BuildPastePlan(null, Entries(First()), null), Is.Empty);

        /// <summary>An empty clipboard plans nothing.</summary>
        [Test]
        public void PastingNothingPlansNothing()
            => Assert.That(ComponentOperations.BuildPastePlan(Host(HostName), null, null), Is.Empty);

        /// <summary>
        /// One step per entry, in the order they were copied, because that is the order the window
        /// lists them in and the order they are then applied.
        /// </summary>
        [Test]
        public void EveryEntryGetsItsOwnStepInOrder()
        {
            GameObject target = Host(HostName);
            List<ComponentClipboardEntry> entries = Entries(First(), Second(), First());

            List<ComponentPasteStep> plan = ComponentOperations.BuildPastePlan(target, entries, null);

            Assert.That(plan, Has.Count.EqualTo(3));
            Assert.That(plan[0].Type, Is.EqualTo(typeof(BoxCollider)));
            Assert.That(plan[1].Type, Is.EqualTo(typeof(SphereCollider)));
            Assert.That(plan[2].Type, Is.EqualTo(typeof(BoxCollider)));
        }

        /// <summary>With nothing offered to overwrite, every entry adds a component of its own.</summary>
        [Test]
        public void WithNothingToOverwriteEveryEntryAddsNew()
        {
            GameObject target = Host(HostName);
            target.AddComponent<BoxCollider>();

            List<ComponentPasteStep> plan = ComponentOperations.BuildPastePlan(target, Entries(First()), null);

            Assert.That(plan[0].IsOverwrite, Is.False);
        }

        /// <summary>A lone entry overwrites the one component of its type that was offered.</summary>
        [Test]
        public void ALoneEntryOverwritesTheOfferedComponent()
        {
            GameObject target = Host(HostName);
            BoxCollider existing = target.AddComponent<BoxCollider>();

            List<ComponentPasteStep> plan = ComponentOperations.BuildPastePlan(target, Entries(First()),
                Offer(existing));

            Assert.That(plan[0].IsOverwrite, Is.True);
            Assert.That(plan[0].Targets, Is.EquivalentTo(new Component[]
            {
                existing
            }));
        }

        /// <summary>
        /// A lone entry overwrites every offered component of its type at once. One copied component
        /// applied to a whole selection of them is the point of offering more than one.
        /// </summary>
        [Test]
        public void ALoneEntryOverwritesEveryOfferedComponentOfItsType()
        {
            GameObject target = Host(HostName);
            BoxCollider first = target.AddComponent<BoxCollider>();
            BoxCollider second = target.AddComponent<BoxCollider>();

            List<ComponentPasteStep> plan = ComponentOperations.BuildPastePlan(target, Entries(First()),
                Offer(first, second));

            Assert.That(plan[0].Targets, Has.Count.EqualTo(2));
        }

        /// <summary>
        /// Several entries of one type are paired with the offered components one by one instead, or
        /// the last entry copied would win on every one of them.
        /// </summary>
        [Test]
        public void SeveralEntriesArePairedWithTheOfferedComponentsOneByOne()
        {
            GameObject target = Host(HostName);
            BoxCollider first = target.AddComponent<BoxCollider>();
            BoxCollider second = target.AddComponent<BoxCollider>();

            List<ComponentPasteStep> plan = ComponentOperations.BuildPastePlan(target,
                Entries(First(), First()), Offer(first, second));

            Assert.That(plan[0].Targets, Is.EquivalentTo(new Component[]
            {
                first
            }));

            Assert.That(plan[1].Targets, Is.EquivalentTo(new Component[]
            {
                second
            }));
        }

        /// <summary>An entry left over once the offered components run out adds a new one.</summary>
        [Test]
        public void AnEntryPastTheOfferedComponentsAddsNew()
        {
            GameObject target = Host(HostName);
            BoxCollider only = target.AddComponent<BoxCollider>();

            List<ComponentPasteStep> plan = ComponentOperations.BuildPastePlan(target,
                Entries(First(), First()), Offer(only));

            Assert.That(plan[0].IsOverwrite, Is.True);
            Assert.That(plan[1].IsOverwrite, Is.False);
        }

        /// <summary>An entry of a type nothing offered adds a new component rather than overwriting.</summary>
        [Test]
        public void AnEntryOfAnUnofferedTypeAddsNew()
        {
            GameObject target = Host(HostName);
            BoxCollider existing = target.AddComponent<BoxCollider>();

            List<ComponentPasteStep> plan = ComponentOperations.BuildPastePlan(target, Entries(Second()),
                Offer(existing));

            Assert.That(plan[0].IsOverwrite, Is.False);
        }

        /// <summary>
        /// A component offered from a different object is ignored. The paste writes onto one object,
        /// so overwriting something on another would be a change nobody asked for.
        /// </summary>
        [Test]
        public void AComponentOnAnotherObjectIsNotOverwritten()
        {
            GameObject target = Host(HostName);
            GameObject other = Host(OtherHostName);
            BoxCollider elsewhere = other.AddComponent<BoxCollider>();

            List<ComponentPasteStep> plan = ComponentOperations.BuildPastePlan(target, Entries(First()),
                Offer(elsewhere));

            Assert.That(plan[0].IsOverwrite, Is.False);
        }

        /// <summary>A transform offered for overwriting is ignored, since none can be pasted anyway.</summary>
        [Test]
        public void AnOfferedTransformIsIgnored()
        {
            GameObject target = Host(HostName);

            List<ComponentPasteStep> plan = ComponentOperations.BuildPastePlan(target, Entries(First()),
                Offer(target.transform));

            Assert.That(plan[0].IsOverwrite, Is.False);
        }

        /// <summary>Nothing is not a component, so there is nothing to capture.</summary>
        [Test]
        public void NothingCannotBeCopied() => Assert.That(ComponentOperations.CanCopy(null), Is.False);

        /// <summary>
        /// A transform cannot be copied. Every object has exactly one and it can neither be added nor
        /// removed, so a snapshot of one could never be pasted anywhere.
        /// </summary>
        [Test]
        public void ATransformCannotBeCopied()
            => Assert.That(ComponentOperations.CanCopy(Host(HostName).transform), Is.False);

        /// <summary>Any other component can be copied.</summary>
        [Test]
        public void AnOrdinaryComponentCanBeCopied()
            => Assert.That(ComponentOperations.CanCopy(First()), Is.True);

        /// <summary>Puts the given components in a list to offer for overwriting.</summary>
        /// <param name="components">The components on offer.</param>
        /// <returns>The list the plan reads.</returns>
        private static List<Component> Offer(params Component[] components) => new(components);

        /// <summary>Wraps the given components as clipboard entries, in order.</summary>
        /// <param name="components">The components to snapshot.</param>
        /// <returns>One entry per component.</returns>
        private static List<ComponentClipboardEntry> Entries(params Component[] components)
        {
            List<ComponentClipboardEntry> entries = new();

            foreach (Component component in components)
                entries.Add(new ComponentClipboardEntry(component));

            return entries;
        }

        /// <summary>An object outside any scene, so nothing the test builds can reach the hierarchy.</summary>
        /// <param name="name">Name for the object.</param>
        /// <returns>The object.</returns>
        private GameObject Host(string name)
        {
            GameObject host = EditorUtility.CreateGameObjectWithHideFlags(name, HideFlags.HideAndDontSave);
            _hosts.Add(host);

            return host;
        }

        /// <summary>A component of the first type, on an object of its own.</summary>
        /// <returns>The component.</returns>
        private BoxCollider First() => Host(HostName).AddComponent<BoxCollider>();

        /// <summary>A component of the second type, so a plan can be checked to keep the two apart.</summary>
        /// <returns>The component.</returns>
        private SphereCollider Second() => Host(HostName).AddComponent<SphereCollider>();
    }
}