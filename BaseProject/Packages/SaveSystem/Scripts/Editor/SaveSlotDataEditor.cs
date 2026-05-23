using UnityEditor;
using Base.SaveSystemPackage.SaveSlot;
using Base.UtilityPackage.Identification.Editor;

namespace Base.SaveSystemPackage.Editor
{
    /// <summary>
    /// Custom editor for <see cref="SaveSlotData"/> that displays the unique ID in a read-only manner.
    /// </summary>
    [CustomEditor(typeof(SaveSlotData))]
    public class SaveSlotDataEditor : UniqueIdEditor<SaveSlotData> { }
}