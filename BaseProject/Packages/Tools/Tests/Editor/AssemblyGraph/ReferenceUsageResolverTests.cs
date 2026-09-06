using System;
using System.Collections.Generic;
using Base.ToolsPackage.Editor.AssemblyGraph;
using NUnit.Framework;

namespace Base.ToolsPackage.Editor.Tests
{
    /// <summary>
    /// Covers the decision the reference check makes, over data written here rather than read out of
    /// the project, so each of the three sources can be shown to carry a reference on its own.
    /// </summary>
    public sealed class ReferenceUsageResolverTests
    {
        private const string Consumer = "Consumer";
        private const string Middle = "Middle";
        private const string Provider = "Provider";
        private const string ProviderNamespace = "Provider.Namespace";

        /// <summary>A reference the compiled metadata names is used.</summary>
        [Test]
        public void Resolve_CreditsMetadataReference()
        {
            ReferenceUsageResolver resolver = Build(Link(Consumer, Provider),
                Empty(),
                Empty());

            Assert.That(Resolve(resolver, Provider), Is.EqualTo(EReferenceStatus.Used));
        }

        /// <summary>
        /// A reference that declares the base class of something the metadata names is used, which
        /// is the case the metadata table can never show because the base leaves no token behind.
        /// </summary>
        [Test]
        public void Resolve_CreditsAncestryOfAReferencedAssembly()
        {
            ReferenceUsageResolver resolver = Build(Link(Consumer, Middle),
                Link(Middle, Provider),
                Empty());

            Assert.That(Resolve(resolver, Provider), Is.EqualTo(EReferenceStatus.Used));
        }

        /// <summary>
        /// A reference whose namespace a using directive names is used, which is the case a folded
        /// constant produces: the literal is inlined and the assembly is gone from the metadata.
        /// </summary>
        [Test]
        public void Resolve_CreditsNamespaceNamedByUsing()
        {
            ReferenceUsageResolver resolver = Build(Link(Consumer, Middle),
                Empty(),
                Link(ProviderNamespace, Provider));

            EReferenceStatus status = resolver.Resolve(resolver.CollectCredited(Consumer),
                Provider,
                new HashSet<string>(StringComparer.Ordinal)
                {
                    ProviderNamespace
                });

            Assert.That(status, Is.EqualTo(EReferenceStatus.Used));
        }

        /// <summary>
        /// A reference none of the three sources reaches is still reported, so closing the two known
        /// blind spots does not amount to turning the check off.
        /// </summary>
        [Test]
        public void Resolve_ReportsWhatNothingReaches()
        {
            ReferenceUsageResolver resolver = Build(Link(Consumer, Middle),
                Empty(),
                Link(ProviderNamespace, Provider));

            Assert.That(Resolve(resolver, Provider), Is.EqualTo(EReferenceStatus.Candidate));
        }

        /// <summary>An assembly whose metadata could not be read reports nothing either way.</summary>
        [Test]
        public void Resolve_ReportsUnknownWithoutMetadata()
        {
            ReferenceUsageResolver resolver = Build(Empty(), Empty(), Empty());

            Assert.That(Resolve(resolver, Provider), Is.EqualTo(EReferenceStatus.Unknown));
        }

        private static ReferenceUsageResolver Build(Dictionary<string, HashSet<string>> metadata,
            Dictionary<string, HashSet<string>> ancestry,
            Dictionary<string, HashSet<string>> namespaces) => new(metadata, ancestry, namespaces);

        private static EReferenceStatus Resolve(ReferenceUsageResolver resolver, string referenceName)
            => resolver.Resolve(resolver.CollectCredited(Consumer),
                referenceName,
                new HashSet<string>(StringComparer.Ordinal));

        private static Dictionary<string, HashSet<string>> Empty() => new(StringComparer.Ordinal);

        private static Dictionary<string, HashSet<string>> Link(string key, string value) => new(StringComparer.Ordinal)
        {
            {
                key, new HashSet<string>(StringComparer.Ordinal)
                {
                    value
                }
            }
        };
    }
}