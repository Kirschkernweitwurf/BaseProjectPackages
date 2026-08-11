using Base.ToolPackage.Editor.CodebaseGraph;
using Base.ToolPackage.Editor.CodebaseGraph.Model;
using NUnit.Framework;

namespace Base.ToolPackage.Editor.Tests
{
    /// <summary>
    /// Checks that the scanner is right about what is alive. Everything this tool says rests on that
    /// one judgement, and most of the ways it can be wrong are invisible: a member reachable only
    /// through generated machinery or through a string looks identical to a dead one until something
    /// asserts otherwise.
    /// <br/><br/>
    /// The scan runs once for the whole fixture, and it is asked to include excluded scopes, because a
    /// test assembly is exactly the kind of code findings are normally suppressed for.
    /// <br/><br/>
    /// Member names are written plainly. That is only safe because the text pass skips string literals
    /// and comments, so naming a const here cannot mark it used, and because the literal pass in the IL
    /// only applies to bodies that dispatch by name, which none of these do.
    /// </summary>
    public sealed class LivenessTests
    {
        private const string Behaviour = "Base.ToolPackage.Editor.Tests.Fixtures.FixtureBehaviour";
        private const string Constants = "Base.ToolPackage.Editor.Tests.Fixtures.FixtureConstants";
        private const string Contract = "Base.ToolPackage.Editor.Tests.Fixtures.IFixtureContract";
        private const string DeadCode = "Base.ToolPackage.Editor.Tests.Fixtures.FixtureDeadCode";
        private const string DescribeMember = "Describe";
        private const string NestingHost = "Base.ToolPackage.Editor.Tests.Fixtures.FixtureNestingHost";
        private const string NestingUnused = NestingHost + ".Unused";
        private const string Orphan = "Base.ToolPackage.Editor.Tests.Fixtures.IFixtureOrphan";
        private const string OrphanedMember = "Orphaned";

        private const string PublishedConstants = "Base.ToolPackage.Editor.Tests.Fixtures."
            + "FixturePublishedConstants";

        private const string PublishedConstName = "PublishedLabel";
        private const string SharedConstName = "SharedLabel";
        private const string UnreadConstName = "UnreadLabel";
        private const string Vector = "Base.ToolPackage.Editor.Tests.Fixtures.FixtureVector";

        private GraphProbe _probe;

        /// <summary>Scans the project once for the whole suite.</summary>
        [OneTimeSetUp]
        public void Scan()
        {
            CodebaseGraphData graph = CodebaseGraphBuilder.Build(null, true);

            Assert.That(graph, Is.Not.Null, "the scan returned nothing");
            _probe = new GraphProbe(graph);

            Assert.That(_probe.FindType(Behaviour),
                Is.Not.Null,
                "the fixture assembly was not scanned, so nothing below means anything");
        }

        /// <summary>A method the engine reaches only through the string handed to Invoke.</summary>
        [Test]
        public void InvokeByNameKeepsItsTargetAlive() => AssertAlive(Behaviour, "InvokedByName");

        /// <summary>A method reached only by subscribing to a field like event.</summary>
        [Test]
        public void EventSubscriptionKeepsItsHandlerAlive() => AssertAlive(Behaviour, "OnChanged");

        /// <summary>An iterator, whose body the compiler moves into a hidden state machine.</summary>
        [Test]
        public void IteratorIsAlive() => AssertAlive(Behaviour, "Countdown");

        /// <summary>A method called only from inside a local function.</summary>
        [Test]
        public void LocalFunctionCallKeepsItsTargetAlive() => AssertAlive(Behaviour, "CalledFromLocalFunction");

        /// <summary>A method called only from inside a lambda.</summary>
        [Test]
        public void LambdaCallKeepsItsTargetAlive() => AssertAlive(Behaviour, "CalledFromLambda");

        /// <summary>
        /// The same, from a lambda inside a property getter. The machinery names an accessor as its
        /// owner, and only the property is registered, so the lookup has to strip the prefix or every
        /// call the lambda makes is dropped without a word.
        /// </summary>
        [Test]
        public void AccessorLambdaCallKeepsItsTargetAlive() => AssertAlive(Behaviour, "CalledFromAccessorLambda");

        /// <summary>An implicit interface implementation, called through the interface.</summary>
        [Test]
        public void ImplicitImplementationIsAlive() => AssertAlive(Behaviour, "Implicit");

        /// <summary>An explicit implementation, whose metadata name carries the interface in front.</summary>
        [Test]
        public void ExplicitImplementationIsAlive() => AssertAlive(Behaviour, "Explicit");

        /// <summary>An auto property Unity writes through its generated backing field.</summary>
        [Test]
        public void FieldSerializeFieldPropertyIsAlive() => AssertAlive(Behaviour, "Prefab");

        /// <summary>A Unity message, called by the engine and by nothing in the code.</summary>
        [Test]
        public void UnityMessageIsAlive() => AssertAlive(Behaviour, "Awake");

        /// <summary>An indexer, which metadata calls Item.</summary>
        [Test]
        public void IndexerIsAlive() => AssertAlive(Vector, "Item");

