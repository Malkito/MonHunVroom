using CupOHappiness.DeviceKeybindDisplay.Integrations.InputSystem;
using UnityEditor;

namespace CupOHappiness.DeviceKeybindDisplay.Editor
{
    [CustomEditor(typeof(InputBindingDisplayBase), true)]
    public class InputBindingDisplayEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            bool usesItems = serializedObject.FindProperty("bindingItemPrefab").objectReferenceValue != null;
            if (usesItems)
                DrawPropertiesExcluding(serializedObject, "actionMapName", "actionName",
                    "displayImage", "bindingNameText", "displayBindingName", "bindingNameExtraMargin");
            else
                DrawPropertiesExcluding(serializedObject, "actionMapName", "actionName");

            EditorGUILayout.HelpBox(
                "First In List selects the first alternative for each composite part. " +
                "Separate Icons shows one item per alternative group. Compact Text joins their labels. " +
                "The item container needs a horizontal Layout.",
                MessageType.Info);

            var inputActions = serializedObject.FindProperty("inputActions");
            var playerInput = serializedObject.FindProperty("playerInput");
            var actionMapName = serializedObject.FindProperty("actionMapName");
            var actionName = serializedObject.FindProperty("actionName");

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Binding Target", EditorStyles.boldLabel);
            InputActionsDropdownUtility.DrawActionMapAndActionDropdowns(inputActions, playerInput, actionMapName, actionName);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
