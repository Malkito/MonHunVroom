using System;
using UnityEngine.InputSystem;

namespace CupOHappiness.DeviceKeybindDisplay.Integrations.InputSystem
{
    internal static class InputDeviceSelectionUtility
    {
        internal static InputDevice FindActiveDevice(InputActionAsset asset, Func<InputDevice, bool> filter = null)
        {
            if (asset == null)
            {
                return null;
            }

            var uiMap = asset.FindActionMap("UI", throwIfNotFound: false);
            if (uiMap != null)
            {
                foreach (var action in uiMap.actions)
                {
                    var device = action.activeControl?.device;
                    if (device != null && (filter == null || filter(device)))
                    {
                        return device;
                    }
                }
            }

            foreach (var map in asset.actionMaps)
            {
                foreach (var action in map.actions)
                {
                    var device = action.activeControl?.device;
                    if (device != null && (filter == null || filter(device)))
                    {
                        return device;
                    }
                }
            }

            foreach (var map in asset.actionMaps)
            {
                foreach (var action in map.actions)
                {
                    foreach (var control in action.controls)
                    {
                        var device = control?.device;
                        if (device != null && (filter == null || filter(device)))
                        {
                            return device;
                        }
                    }
                }
            }

            return null;
        }
    }
}
