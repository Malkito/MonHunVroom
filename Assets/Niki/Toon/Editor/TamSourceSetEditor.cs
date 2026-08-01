#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

namespace CupOHappiness.Toon.Editor
{
    [CustomEditor(typeof(TamSourceSet))]
    public sealed class TamSourceSetEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var source = (TamSourceSet)target;
            EditorGUILayout.LabelField("TAM Array Builder", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Assign one full-resolution, square power-of-two mask per tone. " +
                "Order them light (least ink) to dark (most ink). The builder creates every mip with a linear 2x2 box filter.",
                MessageType.Info);

            var currentCount = source.tones?.Length ?? 0;
            var toneCount = Mathf.Max(2, EditorGUILayout.IntField("Tone Count", Math.Max(2, currentCount)));
            if (toneCount != currentCount)
            {
                ResizeTones(source, toneCount);
            }

            if (source.tones == null) ResizeTones(source, 2);
            for (var index = 0; index < source.tones.Length; index++)
            {
                var tone = source.tones[index] ?? (source.tones[index] = new TamToneSources());
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField($"Tone {index + 1} of {source.tones.Length}", EditorStyles.boldLabel);
                EditorGUI.BeginChangeCheck();
                tone.name = EditorGUILayout.TextField("Name", string.IsNullOrWhiteSpace(tone.name) ? DefaultToneName(index, source.tones.Length) : tone.name);
                tone.sourceTexture = (Texture2D)EditorGUILayout.ObjectField("Base Mask", tone.sourceTexture, typeof(Texture2D), false);
                if (EditorGUI.EndChangeCheck()) EditorUtility.SetDirty(source);
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.Space(4);
            if (GUILayout.Button("Unpack an RGB TAM Texture..."))
            {
                TamRgbUnpackerWindow.ShowWindow();
            }

            EditorGUILayout.Space(6);
            var errors = source.ValidateBaseTextures();
            var nestingWarnings = source.GetBaseTextureNestingWarnings();
            if (errors.Count > 0)
            {
                EditorGUILayout.HelpBox(string.Join("\n", errors), MessageType.Warning);
            }
            else
            {
                EditorGUILayout.HelpBox($"Ready: {source.tones.Length} tone slices × {TamSourceSet.ExpectedMipCount(source.tones[0].sourceTexture.width, source.tones[0].sourceTexture.height)} generated mip levels.", MessageType.None);
            }
            if (nestingWarnings.Count > 0)
            {
                EditorGUILayout.HelpBox(string.Join("\n", nestingWarnings), MessageType.Warning);
            }

            using (new EditorGUI.DisabledScope(errors.Count > 0))
            {
                if (GUILayout.Button("Build TAM Texture Array...", GUILayout.Height(28)))
                {
                    var path = EditorUtility.SaveFilePanelInProject(
                        "Build TAM Texture Array",
                        source.name + "_TAM",
                        "asset",
                        "Choose where to save the generated Texture2DArray.");
                    if (!string.IsNullOrEmpty(path))
                    {
                        try
                        {
                            TamTextureArrayBuilder.BuildGeneratedMips(source, path);
                            AssetDatabase.SaveAssets();
                            EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<Texture2DArray>(path));
                        }
                        catch (ArgumentException exception)
                        {
                            EditorUtility.DisplayDialog("Could not build TAM", exception.Message, "OK");
                        }
                    }
                }
            }
        }

        private static void ResizeTones(TamSourceSet source, int count)
        {
            Undo.RecordObject(source, "Change TAM Tone Count");
            var existing = source.tones ?? Array.Empty<TamToneSources>();
            Array.Resize(ref existing, count);
            for (var index = 0; index < existing.Length; index++)
            {
                if (existing[index] == null) existing[index] = new TamToneSources();
                if (string.IsNullOrWhiteSpace(existing[index].name)) existing[index].name = DefaultToneName(index, count);
            }
            source.tones = existing;
            EditorUtility.SetDirty(source);
        }

        private static string DefaultToneName(int index, int count)
        {
            if (index == 0) return "Light";
            if (index == count - 1) return "Dark";
            return $"Tone {index + 1}";
        }
    }
}
#endif
