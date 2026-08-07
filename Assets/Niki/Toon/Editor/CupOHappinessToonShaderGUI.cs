#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace CupOHappiness.Toon.Editor
{
    /// <summary>
    /// Card-based material inspector for the CupOHappiness TAM Toon extension. ShaderGUI is
    /// Unity's material-inspector boundary; it keeps the existing CupOHappiness state sync
    /// while presenting CupOHappiness and extension controls in their respective groups.
    /// </summary>
    public sealed class CupOHappinessToonShaderGUI : ShaderGUI
    {
        private const string StatePrefix = "CupOHappiness.Toon.Inspector.";

        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            DrawCupOHappinessCards(materialEditor, properties);

            var tamEnabled = FindProperty("_TAMEnabled", properties, false);
            DrawCard("Hatching", materialEditor.target.GetInstanceID(), () =>
            {
                DrawProperty(materialEditor, tamEnabled, "Enable Hatching", "Enables all TAM hatching layers on this material.");
                if (tamEnabled == null || tamEnabled.floatValue <= 0.5f) return;

                DrawCard("Shadow/Form", materialEditor.target.GetInstanceID(), () =>
                {
                    DrawProperties(materialEditor, properties, new[]
                    {
                        P("_TAMShadowFormArray", "TAM Asset", "Validated TAM asset, ordered from light to dark."),
                        P("_TAMShadowFormProjectionScale", "Projection Scale", "Triplanar hatch scale in object space."),
                        P("_TAMShadowFormToneBias", "Tone Bias", "Offsets the selected TAM tone."),
                        P("_TAMShadowFormInkColor", "Ink Color", "Color applied to Shadow/Form hatch strokes."),
                        P("_TAMShadowFormInkOpacity", "Ink Opacity", "Strength of Shadow/Form hatch ink."),
                        P("_TAMShadowFormRange", "Shadow Range", "Zero disables shadow sampling; one covers the full shadow domain."),
                        P("_TAMShadowFormOpacity", "Persistent Form Opacity", "Low-frequency form hatch retained outside the shadow range.")
                    });
                });
                DrawCard("Light", materialEditor.target.GetInstanceID(), () =>
                {
                    DrawProperties(materialEditor, properties, new[]
                    {
                        P("_TAMLightArray", "TAM Asset", "Validated TAM asset, ordered from light to dark."),
                        P("_TAMLightProjectionScale", "Projection Scale", "Triplanar hatch scale in object space."),
                        P("_TAMLightToneBias", "Tone Bias", "Offsets the selected TAM tone."),
                        P("_TAMLightInkColor", "Ink Color", "Base colour for Light hatch strokes; direct light tints it."),
                        P("_TAMLightInkOpacity", "Ink Opacity", "Strength of Light hatch ink."),
                        P("_TAMLightRange", "Light Range", "Zero disables light sampling; one covers the full direct diffuse-light domain.")
                    });
                });
                DrawCard("Highlight Punch-out", materialEditor.target.GetInstanceID(), () =>
                {
                    DrawProperties(materialEditor, properties, new[]
                    {
                        P("_TAMHighlightPunchOutArray", "TAM Asset", "Validated TAM asset used only for CupOHappiness specular and rim highlights."),
                        P("_TAMHighlightPunchOutProjectionScale", "Projection Scale", "Triplanar hatch scale in object space."),
                        P("_TAMHighlightPunchOutToneBias", "Tone Bias", "Offsets the selected TAM tone."),
                        P("_TAMHighlightPunchOutOpacity", "Opacity", "Zero disables punch-out sampling; one applies the full hatch-shaped highlight mask."),
                        P("_TAMLightHighlightTransparency", "Light Hatching Transparency", "In specular and rim highlights, zero hides Light hatching and one keeps it fully visible.")
                    });
                });
                DrawCard("Hatching Influence", materialEditor.target.GetInstanceID(), () =>
                {
                    DrawProperties(materialEditor, properties, new[]
                    {
                        P("_TAMIndirectContribution", "Indirect Light Contribution", "How much indirect diffuse light affects TAM tone selection."),
                        P("_TAMAOContribution", "Occlusion Contribution", "How much ambient occlusion darkens TAM tone selection.")
                    });
                });
            });

            DrawCard("Diagnostics", materialEditor.target.GetInstanceID(), () =>
            {
                EditorGUILayout.HelpBox("Use grayscale views to inspect lighting domains and final effective layer coverage. Restore Final Composition before saving a material or Preset.", MessageType.None);
                DrawProperty(materialEditor, FindProperty("_TAMDebugView", properties, false), "View", "Final Composition is normal output; other views isolate TAM lighting and coverage.");
            });

            foreach (var target in materialEditor.targets)
            {
                SynchronizeCupOHappinessMaterialState((Material)target);
            }
        }

        private static void DrawCupOHappinessCards(MaterialEditor editor, MaterialProperty[] properties)
        {
            var materialId = editor.target.GetInstanceID();
            DrawCard("Surface Options", materialId, () =>
            {
                DrawCupOHappinessProperties(editor, properties, new[] { "_CupSurface", "_CupBlend", "_ZWrite", "_ZTest", "_Cull", "_AlphaClip", "_Cutoff", "_Coverage", "_ColorMask", "_ReceiveSSAO", "_ReceiveDecals", "_ApplyNormalDepthNormal" });
                DrawCupOHappinessCard(editor, properties, "Surface Inputs", new[] { "_BaseColor", "_ShadedBaseColor", "_TexMode", "_BaseMap", "_ShadedBaseMap", "_EnableMaskMap", "_MaskMap", "_EmissionColor", "_ApplyNormal", "_BumpMap", "_BumpScale" });
            });

            DrawCard("Toon Lighting", materialId, () =>
            {
                DrawCupOHappinessProperties(editor, properties, new[] { "_Steps", "_DiffuseFallOff", "_DiffuseStep", "_Ramp", "_GradientMap", "_OcclusionStrength" });
                DrawCupOHappinessCard(editor, properties, "Advanced Toon Lighting", new[] { "_ColorizedShadowsMain", "_ColorizedShadowsAdd", "_LightColorContribution", "_AddLightFallOff" });
                DrawCupOHappinessCard(editor, properties, "Specular Toon Lighting", new[] { "_SpecularHighlights", "_Anisotropic", "_Anisotropy", "_EnergyConservation", "_SpecColor", "_SpecColor2nd", "_Smoothness", "_SpecularStep", "_SpecularFallOff" });
                DrawCupOHappinessCard(editor, properties, "Toon Rim", new[] { "_EnableToonRim", "_ToonRimColor", "_ToonRimPower", "_ToonRimFallOff", "_ToonRimAttenuation" });
                DrawCupOHappinessCard(editor, properties, "Toon Shadows", new[] { "_ReceiveShadows", "_ShadowOffset", "_ShadowFallOff", "_ShadoBiasDirectional", "_ShadowBiasAdditional" });
                DrawCupOHappinessCard(editor, properties, "Toon Outline", new[] { "_OutlineColor", "_Border", "_CompensateScale", "_OutlineInScreenSpace", "_ZWriteOutline", "_ZTestOutline", "_CullOutline" });
                DrawCupOHappinessCard(editor, properties, "Rim Lighting", new[] { "_Rim", "_RimColor", "_RimPower", "_RimFrequency", "_RimMinPower", "_RimPerPositionFrequency" });
            });

            DrawCupOHappinessCard(editor, properties, "Decals", new[] { "_ShadedDecalColor" });
            DrawCupOHappinessCard(editor, properties, "Stencil", new[] { "_Stencil", "_ReadMask", "_WriteMask", "_StencilComp", "_StencilOp", "_StencilFail", "_StencilZFail" });
            DrawCupOHappinessCard(editor, properties, "Advanced", new[] { "_EnvironmentReflections" });
            DrawCupOHappinessCard(editor, properties, "Render Queue", new[] { "_QueueOffset" });
        }

        private static void DrawCupOHappinessCard(MaterialEditor editor, MaterialProperty[] properties, string title, string[] propertyNames)
        {
            DrawCard(title, editor.target.GetInstanceID(), () => DrawCupOHappinessProperties(editor, properties, propertyNames));
        }

        private static void DrawCupOHappinessProperties(MaterialEditor editor, MaterialProperty[] properties, string[] propertyNames)
        {
            foreach (var name in propertyNames)
            {
                DrawProperty(editor, FindProperty(name, properties, false), Nicify(name), CupOHappinessTooltip(name));
            }
        }

        private static void DrawProperties(MaterialEditor editor, MaterialProperty[] properties, PropertyLabel[] labels)
        {
            foreach (var label in labels)
            {
                DrawProperty(editor, FindProperty(label.name, properties, false), label.label, label.tooltip);
            }
        }

        private static void DrawProperty(MaterialEditor editor, MaterialProperty property, string label, string tooltip)
        {
            if (property != null) editor.ShaderProperty(property, new GUIContent(label, tooltip));
        }

        private static void DrawCard(string title, int materialId, Action content)
        {
            var stateKey = StatePrefix + materialId + "." + title;
            var expanded = SessionState.GetBool(stateKey, true);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            var next = EditorGUILayout.Foldout(expanded, title, true, EditorStyles.foldoutHeader);
            if (next != expanded) SessionState.SetBool(stateKey, next);
            if (next)
            {
                EditorGUILayout.Space(2);
                EditorGUI.indentLevel++;
                content();
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(2);
        }

        private static void SynchronizeCupOHappinessMaterialState(Material material)
        {
            var queueOffset = material.HasProperty("_QueueOffset") ? material.GetInt("_QueueOffset") : 0;
            if (material.HasProperty("_TAMEnabled")) SetKeyword(material, "_TAM_ENABLED", material.GetFloat("_TAMEnabled") > 0.5f);
            if (material.HasProperty("_AlphaClip")) SetKeyword(material, "_ALPHATEST_ON", material.GetFloat("_AlphaClip") > 0.5f);
            var isTransparent = material.HasProperty("_CupSurface") && material.GetFloat("_CupSurface") > 0.5f;
            var alphaClipped = material.HasProperty("_AlphaClip") && material.GetFloat("_AlphaClip") > 0.5f;

            if (material.HasProperty("_DisableGBufferPass")) material.SetShaderPassEnabled("UniversalGBuffer", material.GetFloat("_DisableGBufferPass") < 0.5f);
            if (material.HasProperty("_Cull")) material.doubleSidedGI = material.GetFloat("_Cull") < 0.5f;
            if (material.HasProperty("_Emission")) material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.BakedEmissive | (material.GetFloat("_Emission") > 0.5f ? 0 : MaterialGlobalIlluminationFlags.EmissiveIsBlack);

            material.SetShaderPassEnabled("ShadowCaster", !isTransparent);
            if (isTransparent)
            {
                material.SetInt("_ZWrite", 0);
                material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                switch (material.HasProperty("_CupBlend") ? (int)material.GetFloat("_CupBlend") : 0)
                {
                    case 1: SetBlend(material, BlendMode.One, BlendMode.OneMinusSrcAlpha, true, false); break;
                    case 2: SetBlend(material, BlendMode.One, BlendMode.One, true, false); break;
                    case 3: SetBlend(material, BlendMode.DstColor, BlendMode.Zero, false, true); break;
                    default: SetBlend(material, BlendMode.SrcAlpha, BlendMode.OneMinusSrcAlpha, false, false); break;
                }
            }
            else
            {
                material.SetInt("_ZWrite", 1);
                material.SetInt("_SrcBlend", (int)BlendMode.One);
                material.SetInt("_DstBlend", (int)BlendMode.Zero);
                material.DisableKeyword("_SURFACE_TYPE_TRANSPARENT");
            }

            material.SetOverrideTag("RenderType", isTransparent ? "Transparent" : alphaClipped ? "TransparentCutout" : "Opaque");
            material.renderQueue = (isTransparent ? (int)RenderQueue.Transparent : alphaClipped ? (int)RenderQueue.AlphaTest : (int)RenderQueue.Geometry) + queueOffset;
            if (material.HasProperty("_ColorMask") && material.GetFloat("_ColorMask") < 0.5f) material.renderQueue = (int)RenderQueue.Transparent + queueOffset;
            if (material.HasProperty("_MainTex") && material.HasProperty("_BaseMap"))
            {
                var source = material.HasProperty("_AlphaFromMaskMap") && material.GetFloat("_AlphaFromMaskMap") > 0.5f && material.HasProperty("_MaskMap") ? material.GetTexture("_MaskMap") : material.GetTexture("_BaseMap");
                material.SetTexture("_MainTex", source);
            }
        }

        private static void SetKeyword(Material material, string keyword, bool enabled)
        {
            if (enabled) material.EnableKeyword(keyword); else material.DisableKeyword(keyword);
        }

        private static void SetBlend(Material material, BlendMode source, BlendMode destination, bool premultiply, bool modulate)
        {
            material.SetInt("_SrcBlend", (int)source);
            material.SetInt("_DstBlend", (int)destination);
            material.SetInt("_SrcBlendAlpha", (int)BlendMode.One);
            material.SetInt("_DstBlendAlpha", destination == BlendMode.OneMinusSrcAlpha && premultiply ? (int)BlendMode.OneMinusSrcAlpha : (int)BlendMode.Zero);
            if (premultiply) material.EnableKeyword("_ALPHAPREMULTIPLY_ON"); else material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            if (modulate) material.EnableKeyword("_ALPHAMODULATE_ON"); else material.DisableKeyword("_ALPHAMODULATE_ON");
        }

        private static string Nicify(string propertyName) => ObjectNames.NicifyVariableName(propertyName.TrimStart('_'));
        private static string CupOHappinessTooltip(string propertyName)
        {
            switch (propertyName)
            {
                case "_CupSurface": return "Choose whether the material is solid or see-through.";
                case "_CupBlend": return "Choose how a transparent material blends with what is behind it.";
                case "_ZWrite": return "Controls whether this material writes to the depth buffer.";
                case "_ZTest": return "Controls when this material passes the depth test.";
                case "_Cull": return "Choose which side of a surface Unity draws.";
                case "_AlphaClip": return "Cuts away pixels below the alpha threshold for clean holes and edges.";
                case "_Cutoff": return "Sets the alpha value where pixels are cut away.";
                case "_Coverage": return "Uses alpha-to-coverage when supported to soften clipped edges.";
                case "_ColorMask": return "Choose whether the material writes colour, depth only, or both.";
                case "_ReceiveSSAO": return "Lets screen-space ambient occlusion darken this material.";
                case "_ReceiveDecals": return "Lets projected decals affect this material.";
                case "_ApplyNormalDepthNormal": return "Uses the normal map when Unity builds the depth-normal texture.";
                case "_ShadedDecalColor": return "Multiplies the colour of decals in shaded areas.";
                case "_Steps": return "Sets the number of distinct light and shadow bands.";
                case "_DiffuseFallOff": return "Softens the edge between diffuse lighting bands.";
                case "_DiffuseStep": return "Moves the diffuse light-to-shadow boundary.";
                case "_Ramp": return "Choose whether a ramp texture shapes the diffuse lighting.";
                case "_GradientMap": return "Colour ramp used to shape the diffuse lighting bands.";
                case "_OcclusionStrength": return "Controls how strongly occlusion darkens the material.";
                case "_ColorizedShadowsMain": return "Lets the main light colour tint shaded areas.";
                case "_ColorizedShadowsAdd": return "Lets additional light colours tint shaded areas.";
                case "_LightColorContribution": return "Sets how much light colour affects the material.";
                case "_AddLightFallOff": return "Controls how quickly additional lights fade across the surface.";
                case "_SpecularHighlights": return "Adds stylized shiny highlights from direct lights.";
                case "_Anisotropic": return "Stretches highlights along the surface direction, useful for hair or brushed metal.";
                case "_Anisotropy": return "Sets the direction and strength of stretched highlights.";
                case "_EnergyConservation": return "Balances diffuse and specular light so the surface does not become overly bright.";
                case "_SpecColor": return "Sets the colour of the main shiny highlight.";
                case "_SpecColor2nd": return "Sets the colour of the secondary shiny highlight.";
                case "_Smoothness": return "Higher values make highlights smaller and sharper.";
                case "_SpecularStep": return "Moves the boundary where the specular highlight appears.";
                case "_SpecularFallOff": return "Softens the edge of the specular highlight.";
                case "_EnableToonRim": return "Adds a stylized light band around the silhouette.";
                case "_ToonRimColor": return "Sets the colour of the toon rim light.";
                case "_ToonRimPower": return "Controls how far the toon rim spreads from the edge.";
                case "_ToonRimFallOff": return "Controls how sharply the toon rim fades.";
                case "_ToonRimAttenuation": return "Reduces the toon rim in darker or more distant lighting.";
                case "_ReceiveShadows": return "Lets realtime shadows affect this material.";
                case "_ShadowOffset": return "Moves the toon shadow boundary to fine-tune its placement.";
                case "_ShadowFallOff": return "Softens the edge of realtime shadows.";
                case "_ShadoBiasDirectional": return "Adjusts directional-light shadow bias to reduce artifacts.";
                case "_ShadowBiasAdditional": return "Adjusts additional-light shadow bias to reduce artifacts.";
                case "_OutlineColor": return "Sets the colour and transparency of the outline.";
                case "_Border": return "Sets the outline width.";
                case "_CompensateScale": return "Keeps the outline width more consistent when the object is scaled.";
                case "_OutlineInScreenSpace": return "Measures outline width in screen pixels instead of object space.";
                case "_ZWriteOutline": return "Controls whether the outline writes to the depth buffer.";
                case "_ZTestOutline": return "Controls when the outline passes the depth test.";
                case "_CullOutline": return "Choose which side of the outline Unity draws.";
                case "_BaseColor": return "Base colour and transparency of the material.";
                case "_ShadedBaseColor": return "Base colour used in shaded regions.";
                case "_TexMode": return "Choose how base textures are used by the toon material.";
                case "_BaseMap": return "Main colour texture; its alpha can control transparency or clipping.";
                case "_ShadedBaseMap": return "Colour texture used in shaded regions.";
                case "_EnableMaskMap": return "Uses a mask map for emission, specular, occlusion, and smoothness.";
                case "_MaskMap": return "Packed map: red emission, green specular, blue occlusion, alpha smoothness.";
                case "_EmissionColor": return "Adds light from the material itself, independent of scene lights.";
                case "_ApplyNormal": return "Uses the normal map to change how light moves across the surface.";
                case "_BumpMap": return "Normal map that adds small surface detail without extra geometry.";
                case "_BumpScale": return "Sets the strength of the normal-map detail.";
                case "_Rim": return "Adds a rim light around the silhouette.";
                case "_RimColor": return "Sets the colour of the rim light.";
                case "_RimPower": return "Controls how close the rim light stays to the silhouette.";
                case "_RimFrequency": return "Varies the rim light along the surface.";
                case "_RimMinPower": return "Sets the minimum width of the rim light.";
                case "_RimPerPositionFrequency": return "Controls how much rim variation follows object position.";
                case "_Stencil": return "Reference value used when this material writes to the stencil buffer.";
                case "_ReadMask": return "Bit mask used when Unity reads stencil values.";
                case "_WriteMask": return "Bit mask that limits which stencil bits this material can change.";
                case "_StencilComp": return "Comparison Unity uses before drawing pixels based on the stencil buffer.";
                case "_StencilOp": return "Action applied to the stencil buffer when the stencil test passes.";
                case "_StencilFail": return "Action applied to the stencil buffer when the stencil test fails.";
                case "_StencilZFail": return "Action applied to the stencil buffer when the depth test fails.";
                case "_EnvironmentReflections": return "Lets environment reflections contribute to the material.";
                case "_QueueOffset": return "Moves the draw order slightly within this material type.";
                default: return "CupOHappiness material control.";
            }
        }
        private static PropertyLabel P(string name, string label, string tooltip) => new PropertyLabel(name, label, tooltip);

        private readonly struct PropertyLabel
        {
            public readonly string name;
            public readonly string label;
            public readonly string tooltip;
            public PropertyLabel(string name, string label, string tooltip) { this.name = name; this.label = label; this.tooltip = tooltip; }
        }
    }
}
#endif
