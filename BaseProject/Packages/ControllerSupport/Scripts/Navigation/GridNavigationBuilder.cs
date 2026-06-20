using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ControllerSupport
{
    /// <summary>
    /// Wires explicit grid navigation between the active child selectables at runtime. Built for
    /// dynamic grids (card hands, collection views) where item count changes. Call <see cref="Rebuild"/>
    /// after adding or removing items, or rely on automatic rebuilds when children change.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public sealed class GridNavigationBuilder : MonoBehaviour
    {
        [Tooltip("Number of columns. Rows are derived from the active child count.")]
        [SerializeField] private int columns = 1;

        [Tooltip("If true, navigation wraps around the grid edges.")]
        [SerializeField] private bool wrap;

        [Tooltip("If true, rebuilds automatically whenever the child count changes.")]
        [SerializeField] private bool rebuildOnChildrenChanged = true;

        private readonly List<Selectable> _items = new();

#region Unity Callbacks
        private void OnEnable() => Rebuild();

        private void OnTransformChildrenChanged()
        {
            if (rebuildOnChildrenChanged)
                Rebuild();
        }
#endregion

        /// <summary>Recollects active child selectables and rewires explicit grid navigation.</summary>
        private void Rebuild()
        {
            CollectItems();
            WireNavigation();
        }

        private void CollectItems()
        {
            _items.Clear();

            foreach (Transform child in transform)
            {
                if (!child.gameObject.activeInHierarchy)
                    continue;

                Selectable selectable = child.GetComponent<Selectable>();

                if (selectable != null && selectable.IsInteractable())
                    _items.Add(selectable);
            }
        }

        private void WireNavigation()
        {
            if (columns < 1)
                columns = 1;

            int count = _items.Count;

            if (count == 0)
                return;

            int rows = Mathf.CeilToInt(count / (float)columns);

            for (int index = 0; index < count; index++)
            {
                int row = index / columns;
                int column = index % columns;

                Navigation navigation = new()
                {
                    mode = Navigation.Mode.Explicit,
                    selectOnLeft = Neighbor(row, column - 1),
                    selectOnRight = Neighbor(row, column + 1),
                    selectOnUp = Neighbor(row - 1, column),
                    selectOnDown = Neighbor(row + 1, column)
                };

                _items[index].navigation = navigation;
            }

            return;

            Selectable Neighbor(int row, int column)
            {
                if (column < 0 || column >= columns)
                {
                    if (!wrap)
                        return null;

                    column = (column + columns) % columns;
                }

                if (row < 0)
                {
                    if (!wrap)
                        return null;

                    row = rows - 1;
                }
                else if (row >= rows)
                {
                    if (!wrap)
                        return null;

                    row = 0;
                }

                int neighborIndex = row * columns + column;

                if (neighborIndex < 0 || neighborIndex >= count)
                    return null;

                return _items[neighborIndex];
            }
        }
    }
}