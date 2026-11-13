using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace CupOHappiness.DeviceKeybindDisplay.Editor
{
    public sealed class ControllerLayoutBindingsWindow : EditorWindow
    {
        private string layoutName = "SwitchProControllerHID";

        [MenuItem("Tools/Device Keybind Display/Inspect Controller Bindings")]
        private static void Open() => GetWindow<ControllerLayoutBindingsWindow>("Controller Bindings");

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Input System controller layout", EditorStyles.boldLabel);
            layoutName = EditorGUILayout.TextField("Layout", layoutName);
            EditorGUILayout.HelpBox("Enter a registered layout, for example SwitchProControllerHID.", MessageType.Info);
            if (GUILayout.Button("Log available controls"))
                LogControls();
        }

        private void LogControls()
        {
            if (string.IsNullOrWhiteSpace(layoutName) ||
                !InputSystem.ListLayouts().Contains(layoutName, StringComparer.OrdinalIgnoreCase))
            {
                Debug.LogError($"Input System layout '{layoutName}' is not registered.");
                return;
            }

            if (!InputSystem.IsFirstLayoutBasedOnSecond(layoutName, "Gamepad"))
            {
                Debug.LogError($"Layout '{layoutName}' is not based on Gamepad.");
                return;
            }

            InputDevice device = null;
            try
            {
                device = InputSystem.AddDevice(layoutName);
                var controls = device.allControls
                    .Where(control => control is ButtonControl or StickControl or DpadControl)
                    .Select(control => $"{control.path.Substring(device.path.Length).TrimStart('/')} — {control.displayName} ({control.GetType().Name})")
                    .ToArray();
                Debug.Log($"Available controls for {layoutName}:\n{string.Join("\n", controls)}");
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
            finally
            {
                if (device != null && device.added)
                    InputSystem.RemoveDevice(device);
            }
        }
    }
}
