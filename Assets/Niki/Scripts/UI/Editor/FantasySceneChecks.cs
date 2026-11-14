using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using Unity.Netcode.Components;
using TMPro;

namespace CupOHappiness.UI.Editor
{
    public static class FantasySceneChecks
    {
        public static string Run()
        {
            foreach (string path in new[] {
                "Assets/Scenes/GAME SCENES/FantasyGameScene.unity",
                "Assets/Scenes/Juan/Juan_FantasyGameScene.unity" })
            {
                var scene = EditorSceneManager.OpenPreviewScene(path);
                try
                {
                    var objects = scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<Transform>(true)).ToArray();
                    var manager = objects.Select(t => t.GetComponent<UpgradeManager>()).First(m => m);
                    var serialized = new SerializedObject(manager);
                    var ui = (GameObject)serialized.FindProperty("upgradeChoiceUI").objectReferenceValue;
                    Require(ui, path + ": missing choice UI");
                    for (int i = 0; i < 3; i++)
                    {
                        var icon = (Image)serialized.FindProperty("IconSprites").GetArrayElementAtIndex(i).objectReferenceValue;
                        var name = (TMP_Text)serialized.FindProperty("upgradeNames").GetArrayElementAtIndex(i).objectReferenceValue;
                        Require(icon && name, path + ": choice " + i + " has missing icon/name references");
                        ui.SetActive(true);
                        Require(icon.gameObject.activeInHierarchy && name.gameObject.activeInHierarchy, "Choice stays hidden when UI opens");
                        var button = name.GetComponentInParent<Button>(true);
                        Require(button, "Choice has no button");
                        Require(Enumerable.Range(0, button.onClick.GetPersistentEventCount()).Any(n =>
                            button.onClick.GetPersistentTarget(n) == manager &&
                            button.onClick.GetPersistentMethodName(n) == nameof(UpgradeManager.SelectUpgrade)), "Choice has no selection action");
                    }
                    if (path == "Assets/Scenes/GAME SCENES/FantasyGameScene.unity")
                        Require(!objects.Any(t => GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject) > 0),
                            path + ": missing scripts");
                }
                finally { EditorSceneManager.ClosePreviewScene(scene); }
            }
            var rubble = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Placeholder/Rubble.prefab");
            Require(rubble.GetComponent<NetworkObject>() && rubble.GetComponent<NetworkTransform>() &&
                rubble.GetComponent<NetworkRigidbody>(), "Rubble is not a networked physics prefab");
            foreach (string guid in AssetDatabase.FindAssets("t:NetworkPrefabsList"))
            {
                var list = AssetDatabase.LoadAssetAtPath<NetworkPrefabsList>(AssetDatabase.GUIDToAssetPath(guid));
                Require(list.PrefabList.Any(p => p.Prefab == rubble), "Rubble missing from " + AssetDatabase.GetAssetPath(list));
                foreach (var entry in list.PrefabList.Where(p => p.Prefab))
                    Require(!entry.Prefab.GetComponentsInChildren<Transform>(true).Any(t =>
                        GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject) > 0),
                        "Registered prefab has missing scripts: " + AssetDatabase.GetAssetPath(entry.Prefab));
            }
            return "Both Fantasy scenes: 3 visible, bound choices with selection actions. Main Fantasy scene has no missing scripts. Rubble network components and registration passed; registered prefabs have no missing scripts.";
        }

        private static void Require(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException(message);
        }
    }
}
