using UnityEditor;
using UnityEngine;

namespace Base.UtilityPackage.Identification.Editor
{
    /// <summary>
    /// Generic custom editor that displays the unique ID of any object
    /// that implements <see cref="IUniquelyIdentifiable"/>  in a read-only manner.
    /// </summary>
    /// <typeparam name="T">The type of object to display, must implement <see cref="IUniquelyIdentifiable"/>.</typeparam>
    public class UniqueIdEditor<T> : UnityEditor.Editor where T : Object, IUniquelyIdentifiable
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            T data = (T)target;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Unique ID", EditorStyles.boldLabel);
            EditorGUILayout.SelectableLabel(data.UniqueId ?? "<No ID>", EditorStyles.textField);
        }
    }
}