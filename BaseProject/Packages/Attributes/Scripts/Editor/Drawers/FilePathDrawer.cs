using UnityEditor;
using UnityEngine;

namespace Base.AttributePackage.Editor
{
    /// <summary>Draws a file path field with a browse button for <see cref="FilePathAttribute"/>.</summary>
    [CustomPropertyDrawer(typeof(FilePathAttribute))]
    public sealed class FilePathDrawer : PathPickerDrawer
    {
        private const string PanelTitle = "Select File";

        protected override bool IsAbsolute => ((FilePathAttribute)attribute).Absolute;

        protected override string UsageMessage => AttributeNames.Usage<FilePathAttribute>("a string");

        protected override string OpenPanel()
            => EditorUtility.OpenFilePanel(PanelTitle, Application.dataPath, ((FilePathAttribute)attribute).Extension);
    }
}