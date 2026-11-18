using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.UI;

namespace Base.ControllerSupportPackage.Controller.Focus
{
    /// <summary>
    /// Tells whether the player types into a text field, like the cheat console or a player name, so keys
    /// that double as shortcuts, like WASD or T, type instead of acting.
    /// </summary>
    public static class TextEntry
    {
        /// <summary>Whether the selected element is a text field being typed in.</summary>
        public static bool IsTyping()
        {
            EventSystem eventSystem = EventSystem.current;
            if (eventSystem == null)
                return false;

            GameObject selected = eventSystem.currentSelectedGameObject;
            return selected != null
                && selected.TryGetComponent(out TMP_InputField field)
                && field.isFocused;
        }

        /// <summary>
        /// Whether <paramref name="control"/> types a character while a text field is being typed in: a letter, a
        /// digit or space. Arrows, Enter and Esc keep working, so the player can still leave the field.
        /// </summary>
        public static bool IsTyped(InputControl control) => control is KeyControl key
            && (key.keyCode is >= Key.A and <= Key.Digit0
                || key.keyCode == Key.Space)
            && IsTyping();

        /// <summary>
        /// Whether the UI's current move comes from a key that types into the focused text field, like the W of a
        /// name. Move events don't tell their key, so this reads it from the UI module's move action.
        /// </summary>
        public static bool IsMoveTyped()
        {
            EventSystem eventSystem = EventSystem.current;
            if (eventSystem == null
                || !(eventSystem.currentInputModule is InputSystemUIInputModule module)
                || module.move == null
                || module.move.action == null)
                return false;

            return IsTyped(module.move.action.activeControl);
        }
    }
}