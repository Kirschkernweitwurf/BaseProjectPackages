using System.Collections;
using System.Collections.Generic;
using System.Text;
using Object = UnityEngine.Object;

namespace Base.AttributesPackage
{
    /// <summary>
    /// Finds every duplicate group inside a list or array. A group holds all indices that share the same
    /// value. Shared by <see cref="UniqueRule"/> and the editor handler, so both report the same result.
    /// </summary>
    internal static class DuplicateFinder
    {
        private const string GroupSeparator = "; ";
        private const string IndexSeparator = ", ";
        private const string ReasonPrefix = "has duplicate entries at index ";

        /// <summary>
        /// Fills the list with one text per duplicate group, for example "0, 2". Null and empty entries
        /// are skipped, so freshly added slots do not count as duplicates while the list is being filled.
        /// </summary>
        internal static void Collect(IList list, List<string> groups)
        {
            groups.Clear();

            if (list == null)
                return;

            for (int i = 0; i < list.Count; i++)
            {
                object value = list[i];

                // Only the first occurrence opens a group, so the same value is never reported twice.
                if (IsEmpty(value) || HasMatchBefore(list, i, value))
                    continue;

                string group = BuildGroup(list, i, value);
                if (group != null)
                    groups.Add(group);
            }
        }

        /// <summary>
        /// Fills the list with every index that repeats an earlier entry, in ascending order. First
        /// occurrences are not included, so removing these indices leaves one entry per value.
        /// </summary>
        internal static void CollectRepeats(IList list, List<int> indices)
        {
            indices.Clear();

            if (list == null)
                return;

            for (int i = 0; i < list.Count; i++)
            {
                object value = list[i];

                if (!IsEmpty(value) && HasMatchBefore(list, i, value))
                    indices.Add(i);
            }
        }

        /// <summary>Builds the message text for a single duplicate group.</summary>
        internal static string Describe(string group) => ReasonPrefix + group;

        /// <summary>Builds one message text covering all duplicate groups.</summary>
        internal static string Describe(IReadOnlyList<string> groups)
            => ReasonPrefix + string.Join(GroupSeparator, groups);

        private static string BuildGroup(IList list, int index, object value)
        {
            StringBuilder builder = null;

            for (int i = index + 1; i < list.Count; i++)
            {
                if (!AreEqual(value, list[i]))
                    continue;

                builder ??= new StringBuilder().Append(index);
                builder.Append(IndexSeparator).Append(i);
            }

            return builder?.ToString();
        }

        private static bool HasMatchBefore(IList list, int index, object value)
        {
            for (int i = 0; i < index; i++)
            {
                if (AreEqual(value, list[i]))
                    return true;
            }

            return false;
        }

        private static bool AreEqual(object left, object right)
        {
            // Unity's overloaded == is needed so destroyed objects are not treated as equal references.
            if (left is Object leftObject && right is Object rightObject)
                return leftObject == rightObject;

            return Equals(left, right);
        }

        private static bool IsEmpty(object value)
        {
            if (value is string text)
                return string.IsNullOrEmpty(text);

            if (value is Object unityObject)
                return unityObject == null;

            return value == null;
        }
    }
}