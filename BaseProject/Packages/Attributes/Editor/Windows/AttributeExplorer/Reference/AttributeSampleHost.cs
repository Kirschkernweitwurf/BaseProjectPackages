using System;
using Base.AttributesPackage.Samples;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Base.AttributesPackage.Editor.Windows.AttributeExplorer.Reference
{
    /// <summary>
    /// Creates and destroys the object a sample is drawn on, whichever kind of object that is.
    /// </summary>
    /// <remarks>
    /// Most samples are ScriptableObjects, which need nothing but an instance. The ones demonstrating a
    /// component getter, a scene constraint or a scene handle cannot be: those attributes are about a
    /// GameObject and its hierarchy, so their samples are MonoBehaviours and need one to live on.
    /// <para>
    /// The preview object is hidden and never saved. The copy the reader can create in the scene is
    /// visible and selectable, since that is the whole point of it, but still never saved: these samples
    /// live in an editor assembly and a scene holding one would lose the component in a build.
    /// </para>
    /// </remarks>
    internal static class AttributeSampleHost
    {
        private const string ScenePrefix = "Attribute Sample - ";

        /// <summary>Creates the hidden object a preview is drawn on.</summary>
        /// <param name="sampleType">The sample type to instantiate.</param>
        /// <param name="title">The name given to the object.</param>
        /// <param name="root">The GameObject that was created, or null for an asset sample.</param>
        /// <returns>The object the editor should be created for.</returns>
        internal static Object CreatePreview(Type sampleType, string title, out GameObject root)
        {
            if (!IsComponent(sampleType))
            {
                root = null;

                ScriptableObject asset = ScriptableObject.CreateInstance(sampleType);

                asset.name = title;
                asset.hideFlags = HideFlags.DontSave;

                return asset;
            }

            GameObject host = new(title);
            Component component = host.AddComponent(sampleType);

            Build(component);

            root = host.transform.root.gameObject;
            Hide(root.transform);

            return component;
        }

        /// <summary>
        /// Creates a visible copy in the open scene, so the reader can select it and see what only the
        /// Scene view or the real Inspector can show.
        /// </summary>
        /// <param name="sampleType">The sample type to instantiate.</param>
        /// <param name="title">The name the object is given, prefixed so it is recognizable.</param>
        internal static void CreateInScene(Type sampleType, string title)
        {
            if (!IsComponent(sampleType))
                return;

            GameObject host = new(ScenePrefix + title);
            Component component = host.AddComponent(sampleType);

            Build(component);

            GameObject root = host.transform.root.gameObject;

            // Visible and selectable, but still never written into the scene file.
            MarkTemporary(root.transform);

            Undo.RegisterCreatedObjectUndo(root, ScenePrefix + title);

            Selection.activeGameObject = host;
            EditorGUIUtility.PingObject(host);
        }

        /// <summary>Destroys a preview object and everything the sample built around it.</summary>
        /// <param name="root">The root returned when the preview was created.</param>
        internal static void DestroyPreview(GameObject root)
        {
            if (root != null)
                Object.DestroyImmediate(root);
        }

        /// <summary>Whether the sample lives on a GameObject rather than being an asset.</summary>
        /// <param name="sampleType">The sample type to test.</param>
        /// <returns>True for a component sample.</returns>
        internal static bool IsComponent(Type sampleType)
            => sampleType != null && typeof(Component).IsAssignableFrom(sampleType);

        private static void Build(Component component)
        {
            if (component is ISampleSetup setup)
                setup.BuildSample();
        }

        // Applied after the sample built its hierarchy, so a parent it created is hidden as well.
        private static void Hide(Transform root)
        {
            root.gameObject.hideFlags = HideFlags.HideAndDontSave;

            foreach (Transform child in root)
                Hide(child);
        }

        private static void MarkTemporary(Transform root)
        {
            root.gameObject.hideFlags = HideFlags.DontSave;

            foreach (Transform child in root)
                MarkTemporary(child);
        }
    }
}