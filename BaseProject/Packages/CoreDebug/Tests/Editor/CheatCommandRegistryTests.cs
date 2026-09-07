using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using Base.CoreDebugPackage.DebugMenu.CheatConsole;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Base.CoreDebugPackage.Tests
{
    /// <summary>
    /// Which methods become cheat commands.
    /// <para>
    /// Discovery is by attribute and reflection, so a command that stops being found does not fail to
    /// compile and does not throw. It simply is not in the console, and the person looking for it
    /// assumes they misremembered the name.
    /// </para>
    /// </summary>
    public sealed class CheatCommandRegistryTests
    {
        private const string NullAssembliesMessage = "assemblies cannot be null";
        private const string NullTargetsMessage = "targets cannot be null";

        /// <summary>An attributed instance method on a live target becomes a command.</summary>
        [Test]
        public void AnAttributedInstanceMethodIsFound()
            => Assert.That(NamesOf(CheatCommandRegistry.CreateFromTargets(new object[]
            {
                new CheatCommandRegistryProbe()
            })), Contains.Item(CheatCommandRegistryProbe.InstanceCommand));

        /// <summary>A method without the attribute is not, however public it is.</summary>
        [Test]
        public void AMethodWithoutTheAttributeIsNotFound()
            => Assert.That(NamesOf(CheatCommandRegistry.CreateFromTargets(new object[]
            {
                new CheatCommandRegistryProbe()
            })), Does.Not.Contain(nameof(CheatCommandRegistryProbe.NotACommand)));

        /// <summary>
        /// The command is bound to the instance it was found on, so two of them are two commands
        /// rather than one shared entry.
        /// </summary>
        [Test]
        public void EachTargetContributesItsOwnCommand()
        {
            List<CheatCommandInfo> commands = CheatCommandRegistry.CreateFromTargets(new object[]
            {
                new CheatCommandRegistryProbe(),
                new CheatCommandRegistryProbe()
            });

            Assert.That(Count(commands, CheatCommandRegistryProbe.InstanceCommand), Is.EqualTo(2));
        }

        /// <summary>
        /// A destroyed target is passed over rather than reflected on. Scanning one would bind a
        /// command to an object that throws the moment it is invoked.
        /// </summary>
        [Test]
        public void ADestroyedTargetIsPassedOver()
        {
            GameObject host = new(nameof(ADestroyedTargetIsPassedOver))
            {
                hideFlags = HideFlags.HideAndDontSave
            };

            Object.DestroyImmediate(host);

            Assert.That(CheatCommandRegistry.CreateFromTargets(new object[]
            {
                host
            }), Is.Empty);
        }

        /// <summary>No targets means no commands, not an error.</summary>
        [Test]
        public void NoTargetsFindNothing()
            => Assert.That(CheatCommandRegistry.CreateFromTargets(Array.Empty<object>()), Is.Empty);

        /// <summary>
        /// A null list is a caller mistake rather than an empty console, so it is reported and an
        /// empty list comes back instead of an exception mid startup.
        /// </summary>
        [Test]
        public void NullTargetsAreReported()
        {
            LogAssert.Expect(LogType.Error, new Regex(NullTargetsMessage));

            Assert.That(CheatCommandRegistry.CreateFromTargets(null), Is.Empty);
        }

        /// <summary>An attributed static method in a scanned assembly becomes a command.</summary>
        [Test]
        public void AnAttributedStaticMethodIsFound()
            => Assert.That(NamesOf(CheatCommandRegistry.CreateFromStaticMethods(new[]
            {
                typeof(CheatCommandRegistryProbe).Assembly
            })), Contains.Item(CheatCommandRegistryProbe.StaticCommand));

        /// <summary>
        /// An instance method is not picked up by the static scan, or every command would appear twice
        /// as soon as one object registered itself.
        /// </summary>
        [Test]
        public void TheStaticScanLeavesInstanceMethodsAlone()
            => Assert.That(NamesOf(CheatCommandRegistry.CreateFromStaticMethods(new[]
            {
                typeof(CheatCommandRegistryProbe).Assembly
            })), Does.Not.Contain(CheatCommandRegistryProbe.InstanceCommand));

        /// <summary>A null assembly in the list is skipped rather than throwing.</summary>
        [Test]
        public void ANullAssemblyIsSkipped()
            => Assert.That(CheatCommandRegistry.CreateFromStaticMethods(new Assembly[]
            {
                null
            }), Is.Empty);

        /// <summary>A null list of assemblies is reported the same way as null targets.</summary>
        [Test]
        public void NullAssembliesAreReported()
        {
            LogAssert.Expect(LogType.Error, new Regex(NullAssembliesMessage));

            Assert.That(CheatCommandRegistry.CreateFromStaticMethods(null), Is.Empty);
        }

        /// <summary>The names of the commands that were found.</summary>
        /// <param name="commands">The discovered commands.</param>
        /// <returns>One name per command.</returns>
        private static List<string> NamesOf(List<CheatCommandInfo> commands)
        {
            List<string> names = new();

            foreach (CheatCommandInfo command in commands)
                names.Add(command.Attribute.Command);

            return names;
        }

        /// <summary>How many of the commands carry the given name.</summary>
        /// <param name="commands">The discovered commands.</param>
        /// <param name="name">The name to count.</param>
        /// <returns>How many matched.</returns>
        private static int Count(List<CheatCommandInfo> commands, string name)
        {
            int found = 0;

            foreach (CheatCommandInfo command in commands)
            {
                if (command.Attribute.Command == name)
                    found++;
            }

            return found;
        }
    }
}