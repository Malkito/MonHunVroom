using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace LordBreakerX.States
{
    [CustomPropertyDrawer(typeof(State), true)]
    public class StateDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty stateProperty)
        {
            VisualElement rootElement = new VisualElement();

            foreach(SerializedProperty stateFieldProperty in stateProperty)
            {
                PropertyField field = new PropertyField(stateFieldProperty);
                rootElement.Add(field);
            }

            return rootElement;
        }
    }
}
