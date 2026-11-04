using System;
using System.Collections.Generic;
using Base.EditorUIPackage.Editor;
using Base.ToolsPackage.AssetZoo;
using Base.ToolsPackage.Editor.AssetZoo.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Base.ToolsPackage.Editor.AssetZoo.Overlay
{
    /// <summary>
    /// Draws the zoo's labels over the Scene view: zones and names like a map when far away, asset names as they get
    /// close, the numbers on hover. Drawn on screen instead of as text objects in the world, so they stay sharp at any
    /// distance, never overlap, always face the viewer, and leave the art alone in the Game view and in screenshots.
    /// </summary>
    /// <remarks>
    /// This runs in every project that installs the package, and almost none of them hold a zoo, so the hook is only
    /// attached while there is one. It follows the Gizmos button of the Scene view, the switch artists already use
    /// for a clean look.
    /// </remarks>
    [InitializeOnLoad]
    internal static class ZooSceneOverlay
    {
        private const float DotGap = 5f;
        private const float DotSize = 7f;
        private const string GroupCountFormat = "{0}";
        private const float GroupCountGap = 6f;
        private const float MinItemPixels = 18f;
        private const float MinOccluderHeight = 2f;
        private const float MinRulerPixelsPerMeter = 40f;
        private const string NewPrefix = "New  ";
        private const float PillGapBelowAsset = 6f;
        private const float PillPaddingX = 8f;
        private const float PillPaddingY = 3f;
        private const float PillRadius = 8f;
        private const float RulerHeight = 2f;
        private const string RulerLabelFormat = "{0} m";
        private const float RulerLabelGap = 10f;
        private const float RulerMajorTick = 0.14f;
        private const float RulerMinorTick = 0.07f;
        private const float RulerOffset = 0.4f;
        private const float RulerStep = 0.5f;
        private const float RulerThickness = 2f;
        private const float WireThickness = 2f;
        private const float ZoneAlpha = 0.07f;
        private const float ZoneHeight = 0.012f;
        private const float ZoneLineAlpha = 0.65f;
        private const float ZonePadding = 0.6f;

        // The labels sit on top of 3D art of any color, so they carry their own dark plate in both editor themes.
        private static readonly Color DetailColor = new(0.78f, 0.8f, 0.85f, 1f);
        private static readonly Color PillColor = new(0.08f, 0.085f, 0.1f, 0.88f);
        private static readonly Color TextColor = new(0.93f, 0.94f, 0.98f, 1f);

        private static readonly int ControlHash = nameof(ZooSceneOverlay).GetHashCode();
        private static readonly Vector3[] Corners = new Vector3[4];
        private static readonly Dictionary<ZooItem, string> Details = new();
        private static readonly List<Bounds> Occluders = new();
        private static readonly List<Pill> Pills = new();
        private static readonly List<Rect> Placed = new();
        private static readonly GUIContent SharedContent = new();
        private static readonly Dictionary<string, float> Widths = new();
        private static readonly ZooWindowStyles Styles = new();

        private static readonly Comparison<Pill> GroupsThenNearest = (first, second) =>
        {
            if (first.Group != null != (second.Group != null))
                return first.Group != null
                    ? -1
                    : 1;

            return first.Depth.CompareTo(second.Depth);
        };

        private static UnityEngine.Object _hovered;
        private static bool _isAttached;

        static ZooSceneOverlay()
        {
            ZooSceneIndex.Invalidated += OnIndexInvalidated;
            Attach();
        }

        private static void OnIndexInvalidated()
        {
            Details.Clear();
            Attach();
        }

        // Asked from more than one place, and subscribing twice would draw everything twice.
        private static void Attach()
        {
            if (_isAttached)
                return;

            SceneView.duringSceneGui += OnSceneGui;
            _isAttached = true;
        }

        private static void Detach()
        {
            if (!_isAttached)
                return;

            SceneView.duringSceneGui -= OnSceneGui;
            _isAttached = false;
        }

        private static void OnSceneGui(SceneView view)
        {
            ZooRootMarker root = ZooSceneIndex.Root;

            // Coming back empty is the normal case in a project without a zoo, and the hook goes with it. The
            // index brings it back the moment the hierarchy changes.
            if (root == null)
            {
                Detach();
                return;
            }

            int controlId = GUIUtility.GetControlID(ControlHash, FocusType.Passive);
            Event current = Event.current;

            if (!view.drawGizmos
                || !IsHandled(current.type))
                return;

            Styles.EnsureBuilt();

            CollectPills(view, root);
            UnityEngine.Object hovered = FindHovered(current.mousePosition);

            switch (current.type)
            {
                case EventType.Layout:
                    if (hovered != null)
                        HandleUtility.AddControl(controlId, 0f);

                    break;
                case EventType.MouseMove:
                    if (hovered != _hovered)
                    {
                        _hovered = hovered;
                        view.Repaint();
                    }

                    break;
                case EventType.MouseDown:
                    if (hovered != null
                        && current.button == 0
                        && !current.alt
                        && HandleUtility.nearestControl == controlId)
                    {
                        OnPillClicked(hovered, current.clickCount);
                        current.Use();
                    }

                    break;
                case EventType.MouseLeaveWindow:
                    _hovered = null;
                    view.Repaint();

                    break;
                case EventType.Repaint:
                    // The camera can move a label away from a mouse that stands still.
                    if (EditorWindow.mouseOverWindow == view)
                        _hovered = hovered;

                    Draw(view, root);

                    break;
            }
        }

        private static bool IsHandled(EventType type) => type == EventType.Layout
            || type == EventType.MouseMove
            || type == EventType.MouseDown
            || type == EventType.MouseLeaveWindow
            || type == EventType.Repaint;

        private static void OnPillClicked(UnityEngine.Object target, int clickCount)
        {
            if (target is ZooGroup group)
            {
                ZooSceneNavigator.FlyToGroup(group);
                return;
            }

            if (target is not ZooItem item)
                return;

            if (clickCount > 1)
            {
                AssetDatabase.OpenAsset(item.Asset);
                return;
            }

            Selection.activeObject = item.Content != null
                ? item.Content.gameObject
                : item.gameObject;

            EditorGUIUtility.PingObject(item.Asset);
        }

        private static void CollectPills(SceneView view, ZooRootMarker root)
        {
            Pills.Clear();
            Placed.Clear();

            if (root.HasGroupNames)
            {
                foreach (ZooGroup group in ZooSceneIndex.Groups)
                    AddGroupPill(view, group);
            }

            if (root.HasItemNames)
            {
                CollectOccluders();

                foreach (ZooItem item in ZooSceneIndex.Items)
                    AddItemPill(view, item, root.HasSizes);
            }

            Pills.Sort(GroupsThenNearest);

            // Groups first and then near to far, so what is lost to a collision is always the less useful one.
            int kept = 0;

            for (int i = 0; i < Pills.Count; i++)
            {
                Pill pill = Pills[i];

                if (IsOverlapping(pill.Rect))
                    continue;

                Placed.Add(pill.Rect);
                Pills[kept] = pill;
                kept++;
            }

            Pills.RemoveRange(kept, Pills.Count - kept);
        }

        // Only tall things can hide others from a view that looks down at the zoo, and there are few of them, so a
        // zoo of any size stays cheap to check on every repaint.
        private static void CollectOccluders()
        {
            Occluders.Clear();

            foreach (ZooItem item in ZooSceneIndex.Items)
            {
                if (item == null
                    || !item.gameObject.activeInHierarchy
                    || item.MeasuredBounds.size.y < MinOccluderHeight)
                    continue;

                Occluders.Add(item.WorldBounds);
            }
        }

        // A name floating on a wall, for something on the other side of it, only says that the wall has a name.
        private static bool IsBehindOccluder(SceneView view, ZooItem item, Vector3 anchor)
        {
            Vector3 camera = view.camera.transform.position;
            Vector3 towards = anchor - camera;
            float distance = towards.magnitude;
            Ray ray = new(camera, towards);
            Bounds own = item.WorldBounds;

            foreach (Bounds occluder in Occluders)
            {
                if (occluder.Intersects(own))
                    continue;

                if (occluder.IntersectRay(ray, out float hit)
                    && hit < distance)
                    return true;
            }

            return false;
        }

        private static bool IsOverlapping(Rect rect)
        {
            foreach (Rect other in Placed)
            {
                if (rect.Overlaps(other))
                    return true;
            }

            return false;
        }

        private static void AddGroupPill(SceneView view, ZooGroup group)
        {
            if (group == null
                || !group.gameObject.activeInHierarchy)
                return;

            Vector2 size = group.Size;
            Vector3 anchor = group.transform.TransformPoint(-size.x * 0.5f, 0f, ZonePadding);

            if (!TryProject(view, anchor, out Vector3 screen))
                return;

            float width = MeasureGroupText(group);
            float height = Styles.PillGroup.lineHeight + PillPaddingY * 2f;

            Rect rect = new(screen.x - width * 0.5f, screen.y + PillGapBelowAsset, width, height);

            Pills.Add(new Pill(rect, screen.z, group, null));
        }

        private static float MeasureGroupText(ZooGroup group)
        {
            float name = MeasureText(group.GroupName, Styles.PillGroup);
            float count = MeasureText(string.Format(GroupCountFormat, group.AssetCount), Styles.PillDetail);

            return DotSize + DotGap + name + GroupCountGap + count + PillPaddingX * 2f;
        }

        private static void AddItemPill(SceneView view, ZooItem item, bool hasSizes)
        {
            if (item == null
                || item.IsFigure
                || item.Asset == null
                || !item.gameObject.activeInHierarchy)
                return;

            Vector2 footprint = item.Footprint;
            Vector3 anchor = item.transform.TransformPoint(0f, 0f, footprint.y * 0.5f);

            if (!TryProject(view, anchor, out Vector3 screen))
                return;

            // Handle size is how long 80 pixels are at that spot, which makes this an on screen width. Things
            // too small to make out yet get no name yet, which is what keeps a far away zoo calm.
            float meterToPixels = 80f / HandleUtility.GetHandleSize(anchor);
            bool isHovered = _hovered == item;

            if (!isHovered
                && (Mathf.Max(footprint.x, item.MeasuredBounds.size.y) * meterToPixels < MinItemPixels
                    || IsBehindOccluder(view, item, anchor)))
                return;

            Rect rect = MeasureItemRect(item, screen, hasSizes || isHovered);

            Pills.Add(new Pill(rect, screen.z, null, item));
        }

        private static Rect MeasureItemRect(ZooItem item, Vector3 screen, bool hasDetail)
        {
            float width = MeasureText(GetTitle(item), Styles.PillName);
            float height = Styles.PillName.lineHeight + PillPaddingY * 2f;

            if (hasDetail)
            {
                width = Mathf.Max(width, MeasureText(GetDetail(item), Styles.PillDetail));
                height += Styles.PillDetail.lineHeight;
            }

            if (!string.IsNullOrEmpty(item.Problem))
            {
                width += DotSize + DotGap;

                if (hasDetail)
                {
                    width = Mathf.Max(width, MeasureText(item.Problem, Styles.PillDetail) + DotSize + DotGap);
                    height += Styles.PillDetail.lineHeight;
                }
            }

            width += PillPaddingX * 2f;

            return new Rect(screen.x - width * 0.5f, screen.y + PillGapBelowAsset, width, height);
        }

        private static string GetTitle(ZooItem item) => item.IsNew
            ? NewPrefix + item.Asset.name
            : item.Asset.name;

        private static string GetDetail(ZooItem item)
        {
            if (Details.TryGetValue(item, out string detail))
                return detail;

            detail = ZooItemText.Describe(item);
            Details[item] = detail;

            return detail;
        }

        private static float MeasureText(string text, GUIStyle style)
        {
            if (Widths.TryGetValue(text, out float width))
                return width;

            SharedContent.text = text;
            width = style.CalcSize(SharedContent).x;
            Widths[text] = width;

            return width;
        }

        private static bool TryProject(SceneView view, Vector3 world, out Vector3 screen)
        {
            screen = HandleUtility.WorldToGUIPointWithDepth(world);

            return screen.z > 0f
                && screen.x >= 0f
                && screen.x <= view.position.width
                && screen.y >= 0f
                && screen.y <= view.position.height;
        }

        private static UnityEngine.Object FindHovered(Vector2 mouse)
        {
            foreach (Pill pill in Pills)
            {
                if (pill.Rect.Contains(mouse))
                    return pill.Target;
            }

            return null;
        }

        private static void Draw(SceneView view, ZooRootMarker root)
        {
            CompareFunction previousTest = Handles.zTest;
            Color previousColor = Handles.color;
            Handles.zTest = CompareFunction.LessEqual;

            // Handles multiply their colors with this one, so a color left over from the last draw would tint the
            // zones.
            Handles.color = Color.white;

            if (root.HasGroupNames)
            {
                foreach (ZooGroup group in ZooSceneIndex.Groups)
                    DrawZone(group);
            }

            if (_hovered is ZooItem hoveredItem)
            {
                Bounds bounds = hoveredItem.WorldBounds;
                Handles.color = EditorPalette.Accent;
                Handles.zTest = CompareFunction.Always;
                DrawWireBox(bounds);
                Handles.zTest = CompareFunction.LessEqual;
            }

            if (root.HasRuler)
            {
                foreach (ZooGroup group in ZooSceneIndex.Groups)
                    DrawRulerLines(group);
            }

            Handles.zTest = previousTest;
            Handles.color = previousColor;

            Handles.BeginGUI();

            foreach (Pill pill in Pills)
                DrawPill(pill);

            if (root.HasRuler)
            {
                foreach (ZooGroup group in ZooSceneIndex.Groups)
                    DrawRulerLabels(view, group);
            }

            Handles.EndGUI();
        }

        private static void DrawZone(ZooGroup group)
        {
            if (group == null
                || !group.gameObject.activeInHierarchy)
                return;

            Vector2 size = group.Size;
            Transform transform = group.transform;

            Corners[0] = transform.TransformPoint(ZonePadding, ZoneHeight, ZonePadding);
            Corners[1] = transform.TransformPoint(-size.x - ZonePadding, ZoneHeight, ZonePadding);
            Corners[2] = transform.TransformPoint(-size.x - ZonePadding, ZoneHeight, -size.y - ZonePadding);
            Corners[3] = transform.TransformPoint(ZonePadding, ZoneHeight, -size.y - ZonePadding);

            Handles.DrawSolidRectangleWithOutline(Corners, EditorPalette.WithAlpha(group.Color, ZoneAlpha),
                EditorPalette.WithAlpha(group.Color, ZoneLineAlpha));
        }

        private static void DrawWireBox(Bounds bounds)
        {
            Vector3 min = bounds.min;
            Vector3 max = bounds.max;

            Vector3 a = new(min.x, min.y, min.z);
            Vector3 b = new(max.x, min.y, min.z);
            Vector3 c = new(max.x, min.y, max.z);
            Vector3 d = new(min.x, min.y, max.z);
            Vector3 e = new(min.x, max.y, min.z);
            Vector3 f = new(max.x, max.y, min.z);
            Vector3 g = new(max.x, max.y, max.z);
            Vector3 h = new(min.x, max.y, max.z);

            Handles.DrawLine(a, b, WireThickness);
            Handles.DrawLine(b, c, WireThickness);
            Handles.DrawLine(c, d, WireThickness);
            Handles.DrawLine(d, a, WireThickness);
            Handles.DrawLine(e, f, WireThickness);
            Handles.DrawLine(f, g, WireThickness);
            Handles.DrawLine(g, h, WireThickness);
            Handles.DrawLine(h, e, WireThickness);
            Handles.DrawLine(a, e, WireThickness);
            Handles.DrawLine(b, f, WireThickness);
            Handles.DrawLine(c, g, WireThickness);
            Handles.DrawLine(d, h, WireThickness);
        }

        private static Vector3 GetRulerBase(ZooGroup group)
            => group.transform.TransformPoint(RulerOffset, 0f, RulerOffset);

        private static void DrawRulerLines(ZooGroup group)
        {
            if (group == null
                || !group.gameObject.activeInHierarchy)
                return;

            Vector3 start = GetRulerBase(group);
            Vector3 up = group.transform.up;
            Vector3 right = group.transform.right;

            Handles.color = TextColor;
            Handles.DrawLine(start, start + up * RulerHeight, RulerThickness);

            for (float height = 0f; height <= RulerHeight + Mathf.Epsilon; height += RulerStep)
            {
                bool isWholeMeter = Mathf.Approximately(height % 1f, 0f);

                float tick = isWholeMeter
                    ? RulerMajorTick
                    : RulerMinorTick;

                Vector3 point = start + up * height;
                Handles.DrawLine(point - right * tick, point + right * tick, RulerThickness);
            }
        }

        private static void DrawRulerLabels(SceneView view, ZooGroup group)
        {
            if (group == null
                || !group.gameObject.activeInHierarchy)
                return;

            Vector3 start = GetRulerBase(group);

            // Only once a meter is big enough on screen to read against. Further out it is just noise.
            if (80f / HandleUtility.GetHandleSize(start) < MinRulerPixelsPerMeter)
                return;

            Vector3 up = group.transform.up;

            for (int meter = 1; meter <= RulerHeight; meter++)
            {
                if (!TryProject(view, start + up * meter, out Vector3 screen))
                    continue;

                string text = string.Format(RulerLabelFormat, meter);
                float width = MeasureText(text, Styles.PillDetail) + PillPaddingX;
                float height = Styles.PillDetail.lineHeight + PillPaddingY;

                Rect rect = new(screen.x - width - RulerLabelGap, screen.y - height * 0.5f, width, height);

                DrawPlate(rect, PillColor);
                DrawText(rect, text, Styles.PillDetail, DetailColor, TextAnchor.MiddleCenter);
            }
        }

        private static void DrawPill(in Pill pill)
        {
            if (pill.Group != null)
            {
                DrawGroupPill(pill);
                return;
            }

            ZooItem item = pill.Item;

            if (item == null)
                return;

            bool isHovered = _hovered == item;
            ZooRootMarker root = ZooSceneIndex.Root;
            bool hasDetail = isHovered || root.HasSizes;

            Color fill = isHovered
                ? EditorPalette.Accent
                : PillColor;

            Color text = isHovered
                ? EditorPalette.AccentText
                : TextColor;

            DrawPlate(pill.Rect, fill);

            Rect inner = new(pill.Rect.x + PillPaddingX, pill.Rect.y + PillPaddingY,
                pill.Rect.width - PillPaddingX * 2f, pill.Rect.height - PillPaddingY * 2f);

            if (!string.IsNullOrEmpty(item.Problem))
            {
                DrawDot(new Vector2(inner.x + DotSize * 0.5f, inner.y + Styles.PillName.lineHeight * 0.5f),
                    EditorPalette.Warning);

                inner.xMin += DotSize + DotGap;
            }

            Rect line = new(inner.x, inner.y, inner.width, Styles.PillName.lineHeight);
            DrawText(line, GetTitle(item), Styles.PillName, item.IsNew && !isHovered ? EditorPalette.Accent : text,
                TextAnchor.MiddleLeft);

            if (!hasDetail)
                return;

            Color detail = isHovered
                ? EditorPalette.AccentText
                : DetailColor;

            line.y += line.height;
            line.height = Styles.PillDetail.lineHeight;
            DrawText(line, GetDetail(item), Styles.PillDetail, detail, TextAnchor.MiddleLeft);

            if (string.IsNullOrEmpty(item.Problem))
                return;

            line.y += line.height;
            DrawText(line, item.Problem, Styles.PillDetail, detail, TextAnchor.MiddleLeft);
        }

        private static void DrawGroupPill(in Pill pill)
        {
            ZooGroup group = pill.Group;
            bool isHovered = _hovered == group;

            DrawPlate(pill.Rect, isHovered
                ? EditorPalette.Accent
                : PillColor);

            float x = pill.Rect.x + PillPaddingX;
            float y = pill.Rect.y + PillPaddingY;
            float lineHeight = Styles.PillGroup.lineHeight;

            DrawDot(new Vector2(x + DotSize * 0.5f, y + lineHeight * 0.5f), group.Color);
            x += DotSize + DotGap;

            float nameWidth = MeasureText(group.GroupName, Styles.PillGroup);

            Color name = isHovered
                ? EditorPalette.AccentText
                : TextColor;

            DrawText(new Rect(x, y, nameWidth, lineHeight), group.GroupName, Styles.PillGroup, name,
                TextAnchor.MiddleLeft);

            x += nameWidth + GroupCountGap;

            string count = string.Format(GroupCountFormat, group.AssetCount);

            Color countColor = isHovered
                ? EditorPalette.AccentText
                : DetailColor;

            DrawText(new Rect(x, y, MeasureText(count, Styles.PillDetail), lineHeight), count, Styles.PillDetail,
                countColor, TextAnchor.MiddleLeft);
        }

        private static void DrawPlate(Rect rect, Color color) => GUI.DrawTexture(rect, Texture2D.whiteTexture,
            ScaleMode.StretchToFill, true, 0f, color, 0f, PillRadius);

        private static void DrawDot(Vector2 center, Color color)
        {
            Rect dot = new(center.x - DotSize * 0.5f, center.y - DotSize * 0.5f, DotSize, DotSize);

            GUI.DrawTexture(dot, Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 0f, color, 0f,
                DotSize * 0.5f);
        }

        private static void DrawText(Rect rect, string text, GUIStyle style, Color color, TextAnchor anchor)
        {
            Color previous = GUI.contentColor;

            GUI.contentColor = color;
            style.alignment = anchor;
            GUI.Label(rect, text, style);
            GUI.contentColor = previous;
        }

        private readonly struct Pill
        {
            /// <summary>Where the label sits on screen.</summary>
            public Rect Rect { get; }

            /// <summary>How far away its anchor is.</summary>
            public float Depth { get; }

            /// <summary>The group it names, or null.</summary>
            public ZooGroup Group { get; }

            /// <summary>The asset it names, or null.</summary>
            public ZooItem Item { get; }

            /// <summary>What the label stands for.</summary>
            public UnityEngine.Object Target => Group != null
                ? Group
                : Item;

            /// <summary>Records one label to draw.</summary>
            /// <param name="rect">Where it sits on screen.</param>
            /// <param name="depth">How far away its anchor is.</param>
            /// <param name="group">The group it names, or null.</param>
            /// <param name="item">The asset it names, or null.</param>
            public Pill(Rect rect, float depth, ZooGroup group, ZooItem item)
            {
                Rect = rect;
                Depth = depth;
                Group = group;
                Item = item;
            }
        }
    }
}