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

        /// <summary>True when this namespace was set aside during triage.</summary>
        /// <param name="group">Namespace to test.</param>
        /// <returns>True when its findings are hidden.</returns>
        public static bool IsHidden(NamespaceNodeInfo group)
            => !DismissalStore.IsEmpty && DismissalStore.Contains(group.DismissalId);

        /// <summary>True when this type, or the namespace holding it, was set aside during triage.</summary>
        /// <param name="type">Type to test.</param>
        /// <returns>True when its findings are hidden.</returns>
        public static bool IsHidden(TypeNodeInfo type)
        {
            if (DismissalStore.IsEmpty)
                return false;

            return DismissalStore.Contains(type.DismissalId)
                || DismissalStore.ContainsTree(GraphIdentity.ForNamespace(type.Namespace));
        }

        /// <summary>True when this member, or anything containing it, was set aside during triage.</summary>
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
                    "Shows every entry that carries at least one finding.",
                    string.Empty,
                    false),

                [EFinding.DeadMember] = new FindingDescriptor("Never used",
                    "Never used",
                    "No compiled code reads, calls or references this member. It is not a Unity message, "
                    + "not an override, and carries no attribute that would make the engine call it.",
                    "Delete it. Before you do, check whether it is reached through reflection, SendMessage, "
                    + "a UnityEvent wired in the inspector, or an animation event. None of those are visible "
                    + "to a code scan.",
                    false),

                [EFinding.DeadType] = new FindingDescriptor("Unreferenced types",
                    "Nothing references this type",
                    "No other scanned type mentions this one, and it does not derive from a Unity object, "
                    + "so it cannot have been placed in a scene or asset either.",
                    "Delete it, or check whether it is only reached by reflection or from an assembly "
                    + "outside the scan.",
                    false),

                [EFinding.SerializedNeverRead] = new FindingDescriptor("Serialized, never read",
                    "Serialized but never read",
                    "Unity writes this field from the inspector, but no code ever reads the value back. "
                    + "Whatever you set in your prefabs and scenes is being stored and then ignored.",
                    "The report says how many prefabs, scenes and assets set it. Several means a feature "
                    + "that was started and not finished, which is worth writing rather than deleting. "
                    + "None means the field is doing nothing anywhere and can go.",
                    false),

                [EFinding.WriteOnlyField] = new FindingDescriptor("Written, never read",
                    "Written but never read",
                    "Code assigns this field, but nothing ever reads it back, so every value written to it "
                    + "is thrown away.",
                    "Delete the field together with its assignments, or add the read that was intended.",
                    false),

                [EFinding.PrivateCandidate] = new FindingDescriptor("Could be private",
                    "Only its own type uses this",
                    "Every caller of this member sits inside the type that declares it. Nothing else in "
                    + "the project reaches it, so the wider visibility is promising access that nobody "
                    + "takes up.",
                    "Make it private. That is a stronger guarantee than internal, and the compiler will "
                    + "then tell you the moment something outside the type starts depending on it.",
                    true),

                [EFinding.PublicButInternalOnly] = new FindingDescriptor("Could be internal",
                    "Public, used only inside its assembly",
                    "The member is public, but every caller lives in the same assembly. Nothing outside this "
                    + "package touches it, so the public keyword is promising more than it needs to.",
                    "Change it to internal, unless it is deliberately part of the API you want consumers of "
                    + "the package to call.",
                    true),

                [EFinding.ReadOnlyCandidate] = new FindingDescriptor("Could be readonly",
                    "Only written in the constructor",
                    "The field is assigned only inside a constructor of its own type, so its value never "
                    + "changes after the object is built. The field is used and is not redundant.",
                    "Mark it readonly, so the compiler guarantees it stays fixed.",
                    true),

                [EFinding.StaticMutableState] = new FindingDescriptor("Mutable static state",
                    "Mutable static state",
                    "A static field that is not readonly keeps its value across scene loads, and across play "
                    + "sessions when domain reload is disabled. That is a common source of state leaking from "
                    + "one run into the next.",
                    "Make it readonly, move it onto an instance, or clear it from a "
                    + "RuntimeInitializeOnLoadMethod hook using SubsystemRegistration.",
                    false),

                [EFinding.GodClass] = new FindingDescriptor("Very large types",
                    "Very large type",
                    "The type declares an unusually high number of members, or depends on an unusually high "
                    + "number of other types. That normally means it has taken on more than one job.",
                    "Look for a group of members that share the same state and move them into their own type.",
                    false),

                [EFinding.HighInstability] = new FindingDescriptor("Unstable dependencies",
                    "Unstable dependency",
                    "Several types depend on this one, while it in turn depends on many others. Every change "
                    + "to something it uses ripples straight out into everything that uses it.",
                    "Reduce what it depends on, or hide the volatile parts behind an interface that it owns.",
                    false),

                [EFinding.TypeCycle] = new FindingDescriptor("Type cycles",
                    "Type cycle",
                    "This type and the ones listed below all reach each other, directly or through a chain. "
                    + "None of them can be read, moved or tested without the others.",
                    "Break the loop by moving the shared part into a third type, or by putting an interface "
                    + "between them and letting the lower level own it.",
                    false),

                [EFinding.UnusedPublicApi] = new FindingDescriptor("Unused public API",
                    "Public API nothing here calls",
                    "Nothing inside this project uses it, but it is public on a distributable package, "
                    + "so its callers live in other projects that this scan cannot see. That is what a "
                    + "library API is for.",
                    "Usually nothing. Only worth acting on if you are deliberately shrinking the package "
                    + "surface, and then only after checking the consumers you know about.",
                    false),

                [EFinding.UnusedInterfaceMember] = new FindingDescriptor("Unused interface members",
                    "Declared on an interface, never called",
                    "The interface declares it and types implement it, but no code ever calls it through "
                    + "the interface. Deleting it would remove part of the contract, not just some code.",
                    "Check whether the contract still needs it. If it does, this is fine as it is.",
                    false),

                [EFinding.UnimplementedInterfaceMember] = new FindingDescriptor("Unimplemented interface members",
                    "Declared on an interface, implemented by nobody",
                    "The interface declares it, no scanned type implements it, and nothing calls it. That "
                    + "is a piece of contract that exists only on paper.",
                    "Remove it from the interface, or write the implementation it was waiting for.",
                    false),

                [EFinding.NamespaceCycle] = new FindingDescriptor("Namespace cycles",
                    "Namespace cycle",
                    "This namespace and the ones listed below reference each other in a loop. While the loop "
                    + "exists they cannot be split into separate assemblies.",
                    "Move the shared types into a namespace both sides can depend on, or invert one direction "
                    + "with an interface.",
                    false)
            };

            return descriptors;
        }
    }
}
