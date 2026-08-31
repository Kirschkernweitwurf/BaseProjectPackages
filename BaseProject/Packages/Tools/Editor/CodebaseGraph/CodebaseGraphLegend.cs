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
    internal sealed class CodebaseGraphLegend : VisualElement
    {
        private const string CollapsedText = "Legend";
        private const string ExpandedText = "Legend, click to hide";
        private const string MembersTitle = "Members";
        private const string ShapeMemberText = "Narrow: Member";
        private const string ShapeNamespaceText = "Wide and round: Namespace";
        private const string ShapesTitle = "Shapes";
        private const string ShapeTypeText = "Square: Type. Light border: Interface";
        private const string StateDismissedText = "Dismissed";
        private const string StateFindingText = "Warning";
        private const string StateTitle = "Row and badge color";
        private const string TypesTitle = "Types";
        private const string VisibilityTitle = "Left stripe and text color";

        private readonly Button _header;
        private readonly VisualElement _body;

        private bool _isOpen = true;

        /// <summary>Builds the legend, open by default.</summary>
        public CodebaseGraphLegend()
        {
            AddToClassList(CodebaseGraphStyle.LegendClass);

            _header = new Button(Toggle)
            {
                text = ExpandedText
            };

            Add(_header);

            _body = new VisualElement();
            _body.AddToClassList(CodebaseGraphStyle.LegendBodyClass);
            Add(_body);

            BuildBody();
        }

        private static VisualElement BuildShape(string glyph, string text)
        {
            VisualElement row = new();
            row.AddToClassList(CodebaseGraphStyle.LegendEntryClass);
            row.Add(GraphLabel.Build(glyph, CodebaseGraphStyle.LegendGlyphClass));
            row.Add(GraphLabel.Build(text, CodebaseGraphStyle.LegendLabelClass));

            return row;
        }

        private static VisualElement BuildSwatch(string swatchClass, string text)
        {
            VisualElement row = new();
            row.AddToClassList(CodebaseGraphStyle.LegendEntryClass);

            VisualElement swatch = new();
            swatch.AddToClassList(CodebaseGraphStyle.LegendSwatchClass);
            swatch.AddToClassList(swatchClass);
            row.Add(swatch);

            row.Add(GraphLabel.Build(text, CodebaseGraphStyle.LegendLabelClass));
            return row;
        }

        private static VisualElement BuildAccess(EAccessLevel access)
        {
            VisualElement row = BuildShape(GraphSymbols.GetGlyph(EMemberKind.Field), access.ToString());
            row.Q<Label>(className: CodebaseGraphStyle.LegendGlyphClass).style.color = GraphSymbols.GetColor(access);
            row.Q<Label>(className: CodebaseGraphStyle.LegendLabelClass).style.color = GraphSymbols.GetColor(access);

            return row;
        }

        private void BuildBody()
        {
            _body.Add(GraphLabel.Build(ShapesTitle, CodebaseGraphStyle.LegendTitleClass));
            _body.Add(BuildShape(GraphSymbols.NamespaceGlyph, ShapeNamespaceText));
            _body.Add(BuildShape(GraphSymbols.GetGlyph(ETypeKind.Class), ShapeTypeText));
            _body.Add(BuildShape(GraphSymbols.GetGlyph(EMemberKind.Method), ShapeMemberText));

            _body.Add(GraphLabel.Build(TypesTitle, CodebaseGraphStyle.LegendTitleClass));
            foreach (KeyValuePair<ETypeKind, string> pair in GraphSymbols.GetTypeGlyphs())
                _body.Add(BuildShape(pair.Value, pair.Key.ToString()));

            _body.Add(GraphLabel.Build(MembersTitle, CodebaseGraphStyle.LegendTitleClass));
            foreach (KeyValuePair<EMemberKind, string> pair in GraphSymbols.GetMemberGlyphs())
                _body.Add(BuildShape(pair.Value, pair.Key.ToString()));

            _body.Add(GraphLabel.Build(VisibilityTitle, CodebaseGraphStyle.LegendTitleClass));
            foreach (EAccessLevel access in GraphSymbols.GetAccessOrder())
                _body.Add(BuildAccess(access));

            _body.Add(GraphLabel.Build(StateTitle, CodebaseGraphStyle.LegendTitleClass));
            _body.Add(BuildSwatch(CodebaseGraphStyle.LegendSwatchFindingClass, StateFindingText));
            _body.Add(BuildSwatch(CodebaseGraphStyle.LegendSwatchDismissedClass, StateDismissedText));
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