# CupOHappiness Toon & Outline shader reference

This is a Markdown reference for the CupOHappiness Toon & Outline HLSL shader used as the base of **CupOHappiness TAM Toon & Outline**. It is transcribed and condensed from **CupOHappiness URP Toon Shading 1.3** bundled with CupOHappiness URP Essentials 2.07. Refer to that PDF for screenshots and the original wording.

> TAM controls are extension-owned and documented separately. The controls below remain CupOHappiness controls.

## Requirements and Unity 6 outlines

- Use Unity 6 with Universal Render Pipeline 17.0.0 or newer. The shader was tested with Unity 6000.3 / URP 17.3; Unity 6000.0 projects can use their compatible URP 17.0 release.
- A `.unitypackage` cannot install URP for you. Install and configure URP before importing this Toon package.
- The HLSL Toon & Outline shader supports steps or ramps, multiple toon controls, specular highlights, shadows, stencil settings, alpha clipping, and outlines.
- In Unity 6, outlines are scheduled by `CupOHappinessToonOutlineRendererFeature`, not by a second material or a normal two-pass shader setup.
- Add **CupOHappinessToonOutlineRendererFeature** to the active Universal Renderer Data. Its **Layer Mask** must include the outlined objects.
- Use **After Rendering Opaques** for fully opaque outlines. Semi-transparent outlines require choosing an injection point for the scene; separating outline layers across multiple renderer-feature instances can help.

## Surface Options

| Control | Effect |
| --- | --- |
| Surface Type | Chooses opaque or transparent rendering where supported. |
| Blending | Chooses how transparent pixels blend with the background. |
| ZTest | Sets the depth comparison used before a pixel is drawn. |
| Culling | Draws back faces, front faces, or both. The usual default is back-face culling. |
| Alpha Clipping | Discards pixels below the alpha threshold; use it for hard cutouts. |
| Threshold | Sets the alpha value where clipping occurs. |
| Alpha To Coverage | Uses MSAA coverage to soften clipped edges; visible in Game view. |
| Color Mask | In older URP versions, can output colour normally or depth only. Depth-only is an experimental outline-only workflow. |
| Receive SSAO | Lets screen-space ambient occlusion affect the material. Disabling it can preserve cleaner toon bands. |
| Receive Decals | Enables URP DBuffer decals. Toon shaders sample decal albedo and light it with the toon model. |
| Enable Normal in Depth Normal Pass | Uses the normal map in the depth-normal pass. Disabling it can improve performance but changes SSAO and decal results. |

## Decals

**Shaded Decal Color** is a multiplier applied to the sampled URP decal albedo in shaded areas. Set it to white to leave the decal colour unchanged.

## Toon Lighting

| Control | Effect |
| --- | --- |
| Steps | Quantizes N·L lighting into 1–8 toon bands. CupOHappiness calculates and anti-aliases these bands analytically. |
| Diffuse Falloff | Widens the anti-aliased transition between diffuse bands. |
| Diffuse Step | Moves the light/shadow boundaries across the model, allowing artistic shape control and helping hide shadow leaks. |
| Ramp Mode | Replaces step lighting with a ramp texture: **Off** uses steps, **Smooth Sampling** is for smooth or wide stepped ramps, and **Point Sampling** is for small non-linear stepped ramps. |
| Ramp | The texture used for ramp lighting; CupOHappiness samples its red channel. For desktop/console use, BC4/R compression can be appropriate. |
| Occlusion | Controls ambient occlusion. At zero, diffuse ambient light and specular reflections are fully suppressed. |

## Advanced Toon Lighting

| Control | Effect |
| --- | --- |
| Colorize Main Light | At one, the directional light colours all pixels, including unlit ones; normal N·L attenuation and shadows are ignored. |
| Colorize Add Light | The equivalent behaviour for additional lights, using distance attenuation. |
| Light Color Contribution | At zero, lights are fully desaturated and contribute only luminance. |
| Light Falloff | Changes point and spot light falloff. Very low values reduce their smooth falloff for a more graphic result. Combine with Unity's inner/outer spot-angle settings. |

