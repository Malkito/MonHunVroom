#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace CupOHappiness.Toon.Editor
{
    /// <summary>Guides a material author through the release acceptance procedure.</summary>
    public sealed class CupOHappinessToonPublicAcceptanceWindow : EditorWindow
    {
        private const string GuidePath = "docs/acceptance/cup-tam-toon-public-contract.md";
        private const string StatePrefix = "CupOHappiness.Toon.PublicAcceptance.";

        private static readonly CheckItem[] RenderChecks =
        {
            new CheckItem("TAM Off / On", "TAM off matches CupOHappiness Toon & Outline; enabling TAM changes only the intended hatch layers."),
            new CheckItem("CupOHappiness Material Modes", "Check opaque, transparent, alpha-clipped, normal-mapped, and outlined materials."),
            new CheckItem("Tone Response", "Move light, shadows, steps, and ramp; hatch density should follow the toon boundary."),
            new CheckItem("Layer Independence", "Verify Shadow/Form, Light, and Highlight Punch-out use their assigned assets and controls independently."),
            new CheckItem("Highlight Masking", "Check specular, Toon Rim, and Rim Lighting with Light Hatching Transparency at 0 and 1."),
            new CheckItem("Zero Controls", "Set Shadow/Form and Light ranges to zero, and Punch-out opacity to zero; each layer should disappear."),
            new CheckItem("Stability", "Inspect near, mid, and far views while moving, rotating, and non-uniformly scaling the object."),
            new CheckItem("Motion Vectors", "Check moving or skinned geometry with the renderer's motion-vector support enabled.")
        };

        private static readonly CheckItem[] PerformanceChecks =
        {
            new CheckItem("TAM Off", "Capture a warmed-up baseline."),
            new CheckItem("Shadow/Form Only", "Measure the nearest-slice baseline with only Shadow/Form active."),
            new CheckItem("Light Enabled", "Measure Shadow/Form plus Light TAM."),
            new CheckItem("Punch-out Enabled", "Measure Shadow/Form plus Highlight Punch-out TAM."),
            new CheckItem("All Layers", "Measure all three TAM layers with additional lights and outlines off/on.")
        };

        private Vector2 scrollPosition;

        [MenuItem("Tools/CupOHappiness/Toon/Public Acceptance...")]
        public static void ShowWindow()
        {
            var window = GetWindow<CupOHappinessToonPublicAcceptanceWindow>("Public Acceptance");
            window.minSize = new Vector2(460, 420);
            window.Show();
        }

        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            EditorGUILayout.LabelField("CupOHappiness TAM Toon Public Acceptance", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Rebuild the reproducible sample, inspect it in the Game view, then record visual and performance results in the acceptance guide. " +
                "The sample build replaces previously generated PublicAcceptance assets.", MessageType.Info);

            DrawSampleActions();
            DrawChecklist("Rendered Acceptance", "render", RenderChecks);
            DrawChecklist("Performance Capture", "performance", PerformanceChecks);

            EditorGUILayout.Space(6);
            if (GUILayout.Button("Open Acceptance Guide"))
            {
                var path = System.IO.Path.GetFullPath(GuidePath).Replace('\\', '/');
                Application.OpenURL("file:///" + path);
            }
            EditorGUILayout.EndScrollView();
        }

        private static void DrawSampleActions()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Acceptance Sample", EditorStyles.boldLabel);
            var sampleExists = AssetDatabase.LoadAssetAtPath<SceneAsset>(CupOHappinessToonPublicAcceptanceSample.ScenePath) != null;
            EditorGUILayout.LabelField("Status", sampleExists ? "Ready" : "Not created");

            if (GUILayout.Button(sampleExists ? "Rebuild Acceptance Sample" : "Create Acceptance Sample", GUILayout.Height(28)))
            {
                if (!sampleExists || EditorUtility.DisplayDialog(
                    "Rebuild acceptance sample?",
                    "This deletes and recreates the generated PublicAcceptance scene and materials.",
                    "Rebuild", "Cancel"))
                {
                    CupOHappinessToonPublicAcceptanceSample.CreatePublicAcceptanceSample();
                }
            }

            using (new EditorGUI.DisabledScope(!sampleExists))
            {
                if (GUILayout.Button("Open Acceptance Scene", GUILayout.Height(25)))
                {
                    if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                    {
                        EditorSceneManager.OpenScene(CupOHappinessToonPublicAcceptanceSample.ScenePath);
                    }
                }
            }
            EditorGUILayout.EndVertical();
        }

        private static void DrawChecklist(string title, string section, CheckItem[] checks)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
            foreach (var check in checks)
            {
                var key = StatePrefix + section + "." + check.name;
                var complete = SessionState.GetBool(key, false);
                var next = EditorGUILayout.ToggleLeft(new GUIContent(check.name, check.description), complete);
                if (next != complete) SessionState.SetBool(key, next);
                EditorGUILayout.LabelField(check.description, EditorStyles.miniLabel);
            }
            EditorGUILayout.EndVertical();
        }

        private readonly struct CheckItem
        {
            public readonly string name;
            public readonly string description;
            public CheckItem(string name, string description)
            {
                this.name = name;
                this.description = description;
            }
        }
    }
}
#endif
