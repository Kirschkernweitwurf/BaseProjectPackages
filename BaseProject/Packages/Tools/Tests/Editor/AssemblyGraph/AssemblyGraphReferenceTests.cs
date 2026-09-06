using System.Collections.Generic;
using Base.ToolsPackage.Editor.AssemblyGraph;
using NUnit.Framework;

namespace Base.ToolsPackage.Editor.Tests
{
    /// <summary>
    /// Runs the reference check over the project and asserts the two cases it used to get wrong: an
    /// assembly held only by the base class of a type from a third assembly, and one held only by a
    /// constant the compiler folds into a literal. Neither reaches the compiled metadata, so both
    /// were offered for removal out of a project that could not compile without them.
    /// <para>
    /// Both are asserted against the real project rather than a fixture, because the mistake was not
    /// in the decision but in what the decision was given, and only the real assemblies carry that.
    /// </para>
    /// </summary>
    public sealed class AssemblyGraphReferenceTests
    {
        private List<AssemblyNodeInfo> _nodes;

        /// <summary>Builds the graph once for the whole suite, since the scan reads every assembly.</summary>
        [OneTimeSetUp]
        public void BuildGraph() => _nodes = AssemblyGraphModel.Build();

        /// <summary>A reference no token names, yet the compilation cannot do without, stays credited.</summary>
        /// <param name="assemblyName">Assembly that declares the reference.</param>
        /// <param name="referenceName">Reference that has to stay credited.</param>
        [TestCase("Base.CorePackage.Tests", "Base.ServicesPackage")]
        [TestCase("Base.ToolsPackage.Editor", "Base.UtilityPackage.Editor")]
        public void Build_KeepsAReferenceTheCompilerNeeds(string assemblyName, string referenceName)
        {
            AssemblyReferenceInfo reference = Find(assemblyName, referenceName);

            Assert.That(reference.IsCandidate,
                Is.False,
                $"{assemblyName} cannot compile without {referenceName}, so it must not be offered.");
        }

        private AssemblyReferenceInfo Find(string assemblyName, string referenceName)
        {
            AssemblyNodeInfo node = FindNode(assemblyName);

            Assert.That(node, Is.Not.Null, $"The graph holds no assembly named {assemblyName}.");

            foreach (AssemblyReferenceInfo reference in node.References)
            {
                if (reference.TargetName == referenceName)
                    return reference;
            }

            Assert.Fail($"{assemblyName} no longer declares {referenceName}, so this case moved.");
            return null;
        }

        private AssemblyNodeInfo FindNode(string assemblyName)
        {
            foreach (AssemblyNodeInfo node in _nodes)
            {
                if (node.Name == assemblyName)
                    return node;
            }

            return null;
        }
    }
}