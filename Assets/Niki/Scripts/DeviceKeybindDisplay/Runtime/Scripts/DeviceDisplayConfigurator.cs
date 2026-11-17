using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace CupOHappiness.DeviceKeybindDisplay.Runtime
{
    [CreateAssetMenu(fileName = "Device Display Configurator", menuName = "CupOHappiness/Device Keybind Display/Configurator", order = 1)]
    public class DeviceDisplayConfigurator : ScriptableObject
    {
        public static DeviceDisplayConfigurator Instance { get; private set; }

        private void OnEnable() => Instance = this;

        [Serializable]
        public struct DeviceSet
        {
            [FormerlySerializedAs("deviceRawPath")]
            public string deviceLayout;
            public DeviceDisplaySettings deviceDisplaySettings;
        }

        [Serializable]
        public struct DisconnectedSettings
        {
            public string disconnectedDisplayName;
            public Color disconnectedDisplayColor;
        }

        public List<DeviceSet> listDeviceSets = new();
        public DisconnectedSettings disconnectedDeviceSettings;

        public bool TryGetDeviceSet(InputDevice device, out DeviceSet deviceSet) =>
            TryGetDeviceSet(device?.layout, out deviceSet);

        public bool TryGetDeviceSet(string layout, out DeviceSet deviceSet)
        {
            deviceSet = default;
            if (string.IsNullOrEmpty(layout) || listDeviceSets == null)
                return false;

            string bestLayout = null;
            foreach (var candidate in listDeviceSets)
            {
                if (string.IsNullOrEmpty(candidate.deviceLayout) || candidate.deviceDisplaySettings == null)
                    continue;
                if (!InputSystem.IsFirstLayoutBasedOnSecond(layout, candidate.deviceLayout))
                    continue;

                if (bestLayout == null || InputSystem.IsFirstLayoutBasedOnSecond(candidate.deviceLayout, bestLayout))
                {
                    bestLayout = candidate.deviceLayout;
                    deviceSet = candidate;
                }
            }

            return bestLayout != null;
        }

        /// <summary>Returns an optional presentation label for a canonical control or composite key.</summary>
        public string GetDeviceBindingLabel(string deviceLayout, string controlPath)
        {
            if (string.IsNullOrEmpty(controlPath) || !TryGetDeviceSet(deviceLayout, out var set) ||
                set.deviceDisplaySettings.customContextIcons == null)
                return null;
            foreach (var entry in set.deviceDisplaySettings.customContextIcons)
                if (string.Equals(entry.controlPath, controlPath, StringComparison.OrdinalIgnoreCase))
                    return entry.displayName;
            return null;
        }

        public Sprite GetDeviceBindingIcon(string deviceLayout, string controlPath)
        {
            if (string.IsNullOrEmpty(controlPath) || !TryGetDeviceSet(deviceLayout, out var set) ||
                !set.deviceDisplaySettings.deviceHasContextIcons)
                return null;
            return FilterForDeviceInputBinding(set.deviceDisplaySettings, controlPath);
        }

        public Sprite GetDeviceBindingIcon(InputDevice device, string controlPath) =>
            GetDeviceBindingIcon(device?.layout, controlPath);

        public Sprite GetDeviceBindingIcon(PlayerInput playerInput, string controlPath) =>
            GetDeviceBindingIcon(FirstDevice(playerInput), controlPath);

        public Sprite FilterForDeviceInputBinding(DeviceDisplaySettings settings, string controlPath)
        {
            if (settings == null || string.IsNullOrEmpty(controlPath))
                return null;

            // Explicit entries can override the standard gamepad slots.
            if (settings.customContextIcons != null)
                foreach (var entry in settings.customContextIcons)
                    if (string.Equals(entry.controlPath, controlPath, StringComparison.OrdinalIgnoreCase))
                        return entry.customInputContextIcon;

            return controlPath.ToLowerInvariant() switch
            {
                "buttonnorth" => settings.buttonNorthIcon,
                "buttonsouth" => settings.buttonSouthIcon,
                "buttonwest" => settings.buttonWestIcon,
                "buttoneast" => settings.buttonEastIcon,
                "rightshoulder" => settings.triggerRightFrontIcon,
                "righttrigger" => settings.triggerRightBackIcon,
                "leftshoulder" => settings.triggerLeftFrontIcon,
                "lefttrigger" => settings.triggerLeftBackIcon,
                _ => null
            };
        }

        public string GetDeviceName(PlayerInput playerInput)
        {
            var device = FirstDevice(playerInput);
            if (device == null)
                return string.IsNullOrEmpty(disconnectedDeviceSettings.disconnectedDisplayName)
                    ? "Unknown Device" : disconnectedDeviceSettings.disconnectedDisplayName;
            return TryGetDeviceSet(device, out var set) ? set.deviceDisplaySettings.deviceDisplayName : device.displayName;
        }

        public Color GetDeviceColor(PlayerInput playerInput)
        {
            var device = FirstDevice(playerInput);
            if (device == null)
                return disconnectedDeviceSettings.disconnectedDisplayColor;
            return TryGetDeviceSet(device, out var set) ? set.deviceDisplaySettings.deviceDisplayColor : Color.white;
        }

        private static InputDevice FirstDevice(PlayerInput playerInput) =>
            playerInput != null && playerInput.devices.Count > 0 ? playerInput.devices[0] : null;

        public string GetDisconnectedName() => disconnectedDeviceSettings.disconnectedDisplayName;
        public Color GetDisconnectedColor() => disconnectedDeviceSettings.disconnectedDisplayColor;
    }
}
