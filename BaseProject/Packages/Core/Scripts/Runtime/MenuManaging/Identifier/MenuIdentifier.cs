using Base.ToolPackage.MenuManagerWindow;
using UnityEngine;

namespace Base.CorePackage.MenuManaging.Identifier
{
    /// <summary>
    /// A ScriptableObject used to uniquely identify different types of menus in the game.
    /// </summary>
    [DynamicCreateAssetMenu("Scriptable Objects/Base/Menus/New Menu Identifier", "MID_MenuIdentifier")]
    public class MenuIdentifier : ScriptableObject { }
}