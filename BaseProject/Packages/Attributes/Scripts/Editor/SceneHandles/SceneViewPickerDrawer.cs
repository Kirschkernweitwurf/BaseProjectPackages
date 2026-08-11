using System;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Base.AttributePackage.Editor.SceneHandles
{
    /// <summary>
    /// Handles the scene view half of <see cref="SceneViewPickerAttribute"/>. While the field is armed
    /// the next click assigns whatever was hit, and the click is swallowed so the selection does not
    /// jump to the object that was just picked.
    /// </summary>
    internal sealed class SceneViewPickerDrawer : HandleDrawer<SceneViewPickerAttribute>
    {
        private const string Hint = "Click an object to assign it. Escape cancels.";

        private static readonly Vector2 HintPosition = new(10f, 10f);

        private static readonly Vector2 HintSize = new(320f, 40f);

        protected override void Draw(in HandleContext context, SceneViewPickerAttribute attribute)
        {
            if (context.Property.propertyType != SerializedPropertyType.ObjectReference)
                return;

            if (!ScenePickerState.IsArmedFor(context.Property))
                return;

            DrawHint(context);

            // Taking the default control stops the scene view from running its own selection logic while
            // the picker owns the click.
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

            Event current = Event.current;

            if (current.type == EventType.KeyDown && current.keyCode == KeyCode.Escape)
            {
                ScenePickerState.Disarm();
                current.Use();
                return;
            }

            if (current.type != EventType.MouseDown || current.button != 0)
                return;

            Assign(context, HandleUtility.PickGameObject(current.mousePosition, false));

            ScenePickerState.Disarm();
            current.Use();
        }

        private static void DrawHint(in HandleContext context)
        {
            UnityEditor.Handles.BeginGUI();
            GUILayout.BeginArea(new Rect(HintPosition.x, HintPosition.y, HintSize.x, HintSize.y));
            EditorGUILayout.HelpBox($"{context.DisplayName}: {Hint}", MessageType.Info);
            GUILayout.EndArea();
            UnityEditor.Handles.EndGUI();
        }

        // The field may want the GameObject itself or a component on it, so the hit object is narrowed
        // to whatever the field can actually hold before assigning.
        private static void Assign(in HandleContext context, GameObject picked)
        {
            if (picked == null)
                return;

            Type fieldType = context.Field.FieldType;

            if (fieldType == typeof(GameObject))
            {
                context.Property.objectReferenceValue = picked;
                return;
            }

            if (typeof(Component).IsAssignableFrom(fieldType) || fieldType.IsInterface)
            {
                Object component = picked.GetComponent(fieldType);
                if (component != null)
                    context.Property.objectReferenceValue = component;
            }
        }
    }
}