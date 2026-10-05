# Zoo Root Marker

🗞️ **Component** · programmers only

An empty marker component that the Asset Zoo Builder puts on the root object of a built zoo.

## Why it exists

The builder needs to find the zoo it created earlier so it can clear or rebuild it. Looking it up by component type is safe. Looking it up by name would break the moment somebody renames the object, or names their own object the same thing.

## What you need to know

- You never add this yourself. It is hidden from the Add Component menu.
- It has no settings and does nothing at runtime.
- Do not remove it from a built zoo, or the builder will lose track of it and leave the old zoo behind.
- Only one per GameObject.
