# State Machine Monitor

🔧 **Tool** · programmers only

**Menu:** `Tools > Base Packages > Gameplay > State Machine Monitor`

Shows the state machines running in Play mode and what each one is doing right now.

## How to use it

1. Open the window and enter Play mode.
2. Pick a machine from the list on the left.
3. The middle shows its states as boxes with the current state highlighted.
4. Underneath you see the start state, how long the machine has been in its current state, and which transitions are being checked, in the order they are checked.

## Reading the graph

- Columns are ordered by distance from the start state, so you read a machine left to right in the order it can run.
- States nothing can reach end up in the last column. If a state sits there, it can never be entered.

## Good to know

Only machines that are currently alive show up. The monitor keeps weak references, so it never keeps a machine alive by watching it.
