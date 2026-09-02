using UnityEngine;

namespace Base.AttributesPackage.Editor.Windows.AttributeExplorer.Troubleshoot.Samples
{
    /// <summary>
    /// Auto-assign attributes that can never fill their field, so the samples tab can show the case where
    /// a reference stays empty and looks like a forgotten drag instead of a broken attribute.
    /// </summary>
    [TroubleshootSample]
    internal sealed class SampleAutoAssignIssues
    {
        /// <summary>GetComponent returns components, never the GameObject itself.</summary>
        [GetComponent] public GameObject wholeObject;

        /// <summary>A string is not something the hierarchy can be searched for.</summary>
        [Child] public string labelText;

        /// <summary>A ScriptableObject lives in the project, not on a GameObject.</summary>
        [GetComponentInParent] public ScriptableObject notAComponent;
    }
}