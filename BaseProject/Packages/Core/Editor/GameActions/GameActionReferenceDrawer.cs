using System.Collections.Generic;
using Base.CorePackage.GameActions;
using UnityEditor;
using UnityEngine;

namespace Base.CorePackage.Editor.GameActions
{
    /// <summary>
    /// Draws a <see cref="GameActionReference"/> as a dropdown populated from
    /// <see cref="GameActionRegistry"/>, so a designer picks the action by id. An id that no
    /// longer resolves is shown as a distinct missing entry instead of silently reading as none.
    /// </summary>
    [CustomPropertyDrawer(typeof(GameActionReference))]
    public sealed class GameActionReferenceDrawer : PropertyDrawer
    {
        private const int ActionIndexOffset = 1;
        private const string IdFieldName = "id";
        private const string MissingLabelFormat = "Missing: {0}";
        private const int NoneIndex = 0;
        private const string NoneLabel = "None";

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty idProperty = property.FindPropertyRelative(IdFieldName);

            if (idProperty == null)
            {
                EditorGUI.PropertyField(position, property, label);
                return;
            }

            EditorGUI.BeginProperty(position, label, property);

            IReadOnlyList<GameActionInfo> actions = GameActionRegistry.All;
            string storedId = idProperty.stringValue;

            List<string> labels = new(actions.Count + ActionIndexOffset)
            {
                NoneLabel
            };

            int selected = NoneIndex;

            for (int index = 0; index < actions.Count; index++)
            {
                string id = actions[index].Id;
                labels.Add(id);

                if (id == storedId)
                    selected = index + ActionIndexOffset;
            }

            bool isMissing = selected == NoneIndex && !string.IsNullOrEmpty(storedId);
            int missingIndex = labels.Count;

            if (isMissing)
            {
                labels.Add(string.Format(MissingLabelFormat, storedId));
                selected = missingIndex;
            }

            Rect fieldRect = EditorGUI.PrefixLabel(position, label);
            int chosen = EditorGUI.Popup(fieldRect, selected, labels.ToArray());

            if (chosen != selected)
                idProperty.stringValue = ResolveChosenId(chosen, isMissing, missingIndex, storedId, actions);

            EditorGUI.EndProperty();
        }

        private static string ResolveChosenId(int chosen, bool isMissing, int missingIndex, string storedId,
            IReadOnlyList<GameActionInfo> actions)
        {
            if (chosen == NoneIndex)
                return string.Empty;

            if (isMissing
                && chosen == missingIndex)
                return storedId;

            return actions[chosen - ActionIndexOffset].Id;
        }
    }
}