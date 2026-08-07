#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace CupOHappiness.Toon.Editor
{
    /// <summary>Creates the reproducible scene used by the public rendered acceptance procedure.</summary>
    public static class CupOHappinessToonPublicAcceptanceSample
    {
        public const string SceneFolder = "Assets/Niki/Toon/Runtime/Samples/PublicAcceptance";
        public const string ScenePath = SceneFolder + "/CupOHappinessToonPublicAcceptance.unity";
        public const string AlphaClipMaterialPath = SceneFolder + "/Alpha-Clipped TAM.mat";
        public const string NormalMappedMaterialPath = SceneFolder + "/Normal-Mapped TAM.mat";
        public const string TransparentMaterialPath = SceneFolder + "/Transparent TAM.mat";
        public const string LightTamPath = SceneFolder + "/Light TAM.asset";

        private const string BaselineMaterialPath = "Assets/Runtime/Materials/mat_Kaiju_Boss_Toon.mat";
        private const string TamPath = "Assets/Niki/Toon/Runtime/Samples/TamSourceSet_TAM.asset";
        private const string KaijuPath = "Assets/Runtime/Meshes/Kaiju_Boss_v0.2.fbx";

        public static readonly string[] TamEnabledMaterialPaths =
        {
            SceneFolder + "/Static TAM.mat",
            SceneFolder + "/Scaled TAM.mat",
            AlphaClipMaterialPath,
            NormalMappedMaterialPath,
            TransparentMaterialPath,
            SceneFolder + "/Skinned TAM.mat"
        };

        [MenuItem("Tools/CupOHappiness/Toon/Create Public Acceptance Sample")]
        public static void CreatePublicAcceptanceSample()
        {
            CloseGeneratedSceneIfLoaded();
            AssetDatabase.DeleteAsset(SceneFolder);
            EnsureFolder(SceneFolder);

            var baseline = AssetDatabase.LoadAssetAtPath<Material>(BaselineMaterialPath);
            var tam = AssetDatabase.LoadAssetAtPath<Texture2DArray>(TamPath);
            if (baseline == null || tam == null)
            {
                Debug.LogError("Public acceptance sample requires the baseline CupOHappiness TAM material and TamSourceSet_TAM asset.");
                return;
            }

            var lightTam = CreateDistinctLightTamAsset(tam);
            var cutout = CreateCutoutTexture();
            var normal = CreateNormalTexture();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            CreateCamera();
            CreateLight("Main Light", new Vector3(35f, -30f, 0f), LightType.Directional, 1.2f, true);
            CreateLight("Additional Light", new Vector3(55f, 30f, 0f), LightType.Directional, 0.45f, true);

            CreatePrimitive("Static TAM", PrimitiveType.Sphere, new Vector3(-4.5f, 1f, 0f), CreateTamMaterial("Static TAM", baseline, tam, lightTam));
            var scaled = CreatePrimitive("Scaled TAM", PrimitiveType.Capsule, new Vector3(-1.5f, 1f, 0f), CreateTamMaterial("Scaled TAM", baseline, tam, lightTam));
            scaled.transform.localScale = new Vector3(1.5f, 0.65f, 0.8f);
            CreatePrimitive("Alpha-Clipped TAM", PrimitiveType.Quad, new Vector3(1.2f, 1f, 0f), CreateAlphaClipMaterial(baseline, tam, lightTam, cutout));
            CreatePrimitive("Normal-Mapped TAM", PrimitiveType.Sphere, new Vector3(4f, 1f, 0f), CreateNormalMappedMaterial(baseline, tam, lightTam, normal));
            CreatePrimitive("Transparent TAM", PrimitiveType.Cube, new Vector3(6.5f, 1f, 0f), CreateTransparentMaterial(baseline, tam, lightTam));
            CreateSkinnedCase(baseline, tam, lightTam);

            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            Debug.Log($"Created CupOHappiness TAM Toon public acceptance sample: {ScenePath}");
        }

        private static void CreateCamera()
        {
            var camera = new GameObject("Acceptance Camera").AddComponent<Camera>();
            camera.transform.SetPositionAndRotation(new Vector3(0f, 4f, -14f), Quaternion.Euler(12f, 0f, 0f));
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.18f, 0.2f, 0.24f);
        }

        private static void CreateLight(string name, Vector3 eulerAngles, LightType type, float intensity, bool shadows)
        {
            var light = new GameObject(name).AddComponent<Light>();
            light.type = type;
            light.intensity = intensity;
            light.shadows = shadows ? LightShadows.Soft : LightShadows.None;
            light.transform.rotation = Quaternion.Euler(eulerAngles);
        }

        private static GameObject CreatePrimitive(string name, PrimitiveType type, Vector3 position, Material material)
        {
            var gameObject = GameObject.CreatePrimitive(type);
            gameObject.name = name;
            gameObject.transform.position = position;
            gameObject.GetComponent<Renderer>().sharedMaterial = material;
            return gameObject;
        }

        private static void CreateSkinnedCase(Material baseline, Texture2DArray tam, Texture2DArray lightTam)
        {
            var model = AssetDatabase.LoadAssetAtPath<GameObject>(KaijuPath);
            if (model == null)
            {
                Debug.LogWarning("The Kaiju model is unavailable; the Skinned TAM case was not created.");
                return;
            }

            var instance = PrefabUtility.InstantiatePrefab(model) as GameObject;
            instance.name = "Skinned TAM";
            instance.transform.position = new Vector3(0f, 0f, 4f);
            instance.transform.localScale = Vector3.one * 0.35f;
            var material = CreateTamMaterial("Skinned TAM", baseline, tam, lightTam);
            foreach (var renderer in instance.GetComponentsInChildren<Renderer>())
            {
                renderer.sharedMaterial = material;
            }
        }

        private static Material CreateTamMaterial(string name, Material baseline, Texture2DArray tam, Texture2DArray lightTam)
        {
            var material = new Material(baseline) { name = name };
            material.SetTexture("_TAMShadowFormArray", tam);
            material.SetTexture("_TAMLightArray", lightTam);
            material.SetFloat("_TAMLightProjectionScale", 0.8f);
            material.SetFloat("_TAMLightToneBias", -0.15f);
            material.SetColor("_TAMLightInkColor", new Color(0.15f, 0.3f, 0.7f));
            material.SetFloat("_TAMLightInkOpacity", 0.55f);
            material.SetFloat("_TAMLightRange", 0.65f);
            material.SetTexture("_TAMHighlightPunchOutArray", tam);
            material.SetFloat("_TAMHighlightPunchOutProjectionScale", 1.25f);
            material.SetFloat("_TAMHighlightPunchOutToneBias", 0.1f);
            material.SetFloat("_TAMHighlightPunchOutOpacity", 0.65f);
            material.SetFloat("_TAMEnabled", 1f);
            material.EnableKeyword("_TAM_ENABLED");
            material.SetFloat("_TAMShadowFormRange", 0.5f);
            material.SetFloat("_TAMShadowFormOpacity", 0.15f);
            material.SetFloat("_TAMShadowFormInkOpacity", 0.5f);
            AssetDatabase.CreateAsset(material, SceneFolder + "/" + name + ".mat");
            return material;
        }

        private static Material CreateAlphaClipMaterial(Material baseline, Texture2DArray tam, Texture2DArray lightTam, Texture2D cutout)
        {
            var material = CreateTamMaterial("Alpha-Clipped TAM", baseline, tam, lightTam);
            material.SetTexture("_BaseMap", cutout);
            material.SetTexture("_MainTex", cutout);
            material.SetFloat("_TexMode", 1f);
            material.EnableKeyword("_TEXMODE_ONE");
            material.SetFloat("_AlphaClip", 1f);
            material.EnableKeyword("_ALPHATEST_ON");
            material.SetOverrideTag("RenderType", "TransparentCutout");
            material.renderQueue = (int)RenderQueue.AlphaTest;
            return material;
        }

        private static Material CreateNormalMappedMaterial(Material baseline, Texture2DArray tam, Texture2DArray lightTam, Texture2D normal)
        {
            var material = CreateTamMaterial("Normal-Mapped TAM", baseline, tam, lightTam);
            material.SetTexture("_BumpMap", normal);
            material.SetFloat("_ApplyNormal", 1f);
            material.EnableKeyword("_NORMALMAP");
            return material;
        }

        private static Material CreateTransparentMaterial(Material baseline, Texture2DArray tam, Texture2DArray lightTam)
        {
            var material = CreateTamMaterial("Transparent TAM", baseline, tam, lightTam);
            material.SetFloat("_CupSurface", 1f);
            material.SetFloat("_CupBlend", 0f);
            material.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
            material.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
            material.SetFloat("_ZWrite", 0f);
            material.SetColor("_BaseColor", new Color(0.75f, 0.85f, 1f, 0.55f));
            material.SetOverrideTag("RenderType", "Transparent");
            material.renderQueue = (int)RenderQueue.Transparent;
            material.SetShaderPassEnabled("ShadowCaster", false);
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            return material;
        }

        private static Texture2DArray CreateDistinctLightTamAsset(Texture2DArray source)
        {
            var lightTam = new Texture2DArray(source.width, source.height, source.depth, TextureFormat.RGBA32, true, true)
            {
                name = "Light TAM"
            };

            for (var slice = 0; slice < source.depth; slice++)
            {
                for (var mip = 0; mip < source.mipmapCount; mip++)
                {
                    var sourcePixels = source.GetPixels(slice, mip);
                    var lightPixels = new Color[sourcePixels.Length];
                    var mipWidth = Mathf.Max(1, source.width >> mip);
                    var mipHeight = Mathf.Max(1, source.height >> mip);
                    for (var y = 0; y < mipHeight; y++)
                    {
                        for (var x = 0; x < mipWidth; x++)
                        {
                            // Shift every tone and mip horizontally. This creates a
                            // visibly distinct stroke layout without changing the
                            // authored light-to-dark coverage hierarchy.
                            var sourceX = (x + Mathf.Max(1, mipWidth / 3)) % mipWidth;
                            lightPixels[y * mipWidth + x] = sourcePixels[y * mipWidth + sourceX];
                        }
                    }
                    lightTam.SetPixels(lightPixels, slice, mip);
                }
            }

            lightTam.Apply(false, true);
            AssetDatabase.CreateAsset(lightTam, LightTamPath);
            return lightTam;
        }

        private static Texture2D CreateCutoutTexture()
        {
            var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false) { name = "Acceptance Cutout" };
            texture.SetPixels(new[] { Color.white, new Color(1f, 1f, 1f, 0f), new Color(1f, 1f, 1f, 0f), Color.white });
            texture.Apply();
            AssetDatabase.CreateAsset(texture, SceneFolder + "/Acceptance Cutout.asset");
            return texture;
        }

        private static Texture2D CreateNormalTexture()
        {
            var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false, true) { name = "Acceptance Normal" };
            texture.SetPixels(new[] { new Color(0.5f, 0.5f, 1f), new Color(0.65f, 0.5f, 0.95f), new Color(0.5f, 0.65f, 0.95f), new Color(0.35f, 0.5f, 0.95f) });
            texture.Apply();
            AssetDatabase.CreateAsset(texture, SceneFolder + "/Acceptance Normal.asset");
            return texture;
        }

        private static void CloseGeneratedSceneIfLoaded()
        {
            var existing = SceneManager.GetSceneByPath(ScenePath);
            if (existing.IsValid() && existing.isLoaded)
            {
                EditorSceneManager.CloseScene(existing, true);
            }
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
