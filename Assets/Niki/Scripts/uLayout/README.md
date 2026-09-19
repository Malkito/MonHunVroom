# uLayout

**uLayout** is a simple UI layout system designed as a drop-in replacement for Unity's `VerticalLayoutGroup` and `HorizontalLayoutGroup`. It implements a core subset of the CSS *flexbox* specification and operates on `RectTransform`s, so it remains compatible with native uGUI components such as `Image`, `RectMask2D`, and `ScrollRect`.

It's designed with performance in mind. Components use Unity's native canvas layout events and only rebuild after relevant size, hierarchy, or content changes. The demo scene costs ~1.5ms in a standalone build on the author's machine (i9-9900k), most of which is TMP_Text updates.

---

## Installation
Download the `.unitypackage` from the project's Releases page and import it through Unity's `Assets > Import Package > Custom Package...` menu.

UPM and Git URL installation are not currently supported. `LayoutText` additionally requires TextMeshPro, which is included with Unity's uGUI package.

---

## Setup
uLayout components can be quickly added to your scene via `GameObject > UI > Layout` (or the scene-view right-click menu). In Unity 6.3 and later, use `GameObject > UI (Canvas) > Layout`.

All layout elements can choose one of four `SizingMode` options for each axis:\
`FitContent`: fits the rect tightly around its contents when the component can measure content (`Layout`, `GridLayout`, or `LayoutText`)\
`Fixed`: keeps the current `RectTransform` size\
`Grow`: grows to fill available parent space; Grow children share remaining space, subject to their constraints\
`Percent`: uses a 0-to-1 fraction of the available parent size

Minimum and maximum size constraints apply after a size is resolved. Avoid a `FitContent` parent with a `Grow` or `Percent` child on the same axis because that creates a circular size dependency. The child Inspector shows a warning below Size Mode when it detects this setup.

Use `LayoutItem` for a normal leaf object. Use `Layout` for flex-style row or column placement. Use `GridLayout` for responsive grid placement. Both container types inherit from `LayoutItem`, so they can use uLayout sizing and floating rules inside another container.

Further explanation and examples can be found in the sample scene at `Samples/LayoutDemo/LayoutDemo.unity`. If you've never used CSS flexbox before, I also recommend taking a look at [this guide](https://css-tricks.com/snippets/css/a-guide-to-flexbox/) that covers the basics with super helpful illustrations :)

### Text Support
uLayout also supports TextMeshPro `TMP_Text` objects, using the `LayoutText` component. This also derives from `LayoutItem`, offering the same sizing options. This allows text objects to resize depending on contents and font size. Resizing text is fairly expensive, so you generally want to avoid resizing text as much as possible during runtime.

### Tabs
uLayout includes a reusable uGUI tabs subsystem built on Unity's standard `Toggle` and `ToggleGroup` components. Create tabs through `GameObject > UI > Layout > Tabs` (or `GameObject > UI (Canvas) > Layout > Tabs` in Unity 6.3 and later).

`TabGroup` owns selection state and a list of explicit `TabEntry` button/page pairs. `TabButton` adapts a standard `Toggle`, so pointer, touch, keyboard, gamepad navigation, interactable state, `CanvasGroup`, Color Tint, Sprite Swap, and Animator transitions continue to use built-in Unity behavior. Images and pages are optional.

---

## Component Settings
### LayoutItem
- **Margin**: Extra space outside the item
- **Size Mode**: Sets the rect sizing mode for each axis. `FitContent` has no effect on a base `LayoutItem`; use a measurable derived type
  - **x** (`SizingMode`)
  - **y** (`SizingMode`)
- **Percentage**: Per-axis fraction used by `Percent`
- **Min Size / Max Size**: Per-axis size constraints
- **Is Floating**: Removes the item from normal flow and optionally attaches it to its parent, root canvas, or another `RectTransform`

### Layout (&larr; `LayoutItem`)
- **Padding**: Set a buffer width between each edge and the layout contents
  - **top, bottom, left, right** (`float`)
- **Direction** (`enum`)
  - `Row`: Position children left-to-right
  - `Column`: Position children top-to-bottom
  - `RowReverse`: Position children right-to-left
  - `ColumnReverse`: Position children bottom-to-top
- **Justify Content** (`enum`)
  - `Start`: Align children to the start of the primary axis (depends on `Direction`: left for Row, top for Column, etc)
  - `Center`: Align children to the center of the primary axis
  - `End`: Align children to the end of the primary axis
  - `SpaceBetween`: Space children evenly across the primary axis
- **Align Content** (`enum`): Positions flex lines on the cross axis
  - `Start`, `Center`, `End`
