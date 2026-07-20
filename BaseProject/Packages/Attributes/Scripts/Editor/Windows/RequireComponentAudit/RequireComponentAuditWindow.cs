using System;
using System.Collections.Generic;
using System.Reflection;
using Base.ToolPackage.MenuManagerWindow;
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
    public sealed class RequireComponentAuditWindow : EditorWindow
    {
        private const string MenuPath = "Tools/Base Packages/Unity Editor/References/GetComponent Require Audit";
        private const float RowHeight = 22f;
        private const string WindowTitle = "GetComponent Audit";

        private static readonly string GetComponentLabel = AttributeNames.Display<GetComponentAttribute>();
        private static readonly string RequireComponentLabel = AttributeNames.Display<RequireComponent>();

        [SerializeField] private Vector2 scrollPosition;

        private readonly List<FieldInfo> _missing = new();

#region Unity Callbacks
        private void OnEnable()
        {
            titleContent = new GUIContent(WindowTitle);
            Rescan();
        }

        private void OnGUI()
        {
            DrawActionBar();

            if (_missing.Count == 0)
            {
                EditorGUILayout.HelpBox($"Every [{GetComponentLabel}] field has a matching [{RequireComponentLabel}].",
                    MessageType.Info);

                return;
            }

            EditorGUILayout.HelpBox($"{_missing.Count} field(s) missing a [{RequireComponentLabel}].",
                MessageType.Warning);

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            foreach (FieldInfo field in _missing)
                DrawRow(field);

            EditorGUILayout.EndScrollView();
        }
#endregion

        [DynamicMenuItem(MenuPath)]
        private static void Open()
        {
            RequireComponentAuditWindow window = GetWindow<RequireComponentAuditWindow>();

            window.minSize = new Vector2(360f, 200f);
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

        private static void DrawRow(FieldInfo field)
        {
            Type declaring = field.DeclaringType;

            if (declaring == null)
                return;

            Type fieldType = field.FieldType;

            EditorGUILayout.BeginHorizontal(GUILayout.Height(RowHeight));

            EditorGUILayout.LabelField(
                $"{declaring.Name}.{field.Name}  needs  [{RequireComponentLabel}(typeof({fieldType.Name}))]");

            if (GUILayout.Button("Open", GUILayout.Width(60f)))
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
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            if (GUILayout.Button("Rescan", EditorStyles.toolbarButton, GUILayout.Width(80f)))
                Rescan();

            GUILayout.FlexibleSpace();

            EditorGUILayout.EndHorizontal();
        }
    }
}