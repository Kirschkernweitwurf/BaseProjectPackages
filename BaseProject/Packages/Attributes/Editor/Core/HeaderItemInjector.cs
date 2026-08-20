using System;
using System.Collections;
using System.Reflection;
using Base.AttributePackage.Editor.Core;
using UnityEditor;
using UnityEditorInternal;

namespace Base.AttributePackage.Editor.Drawers
{
    /// <summary>
    /// Registers <see cref="HeaderItemRenderer"/> with Unity's component header so header controls can
    /// be drawn at all.
    /// </summary>
    /// <remarks>
    /// Overriding <c>Editor.OnHeaderGUI</c> does not work here. Unity only calls it for the editor that
    /// owns the whole inspector, which is the GameObject or the asset. The title bar of each component
    /// is drawn by the inspector itself, and the only extension point it offers is the internal list
    /// behind <c>[EditorHeaderItem]</c>. Appending to that list through reflection is what every tool
    /// that puts controls in a component header ends up doing.
    /// <para>
    /// Unity builds the list lazily, the first time any header is drawn, and skips rebuilding it once it
    /// exists. Injecting before that point would replace it and take Unity's own header items with it,
    /// so this waits for the list to appear instead of forcing it into being. If the field is gone
    /// because the internal API changed, the hook quietly gives up and header controls stop appearing;
    /// nothing else in the package depends on it.
    /// </para>
    /// </remarks>
    [InitializeOnLoad]
    internal static class HeaderItemInjector
    {
        private const BindingFlags FieldFlags = BindingFlags.NonPublic | BindingFlags.Static;
        private const string HeaderItemsField = "s_EditorHeaderItemsMethods";

        private static readonly FieldInfo _headerItems;

        static HeaderItemInjector()
        {
            _headerItems = typeof(EditorGUIUtility).GetField(HeaderItemsField, FieldFlags);

            if (_headerItems == null)
                return;

            EditorApplication.update += TryInject;
        }

        private static void TryInject()
        {
            // Null means Unity has not drawn a header yet. Keep waiting rather than creating the list,
            // which would suppress Unity's own header items.
            if (_headerItems.GetValue(null) is not IList items)
                return;

            EditorApplication.update -= TryInject;

            MethodInfo drawMethod = typeof(HeaderItemRenderer).GetMethod(HeaderItemRenderer.DrawMethodName, FieldFlags);

            Type delegateType = items.GetType().GetGenericArguments()[0];

            if (drawMethod == null)
                return;

            items.Add(Delegate.CreateDelegate(delegateType, drawMethod));

            // Headers already on screen were laid out before the hook existed and would stay empty.
            InternalEditorUtility.RepaintAllViews();
        }
    }
}