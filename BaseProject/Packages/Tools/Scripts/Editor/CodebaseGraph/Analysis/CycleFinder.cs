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
