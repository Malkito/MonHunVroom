using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace CupOHappiness.UI
{
    [CustomEditor(typeof(TabButton)), CanEditMultipleObjects]
    public class TabButton_Editor : Editor
    {
        private SerializedProperty _group;
        private SerializedProperty _toggle;
        private SerializedProperty _onSelected;
        private SerializedProperty _onDeselected;

        private void OnEnable() {
            _group = serializedObject.FindProperty("m_group");
            _toggle = serializedObject.FindProperty("m_toggle");
            _onSelected = serializedObject.FindProperty("m_onSelected");
            _onDeselected = serializedObject.FindProperty("m_onDeselected");
        }

        public override void OnInspectorGUI() {
            serializedObject.Update();
            EditorGUILayout.PropertyField(_group);
            EditorGUILayout.PropertyField(_toggle);
            EditorGUILayout.PropertyField(_onSelected);
            EditorGUILayout.PropertyField(_onDeselected);

            if(serializedObject.ApplyModifiedProperties()) {
                foreach(Object selected in targets) {
                    if(selected is TabButton button && button.Group)
                        button.Group.Register(button);
                }
            }

            EditorGUILayout.HelpBox(
                "Input, navigation, interactable state, and visual transitions come from the standard Toggle component. Images and other graphics are optional.",
                MessageType.Info
            );

            if(GUILayout.Button("Repair Toggle")) {
                foreach(Object selected in targets) {
                    if(selected is not TabButton button)
                        continue;
                    Toggle toggle = button.GetComponent<Toggle>();
                    if(!toggle)
                        toggle = Undo.AddComponent<Toggle>(button.gameObject);
                    if(!toggle.targetGraphic)
                        toggle.targetGraphic = button.GetComponent<Graphic>();
                    if(button.Group)
                        button.Group.Register(button);
                    EditorUtility.SetDirty(button);
                }
                serializedObject.Update();
            }
        }
    }
}
