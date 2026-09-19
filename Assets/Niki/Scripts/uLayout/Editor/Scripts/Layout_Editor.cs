/*
    Copyright (c) 2026 Nikita Mitrovich

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
        CustomEditor(typeof(Layout), true),
        CanEditMultipleObjects
    ]
    public class Layout_Editor : LayoutItem_Editor
    {
        private Layout _layout;
        private SerializedProperty _padding;
        private SerializedProperty _direction;
        private SerializedProperty _justifyContent;
        private SerializedProperty _alignContent;
        private SerializedProperty _alignItems;
        private SerializedProperty _innerSpacing;
        private SerializedProperty _ignoreChildScale;
        private SerializedProperty _wrap;
        private SerializedProperty _crossAxisGap;
        private SerializedProperty _reverse;
        
        protected override void OnEnable() {
            base.OnEnable();
            _layout = target as Layout;

            _padding = serializedObject.FindProperty("m_padding");
            _direction = serializedObject.FindProperty("m_direction");
            _justifyContent = serializedObject.FindProperty("m_justifyContent");
            _alignContent = serializedObject.FindProperty("m_alignContent");
            _alignItems = serializedObject.FindProperty("m_alignItems");
            _innerSpacing = serializedObject.FindProperty("m_innerSpacing");
            _ignoreChildScale = serializedObject.FindProperty("m_ignoreChildScale");
            _wrap = serializedObject.FindProperty("m_wrap");
            _crossAxisGap = serializedObject.FindProperty("m_crossAxisGap");
            _reverse = serializedObject.FindProperty("m_reverse");
        }

        public override void OnInspectorGUI() {
            base.OnInspectorGUI();

            if(!_layout)
                return;

            DrawProperty(_padding);
            // Flex presets lock direction to their Row/Column preset and expose
            // Reverse instead. Do not show a control that cannot be applied.
            if(_layout is FlexRow || _layout is FlexColumn) {
                if(_reverse != null)
                    DrawProperty(_reverse);
            }
            else {
                DrawProperty(_direction);
            }
            DrawProperty(_justifyContent);
            // Align Content only affects the distribution of multiple flex lines.
            // Keep it out of the inspector when wrapping is disabled.
            bool wrapEnabled = _wrap != null && (Layout.FlexWrap)_wrap.enumValueIndex != Layout.FlexWrap.NoWrap;
            if(wrapEnabled)
                DrawProperty(_alignContent);
            DrawProperty(_alignItems);

            if((Layout.Justification)_justifyContent.enumValueIndex != Layout.Justification.SpaceBetween)
                DrawProperty(_innerSpacing);
            
            DrawProperty(_ignoreChildScale);

            EditorGUILayout.Space();
            DrawProperty(_wrap);
            if(wrapEnabled)
                DrawProperty(_crossAxisGap);

            if(serializedObject.hasModifiedProperties) {
                serializedObject.ApplyModifiedProperties();
                _layout.SetDirty();
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

