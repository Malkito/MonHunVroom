#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace CupOHappiness.Toon.Editor
{
    /// <summary>Converts scene- or Project-window-selected materials to and from CupOHappiness Toon.</summary>
    public sealed class CupOHappinessMaterialConverterWindow : EditorWindow
    {
        private const string ToonShaderName = "CupOHappiness/Toon & Outline";
        private const string UrpLitShaderName = "Universal Render Pipeline/Lit";
        private const string UrpSimpleLitShaderName = "Universal Render Pipeline/Simple Lit";
        private const string UrpUnlitShaderName = "Universal Render Pipeline/Unlit";

        private enum Scope { SelectionAndChildren, ActiveScene, ProjectSelection }
        private Scope scope = Scope.SelectionAndChildren;
        private Material toonSettingsReference;

        [MenuItem("Tools/CupOHappiness/Toon/Convert Scene Materials...")]
        public static void ShowWindow()
        {
            var window = GetWindow<CupOHappinessMaterialConverterWindow>("Convert Materials");
            window.minSize = new Vector2(460, 330);
            window.Show();
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("CupOHappiness Toon Material Converter", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Convert materials assigned to scene renderers, or select material assets and folders in the Project window. " +
                "A Toon Settings Reference copies its Toon controls first; source surface settings and inputs then replace the reference values.",
                MessageType.Info);

            scope = (Scope)EditorGUILayout.EnumPopup(
                new GUIContent("Source", "Choose scene renderers or selected material assets/folders in the Project window."), scope);

            var toonCandidates = CollectMaterials(IsUrpSourceMaterial);
            var litCandidates = CollectMaterials(IsCupOHappinessToon);
            DrawSourceSummary(toonCandidates.Count, litCandidates.Count);

            EditorGUILayout.Space(8);
            toonSettingsReference = (Material)EditorGUILayout.ObjectField(
                new GUIContent("Toon Settings Reference", "Optional CupOHappiness Toon material. Its Toon settings are copied, then source surface settings and inputs are restored."),
                toonSettingsReference, typeof(Material), false);
            var referenceIsValid = toonSettingsReference == null || IsCupOHappinessToon(toonSettingsReference);
            if (!referenceIsValid)
            {
                EditorGUILayout.HelpBox("The reference must use the CupOHappiness Toon shader.", MessageType.Error);
            }

            using (new EditorGUI.DisabledScope(toonCandidates.Count == 0 || !referenceIsValid))
            {
                if (GUILayout.Button("Convert to Toon Shader", GUILayout.Height(30)))
                {
                    ConvertToToon(toonCandidates, toonSettingsReference);
                }
            }
            using (new EditorGUI.DisabledScope(litCandidates.Count == 0))
            {
                if (GUILayout.Button("Convert to URP Lit", GUILayout.Height(30)))
                {
                    ConvertToUrpLit(litCandidates);
                }
            }
        }

        private void DrawSourceSummary(int toonCandidateCount, int litCandidateCount)
        {
            if (scope == Scope.ProjectSelection)
            {
                EditorGUILayout.LabelField("Selected Project Materials", (toonCandidateCount + litCandidateCount).ToString());
            }
            else
            {
                EditorGUILayout.LabelField("Renderers", CollectRenderers().Count.ToString());
            }
            EditorGUILayout.LabelField("URP Lit / Simple Lit / Unlit Materials", toonCandidateCount.ToString());
            EditorGUILayout.LabelField("CupOHappiness Toon Materials", litCandidateCount.ToString());
        }

        private List<Material> CollectMaterials(Predicate<Material> matches)
        {
            return scope == Scope.ProjectSelection
                ? CollectProjectMaterials(matches)
                : CollectRendererMaterials(CollectRenderers(), matches);
        }

        private List<Renderer> CollectRenderers()
        {
            var renderers = new List<Renderer>();
            if (scope == Scope.ActiveScene)
            {
                foreach (var renderer in FindObjectsByType<Renderer>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                {
                    if (renderer.gameObject.scene == SceneManager.GetActiveScene()) renderers.Add(renderer);
                }
                return renderers;
            }

            var seen = new HashSet<Renderer>();
            foreach (var gameObject in Selection.gameObjects)
            {
                foreach (var renderer in gameObject.GetComponentsInChildren<Renderer>(true))
                {
                    if (seen.Add(renderer)) renderers.Add(renderer);
                }
            }
            return renderers;
        }

        private static List<Material> CollectRendererMaterials(IEnumerable<Renderer> renderers, Predicate<Material> matches)
        {
            var materials = new List<Material>();
            var seen = new HashSet<Material>();
            foreach (var renderer in renderers)
            {
                foreach (var material in renderer.sharedMaterials)
                {
                    if (material != null && matches(material) && seen.Add(material)) materials.Add(material);
                }
            }
            return materials;
        }

        private static List<Material> CollectProjectMaterials(Predicate<Material> matches)
        {
            var materials = new List<Material>();
            var seen = new HashSet<Material>();
            foreach (var selectedObject in Selection.objects)
            {
                var path = AssetDatabase.GetAssetPath(selectedObject);
                if (string.IsNullOrEmpty(path)) continue;

                if (selectedObject is Material material)
                {
                    AddProjectMaterial(material, matches, seen, materials);
                    continue;
                }

                if (!AssetDatabase.IsValidFolder(path)) continue;
                foreach (var guid in AssetDatabase.FindAssets("t:Material", new[] { path }))
                {
                    AddProjectMaterial(AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(guid)), matches, seen, materials);
                }
            }
            return materials;
        }

        private static void AddProjectMaterial(Material material, Predicate<Material> matches, HashSet<Material> seen, List<Material> materials)
        {
            if (material != null && matches(material) && seen.Add(material)) materials.Add(material);
        }

        private void ConvertToToon(List<Material> materials, Material reference)
        {
            var targetShader = Shader.Find(ToonShaderName);
            if (targetShader == null)
            {
                EditorUtility.DisplayDialog("Shader unavailable", $"Could not find '{ToonShaderName}'.", "OK");
                return;
            }

            var referenceDescription = reference == null ? "default Toon settings" : $"Toon settings from '{reference.name}'";
            if (!ConfirmConversion(materials.Count, $"Convert to CupOHappiness Toon using {referenceDescription}?")) return;

            Undo.RecordObjects(materials.ToArray(), "Convert to CupOHappiness Toon");
            foreach (var material in materials)
            {
                var sourceValues = MaterialValues.Capture(material);
                material.shader = targetShader;
                if (reference != null) material.CopyPropertiesFromMaterial(reference);
                sourceValues.ApplyToToon(material, reference == null);
                EditorUtility.SetDirty(material);
            }
            AssetDatabase.SaveAssets();
            ShowNotification(new GUIContent($"Converted {materials.Count} material asset(s) to Toon."));
        }

        private void ConvertToUrpLit(List<Material> materials)
        {
            var targetShader = Shader.Find(UrpLitShaderName);
            if (targetShader == null)
            {
                EditorUtility.DisplayDialog("Shader unavailable", $"Could not find '{UrpLitShaderName}'.", "OK");
                return;
            }
            if (!ConfirmConversion(materials.Count, "Convert to URP Lit?")) return;

            Undo.RecordObjects(materials.ToArray(), "Convert to URP Lit");
            foreach (var material in materials)
            {
                var values = MaterialValues.Capture(material);
                material.shader = targetShader;
                values.ApplyToUrpLit(material);
                EditorUtility.SetDirty(material);
            }
            AssetDatabase.SaveAssets();
            ShowNotification(new GUIContent($"Converted {materials.Count} material asset(s) to URP Lit."));
        }

        private static bool ConfirmConversion(int count, string action)
        {
            return EditorUtility.DisplayDialog(
                "Convert Shared Materials",
                $"{action}\n\nThis modifies {count} shared material asset(s) and every object that uses them. You can undo this operation.",
                "Convert", "Cancel");
        }

        private static bool IsUrpSourceMaterial(Material material)
        {
            if (material.shader == null) return false;
            var shaderName = material.shader.name;
            return shaderName == UrpLitShaderName || shaderName == UrpSimpleLitShaderName || shaderName == UrpUnlitShaderName;
        }

        private static bool IsCupOHappinessToon(Material material) => material.shader != null && material.shader.name == ToonShaderName;

        private readonly struct MaterialValues
        {
            private readonly Texture baseMap;
            private readonly Color baseColor;
            private readonly Texture bumpMap;
            private readonly float bumpScale;
            private readonly Color emissionColor;
            private readonly float smoothness;
            private readonly float surface;
            private readonly float blend;
            private readonly float cull;
            private readonly float alphaClip;
            private readonly float cutoff;
            private readonly float zTest;
            private readonly float queueOffset;
            private readonly int renderQueue;
            private readonly Vector2 baseMapScale;
            private readonly Vector2 baseMapOffset;

            private MaterialValues(Material material)
            {
                baseMap = GetTexture(material, "_BaseMap");
                baseColor = GetColor(material, "_BaseColor", Color.white);
                bumpMap = GetTexture(material, "_BumpMap");
                bumpScale = GetFloat(material, "_BumpScale", 1f);
                emissionColor = GetColor(material, "_EmissionColor", Color.black);
                smoothness = GetFloat(material, "_Smoothness", 0.5f);
                surface = GetFloat(material, material.HasProperty("_CupSurface") ? "_CupSurface" : "_Surface", 0f);
                blend = GetFloat(material, material.HasProperty("_CupBlend") ? "_CupBlend" : "_Blend", 0f);
                cull = GetFloat(material, "_Cull", 2f);
                alphaClip = GetFloat(material, "_AlphaClip", 0f);
                cutoff = GetFloat(material, "_Cutoff", 0.5f);
                zTest = GetFloat(material, "_ZTest", 4f);
                queueOffset = GetFloat(material, "_QueueOffset", 0f);
                renderQueue = material.renderQueue;
                baseMapScale = baseMap != null && material.HasProperty("_BaseMap") ? material.GetTextureScale("_BaseMap") : Vector2.one;
                baseMapOffset = baseMap != null && material.HasProperty("_BaseMap") ? material.GetTextureOffset("_BaseMap") : Vector2.zero;
            }

            public static MaterialValues Capture(Material material) => new MaterialValues(material);

            public void ApplyToToon(Material material, bool disableTam)
            {
                ApplySharedInputs(material);
                SetFloat(material, "_CupSurface", surface);
                SetFloat(material, "_CupBlend", blend);
                if (disableTam) SetKeyword(material, "_TAM_ENABLED", false);
                ApplyRenderState(material, surface > 0.5f, alphaClip > 0.5f, (int)blend, renderQueue);
            }

            public void ApplyToUrpLit(Material material)
            {
                ApplySharedInputs(material);
                SetFloat(material, "_Surface", surface);
                SetFloat(material, "_Blend", blend);
                ApplyRenderState(material, surface > 0.5f, alphaClip > 0.5f, (int)blend, renderQueue);
            }

            private void ApplySharedInputs(Material material)
            {
                SetTexture(material, "_BaseMap", baseMap);
                SetColor(material, "_BaseColor", baseColor);
                SetTexture(material, "_BumpMap", bumpMap);
                SetFloat(material, "_BumpScale", bumpScale);
                SetColor(material, "_EmissionColor", emissionColor);
                SetFloat(material, "_Smoothness", smoothness);
                SetFloat(material, "_Cull", cull);
                SetFloat(material, "_AlphaClip", alphaClip);
                SetFloat(material, "_Cutoff", cutoff);
                SetFloat(material, "_ZTest", zTest);
                SetFloat(material, "_QueueOffset", queueOffset);
                if (material.HasProperty("_BaseMap"))
                {
                    material.SetTextureScale("_BaseMap", baseMapScale);
                    material.SetTextureOffset("_BaseMap", baseMapOffset);
                }
                SetKeyword(material, "_ALPHATEST_ON", alphaClip > 0.5f);
                SetKeyword(material, "_NORMALMAP", bumpMap != null);
                SetKeyword(material, "_EMISSION", emissionColor.maxColorComponent > 0f);
            }

            private static void ApplyRenderState(Material material, bool transparent, bool alphaClipped, int blendMode, int sourceRenderQueue)
            {
                material.SetShaderPassEnabled("ShadowCaster", !transparent);
                SetKeyword(material, "_SURFACE_TYPE_TRANSPARENT", transparent);
                SetKeyword(material, "_ALPHAPREMULTIPLY_ON", transparent && (blendMode == 1 || blendMode == 2));
                SetKeyword(material, "_ALPHAMODULATE_ON", transparent && blendMode == 3);
                if (transparent)
                {
                    material.SetInt("_ZWrite", 0);
                    switch (blendMode)
                    {
                        case 1: SetBlend(material, BlendMode.One, BlendMode.OneMinusSrcAlpha); break;
                        case 2: SetBlend(material, BlendMode.One, BlendMode.One); break;
                        case 3: SetBlend(material, BlendMode.DstColor, BlendMode.Zero); break;
                        default: SetBlend(material, BlendMode.SrcAlpha, BlendMode.OneMinusSrcAlpha); break;
                    }
                    material.SetOverrideTag("RenderType", "Transparent");
                }
                else
                {
                    material.SetInt("_ZWrite", 1);
                    SetBlend(material, BlendMode.One, BlendMode.Zero);
                    material.SetOverrideTag("RenderType", alphaClipped ? "TransparentCutout" : "Opaque");
                }
                material.renderQueue = sourceRenderQueue >= 0 ? sourceRenderQueue : transparent ? (int)RenderQueue.Transparent : alphaClipped ? (int)RenderQueue.AlphaTest : (int)RenderQueue.Geometry;
            }

            private static void SetBlend(Material material, BlendMode source, BlendMode destination)
            {
                SetFloat(material, "_SrcBlend", (float)source);
                SetFloat(material, "_DstBlend", (float)destination);
            }

            private static float GetFloat(Material material, string property, float fallback) => material.HasProperty(property) ? material.GetFloat(property) : fallback;
            private static Color GetColor(Material material, string property, Color fallback) => material.HasProperty(property) ? material.GetColor(property) : fallback;
            private static Texture GetTexture(Material material, string property) => material.HasProperty(property) ? material.GetTexture(property) : null;
            private static void SetFloat(Material material, string property, float value) { if (material.HasProperty(property)) material.SetFloat(property, value); }
            private static void SetColor(Material material, string property, Color value) { if (material.HasProperty(property)) material.SetColor(property, value); }
            private static void SetTexture(Material material, string property, Texture value) { if (material.HasProperty(property)) material.SetTexture(property, value); }
            private static void SetKeyword(Material material, string keyword, bool enabled) { if (enabled) material.EnableKeyword(keyword); else material.DisableKeyword(keyword); }
        }
    }
}
#endif