## Specular Toon Lighting

CupOHappiness uses Blinn-Phong-style specular lighting, not PBR specular lighting.

| Control | Effect |
| --- | --- |
| Enable Specular Highlights | Enables stylized shiny highlights. |
| Anisotropic Specular / Anisotropy | Enables and directs stretched GGX-style highlights along tangent or bitangent. It costs more and generally needs smoothness retuning. |
| Energy Conservation | Makes smoothness affect highlight brightness as well as size. Without it, smoothness mainly controls size. |
| Specular / Secondary Specular | Colours for the highlight. A Mask Map selects between them; white uses Specular and black uses Secondary Specular. |
| Smoothness | Controls highlight size and, with energy conservation, brightness. It also controls environment reflections where enabled. |
| Specular Step | Sets where a highlight begins; values near 0.5 are typical. |
| Specular Falloff | Softens the highlight edge. Smaller values make a sharper edge. |

## Toon Rim and Toon Shadows

| Group | Control | Effect |
| --- | --- | --- |
| Toon Rim | Rim Color | Colour of the rim light. |
| Toon Rim | Rim Power | Width of the rim lighting. |
| Toon Rim | Rim Falloff | Edge softness; lower values are sharper. |
| Toon Rim | Rim Attenuation | At zero the rim remains full in shade; at one it is cancelled by N·L and shadows. |
| Toon Shadows | Receive Shadows | Enables realtime shadows on the material. |
| Toon Shadows | Shadow Offset | Offsets cast shadows to reduce self-shadowing artifacts. Default is 1. |
| Toon Shadows | Shadow Falloff | Shadow-edge softness. A value around 0.5 is a useful starting point. |
| Toon Shadows | Shadow Bias Directional / Additional | Values above zero reduce directional or additional-light shadow strength. |

## Toon Outline

The following controls are available on the Toon & Outline HLSL shader.

| Control | Effect |
| --- | --- |
| Color | Outline RGB colour and alpha opacity. |
| Width | Outline width. It normally changes with object scale. |
| Compensate Object Scale | Keeps width more consistent across scaled instances. Complex skinned hierarchies can still need adjustment. |
| Calculate Width in Screen Space | Keeps width stable with distance, though it may look less natural for toon outlines. |
| ZWrite Outline | Controls whether the outline writes depth. |
| ZTest Outline | Depth test for the outline. Use **Less** when alpha clipping is enabled. |
| Culling Outline | Face culling for the outline; front-face culling is the normal default. |

### Alpha-clipped outlines

For alpha-clipped Toon & Outline materials, set **ZTest Outline** to **Less**. CupOHappiness draws the outline after the shaded surface and uses depth to prevent the outline from drawing over the visible shaded area. This avoids gaps without an extra alpha lookup.

## Surface Inputs

| Control | Effect |
| --- | --- |
| Color | Diffuse RGB colour and alpha. |
| Shaded Color | Diffuse RGB colour for unlit pixels. |
| Texture Mode | **Off:** no diffuse textures; **One:** lit Albedo only; **Two:** lit Albedo and Shaded Albedo. |
| Albedo | Diffuse texture for lit pixels. Its alpha can drive clipping or transparency. Tiling and offset affect texture lookups. |
| Shaded Albedo | Diffuse texture for unlit pixels. |
| Enable Mask Map | Enables the packed Mask Map. |
| Mask Map | Red: emission mask; green: specular-colour selection; blue: occlusion; alpha: smoothness. |
| Emission Color | Applied when the Mask Map is enabled. |
| Enable Normal Map / Normal Map / Normal Scale | Enables and controls normal-map surface detail. |

## Shader Graph differences

The Toon Lighting V2 Shader Graph subgraph has similar lighting controls, but Shader Graph does not expose every HLSL inspector capability. Surface options are normally configured in the graph's master node; alpha-to-coverage, Color Mask, Shadow Offset, and the custom outline pass are unavailable. For outlines, use either a second outline material (and `DisallowGPUDrivenRendering` when needed) or a Render Objects renderer feature with an outline material override.
