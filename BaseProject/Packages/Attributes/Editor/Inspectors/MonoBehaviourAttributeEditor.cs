using Base.AttributePackage.Editor.SceneHandles;
using UnityEditor;
using UnityEngine;

namespace Base.AttributePackage.Editor
{
    /// <summary>
    /// Applies the attribute package inspector to all MonoBehaviour types without an own editor, and
    /// hosts the scene view handles. Only the component editor does that: an asset has no transform for
    /// a handle to be positioned against.
    /// </summary>
    [CustomEditor(typeof(MonoBehaviour), true)]
    [CanEditMultipleObjects]
    internal sealed class MonoBehaviourAttributeEditor : AttributePackageEditor
    {
        // The inspector's own serializedObject must not be touched from OnSceneGUI. The scene view can
        // repaint while the inspector is mid-draw, which would leave two walks running over the same
        // object, and Unity logs a warning when it catches it. This editor therefore owns a second one.
        // It is kept alive rather than rebuilt every frame, since OnSceneGUI runs on every repaint.
        private SerializedObject _sceneObject;

#region Unity Callbacks
        private void OnDisable() => Release();

        // Found by Unity through reflection on the concrete editor type, which is why it is declared
        // here rather than inherited from the abstract base.
        private void OnSceneGUI()
        {
            if (target == null)
            {
                Release();
                return;
            }

            // Multi-object selections and undo can swap the target under a live editor, so the cached
            // view is rebuilt whenever it no longer belongs to the object being drawn.
            if (_sceneObject == null || _sceneObject.targetObject != target)
            {
                Release();
                _sceneObject = new SerializedObject(target);
            }

            HandleRenderer.Draw(_sceneObject);
        }
#endregion

        private void Release()
        {
            _sceneObject?.Dispose();
            _sceneObject = null;
        }
    }
}