using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace CupOHappiness.UI
{
    [CustomEditor(typeof(TabGroup)), CanEditMultipleObjects]
    public class TabGroup_Editor : Editor
    {
        private TabGroup _group;
        private SerializedProperty _tabs;
        private SerializedProperty _initialIndex;
        private SerializedProperty _allowNoSelection;
        private SerializedProperty _navigation;
        private SerializedProperty _skipNonInteractable;
        private SerializedProperty _preserveSelection;
        private SerializedProperty _onSelectedIndexChanged;
        private SerializedProperty _toggleGroup;

        private void OnEnable() {
            _group = target as TabGroup;
            _tabs = serializedObject.FindProperty("m_tabs");
            _initialIndex = serializedObject.FindProperty("m_initialIndex");
            _allowNoSelection = serializedObject.FindProperty("m_allowNoSelection");
            _navigation = serializedObject.FindProperty("m_navigation");
            _skipNonInteractable = serializedObject.FindProperty("m_skipNonInteractable");
            _preserveSelection = serializedObject.FindProperty("m_preserveSelection");
            _onSelectedIndexChanged = serializedObject.FindProperty("m_onSelectedIndexChanged");
            _toggleGroup = serializedObject.FindProperty("m_toggleGroup");
        }

        public override void OnInspectorGUI() {
            serializedObject.Update();
            EditorGUILayout.PropertyField(_tabs, true);
            EditorGUILayout.PropertyField(_initialIndex);
            EditorGUILayout.PropertyField(_allowNoSelection);
            EditorGUILayout.PropertyField(_navigation);
            EditorGUILayout.PropertyField(_skipNonInteractable);
            EditorGUILayout.PropertyField(_preserveSelection);
            EditorGUILayout.PropertyField(_toggleGroup);
            EditorGUILayout.PropertyField(_onSelectedIndexChanged);

            if(serializedObject.ApplyModifiedProperties() && _group) {
                _group.RepairConfiguration();
                EditorUtility.SetDirty(_group);
            }

            EditorGUILayout.Space();
            using(new EditorGUILayout.HorizontalScope()) {
                if(GUILayout.Button("Collect Direct Child Tabs"))
                    CollectTabs();
                if(GUILayout.Button("Repair Toggle Setup"))
                    RepairSetup();
            }

            DrawValidation();
            if(Application.isPlaying && _group)
                EditorGUILayout.HelpBox($"Selected index: {_group.SelectedIndex}", MessageType.Info);
        }

        private void CollectTabs() {
            foreach(Object selected in targets) {
                if(selected is not TabGroup group)
                    continue;
                Undo.RecordObject(group, "Collect Tab Buttons");
                group.CollectDirectChildTabs();
                group.RepairConfiguration();
                EditorUtility.SetDirty(group);
                PrefabUtility.RecordPrefabInstancePropertyModifications(group);
            }
            serializedObject.Update();
        }

        private void RepairSetup() {
            foreach(Object selected in targets) {
                if(selected is not TabGroup group)
                    continue;

                if(!group.GetComponent<ToggleGroup>())
                    Undo.AddComponent<ToggleGroup>(group.gameObject);

                foreach(TabButton button in group.GetComponentsInChildren<TabButton>(true)) {
                    if(!button.GetComponent<Toggle>())
                        Undo.AddComponent<Toggle>(button.gameObject);
                }

                Undo.RecordObject(group, "Repair Tab Setup");
                group.RepairConfiguration();
                EditorUtility.SetDirty(group);
                PrefabUtility.RecordPrefabInstancePropertyModifications(group);
            }
            serializedObject.Update();
        }

        private void DrawValidation() {
            if(!_group)
                return;

            HashSet<TabButton> buttons = new();
            HashSet<GameObject> pages = new();
            for(int i = 0; i < _group.Tabs.Count; i++) {
                TabEntry entry = _group.Tabs[i];
                if(entry == null || !entry.Button) {
                    EditorGUILayout.HelpBox($"Tab entry {i} has no button.", MessageType.Warning);
                    continue;
                }
                if(!buttons.Add(entry.Button))
                    EditorGUILayout.HelpBox($"Tab button \"{entry.Button.name}\" is assigned more than once.", MessageType.Warning);
                if(!entry.Button.Toggle)
                    EditorGUILayout.HelpBox($"Tab button \"{entry.Button.name}\" needs a Toggle.", MessageType.Warning);
                if(entry.Button.Group && entry.Button.Group != _group)
                    EditorGUILayout.HelpBox($"Tab button \"{entry.Button.name}\" references another TabGroup.", MessageType.Warning);
                if(entry.Page && !pages.Add(entry.Page))
                    EditorGUILayout.HelpBox($"Page \"{entry.Page.name}\" is assigned more than once.", MessageType.Warning);
            }

            if(_group.Tabs.Count > 0 && _group.InitialIndex >= _group.Tabs.Count)
                EditorGUILayout.HelpBox("Initial Index is outside the tab list.", MessageType.Warning);
        }
    }
}
