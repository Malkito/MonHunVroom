using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ParticleInstance))]
public class PrefabInstanceDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        //base.OnGUI(position, property, label);

        EditorGUI.BeginProperty(position, label, property);

        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

        Rect prefabRect = new Rect(position.x, position.y, position.width, position.height);

        EditorGUI.PropertyField(prefabRect, property.FindPropertyRelative("_effectPrefab"), GUIContent.none);

        EditorGUI.EndProperty();
    }
}
