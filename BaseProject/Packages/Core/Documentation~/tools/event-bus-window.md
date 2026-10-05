# Event Bus Window

**Menu:** `Tools > Base Packages > Runtime > Event Bus`

**For:** Programmers

A live view of the event bus while the game runs: every event type that has listeners, and who is listening, in the order the bus calls them.

## What it is good for

- Checking that a listener actually subscribed.
- Finding listeners that should be gone, for example from an object that was destroyed.
- Seeing the call order when two listeners react to the same event.

## How to use it

1. Open the window and enter Play mode.
2. The list refreshes on its own. You do not need to press anything.
3. Expand an event type to see its subscribers.

## Good to know

- Lambdas are shown with the class that really owns them, not the compiler-generated one.
- The window only reads. It never changes subscriptions.
