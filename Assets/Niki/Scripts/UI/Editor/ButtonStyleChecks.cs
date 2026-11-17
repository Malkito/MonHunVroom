using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CupOHappiness.UI.Editor
{
    public static class ButtonStyleChecks
    {
        private const string SharedButton = "Assets/Niki/Runtime/Prefabs/UI/Shared/Button.prefab";

        public static string Run()
        {
            var shared = AssetDatabase.LoadAssetAtPath<GameObject>(SharedButton);
            Require(shared.transform.childCount == 3, "Unexpected shared button hierarchy");
            Require(!shared.transform.Find("Button Surface"), "Redundant button surface wrapper");
            int buttons = 0;
            foreach (string guid in AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Niki/Runtime/Prefabs/UI" }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                foreach (var button in AssetDatabase.LoadAssetAtPath<GameObject>(path).GetComponentsInChildren<Button>(true))
                {
                    Require(PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(button.gameObject) == SharedButton,
                        path + ": not a shared button");
                    var layout = new SerializedObject(button.GetComponent<Layout>());
                    foreach (string side in new[] { "top", "bottom", "left", "right" })
                        Require(Mathf.Abs(layout.FindProperty("m_padding." + side).floatValue
                            - (side == "top" || side == "bottom" ? 22 : 16)) < .01f, "Wrong " + side + " padding");
                    foreach (string name in new[] { "Background Mask", "Highlight" })
                    {
                        var item = button.transform.Find(name).GetComponent<LayoutItem>();
                        Require(item.IsFloating && item.AttachTo == LayoutItem.FloatingAttachTo.Parent
                            && item.SizeMode.x == SizingMode.Grow && item.SizeMode.y == SizingMode.Grow,
                            name + " does not follow its parent");
                        Require(!item.BringToFront, name + " can cover the label");
                    }
                    var mask = button.transform.Find("Background Mask").GetComponent<Mask>();
                    Require(mask && !mask.showMaskGraphic, "Invalid smoke mask");
                    Require(mask.GetComponent<Image>().sprite.name == "sprt_Button_Mask", "Wrong mask texture");
                    Require(mask.GetComponentInChildren<RawImage>().texture.name == "sprt_Smoke_Texture", "Wrong smoke texture");
                    if (mask.gameObject.activeSelf)
                    {
                        Require(button.GetComponent<Image>().enabled
                            && button.GetComponent<Image>().sprite.name == "sprt_Button_Border", "Missing border");
                        Require(button.targetGraphic == button.transform.Find("Highlight").GetComponent<Image>(), "Wrong highlight target");
                        Require(button.GetComponent<Layout>().SizeMode.y == SizingMode.FitContent, "Fixed height conflicts with padding");
                    }
                    buttons++;
                }
            }
            var scene = EditorSceneManager.NewPreviewScene();
            try
            {
                var root = (GameObject)PrefabUtility.InstantiatePrefab(shared, scene);
                root.SetActive(true);
                var button = root.GetComponent<Button>();
                button.interactable = true;
                var navigation = button.navigation;
                navigation.mode = Navigation.Mode.None;
                button.navigation = navigation;
                var colors = button.colors;
                colors.fadeDuration = 0;
                button.colors = colors;
                var pointer = new PointerEventData(null) { button = PointerEventData.InputButton.Left };
                button.OnPointerUp(pointer); button.OnPointerExit(pointer); button.OnDeselect(new BaseEventData(null));
                Require(Alpha(button) < .01f, "Normal state is highlighted");
                button.OnPointerEnter(pointer);
                Require(Mathf.Abs(Alpha(button) - .85f) < .01f, "Missing hover overlay");
                button.OnPointerDown(pointer);
                Require(Alpha(button) > .99f, "Missing pressed overlay");
                button.OnPointerUp(pointer); button.OnPointerExit(pointer); button.OnSelect(new BaseEventData(null));
                Require(Mathf.Abs(Alpha(button) - .85f) < .01f, "Missing selection overlay");
                button.interactable = false;
                Require(Alpha(button) < .01f, "Disabled button is highlighted");
            }
            finally { EditorSceneManager.ClosePreviewScene(scene); }
            return buttons + " shared buttons, exact padding, hierarchy, and 5 selection states passed.";
        }

        // Check the current scene without a forced layout rebuild.
        public static string CheckLiveLobby()
        {
            int count = 0;
            foreach (var root in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
            foreach (var panel in root.GetComponentsInChildren<MultiplayerLobbyPanel>())
            foreach (var button in panel.GetComponentsInChildren<Button>())
            {
                var rect = ((RectTransform)button.transform).rect;
                Require(rect.width > 1 && rect.height > 1, button.name + ": zero size");
                foreach (string name in new[] { "Background Mask", "Highlight" })
                    Require(Vector2.Distance(rect.size, ((RectTransform)button.transform.Find(name)).rect.size) < .5f,
                        button.name + ": " + name + " has the wrong size");
                var label = button.transform.Find("txt_Label");
                if (label.gameObject.activeInHierarchy)
                {
                    var bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(button.transform, label);
                    Require(Mathf.Abs(rect.yMax - bounds.max.y - 22) < .5f
                        && Mathf.Abs(bounds.min.y - rect.yMin - 22) < .5f, button.name + ": incorrect vertical text padding");
                    Require(bounds.min.x - rect.xMin >= 15.5f && rect.xMax - bounds.max.x >= 15.5f,
                        button.name + ": insufficient horizontal text padding");
                }
                count++;
            }
            Require(count > 0, "No active lobby");
            return count + " live buttons passed size and padding checks without a forced rebuild.";
        }

        private static float Alpha(Button button) => button.targetGraphic.canvasRenderer.GetColor().a;
        private static void Require(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException(message);
        }
    }
}
