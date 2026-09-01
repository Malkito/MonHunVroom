/*
    Copyright (c) 2026 Alex Howe

    Permission is hereby granted, free of charge, to any person obtaining a copy
    of this software and associated documentation files (the "Software"), to deal
    in the Software without restriction, including without limitation the rights
    to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
    copies of the Software, and to permit persons to whom the Software is
    furnished to do so, subject to the following conditions:

    The above copyright notice and this permission notice shall be included in all
    copies or substantial portions of the Software.
*/
using UnityEditor;
using UnityEngine;

namespace CupOHappiness.UI
{
    [
        CustomEditor(typeof(Layout)),
        CanEditMultipleObjects
    ]
    public class Layout_Editor : LayoutItem_Editor
    {
        private Layout _layout;
        private SerializedProperty _padding;
        private SerializedProperty _direction;
        private SerializedProperty _justifyContent;
        private SerializedProperty _alignContent;
        private SerializedProperty _innerSpacing;
        private SerializedProperty _ignoreChildScale;
        
        protected override void OnEnable() {
            base.OnEnable();
            _layout = target as Layout;

            _padding = serializedObject.FindProperty("m_padding");
            _direction = serializedObject.FindProperty("m_direction");
            _justifyContent = serializedObject.FindProperty("m_justifyContent");
            _alignContent = serializedObject.FindProperty("m_alignContent");
            _innerSpacing = serializedObject.FindProperty("m_innerSpacing");
            _ignoreChildScale = serializedObject.FindProperty("m_ignoreChildScale");
        }

        public override void OnInspectorGUI() {
            base.OnInspectorGUI();

            if(!_layout)
                return;

            DrawProperty(_padding);
            DrawProperty(_direction);
            DrawProperty(_justifyContent);
            DrawProperty(_alignContent);

            if((Layout.Justification)_justifyContent.enumValueFlag == Layout.Justification.SpaceBetween) {
                GUI.enabled = false;
            }
            DrawProperty(_innerSpacing);
            GUI.enabled = true;
            
            DrawProperty(_ignoreChildScale);

            if(serializedObject.hasModifiedProperties) {
                _layout.SetDirty();
                serializedObject.ApplyModifiedProperties();
            }
            
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox(
                $"Tracking {_layout.ChildCount} layout elements.\nHorizontal Grow: {_layout.GrowChildCount.x}, Vertical Grow: {_layout.GrowChildCount.y}",
                MessageType.Info
            );
            if(GUILayout.Button("Refresh Child Cache")) {
                _layout.RefreshChildCache();
                EditorApplication.QueuePlayerLoopUpdate();
            }
        }
    }
}
