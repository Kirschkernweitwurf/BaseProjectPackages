using System;
using Base.AttributesPackage.Editor.Core;

namespace Base.AttributesPackage.Editor.Windows.AttributeExplorer.Reference
{
    /// <summary>
    /// Forgets the first-draw state of a sample type, so the defaults its attributes declare are applied
    /// again every time the sample is opened.
    /// </summary>
    /// <remarks>
    /// A sample page is documentation, not a document being worked on. Unity stores the expanded flag
    /// per type, so folding a field away once left every later visit showing something the sample never
    /// meant to demonstrate, which reads as the attribute being broken rather than as a leftover.
    /// </remarks>
    internal static class SamplePreviewDefaults
    {
        /// <summary>Marks every property of the sample type as unseen again.</summary>
        /// <param name="sampleType">The sample type about to be shown.</param>
        internal static void Reapply(Type sampleType) => FirstDraw.Forget(sampleType);
    }
}