using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace CupOHappiness.UI
{
    public static class ReadyPlayerRowChecks
    {
        private const string Path = "Assets/Niki/Runtime/Prefabs/UI/Lobby/ReadyPlayerRow.prefab";
        private const string SourcePath = "Assets/Niki/Runtime/Prefabs/UI/Lobby/SessionPlayerRow.prefab";

        public static string Run()
        {
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(Path);
            Require(asset, "ReadyPlayerRow prefab is missing");
            Require(PrefabUtility.GetPrefabAssetType(asset) == PrefabAssetType.Variant, "ReadyPlayerRow is not a prefab variant");
            var source = PrefabUtility.GetCorrespondingObjectFromSource(asset);
            Require(source && AssetDatabase.GetAssetPath(source) == SourcePath, "ReadyPlayerRow source is not SessionPlayerRow.prefab");
            Require(!asset.GetComponentsInChildren<Transform>(true).Any(t => GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject) != 0), "ReadyPlayerRow has missing scripts");

            var contents = PrefabUtility.LoadPrefabContents(Path);
            try
            {
                var row = contents.GetComponent<ReadyPlayerRow>();
                Require(row, "ReadyPlayerRow component is missing");
                var serialized = new SerializedObject(row);
                var name = (TMPro.TMP_Text)serialized.FindProperty("playerName").objectReferenceValue;
                var mark = (GameObject)serialized.FindProperty("readyMark").objectReferenceValue;
                Require(name && mark, "ReadyPlayerRow fields are unassigned");
                row.Bind("A", true);
                Require(name.text == "A" && mark.activeSelf, "Bind(A, true) failed");
                row.Bind("A", false);
                Require(!mark.activeSelf, "Bind(A, false) failed");
                row.Bind("", true);
                Require(name.text == "Player", "Empty name fallback failed");
                var kick = contents.GetComponentsInChildren<UnityEngine.UI.Button>(true).FirstOrDefault();
                Require(kick && !kick.gameObject.activeSelf, "Kick button is not inactive");
                var badge = contents.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == "Host Badge");
                Require(badge && !badge.gameObject.activeSelf, "Host Badge is not inactive");
            }
            finally { PrefabUtility.UnloadPrefabContents(contents); }

            var original = AssetDatabase.LoadAssetAtPath<GameObject>(SourcePath);
            Require(original && !original.GetComponent<ReadyPlayerRow>(), "Original SessionPlayerRow has ReadyPlayerRow");
            return "ReadyPlayerRow variant and Bind checks passed.";
        }

        private static void Require(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException(message);
        }
    }
}
