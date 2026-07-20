using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Base.AttributePackage.Editor.Windows.RequiredReferenceWindow
{
    /// <summary>Scans scene objects and ScriptableObject assets for validation issues, grouped per owner.</summary>
    public static class RequiredReferenceCollector
    {
        private const string AssetFilter = "t:ScriptableObject";

        /// <summary>Returns one group per scene object with issues. Scene objects group by GameObject.</summary>
        public static List<RequiredReferenceGroup> CollectScene(out int total)
        {
            total = 0;
            List<RequiredReferenceGroup> groups = new();
            Dictionary<Object, RequiredReferenceGroup> map = new();
            List<ReferenceIssue> buffer = new();

            MonoBehaviour[] behaviours =
                Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            foreach (MonoBehaviour behaviour in behaviours)
            {
                buffer.Clear();
                ReferenceValidationScanner.Collect(behaviour, buffer);

                foreach (ReferenceIssue issue in buffer)
                {
                    if (issue.Owner is not Component component)
                        continue;

                    if (Add(map, groups, component.gameObject, component.GetType().Name, issue.Path))
                        total++;
                }
            }

            return groups;
        }

        /// <summary>Returns one group per ScriptableObject asset with issues.</summary>
        public static List<RequiredReferenceGroup> CollectAssets(out int total)
        {
            total = 0;
            List<RequiredReferenceGroup> groups = new();
            Dictionary<Object, RequiredReferenceGroup> map = new();
            List<ReferenceIssue> buffer = new();

            foreach (string guid in AssetDatabase.FindAssets(AssetFilter))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                ScriptableObject asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
                if (asset == null)
                    continue;

                buffer.Clear();
                ReferenceValidationScanner.Collect(asset, buffer);

                foreach (ReferenceIssue issue in buffer)
                {
                    if (Add(map, groups, asset, asset.GetType().Name, issue.Path))
                        total++;
                }
            }

            return groups;
        }

        private static bool Add(Dictionary<Object, RequiredReferenceGroup> map,
            List<RequiredReferenceGroup> groups, Object owner, string ownerType, string path)
        {
            if (owner == null)
                return false;

            if (!map.TryGetValue(owner, out RequiredReferenceGroup group))
            {
                group = new RequiredReferenceGroup(owner);
                map[owner] = group;
                groups.Add(group);
            }

            group.Entries.Add(new RequiredReferenceEntry(ownerType, path));
            return true;
        }
    }
}