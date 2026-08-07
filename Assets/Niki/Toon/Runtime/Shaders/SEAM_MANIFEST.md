# TAM Toon integration seam manifest

## Compatibility gate

| Dependency | Reviewed version |
| --- | --- |
| Unity | 6000.0 or newer (tested with 6000.3.16f1) |
| Universal Render Pipeline | 17.0.0 or newer (tested with 17.3.0) |
| CupOHappiness Toon | Current package |

The extension is self-contained with respect to custom shader code: CupOHappiness owns its shader, inspector, renderer feature, and outline scheduling. It still requires Universal Render Pipeline 17.0.0 or newer. A `.unitypackage` does not install Package Manager dependencies, so install a compatible URP package before importing this shader package.

## Baseline contract

`CupOHappiness/Toon & Outline` is an extension-owned copy of CupOHappiness's **Toon & Outline HLSL** shader. Its TAM controls default off, so a material with default settings takes the unchanged CupOHappiness lighting path. When enabled, only ForwardLit samples the independently assigned Shadow/Form, Light, and Highlight Punch-out light-to-dark TAM arrays in transform-scale-compensated object-space triplanar coordinates; each array's slice count is queried from its assigned asset. The checked-in baseline material is `Assets/Runtime/Materials/mat_Kaiju_Boss_Toon.mat`.

The copied shader retains `ForwardLit`, `CupOHappinessOutline`, `ShadowCaster`, `GBuffer`, `DepthOnly`, `DepthNormals`, `Meta`, `MotionVectors`, and `XRMotionVectors`. `CupOHappinessOutline` is scheduled by `CupOHappiness.Toon.CupOHappinessToonOutlineRendererFeature`.

## Copied CupOHappiness sources

All hashes are SHA-256 hashes of the reviewed CupOHappiness 2.07 source. Recalculate them before updating this extension for a CupOHappiness upgrade.

| Extension path | CupOHappiness source path | SHA-256 | Local modification / reason |
| --- | --- | --- | --- |
| `Toon/Toon & Outline.shader` | `Assets/CupOHappiness URP Essentials/Core/Shaders/Toon/Toon & Outline.shader` | `07552cd342f3748a4c53c83c3d14d88aa119c1ecc152f984611d186f220d84a4` | Shader name identifies the extension. ForwardLit alone raises its target to 4.5 and adds the fragment-local `_TAM_ENABLED` variant; all other pass state and keywords are preserved. |
| `Includes/Toon Lighting.hlsl` | `Assets/CupOHappiness URP Essentials/Core/Shaders/Includes/Toon Lighting.hlsl` | `c44ab81e0fd18f3ea2a610e7c73bd6d1ca953f3e959fcece697da2da3bf61221` | Include guard is extension-owned. The forward-only seam exports post-step/post-ramp direct diffuse and specular tone, plus CupOHappiness-derived shadow/surface/light/specular tints. It never derives density from final RGB or includes emission/rim. |
| `Includes/TAM Triplanar.hlsl` | N/A | N/A | Extension-owned ForwardLit-only nearest-slice Shadow/Form, Light, and Highlight Punch-out Texture2DArray sampling; each uses transform-scale-compensated object-space triplanar projection and a zero range/opacity sampling guard. Light coverage excludes the exported unified highlight domain. |
| `Toon/Includes/Toon Inputs.hlsl` | `Assets/CupOHappiness URP Essentials/Core/Shaders/Toon/Includes/Toon Inputs.hlsl` | `d703c222445d3dfc853ac85bda58ca744dc7adedfdc122553d5ca2feccb60e99` | Include guard and relative lighting-include path target the extension copy; extends `UnityPerMaterial` with Shadow/Form, Light, and Highlight Punch-out TAM controls. |
| `Toon/Includes/Toon ForwardLit Pass.hlsl` | `Assets/CupOHappiness URP Essentials/Core/Shaders/Toon/Includes/Toon ForwardLit Pass.hlsl` | `9b023750478527f464be75e6df7e4e6540b514cb052eb64419cf3202cf8743b2` | Carries explicit object-space position/normal varyings, resolves exported direct tone with independent indirect/AO opt-ins, and composes Shadow/Form, then diffuse-only Light, then Highlight Punch-out after CupOHappiness lighting and before fog; no non-colour pass samples TAM. |
| `Toon/Includes/Toon SurfaceData.hlsl` | `Assets/CupOHappiness URP Essentials/Core/Shaders/Toon/Includes/Toon SurfaceData.hlsl` | `9fcb2df58102be097f2ab5943aaefc054da335748ed860a8e33a17b89173ca74` | None. |
| `Toon/Includes/Toon SurfaceInputs.hlsl` | `Assets/CupOHappiness URP Essentials/Core/Shaders/Toon/Includes/Toon SurfaceInputs.hlsl` | `aeecff1b7481214e16e9ad56990cec790db2e6028b2518984eee5e342a2b0e83` | Include guard is extension-owned to avoid collisions. |
| `Toon/Includes/Toon Outline Pass.hlsl` | `Assets/CupOHappiness URP Essentials/Core/Shaders/Toon/Includes/Toon Outline Pass.hlsl` | `a9abacdc122b713dbe2d06166665498c1ad82139bcba6f096f1447b961adcc4e` | None; TAM never samples in the outline pass. |
| `Toon/Includes/Toon Outline MV.hlsl` | `Assets/CupOHappiness URP Essentials/Core/Shaders/Toon/Includes/Toon Outline MV.hlsl` | `667a9d80f6bbc61f649a1a9a857b2e006d9be12c437243605d447b821bcab8b0` | Include guard is extension-owned to avoid collisions. |
| `Toon/Includes/Toon ShadowCaster Pass.hlsl` | `Assets/CupOHappiness URP Essentials/Core/Shaders/Toon/Includes/Toon ShadowCaster Pass.hlsl` | `dc7f4051fdb4f862e0b7273ebb52d14e9e156d7a854f044105bbc328e6c82d90` | None; TAM does not affect shadow casting. |
| `Toon/Includes/Toon DepthOnly Pass.hlsl` | `Assets/CupOHappiness URP Essentials/Core/Shaders/Toon/Includes/Toon DepthOnly Pass.hlsl` | `17396f4c2ddfcb133c297b941620d56d1110069210a619556f37e62703d1eb5c` | None. |
| `Toon/Includes/Toon DepthNormal Pass.hlsl` | `Assets/CupOHappiness URP Essentials/Core/Shaders/Toon/Includes/Toon DepthNormal Pass.hlsl` | `aa022fa5701a490af84cf1fd4bb17dd062994ee81af6d837f0b3965c8442f10e` | None. |
| `Toon/Includes/Toon Meta Pass.hlsl` | `Assets/CupOHappiness URP Essentials/Core/Shaders/Toon/Includes/Toon Meta Pass.hlsl` | `3fd1d731b47df7288947cc4c328d6a4786066bd3294c6ef5c7f4b327c78d91ea` | None. |

## Upgrade procedure

1. Confirm the receiving Unity 6 editor has URP 17.0.0 or newer installed.
2. Diff every source above against its CupOHappiness replacement and update the recorded hash.
3. Preserve all non-colour passes and their keyword declarations unchanged.
4. Re-run the baseline contract tests and rendered CupOHappiness-equivalence checks before adding TAM behavior.
