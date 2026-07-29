using Base.CorePackage.MenuManaging.Identifier;
using UnityEditor;

namespace Base.CorePackage.Editor.MenuManaging.Identifier
{
    /// <summary>
    /// Watches for created and moved <see cref="MenuIdentifier"/> assets
    /// and queues regeneration of the accessor class and registry.
    /// </summary>
    /// <remarks>
    /// Deletion is handled by <see cref="MenuIdentifierDeleteProcessor"/>. It cannot be handled here,
    /// because a deleted or moved-from path no longer resolves to a type.
    /// </remarks>
    internal sealed class MenuIdentifierPostprocessor : AssetPostprocessor
    {
#region Unity Callbacks
        private static void OnPostprocessAllAssets(string[] imported, string[] deleted,
            string[] movedTo, string[] movedFrom)
        {
            if (MenuIdentifierAssets.AnyIsMenuIdentifier(imported)
                || MenuIdentifierAssets.AnyIsMenuIdentifier(movedTo))
                MenuIdentifierRegenerationScheduler.Schedule();
        }
#endregion
    }
}