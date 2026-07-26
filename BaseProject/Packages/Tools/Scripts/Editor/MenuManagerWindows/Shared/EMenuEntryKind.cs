#if UNITY_EDITOR
namespace Base.ToolPackage.Editor.MenuManagerWindows
{
    /// <summary>Kind of a managed menu entry.</summary>
    public enum EMenuEntryKind
    {
        /// <summary>A method invoked from an editor menu.</summary>
        MenuItem,

        /// <summary>A ScriptableObject asset created from the Assets/Create menu.</summary>
        CreateAsset
    }
}
#endif