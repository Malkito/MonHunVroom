using CupOHappiness.DeviceKeybindDisplay.Integrations.InputSystem;
using UnityEditor;

namespace CupOHappiness.DeviceKeybindDisplay.Editor
{
    [CustomEditor(typeof(InputBindingReader))]
    public class InputBindingReaderEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawPropertiesExcluding(serializedObject, "actionMapName", "actionName");

            var actionMapName = serializedObject.FindProperty("actionMapName");
            var actionName = serializedObject.FindProperty("actionName");

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Binding Target", EditorStyles.boldLabel);
            var playerInput = serializedObject.FindProperty("playerInput");
            InputActionsDropdownUtility.DrawActionMapAndActionDropdowns(null, playerInput, actionMapName, actionName);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
