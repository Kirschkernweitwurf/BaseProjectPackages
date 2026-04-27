#if UNITY_EDITOR
using UnityEditor;

namespace Base.SystemsCorePackage.MenuManaging.Identifier.Editor
{
    internal class MenuIdentifierPostprocessor : AssetPostprocessor
    {
        private static void OnPostprocessAllAssets(string[] imported, string[] deleted,
            string[] movedTo, string[] movedFrom)
        {
            bool affected =
                AnyIsMenuIdentifier(imported) ||
                AnyIsMenuIdentifier(deleted) ||
                AnyIsMenuIdentifier(movedTo) ||
                AnyIsMenuIdentifier(movedFrom);

            if (affected)
                MenuIdentifierGenerator.Regenerate();
        }

        private static bool AnyIsMenuIdentifier(string[] paths)
        {
            foreach (string path in paths)
            {
                if (!path.EndsWith(".asset"))
                    continue;

                if (AssetDatabase.GetMainAssetTypeAtPath(path) == typeof(MenuIdentifier))
                    return true;
            }
            return false;
        }
    }
}
#endif