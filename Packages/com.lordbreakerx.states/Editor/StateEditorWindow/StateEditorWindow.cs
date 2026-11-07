using LordBreakerX.EditorUtilities;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace LordBreakerX.States
{
    public class StateEditorWindow : EditorWindow
    {
        private StatesAsset _asset;

        private TwoPaneSplitView _splitView;

        private List<StatesAsset> _availableAssets = new List<StatesAsset>();

        private PopupField<StatesAsset> _assetSelector;

        private ObjectPanel<StateReference> _objectPanel;

        private StatesPanel _statesPanel;

        public StatesAsset Asset { get => _asset; }

        [MenuItem("Window/State Editor")]
        public static void Open() => Open(null);

        public static void Open(StatesAsset asset)
        {
            StateEditorWindow editorWindow = GetWindow<StateEditorWindow>();

            editorWindow.titleContent = new GUIContent($"State Editor");
            editorWindow.SetAsset(asset);
        }

        private void CreateGUI()
        {
            UpdateAvilableAssets();

            Toolbar toolbar = new Toolbar();
            
            _assetSelector = new PopupField<StatesAsset>(_availableAssets, 0);
            _assetSelector.RegisterValueChangedCallback(OnChangeAsset);
            _assetSelector.formatListItemCallback = FormatAssetDropdown;
            _assetSelector.formatSelectedValueCallback = FormatAssetDropdown;

            toolbar.Add(_assetSelector);

            _splitView = new TwoPaneSplitView(0, 200, TwoPaneSplitViewOrientation.Horizontal);

            _objectPanel = new ObjectPanel<StateReference>("State Properties");
            _objectPanel.SetObject(null);

            _statesPanel = new StatesPanel(this, (stateRef) =>
            {
                _objectPanel.SetObject(stateRef);
            });

            _objectPanel.RegisterCallback<ChangeEvent<string>>((evt) =>
            {
                _statesPanel.RefreshItems();
            });

            _splitView.Add(_statesPanel);
            _splitView.Add(_objectPanel);

            rootVisualElement.Add(toolbar);

            rootVisualElement.Add(_splitView);

            if (_asset != null) SetAsset(_asset);
            else SetAsset(_assetSelector.value);
        }

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
            UpdateAvilableAssets();
        }

        private string FormatAssetDropdown(StatesAsset asset)
        {
            return asset.name;
        }

        private void OnChangeAsset(ChangeEvent<StatesAsset> evt)
        {
            SetAsset(evt.newValue);
        }

        private void UpdateAvilableAssets()
        {
            _availableAssets.Clear();

            string[] guids = AssetDatabase.FindAssets("t:StatesAsset");

            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);

                if (string.IsNullOrEmpty(assetPath)) continue;

                StatesAsset asset = AssetDatabase.LoadAssetAtPath<StatesAsset>(assetPath);

                if (asset != null)
                    _availableAssets.Add(asset);
            }
        }

        private void SetAsset(StatesAsset asset)
        {
            _asset = asset;

            if (_asset == null)
            {
                _splitView?.SetEnabled(false);

                if (_availableAssets.Count > 0)
                {
                    _assetSelector.value = _availableAssets[0];
                    SetAsset(_assetSelector.value);
                }
            }
            else
            {
                _statesPanel.SetItemsSource(asset.States);

                UpdateStartingState();

                if (_asset.States.Count > 0)
                {
                    _statesPanel.View.SetSelection(-1);
                    _statesPanel.View.SetSelection(0);
                    _objectPanel.SetObject(_asset.States[0]);
                }
                else
                {
                    _statesPanel.View.SetSelection(-1);
                    _objectPanel.SetObject(null);
                }

                _splitView?.SetEnabled(true);
            }
        }

        public void UpdateStartingState()
        {
            if (_asset.StartingState == null || !_asset.States.Contains(_asset.StartingState))
            {
                if (_asset.States.Count > 0)
                    _asset.StartingState = _asset.States[0];
                else
                    _asset.StartingState = null;

                _statesPanel.RefreshItems();
            }
        }
    }
}
