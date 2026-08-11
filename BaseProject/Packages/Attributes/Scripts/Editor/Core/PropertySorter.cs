using System;
using System.Collections.Generic;
using UnityEditor;

namespace Base.AttributePackage.Editor
{
    /// <summary>
    /// Reorders the drawn properties according to <see cref="PropertyOrderAttribute"/>.
    /// </summary>
    /// <remarks>
    /// The sort is stable, so a field without the attribute never moves relative to its neighbours and
    /// a single marked field moves alone. Unmarked fields count as zero, which puts them between the
    /// negatives and the positives and makes "pin this to the top" a matter of one negative number.
    /// <para>
    /// The list is only reordered when at least one field asks for it, so the common case pays nothing
    /// beyond the check.
    /// </para>
    /// </remarks>
    internal static class PropertySorter
    {
        /// <summary>Sorts the properties in place.</summary>
        /// <param name="properties">The properties to sort.</param>
        /// <param name="type">The inspected type, used to read the attributes.</param>
        public static void Sort(List<SerializedProperty> properties, Type type)
        {
            int[] orders = new int[properties.Count];
            bool ordered = false;

            for (int i = 0; i < properties.Count; i++)
            {
                PropertyOrderAttribute attribute = ReflectionCache.GetAttribute<PropertyOrderAttribute>(
                    ReflectionCache.GetField(type, properties[i].name));

                orders[i] = attribute?.Order ?? 0;
                ordered |= attribute != null;
            }

            if (!ordered)
                return;

            Apply(properties, orders);
        }

        // An insertion sort rather than List.Sort, because List.Sort is not stable and an unstable sort
        // here would shuffle every field that shares an order, which is most of them.
        private static void Apply(List<SerializedProperty> properties, int[] orders)
        {
            for (int i = 1; i < properties.Count; i++)
            {
                SerializedProperty property = properties[i];
                int order = orders[i];
                int j = i - 1;

                while (j >= 0 && orders[j] > order)
                {
                    properties[j + 1] = properties[j];
                    orders[j + 1] = orders[j];
                    j--;
                }

                properties[j + 1] = property;
                orders[j + 1] = order;
            }
        }
    }
}