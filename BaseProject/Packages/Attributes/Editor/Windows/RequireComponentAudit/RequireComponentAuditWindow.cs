using System;
using System.Collections.Generic;
using System.Reflection;
using Base.AttributePackage.Editor.Core;
using Base.EditorUiPackage;
using Base.UtilityPackage.Menus;
using UnityEditor;
using UnityEngine;

namespace Base.AttributePackage.Editor.Windows.RequireComponentAudit
{
    /// <summary>
    /// Audits <see cref="GetComponentAttribute"/> fields and lists the ones whose class is missing a
    /// matching <see cref="RequireComponent"/>. Each row opens the offending script.
    /// <see cref="GetComponentInParentAttribute"/> is ignored, since its target lives on a parent, not
    /// the same GameObject.
    /// </summary>
    internal sealed class RequireComponentAuditWindow : EditorWindow
    {
        private const string EmptyMessage = "Nothing to fix";
        private const string MenuPath = "Tools/Base Packages/Unity Editor/References/GetComponent Require Audit";
        private const float MinimumHeight = 200f;
        private const float MinimumWidth = 360f;
        private const float OpenButtonWidth = 60f;
        private const string OpenLabel = "Open";
        private const float RescanButtonWidth = 80f;
        private const string RescanLabel = "Rescan";
        private const string WindowTitle = "GetComponent Audit";
        private static readonly string GetComponentLabel = AttributeNames.Display<GetComponentAttribute>();
        private static readonly string RequireComponentLabel = AttributeNames.Display<RequireComponent>();
        private static readonly string Description =
            $"Lists every [{GetComponentLabel}] field whose class has no matching "
            + $"[{RequireComponentLabel}], so the component it looks for can go missing at runtime. "
            + $"[{AttributeNames.Display<GetComponentInParentAttribute>()}] is left out, because its "
            + "target lives on a parent rather than the same GameObject.";

        // Declared after the two labels it reads, because static field initializers run in the order
        // they are written, and built once rather than per repaint.







        [SerializeField] private Vector2 scrollPosition;

        private readonly List<FieldInfo> _missing = new();
        private readonly EditorWindowStyles _styles = new();

#region Unity Callbacks
        private void OnEnable()
        {
            titleContent = new GUIContent(WindowTitle);
            Rescan();
        }

        private void OnGUI()
        {
            _styles.EnsureBuilt();

            EditorWindowChrome.DrawHeader(_styles, WindowTitle, Description);

            DrawActionBar();

            if (_missing.Count == 0)
            {
                EditorWindowChrome.DrawEmptyState(_styles, EditorIcons.Success, EmptyMessage,
                    $"Every [{GetComponentLabel}] field has a matching [{RequireComponentLabel}].");

                return;
            }

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            EditorWindowChrome.BeginCard(_styles);

            for (int i = 0; i < _missing.Count; i++)
                DrawRow(_missing[i], i);

            EditorWindowChrome.EndCard();

            EditorGUILayout.EndScrollView();

            EditorWindowChrome.DrawFooter(_styles,
                $"{_missing.Count} field(s) missing a [{RequireComponentLabel}].");
        }

        private void OnDisable() => _styles.Dispose();
#endregion

        [DynamicMenuItem(MenuPath)]
        private static void Open()
        {
            RequireComponentAuditWindow window = GetWindow<RequireComponentAuditWindow>();

            window.minSize = new Vector2(MinimumWidth, MinimumHeight);
            window.Show();
        }

        private static bool IsMissing(FieldInfo field)
        {
            Type declaringType = field.DeclaringType;
            Type fieldType = field.FieldType;

            if (declaringType == null
                || !typeof(Component).IsAssignableFrom(fieldType))
                return false;

            IEnumerable<RequireComponent> requirements =
                declaringType.GetCustomAttributes<RequireComponent>(inherit: true);

            foreach (RequireComponent attribute in requirements)
            {
                if (Satisfies(attribute, fieldType))
                    return false;
            }

            return true;
        }

        private static bool Satisfies(RequireComponent attribute, Type fieldType)
            => IsMatch(attribute.m_Type0, fieldType)
                || IsMatch(attribute.m_Type1, fieldType)
                || IsMatch(attribute.m_Type2, fieldType);

        private static bool IsMatch(Type required, Type fieldType) => required != null
            && fieldType.IsAssignableFrom(required);

        private static int CompareFields(FieldInfo left, FieldInfo right)
        {
            int byType = string.CompareOrdinal(left.DeclaringType?.FullName, right.DeclaringType?.FullName);

            return byType != 0
                ? byType
                : string.CompareOrdinal(left.Name, right.Name);
        }

        private static void OpenScript(Type type)
        {
            MonoScript script = FindScript(type);

            if (script != null)
                AssetDatabase.OpenAsset(script);
        }

        private static MonoScript FindScript(Type type)
        {
            foreach (string guid in AssetDatabase.FindAssets($"t:MonoScript {type.Name}"))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                MonoScript script = AssetDatabase.LoadAssetAtPath<MonoScript>(path);

                if (script != null && script.GetClass() == type)
                    return script;
            }

            return null;
        }

        private void DrawRow(FieldInfo field, int index)
        {
            Type declaring = field.DeclaringType;

            if (declaring == null)
                return;

            Type fieldType = field.FieldType;

            Rect row = EditorGUILayout.BeginHorizontal(GUILayout.Height(EditorTableStyles.RowHeight));

            EditorRows.DrawRowBackground(row, index);

            GUILayout.Label($"{declaring.Name}.{field.Name}", _styles.NameBold);
            GUILayout.Label($"needs [{RequireComponentLabel}(typeof({fieldType.Name}))]", _styles.Detail);

            GUILayout.FlexibleSpace();

            if (EditorWindowChrome.SecondaryButton(_styles, OpenLabel, GUILayout.Width(OpenButtonWidth)))
                OpenScript(declaring);

            EditorGUILayout.EndHorizontal();
        }

        private void Rescan()
        {
            _missing.Clear();

            foreach (FieldInfo field in TypeCache.GetFieldsWithAttribute<GetComponentAttribute>())
            {
                if (IsMissing(field))
                    _missing.Add(field);
            }

            _missing.Sort(CompareFields);
        }

        private void DrawActionBar()
        {
            EditorGUILayout.BeginHorizontal();

            if (EditorWindowChrome.SecondaryButton(_styles, RescanLabel, GUILayout.Width(RescanButtonWidth)))
                Rescan();

            GUILayout.FlexibleSpace();

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(EditorMetrics.ItemGap);
        }
    }
}