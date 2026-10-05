# Audio

Everything here plays an Audio Container, never a raw clip. If you find yourself wanting to drag an `.wav` somewhere, make a container for it first.

## Play Audio On Click

Plays a sound when the object is clicked with the mouse.

| Field | What it does |
| --- | --- |
| **Click Sound** | The Audio Container to play |

## Play Audio On Hover

Plays a sound when the pointer enters the object.

| Field | What it does |
| --- | --- |
| **Hover Sound** | The Audio Container to play |

## Play Audio On Select

Plays a sound when the object becomes the selected UI element. This is the gamepad and keyboard version of hover.

| Field | What it does |
| --- | --- |
| **Select Sound** | The Audio Container to play |

## Play Audio On Submit

Plays a sound when the object is submitted, which covers a mouse click, the Enter key, and the gamepad confirm button.

| Field | What it does |
| --- | --- |
| **Submit Sound** | The Audio Container to play |

### Which one to use

| You want | Add |
| --- | --- |
| Mouse hover feedback | Play Audio On Hover |
| Gamepad and keyboard navigation feedback | Play Audio On Select |
| Confirm sound that works with every input device | Play Audio On Submit |
| Mouse click only | Play Audio On Click |

For a button that supports mouse and gamepad, the usual setup is **Play Audio On Hover** plus **Play Audio On Select** for the highlight, and **Play Audio On Submit** for the confirm. Adding both On Click and On Submit will double up the sound on a mouse click.

### Requirements

- An Event System in the scene.
- A Graphic on the object with **Raycast Target** turned on, for the pointer events.
- If the container field is left empty, the object logs a warning at runtime and stays silent.

---

## Audio Manager

The service that actually plays sounds. It lives on a manager prefab and is set up once per project. You should not need to add it to a scene yourself.

| Field | What it does |
| --- | --- |
| **Minimum Delay** | Shortest gap allowed between two plays of the same container, in seconds. Stops rapid retriggers from stacking. |
| **Min Pitch Inclusive** | Lower bound used when a container has Randomize Pitch on |
| **Max Pitch Inclusive** | Upper bound used when a container has Randomize Pitch on |
| **Audio Pool Manager** | The pool that supplies audio sources. Required. |

## Audio Pool Manager

Keeps a pool of reusable audio sources per audio type, so playing a sound never creates a new GameObject at runtime. Also part of the manager prefab.

| Field | What it does |
| --- | --- |
| **Pool Parent** | Transform the pooled sources are parented to |
| **Is Clearing Pool After Scene Load** | Return all sources to the pool when a new scene loads |
| **Audio Source 2D Prefab** | Prefab used for `Sfx2D` |
| **Audio Source 3D Prefab** | Prefab used for `Sfx3D` |
| **Audio Source Music Prefab** | Prefab used for `Music` |
| **Audio Source UI Prefab** | Prefab used for `UI` |

Each prefab is where the mixer group is assigned, so these four prefabs are what actually connect an Audio Container's **Audio Type** to the player's volume sliders. If a category of sound ignores its slider, check the prefab for that type first.
