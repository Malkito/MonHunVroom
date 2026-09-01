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
        CustomEditor(typeof(LayoutItem)),
        CanEditMultipleObjects
    ]
    public class LayoutItem_Editor : Editor
    {
        private LayoutItem _item;

        private SerializedProperty _log;
        private SerializedProperty _margin;
        private SerializedProperty _sizing;
        private SerializedProperty _percentage;
        private SerializedProperty _minSize;
        private SerializedProperty _maxSize;
        private SerializedProperty _isFloating;
        private SerializedProperty _attachTo;
        private SerializedProperty _attachPoints;
        private SerializedProperty _offset;
        private SerializedProperty _attachTarget;
        private SerializedProperty _floatingExpand;
        private SerializedProperty _bringToFront;

        protected virtual void OnEnable() {
            _item = target as LayoutItem;

            _log = serializedObject.FindProperty("m_log");
            _margin = serializedObject.FindProperty("m_margin");
            _sizing = serializedObject.FindProperty("m_sizing");
            _percentage = serializedObject.FindProperty("m_percentage");
            _minSize = serializedObject.FindProperty("m_minSize");
            _maxSize = serializedObject.FindProperty("m_maxSize");
            _isFloating = serializedObject.FindProperty("m_isFloating");
            _attachTo = serializedObject.FindProperty("m_attachTo");
            _attachPoints = serializedObject.FindProperty("m_attachPoints");
            _offset = serializedObject.FindProperty("m_offset");
            _attachTarget = serializedObject.FindProperty("m_attachTarget");
            _floatingExpand = serializedObject.FindProperty("m_floatingExpand");
            _bringToFront = serializedObject.FindProperty("m_bringToFront");
        }

        public override void OnInspectorGUI() {
            if(!_item)
                return;
            
            DrawProperty(_log);

            DrawProperty(_isFloating);
            bool isFloating = _isFloating.boolValue;
            if(_isFloating.boolValue) {
                DrawProperty(_attachTo);
                switch((LayoutItem.FloatingAttachTo)_attachTo.enumValueIndex) {
                    case LayoutItem.FloatingAttachTo.None:
                        EditorGUILayout.HelpBox("Attach To = None keeps this object outside normal layout flow and lets you place it manually with its RectTransform.", MessageType.Info);
                        break;
                    case LayoutItem.FloatingAttachTo.Parent:
                        EditorGUILayout.HelpBox("Attach To = Parent positions this object relative to its direct parent RectTransform.", MessageType.Info);
                        break;
                    case LayoutItem.FloatingAttachTo.Root:
                        EditorGUILayout.HelpBox("Attach To = Root positions this object relative to the root canvas, which is useful for screen-level overlays and HUD elements.", MessageType.Info);
                        break;
                    case LayoutItem.FloatingAttachTo.RectTransform:
                        EditorGUILayout.HelpBox("Attach To = RectTransform positions this object relative to another specific UI element. Assign that target in Attach Target.", MessageType.Info);
                        break;
                }
                if((LayoutItem.FloatingAttachTo)_attachTo.enumValueIndex == LayoutItem.FloatingAttachTo.RectTransform) {
                    DrawProperty(_attachTarget);
                }
                DrawProperty(_attachPoints);
                DrawProperty(_offset);
                DrawProperty(_floatingExpand);
                DrawProperty(_bringToFront);
            }
            
            EditorGUILayout.Space();
            DrawProperty(_margin);
            DrawProperty(_sizing);
            DrawProperty(_percentage);
            DrawProperty(_minSize);
            DrawProperty(_maxSize);

            if(serializedObject.hasModifiedProperties) {
                _item.SetDirty();
                serializedObject.ApplyModifiedProperties();
                EditorApplication.QueuePlayerLoopUpdate();
            }
        }

        protected void DrawProperty(SerializedProperty property) {
            EditorGUILayout.PropertyField(property, new GUIContent(property.displayName, property.tooltip), true);
        }
    }
}
