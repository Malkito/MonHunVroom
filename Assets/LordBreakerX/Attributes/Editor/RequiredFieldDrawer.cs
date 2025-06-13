using UnityEditor;
using UnityEngine;

namespace LordBreakerX.Attributes
{
    [CustomPropertyDrawer(typeof(RequiredFieldAttribute))]
    public class RequiredFieldDrawer : PropertyDrawer
    {
        private static readonly float ERROR_BOX_HEIGHT = EditorGUIUtility.singleLineHeight * 2;
        private static readonly float PROPERTY_HEIGHT = EditorGUIUtility.singleLineHeight * 1;
        private static readonly float SPACING_HEIGHT = EditorGUIUtility.standardVerticalSpacing;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Rect propertyRect = new Rect(position.x, position.y, position.width, PROPERTY_HEIGHT);

            if (ShouldShowError(property))
            {
                Rect helpBoxRect = new Rect(position.x, position.y, position.width, ERROR_BOX_HEIGHT);
                EditorGUI.HelpBox(helpBoxRect, $"{property.displayName} is an required field!", MessageType.Error);
                propertyRect = new Rect(propertyRect.x, propertyRect.y + ERROR_BOX_HEIGHT + SPACING_HEIGHT, propertyRect.width, propertyRect.height);
            }
            EditorGUI.PropertyField(propertyRect, property, label);
        }

        private bool ShouldShowError(SerializedProperty property)
        {
            switch (property.propertyType)
            {
                case SerializedPropertyType.ObjectReference:
                    return property.objectReferenceValue == null;
                default:
                    return false;
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (ShouldShowError(property)) return ERROR_BOX_HEIGHT + PROPERTY_HEIGHT + SPACING_HEIGHT;
            else return PROPERTY_HEIGHT;
        }

    }
}