- **Align Items** (`enum`): Positions each child inside its flex line on the cross axis
  - `Start`, `Center`, `End`
- **Inner Spacing** (`float`): Sets the gap between children on the primary layout axis. It is ignored by `Justification.SpaceBetween`
- **Ignore Child Scale** (`bool`): Whether to ignore child `RectTransform` scale when calculating fit size and placing children
- **Wrap** (`FlexWrap`): Controls whether children stay on one line or form additional flex lines when they overflow the main axis
  - `NoWrap`: Keep all children on one line
  - `Wrap`: Add lines in the normal cross-axis direction
  - `WrapReverse`: Add lines in the reverse cross-axis direction
- **Cross Axis Gap** (`float`): Sets the gap between wrapped lines

`Align Content` positions the flex lines. `Align Items` positions children inside each line. Wrapping is disabled by default. `Inner Spacing` applies within each line; use `Cross Axis Gap` for spacing between lines.

uLayout does not manage clipping or scrolling. To scroll a uLayout container, use Unity's built-in `ScrollRect` and assign the uLayout object as its `content`. Configure the viewport, masking, scroll axes, and scrollbars on that `ScrollRect`.

```text
Viewport (RectMask2D + ScrollRect)
└── Content (Layout)
    └── UI children
```

The uLayout `Layout` and Unity `ScrollRect` have separate responsibilities: uLayout positions content children; `ScrollRect` handles viewport clipping and scrolling. Do not put a built-in `LayoutGroup` on the same GameObject as a uLayout container.

### GridLayout (&larr; `LayoutItem`)
`GridLayout` supports independent track settings for columns and rows:
- **FixedCount**: Uses the requested track count. Invalid or zero counts are clamped to one.
- **AutoFit**: Creates tracks to fit the available space and collapses unused tracks.
- **AutoFill**: Creates tracks to fit the available space and keeps unused tracks.
- **Min Track Size** (`float`): Minimum track size in pixels. Tracks can become smaller when the container is smaller than the minimum.
- **Fill Extra Space** (`float`): Set to 0 to keep tracks at their minimum size. A value above 0 distributes free space evenly across the tracks.

`ColumnTracks` controls columns and `RowTracks` controls rows. Non-fixed row modes create the rows required by item flow. `Gap` uses X between columns and Y between rows, and `Padding` is the inner space around the grid. `Child Alignment X/Y` aligns items inside their cells. Fixed-size children keep their size; `Grow` and `Percent` children resolve against the cell size minus margins. Inactive, disabled `LayoutItem`, floating, and standard `LayoutElement.ignoreLayout` children do not consume cells. A grid with no active children still resolves at least one track.

### TabGroup
- **Tabs**: Explicit `TabButton` and optional page pairs
- **Initial Index**: Tab selected when the group starts
- **Allow No Selection**: Allows all toggles and pages to be off
- **Navigation**: Clamp at the ends or wrap around
- **Skip Non Interactable**: Next/Previous navigation skips disabled controls
- **Preserve Selection**: Keeps the selected index when the group is disabled and enabled
- **On Selected Index Changed**: Selection observer event

`Select`, `NextTab`, `PreviousTab`, `Register`, `Unregister`, and `SetPage` support runtime menus. Selecting the current tab does not invoke events again. Pages use `SetActive`; leave a page empty and use button/group events for Animator, CanvasGroup, Addressables, pooling, or custom page behavior.

### TabButton
`TabButton` requires a standard `Toggle`, but does not require an `Image`. Configure target graphics, navigation, transitions, and interactable state on the Toggle. Optional selected and deselected events provide the extension seam for sounds, animation, analytics, and other game behavior.

Tab behavior is independent of geometry. A tab bar can use uLayout `Layout`, `GridLayout`, or a built-in Unity LayoutGroup. Never put uLayout `Layout` and a built-in LayoutGroup on the same GameObject because both will drive the same child RectTransforms.

---

## Known Issues
- Scrollbars and viewport masking must be configured on Unity's built-in `ScrollRect`.
- Do not put a built-in `LayoutGroup` on the same GameObject as a uLayout `Layout` or `GridLayout`; both systems would drive the same child `RectTransform`s.
- A `FitContent` parent and a `Grow` or `Percent` child on the same axis create a circular size dependency.

---

## Contributing
Contributions are welcome and greatly appreciated! Open a pull request and I'll do my best to review it in a timely manner. For contributors: **Please do not go through and alter existing code style and/or syntax.** This makes it needlessly difficult to parse your changes, and I will simply end up changing it back anyway!


