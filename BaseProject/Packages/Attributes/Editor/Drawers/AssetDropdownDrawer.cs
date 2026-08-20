using System;
using System.Collections.Generic;
using Base.UtilityPackage.Editor;
using Base.UtilityPackage.Editor.Dropdown;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Base.AttributePackage.Editor
{
    /// <summary>
    /// Draws a searchable dropdown of matching project assets for <see cref="AssetDropdownAttribute"/>.
    /// </summary>
    /// <remarks>
    /// Results are cached per filter and dropped whenever the project changes, because an asset database
    /// search on every repaint is not affordable and the answer only moves when an asset does.
    /// </remarks>
    [CustomPropertyDrawer(typeof(AssetDropdownAttribute))]
    internal sealed class AssetDropdownDrawer : WarningFieldDrawer
    {
        private const string ClearLabel = "None";
        private const string NoMatchMessage = "No asset matched the filter.";
        private const string TypeFilterFormat = "t:{0}";

        protected override string UsageMessage => AttributeNames.Usage<AssetDropdownAttribute>("an object reference");

        private static readonly Dictionary<string, Object[]> Cache = new();

        // Kept alive between openings so the dropdown remembers its scroll position and search text.
        private readonly AdvancedDropdownState _state = new();

        private Object[] _assets;

        static AssetDropdownDrawer()
        {
            EditorApplication.projectChanged += Cache.Clear;
            AssemblyReloadEvents.beforeAssemblyReload += Cache.Clear;
        }

        protected override bool IsSupported(SerializedProperty property)
            => property.propertyType == SerializedPropertyType.ObjectReference;

        protected override string Evaluate(SerializedProperty property)
        {
            _assets = Collect((AssetDropdownAttribute)attribute, fieldInfo?.FieldType);

            return _assets.Length > 0
                ? null
                : NoMatchMessage;
        }

        protected override void DrawField(Rect rect, SerializedProperty property, GUIContent label, bool complete)
        {
            if (!complete)
            {
                EditorGUI.PropertyField(rect, property, label);
                return;
            }

            Rect buttonRect = LabeledField.Prefix(rect, label);

            Object current = property.objectReferenceValue;
            string caption = current == null
                ? ClearLabel
                : current.name;

            if (!EditorGUI.DropdownButton(buttonRect, ScratchContent.For(caption), FocusType.Keyboard))
                return;

            Show(buttonRect, property, label.text);
        }

        // The label is the asset path below its folder, so assets sharing a name stay distinguishable
        // and the dropdown groups them by where they live.
        private static string LabelFor(Object asset)
        {
            string path = AssetDatabase.GetAssetPath(asset);

            if (string.IsNullOrEmpty(path))
                return asset.name;

            int assets = path.IndexOf('/');

            return assets < 0
                ? path
                : path[(assets + 1)..];
        }

        private static Object[] Collect(AssetDropdownAttribute settings, Type fieldType)
        {
            string filter = settings.Filter ?? BuildFilter(fieldType);
            string key = filter + "|" + string.Join("|", settings.SearchInFolders ?? Array.Empty<string>());

            if (Cache.TryGetValue(key, out Object[] cached))
                return cached;

            List<Object> found = new();

            foreach (string guid in AssetDatabase.FindAssets(filter, settings.SearchInFolders))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                Object asset = fieldType == null
                    ? AssetDatabase.LoadMainAssetAtPath(path)
                    : AssetDatabase.LoadAssetAtPath(path, fieldType);

                if (asset != null)
                    found.Add(asset);
            }

            found.Sort(comparison: (a, b) => string.CompareOrdinal(LabelFor(a), LabelFor(b)));

            Object[] result = found.ToArray();
            Cache[key] = result;
            return result;
        }

        private static string BuildFilter(Type fieldType) => fieldType == null
            ? string.Empty
            : string.Format(TypeFilterFormat, fieldType.Name);

        private void Show(Rect rect, SerializedProperty property, string title)
        {
            List<string> labels = new(_assets.Length + 1)
            {
                ClearLabel
            };

            foreach (Object asset in _assets)
                labels.Add(LabelFor(asset));

            // The property is captured for the callback, which runs after this OnGUI call has returned.
            SerializedProperty captured = property.Copy();
            Object[] assets = _assets;

            SearchableDropdown menu = new(_state, title, labels, onSelected: index =>
            {
                captured.objectReferenceValue = index <= 0
                    ? null
                    : assets[index - 1];

                captured.serializedObject.ApplyModifiedProperties();
            });

            menu.Show(rect);
        }
    }
}