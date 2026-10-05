# Order Manager

🔧 **Tool** · programmers only

**Menu:** `Tools > Base Packages > Code > Generation > Order Manager`

A small editor for named number constants, plus a generator that writes them into a C# file. It is mostly used for menu priorities, so all the magic numbers live in one place instead of being scattered across scripts.

## How to use it

1. Open the window.
2. Set the output settings at the top:
   - **Output Directory**: where the file goes, for example `Assets/Generated/UnityConstants`.
   - **Namespace**: the namespace of the generated code.
   - **Root Class Name**: the class name, and the file name. Default is `MenuOrders`.
3. Add constants. Each one has a **name**, a **value** and an optional **comment**.
4. Press **Generate**.

## Result

```csharp
public static class MenuOrders
{
    /// <summary>Everything under the GameObject menu.</summary>
    public const int GameObject = 20;
}
```

## Notes

- The list is stored in `ProjectSettings`, so the whole team shares it through version control.
- The comment field becomes an XML summary above the constant.
- Do not edit the generated file by hand.
