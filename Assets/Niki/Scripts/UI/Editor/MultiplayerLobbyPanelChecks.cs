using System;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace CupOHappiness.UI.Editor
{
    public static class MultiplayerLobbyPanelChecks
    {
        private const string Prefab = "Assets/Niki/Runtime/Prefabs/UI/Lobby/Panel_MultiplayerLobby.prefab";

        public static string Capture(int width, int height, bool populated = false, int fixturePlayers = 2, string buttonState = "Normal")
        {
            var scene = EditorSceneManager.NewPreviewScene();
            Camera camera = null;
            RenderTexture target = null;
            Texture2D pixels = null;
            var previous = RenderTexture.active;
            try
            {
                var cameraObject = new GameObject("Lobby Preview Camera", typeof(Camera));
                SceneManager.MoveGameObjectToScene(cameraObject, scene);
                camera = cameraObject.GetComponent<Camera>();
                camera.scene = scene;
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = Color.black;
                camera.orthographic = true;
                camera.orthographicSize = height / 2f;
                camera.nearClipPlane = .01f;
                camera.farClipPlane = 100;
                target = new RenderTexture(width, height, 24);
                camera.targetTexture = target;
                var canvasObject = new GameObject("Preview Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
                SceneManager.MoveGameObjectToScene(canvasObject, scene);
                var canvas = canvasObject.GetComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceCamera;
                canvas.worldCamera = camera;
                canvas.planeDistance = 10;
                var root = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Prefab), scene);
                root.transform.SetParent(canvas.transform, false);
                root.SetActive(true);
                var rootRect = (RectTransform)root.transform;
                rootRect.anchorMin = Vector2.zero; rootRect.anchorMax = Vector2.one;
                rootRect.offsetMin = rootRect.offsetMax = Vector2.zero;
                Canvas.ForceUpdateCanvases();
                root.SendMessage("OnRectTransformDimensionsChange", SendMessageOptions.DontRequireReceiver);
                if (populated)
                {
                    root.GetComponentsInChildren<TMP_Text>(true).First(t => t.text == "No active session").text = "Harbor Hangout";
                    root.GetComponentsInChildren<TMP_Text>(true).First(t => t.text == "PLAYERS  (0 / 4)").text = "PLAYERS  (2 / 4)";
                    root.GetComponentsInChildren<TMP_Text>(true).First(t => t.name == "Session Code Text").text = "7KJ4P";
                    var content = root.GetComponentsInChildren<Transform>(true).First(t => t.name == "Open Slot 1").parent;
                    var rowAsset = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Niki/Runtime/Prefabs/UI/Lobby/SessionPlayerRow.prefab");
                    foreach (string name in Enumerable.Range(0, fixturePlayers).Select(i => i == 0 ? "Nikita  [HOST]" : i == 1 ? "Rook" : "Player with a very long display name " + i).Reverse())
                    {
                        var row = (GameObject)PrefabUtility.InstantiatePrefab(rowAsset, content);
                        row.GetComponentsInChildren<TMP_Text>(true).First(t => t.name == "Player Name").text = name;
                        row.GetComponentsInChildren<TMP_Text>(true).First(t => t.name == "Host Badge").gameObject.SetActive(name.Contains("HOST"));
                        foreach (var b in row.GetComponentsInChildren<Button>(true)) b.gameObject.SetActive(false);
                        row.transform.SetAsFirstSibling();
                    }
                    content.Find("Open Slot 1").gameObject.SetActive(false);
                    content.Find("Open Slot 2").gameObject.SetActive(false);
                }
                for (int pass = 0; pass < 5; pass++)
                {
                    foreach (var layout in root.GetComponentsInChildren<Layout>(true)) { layout.RefreshChildCache(); layout.SetDirty(); }
                    Canvas.ForceUpdateCanvases();
                    foreach (var rect in root.GetComponentsInChildren<RectTransform>().Reverse())
                        LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
                }
                var scroll = root.GetComponentInChildren<ScrollRect>();
                if (!scroll || !scroll.content || !scroll.viewport || !scroll.vertical || scroll.horizontal)
                    throw new InvalidOperationException("Invalid player scrolling setup.");
                if (populated && fixturePlayers > 4)
                {
                    if (scroll.content.rect.height <= scroll.viewport.rect.height)
                        throw new InvalidOperationException("Overflow fixture did not grow the list.");
                    scroll.verticalNormalizedPosition = 0;
                    if (Mathf.Abs(scroll.verticalNormalizedPosition) > .01f)
                        throw new InvalidOperationException("Player list cannot scroll to its bottom.");
                    scroll.verticalNormalizedPosition = 1;
                }
                var design = (RectTransform)root.transform.Find("Lobby Layout");
                if (design.rect.width < 1099 || design.rect.height < 709)
                    throw new InvalidOperationException("Invalid design size: " + design.rect);
                if (root.GetComponentsInChildren<Transform>(true).Any(t => GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject) != 0))
                    throw new InvalidOperationException("Missing scripts in panel.");
                var controller = new SerializedObject(root.GetComponent<MultiplayerLobbyPanel>());
                foreach (string field in new[] { "design", "sessionName", "playerCount", "status", "playerNameInput",
                    "sessionNameInput", "joinCodeInput", "createButton", "joinButton", "createGate", "joinGate",
                    "playerContent", "playerRowPrefab", "levelSelect" })
                    if (!controller.FindProperty(field).objectReferenceValue)
                        throw new InvalidOperationException("Unbound lobby field: " + field);
                foreach (var widget in root.GetComponentsInChildren<MonoBehaviour>(true)
                    .Where(c => c && (c.GetType().Name == "CreateSession" || c.GetType().Name == "JoinSessionByCode")))
                {
                    string prefix = widget.GetType().Name == "CreateSession" ? "create" : "join";
                    var button = widget.GetComponentInChildren<Button>(true);
                    if (new SerializedObject(widget).FindProperty("m_EnterSessionButton").objectReferenceValue != button
                        || controller.FindProperty(prefix + "Button").objectReferenceValue != button
                        || ((CanvasGroup)controller.FindProperty(prefix + "Gate").objectReferenceValue).gameObject != button.gameObject)
                        throw new InvalidOperationException("Incorrect " + prefix + " button binding.");
                }
                var rowData = new SerializedObject((LobbyPlayerRow)controller.FindProperty("playerRowPrefab").objectReferenceValue);
                foreach (string field in new[] { "playerName", "role", "kickButton" })
                    if (!rowData.FindProperty(field).objectReferenceValue)
                        throw new InvalidOperationException("Unbound player row field: " + field);
                foreach (var button in root.GetComponentsInChildren<Button>())
                {
                    var text = button.transform.Find("txt_Label");
                    if (!text || !text.gameObject.activeInHierarchy) continue;
                    var bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(button.transform, text);
                    var rect = ((RectTransform)button.transform).rect;
                    if (bounds.min.y < rect.yMin - .5f || bounds.max.y > rect.yMax + .5f)
                        throw new InvalidOperationException("Label is outside button: " + button.name);
                }
                if (buttonState != "Normal")
                {
                    foreach (var button in root.GetComponentsInChildren<Button>())
                    {
                        var colors = button.colors;
                        colors.fadeDuration = 0;
                        button.colors = colors;
                        var navigation = button.navigation;
                        navigation.mode = Navigation.Mode.None;
                        button.navigation = navigation;
                        button.interactable = buttonState != "Disabled";
                        var pointer = new UnityEngine.EventSystems.PointerEventData(null)
                        { button = UnityEngine.EventSystems.PointerEventData.InputButton.Left };
                        if (buttonState == "Highlighted" || buttonState == "Pressed") button.OnPointerEnter(pointer);
                        if (buttonState == "Pressed") button.OnPointerDown(pointer);
                        if (buttonState == "Selected") button.OnSelect(new UnityEngine.EventSystems.BaseEventData(null));
                    }
                }
                camera.Render();
                RenderTexture.active = target;
                pixels = new Texture2D(width, height, TextureFormat.RGB24, false);
                pixels.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                pixels.Apply();
                string path = $"Temp/LobbyPanelPreview/lobby-{width}x{height}{(populated ? "-populated" : "")}{(fixturePlayers > 4 ? "-overflow" : "")}{(buttonState == "Normal" ? "" : "-" + buttonState)}.png";
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                File.WriteAllBytes(path, pixels.EncodeToPNG());
                return path;
            }
            finally
            {
                RenderTexture.active = previous;
                if (pixels) Object.DestroyImmediate(pixels);
                if (camera) camera.targetTexture = null;
                if (target) { target.Release(); Object.DestroyImmediate(target); }
                EditorSceneManager.ClosePreviewScene(scene);
            }
        }
    }
}
