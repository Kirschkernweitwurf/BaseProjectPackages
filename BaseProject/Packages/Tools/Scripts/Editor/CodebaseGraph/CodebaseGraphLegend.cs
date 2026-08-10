using System.Collections.Generic;
using Base.ToolPackage.Editor.CodebaseGraph.Model;
using UnityEngine.UIElements;

namespace Base.ToolPackage.Editor.CodebaseGraph
{
    /// <summary>
    /// Explains the encodings on the graph. Several things are being said at once by shape, glyph and
    /// color, and a reader who has to guess at any of them will end up trusting none of them. Built from
    /// the same catalog the nodes draw from, so it cannot describe something that is not on screen.
    /// </summary>
    public sealed class CodebaseGraphLegend : VisualElement
    {
        private const string BodyClass = "legend-body";
        private const string CollapsedText = "Legend";
        private const string DismissedSwatchClass = "legend-swatch-dismissed";
        private const string EntryClass = "legend-entry";
        private const string ExpandedText = "Legend, click to hide";
        private const string FindingSwatchClass = "legend-swatch-finding";
        private const string GlyphClass = "legend-glyph";
        private const string LabelClass = "legend-label";
        private const string LegendClass = "legend";
        private const string MembersTitle = "Members";
        private const string ShapeMemberText = "Narrow: Member";
        private const string ShapeNamespaceText = "Wide and round: Namespace";
        private const string ShapesTitle = "Shapes";
        private const string ShapeTypeText = "Square: Type. Light border: Interface";
        private const string StateDismissedText = "Dismissed";
        private const string StateFindingText = "Warning";
        private const string StateTitle = "Row and badge color";
        private const string SwatchClass = "legend-swatch";
        private const string TitleClass = "legend-title";
        private const string TypesTitle = "Types";
        private const string VisibilityTitle = "Left stripe and text color";

        private readonly Button _header;
        private readonly VisualElement _body;

        private bool _isOpen = true;

        /// <summary>Builds the legend, open by default.</summary>
        public CodebaseGraphLegend()
        {
            AddToClassList(LegendClass);

            _header = new Button(Toggle)
            {
                text = ExpandedText
            };

            Add(_header);

            _body = new VisualElement();
            _body.AddToClassList(BodyClass);
            Add(_body);

            BuildBody();
        }

        private static Label BuildLabel(string text, string styleClass)
        {
            Label label = new(text);
            label.AddToClassList(styleClass);
            return label;
        }

        private void BuildBody()
        {
            _body.Add(BuildLabel(ShapesTitle, TitleClass));
            _body.Add(BuildShape(GraphSymbols.NamespaceGlyph, ShapeNamespaceText));
            _body.Add(BuildShape(GraphSymbols.GetGlyph(ETypeKind.Class), ShapeTypeText));
            _body.Add(BuildShape(GraphSymbols.GetGlyph(EMemberKind.Method), ShapeMemberText));

            _body.Add(BuildLabel(TypesTitle, TitleClass));
            foreach (KeyValuePair<ETypeKind, string> pair in GraphSymbols.GetTypeGlyphs())
                _body.Add(BuildShape(pair.Value, pair.Key.ToString()));

            _body.Add(BuildLabel(MembersTitle, TitleClass));
            foreach (KeyValuePair<EMemberKind, string> pair in GraphSymbols.GetMemberGlyphs())
                _body.Add(BuildShape(pair.Value, pair.Key.ToString()));

            _body.Add(BuildLabel(VisibilityTitle, TitleClass));
            foreach (EAccessLevel access in GraphSymbols.GetAccessOrder())
                _body.Add(BuildAccess(access));

            _body.Add(BuildLabel(StateTitle, TitleClass));
            _body.Add(BuildSwatch(FindingSwatchClass, StateFindingText));
            _body.Add(BuildSwatch(DismissedSwatchClass, StateDismissedText));
        }

        private VisualElement BuildShape(string glyph, string text)
        {
            VisualElement row = new();
            row.AddToClassList(EntryClass);
            row.Add(BuildLabel(glyph, GlyphClass));
            row.Add(BuildLabel(text, LabelClass));

            return row;
        }

        private VisualElement BuildSwatch(string swatchClass, string text)
        {
            VisualElement row = new();
            row.AddToClassList(EntryClass);

            VisualElement swatch = new();
            swatch.AddToClassList(SwatchClass);
            swatch.AddToClassList(swatchClass);
            row.Add(swatch);

            row.Add(BuildLabel(text, LabelClass));
            return row;
        }

        private VisualElement BuildAccess(EAccessLevel access)
        {
            VisualElement row = BuildShape(GraphSymbols.GetGlyph(EMemberKind.Field), access.ToString());
            row.Q<Label>(className: GlyphClass).style.color = GraphSymbols.GetColor(access);
            row.Q<Label>(className: LabelClass).style.color = GraphSymbols.GetColor(access);

            return row;
        }

        private void Toggle()
        {
            _isOpen = !_isOpen;

            _header.text = _isOpen
                ? ExpandedText
                : CollapsedText;

            _body.style.display = _isOpen
                ? DisplayStyle.Flex
                : DisplayStyle.None;
        }
    }
}