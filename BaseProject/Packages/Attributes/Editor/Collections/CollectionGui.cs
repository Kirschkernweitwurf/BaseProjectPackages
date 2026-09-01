using UnityEditor;

namespace Base.AttributePackage.Editor.Collections
{
    /// <summary>
    /// Shared metrics, labels and small controls for the list and table renderers, so both look and
    /// behave the same and neither carries a raw number of its own.
    /// </summary>
    internal static class CollectionGui
    {
        /// <summary>Cancel label of the delete confirmation dialog.</summary>
        internal const string ConfirmCancel = "Cancel";

        /// <summary>Accept label of the delete confirmation dialog.</summary>
        internal const string ConfirmDelete = "Delete";

        /// <summary>
        /// Removes an element, working around the two-step delete Unity does on object references.
        /// </summary>
        /// <remarks>
        /// The first delete on a populated object reference clears the reference and leaves the row, so
        /// a second one is needed to remove the row itself. On every other kind of element the first
        /// delete is enough, which is why the size is checked rather than the type.
        /// </remarks>
        /// <param name="array">The array to remove from.</param>
        /// <param name="index">Index of the element to remove.</param>
        internal static void DeleteElement(SerializedProperty array, int index)
        {
            SerializedProperty element = array.GetArrayElementAtIndex(index);

            if (element.propertyType == SerializedPropertyType.ObjectReference
                && element.objectReferenceValue != null)
                element.objectReferenceValue = null;

            int size = array.arraySize;
            array.DeleteArrayElementAtIndex(index);

            if (array.arraySize == size)
                array.DeleteArrayElementAtIndex(index);
        }

        /// <summary>Asks before removing a row, when the caller wants a confirmation.</summary>
        /// <param name="label">What is being removed, shown in the dialog.</param>
        /// <param name="required">Whether a confirmation is wanted at all.</param>
        /// <returns>True when the removal should go ahead.</returns>
        internal static bool ConfirmRemoval(string label, bool required)
        {
            if (!required)
                return true;

            return EditorUtility.DisplayDialog(ConfirmDelete, $"Remove {label}?", ConfirmDelete, ConfirmCancel);
        }
    }
}