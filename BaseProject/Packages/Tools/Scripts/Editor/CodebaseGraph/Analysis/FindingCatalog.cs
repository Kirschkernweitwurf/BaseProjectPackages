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

        /// <summary>
        /// True when one finding on this namespace is hidden, either because the namespace was set
        /// aside wholesale or because this particular finding was.
        /// </summary>
        /// <param name="finding">Finding to test.</param>
        /// <param name="group">Namespace to test.</param>
        /// <returns>True when that finding is hidden.</returns>
        public static bool IsHidden(EFinding finding, NamespaceNodeInfo group)
            => IsHidden(group)
                || DismissalStore.Contains(GraphIdentity.ForFinding(group.DismissalId, finding));

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

        /// <summary>True when one finding on this type is hidden.</summary>
        /// <param name="finding">Finding to test.</param>
        /// <param name="type">Type to test.</param>
        /// <returns>True when that finding is hidden.</returns>
        public static bool IsHidden(EFinding finding, TypeNodeInfo type)
            => IsHidden(type)
                || DismissalStore.Contains(GraphIdentity.ForFinding(type.DismissalId, finding));

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

        /// <summary>True when one finding on this member is hidden.</summary>
        /// <param name="finding">Finding to test.</param>
        /// <param name="declaring">Type the member is declared on.</param>
        /// <param name="member">Member to test.</param>
        /// <returns>True when that finding is hidden.</returns>
        public static bool IsHidden(EFinding finding, TypeNodeInfo declaring, MemberNodeInfo member)
            => IsHidden(declaring, member)
                || DismissalStore.Contains(GraphIdentity.ForFinding(member.DismissalId, finding));

        /// <summary>True when anything reported on this member is still showing.</summary>
        /// <param name="declaring">Type the member is declared on.</param>
        /// <param name="member">Member to test.</param>
        /// <returns>True when at least one finding survives triage.</returns>
        public static bool HasVisibleFindings(TypeNodeInfo declaring, MemberNodeInfo member)
        {
            if (!member.HasIssues || IsHidden(declaring, member))
                return false;

            foreach (EFinding finding in Order)
            {
                if (MemberFlags.TryGetValue(finding, out EMemberIssue flag)
                    && member.Issues.HasFlag(flag)
                    && !IsHidden(finding, declaring, member))
                    return true;
            }

            return false;
        }

        /// <summary>True when anything reported on this type itself is still showing.</summary>
        /// <param name="type">Type to test.</param>
        /// <returns>True when at least one finding survives triage.</returns>
        public static bool HasVisibleFindings(TypeNodeInfo type)
        {
            if (!type.HasIssues || IsHidden(type))
                return false;

            foreach (EFinding finding in Order)
            {
                if (TypeFlags.TryGetValue(finding, out ETypeIssue flag)
                    && type.Issues.HasFlag(flag)
                    && !IsHidden(finding, type))
                    return true;
            }

            return false;
        }

        /// <summary>Counts the findings on a type's members that are still showing.</summary>
        /// <param name="type">Type to count inside.</param>
        /// <returns>The number of members with a visible finding.</returns>
        public static int CountVisibleMemberFindings(TypeNodeInfo type)
        {
            int count = 0;

            foreach (MemberNodeInfo member in type.Members)
            {
                if (HasVisibleFindings(type, member))
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
                if (member.HasIssues && !HasVisibleFindings(type, member))
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
                if (type.HasIssues && !HasVisibleFindings(type))
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
                if (HasVisibleFindings(type))
                    count++;

                count += CountVisibleMemberFindings(type);
            }

            return count;
        }

        /// <summary>Lists what was reported on a type, ignoring whether any of it was dismissed.</summary>
        /// <param name="type">Type to inspect.</param>
        /// <returns>The findings raised on it.</returns>
        public static IEnumerable<EFinding> ReadReported(TypeNodeInfo type)
        {
            foreach (EFinding finding in Order)
            {
                if (TypeFlags.TryGetValue(finding, out ETypeIssue flag) && type.Issues.HasFlag(flag))
                    yield return finding;
            }
        }

        /// <summary>Lists what was reported on a member, ignoring whether any of it was dismissed.</summary>
        /// <param name="member">Member to inspect.</param>
        /// <returns>The findings raised on it.</returns>
        public static IEnumerable<EFinding> ReadReported(MemberNodeInfo member)
        {
            foreach (EFinding finding in Order)
            {
                if (MemberFlags.TryGetValue(finding, out EMemberIssue flag) && member.Issues.HasFlag(flag))
                    yield return finding;
            }
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
                if (!MemberFlags.TryGetValue(finding, out EMemberIssue flag) || !member.Issues.HasFlag(flag))
                    continue;

                if (declaring == null || !IsHidden(finding, declaring, member))
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
                if (TypeFlags.TryGetValue(finding, out ETypeIssue flag)
                    && type.Issues.HasFlag(flag)
                    && !IsHidden(finding, type))
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

            if (group.CyclePartners.Count > 0 && !IsHidden(EFinding.NamespaceCycle, group))
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
                return declaring == null
                    ? member.HasIssues
                    : HasVisibleFindings(declaring, member);

            return MemberFlags.TryGetValue(finding, out EMemberIssue flag)
                && member.Issues.HasFlag(flag)
                && (declaring == null || !IsHidden(finding, declaring, member));
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
                return HasVisibleFindings(type) || CountVisibleMemberFindings(type) > 0;
            }

            if (TypeFlags.TryGetValue(finding, out ETypeIssue typeFlag)
                && type.Issues.HasFlag(typeFlag)
                && !IsHidden(finding, type))
                return true;

            if (!MemberFlags.TryGetValue(finding, out EMemberIssue memberFlag))
                return false;

            foreach (MemberNodeInfo member in type.Members)
            {
                if (member.Issues.HasFlag(memberFlag) && !IsHidden(finding, type, member))
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
                return group.CyclePartners.Count > 0 && !IsHidden(finding, group);

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
                    "Nothing calls this or reads it. It is not a Unity message and nothing marks it as "
                    + "something the engine calls.",
                    "Delete it. First check the four things no scan can see: reflection, Invoke or "
                    + "SendMessage by name, a UnityEvent set in the inspector and an animation event.",
                    false),

                [EFinding.DeadType] = new FindingDescriptor("Unreferenced types",
                    "Nothing references this type",
                    "No other type mentions this one, and it is not a Unity object, so it cannot be "
                    + "sitting on a prefab or in a scene either.",
                    "Delete it, unless something reaches it by reflection or from code outside this "
                    + "project.",
                    false),

                [EFinding.SerializedNeverRead] = new FindingDescriptor("Serialized, never read",
                    "Serialized but never read",
                    "You can set this in the inspector, but no code ever reads it. Whatever you type in "
                    + "is saved and then ignored.",
                    "The line says how many prefabs and scenes set it. Several means an unfinished "
                    + "feature, so write the code. None means nobody ever used it, so delete the field.",
                    false),

                [EFinding.WriteOnlyField] = new FindingDescriptor("Written, never read",
                    "Written but never read",
                    "Code writes to this field and nothing ever reads it, so every value is thrown away.",
                    "Delete the field and the lines that write it, or add the read that was meant to be "
                    + "there.",
                    false),

                [EFinding.PrivateCandidate] = new FindingDescriptor("Could be private",
                    "Only its own type uses this",
                    "Only this type uses it. Nothing else in the project touches it.",
                    "Make it private. The compiler will then tell you the moment something outside starts "
                    + "using it.",
                    true),

                [EFinding.PublicButInternalOnly] = new FindingDescriptor("Could be internal",
                    "Public, used only inside its assembly",
                    "Only code in the same assembly uses it, so public promises more than anyone needs.",
                    "Make it internal, unless you meant it as API for other projects to call.",
                    true),

                [EFinding.ReadOnlyCandidate] = new FindingDescriptor("Could be readonly",
                    "Only written in the constructor",
                    "Only the constructor sets this, so its value never changes afterwards. It is used, "
                    + "nothing is wrong with it.",
                    "Add readonly, so the compiler keeps it that way.",
                    true),

                [EFinding.StaticMutableState] = new FindingDescriptor("Mutable static state",
                    "Mutable static state",
                    "A static field that is not readonly keeps its value between scenes and between play "
                    + "sessions when domain reload is off. That is how one run leaks into the next.",
                    "Add readonly, move it onto an instance or clear it on play from a "
                    + "RuntimeInitializeOnLoadMethod with SubsystemRegistration.",
                    false),

                [EFinding.GodClass] = new FindingDescriptor("Very large types",
                    "Very large type",
                    "This type reaches into a lot of other namespaces, depends on a lot of other types "
                    + "or has a great many members. That usually means it does more than one job. Enums "
                    + "and lookup tables are never reported.",
                    "Find members that share the same data and move them into a type of their own.",
                    false),

                [EFinding.HighInstability] = new FindingDescriptor("Hard to change safely",
                    "Load bearing and concrete",
                    "A lot of code depends on this, it depends on almost nothing and there is no "
                    + "interface in front of it. So every change here reaches everything that uses it. "
                    + "The numbers are under Shape numbers below.",
                    "Put an interface in front of the part that changes and let callers use that. Leaving "
                    + "it alone is fine if the code is settled.",
                    false),

                [EFinding.TypeCycle] = new FindingDescriptor("Type cycles",
                    "Type cycle",
                    "These types depend on each other in a circle, so none of them can be moved or tested "
                    + "without the rest. The line shows the loop and which arrow is cheapest to cut.",
                    "Cut one arrow. Move the shared part into a third type, or put an interface between "
                    + "them.",
                    false),

                [EFinding.NamespaceCycle] = new FindingDescriptor("Namespace cycles",
                    "Namespace cycle",
                    "These namespaces use each other in a circle. While that is true they cannot be split "
                    + "into separate assemblies. The line shows the loop and the cheapest arrow to cut.",
                    "Cut one arrow. Move the shared types into a namespace both sides can use, or turn "
                    + "one direction around with an interface.",
                    false),

                [EFinding.UnusedPublicApi] = new FindingDescriptor("Unused public API",
                    "Public API nothing here calls",
                    "Nothing here uses it, but it is public in a package, so its callers live in other "
                    + "projects this scan cannot see. That is what a library is for.",
                    "Usually nothing. Only act on it if you are deliberately shrinking the package, and "
                    + "then check the projects you know about first.",
                    false),

                [EFinding.UnusedInterfaceMember] = new FindingDescriptor("Unused interface members",
                    "Declared on an interface, never called",
                    "Types implement this, but nothing ever calls it through the interface. Removing it "
                    + "changes the contract, not just some code.",
                    "Decide whether the interface still needs it. If it does, leave it alone.",
                    false),

                [EFinding.UnimplementedInterfaceMember] = new FindingDescriptor(
                    "Unimplemented interface members",
                    "Declared on an interface, implemented by nobody",
                    "The interface declares it, nothing implements it and nothing calls it. It only "
                    + "exists on paper.",
                    "Remove it from the interface, or write the implementation it is waiting for.",
                    false)
            };

            return descriptors;
        }
    }
}
