using System;
using UnityEngine;

namespace Base.AttributePackage.Editor.Windows.AttributeSamples.Samples
{
    /// <summary>Foldouts, tabs and side-by-side rows.</summary>
    [AttributeSample("Layout")]
    internal sealed class GroupingSample : ScriptableObject
    {
        [InfoBox("Consecutive fields sharing a name form a group. The run ends where the name changes.")]
        [Foldout("Bounds")] public float width = 1f;

        [Foldout("Bounds")] public float height = 2f;

        [Horizontal("size")] public int columns = 4;

        [Horizontal("size")] public int rows = 3;

        [Horizontal("weighted", Weight = 3f)] public string label = "Takes most of the row";

        [Horizontal("weighted", Weight = 1f)] public int count = 1;

        [Tab("General", "Settings")] public string profile = "Default";

        [Tab("General", "Settings")] public bool enabled = true;

        [Tab("Advanced", "Settings")] public float threshold = 0.5f;
    }
}