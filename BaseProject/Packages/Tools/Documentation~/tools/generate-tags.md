# Generate Tags

**Menu:** `Tools > Base Packages > Code > Generation > Generate Tags`

Writes a `Tags` class that contains every tag in the project as a const string. This means no more typing tag names by hand and no more typos that only show up at runtime.

## How to use it

1. Add or rename your tags in the Tags & Layers settings.
2. Run the menu command.
3. The file is written and Unity refreshes.

## Result

```csharp
public static class Tags
{
    public const string Player = "Player";
    public const string Enemy = "Enemy";
}
```

Use it like this:

```csharp
if (other.CompareTag(Tags.Player)) { }
```

## Notes

- Run it again after every tag change. The file is overwritten, so nothing gets stale.
- Names that are not valid C# identifiers are cleaned up automatically.
- Do not edit the generated file by hand.
