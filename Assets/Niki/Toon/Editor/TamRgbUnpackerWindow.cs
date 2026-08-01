#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace CupOHappiness.Toon.Editor
{
    public sealed class TamRgbUnpackerWindow : EditorWindow
    {
        private Texture2D packedTexture;
        private TamSourceSet targetSourceSet;
        private bool invertCoverage;
        private string outputFolder = "Assets/Niki/Toon/Runtime/TAM Sources";
        private string assetPrefix = "TAM";

        [MenuItem("Tools/CupOHappiness/Toon/Unpack RGB TAM Texture...")]
        public static void ShowWindow()
        {
            var window = GetWindow<TamRgbUnpackerWindow>("Unpack RGB TAM");
            if (Selection.activeObject is Texture2D selectedTexture)
            {
                window.packedTexture = selectedTexture;
                window.assetPrefix = selectedTexture.name;
            }
            window.minSize = new Vector2(440, 260);
            window.Show();
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Unpack RGB TAM", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Converts one packed RGB TAM into three linear Texture2D assets: " +
                "Red → Light, Green → Mid, Blue → Dark. Each output stores its ink coverage in red, " +
                "ready for a TAM Source Set and the array builder.", MessageType.Info);

            packedTexture = (Texture2D)EditorGUILayout.ObjectField("Packed RGB Texture", packedTexture, typeof(Texture2D), false);
            targetSourceSet = (TamSourceSet)EditorGUILayout.ObjectField("Source Set (optional)", targetSourceSet, typeof(TamSourceSet), false);
            invertCoverage = EditorGUILayout.Toggle("Invert Ink Coverage", invertCoverage);
            if (invertCoverage)
            {
                EditorGUILayout.HelpBox("Writes 1 - channel value: black becomes ink and white becomes paper.", MessageType.None);
            }
            outputFolder = EditorGUILayout.TextField("Output Folder", outputFolder);
            assetPrefix = EditorGUILayout.TextField("Asset Prefix", assetPrefix);

            EditorGUILayout.Space(6);
            using (new EditorGUI.DisabledScope(packedTexture == null || string.IsNullOrWhiteSpace(outputFolder)))
            {
                if (GUILayout.Button("Unpack RGB Channels", GUILayout.Height(30)))
                {
                    Unpack();
                }
            }
        }

        private void Unpack()
        {
            try
            {
                var readableSource = EnsureReadable(packedTexture);
                var sourcePixels = readableSource.GetPixels();
                EnsureAssetFolder(outputFolder);

                var light = CreateChannelTexture(sourcePixels, readableSource.width, readableSource.height, 0, "Light", invertCoverage);
                var mid = CreateChannelTexture(sourcePixels, readableSource.width, readableSource.height, 1, "Mid", invertCoverage);
                var dark = CreateChannelTexture(sourcePixels, readableSource.width, readableSource.height, 2, "Dark", invertCoverage);
                var textures = new[] { light, mid, dark };
                var names = new[] { "Light", "Mid", "Dark" };

                for (var index = 0; index < textures.Length; index++)
                {
                    var path = AssetDatabase.GenerateUniqueAssetPath(
                        $"{outputFolder.TrimEnd('/')}/{assetPrefix}_{names[index]}.asset");
                    AssetDatabase.CreateAsset(textures[index], path);
                }

                if (targetSourceSet != null)
                {
                    Undo.RecordObject(targetSourceSet, "Assign Unpacked TAM Textures");
                    targetSourceSet.tones = new[]
                    {
                        new TamToneSources { name = "Light", sourceTexture = light },
                        new TamToneSources { name = "Mid", sourceTexture = mid },
                        new TamToneSources { name = "Dark", sourceTexture = dark }
                    };
                    EditorUtility.SetDirty(targetSourceSet);
                }

                AssetDatabase.SaveAssets();
                Selection.activeObject = targetSourceSet != null ? (UnityEngine.Object)targetSourceSet : light;
                EditorGUIUtility.PingObject(Selection.activeObject);
                ShowNotification(new GUIContent("Created Light, Mid, and Dark TAM masks."));
            }
            catch (Exception exception)
            {
                EditorUtility.DisplayDialog("Could not unpack RGB TAM", exception.Message, "OK");
            }
        }

        private static Texture2D EnsureReadable(Texture2D source)
        {
            if (source.isReadable) return source;

            var path = AssetDatabase.GetAssetPath(source);
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null)
            {
                throw new ArgumentException($"{source.name} must have Read/Write enabled.");
            }

            importer.isReadable = true;
            importer.SaveAndReimport();
            var readable = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (readable == null || !readable.isReadable)
            {
                throw new ArgumentException($"Could not enable Read/Write for {source.name}.");
            }
            return readable;
        }

        private static Texture2D CreateChannelTexture(Color[] source, int width, int height, int channel, string toneName, bool invert)
        {
            var destination = new Color[source.Length];
            for (var index = 0; index < source.Length; index++)
            {
                var coverage = source[index][channel];
                destination[index] = new Color(invert ? 1.0f - coverage : coverage, 0.0f, 0.0f, 1.0f);
            }

            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false, true)
            {
                name = toneName,
                wrapMode = TextureWrapMode.Repeat,
                filterMode = FilterMode.Bilinear,
                anisoLevel = 1
            };
            texture.SetPixels(destination);
            texture.Apply(false, false);
            return texture;
        }

        private static void EnsureAssetFolder(string folder)
        {
            folder = folder.Replace('\\', '/').TrimEnd('/');
            if (AssetDatabase.IsValidFolder(folder)) return;
            var parent = Path.GetDirectoryName(folder)?.Replace('\\', '/');
            if (string.IsNullOrEmpty(parent) || parent == folder || !parent.StartsWith("Assets", StringComparison.Ordinal))
            {
                throw new ArgumentException("Output Folder must be inside Assets.");
            }
            EnsureAssetFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(folder));
        }
    }
}
#endif
