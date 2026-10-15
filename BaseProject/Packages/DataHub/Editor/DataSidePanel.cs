using System;
using Base.DataHubPackage.Editor.Lists;
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
        private const string ContentsTabTooltip = "The asset lists of the selected asset, like the cards of a deck, "
            + "one line per entry with its count. Change the counts, add entries, or drag rows from the table onto a "
            + "list.";
        private const string InspectorTabLabel = "Inspector";
        private const string InspectorTabTooltip = "The normal inspector of the selected rows, for the fields a cell "
            + "cannot edit, like effect lists, and for the buttons of the asset. With several rows selected, a change "
            + "goes to all of them.";
        private const string ManyFormat = "{0} assets";
        private const string ManyTooltip = "Edits in the table and in the Inspector tab go to every selected asset.";
        private const string MixedTypesText = "Different types";
        private const string NothingSelectedHint = "Pick a row in the table";
        private const string NothingSelectedTitle = "Nothing selected";
        private const string PingTooltip = "Show the selected asset in the Project window.";
        private const string UsedInTabLabel = "Used In";
        private const string UsedInTabTooltip = "Every deck, collection or other list in the project that can hold "
            + "the selected rows, with how often they are in it. Plus and minus change it for every selected row.";

        private static readonly GUIContent[] TabContents =
        {
            new(InspectorTabLabel, InspectorTabTooltip),
            new(ContentsTabLabel, ContentsTabTooltip),
            new(UsedInTabLabel, UsedInTabTooltip)
        };

        private readonly DataContentsPanel _contents = new();
        private readonly DataInspectorPanel _inspector = new();
        private readonly DataUsagePanel _usage = new();

        /// <summary>Forgets every list found for the Used In tab, after the project changed.</summary>
        internal void Invalidate() => _usage.Invalidate();

        /// <summary>Releases the inspector and every cached serialized object.</summary>
        internal void Dispose()
        {
            _inspector.Dispose();
            _contents.Dispose();
            _usage.Dispose();
        }

        /// <summary>Draws the panel.</summary>
        /// <param name="area">The area to fill.</param>
        /// <param name="targets">The selected assets.</param>
        /// <param name="shownTab">The tab drawn this pass, fixed since the last layout.</param>
        /// <param name="tab">The tab the user picked, which takes effect on the next layout.</param>
        /// <param name="index">The assets of the project, for the Used In tab.</param>
        /// <param name="styles">The built window styles.</param>
        /// <returns>True when a value was changed through the panel.</returns>
        internal bool Draw(Rect area, Object[] targets, EDataPanelTab shownTab, ref EDataPanelTab tab,
            DataAssetIndex index, DataHubStyles styles)
        {
            DataHubGui.Fill(area, DataHubStyles.SidebarColor);

            Rect header = new(area.x, area.y, area.width, DataHubStyles.PaneHeaderHeight);
            DrawIdentity(header, targets, styles);

            float padding = DataHubStyles.PanelPadding;
            float width = Mathf.Max(0f, area.width - padding * 2f);

            Rect tabs = new(area.x + padding,
                header.yMax + (DataHubStyles.FilterBarHeight - DataHubStyles.ButtonHeight) * 0.5f, width,
                DataHubStyles.ButtonHeight);

            tab = (EDataPanelTab)DataHubGui.Segmented(tabs, (int)tab, TabContents, styles);

            float top = header.yMax + DataHubStyles.FilterBarHeight;
            Rect body = new(area.x + padding, top, width, Mathf.Max(0f, area.yMax - top - padding));

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
        private static void DrawIdentity(Rect header, Object[] targets, DataHubStyles styles)
        {
            // A deleted asset stays selected until the next rebuild, a moment later.
            if (targets.Length == 0
                || Array.Exists(targets, target => target == null))
            {
                DataHubGui.PaneTitle(header, null, null, NothingSelectedTitle, NothingSelectedHint, styles);
                return;
            }

            string title = targets.Length == 1
                ? targets[0].name
                : string.Format(ManyFormat, targets.Length);

            Rect titleArea = header;

            if (targets.Length == 1)
            {
                Rect ping = new(header.xMax - DataHubStyles.PanelPadding - DataHubStyles.IconButtonWidth,
                    header.y + (header.height - DataHubStyles.ButtonHeight) * 0.5f, DataHubStyles.IconButtonWidth,
                    DataHubStyles.ButtonHeight);

                if (DataHubGui.IconButton(ping, DataHubIcons.Ping, PingTooltip, styles))
                    EditorGUIUtility.PingObject(targets[0]);

                titleArea.width = ping.x - header.x;
            }

            string tooltip = targets.Length == 1
                ? AssetDatabase.GetAssetPath(targets[0])
                : ManyTooltip;

            DataHubGui.PaneTitle(titleArea, DataHubIcons.Of(targets[0]), null, title, TypeText(targets), styles,
                tooltip);
        }
    }
}