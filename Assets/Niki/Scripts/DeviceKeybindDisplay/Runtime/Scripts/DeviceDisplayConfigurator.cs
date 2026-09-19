using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CupOHappiness.DeviceKeybindDisplay.Runtime
{
    [CreateAssetMenu(fileName = "Device Display Configurator", menuName = "CupOHappiness/Device Keybind Display/Configurator", order = 1)]
    public class DeviceDisplayConfigurator : ScriptableObject
    {
        public static DeviceDisplayConfigurator Instance { get; private set; }

        private void OnEnable()
        {
            Instance = this;
        }

        [System.Serializable]
        public struct DeviceSet
        {
            public string deviceRawPath;
            public DeviceDisplaySettings deviceDisplaySettings;
        }

        [System.Serializable]
        public struct DisconnectedSettings
        {
            public string disconnectedDisplayName;
            public Color disconnectedDisplayColor;
        }

        public List<DeviceSet> listDeviceSets = new List<DeviceSet>();
        public DisconnectedSettings disconnectedDeviceSettings;

        private Color fallbackDisplayColor = Color.white;

        private static readonly Dictionary<string, System.Func<DeviceDisplaySettings, Sprite>> IconLookup = new(System.StringComparer.OrdinalIgnoreCase)
        {
            { "Button North", s => s.buttonNorthIcon }, { "Y", s => s.buttonNorthIcon }, { "Triangle", s => s.buttonNorthIcon }, { "North", s => s.buttonNorthIcon },
            { "Button South", s => s.buttonSouthIcon }, { "A", s => s.buttonSouthIcon }, { "Cross", s => s.buttonSouthIcon }, { "South", s => s.buttonSouthIcon }, { "Submit", s => s.buttonSouthIcon },
            { "Button West", s => s.buttonWestIcon }, { "X", s => s.buttonWestIcon }, { "Square", s => s.buttonWestIcon }, { "West", s => s.buttonWestIcon },
            { "Button East", s => s.buttonEastIcon }, { "B", s => s.buttonEastIcon }, { "Circle", s => s.buttonEastIcon }, { "East", s => s.buttonEastIcon }, { "Cancel", s => s.buttonEastIcon }, { "Back", s => s.buttonEastIcon },
            { "Right Shoulder", s => s.triggerRightFrontIcon }, { "RB", s => s.triggerRightFrontIcon }, { "R1", s => s.triggerRightFrontIcon },
            { "Left Shoulder", s => s.triggerLeftFrontIcon }, { "LB", s => s.triggerLeftFrontIcon }, { "L1", s => s.triggerLeftFrontIcon },
            { "Right Trigger", s => s.triggerRightBackIcon }, { "RT", s => s.triggerRightBackIcon }, { "R2", s => s.triggerRightBackIcon }, { "trigger", s => s.triggerRightBackIcon },
            { "Left Trigger", s => s.triggerLeftBackIcon }, { "LT", s => s.triggerLeftBackIcon }, { "L2", s => s.triggerLeftBackIcon },
            { "Enter", s => s.buttonSouthIcon }, { "Return", s => s.buttonSouthIcon }, { "Escape", s => s.buttonEastIcon }
        };

        private static readonly Dictionary<string, string> SonyMap = new(System.StringComparer.OrdinalIgnoreCase) { ["{Back}"] = "Circle", ["{Cancel}"] = "Circle", ["{Menu}"] = "Options", ["{PrimaryAction}"] = "Cross", ["{SecondaryAction}"] = "Square", ["{Submit}"] = "Cross", ["{Hatswitch}"] = "D-Pad", ["{Primary2DMotion}"] = "Left Stick", ["{Secondary2DMotion}"] = "Right Stick", ["{SecondaryTrigger}"] = "L2/R2" };
        private static readonly Dictionary<string, string> XboxMap = new(System.StringComparer.OrdinalIgnoreCase) { ["{Back}"] = "B", ["{Cancel}"] = "B", ["{Menu}"] = "Menu", ["{PrimaryAction}"] = "A", ["{SecondaryAction}"] = "X", ["{Submit}"] = "A", ["{Hatswitch}"] = "D-Pad", ["{Primary2DMotion}"] = "Left Stick", ["{Secondary2DMotion}"] = "Right Stick", ["{SecondaryTrigger}"] = "LT/RT" };
        private static readonly Dictionary<string, string> MouseMap = new(System.StringComparer.OrdinalIgnoreCase) { ["{Back}"] = "Back", ["{Forward}"] = "Forward", ["{Point}"] = "Position", ["{PrimaryAction}"] = "Left Button", ["{SecondaryAction}"] = "Right Button", ["{ScrollHorizontal}"] = "Scroll Left/Right", ["{ScrollVertical}"] = "Scroll Up/Down", ["{Secondary2DMotion}"] = "Delta" };

        private static readonly Dictionary<string, Dictionary<string, string>> WildcardResolutionTable = new(System.StringComparer.OrdinalIgnoreCase)
        {
            ["Keyboard"] = new(System.StringComparer.OrdinalIgnoreCase) { ["{Back}"] = "Escape", ["{Cancel}"] = "Escape", ["{Submit}"] = "Enter" },
            ["DualSenseGamepadHID"] = SonyMap,
            ["DualSenseGampadiOS"] = SonyMap,
            ["DualShock3GamepadHID"] = SonyMap,
            ["DualShock4GamepadAndroid"] = SonyMap,
            ["DualShock4GamepadHID"] = SonyMap,
            ["DualShock4GampadiOS"] = SonyMap,
            ["DualShockGamepad"] = SonyMap,
            ["SwitchProControllerHID"] = new(System.StringComparer.OrdinalIgnoreCase) { ["{Back}"] = "B", ["{Cancel}"] = "B", ["{Menu}"] = "Plus", ["{PrimaryAction}"] = "A", ["{SecondaryAction}"] = "Y", ["{Submit}"] = "A", ["{Hatswitch}"] = "D-Pad", ["{Primary2DMotion}"] = "Left Stick", ["{Secondary2DMotion}"] = "Right Stick", ["{SecondaryTrigger}"] = "ZL/ZR" },
            ["XboxOneGamepadAndroid"] = XboxMap,
            ["XboxOneGampadiOS"] = XboxMap,
            ["XInputController"] = XboxMap,
            ["XInputControllerWindows"] = XboxMap,
            ["Gamepad"] = new(System.StringComparer.OrdinalIgnoreCase) { ["{Back}"] = "Button East", ["{Cancel}"] = "Button East", ["{Menu}"] = "Start", ["{PrimaryAction}"] = "Button South", ["{SecondaryAction}"] = "Button West", ["{Submit}"] = "Button South", ["{Hatswitch}"] = "D-Pad", ["{Primary2DMotion}"] = "Left Stick", ["{Secondary2DMotion}"] = "Right Stick", ["{SecondaryTrigger}"] = "LT/RT" },
            ["Mouse"] = MouseMap,
            ["VirtualMouse"] = MouseMap,
            ["Touchscreen"] = new(System.StringComparer.OrdinalIgnoreCase) { ["{Point}"] = "Position", ["{PrimaryAction}"] = "Primary Touch Tap", ["{Secondary2DMotion}"] = "Delta" }
        };

        public bool TryResolveWildcardForDevice(string deviceLayout, string wildcard, out string resolution)
        {
            resolution = null;
            if (string.IsNullOrEmpty(deviceLayout) || string.IsNullOrEmpty(wildcard)) return false;

            if (WildcardResolutionTable.TryGetValue(deviceLayout, out var resolutionMap))
            {
                if (resolutionMap.TryGetValue(wildcard, out resolution)) return true;
                if (!wildcard.StartsWith("{") && resolutionMap.TryGetValue("{" + wildcard + "}", out resolution)) return true;
            }

            // Fallback to base categories
            if (InputSystem.IsFirstLayoutBasedOnSecond(deviceLayout, "Gamepad") && WildcardResolutionTable.TryGetValue("Gamepad", out var gpMap))
            {
                if (gpMap.TryGetValue(wildcard, out resolution)) return true;
            }

            return false;
        }

        public string GetDeviceName(PlayerInput playerInput)
        {
            if (playerInput == null || playerInput.devices.Count == 0 || playerInput.devices[0] == null)
            {
                return string.IsNullOrEmpty(disconnectedDeviceSettings.disconnectedDisplayName) ? "Unknown Device" : disconnectedDeviceSettings.disconnectedDisplayName;
            }

            if (TryGetDeviceSet(playerInput.devices[0], out var deviceSet))
            {
                return deviceSet.deviceDisplaySettings != null ? deviceSet.deviceDisplaySettings.deviceDisplayName : playerInput.devices[0].ToString();
            }

            return playerInput.devices[0].ToString();
        }

        public Color GetDeviceColor(PlayerInput playerInput)
        {
            if (playerInput == null || playerInput.devices.Count == 0 || playerInput.devices[0] == null)
            {
                return disconnectedDeviceSettings.disconnectedDisplayColor;
            }

            if (TryGetDeviceSet(playerInput.devices[0], out var deviceSet))
            {
                return deviceSet.deviceDisplaySettings != null ? deviceSet.deviceDisplaySettings.deviceDisplayColor : fallbackDisplayColor;
            }

            return fallbackDisplayColor;
        }

        public Sprite GetDeviceBindingIcon(InputDevice device, string playerInputDeviceInputBinding)
        {
            if (device == null)
            {
                return null;
            }

            if (TryGetDeviceSet(device, out var deviceSet) && deviceSet.deviceDisplaySettings?.deviceHasContextIcons == true)
            {
                return FilterForDeviceInputBinding(deviceSet.deviceDisplaySettings, playerInputDeviceInputBinding);
            }

            return null;
        }

        public Sprite GetDeviceBindingIcon(PlayerInput playerInput, string playerInputDeviceInputBinding)
        {
            if (playerInput == null || playerInput.devices.Count == 0 || playerInput.devices[0] == null)
            {
                return null;
            }

            if (TryGetDeviceSet(playerInput.devices[0], out var deviceSet))
            {
                if (deviceSet.deviceDisplaySettings?.deviceHasContextIcons == true)
                {
                    return FilterForDeviceInputBinding(deviceSet.deviceDisplaySettings, playerInputDeviceInputBinding);
                }
            }

            return null;
        }

        public bool TryGetDeviceSet(InputDevice device, out DeviceSet deviceSet)
        {
            deviceSet = default;
            if (device == null || listDeviceSets == null) return false;

            string deviceRawPath = device.ToString();
            string categoryPath = null;
            if (device is Gamepad) categoryPath = "Gamepad:/Gamepad";
            else if (device is Keyboard) categoryPath = "Keyboard:/Keyboard";
            else if (device is Mouse) categoryPath = "Mouse:/Mouse";
            else if (device is Touchscreen) categoryPath = "Touchscreen:/Touchscreen";

            DeviceSet? genericMatch = null;
            DeviceSet? specificMatch = null;

            for (int i = 0; i < listDeviceSets.Count; i++)
            {
                var set = listDeviceSets[i];
                if (set.deviceRawPath == deviceRawPath) specificMatch = set;
                if (categoryPath != null && set.deviceRawPath == categoryPath) genericMatch = set;
            }

            if (specificMatch.HasValue)
            {
                deviceSet = specificMatch.Value;
                return true;
            }

            if (genericMatch.HasValue)
            {
                deviceSet = genericMatch.Value;
                return true;
            }

            return false;
        }

        public bool TryGetGenericDeviceSet(InputDevice device, out DeviceSet deviceSet)
        {
            deviceSet = default;
            if (device == null || listDeviceSets == null) return false;

            string categoryPath = null;
            if (device is Gamepad) categoryPath = "Gamepad:/Gamepad";
            else if (device is Keyboard) categoryPath = "Keyboard:/Keyboard";
            else if (device is Mouse) categoryPath = "Mouse:/Mouse";
            else if (device is Touchscreen) categoryPath = "Touchscreen:/Touchscreen";

            if (categoryPath == null) return false;

            for (int i = 0; i < listDeviceSets.Count; i++)
            {
                if (listDeviceSets[i].deviceRawPath == categoryPath)
                {
                    deviceSet = listDeviceSets[i];
                    return true;
                }
            }
            return false;
        }

        public Sprite FilterForDeviceInputBinding(DeviceDisplaySettings settings, string inputBinding)
        {
            if (settings == null) return null;

            Sprite spriteIcon = null;
            if (IconLookup.TryGetValue(inputBinding, out var resolver))
            {
                spriteIcon = resolver(settings);
            }

            if (spriteIcon != null) return spriteIcon;

            if (settings.customContextIcons != null)
            {
                for (int i = 0; i < settings.customContextIcons.Count; i++)
                {
                    var entry = settings.customContextIcons[i];
                    if (string.Equals(entry.customInputContextString, inputBinding, System.StringComparison.OrdinalIgnoreCase))
                    {
                        return entry.customInputContextIcon;
                    }
                }
            }

            return null;
        }

        public string GetDisconnectedName() { return disconnectedDeviceSettings.disconnectedDisplayName; }
        public Color GetDisconnectedColor() { return disconnectedDeviceSettings.disconnectedDisplayColor; }
    }
}
