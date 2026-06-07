#if UNITY_EDITOR
namespace Base.Localization
{
    /// <summary>
    /// The direction to sync String Table Collections with Google Sheets.
    /// </summary>
    public enum ESyncDirection : byte
    {
        Pull = 0,
        Push = 1
    }
}
#endif