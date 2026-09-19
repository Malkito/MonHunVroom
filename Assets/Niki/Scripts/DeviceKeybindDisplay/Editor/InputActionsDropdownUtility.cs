using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine.InputSystem;

namespace CupOHappiness.DeviceKeybindDisplay.Editor
{
    internal static class InputActionsDropdownUtility
    {
        internal static void DrawActionMapAndActionDropdowns(
            SerializedProperty inputActionsProperty,
            SerializedProperty playerInputProperty,
            SerializedProperty actionMapProperty,
            SerializedProperty actionNameProperty)
        {
            var asset = GetReferencedAsset(inputActionsProperty, playerInputProperty);
            if (asset == null)
            {
                EditorGUILayout.HelpBox(
                    "Assign an InputActionAsset or PlayerInput to populate the action dropdowns.",
                    MessageType.Info);
                EditorGUILayout.PropertyField(actionMapProperty);
                EditorGUILayout.PropertyField(actionNameProperty);
                return;
            }

            var maps = asset.actionMaps;
            if (maps.Count == 0)
            {
                EditorGUILayout.PropertyField(actionMapProperty);
                EditorGUILayout.PropertyField(actionNameProperty);
                return;
            }

            string[] mapNames = maps.Select(map => map.name).ToArray();
            int mapIndex = System.Array.IndexOf(mapNames, actionMapProperty.stringValue);
            if (mapIndex < 0)
            {
                mapIndex = 0;
            }

            mapIndex = EditorGUILayout.Popup("Action Map", mapIndex, mapNames);
            actionMapProperty.stringValue = mapNames[mapIndex];

            var actions = maps[mapIndex].actions;
            if (actions.Count == 0)
            {
                actionNameProperty.stringValue = string.Empty;
                EditorGUILayout.LabelField("Action", "No actions");
                return;
            }

            string[] actionNames = actions.Select(action => action.name).ToArray();
            int actionIndex = System.Array.IndexOf(actionNames, actionNameProperty.stringValue);
            if (actionIndex < 0)
            {
                actionIndex = 0;
            }

            actionIndex = EditorGUILayout.Popup("Action", actionIndex, actionNames);
            actionNameProperty.stringValue = actionNames[actionIndex];
        }

        internal static void DrawContextActionDropdown(
            SerializedProperty inputActionsProperty,
            SerializedProperty playerInputProperty,
            SerializedProperty actionMapProperty,
            SerializedProperty actionNameProperty,
            string label = "Action")
        {
            var asset = GetReferencedAsset(inputActionsProperty, playerInputProperty);
            if (asset == null)
            {
                EditorGUILayout.HelpBox(
                    "Assign an InputActionAsset or PlayerInput to populate the action dropdowns.",
                    MessageType.Info);
                EditorGUILayout.PropertyField(actionMapProperty);
                EditorGUILayout.PropertyField(actionNameProperty);
                return;
            }

            var options = new List<string>();
            foreach (var map in asset.actionMaps)
            {
                foreach (var action in map.actions)
                {
                    options.Add($"{map.name} / {action.name}");
                }
            }

            if (options.Count == 0)
            {
                EditorGUILayout.PropertyField(actionMapProperty);
                EditorGUILayout.PropertyField(actionNameProperty);
                return;
            }

            string current = $"{actionMapProperty.stringValue} / {actionNameProperty.stringValue}";
            int index = options.IndexOf(current);
            if (index < 0)
            {
                index = 0;
            }

            index = EditorGUILayout.Popup(label, index, options.ToArray());
            var selected = options[index].Split(new[] { " / " }, System.StringSplitOptions.None);
            actionMapProperty.stringValue = selected[0];
            actionNameProperty.stringValue = selected[1];
        }

        private static InputActionAsset GetReferencedAsset(
            SerializedProperty inputActionsProperty,
            SerializedProperty playerInputProperty)
        {
            if (inputActionsProperty?.objectReferenceValue is InputActionAsset inputActions)
            {
                return inputActions;
            }

            if (playerInputProperty?.objectReferenceValue is PlayerInput playerInput)
            {
                return playerInput.actions;
            }

            return null;
        }
    }
}
