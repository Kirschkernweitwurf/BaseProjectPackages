# Confirmation Menu

🗞️ **Component** · programmers only

The "Are you sure?" popup. It shows a message plus a confirm and a cancel button. You build the layout, the component fills in the text.

## Where to put it

On the root GameObject of your confirmation popup, in the same place you would put any other menu. The popup needs to be registered like every other menu so buttons can find it.

## Fields

| Field | Required | What it does |
| --- | --- | --- |
| Confirm Button | Yes | The button that accepts. |
| Cancel Button | Yes | The button that aborts. |
| Message Text | Yes | Text field for the question. |
| Confirm Button Text | Yes | Text field inside the confirm button. |
| Cancel Button Text | Yes | Text field inside the cancel button. |
| Default Confirm Text | No  | Used when a button does not set its own label. Default: `Confirm`. |
| Default Cancel Text | No  | Used when a button does not set its own label. Default: `Cancel`. |
