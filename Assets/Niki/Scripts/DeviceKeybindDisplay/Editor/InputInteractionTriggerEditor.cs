using CupOHappiness.DeviceKeybindDisplay.Integrations.InputSystem;
using UnityEditor;

namespace CupOHappiness.DeviceKeybindDisplay.Editor
{
    [CustomEditor(typeof(InputInteractionTrigger))]
    public class InputInteractionTriggerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawPropertiesExcluding(serializedObject, "actionMapName", "actionName");

            var inputActions = serializedObject.FindProperty("inputActions");
            var actionMapName = serializedObject.FindProperty("actionMapName");
            var actionName = serializedObject.FindProperty("actionName");

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Binding Target", EditorStyles.boldLabel);
            InputActionsDropdownUtility.DrawContextActionDropdown(inputActions, null, actionMapName, actionName);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
