using Base.ToolPackage.Editor.CodebaseGraph.Model;

namespace Base.ToolPackage.Editor.CodebaseGraph.Analysis
{
    /// <summary>
    /// Turns raw coupling numbers into findings. Both rules here are deliberately narrow: a size rule
    /// that counts declarations flags every enum and every constants holder, and a coupling rule with
    /// no sense of abstraction flags every stable utility in the project.
    /// </summary>
    internal static class CouplingMetrics
    {
        private const int DataHolderMembers = 8;
        private const float DataHolderShare = 0.8f;
        private const int GodClassFanOut = 25;

        /// <summary>
        /// Member count at which a type is called large. Deliberately blunt and deliberately high: a
        /// hundred members is a number anyone can read off the node and argue with, where a compiled
        /// byte count is not. Compiled size was tried and dropped, because a real scan showed no break
        /// anywhere in the distribution, and a threshold with no break behind it only ever names
        /// whichever file happens to be biggest that week.
        /// </summary>
        private const int GodClassMembers = 100;

        private const int GodClassMinimumMembers = 20;
        private const int GodClassNamespaceReach = 12;
        private const float PainMaximumAbstractness = 0.2f;
        private const float PainMaximumInstability = 0.3f;
        private const int PainMinimumFanIn = 10;

        /// <summary>
        /// True when the type carries so much that it almost certainly does more than one job. Reach is
        /// the signal that carries this: depending on a dozen different namespaces is a claim about
        /// structure, where size is only a claim about verbosity. The member count is kept as a coarse
        /// second opinion and set high enough that it only agrees with the obvious cases.
        /// </summary>
        /// <param name="type">Type to test.</param>
        /// <returns>True when the type looks overloaded.</returns>
        internal static bool IsGodClass(TypeNodeInfo type)
        {
            if (IsExempt(type))
                return false;

            // A lookup table naming thirty types in typeof reaches far and does one job. Reach alone
            // cannot tell them apart, but size can: nothing this small holds two responsibilities.
            // Calibrated against one case, FieldTypeCheck: ninety seven lines, a thirty entry rule
            // table and twelve one line predicates, counting fourteen members. Twenty clears it with
            // room. Move the number if a real god class ever lands under it, not to settle an argument
            // about a borderline type.
            if (type.Members.Count < GodClassMinimumMembers)
                return false;

            return type.NamespaceReach > GodClassNamespaceReach
                || type.FanOut > GodClassFanOut
                || type.Members.Count > GodClassMembers;
        }

        /// <summary>
        /// True when a type is load bearing and concrete at the same time. Plenty of code depends on it,
        /// it depends on little, and almost nothing about it is abstract, so there is no seam to change
        /// behind and every edit reaches everything that uses it.
        /// <br/><br/>
        /// The other corner, an abstraction nothing uses, is deliberately not reported here. The unused
        /// API and unused interface member findings say that more precisely already.
        /// </summary>
        /// <param name="type">Type to test.</param>
        /// <returns>True when the type is hard to change safely.</returns>
        internal static bool IsHardToChange(TypeNodeInfo type)
        {
            if (IsExempt(type) || IsMeantToBeDependedOn(type))
                return false;

            return type.FanIn >= PainMinimumFanIn
                && type.Abstractness <= PainMaximumAbstractness
                && type.Instability <= PainMaximumInstability;
        }

        /// <summary>
        /// True when a type is not really doing anything, whatever its size. Enums and static holders
        /// are obvious, but a lookup table declared as an ordinary class is the same thing wearing a
        /// different hat, and is recognized by its members being almost all consts and static readonly.
        /// </summary>
        /// <summary>
        /// True when being depended on by everything is the whole point of the type. An attribute is
        /// applied to other code by definition, and a key or a record carries data and no behaviour.
        /// Telling either to put an interface in front of itself is advice nobody can act on.
        /// <br/><br/>
        /// Implementing an interface is deliberately not enough. An empty marker says nothing about
        /// whether callers actually go through it, and a concrete workhorse that happens to carry one
        /// is exactly what this finding is for.
        /// </summary>
        private static bool IsMeantToBeDependedOn(TypeNodeInfo type)
            => type.IsAttribute || type.BehaviourMemberCount == 0;

        private static bool IsExempt(TypeNodeInfo type)
        {
            if (type.Kind == ETypeKind.Enum || type.IsStatic)
                return true;

            return type.Members.Count >= DataHolderMembers && type.DataMemberShare >= DataHolderShare;
        }
    }
}