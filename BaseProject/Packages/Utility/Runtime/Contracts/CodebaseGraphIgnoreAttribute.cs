using System;

namespace Base.UtilityPackage.Contracts
{
    /// <summary>
    /// Marks a type or member the Codebase Graph must never report on. Use it where a finding is wrong
    /// for a reason the scan cannot see and never will: a member reached only through reflection, a
    /// serialized field written by a build hook, a fixture that exists to hold broken code on purpose.
    /// <para>
    /// The tool matches this by name rather than by type, so it lives here rather than in the tool and
    /// nothing has to reference the tool to use it. It also survives renaming the member, which the
    /// older same line comment marker does not.
    /// </para>
    /// <para>
    /// It silences every finding on what it marks, forever. Where the member really is used and the scan
    /// simply cannot see the caller, <c>[UsedImplicitly]</c> says that more precisely and leaves the
    /// findings that are about design rather than about use still reporting.
    /// </para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Class
        | AttributeTargets.Struct
        | AttributeTargets.Interface
        | AttributeTargets.Enum
        | AttributeTargets.Method
        | AttributeTargets.Property
        | AttributeTargets.Field
        | AttributeTargets.Event)]
    public sealed class CodebaseGraphIgnoreAttribute : Attribute
    {
    }
}