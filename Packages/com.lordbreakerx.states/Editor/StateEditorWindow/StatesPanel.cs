using LordBreakerX.EditorUtilities;
using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace LordBreakerX.States
{
    public class StatesPanel : ListPanel<StateReference>
    {
        private string _title = "States";

        public override string HeaderTitle { get => _title; set => _title = value; }

        private Button _internalAddButton;

        private System.Action<StateReference> _onSelection;

        private StateEditorWindow _parentWindow;

        public ListView View { get => PanelListView; }

        public override string ItemDisplayName => "State";

        public StatesPanel(StateEditorWindow parent, System.Action<StateReference> onSelection) : base()
        {
            _parentWindow = parent;

            _internalAddButton = this.Q<Button>("header-button");

            _onSelection = onSelection;
        }

        public override StateReference CopyItem(StateReference stateRef)
        {
            StateReference stateReference = ScriptableObject.Instantiate(stateRef);
            stateReference.name = stateRef.name;
            stateReference.ID = stateRef.ID;
            return stateReference;
        }

        public override void SetItemName(IList itemsSource, int index, string name)
        {
            if (itemsSource[index] is StateReference stateRef)
            {
                stateRef.name = name;
                stateRef.ID = name;

                EditorUtility.SetDirty(stateRef);
                AssetDatabase.Refresh();
            }
        }

        protected override string GetItemName(StateReference stateRef)
        {
            if (_parentWindow.Asset != null && _parentWindow.Asset.StartingState == stateRef)
                return $"\u2726 {stateRef.ID}";
            else
                return stateRef.ID;
        }

        protected override ContextualMenuManipulator OnCreateItemContextMenu(ListItem<StateReference> item)
        {
            return new ContextualMenuManipulator((evt) =>
            {
                evt.menu.AppendAction("Make Starting State", (action) =>
                {
                    if (item.Data != null && _parentWindow.Asset != null)
                    {
                        _parentWindow.Asset.StartingState = item.Data;

                        RefreshItems();

                        _parentWindow.UpdateStartingState();

                        EditorUtility.SetDirty(_parentWindow.Asset);
                        AssetDatabase.SaveAssets();
                        AssetDatabase.Refresh();
                    }
                });
                evt.menu.AppendSeparator();
            });
        }

        protected override string GetDefaultName(int index)
        {
            return $"State {index}";
        }

        protected override Texture GetIcon(StateReference stateRef)
        {
            System.Type stateType = stateRef.GetStateType();
            return IconUtility.GetTypeIcon(stateType);
        }

        protected override StateReference CreateItem()
        {
            UnityEditor.PopupWindow.Show(_internalAddButton.worldBound, new StateBuilderMenu((builder) =>
            {
                if (builder != null && builder.Value != null)
                {
                    AddItem(builder.Value);
                }
            }));

            return null;
        }

        protected override void OnSelectionChanged(StateReference selectedItem)
        {
            _onSelection?.Invoke(selectedItem);
        }

        public override void AddItem(StateReference item)
        {
            base.AddItem(item);

            if (item != null)
            {
                string assetPath = AssetDatabase.GetAssetPath(_parentWindow.Asset);

                _parentWindow.UpdateStartingState();

                AssetDatabase.AddObjectToAsset(item, assetPath);

                EditorUtility.SetDirty(_parentWindow.Asset);
                EditorUtility.SetDirty(item);

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }

        public override void RemoveItem(StateReference item)
        {
            base.RemoveItem(item);

            if (item != null)
            {
                _parentWindow.UpdateStartingState();
                AssetDatabase.RemoveObjectFromAsset(item);
                EditorUtility.SetDirty(_parentWindow.Asset);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }

        public override void OnRename(string newName)
        {
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}
