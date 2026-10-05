# GetComponent Require Audit

**Menu:** `Tools > Base Packages > Unity Editor > References > GetComponent Require Audit`

## What it does

Finds every field marked with `[GetComponent]` whose class is missing a matching
`[RequireComponent]`.

`[GetComponent]` expects the component to sit on the same GameObject. If the class does not also declare `[RequireComponent(typeof(...))]`, nothing stops someone from adding the script to a GameObject that has no such component, and the reference silently stays empty.

## How to use it

1. Open the window from the menu.
2. If everything is fine, you get a green info box.
3. Otherwise every problem is listed as one row, for example:

   ```csharp
   PlayerMover.rigidbody  needs  [RequireComponent(typeof(Rigidbody))]
   ```

4. Press **Open** on a row to open that script in the code editor.
5. Add the missing attribute above the class.
6. Press **Rescan** to check again.

## Good to know

- `[GetComponentInParent]` is intentionally ignored. Its target lives on a parent object, so
  `[RequireComponent]` does not apply.
- Only fields whose type is a Component are checked.
- The list is sorted by class name, then field name.
