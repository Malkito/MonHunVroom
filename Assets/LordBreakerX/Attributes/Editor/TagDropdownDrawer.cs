using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace LordBreakerX.Attributes
{
    [CustomPropertyDrawer(typeof(TagDropdownAttribute))]
    public class TagDropdownDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            List<string> tags = new List<string>(UnityEditorInternal.InternalEditorUtility.tags);

            if (property.propertyType == SerializedPropertyType.String)
            {
                int index = tags.IndexOf(property.stringValue);
                index = EditorGUI.Popup(position, label.text, index, tags.ToArray());
                if (index >= 0)
                {
                    property.stringValue = tags[index];
                }
            }
            else
            {
                Rect helpBoxRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight * 2);
                string errorMessage = $"{label.text} must be a string or string array for the TagDropdownAttribute to work.";
                EditorGUI.HelpBox(helpBoxRect, errorMessage, MessageType.Error);
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.String && !(property.isArray && property.propertyType == SerializedPropertyType.Generic))
            {
                return EditorGUIUtility.singleLineHeight * 2;
            }

            float height = EditorGUIUtility.singleLineHeight;
            if (property.isArray && property.propertyType == SerializedPropertyType.Generic)
            {
                height += EditorGUIUtility.singleLineHeight * property.arraySize;
            }
            return height;
        }
    }
}