#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace CupOHappiness.Toon.Editor
{
    public static class TamSampleAssetGenerator
    {
        private const string Folder = "Assets/Niki/Toon/Runtime/Samples";
        private const string SourcePath = Folder + "/TamSampleSourceSet.asset";
        private const string ArrayPath = Folder + "/TamSample.asset";

        [MenuItem("Tools/CupOHappiness/Toon/Create Sample TAM Asset")]
        public static void CreateSampleAsset()
        {
            EnsureFolder(Folder);
            AssetDatabase.DeleteAsset(SourcePath);
            AssetDatabase.DeleteAsset(ArrayPath);

            var source = ScriptableObject.CreateInstance<TamSourceSet>();
            source.name = "TamSampleSourceSet";
            AssetDatabase.CreateAsset(source, SourcePath);
            source.tones = new[]
            {
                CreateTone("Light", 0),
                CreateTone("Mid", 1),
                CreateTone("Dark", 2)
            };
            EditorUtility.SetDirty(source);
            TamTextureArrayBuilder.BuildGeneratedMips(source, ArrayPath);
            AssetDatabase.SaveAssets();
        }

        private static TamToneSources CreateTone(string toneName, int toneIndex)
        {
            const int size = 64;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false, true) { name = toneName + " Base" };
            texture.SetPixels(CreateNestedHatchPixels(size, toneIndex));
            texture.Apply();
            AssetDatabase.AddObjectToAsset(texture, SourcePath);
            return new TamToneSources { name = toneName, sourceTexture = texture };
        }

        private static Color[] CreateNestedHatchPixels(int size, int toneIndex)
        {
            var pixels = new Color[size * size];
            var strokeSpacing = Mathf.Max(2, size / 8);
            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    var isInk = (x + y) % strokeSpacing == 0;
                    if (toneIndex >= 1)
                    {
                        isInk |= Mathf.Abs(x - y) % strokeSpacing == 0;
                    }
                    if (toneIndex >= 2)
                    {
                        isInk |= (2 * x + y) % strokeSpacing == 0;
                    }

                    pixels[y * size + x] = isInk ? Color.red : Color.black;
                }
            }
            return pixels;
        }

        private static void EnsureFolder(string folder)
        {
            if (AssetDatabase.IsValidFolder(folder)) return;
            var parent = System.IO.Path.GetDirectoryName(folder)?.Replace('\\', '/');
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, System.IO.Path.GetFileName(folder));
        }
    }
}
#endif
