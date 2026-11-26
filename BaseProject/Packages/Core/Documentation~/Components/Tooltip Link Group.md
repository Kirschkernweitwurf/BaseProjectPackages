---
type: Component
package: Core
audience: Everyone
tags:
  - designer
  - artist
  - ui
summary: Shows something a tooltip names beside it, as a picture with its own panels.
---
# Tooltip Link Group

🔖 **Component** · for everyone

**Tags:** #designer #artist #ui

Shows one link of a tooltip beside its main card, as a card of its own: the picture of something the text names, like the card a rule deals, on the left, then that thing's own panels, like the terms of that card. The [Tooltip View](Tooltip%20View.md) copies the group for every further link. Where a row of links would be too wide for either side of the object, the view stacks them: the panels go below the picture. The picture is drawn by a component that derives from `TooltipSubjectView`, because only the project knows what its things look like.

**Add it:** Inspector › **Add Component** › search `Tooltip Link Group`. Only one per GameObject.

## How to use it

1. Inside the Tooltip View's content, next to the main card, add the group: a RectTransform with a Horizontal Layout Group (**Control Child Size** on, **Child Force Expand** off, upper left) and the card's background and shadow as children that ignore the layout, like the aside card.
2. Add the column as its first child: a Vertical Layout Group (upper left) holding the picture, a GameObject with your `TooltipSubjectView` component and what it draws, sized by a Layout Element with some margin.
3. Add the panels holder as its second child: an empty Vertical Layout Group. The panels are copies of the view's panel template.
4. Add **Tooltip Link Group** to the group and drag in the picture, the column and the panels holder.
5. Drag the group into the Tooltip View's **Link Template** and the content's layout group into its **Row**.

## Fields

### References

| Field | Type | Default | What it does |
| --- | --- | --- | --- |
| **Subject** | Tooltip Subject View |  | **Required.** Draws the link's picture, like a card face. Hidden when it can't draw the subject. |
| **Column** | Horizontal Or Vertical Layout Group |  | **Required.** Column in the row that holds the picture, and the panels below it while the link is stacked. |
| **Card** | Transform |  | **Required.** Holds the link's panels beside the picture, copies of the view's panel template. Hidden when the link has none. |

## Good to know

- Links come from an `ITooltipLinkProvider` next to the [Tooltip Trigger](Tooltip%20Trigger.md). Each `TooltipLink` carries a subject, handed to the picture's `TooltipSubjectView.TryShow`, and its own panels.
- A link whose picture can't draw its subject and that has no panels is left out.
- The picture and the panels never take the pointer or the focus: the view's content never blocks raycasts. Turn off buttons and other selectables inside the picture, so a gamepad never steps into the tooltip.
- A picture that looks like things on the screen, like a card face over a deck of cards, reads as part of the tooltip when the link is a card of its own with the tooltip's background.
- Copies keep the panels they brought along and reuse them, so a big link only creates them once.

## Related

- [🔖 Tooltip View](Tooltip%20View.md)
- [🔖 Tooltip Trigger](Tooltip%20Trigger.md)
- [🔖 Tooltip Panel](Tooltip%20Panel.md)