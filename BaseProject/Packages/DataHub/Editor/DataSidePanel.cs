using System;
using System.IO;
using Base.DataHubPackage.Editor.Bookmarks;
using Base.DataHubPackage.Editor.Lists;
using Base.DataHubPackage.Editor.Preview;
using Base.DataHubPackage.Editor.Table;
using Base.EditorUIPackage.Editor;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Base.DataHubPackage.Editor
{
    /// <summary>
    /// The right panel of the Data Hub: which assets are selected, the tabs, and the inspector, contents
    /// or usage of the selection under them.
    /// </summary>
    internal sealed class DataSidePanel
    {
        private const string ContentsTabLabel = "Contents";
        private const string ContentsTabTooltip = "What the selected asset holds, like the cards in a deck. Add, "
            + "remove and count them here.";
        private const string InspectorTabLabel = "Inspector";
        private const string InspectorTabTooltip = "Unity's full inspector, for everything the table can't edit.";
        private const string ManyFormat = "{0} assets";
        private const string ManyTooltip = "Changes here go to all of them at once.";
        private const string MixedTypesText = "Different types";
        private const string NothingSelectedHint = "Pick a row in the table";
        private const string NothingSelectedTitle = "Nothing selected";
        private const string PingTooltip = "Click to find it in the Project window.";
        private const string PinnedTooltip = "Pinned to the sidebar. Click to unpin it.";
        private const string PinTooltip = "Pin it to the sidebar, to come back to it in one click.";
        private const string PreviewTabLabel = "Preview";
        private const string PreviewTabTooltip = "How the selected asset looks, and what its checks found.";
        private const string UsedInTabLabel = "Used In";
        private const string UsedInTabTooltip = "Where the selected assets are used, and where else they could go.";

        // The order the tabs are shown in, which differs from the saved values so older windows keep their tab.
        private static readonly EDataPanelTab[] TabOrder =
        {
            EDataPanelTab.Preview,
            EDataPanelTab.Inspector,
            EDataPanelTab.Contents,
            EDataPanelTab.UsedIn
        };

        private static readonly GUIContent[] TabContents =
        {
            new(PreviewTabLabel, PreviewTabTooltip),
            new(InspectorTabLabel, InspectorTabTooltip),
            new(ContentsTabLabel, ContentsTabTooltip),
            new(UsedInTabLabel, UsedInTabTooltip)
        };

        private readonly DataContentsPanel _contents;
        private readonly DataInspectorPanel _inspector = new();
        private readonly DataPreviewPanel _preview;
        private readonly DataUsagePanel _usage = new();

        /// <summary>Creates the panel.</summary>
        /// <param name="registry">The extensions, for the summaries of the Contents tab.</param>
        internal DataSidePanel(DataHubRegistry registry)
        {
            _contents = new DataContentsPanel(registry);
            _preview = new DataPreviewPanel(registry);
        }

        /// <summary>Forgets every list found for the Used In tab, after the project changed.</summary>
        internal void Invalidate() => _usage.Invalidate();

        /// <summary>Releases the inspector and every cached serialized object.</summary>
        internal void Dispose()
        {
            _inspector.Dispose();
            _preview.Dispose();
            _contents.Dispose();
            _usage.Dispose();
        }

        /// <summary>Draws the panel.</summary>
        /// <param name="area">The area to fill.</param>
        /// <param name="targets">The selected assets.</param>
        /// <param name="shownTab">The tab drawn this pass, fixed since the last layout.</param>
        /// <param name="tab">The tab the user picked, which takes effect on the next layout.</param>
        /// <param name="index">The assets of the project, for the Used In tab.</param>
        /// <param name="row">The table row of the first selected asset, for the checks in the Preview tab.</param>
        /// <param name="pins">The pinned assets, for the pin button.</param>
        /// <param name="styles">The built window styles.</param>
        /// <returns>True when a value was changed through the panel.</returns>
        internal bool Draw(Rect area, Object[] targets, EDataPanelTab shownTab, ref EDataPanelTab tab,
            DataAssetIndex index, DataRow row, DataPins pins, DataHubStyles styles)
        {
            DataHubGui.Fill(area, DataHubStyles.SidebarColor);

            Rect header = new(area.x, area.y, area.width, DataHubStyles.PaneHeaderHeight);
            DrawIdentity(header, targets, pins, styles);

            float padding = DataHubStyles.PanelPadding;
            float width = Mathf.Max(0f, area.width - padding * 2f);

            Rect tabs = new(area.x + padding,
                header.yMax + (DataHubStyles.FilterBarHeight - DataHubStyles.ButtonHeight) * 0.5f, width,
                DataHubStyles.ButtonHeight);

            int picked = DataHubGui.Segmented(tabs, Array.IndexOf(TabOrder, tab), TabContents, styles);
            tab = TabOrder[Mathf.Max(0, picked)];

            float top = header.yMax + DataHubStyles.FilterBarHeight;
            Rect body = new(area.x + padding, top, width, Mathf.Max(0f, area.yMax - top - padding));

            if (shownTab == EDataPanelTab.Preview)
            {
                _preview.Draw(body, targets, row, styles);
                return false;
            }

            return shownTab switch
            {
                EDataPanelTab.Contents => _contents.Draw(body, targets, styles),
                EDataPanelTab.UsedIn => _usage.Draw(body, targets, index, styles),
                _ => _inspector.Draw(body, targets, styles)
            };
        }

        private static string TypeText(Object[] targets)
        {
            Type type = targets[0].GetType();

            return Array.Exists(targets, target => target.GetType() != type)
                ? MixedTypesText
                : ObjectNames.NicifyVariableName(type.Name);
        }

        // The selection by name, so the panel says what it is showing before the tabs say how.
        private static void DrawIdentity(Rect header, Object[] targets, DataPins pins, DataHubStyles styles)
        {
            // A deleted asset stays selected until the next rebuild, a moment later.
            if (targets.Length == 0
                || Array.Exists(targets, target => target == null))
            {
                DataHubGui.PaneTitle(header, null, NothingSelectedTitle, NothingSelectedHint, styles);
                return;
            }

            string title = targets.Length == 1
                ? targets[0].name
                : string.Format(ManyFormat, targets.Length);

            Rect titleArea = header;

            if (targets.Length == 1)
            {
                float y = header.y + (header.height - DataHubStyles.ButtonHeight) * 0.5f;
                float right = header.xMax - DataHubStyles.PanelPadding - DataHubStyles.IconButtonWidth;

                if (DataHubGui.IconButton(new Rect(right, y, DataHubStyles.IconButtonWidth,
                        DataHubStyles.ButtonHeight), DataHubIcons.Ping, PingTooltip, styles))
                    EditorGUIUtility.PingObject(targets[0]);

                right -= DataHubStyles.IconButtonWidth + EditorMetrics.TightGap;

                bool isPinned = pins.IsPinned(targets[0]);

                Texture pinIcon = isPinned
                    ? DataHubIcons.Pinned
                    : DataHubIcons.Pin;

                string pinTooltip = isPinned
                    ? PinnedTooltip
                    : PinTooltip;

                if (DataHubGui.IconButton(new Rect(right, y, DataHubStyles.IconButtonWidth,
                        DataHubStyles.ButtonHeight), pinIcon, pinTooltip, styles))
                    pins.Toggle(targets[0]);

                titleArea.width = right - header.x;
            }

            string tooltip = targets.Length == 1
                ? AssetDatabase.GetAssetPath(targets[0])
                : ManyTooltip;

            string subtitle = targets.Length == 1
                ? Path.GetDirectoryName(AssetDatabase.GetAssetPath(targets[0]))
                : ManyTooltip;

            DataHubGui.PaneTitle(titleArea, TypeText(targets), title, subtitle, styles, tooltip);
        }
    }
}