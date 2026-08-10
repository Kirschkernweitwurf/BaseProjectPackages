using System.Collections.Generic;
using Base.ToolPackage.Editor.CodebaseGraph.Model;

namespace Base.ToolPackage.Editor.CodebaseGraph.Analysis
{
    /// <summary>
    /// The single place that knows what each finding means. A red line on a node is useless on its own,
    /// so every finding carries an explanation of what the scan actually saw and what to do about it.
    /// </summary>
    public static class FindingCatalog
    {
        /// <summary>Order the findings appear in the dropdown.</summary>
        private static readonly EFinding[] Order =
        {
            EFinding.None,
            EFinding.Any,
            EFinding.DeadMember,
            EFinding.DeadType,
            EFinding.UnimplementedInterfaceMember,
            EFinding.SerializedNeverRead,
            EFinding.WriteOnlyField,
            EFinding.PrivateCandidate,
            EFinding.PublicButInternalOnly,
            EFinding.ReadOnlyCandidate,
            EFinding.StaticMutableState,
            EFinding.GodClass,
            EFinding.HighInstability,
            EFinding.TypeCycle,
            EFinding.NamespaceCycle,
            EFinding.UnusedPublicApi,
            EFinding.UnusedInterfaceMember
        };

        /// <summary>Member flag each finding maps to, for the ones that live on a member.</summary>
        private static readonly Dictionary<EFinding, EMemberIssue> MemberFlags = new()
        {
            [EFinding.DeadMember] = EMemberIssue.DeadMember,
            [EFinding.PrivateCandidate] = EMemberIssue.PrivateCandidate,
            [EFinding.PublicButInternalOnly] = EMemberIssue.PublicButInternalOnly,
            [EFinding.ReadOnlyCandidate] = EMemberIssue.ReadOnlyCandidate,
            [EFinding.SerializedNeverRead] = EMemberIssue.SerializedNeverRead,
            [EFinding.StaticMutableState] = EMemberIssue.StaticMutableState,
            [EFinding.UnimplementedInterfaceMember] = EMemberIssue.UnimplementedInterfaceMember,
            [EFinding.UnusedInterfaceMember] = EMemberIssue.UnusedInterfaceMember,
            [EFinding.UnusedPublicApi] = EMemberIssue.UnusedPublicApi,
            [EFinding.WriteOnlyField] = EMemberIssue.WriteOnlyField
        };

        /// <summary>Type flag each finding maps to, for the ones that live on a type.</summary>
        private static readonly Dictionary<EFinding, ETypeIssue> TypeFlags = new()
        {
            [EFinding.DeadType] = ETypeIssue.DeadType,
            [EFinding.GodClass] = ETypeIssue.GodClass,
            [EFinding.HighInstability] = ETypeIssue.HighInstability,
            [EFinding.TypeCycle] = ETypeIssue.TypeCycle,
            [EFinding.UnusedPublicApi] = ETypeIssue.UnusedPublicType
        };

        private static readonly Dictionary<EFinding, FindingDescriptor> Descriptors = BuildDescriptors();

        /// <summary>Builds the dropdown entries in display order.</summary>
        /// <returns>The labels to show.</returns>
        public static List<string> BuildChoices()
        {
            List<string> choices = new(Order.Length);

            foreach (EFinding finding in Order)
                choices.Add(Describe(finding).FilterLabel);

            return choices;
        }

        /// <summary>Returns the finding at a dropdown position.</summary>
        /// <param name="index">Index the dropdown reported.</param>
        /// <returns>The matching finding, or none when the index is out of range.</returns>
        public static EFinding GetAt(int index)
            => index < 0 || index >= Order.Length
                ? EFinding.None
                : Order[index];

        /// <summary>Returns the dropdown position of a finding.</summary>
        /// <param name="finding">Finding to locate.</param>
        /// <returns>The index, or zero when the finding is not listed.</returns>
        public static int GetIndex(EFinding finding)
        {
            for (int index = 0; index < Order.Length; index++)
            {
                if (Order[index] == finding)
                    return index;
            }

            return 0;
        }

        /// <summary>Returns everything the window can say about a finding.</summary>
        /// <param name="finding">Finding to describe.</param>
        /// <returns>The descriptor.</returns>
        public static FindingDescriptor Describe(EFinding finding)
            => Descriptors.TryGetValue(finding, out FindingDescriptor descriptor)
                ? descriptor
                : Descriptors[EFinding.None];

