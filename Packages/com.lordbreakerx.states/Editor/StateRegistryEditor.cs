using UnityEditor;
using UnityEngine.UIElements;

namespace LordBreakerX.States
{
    [CustomEditor(typeof(StatesAsset))]
    public class StateRegistryEditor : Editor
    {
        private Button _editButton;

        private void OnEnable()
        {
            EditorApplication.projectChanged += OnProjectChanged;
        }

        private void OnDisable()
        {
            EditorApplication.projectChanged -= OnProjectChanged;
        }

        private void OnProjectChanged()
        {
            _editButton.text = $"Edit {target.name}";
        }

        public override VisualElement CreateInspectorGUI()
        {
            VisualElement rootElement = new VisualElement();

            _editButton = new Button(OnEdit);

            _editButton.text = $"Edit {target.name}";
            _editButton.style.fontSize = 20;

            rootElement.Add(_editButton);
            return rootElement;
        }

        private void OnEdit()
        {
            StateEditorWindow.Open(target as StatesAsset);   
        }
    }
}
