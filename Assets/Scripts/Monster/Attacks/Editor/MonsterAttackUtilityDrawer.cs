using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

[CustomPropertyDrawer(typeof(MonsterAbilityUtility))]
public class MonsterAttackUtilityDrawer : PropertyDrawer
{
    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        VisualElement root = new VisualElement();

        PropertyField rangeField = new PropertyField(property.FindPropertyRelative("_targetRange"), "Random Target Range");

        root.Add(rangeField);

        return root;
    }
}
