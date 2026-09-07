using System.Collections.Generic;
using System.Linq;
using Base.ToolsPackage.Editor.CodebaseGraph.Architecture;
using Base.ToolsPackage.Editor.CodebaseGraph.Model;
using NUnit.Framework;

namespace Base.ToolsPackage.Editor.Tests.CodebaseGraph
{
    /// <summary>
    /// Which assembly references the roll-up says are needed.
    /// <para>
    /// The report built on this tells you a declared reference can go, and acting on that is what
    /// stops a build. The case that matters is the quiet one: naming a class is enough to make the
    /// compiler load everything it derives from, so an assembly needs a reference to whatever declares
    /// the base even though it writes that name nowhere.
    /// </para>
    /// </summary>
    public sealed class AssemblyEdgeRollUpTests
    {
        private const string BaseAssembly = "Probe.Services";
        private const string MiddleAssembly = "Probe.Core";
        private const string UserAssembly = "Probe.Editor";

        /// <summary>An assembly that names a type in another one gets the obvious edge.</summary>
        [Test]
        public void UsingATypeMakesAnEdgeToItsAssembly()
        {
            AssemblyEdgeGraph graph = Build();

            Assert.That(Targets(graph, UserAssembly), Contains.Item(MiddleAssembly));
        }

        /// <summary>
        /// The edge the compiler needs and nobody writes. The used type derives from one in a third
        /// assembly, so that reference has to be declared even though the name never appears.
        /// </summary>
        [Test]
        public void UsingATypeMakesAnEdgeToWhereItsBaseLives()
        {
            AssemblyEdgeGraph graph = Build();

            Assert.That(Targets(graph, UserAssembly), Contains.Item(BaseAssembly));
        }

        /// <summary>The base type is named on the edge, so the report can say why it is needed.</summary>
        [Test]
        public void TheEdgeNamesTheBaseTypeBehindIt()
        {
            AssemblyEdgeInfo edge = Build().Edges
                .First(candidate => candidate.SourceName == UserAssembly && candidate.TargetName == BaseAssembly);

            Assert.That(edge.TargetTypeNames, Contains.Item("Probe.ServiceBase"));
        }

        /// <summary>
        /// A base in the same assembly as the user adds nothing, since an assembly never declares a
        /// reference to itself.
        /// </summary>
        [Test]
        public void ABaseInTheUsingAssemblyIsNotAnEdge()
        {
            AssemblyEdgeGraph graph = Build(baseAssembly: UserAssembly);

            Assert.That(Targets(graph, UserAssembly), Does.Not.Contain(UserAssembly));
        }

        /// <summary>The whole chain counts, not only the first step up from the used type.</summary>
        [Test]
        public void TheWholeBaseChainCounts()
        {
            AssemblyEdgeGraph graph = Build();
            IEnumerable<string> targets = Targets(graph, UserAssembly);

            Assert.That(targets, Contains.Item(MiddleAssembly));
            Assert.That(targets, Contains.Item(BaseAssembly));
        }

        /// <summary>The assemblies an assembly is said to need.</summary>
        /// <param name="graph">The rolled up graph.</param>
        /// <param name="source">The assembly to read the edges of.</param>
        /// <returns>One name per assembly it reaches.</returns>
        private static IEnumerable<string> Targets(AssemblyEdgeGraph graph, string source)
            => graph.Edges.Where(edge => edge.SourceName == source).Select(edge => edge.TargetName);

        /// <summary>
        /// Three types in three assemblies: a user that names a middle class, and a middle class that
        /// derives from a base one.
        /// </summary>
        /// <param name="baseAssembly">Assembly the base class is declared in.</param>
        /// <returns>The rolled up graph.</returns>
        private static AssemblyEdgeGraph Build(string baseAssembly = BaseAssembly)
        {
            CodebaseGraphData graph = new();
            TypeNodeInfo baseType = Type(1, "ServiceBase", baseAssembly);
            TypeNodeInfo middle = Type(2, "Manager", MiddleAssembly);
            TypeNodeInfo user = Type(3, "ManagerWindow", UserAssembly);

            middle.BaseTypeKey = baseType.Key;
            user.AddOutgoing(middle.Key);

            foreach (TypeNodeInfo type in new[]
                     {
                         baseType,
                         middle,
                         user
                     })
                graph.Types[type.Key] = type;

            return AssemblyEdgeRollUp.Build(graph);
        }

        /// <summary>Builds one type node for the probe graph.</summary>
        /// <param name="token">Metadata token standing in for a real one.</param>
        /// <param name="name">Short name of the type.</param>
        /// <param name="assembly">Assembly it is declared in.</param>
        /// <returns>The node.</returns>
        private static TypeNodeInfo Type(int token, string name, string assembly)
            => new(new TypeKey(assembly, token), name, "Probe." + name, "Probe", assembly,
                ETypeKind.Class, EAccessLevel.Public, false, false, false, false);
    }
}