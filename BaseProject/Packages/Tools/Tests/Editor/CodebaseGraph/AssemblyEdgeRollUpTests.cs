using Base.ToolPackage.Editor.AssemblyGraph.Architecture;
using Base.ToolPackage.Editor.CodebaseGraph;
using Base.ToolPackage.Editor.CodebaseGraph.Model;
using NUnit.Framework;

namespace Base.ToolPackage.Editor.Tests
{
    /// <summary>
    /// Checks that the roll-up only reports dependencies the compiler actually requires. An edge the
    /// asmdef does not declare cannot be real, and a rule built on one reads as an architectural
    /// violation that is not there.
    /// <br/><br/>
    /// The case guarded here is an interface inherited through a base type in a third assembly.
    /// <c>Type.GetInterfaces</c> hands back the inherited set as well as the declared one, so the scan
    /// records a relation the source never wrote and needs no reference for. The AsmdefProbe assembly
    /// is that case in its smallest form: one empty subclass of a type from Base.CorePackage whose
    /// interface lives in Base.ServicePackage, which AsmdefProbe does not reference.
    /// </summary>
    public sealed class AssemblyEdgeRollUpTests
    {
        private const string CoreAssembly = "Base.CorePackage";
        private const string ProbeAssembly = "AsmdefProbe";
        private const string ServiceAssembly = "Base.ServicePackage";

        private AssemblyEdgeGraph _edges;

        /// <summary>Scans the project once and rolls it up for the whole suite.</summary>
        [OneTimeSetUp]
        public void RollUp()
        {
            CodebaseGraphData graph = CodebaseGraphBuilder.Build(null, true);

            Assert.That(graph, Is.Not.Null, "the scan returned nothing");

            _edges = AssemblyEdgeRollUp.Build(graph);

            Assert.That(_edges.Assemblies,
                Contains.Item(ProbeAssembly),
                "the probe assembly was not scanned, so nothing below means anything");
        }

        /// <summary>The relation the probe does write, which has to survive the fold.</summary>
        [Test]
        public void ProbeKeepsTheEdgeToTheAssemblyItNames() => Assert.That(_edges.Find(ProbeAssembly, CoreAssembly),
            Is.Not.Null,
            "the probe subclasses a type from " + CoreAssembly + ", so that edge is required");

        /// <summary>The relation the probe does not write, which the fold has to drop.</summary>
        [Test]
        public void ProbeDoesNotReachTheAssemblyOfAnInheritedInterface() => Assert.That(
            _edges.Find(ProbeAssembly, ServiceAssembly),
            Is.Null,
            "the probe names nothing from "
            + ServiceAssembly
            + " and does not reference it, so "
            + "an edge here is the inherited interface being counted as a usage");
    }
}