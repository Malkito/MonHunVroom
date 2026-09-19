# CupOHappiness.DeviceKeybindDisplay

This local Unity package contains the standalone device keybind display and icon lookup system.

Contents:
- Runtime ScriptableObject types for device display names, colors, and binding icons
- Custom inspectors for editing device display assets
- Starter device display assets for keyboard, generic gamepad, Xbox, PlayStation 4, and Nintendo Switch Pro controllers
- Input icon sprites used by those assets

Install it into another project as a local package and reference the `DeviceDisplayConfigurator` and `DeviceDisplaySettings` assets from your UI or gameplay scripts.

## Set up one keybind graphic

1. Add a `DeviceDisplayConfigurator` asset from `Runtime/Assets/DeviceDisplaySettings` to your scene or a serialized field on a MonoBehaviour.
2. Add a UI `Image` where you want the keybind icon to appear.
3. Get the current binding as a human-readable string from your `InputAction`.
4. Ask the configurator for the sprite for the current player's device and assign it to the `Image`.

Example:

```csharp
using CupOHappiness.DeviceKeybindDisplay.Runtime;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class KeybindGraphicExample : MonoBehaviour
{
    public PlayerInput playerInput;
    public DeviceDisplayConfigurator deviceDisplayConfigurator;
    public Image bindingImage;
    public string actionName = "Jump";

    public void RefreshGraphic()
    {
        InputAction action = playerInput.actions.FindAction(actionName);
        int bindingIndex = action.GetBindingIndexForControl(action.controls[0]);

        string bindingName = InputControlPath.ToHumanReadableString(
            action.bindings[bindingIndex].effectivePath,
            InputControlPath.HumanReadableStringOptions.OmitDevice);

        bindingImage.sprite = deviceDisplayConfigurator.GetDeviceBindingIcon(playerInput, bindingName);
    }
}
```

If `GetDeviceBindingIcon` returns `null`, that binding does not have a matching sprite in the selected device settings yet, so you can fall back to text or add a custom context icon entry to the relevant `DeviceDisplaySettings` asset.

## Built-in Unity Input System integration

The package uses `PlayerInput` and the project's `InputSystem_Actions.inputactions` asset. Use these components from `CupOHappiness.DeviceKeybindDisplay.Integrations.InputSystem`:

- `InputBindingDisplay`
- `InputBindingReader`
- `InputInteractionTrigger`

The editor inspectors read action maps and actions directly from `InputSystem_Actions.inputactions`. Wildcard bindings such as `{Submit}` use the active Unity device layout and the matching `DeviceDisplayConfigurator` mapping, with a control-name fallback when no mapping exists.
