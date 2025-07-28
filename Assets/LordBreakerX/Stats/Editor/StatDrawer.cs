using LordBreakerX.Stats;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(Stat))]
public class StatDrawer : PropertyDrawer
{
    private const string FoldoutKey = "StatDrawerFoldout_";
    private static readonly Dictionary<string, bool> foldoutStates = new();

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        string key = GetFoldoutKey(property);
        bool expanded = GetFoldoutState(key);

        if (!expanded)
            return EditorGUIUtility.singleLineHeight;

        var statType = (StatType)property.FindPropertyRelative("_statType").enumValueIndex;
        int lines = 1; // statType only now (removed id line)

        switch (statType)
        {
            case StatType.Static:
                lines += 1;
                break;
            case StatType.Range:
                lines += 2;
                break;
            case StatType.Curve:
                lines += 1;
                break;
        }

        return EditorGUIUtility.singleLineHeight * (lines + 1) + EditorGUIUtility.standardVerticalSpacing * (lines);
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        string key = GetFoldoutKey(property);
        bool expanded = GetFoldoutState(key);

        Rect foldoutRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
        expanded = EditorGUI.Foldout(foldoutRect, expanded, label, true);
        SetFoldoutState(key, expanded);

        if (!expanded)
            return;

        EditorGUI.indentLevel++;
        float lineHeight = EditorGUIUtility.singleLineHeight;
        float spacing = EditorGUIUtility.standardVerticalSpacing;
        float y = position.y + lineHeight + spacing;

        SerializedProperty typeProp = property.FindPropertyRelative("_statType");
        SerializedProperty valProp = property.FindPropertyRelative("_value");
        SerializedProperty minProp = property.FindPropertyRelative("_minValue");
        SerializedProperty maxProp = property.FindPropertyRelative("_maxValue");
        SerializedProperty curveProp = property.FindPropertyRelative("_curveValue");

        Rect line = new Rect(position.x, y, position.width, lineHeight);
        EditorGUI.PropertyField(line, typeProp);
        y += lineHeight + spacing;

        StatType statType = (StatType)typeProp.enumValueIndex;

        if (statType == StatType.Static)
        {
            line.y = y;
            EditorGUI.PropertyField(line, valProp);
        }
        else if (statType == StatType.Range)
        {
            line.y = y;
            EditorGUI.PropertyField(line, minProp);
            line.y += lineHeight + spacing;
            EditorGUI.PropertyField(line, maxProp);
        }
        else if (statType == StatType.Curve)
        {
            line.y = y;
            EditorGUI.PropertyField(line, curveProp);
        }

        EditorGUI.indentLevel--;
    }

    private string GetFoldoutKey(SerializedProperty property)
    {
        return FoldoutKey + property.propertyPath;
    }

    private bool GetFoldoutState(string key)
    {
        if (!foldoutStates.TryGetValue(key, out bool value))
            value = true;

        return value;
    }

    private void SetFoldoutState(string key, bool value)
    {
        foldoutStates[key] = value;
    }
}
