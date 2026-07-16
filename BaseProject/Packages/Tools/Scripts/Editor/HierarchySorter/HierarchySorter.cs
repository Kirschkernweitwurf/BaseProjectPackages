#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using Base.UtilityPackage.Generated;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Base.ToolPackage.Editor.HierarchySorter
{
    /// <summary>
    /// Sorts the children of a GameObject or a whole scene alphabetically, recursively.
    /// </summary>
    internal static class HierarchySorter
    {
        private const string MenuPathScene = "GameObject/Base/Sort Active Scene Alphabetically";
        private const string MenuPathSelected = "GameObject/Base/Sort Children Alphabetically";
        private const string UndoLabel = "Sort Children Alphabetically";

        [MenuItem(MenuPathSelected, false, MenuOrders.GameObject)]
        private static void SortSelected()
        {
            GameObject selected = Selection.activeGameObject;
            if (selected == null)
                return;

            Undo.RegisterFullObjectHierarchyUndo(selected, UndoLabel);
            SortRecursive(selected.transform);
            EditorSceneManager.MarkSceneDirty(selected.scene);
        }

        [MenuItem(MenuPathSelected, true, MenuOrders.GameObject)]
        private static bool CanSortSelected() => Selection.activeGameObject != null;

        [MenuItem(MenuPathScene, false, MenuOrders.GameObject)]
        private static void SortActiveScene()
        {
            Scene scene = SceneManager.GetActiveScene();
            if (!scene.IsValid())
                return;

            GameObject[] roots = scene.GetRootGameObjects();
            foreach (GameObject root in roots)
                Undo.RegisterFullObjectHierarchyUndo(root, UndoLabel);

            // Roots have no parent, so their order is set on the root transforms directly.
            SortSiblings(ToTransforms(roots));
            foreach (GameObject root in roots)
                SortRecursive(root.transform);

            EditorSceneManager.MarkSceneDirty(scene);
        }

        private static void SortRecursive(Transform parent)
        {
            SortChildren(parent);
            for (int i = 0; i < parent.childCount; i++)
                SortRecursive(parent.GetChild(i));
        }

        private static void SortChildren(Transform parent)
        {
            int childCount = parent.childCount;
            if (childCount < 2)
                return;

            List<Transform> children = new(childCount);
            for (int i = 0; i < childCount; i++)
                children.Add(parent.GetChild(i));

            SortSiblings(children);
        }

        private static void SortSiblings(List<Transform> siblings)
        {
            siblings.Sort(CompareByName);
            for (int i = 0; i < siblings.Count; i++)
                siblings[i].SetSiblingIndex(i);
        }

        private static List<Transform> ToTransforms(GameObject[] objects)
        {
            List<Transform> transforms = new(objects.Length);
            foreach (GameObject obj in objects)
                transforms.Add(obj.transform);

            return transforms;
        }

        private static int CompareByName(Transform a, Transform b)
            => string.Compare(a.name, b.name, StringComparison.OrdinalIgnoreCase);
    }
}
#endif