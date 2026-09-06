using System;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Base.AttributesPackage.Editor.Windows.AttributeExplorer.Reference
{
    /// <summary>
    /// The live sample behind an attribute page: the object carrying the attribute, the inspector
    /// drawing it, the script it came from and the snippet read out of that script.
    /// <para>
    /// Held apart from the pane because all four are one lifetime. They are created together, they are
    /// reused together when the same attribute is opened twice, and they have to be destroyed together
    /// or a temporary object and an editor are left behind on every page turn.
    /// </para>
    /// </summary>
    internal sealed class AttributeSamplePreview
    {
        private GameObject _host;

        /// <summary>The object carrying the attribute, or null while no attribute is shown.</summary>
        internal Object Instance { get; private set; }

        /// <summary>The inspector drawing that object, or null while no attribute is shown.</summary>
        internal UnityEditor.Editor Editor { get; private set; }

        /// <summary>The script the sample is written in, or null when it has none.</summary>
        internal MonoScript Script { get; private set; }

        /// <summary>The part of that script the page shows, or an empty string when it has none.</summary>
        internal string Snippet { get; private set; } = string.Empty;

        /// <summary>
        /// Shows the sample for the given entry, reusing what is already built when the same attribute
        /// is opened again.
        /// </summary>
        /// <param name="entry">The entry whose sample to show.</param>
        internal void Show(in AttributeSampleEntry entry)
        {
            if (Instance == null || Instance.GetType() != entry.SampleType)
                Build(entry);

            // A page is opened to be read, not continued, so the sample presents as authored rather
            // than as the last reader happened to leave it.
            SamplePreviewDefaults.Reapply(entry.SampleType);

            Snippet = Script == null
                ? string.Empty
                : AttributeSampleSource.Extract(Script.text, entry.SampleType.Name);
        }

        /// <summary>Destroys the sample and everything built around it.</summary>
        internal void Release()
        {
            if (Editor != null)
                Object.DestroyImmediate(Editor);

            AttributeSampleHost.DestroyPreview(_host);

            // A component sample is destroyed together with the object it lives on, so only an asset
            // sample is left to clean up on its own.
            if (_host == null && Instance != null)
                Object.DestroyImmediate(Instance);

            Editor = null;
            Instance = null;
            Script = null;
            Snippet = string.Empty;
            _host = null;
        }

        private static MonoScript ScriptOf(Object instance) => instance switch
        {
            MonoBehaviour behaviour => MonoScript.FromMonoBehaviour(behaviour),
            ScriptableObject asset => MonoScript.FromScriptableObject(asset),
            _ => null
        };

        private void Build(in AttributeSampleEntry entry)
        {
            Release();

            // Never saved, so nothing here can be committed by accident.
            Instance = AttributeSampleHost.CreatePreview(entry.SampleType, entry.Title, out _host);
            Editor = UnityEditor.Editor.CreateEditor(Instance);
            Script = ScriptOf(Instance);
        }
    }
}