using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace CupOHappiness.UI
{
    public static class ReadyRosterChecks
    {
        private const string RowPath = "Assets/Niki/Runtime/Prefabs/UI/Lobby/ReadyPlayerRow.prefab";

        public static string Run()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<ReadyPlayerRow>(RowPath);
            Require(prefab, "ReadyPlayerRow prefab is missing");
            var scene = EditorSceneManager.NewPreviewScene();
            try
            {
                var go = new GameObject("RosterCheck");
                EditorSceneManager.MoveGameObjectToScene(go, scene);
                var roster = go.AddComponent<ReadyRoster>();
                var so = new SerializedObject(roster);
                so.FindProperty("rowPrefab").objectReferenceValue = prefab;
                so.ApplyModifiedPropertiesWithoutUndo();

                Require(go.transform.childCount == 0, "Roster starts with children");
                roster.Refresh(new List<ReadyEntry> { Entry(2, "C", false), Entry(0, "Host", false), Entry(1, "B", false) });
                Require(go.transform.childCount == 3 && roster.RowCount == 3, "Expected 3 rows");
                Require(Name(go, 0) == "Host" && Name(go, 1) == "B" && Name(go, 2) == "C", "Rows are not host first, then by client id");
                roster.Refresh(new List<ReadyEntry> { Entry(2, "C", false), Entry(0, "Host", false), Entry(1, "B", false) });
                Require(go.transform.childCount == 3, "Repeated refresh made duplicates");
                roster.Refresh(new List<ReadyEntry> { Entry(2, "C", false), Entry(0, "Host", true), Entry(1, "B", false) });
                Require(Mark(go, 0) && !Mark(go, 1) && !Mark(go, 2), "Ready flag did not update only the host row");
                roster.Refresh(new List<ReadyEntry> { Entry(0, "Host", true), Entry(2, "C", false) });
                Require(go.transform.childCount == 2 && roster.RowCount == 2 && Name(go, 1) == "C", "Removed player row was not destroyed");
                return "ReadyRoster checks passed: 0 -> 3 -> 2 rows, host first, no duplicates, per-row ready flag.";
            }
            finally { EditorSceneManager.ClosePreviewScene(scene); }
        }

        private static ReadyEntry Entry(ulong id, string name, bool ready) =>
            new ReadyEntry { clientId = id, name = new FixedString64Bytes(name), ready = ready };

        private static string Name(GameObject go, int i) =>
            go.transform.GetChild(i).GetComponentInChildren<TMPro.TMP_Text>(true).text;

        private static bool Mark(GameObject go, int i)
        {
            var so = new SerializedObject(go.transform.GetChild(i).GetComponent<ReadyPlayerRow>());
            return ((GameObject)so.FindProperty("readyMark").objectReferenceValue).activeSelf;
        }

        private static void Require(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException(message);
        }
    }
}
