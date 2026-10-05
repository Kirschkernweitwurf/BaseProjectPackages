# Generate Layers

🔧 **Tool** · programmers only

**Tags:** #programmer #code-generation

**Menu:** `Tools > Base Packages > Code > Generation > Generate Layers`

Writes a `Layers` class that contains every layer in the project as a const int, plus a nested `Masks` class with the matching bit mask values.

## How to use it

1. Add or rename your layers in the Tags & Layers settings.
2. Run the menu command.
3. The file is written and Unity refreshes.

## Result

```csharp
public static class Layers
{
    public const int Default = 0;
    public const int Water = 4;

    public static class Masks
    {
        public const int Default = 1 << 0;
        public const int Water = 1 << 4;
    }
}
```

Use the index when you set `gameObject.layer`, and the mask when you do a raycast or fill a `LayerMask` field.

## Notes

- Run it again after every layer change.
- Do not edit the generated file by hand.
