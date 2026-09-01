using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;

namespace Base.AttributePackage.Editor.Core
{
    /// <summary>
    /// Reorders the drawn properties according to <see cref="PropertyOrderAttribute"/>.
    /// </summary>
    /// <remarks>
    /// Sections, foldouts, tabs and horizontal rows are all runs of consecutive fields in this package,
    /// so a naive sort would let one ordered field land in the middle of a run and split it. Fields are
    /// therefore grouped into blocks first, a block being either a single ungrouped field or a whole
    /// run, and the blocks are what gets sorted. A run moves as one thing or not at all.
    /// <para>
    /// Sorting happens inside a section rather than across the object, so an ordered field cannot jump
    /// out of the section it was written in. The block that opens a section carries its title and stays
    /// first, since the header is drawn from that field.
    /// </para>
    /// <para>
    /// The sort is stable and unmarked blocks count as zero, which puts them between the negatives and
    /// the positives and makes "pin this to the top" a matter of one negative number. Nothing is
    /// reordered unless at least one field asks for it.
    /// </para>
    /// </remarks>
    internal static class PropertySorter
    {
        // Reused between inspectors so sorting does not allocate a list per repaint.
        private static readonly List<Block> Blocks = new();

        private static readonly List<SerializedProperty> Sorted = new();

        /// <summary>Sorts the properties in place.</summary>
        /// <param name="properties">The properties to sort.</param>
        /// <param name="type">The inspected type, used to read the attributes.</param>
        internal static void Sort(List<SerializedProperty> properties, Type type)
        {
            if (!Collect(properties, type))
                return;

            SortSections();
            Rebuild(properties);
        }

        // Returns false when nothing asked to be reordered, which is the common case.
        private static bool Collect(List<SerializedProperty> properties, Type type)
        {
            Blocks.Clear();

            bool ordered = false;
            int index = 0;

            while (index < properties.Count)
            {
                FieldInfo field = ReflectionCache.GetField(type, properties[index].name);
                string run = RunKey(field);

                int end = index + 1;

                while (run != null
                       && end < properties.Count
                       && RunKey(ReflectionCache.GetField(type, properties[end].name)) == run)
                    end++;

                Block block = new(index, end, Order(type, properties, index, end),
                    ReflectionCache.GetAttribute<TitleAttribute>(field) != null);

                ordered |= block.Order != 0;

                Blocks.Add(block);
                index = end;
            }

            return ordered;
        }

        // A run is identified by the group it belongs to. Two adjacent runs with different names are two
        // blocks, which is what keeps a tab group from absorbing the row underneath it.
        private static string RunKey(FieldInfo field)
        {
            if (field == null)
                return null;

            HorizontalAttribute horizontal = ReflectionCache.GetAttribute<HorizontalAttribute>(field);
            if (horizontal != null)
                return nameof(HorizontalAttribute) + horizontal.Group;

            TabAttribute tab = ReflectionCache.GetAttribute<TabAttribute>(field);
            if (tab != null)
                return nameof(TabAttribute) + tab.Group;

            FoldoutAttribute foldout = ReflectionCache.GetAttribute<FoldoutAttribute>(field);

            return foldout != null
                ? nameof(FoldoutAttribute) + foldout.Name
                : null;
        }

        // The lowest order among the members wins, so pinning any one field of a run pins the run.
        private static int Order(Type type, List<SerializedProperty> properties, int start, int end)
        {
            int lowest = 0;

            for (int i = start; i < end; i++)
            {
                PropertyOrderAttribute attribute = ReflectionCache.GetAttribute<PropertyOrderAttribute>(
                    ReflectionCache.GetField(type, properties[i].name));

                if (attribute != null)
                    lowest = Math.Min(lowest, attribute.Order);
            }

            return lowest;
        }

        private static void SortSections()
        {
            int start = 0;

            for (int i = 1; i <= Blocks.Count; i++)
            {
                if (i < Blocks.Count && !Blocks[i].OpensSection)
                    continue;

                // The block that opens a section carries the title, so the run starts one past it.
                Apply(Blocks[start].OpensSection
                    ? start + 1
                    : start, i);

                start = i;
            }
        }

        // An insertion sort rather than List.Sort, because List.Sort is not stable and an unstable sort
        // here would shuffle every block that shares an order, which is most of them.
        private static void Apply(int start, int end)
        {
            for (int i = start + 1; i < end; i++)
            {
                Block block = Blocks[i];
                int j = i - 1;

                while (j >= start && Blocks[j].Order > block.Order)
                {
                    Blocks[j + 1] = Blocks[j];
                    j--;
                }

                Blocks[j + 1] = block;
            }
        }

        private static void Rebuild(List<SerializedProperty> properties)
        {
            Sorted.Clear();

            foreach (Block block in Blocks)
            {
                for (int i = block.Start; i < block.End; i++)
                    Sorted.Add(properties[i]);
            }

            properties.Clear();
            properties.AddRange(Sorted);
        }

        private readonly struct Block
        {
            /// <summary>Index of the first property in the block.</summary>
            internal readonly int Start;

            /// <summary>Index one past the last property in the block.</summary>
            internal readonly int End;

            /// <summary>The order the block sorts by. Lower comes first.</summary>
            internal readonly int Order;

            /// <summary>
            /// True when a title starts this block, which is what keeps a section's fields together
            /// rather than letting them sort away from the heading that names them.
            /// </summary>
            internal readonly bool OpensSection;

            /// <summary>Records one run of properties that has to stay contiguous.</summary>
            /// <param name="start">Index of the first property in the block.</param>
            /// <param name="end">Index one past the last property in the block.</param>
            /// <param name="order">The order the block sorts by.</param>
            /// <param name="opensSection">Whether a title starts this block.</param>
            internal Block(int start, int end, int order, bool opensSection)
            {
                Start = start;
                End = end;
                Order = order;
                OpensSection = opensSection;
            }
        }
    }
}