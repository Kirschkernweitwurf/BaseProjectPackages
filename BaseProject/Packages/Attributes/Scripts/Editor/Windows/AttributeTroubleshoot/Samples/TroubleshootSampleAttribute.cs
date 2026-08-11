using System;

namespace Base.AttributePackage.Editor.Windows.AttributeTroubleshoot.Samples
{
    /// <summary>
    /// Marks a type as a deliberately broken example for the troubleshoot window. The project scan skips
    /// these types so they never appear as real findings, and the samples tab scans nothing else.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    internal sealed class TroubleshootSampleAttribute : Attribute { }
}