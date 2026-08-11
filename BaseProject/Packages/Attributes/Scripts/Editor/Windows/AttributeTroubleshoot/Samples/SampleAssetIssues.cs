using UnityEngine;

namespace Base.AttributePackage.Editor.Windows.AttributeTroubleshoot.Samples
{
    /// <summary>
    /// Auto-assign attributes on an asset, so the samples tab can show the case where the attribute is
    /// correct in isolation but the type it sits on has no hierarchy to search.
    /// </summary>
    [TroubleshootSample]
    public sealed class SampleAssetIssues : ScriptableObject
    {
        /// <summary>An asset has no GameObject, so there is nothing to call GetComponent on.</summary>
        [GetComponent] public Rigidbody body;

        /// <summary>An asset has no children either.</summary>
        [Child] public Transform anchor;
    }
}