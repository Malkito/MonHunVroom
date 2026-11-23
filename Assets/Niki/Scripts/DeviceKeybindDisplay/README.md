# CupOHappiness.DeviceKeybindDisplay

Local Unity package for device names, colors, and binding graphics.

## Display setup

`InputBindingDisplay` reads a shared `InputActionAsset`. `MultiplayerInputBindingDisplay` reads a player's `PlayerInput` and only uses paired devices. Both inherit the item renderer and Inspector options from `InputBindingDisplayBase`.

Assign:
- **Device Display Configurator**: the device presentation database.
- **Binding Item Prefab**: a prefab with `BindingDisplayItem`, an Image, and TMP text.
- **Binding Items Parent**: a horizontal layout container, or leave empty to use this transform.
- **Composite Binding Mode**: one of the modes below.

The supplied `Assets/►Niki/Runtime/Prefabs/UI/BindingDisplay.prefab` references `BindingItem.prefab`. Generated copies are reused during refresh, hidden when unused, and removed when the display is disabled. The renderer keeps the prefab's authored sizing.

`BindingDisplayItem` shows an icon or a text keycap using its authored Image sprite as the fallback background. `ShowSeparator` displays text without that background.

Legacy Image/Text references remain supported when no item prefab is assigned.

## Composite modes

| Mode | WASD + arrow alternatives |
| --- | --- |
| First In List | WASD icon |
| Separate Icons (default) | WASD icon / arrow-key icon |
| Compact Text | WASD / Arrow keys |

Repeated bindings with the same part name are alternatives. First In List selects the first declared binding for each named part. Separate Icons groups the first entries together, then the second entries, and so on. A singleton part is reused across groups; a missing entry in a shorter non-singleton list is shown as `—`, not silently replaced.

An alternative group uses its configured compound icon when available. Otherwise, Unity's composite display format produces an item for each required part and text separators between them. For example, Shift+E displays a Shift keycap, +, and an E keycap without changing the display setup. This uses the same rule for all composite types, not a special modifier mapping.

The current interactive rebind flow still overrides one binding path; it does not create a modifier composite from a single binding. Modifier capture and composite creation are separate from display resolution.

A custom compound entry uses the registered composite **type** and sorted named control paths:

```text
vector2composite(down=s,left=a,right=d,up=w)
vector2composite(down=downarrow,left=leftarrow,right=rightarrow,up=uparrow)
```

Unity's `Dpad` and `2DVector` registration names both resolve to `Vector2Composite`, so they use the same icon key. The keyboard settings include both entries with the compact labels `WASD` and `Arrow keys`.

The optional **Compact Label** in a device's custom icon entry is presentation metadata, not a binding alias. Rebinding a part changes the structural key, preventing a stale group icon or label from matching.

## Binding resolution

`BindingDisplayResolver` is shared by the display and `InputBindingReader`. A nearby display supplies the reader's composite mode and presentation database.

Unity resolves control names, aliases, usages such as `*/{Submit}`, and wildcards. Only `effectivePath` is used: overrides replace the original path, and an empty override disables the binding. Automatic prompts prefer the selected device and use a stable fallback order among allowed devices. Explicit row targets display the requested binding or composite part.

Icon keys are device-relative control paths, not display labels. Configurator entries use layout names, and the most specific configured matching layout wins. Custom control entries take precedence over the standard gamepad slots.

Binding and device changes refresh the display. A concrete explicit single-player binding can show its layout-based label without a connected control; player-scoped prompts never use unpaired controls.

## Unity references

- [Action bindings, display strings, overrides, and binding-resolution notifications](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.14/manual/ActionBindings.html)
- [Control paths, aliases, usages, and wildcards](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.14/manual/Controls.html)
