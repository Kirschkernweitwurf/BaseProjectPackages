using UnityEditor;
using UnityEngine;

namespace Base.AttributePackage.Editor
{
    /// <summary>Draws a folder path field with a browse button for <see cref="FolderPathAttribute"/>.</summary>
    [CustomPropertyDrawer(typeof(FolderPathAttribute))]
    public sealed class FolderPathDrawer : PathPickerDrawer
    {
        private const string PanelTitle = "Select Folder";

        protected override bool IsAbsolute => ((FolderPathAttribute)attribute).Absolute;

        protected override string UsageMessage => AttributeNames.Usage<FolderPathAttribute>("a string");

        protected override string OpenPanel()
            => EditorUtility.OpenFolderPanel(PanelTitle, Application.dataPath, string.Empty);
    }
}