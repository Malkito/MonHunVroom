using System;
using System.Collections.Generic;
using UnityEngine;

namespace CupOHappiness.Toon
{
    [Serializable]
    public sealed class TamToneSources
    {
        public string name;

        // Designer-facing source: one full-resolution, pixel-nested mask per tone.
        // The editor builder generates the complete mip chain from this texture.
        public Texture2D sourceTexture;

        // Legacy/advanced path: explicitly authored scale levels.
        public Texture2D[] authoredMips;
    }

    [CreateAssetMenu(menuName = "CupOHappiness/Toon/TAM Texture Set", fileName = "TAM_")]
    public sealed class TamSourceSet : ScriptableObject
    {
        [Tooltip("Ordered from the lightest (least ink) tone to the darkest (most ink) tone.")]
        public TamToneSources[] tones;

        public IReadOnlyList<string> Validate()
        {
            var errors = new List<string>();
            if (tones == null || tones.Length < 2)
            {
                errors.Add("Provide at least two tones, ordered light to dark.");
                return errors;
            }

            var baseTexture = GetTexture(0, 0);
            if (baseTexture == null)
            {
                errors.Add("Tone 0, authored mip 0 must be assigned.");
                return errors;
            }

            var mipCount = ExpectedMipCount(baseTexture.width, baseTexture.height);
            for (var tone = 0; tone < tones.Length; tone++)
            {
                var row = tones[tone];
                if (row == null || row.authoredMips == null || row.authoredMips.Length != mipCount)
                {
                    errors.Add($"Tone {tone} must provide exactly {mipCount} authored mips (mip 0 through {mipCount - 1}).");
                    continue;
                }

                for (var mip = 0; mip < mipCount; mip++)
                {
                    var texture = row.authoredMips[mip];
                    var expectedWidth = Math.Max(1, baseTexture.width >> mip);
                    var expectedHeight = Math.Max(1, baseTexture.height >> mip);
                    if (texture == null)
                    {
                        errors.Add($"Tone {tone}, mip {mip} is not assigned.");
                    }
                    else if (texture.width != expectedWidth || texture.height != expectedHeight)
                    {
                        errors.Add($"Tone {tone}, mip {mip} must be {expectedWidth}×{expectedHeight}, but is {texture.width}×{texture.height}.");
                    }
                    else if (texture.format != baseTexture.format)
                    {
                        errors.Add($"Tone {tone}, mip {mip} must use {baseTexture.format}, but uses {texture.format}.");
                    }
                }
            }

            if (errors.Count == 0)
            {
                ValidateNestedCoverage(errors, mipCount);
            }

            return errors;
        }

        /// <summary>
        /// Validates the designer workflow: one square, power-of-two base texture
        /// per tone. The editor generates a box-filtered mip chain from these.
        /// </summary>
        public IReadOnlyList<string> ValidateBaseTextures()
        {
            var errors = new List<string>();
            if (tones == null || tones.Length < 2)
            {
                errors.Add("Provide at least two base textures, ordered light to dark.");
                return errors;
            }

            var first = tones[0]?.sourceTexture;
            if (first == null)
            {
                errors.Add("Tone 0 must have a base texture assigned.");
                return errors;
            }

            if (first.width != first.height || !Mathf.IsPowerOfTwo(first.width))
            {
                errors.Add("Base textures must be square power-of-two images (for example, 512x512).");
            }

            for (var tone = 0; tone < tones.Length; tone++)
            {
                var texture = tones[tone]?.sourceTexture;
                if (texture == null)
                {
                    errors.Add($"Tone {tone} has no base texture assigned.");
                    continue;
                }

                if (texture.width != first.width || texture.height != first.height)
                {
                    errors.Add($"Tone {tone} must be {first.width}x{first.height}.");
                }
                if (!texture.isReadable)
                {
                    errors.Add($"Tone {tone} must have Read/Write enabled.");
                }

            }

            return errors;
        }

        /// <summary>
        /// Returns artistic-quality warnings without blocking the automatic
        /// builder. Nested ink is recommended for stable TAM interpolation,
        /// but stylized maps may intentionally opt out.
        /// </summary>
        public IReadOnlyList<string> GetBaseTextureNestingWarnings()
        {
            var warnings = new List<string>();
            if (tones == null || tones.Length < 2 || tones[0]?.sourceTexture == null || !tones[0].sourceTexture.isReadable)
            {
                return warnings;
            }

            var first = tones[0].sourceTexture;
            var lighterPixels = first.GetPixels();
            for (var tone = 1; tone < tones.Length; tone++)
            {
                var texture = tones[tone]?.sourceTexture;
                if (texture == null || !texture.isReadable || texture.width != first.width || texture.height != first.height)
                {
                    continue;
                }

                var darkerPixels = texture.GetPixels();
                for (var pixel = 0; pixel < lighterPixels.Length; pixel++)
                {
                    if (darkerPixels[pixel].r + 0.001f < lighterPixels[pixel].r)
                    {
                        warnings.Add($"Tone {tone} does not retain all ink from the lighter tone. The array will build, but hatch strokes may pop while lighting changes.");
                        break;
                    }
                }
                lighterPixels = darkerPixels;
            }

            return warnings;
        }

        public static int ExpectedMipCount(int width, int height)
        {
            var count = 1;
            while (width > 1 || height > 1)
            {
                width = Math.Max(1, width >> 1);
                height = Math.Max(1, height >> 1);
                count++;
            }

            return count;
        }

        private void ValidateNestedCoverage(List<string> errors, int mipCount)
        {
            for (var mip = 0; mip < mipCount; mip++)
            {
                for (var tone = 0; tone < tones.Length; tone++)
                {
                    if (!tones[tone].authoredMips[mip].isReadable)
                    {
                        errors.Add($"Tone {tone}, mip {mip} must have Read/Write enabled so nesting can be validated.");
                        return;
                    }
                }

                var lighterPixels = tones[0].authoredMips[mip].GetPixels();
                for (var tone = 1; tone < tones.Length; tone++)
                {
                    var darkerPixels = tones[tone].authoredMips[mip].GetPixels();
                    for (var pixel = 0; pixel < lighterPixels.Length; pixel++)
                    {
                        if (darkerPixels[pixel].r + 0.001f < lighterPixels[pixel].r)
                        {
                            errors.Add($"Tone {tone}, mip {mip}, pixel {pixel} does not retain ink from the lighter tone. Tones must remain pixel-nested from light to dark.");
                            break;
                        }
                    }

                    lighterPixels = darkerPixels;
                }
            }
        }

        private Texture2D GetTexture(int tone, int mip)
        {
            return tones != null && tones.Length > tone && tones[tone] != null &&
                   tones[tone].authoredMips != null && tones[tone].authoredMips.Length > mip
                ? tones[tone].authoredMips[mip]
                : null;
        }
    }
}
