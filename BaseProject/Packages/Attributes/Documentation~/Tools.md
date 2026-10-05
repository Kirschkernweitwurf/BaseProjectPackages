# Tools

🧰 **Tools** · menu items and editor windows

Four editor windows and one switch ship with the Attributes package.

| Tool | What it is for | For whom |
| --- | --- | --- |
| [🪛 Attributes Window](Tools/Attributes%20Window.md) | Every attribute explained, with live samples, plus a scan for ones that cannot work | Everyone |
| [🪛 Required References Window](Tools/Required%20References%20Window.md) | Lists every required field that is still empty, in scenes and assets | Everyone |
| [🪛 Assign GetComponents](Tools/Assign%20GetComponents.md) | Fills all auto-assign fields across prefabs and open scenes in one click | Everyone |
| [🔧 GetComponent Require Audit](Tools/GetComponent%20Require%20Audit.md) | Finds classes missing a `[RequireComponent]` | Programmers |
| [🪛 Disable Attribute Inspector](Tools/Disable%20Attribute%20Inspector.md) | Emergency switch back to Unity's own inspector | Everyone |

The last three live under `Tools > Base Packages > Unity Editor > References`. The Attributes window lives under `Tools > Base Packages > Unity Editor > Project Health`.

> [!NOTE]
> Menu paths listed on the child pages are defaults. They can be moved with the Menu Item Manager in the Tools package.

## Typical workflow

1. Run **Assign GetComponents** to fill everything that can fill itself.
2. Open **Required References** to see what is left and click your way through the fixes.
3. Programmers run **GetComponent Require Audit** now and then to keep the scripts clean.
4. Programmers run the **Attributes** window's Troubleshoot tab now and then to catch attributes that stopped pointing at anything.

Reach for the **Attributes** window's Reference tab whenever you are not sure which attribute to use, or what one of them does.

See the [📦 Attributes Package](index.md) overview for the attributes these tools react to.
