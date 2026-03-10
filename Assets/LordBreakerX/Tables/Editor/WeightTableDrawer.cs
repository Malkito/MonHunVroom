#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace LordBreakerX.Tables
{

    [CustomPropertyDrawer(typeof(WeightTable<>), true)]
    public class WeightTableDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            VisualElement root = new VisualElement();

            PropertyField weightedEntries = new PropertyField(property.FindPropertyRelative("_weightedEntries"), property.displayName);
            
            root.Add(weightedEntries);

            return root;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            int indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            Rect weightedEntriesRect = new Rect(position.x, position.y, position.width, position.height);

            EditorGUI.PropertyField(weightedEntriesRect, property.FindPropertyRelative("_weightedEntries"), label);

            EditorGUI.indentLevel = indent;

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            SerializedProperty weightedEntries = property.FindPropertyRelative("_weightedEntries");

            return EditorGUI.GetPropertyHeight(weightedEntries, label, true);
        }
    }
}
#endif