using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UINavigation = UnityEngine.UI.Navigation;

namespace Base.ControllerSupport.Controller.Navigation
{
    /// <summary>
    /// Computes and writes explicit four-way navigation across a set of <see cref="NavigableElement"/>s
    /// from their on-screen position. For each element it picks the nearest aligned neighbor in every
    /// direction, mirroring uGUI's built-in directional scoring so the result feels familiar.
    /// </summary>
    public static class NavigationBuilder
    {
        /// <summary>
        /// Wires explicit up, down, left and right navigation across the navigable elements in the list.
        /// Non-navigable elements are skipped. When <paramref name="wrap"/> is true, edges loop to the
        /// opposite side of the set.
        /// </summary>
        public static void Wire(IReadOnlyList<NavigableElement> elements, bool wrap)
        {
            if (elements == null)
                return;

            List<NavigableElement> active = Collect(elements);

            if (active.Count == 0)
                return;

            foreach (NavigableElement element in active)
            {
                UINavigation navigation = new()
                {
                    mode = UINavigation.Mode.Explicit,
                    selectOnUp = FindNeighbour(element, active, Vector2.up, wrap),
                    selectOnDown = FindNeighbour(element, active, Vector2.down, wrap),
                    selectOnLeft = FindNeighbour(element, active, Vector2.left, wrap),
                    selectOnRight = FindNeighbour(element, active, Vector2.right, wrap)
                };

                element.Selectable.navigation = navigation;
            }
        }

        private static List<NavigableElement> Collect(IReadOnlyList<NavigableElement> elements)
        {
            List<NavigableElement> active = new();

            foreach (NavigableElement element in elements)
            {
                if (element != null && element.IsNavigable())
                    active.Add(element);
            }

            return active;
        }

        private static Selectable FindNeighbour(NavigableElement source, List<NavigableElement> candidates,
            Vector2 direction, bool wrap)
        {
            Vector2 origin = GetCenter(source.Selectable);

            Selectable best = null;
            float bestScore = float.NegativeInfinity;

            Selectable wrapBest = null;
            float wrapScore = float.NegativeInfinity;

            foreach (NavigableElement candidate in candidates)
            {
                if (candidate == source)
                    continue;

                Vector2 toTarget = GetCenter(candidate.Selectable) - origin;
                float forward = Vector2.Dot(direction, toTarget);

                if (forward > 0f)
                {
                    // Favor aligned and near candidates, the same heuristic uGUI uses internally.
                    float score = forward / toTarget.sqrMagnitude;

                    if (score > bestScore)
                    {
                        bestScore = score;
                        best = candidate.Selectable;
                    }

                    continue;
                }

                if (!wrap)
                    continue;

                // Track the element furthest in the opposite direction as the wrap target.
                float opposite = Vector2.Dot(-direction, toTarget);

                if (!(opposite > wrapScore))
                    continue;

                wrapScore = opposite;
                wrapBest = candidate.Selectable;
            }

            if (best != null)
                return best;

            return wrap
                ? wrapBest
                : null;
        }

        private static Vector2 GetCenter(Selectable selectable)
        {
            if (selectable.transform is RectTransform rectTransform)
                return rectTransform.TransformPoint(rectTransform.rect.center);

            return selectable.transform.position;
        }
    }
}