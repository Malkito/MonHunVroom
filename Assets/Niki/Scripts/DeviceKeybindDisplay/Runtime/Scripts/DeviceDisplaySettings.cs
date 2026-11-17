using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace CupOHappiness.DeviceKeybindDisplay.Runtime
{
    [System.Serializable]
    public struct CustomInputContextIcon
    {
        [FormerlySerializedAs("customInputContextString")]
        [Tooltip("Device-relative control path (e.g. leftStickPress), or a canonical composite key.")]
        public string controlPath;
        [Tooltip("Optional compact label, for example WASD or Arrow keys. This does not change binding resolution.")]
        public string displayName;
        public Sprite customInputContextIcon;
    }

    [CreateAssetMenu(fileName = "Device Display Settings", menuName = "CupOHappiness/Device Keybind Display/Settings", order = 1)]
    public class DeviceDisplaySettings : ScriptableObject
    {
        public string deviceDisplayName;
        public Color deviceDisplayColor;
        public bool deviceHasContextIcons;
        public Sprite buttonNorthIcon;
        public Sprite buttonSouthIcon;
        public Sprite buttonWestIcon;
        public Sprite buttonEastIcon;
        public Sprite triggerRightFrontIcon;
        public Sprite triggerRightBackIcon;
        public Sprite triggerLeftFrontIcon;
        public Sprite triggerLeftBackIcon;
        public List<CustomInputContextIcon> customContextIcons = new();
    }
}