        /// <summary>True when this namespace was dismissed during triage.</summary>
        /// <param name="group">Namespace to test.</param>
        /// <returns>True when its findings are hidden.</returns>
        public static bool IsHidden(NamespaceNodeInfo group)
            => !DismissalStore.IsEmpty && DismissalStore.Contains(group.DismissalId);

        /// <summary>True when this type, or the namespace holding it, was dismissed during triage.</summary>
        /// <param name="type">Type to test.</param>
        /// <returns>True when its findings are hidden.</returns>
        public static bool IsHidden(TypeNodeInfo type)
        {
            if (DismissalStore.IsEmpty)
                return false;

            return DismissalStore.Contains(type.DismissalId)
                || DismissalStore.ContainsTree(GraphIdentity.ForNamespace(type.Namespace));
        }

        /// <summary>True when this member, or anything containing it, was dismissed during triage.</summary>
        /// <param name="declaring">Type the member is declared on.</param>
        /// <param name="member">Member to test.</param>
        /// <returns>True when its findings are hidden.</returns>
        public static bool IsHidden(TypeNodeInfo declaring, MemberNodeInfo member)
        {
            if (DismissalStore.IsEmpty)
                return false;

            return DismissalStore.Contains(member.DismissalId)
                || DismissalStore.ContainsTree(declaring.DismissalId)
                || DismissalStore.ContainsTree(GraphIdentity.ForNamespace(declaring.Namespace));
        }

        /// <summary>Counts the findings on a type's members that are still showing.</summary>
        /// <param name="type">Type to count inside.</param>
        /// <returns>The number of members with a visible finding.</returns>
        public static int CountVisibleMemberFindings(TypeNodeInfo type)
        {
            int count = 0;

            foreach (MemberNodeInfo member in type.Members)
            {
                if (member.HasIssues && !IsHidden(type, member))
                    count++;
            }

            return count;
        }

        /// <summary>Counts the members of a type whose findings were dismissed.</summary>
        /// <param name="type">Type to count inside.</param>
        /// <returns>The number of members with a silenced finding.</returns>
        public static int CountDismissedMemberFindings(TypeNodeInfo type)
        {
            if (DismissalStore.IsEmpty)
                return 0;

            int count = 0;

            foreach (MemberNodeInfo member in type.Members)
            {
                if (member.HasIssues && IsHidden(type, member))
                    count++;
            }

            return count;
        }

        /// <summary>Counts everything inside a namespace whose findings were dismissed.</summary>
        /// <param name="group">Namespace to count inside.</param>
        /// <returns>The number of types and members with a silenced finding.</returns>
        public static int CountDismissedFindings(NamespaceNodeInfo group)
        {
            if (DismissalStore.IsEmpty)
                return 0;

            int count = 0;

            foreach (TypeNodeInfo type in group.Types)
            {
                if (type.HasIssues && IsHidden(type))
                    count++;

                count += CountDismissedMemberFindings(type);
            }

            return count;
        }

        /// <summary>Counts every finding still showing inside a namespace.</summary>
        /// <param name="group">Namespace to count inside.</param>
        /// <returns>The number of types and members with a visible finding.</returns>
        public static int CountVisibleFindings(NamespaceNodeInfo group)
        {
            int count = 0;

            foreach (TypeNodeInfo type in group.Types)
            {
                if (type.HasIssues && !IsHidden(type))
                    count++;

                count += CountVisibleMemberFindings(type);
            }

            return count;
        }

        /// <summary>Collects every finding reported on a member.</summary>
        /// <param name="member">Member to inspect.</param>
        /// <param name="declaring">Type the member is declared on.</param>
        /// <param name="findings">List that receives the findings.</param>
        public static void Collect(MemberNodeInfo member, TypeNodeInfo declaring, List<EFinding> findings)
        {
            if (declaring != null && IsHidden(declaring, member))
                return;

            // Walking the ordered list rather than the lookup keeps badge order the same on every run.
            foreach (EFinding finding in Order)
            {
                if (MemberFlags.TryGetValue(finding, out EMemberIssue flag) && member.Issues.HasFlag(flag))
                    findings.Add(finding);
            }
        }

        /// <summary>Collects every finding reported on a type itself, ignoring its members.</summary>
        /// <param name="type">Type to inspect.</param>
        /// <param name="findings">List that receives the findings.</param>
        public static void Collect(TypeNodeInfo type, List<EFinding> findings)
        {
            if (IsHidden(type))
                return;

            foreach (EFinding finding in Order)
            {
                if (TypeFlags.TryGetValue(finding, out ETypeIssue flag) && type.Issues.HasFlag(flag))
                    findings.Add(finding);
            }
        }

