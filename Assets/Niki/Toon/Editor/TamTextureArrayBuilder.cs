#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace CupOHappiness.Toon.Editor
{
    public static class TamTextureArrayBuilder
    {
        public static Texture2DArray Build(TamSourceSet source, string assetPath)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            var errors = source.Validate();
            if (errors.Count > 0)
            {
                throw new ArgumentException(string.Join("\n", errors), nameof(source));
            }

            ValidateEditorImportSettings(source);
            var first = source.tones[0].authoredMips[0];
            var array = new Texture2DArray(first.width, first.height, source.tones.Length, first.format, true, true)
            {
                name = Path.GetFileNameWithoutExtension(assetPath),
                wrapMode = TextureWrapMode.Repeat,
                filterMode = FilterMode.Bilinear,
                anisoLevel = 1
            };

            for (var tone = 0; tone < source.tones.Length; tone++)
            {
                for (var mip = 0; mip < source.tones[tone].authoredMips.Length; mip++)
                {
                    array.SetPixels(source.tones[tone].authoredMips[mip].GetPixels(), tone, mip);
                }
            }

            array.Apply(false, false);
            EnsureAssetFolder(Path.GetDirectoryName(assetPath));
            if (AssetDatabase.LoadMainAssetAtPath(assetPath) != null)
            {
                AssetDatabase.DeleteAsset(assetPath);
            }
            AssetDatabase.CreateAsset(array, assetPath);
            return array;
        }

        /// <summary>
        /// Designer workflow: builds a Texture2DArray from one full-resolution
        /// source mask per light-to-dark tone. Each mip is generated with a
        /// linear 2x2 box filter, preserving coverage and tone nesting.
        /// </summary>
        public static Texture2DArray BuildGeneratedMips(TamSourceSet source, string assetPath)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            var errors = source.ValidateBaseTextures();
            if (errors.Count > 0)
            {
                throw new ArgumentException(string.Join("\n", errors), nameof(source));
            }

            ValidateBaseTextureImportSettings(source);
            foreach (var warning in source.GetBaseTextureNestingWarnings())
            {
                Debug.LogWarning($"TAM build: {warning}", source);
            }
            var first = source.tones[0].sourceTexture;
            var array = new Texture2DArray(first.width, first.height, source.tones.Length, TextureFormat.RGBA32, true, true)
            {
                name = Path.GetFileNameWithoutExtension(assetPath),
                wrapMode = TextureWrapMode.Repeat,
                filterMode = FilterMode.Bilinear,
                anisoLevel = 1
            };

            for (var tone = 0; tone < source.tones.Length; tone++)
            {
                var pixels = ToCoveragePixels(source.tones[tone].sourceTexture.GetPixels());
                var width = first.width;
                var height = first.height;
                for (var mip = 0; mip < array.mipmapCount; mip++)
                {
                    array.SetPixels(pixels, tone, mip);
                    if (mip + 1 < array.mipmapCount)
                    {
                        pixels = DownsampleCoverageBox(pixels, width, height);
                        width = Math.Max(1, width >> 1);
                        height = Math.Max(1, height >> 1);
                    }
                }
            }

            array.Apply(false, false);
            EnsureAssetFolder(Path.GetDirectoryName(assetPath));
            if (AssetDatabase.LoadMainAssetAtPath(assetPath) != null)
            {
                AssetDatabase.DeleteAsset(assetPath);
            }
            AssetDatabase.CreateAsset(array, assetPath);
            return array;
        }

        private static Color[] ToCoveragePixels(Color[] source)
        {
            var coverage = new Color[source.Length];
            for (var index = 0; index < source.Length; index++)
            {
                coverage[index] = new Color(source[index].r, 0.0f, 0.0f, 1.0f);
            }
            return coverage;
        }

        private static Color[] DownsampleCoverageBox(Color[] source, int width, int height)
        {
            var destinationWidth = Math.Max(1, width >> 1);
            var destinationHeight = Math.Max(1, height >> 1);
            var destination = new Color[destinationWidth * destinationHeight];
            for (var y = 0; y < destinationHeight; y++)
            {
                for (var x = 0; x < destinationWidth; x++)
                {
                    var x0 = x * 2;
                    var y0 = y * 2;
                    var x1 = Math.Min(x0 + 1, width - 1);
                    var y1 = Math.Min(y0 + 1, height - 1);
                    var value = (source[y0 * width + x0].r + source[y0 * width + x1].r +
                                 source[y1 * width + x0].r + source[y1 * width + x1].r) * 0.25f;
                    destination[y * destinationWidth + x] = new Color(value, 0.0f, 0.0f, 1.0f);
                }
            }
            return destination;
        }

        private static void EnsureAssetFolder(string assetFolder)
        {
            if (string.IsNullOrEmpty(assetFolder) || AssetDatabase.IsValidFolder(assetFolder))
            {
                return;
            }

            var parent = Path.GetDirectoryName(assetFolder)?.Replace('\\', '/');
            if (string.IsNullOrEmpty(parent) || parent == assetFolder)
            {
                throw new ArgumentException($"'{assetFolder}' is not an Assets-relative folder.", nameof(assetFolder));
            }

            EnsureAssetFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(assetFolder));
        }

        private static void ValidateBaseTextureImportSettings(TamSourceSet source)
        {
            var errors = new List<string>();
            for (var tone = 0; tone < source.tones.Length; tone++)
            {
                var texture = source.tones[tone].sourceTexture;
                var importer = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(texture)) as TextureImporter;
                if (importer == null) continue;
                if (importer.sRGBTexture) errors.Add($"{texture.name} must use Linear colour space (sRGB disabled).");
                if (importer.wrapMode != TextureWrapMode.Repeat) errors.Add($"{texture.name} must use Repeat wrapping.");
            }
            if (errors.Count > 0) throw new ArgumentException(string.Join("\n", errors), nameof(source));
        }

        private static void ValidateEditorImportSettings(TamSourceSet source)
        {
            var errors = new List<string>();
            for (var tone = 0; tone < source.tones.Length; tone++)
            {
                foreach (var texture in source.tones[tone].authoredMips)
                {
                    var importer = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(texture)) as TextureImporter;
                    if (importer == null)
                    {
                        continue;
                    }

                    if (!importer.isReadable)
                    {
                        errors.Add($"{texture.name} must have Read/Write enabled.");
                    }
                    if (importer.sRGBTexture)
                    {
                        errors.Add($"{texture.name} must use Linear colour space (sRGB disabled).");
                    }
                    if (importer.wrapMode != TextureWrapMode.Repeat)
                    {
                        errors.Add($"{texture.name} must use Repeat wrapping.");
                    }
                }
            }

            if (errors.Count > 0)
            {
                throw new ArgumentException(string.Join("\n", errors), nameof(source));
            }
        }
    }
}
#endif
