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
    [CustomEditor(typeof(GridLayout)), CanEditMultipleObjects]
    public class GridLayout_Editor : LayoutItem_Editor
    {
        private GridLayout _grid;
        private SerializedProperty _columnTracks;
        private SerializedProperty _rowTracks;
        private SerializedProperty _padding;
        private SerializedProperty _gap;
        private SerializedProperty _childAlignmentX;
        private SerializedProperty _childAlignmentY;

        protected override void OnEnable() {
            base.OnEnable();
            _grid = target as GridLayout;
            _columnTracks = serializedObject.FindProperty("m_columnTracks");
            _rowTracks = serializedObject.FindProperty("m_rowTracks");
            _padding = serializedObject.FindProperty("m_padding");
            _gap = serializedObject.FindProperty("m_gap");
            _childAlignmentX = serializedObject.FindProperty("m_childAlignmentX");
            _childAlignmentY = serializedObject.FindProperty("m_childAlignmentY");
        }

        public override void OnInspectorGUI() {
            base.OnInspectorGUI();
            if(!_grid)
                return;

            EditorGUILayout.Space();
            DrawTrackDefinition(
                _columnTracks,
                "Columns",
                "Columns place direct children from left to right. Auto Fit removes empty columns. Auto Fill keeps empty columns that fit in the available width."
            );
            DrawTrackDefinition(
                _rowTracks,
                "Rows",
                "Rows use the resolved column count. Fixed Count keeps empty rows and adds rows when child flow needs them. Auto Fit and Auto Fill follow child flow."
            );
            DrawProperty(_gap);
            DrawProperty(_padding);
            DrawChildAlignment();

            if(serializedObject.hasModifiedProperties) {
                serializedObject.ApplyModifiedProperties();
                foreach(Object targetObject in targets) {
                    if(targetObject is GridLayout grid)
                        grid.SetDirty();
                }
                EditorApplication.QueuePlayerLoopUpdate();
            }

            EditorGUILayout.Space();
            DrawFixedRowOverflowWarning();
            DrawLiveMeasurements();
        }

        private void DrawChildAlignment() {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Child Alignment", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Sets the position of children that do not fill their grid cell.",
                MessageType.Info
            );
            EditorGUILayout.PropertyField(_childAlignmentX, new GUIContent("Horizontal"));
            EditorGUILayout.PropertyField(_childAlignmentY, new GUIContent("Vertical"));
        }

        private void DrawFixedRowOverflowWarning() {
            foreach(Object targetObject in targets) {
                if(targetObject is not GridLayout grid)
                    continue;

                GridTrackDef rows = grid.RowTracks;
                if(rows.mode == GridTrackMode.FixedCount && grid.ResolvedRows > rows.count) {
                    EditorGUILayout.HelpBox(
                        "The current active child count does not fit the fixed count configuration.",
                        MessageType.Warning
                    );
                    return;
                }
            }
        }

        private void DrawLiveMeasurements() {
            GridLayout[] grids = new GridLayout[targets.Length];
            int count = 0;
            foreach(Object targetObject in targets) {
                if(targetObject is GridLayout grid)
                    grids[count++] = grid;
            }

            if(count == 0)
                return;

            string targetLabel = count == 1
                ? $"Target: {grids[0].name}"
                : $"Targets: {count} selected (values marked Mixed differ)";
            EditorGUILayout.LabelField("Live resolved measurements", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                $"{targetLabel}\n" +
                $"Active children: {FormatMixedInt(grids, count, grid => grid.ActiveChildCount)}\n" +
                $"Resolved rows: {FormatMixedInt(grids, count, grid => grid.ResolvedRows)}\n" +
                $"Resolved columns: {FormatMixedInt(grids, count, grid => grid.ResolvedColumns)}\n" +
                $"Column width: {FormatMixedFloat(grids, count, grid => grid.ColumnWidth)} pixels\n" +
                $"Row height: {FormatMixedFloat(grids, count, grid => grid.RowHeight)} pixels",
                MessageType.Info
            );
        }

        private static string FormatMixedInt(GridLayout[] grids, int count, System.Func<GridLayout, int> selector) {
            int value = selector(grids[0]);
            for(int i = 1; i < count; i++) {
                if(selector(grids[i]) != value)
                    return "Mixed";
            }
            return value.ToString();
        }

        private static string FormatMixedFloat(GridLayout[] grids, int count, System.Func<GridLayout, float> selector) {
            float value = selector(grids[0]);
            for(int i = 1; i < count; i++) {
                if(!Mathf.Approximately(selector(grids[i]), value))
                    return "Mixed";
            }
            return value.ToString("0.##");
        }

        private void DrawTrackDefinition(SerializedProperty track, string title, string description) {
            EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(description, MessageType.Info);

            SerializedProperty mode = track.FindPropertyRelative("mode");
            SerializedProperty count = track.FindPropertyRelative("count");
            SerializedProperty minTrackSize = track.FindPropertyRelative("minTrackSize");
            SerializedProperty fillExtraSpace = track.FindPropertyRelative("fillExtraSpace");

            DrawProperty(mode);
            bool mixedMode = mode.hasMultipleDifferentValues;
            GridTrackMode selectedMode = (GridTrackMode)mode.enumValueIndex;
            if(mixedMode || GridTrackResolver.UsesCount(selectedMode))
                DrawProperty(count);
            if(mixedMode || GridTrackResolver.UsesMinTrackSize(selectedMode))
                DrawProperty(minTrackSize);
            if(mixedMode || GridTrackResolver.UsesFillExtraSpace(selectedMode)) {
                EditorGUILayout.PropertyField(
                    fillExtraSpace,
                    new GUIContent(
                        "Fill Extra Space",
                        "Set to 0 to keep tracks at their minimum size. Set above 0 to stretch tracks evenly and use free space."
                    ),
                    true
                );
            }
        }
    }
}

