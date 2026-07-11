using System.Collections.Generic;
using Base.AttributePackage.Validation;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Base.AttributePackage.Editor.Windows.RequiredReferenceWindow
{
    /// <summary>Scans the open scenes for missing required references and groups them per object.</summary>
    public static class RequiredReferenceCollector
    {
        /// <summary>Returns one group per object with missing references and reports the total count.</summary>
        public static List<RequiredReferenceGroup> Collect(out int total)
        {
            total = 0;
            List<RequiredReferenceGroup> groups = new();
            Dictionary<GameObject, RequiredReferenceGroup> map = new();

            MonoBehaviour[] behaviours =
                Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            List<MissingRequiredReference> buffer = new();

            foreach (MonoBehaviour behaviour in behaviours)
            {
                buffer.Clear();
                RequiredReferenceScanner.Collect(behaviour, buffer);

                foreach (MissingRequiredReference missing in buffer)
                {
                    if (Add(map, groups, missing))
                        total++;
                }
            }

            return groups;
        }

        private static bool Add(Dictionary<GameObject, RequiredReferenceGroup> map,
            List<RequiredReferenceGroup> groups, MissingRequiredReference missing)
        {
            if (missing.Component == null)
                return false;

            GameObject owner = missing.Component.gameObject;
            if (!map.TryGetValue(owner, out RequiredReferenceGroup group))
            {
                group = new RequiredReferenceGroup(owner);
                map[owner] = group;
                groups.Add(group);
            }

            group.Entries.Add(new RequiredReferenceEntry(missing.Component.GetType().Name, missing.Path));

            return true;
        }
    }
}