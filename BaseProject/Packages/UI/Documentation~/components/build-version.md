# Build Version

Shows the version and build number, for example `1.2.3 [47]`. Put it in a corner of the main menu so bug reports say which build they came from.

## Where to put it

On a GameObject in your UI, usually next to the text field it writes into.

## Fields

| Field | Required | What it does |
| --- | --- | --- |
| Hide On Release | No  | On: the text is hidden in the finished game and only shows in the Editor and test builds. |
| Version Text | Yes | The text field it writes into. |

## Good to know

- The text field must be **TextMeshPro**.
- The number comes from the version set in the project settings. The build number counts up on its own with every build, nobody has to maintain it.
- In the Editor the field can be empty, because a build has to run once before there is anything to show.