        /// <summary>An operator, which metadata calls op_Equality.</summary>
        [Test]
        public void OperatorIsAlive() => AssertAlive(Vector, "op_Equality");

        /// <summary>A const read from a different file, whose value the compiler inlined.</summary>
        [Test]
        public void ConstReadFromAnotherFileIsAlive() => AssertAlive(Constants, SharedConstName);

        /// <summary>A default interface method carries its own body, so nothing is missing.</summary>
        [Test]
        public void DefaultInterfaceMethodIsNotReportedUnimplemented()
        {
            string reported = _probe.Describe(Contract, DescribeMember);

            Assert.That(_probe.HasIssue(Contract, DescribeMember, EMemberIssue.UnimplementedInterfaceMember),
                Is.False,
                $"Describe carries a body, so nothing is waiting to be written: {reported}");
        }

        /// <summary>An interface member nobody implements is the loud version of the same finding.</summary>
        [Test]
        public void UnimplementedInterfaceMemberIsReported()
        {
            string reported = _probe.Describe(Orphan, OrphanedMember);

            Assert.That(_probe.HasIssue(Orphan, OrphanedMember, EMemberIssue.UnimplementedInterfaceMember),
                Is.True,
                $"Orphaned is implemented by nobody: {reported}");
        }

        /// <summary>
        /// A type holding only nested types is referenced by nothing, because every use names the nested
        /// type instead, and a const inside leaves no trace at all. It is alive while they are.
        /// </summary>
        [Test]
        public void TypeHoldingLiveNestedTypesIsNotReportedDead()
        {
            Assert.That(_probe.FindType(NestingHost),
                Is.Not.Null,
                "the nesting fixture was never collected");

            Assert.That(_probe.HasTypeIssue(NestingHost, ETypeIssue.DeadType),
                Is.False,
                "the host holds a nested type that is read, so it is reachable");
        }

        /// <summary>A nested type nothing reads is dead, whatever its outer type is doing.</summary>
        [Test]
        public void UnusedNestedTypeIsReported() => Assert.That(_probe.HasTypeIssue(NestingUnused, ETypeIssue.DeadType),
            Is.True,
            $"nothing reads it: {_probe.DescribeType(NestingUnused)}");

        /// <summary>A tool that reports nothing passes every test above, so this one has to fail it.</summary>
        [Test]
        public void UnusedMethodIsReported() => Assert.That(
            _probe.HasIssue(DeadCode, "NeverCalled", EMemberIssue.DeadMember),
            Is.True,
            $"NeverCalled is called by nothing: {_probe.DescribeType(DeadCode)}");

        /// <summary>A field nothing touches at all.</summary>
        [Test]
        public void UntouchedFieldIsReported() => Assert.That(
            _probe.HasIssue(DeadCode, "_untouched", EMemberIssue.DeadMember),
            Is.True,
            $"_untouched is neither read nor written: {_probe.DescribeType(DeadCode)}");

        /// <summary>A field written and never read.</summary>
        [Test]
        public void WriteOnlyFieldIsReported() => Assert.That(
            _probe.HasIssue(DeadCode, "_writeOnly", EMemberIssue.WriteOnlyField),
            Is.True,
            $"_writeOnly is assigned and never read: {_probe.DescribeType(DeadCode)}");

        /// <summary>A serialized field Unity writes and no code reads.</summary>
        [Test]
        public void SerializedFieldNeverReadIsReported() => Assert.That(
            _probe.HasIssue(Behaviour, "_neverRead", EMemberIssue.SerializedNeverRead),
            Is.True,
            $"_neverRead is serialized and never read: {_probe.DescribeType(Behaviour)}");

        /// <summary>A const nothing reads, which the text pass has to fail to find.</summary>
        [Test]
        public void UnreadConstIsReported() => Assert.That(
            _probe.HasIssue(Constants, UnreadConstName, EMemberIssue.DeadMember),
            Is.True,
            $"the unread const is read nowhere: {_probe.DescribeType(Constants)}");

        /// <summary>
        /// The same shape published rather than internal. In a package that is API a consumer may be
        /// calling, so it is reported as unused API and never as dead. Without this the rule lives only
        /// in one line of PackageApi and nothing would notice if it changed.
        /// </summary>
        [Test]
        public void UnreadPublishedConstIsReportedAsApi()
        {
            Assert.That(_probe.HasIssue(PublishedConstants, PublishedConstName, EMemberIssue.UnusedPublicApi),
                Is.True,
                $"a published const nothing reads is unused API: {_probe.DescribeType(PublishedConstants)}");

            Assert.That(_probe.HasIssue(PublishedConstants, PublishedConstName, EMemberIssue.DeadMember),
                Is.False,
                "published API is never reported dead, because its callers are outside the scan");
        }

        private void AssertAlive(string typeName, string memberName)
        {
            Assert.That(_probe.FindMember(typeName, memberName),
                Is.Not.Null,
                $"{memberName} was never collected from {typeName}");

            Assert.That(_probe.HasIssue(typeName, memberName, EMemberIssue.DeadMember),
                Is.False,
                $"{memberName} is reachable but was reported dead: {_probe.Describe(typeName, memberName)}");
        }
    }
}