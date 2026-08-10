using System;
using System.Collections.Generic;

namespace Base.ToolPackage.Editor.CodebaseGraph.Analysis
{
    /// <summary>
    /// Finds groups of nodes that can all reach each other. Written iteratively rather than
    /// recursively, because a deep dependency chain would otherwise blow the stack on a large project.
    /// </summary>
    public static class CycleFinder
    {
        private const int ShortestPossibleCycle = 2;

        /// <summary>
        /// Finds the tightest loop inside a component. A strongly connected component is not a cycle:
        /// forty tangled types can all reach each other without any of them sitting on a short loop, and
        /// a finding that names all forty is true and unusable. The shortest loop through it is three or
        /// four names, and one of its edges can actually be cut.
        /// </summary>
        /// <typeparam name="T">Node identity type.</typeparam>
        /// <param name="component">Nodes of one strongly connected component.</param>
        /// <param name="getTargets">Returns the nodes a given node depends on.</param>
        /// <returns>The cycle in order, or an empty list when the component holds none.</returns>
        public static List<T> FindShortestCycle<T>(IReadOnlyCollection<T> component,
            Func<T, IEnumerable<T>> getTargets)
        {
            HashSet<T> members = new(component);
            List<T> best = null;

            foreach (T start in component)
            {
                List<T> found = FindCycleFrom(start, members, getTargets);

                if (found != null && (best == null || found.Count < best.Count))
                    best = found;

                // Two nodes pointing at each other is as tight as a loop can be.
                if (best != null && best.Count == ShortestPossibleCycle)
                    break;
            }

            return best ?? new List<T>();
        }

        private static List<T> FindCycleFrom<T>(T start,
            HashSet<T> members,
            Func<T, IEnumerable<T>> getTargets)
        {
            Dictionary<T, T> parents = new();
            HashSet<T> seen = new() { start };
            Queue<T> pending = new();

            pending.Enqueue(start);

            while (pending.Count > 0)
            {
                T current = pending.Dequeue();

                foreach (T target in getTargets(current))
                {
                    if (!members.Contains(target))
                        continue;

                    if (EqualityComparer<T>.Default.Equals(target, start))
                        return BuildPath(start, current, parents);

                    if (!seen.Add(target))
                        continue;

                    parents[target] = current;
                    pending.Enqueue(target);
                }
            }

            return null;
        }

        private static List<T> BuildPath<T>(T start, T last, Dictionary<T, T> parents)
        {
            List<T> path = new();
            T current = last;

            while (!EqualityComparer<T>.Default.Equals(current, start))
            {
                path.Add(current);

                if (!parents.TryGetValue(current, out T parent))
                    break;

                current = parent;
            }

            path.Add(start);
            path.Reverse();

            return path;
        }

        /// <summary>Returns every group of two or more nodes that sit in a dependency cycle.</summary>
        /// <typeparam name="T">Node identity type.</typeparam>
        /// <param name="nodes">All nodes to consider.</param>
        /// <param name="getTargets">Returns the nodes a given node depends on.</param>
        /// <returns>One list per cycle, each holding the members of that cycle.</returns>
        public static List<List<T>> FindCycles<T>(IEnumerable<T> nodes, Func<T, IEnumerable<T>> getTargets)
        {
            Dictionary<T, int> index = new();
            Dictionary<T, int> lowLink = new();
            HashSet<T> onStack = new();
            Stack<T> component = new();
            Stack<Frame<T>> work = new();
            List<List<T>> cycles = new();
            int nextIndex = 0;

            foreach (T node in nodes)
            {
                if (index.ContainsKey(node))
                    continue;

                Visit(node, ref nextIndex, index, lowLink, onStack, component, work);

                while (work.Count > 0)
                    Step(getTargets, ref nextIndex, index, lowLink, onStack, component, work, cycles);
            }

            return cycles;
        }

        private static void Visit<T>(T node,
            ref int nextIndex,
            Dictionary<T, int> index,
            Dictionary<T, int> lowLink,
            HashSet<T> onStack,
            Stack<T> component,
            Stack<Frame<T>> work)
        {
            index[node] = nextIndex;
            lowLink[node] = nextIndex;
            nextIndex++;

            component.Push(node);
            onStack.Add(node);
            work.Push(new Frame<T>(node));
        }

        private static void Step<T>(Func<T, IEnumerable<T>> getTargets,
            ref int nextIndex,
            Dictionary<T, int> index,
            Dictionary<T, int> lowLink,
            HashSet<T> onStack,
            Stack<T> component,
            Stack<Frame<T>> work,
            List<List<T>> cycles)
        {
            Frame<T> frame = work.Peek();
            frame.EnsureEnumerator(getTargets);

            if (frame.Enumerator.MoveNext())
            {
                T target = frame.Enumerator.Current;

                if (!index.ContainsKey(target))
                {
                    Visit(target, ref nextIndex, index, lowLink, onStack, component, work);
                    return;
                }

                if (onStack.Contains(target))
                    lowLink[frame.Node] = Math.Min(lowLink[frame.Node], index[target]);

                return;
            }

            work.Pop();

            if (work.Count > 0)
            {
                T parent = work.Peek().Node;
                lowLink[parent] = Math.Min(lowLink[parent], lowLink[frame.Node]);
            }

            if (lowLink[frame.Node] != index[frame.Node])
                return;

            List<T> found = new();
            T popped;

            do
            {
                popped = component.Pop();
                onStack.Remove(popped);
                found.Add(popped);
            }
            while (!EqualityComparer<T>.Default.Equals(popped, frame.Node));

            if (found.Count > 1)
                cycles.Add(found);
        }

        /// <summary>One stack frame of the iterative depth first walk.</summary>
        /// <typeparam name="T">Node identity type.</typeparam>
        private sealed class Frame<T>
        {
            public T Node { get; }

            public IEnumerator<T> Enumerator { get; private set; }

            public Frame(T node) => Node = node;

            public void EnsureEnumerator(Func<T, IEnumerable<T>> getTargets)
                => Enumerator ??= getTargets(Node).GetEnumerator();
        }
    }
}
