using Base.UtilityPackage.Generated;
using UnityEngine;

namespace Base.CorePackage.MenuManaging.Identifier
{
    /// <summary>
    /// A ScriptableObject used to uniquely identify different types of menus in the game.
    /// </summary>
    [CreateAssetMenu(fileName = "MenuIdentifier", menuName = "Scriptable Objects/Base/Menus/Menu Identifier",
        order = MenuOrders.Asset)]
    public class MenuIdentifier : ScriptableObject { }
}