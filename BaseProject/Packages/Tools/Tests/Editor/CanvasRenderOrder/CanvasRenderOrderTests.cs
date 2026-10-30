using System.Collections.Generic;
using Base.ToolsPackage.Editor.CanvasRenderOrder;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Base.ToolsPackage.Editor.Tests.CanvasRenderOrder
{
    /// <summary>
    /// What the Canvas Render Order window lists, in which order, and how a change is written back.
    /// <para>
    /// The window is only worth anything if its order is the one Unity draws in. A list that puts an overlay
    /// behind a camera canvas, or shows a nested canvas that draws with its parent, sends people fixing the
    /// wrong number.
    /// </para>
    /// </summary>
    public sealed class CanvasRenderOrderTests
    {
        private const string BackName = "Back Canvas";
        private const string FrontName = "Front Canvas";
        private const string NestedName = "Nested Canvas";
        private const string PrefabPath = "Assets/CanvasRenderOrderTestPrefab.prefab";
        private const string RootName = "Root Canvas";

        private readonly List<GameObject> _objects = new();

        /// <summary>Hands back the objects and the prefab the test built.</summary>
        [TearDown]
        public void Cleanup()
        {
            foreach (GameObject created in _objects)
            {
                if (created != null)
                    Object.DestroyImmediate(created);
            }

            _objects.Clear();
            AssetDatabase.DeleteAsset(PrefabPath);
        }

        /// <summary>Every overlay draws after every camera canvas, however low its order is.</summary>
        [Test]
        public void OverlayDrawsAfterCameraCanvasWhateverTheOrder()
        {
            Canvas overlay = CreateCanvas(FrontName, RenderMode.ScreenSpaceOverlay, -100);
            Canvas world = CreateCanvas(BackName, RenderMode.WorldSpace, 100);

            List<CanvasEntry> entries = ScanOwn();
            entries.Sort(CanvasEntry.CompareRenderOrder);

            Assert.That(entries, Has.Count.EqualTo(2));
            Assert.That(entries[0].Canvas, Is.EqualTo(world));
            Assert.That(entries[1].Canvas, Is.EqualTo(overlay));
        }

        /// <summary>Inside one pass the lower order draws first.</summary>
        [Test]
        public void LowerOrderDrawsFirst()
        {
            Canvas front = CreateCanvas(FrontName, RenderMode.ScreenSpaceOverlay, 5);
            Canvas back = CreateCanvas(BackName, RenderMode.ScreenSpaceOverlay, 1);

            List<CanvasEntry> entries = ScanOwn();
            entries.Sort(CanvasEntry.CompareRenderOrder);

            Assert.That(entries[0].Canvas, Is.EqualTo(back));
            Assert.That(entries[1].Canvas, Is.EqualTo(front));
        }

        /// <summary>A nested canvas only sorts on its own with Override Sorting.</summary>
        [Test]
        public void NestedCanvasIsListedOnlyWithOverrideSorting()
        {
            Canvas root = CreateCanvas(RootName, RenderMode.ScreenSpaceOverlay, 0);
            Canvas nested = CreateCanvas(NestedName, RenderMode.ScreenSpaceOverlay, 3);
            nested.transform.SetParent(root.transform);

            Assert.That(ScanOwn(), Has.Count.EqualTo(1));

            nested.overrideSorting = true;
            List<CanvasEntry> entries = ScanOwn();

            Assert.That(entries, Has.Count.EqualTo(2));
            Assert.That(entries.Find(entry => entry.Canvas == nested).Pass, Is.EqualTo(ECanvasPass.Overlay));
        }

        /// <summary>Two canvases on the same layer and order share a slot, a different order does not.</summary>
        [Test]
        public void SameLayerAndOrderShareASlot()
        {
            CreateCanvas(BackName, RenderMode.WorldSpace, 2);
            CreateCanvas(FrontName, RenderMode.WorldSpace, 2);

            List<CanvasEntry> entries = ScanOwn();

            Assert.That(entries[0].IsSameSlot(entries[1]), Is.True);

            entries[1].SetSorting(entries[1].SortingLayerId, 3);

            Assert.That(entries[0].IsSameSlot(entries[1]), Is.False);
        }

        /// <summary>A scene canvas takes the new order and the change can be undone.</summary>
        [Test]
        public void WritingASceneCanvasIsUndoable()
        {
            Canvas canvas = CreateCanvas(RootName, RenderMode.ScreenSpaceOverlay, 1);
            CanvasEntry entry = ScanOwn()[0];

            Assert.That(CanvasSortingWriter.TryWrite(entry, canvas.sortingLayerID, 7), Is.True);
            Assert.That(canvas.sortingOrder, Is.EqualTo(7));

            Undo.PerformUndo();

            Assert.That(canvas.sortingOrder, Is.EqualTo(1));
        }

        /// <summary>A prefab canvas, also a nested one, is found by its path and saved into the asset.</summary>
        [Test]
        public void WritingAPrefabCanvasSavesTheAsset()
        {
            Canvas root = CreateCanvas(RootName, RenderMode.ScreenSpaceOverlay, 0);
            Canvas nested = CreateCanvas(NestedName, RenderMode.ScreenSpaceOverlay, 2);
            nested.transform.SetParent(root.transform);
            nested.overrideSorting = true;
            PrefabUtility.SaveAsPrefabAsset(root.gameObject, PrefabPath);

            CanvasEntry entry = CanvasScanner.ScanPrefabs()
                .Find(found => found.AssetPath == PrefabPath && found.Name == NestedName);

            Assert.That(entry, Is.Not.Null);
            Assert.That(CanvasSortingWriter.TryWrite(entry, nested.sortingLayerID, 9), Is.True);

            GameObject saved = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            Canvas savedNested = saved.transform.GetChild(0).GetComponent<Canvas>();

            Assert.That(savedNested.sortingOrder, Is.EqualTo(9));
        }

        private Canvas CreateCanvas(string objectName, RenderMode renderMode, int sortingOrder)
        {
            GameObject created = new(objectName);
            _objects.Add(created);

            Canvas canvas = created.AddComponent<Canvas>();
            canvas.renderMode = renderMode;
            canvas.sortingOrder = sortingOrder;

            return canvas;
        }

        // The open scene may hold canvases of its own, only the ones this test built count.
        private List<CanvasEntry> ScanOwn() => CanvasScanner.ScanScenes()
            .FindAll(entry => _objects.Contains(entry.Canvas.gameObject));
    }
}