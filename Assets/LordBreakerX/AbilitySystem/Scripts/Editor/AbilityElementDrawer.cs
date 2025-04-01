using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace LordBreakerX.AbilitySystem
{
    [CustomPropertyDrawer(typeof(AbilityElement))]
    public class AbilityElementDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            VisualElement root = new VisualElement();

            PropertyField idField = new PropertyField(property.FindPropertyRelative("_id"), "ID");
            PropertyField abilityField = new PropertyField(property.FindPropertyRelative("_ability"), "Ability");

            root.Add(idField);
            root.Add(abilityField);

            return root;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }
    }
}
