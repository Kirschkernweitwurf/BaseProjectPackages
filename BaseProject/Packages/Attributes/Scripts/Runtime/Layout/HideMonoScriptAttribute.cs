using System;

namespace Base.AttributePackage
{
    /// <summary>
    /// Hides the read-only Script row at the top of the inspector for the decorated type.
    /// </summary>
    /// <remarks>
    /// That row exists so a broken script reference can be repaired, which is worth exactly one click
    /// every few months and costs a line of vertical space on every selection in between.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class HideMonoScriptAttribute : Attribute { }
}