        /// <summary>Collects every finding reported on a namespace itself.</summary>
        /// <param name="group">Namespace to inspect.</param>
        /// <param name="findings">List that receives the findings.</param>
        public static void Collect(NamespaceNodeInfo group, List<EFinding> findings)
        {
            if (IsHidden(group))
                return;

            if (group.CyclePartners.Count > 0)
                findings.Add(EFinding.NamespaceCycle);
        }

        /// <summary>Checks a member against the current finding filter.</summary>
        /// <param name="finding">Finding to filter by.</param>
        /// <param name="member">Member to test.</param>
        /// <returns>True when the member should be shown.</returns>
        public static bool IsMatch(EFinding finding, MemberNodeInfo member, TypeNodeInfo declaring)
        {
            if (finding == EFinding.None)
                return true;

            if (declaring != null && IsHidden(declaring, member))
                return false;

            if (finding == EFinding.Any)
                return member.HasIssues;

            return MemberFlags.TryGetValue(finding, out EMemberIssue flag) && member.Issues.HasFlag(flag);
        }

        /// <summary>Checks a type against the current finding filter, including its members.</summary>
        /// <param name="finding">Finding to filter by.</param>
        /// <param name="type">Type to test.</param>
        /// <returns>True when the type should be shown.</returns>
        public static bool IsMatch(EFinding finding, TypeNodeInfo type)
        {
            if (finding == EFinding.None)
                return true;

            if (finding == EFinding.Any)
            {
                return (type.HasIssues && !IsHidden(type)) || CountVisibleMemberFindings(type) > 0;
            }

            if (TypeFlags.TryGetValue(finding, out ETypeIssue typeFlag)
                && type.Issues.HasFlag(typeFlag)
                && !IsHidden(type))
                return true;

            if (!MemberFlags.TryGetValue(finding, out EMemberIssue memberFlag))
                return false;

            foreach (MemberNodeInfo member in type.Members)
            {
                if (member.Issues.HasFlag(memberFlag) && !IsHidden(type, member))
                    return true;
            }

            return false;
        }

        /// <summary>Checks a namespace against the current finding filter, including its types.</summary>
        /// <param name="finding">Finding to filter by.</param>
        /// <param name="group">Namespace to test.</param>
        /// <returns>True when the namespace should be shown.</returns>
        public static bool IsMatch(EFinding finding, NamespaceNodeInfo group)
        {
            if (finding == EFinding.None)
                return true;

            if (finding == EFinding.NamespaceCycle)
                return group.CyclePartners.Count > 0 && !IsHidden(group);

            foreach (TypeNodeInfo type in group.Types)
            {
                if (IsMatch(finding, type))
                    return true;
            }

            return false;
        }

