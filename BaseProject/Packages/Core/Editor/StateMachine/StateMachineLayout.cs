using System.Collections.Generic;
using Base.CorePackage.StateMachine;
using UnityEngine;

namespace Base.CorePackage.Editor.StateMachine
{
    /// <summary>
    /// Arranges a machine into columns by distance from the state it started in, so it reads left to right
    /// in the order it can actually run. States nothing can reach are collected in a trailing column, which
    /// makes them obvious instead of leaving them scattered.
    /// </summary>
    internal static class StateMachineLayout
    {
        /// <summary>Height of a drawn state node.</summary>
        internal const float NodeHeight = 40f;

        /// <summary>Width of a drawn state node.</summary>
        internal const float NodeWidth = 168f;

        /// <summary>Space kept around the whole drawing.</summary>
        internal const float Padding = 28f;

        private const float ColumnGap = 96f;
        private const float RowGap = 34f;

        /// <summary>Where the any state node is parked, above and left of the first column.</summary>
        internal static readonly Vector2 AnyStatePosition = new(Padding, Padding);

        /// <summary>Calculates a position for every state of a machine.</summary>
        /// <param name="machine">The machine to lay out.</param>
        /// <param name="hasAnyStateEdges">True when the any state node is drawn and needs room above.</param>
        /// <returns>One position per state name.</returns>
        internal static Dictionary<string, Vector2> Calculate(IStateMachineInfo machine, bool hasAnyStateEdges)
        {
            Dictionary<string, Vector2> placements = new();

            if (machine == null || machine.StateNames.Count == 0)
                return placements;

            List<List<string>> columns = BuildColumns(machine);

            float top = Padding + (hasAnyStateEdges
                ? NodeHeight + RowGap * 2f
                : 0f);

            float tallest = 0f;

            foreach (List<string> column in columns)
                tallest = Mathf.Max(tallest, MeasureHeight(column.Count));

            for (int index = 0; index < columns.Count; index++)
            {
                float x = Padding + index * (NodeWidth + ColumnGap);
                float y = top + (tallest - MeasureHeight(columns[index].Count)) * 0.5f;

                foreach (string stateName in columns[index])
                {
                    placements[stateName] = new Vector2(x, y);
                    y += NodeHeight + RowGap;
                }
            }

            return placements;
        }

        private static float MeasureHeight(int count) => count == 0
            ? 0f
            : count * (NodeHeight + RowGap) - RowGap;

        // Breadth first from the state the machine started in, so a column holds everything that is the
        // same number of transitions away. Targets of any state transitions start one column in, because
        // they can be entered from anywhere but are not where the machine begins.
        private static Dictionary<string, int> ComputeColumns(IStateMachineInfo machine)
        {
            Dictionary<string, int> columnByState = new();
            Queue<string> pending = new();

            string entry = string.IsNullOrEmpty(machine.InitialStateName)
                ? machine.StateNames[0]
                : machine.InitialStateName;

            columnByState[entry] = 0;
            pending.Enqueue(entry);

            foreach (StateMachineEdge edge in machine.Edges)
            {
                if (!edge.IsFromAnyState || columnByState.ContainsKey(edge.To))
                    continue;

                columnByState[edge.To] = 1;
                pending.Enqueue(edge.To);
            }

            while (pending.Count > 0)
            {
                string current = pending.Dequeue();
                int next = columnByState[current] + 1;

                foreach (StateMachineEdge edge in machine.Edges)
                {
                    if (edge.From != current || columnByState.ContainsKey(edge.To))
                        continue;

                    columnByState[edge.To] = next;
                    pending.Enqueue(edge.To);
                }
            }

            return columnByState;
        }

        private static List<List<string>> BuildColumns(IStateMachineInfo machine)
        {
            Dictionary<string, int> columnByState = ComputeColumns(machine);

            int highest = 0;

            foreach (KeyValuePair<string, int> pair in columnByState)
                highest = Mathf.Max(highest, pair.Value);

            // Everything the machine cannot reach from its initial state goes into one column behind it.
            int unreachableColumn = highest + 1;
            List<List<string>> columns = new(unreachableColumn + 1);

            for (int i = 0; i <= unreachableColumn; i++)
                columns.Add(new List<string>());

            foreach (string stateName in machine.StateNames)
            {
                int column = columnByState.TryGetValue(stateName, out int found)
                    ? found
                    : unreachableColumn;

                columns[column].Add(stateName);
            }

            for (int i = columns.Count - 1; i >= 0; i--)
            {
                if (columns[i].Count == 0)
                    columns.RemoveAt(i);
            }

            return columns;
        }
    }
}