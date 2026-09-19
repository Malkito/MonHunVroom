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

            DrawPropertiesExcluding(serializedObject, "actionMapName", "actionName");

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