        private static Dictionary<EFinding, FindingDescriptor> BuildDescriptors()
        {
            Dictionary<EFinding, FindingDescriptor> descriptors = new()
            {
                [EFinding.None] = new FindingDescriptor("Show everything",
                    "No finding",
                    "Nothing was reported here.",
                    string.Empty,
                    false),

                [EFinding.Any] = new FindingDescriptor("Anything worth a look",
                    "Any finding",
                    "Every entry carrying at least one finding.",
                    string.Empty,
                    false),

                [EFinding.DeadMember] = new FindingDescriptor("Never used members",
                    "Never used",
                    "Nothing in the compiled code reads this member, calls it or hands it around. It is "
                    + "not a Unity message, it overrides nothing, and it carries no attribute that would "
                    + "have the engine reach for it.",
                    "Delete it. Before you do, rule out the four things a code scan cannot see: "
                    + "reflection, SendMessage or Invoke by name, a UnityEvent wired in the inspector, "
                    + "and an animation event.",
                    false),

                [EFinding.DeadType] = new FindingDescriptor("Unreferenced types",
                    "Nothing references this type",
                    "No other scanned type names this one, and it does not derive from a Unity object, so "
                    + "it cannot have been dropped into a scene or an asset either.",
                    "Delete it, unless it is reached by reflection or from an assembly outside the scan.",
                    false),

                [EFinding.SerializedNeverRead] = new FindingDescriptor("Serialized, never read",
                    "Serialized but never read",
                    "Unity writes this field from the inspector and no code ever reads it back, so "
                    + "whatever you set is being stored and then ignored.",
                    "The line says how many prefabs, scenes and assets set it. Several means a feature "
                    + "that was started and left unfinished, which is worth writing rather than deleting. "
                    + "None means the field does nothing anywhere and can go.",
                    false),

                [EFinding.WriteOnlyField] = new FindingDescriptor("Written, never read",
                    "Written but never read",
                    "Code assigns this field and nothing ever reads it back, so every value written to it "
                    + "is thrown away.",
                    "Delete the field and its assignments, or add the read that was meant to be there.",
                    false),

                [EFinding.PrivateCandidate] = new FindingDescriptor("Could be private",
                    "Only its own type uses this",
                    "Every caller sits inside the type that declares it. Nothing else in the project "
                    + "reaches it, so the wider visibility offers access that nobody takes up.",
                    "Make it private. That is a stronger guarantee than internal, and the compiler will "
                    + "tell you the moment something outside the type starts depending on it.",
                    true),

                [EFinding.PublicButInternalOnly] = new FindingDescriptor("Could be internal",
                    "Public, used only inside its assembly",
                    "Every caller lives in the same assembly. Nothing outside this one touches it, so "
                    + "public is promising more than anything asks for.",
                    "Make it internal, unless it is deliberately part of an API you want other code to "
                    + "call.",
                    true),

                [EFinding.ReadOnlyCandidate] = new FindingDescriptor("Could be readonly",
                    "Only written in the constructor",
                    "The field is assigned only inside a constructor of its own type, so its value never "
                    + "changes once the object exists. It is used, and it is not redundant.",
                    "Mark it readonly, so the compiler holds it to that.",
                    true),

                [EFinding.StaticMutableState] = new FindingDescriptor("Mutable static state",
                    "Mutable static state",
                    "A static field that is not readonly keeps its value across scene loads, and across "
                    + "play sessions when domain reload is off. That is where state leaks from one run "
                    + "into the next.",
                    "Make it readonly, move it onto an instance, or clear it from a "
                    + "RuntimeInitializeOnLoadMethod hook using SubsystemRegistration.",
                    false),

                [EFinding.GodClass] = new FindingDescriptor("Very large types",
                    "Very large type",
                    "The type declares an unusual number of members, or depends on an unusual number of "
                    + "other types. That normally means it has picked up more than one job.",
                    "Find a group of members that share the same state and move them into a type of their "
                    + "own.",
                    false),

                [EFinding.HighInstability] = new FindingDescriptor("Unstable dependencies",
                    "Unstable dependency",
                    "Plenty of types depend on this one while it depends on plenty of others, so a change "
                    + "to anything it uses travels straight out to everything that uses it.",
                    "Cut down what it depends on, or put an interface it owns in front of the volatile "
                    + "parts.",
                    false),

                [EFinding.TypeCycle] = new FindingDescriptor("Type cycles",
                    "Type cycle",
                    "These types all reach each other, directly or along a chain. The entry writes the "
                    + "loop out edge by edge, because it usually closes through a step you would not "
                    + "guess. None of them can be read, moved or tested without the others.",
                    "Break one edge. Move the shared part into a third type, or put an interface between "
                    + "them and let the lower level own it.",
                    false),

                [EFinding.NamespaceCycle] = new FindingDescriptor("Namespace cycles",
                    "Namespace cycle",
                    "These namespaces reference each other in a loop, written out edge by edge. While it "
                    + "exists they cannot be split into separate assemblies.",
                    "Move the shared types into a namespace both sides can depend on, or invert one "
                    + "direction with an interface.",
                    false),

                [EFinding.UnusedPublicApi] = new FindingDescriptor("Unused public API",
                    "Public API nothing here calls",
                    "Nothing in this project uses it, but it is published from a distributable package, "
                    + "so its callers live in projects this scan cannot see. That is what a library API "
                    + "is for.",
                    "Usually nothing. Worth acting on only if you are deliberately shrinking the package "
                    + "surface, and then only after checking the consumers you know about.",
                    false),

                [EFinding.UnusedInterfaceMember] = new FindingDescriptor("Unused interface members",
                    "Declared on an interface, never called",
                    "The interface declares it and types implement it, but nothing ever calls it through "
                    + "the interface. Removing it would take away part of the contract, not just some "
                    + "code.",
                    "Decide whether the contract still needs it. If it does, leave it alone.",
                    false),

                [EFinding.UnimplementedInterfaceMember] = new FindingDescriptor(
                    "Unimplemented interface members",
                    "Declared on an interface, implemented by nobody",
                    "The interface declares it, no scanned type implements it, and nothing calls it. It "
                    + "is contract that exists only on paper.",
                    "Take it off the interface, or write the implementation it has been waiting for.",
                    false)
            };

            return descriptors;
        }
    }
}
