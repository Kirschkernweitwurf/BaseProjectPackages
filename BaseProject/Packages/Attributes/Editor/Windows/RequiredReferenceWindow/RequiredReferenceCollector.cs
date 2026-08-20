using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Base.AttributePackage.Editor.Drawers.Windows.RequiredReferenceWindow
{
    /// <summary>Scans scene objects and ScriptableObject assets for validation issues, grouped per owner.</summary>
    internal static class RequiredReferenceCollector
    {
        /// <summary>Returns one group per scene object with issues. Scene objects group by GameObject.</summary>
        public static List<RequiredReferenceGroup> CollectScene(out int total)
        {
            MonoBehaviour[] behaviors =
                Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            return Collect(behaviors, ResolveSceneOwner, out total);
        }

        /// <summary>Returns one group per ScriptableObject asset with issues.</summary>
        public static List<RequiredReferenceGroup> CollectAssets(out int total)
        {
            List<ScriptableObject> assets = new(ScriptableObjectAssets.LoadAll());
            return Collect(assets, ResolveAssetOwner, out total);
        }

        private static List<RequiredReferenceGroup> Collect<T>(IReadOnlyList<T> sources,
            Func<Object, Object> resolveOwner, out int total) where T : Object
        {
            total = 0;
            List<RequiredReferenceGroup> groups = new();
            Dictionary<Object, RequiredReferenceGroup> map = new();
            List<ReferenceIssue> buffer = new();

            foreach (T source in sources)
            {
                buffer.Clear();
                ReferenceValidationScanner.Collect(source, buffer);

                foreach (ReferenceIssue issue in buffer)
                {
                    if (issue.Owner == null)
                        continue;

                    Object owner = resolveOwner(issue.Owner);
                    if (owner == null)
                        continue;

                    Add(map, groups, owner, issue.Owner.GetType().Name, issue.Path);
                    total++;
                }
            }

            return groups;
        }

        // Scene issues are grouped by GameObject, so all components of one object share a header.
        private static Object ResolveSceneOwner(Object issueOwner) => issueOwner is Component component
            ? component.gameObject
            : null;

        private static Object ResolveAssetOwner(Object issueOwner) => issueOwner;

        private static void Add(Dictionary<Object, RequiredReferenceGroup> map,
            List<RequiredReferenceGroup> groups, Object owner, string ownerType, string path)
        {
            if (!map.TryGetValue(owner, out RequiredReferenceGroup group))
            {
                group = new RequiredReferenceGroup(owner);
                map[owner] = group;
                groups.Add(group);
            }

            group.Entries.Add(new RequiredReferenceEntry(ownerType, path));
        }
    }
}