#ifndef CUPOFHAPPINESS_TOON_TRIPLANAR_INCLUDED
#define CUPOFHAPPINESS_TOON_TRIPLANAR_INCLUDED

// TAM is applied after Toon lighting has established its light, shadow, and
// highlight domains. Keeping this file ForwardLit-only prevents hatching from
// changing depth, shadows, motion vectors, or baked-material output.
//
// Each layer owns a different visual job: Shadow/Form adds dark-side structure,
// Light adds bright-side strokes, and Highlight Punch-out removes highlights
// through a hatch pattern. Keep the layers independent so an artist can tune
// one without surprising changes in the others.
//
// This ForwardLit-only declaration keeps all TAM assets out of CupOHappiness's
// non-colour passes.
TEXTURE2D_ARRAY(_TAMShadowFormArray); SAMPLER(sampler_TAMShadowFormArray);
TEXTURE2D_ARRAY(_TAMLightArray); SAMPLER(sampler_TAMLightArray);
TEXTURE2D_ARRAY(_TAMHighlightPunchOutArray); SAMPLER(sampler_TAMHighlightPunchOutArray);

// Authored slices run light to dark. We intentionally choose one nearest slice
// instead of blending slices: it preserves the designed stroke pattern and
// bounds each layer to three texture samples (one per projection).
float CupOHappinessTAMSampleShadowFormTone(float2 uv, float tone)
{
    uint width;
    uint height;
    uint sliceCount;
    _TAMShadowFormArray.GetDimensions(width, height, sliceCount);

    float slice = saturate(tone) * max(0.0, (float)sliceCount - 1.0);
    return saturate(SAMPLE_TEXTURE2D_ARRAY(
        _TAMShadowFormArray, sampler_TAMShadowFormArray, uv, round(slice)).r);
}

float CupOHappinessTAMSampleShadowFormTriplanar(float3 positionOS, float3 normalOS, float tone)
{
    // Compensate object-space coordinates for scale so a hatch keeps its visual
    // size when an instance is stretched. This also avoids camera-dependent
    // swimming and makes TAM independent of the material's base-map UVs.
    float3 objectScale = max(float3(
        length(unity_ObjectToWorld._m00_m10_m20),
        length(unity_ObjectToWorld._m01_m11_m21),
        length(unity_ObjectToWorld._m02_m12_m22)),
        1e-5);
    float3 position = positionOS * objectScale * max(_TAMShadowFormProjectionScale, 1e-5);
    float3 normal = normalize(normalOS / objectScale);

    // A sharper blend reduces visible mixing of unrelated strokes at edges
    // between projections, while normalization prevents brightness changes.
    float3 weights = pow(abs(normal), 4.0);
    weights /= max(weights.x + weights.y + weights.z, 1e-5);

    return CupOHappinessTAMSampleShadowFormTone(position.zy, tone) * weights.x
        + CupOHappinessTAMSampleShadowFormTone(position.xz, tone) * weights.y
        + CupOHappinessTAMSampleShadowFormTone(position.xy, tone) * weights.z;
}

half CupOHappinessTAMShadowFormCoverage(half directToonTone)
{
    // A positive range remaps the selected shadow domain to 0..1. Exact zero
    // is handled by the caller before any texture lookup.
    return saturate(1.0h - saturate(directToonTone) / _TAMShadowFormRange);
}

half3 CupOHappinessTAMComposeShadowFormInk(
    half3 cupColor,
    float3 positionOS,
    float3 normalOS,
    half directToonTone,
    half inkAlpha)
{
    // Range doubles as the enable control. Branch before sampling so a disabled
    // layer costs no texture reads, including its persistent form hatch.
    if (_TAMShadowFormRange <= 0.0h)
    {
        return cupColor;
    }

    half shadowCoverage = CupOHappinessTAMShadowFormCoverage(directToonTone);
    half shadowInk = CupOHappinessTAMSampleShadowFormTriplanar(
        positionOS, normalOS, saturate(shadowCoverage + _TAMShadowFormToneBias))
        * shadowCoverage * _TAMShadowFormInkOpacity;
    half formInk = CupOHappinessTAMSampleShadowFormTriplanar(
        positionOS, normalOS, saturate(_TAMShadowFormToneBias))
        * _TAMShadowFormOpacity;
    return lerp(cupColor, _TAMShadowFormInkColor.rgb * inkAlpha, saturate(shadowInk + formInk));
}

// The Light layer uses only the upper direct-diffuse range. Highlights are kept
// separate because their shape belongs to specular and rim lighting, not to
// bright diffuse light.
float CupOHappinessTAMSampleLightTone(float2 uv, float tone)
{
    uint width;
    uint height;
    uint sliceCount;
    _TAMLightArray.GetDimensions(width, height, sliceCount);

    float slice = saturate(tone) * max(0.0, (float)sliceCount - 1.0);
    return saturate(SAMPLE_TEXTURE2D_ARRAY(
        _TAMLightArray, sampler_TAMLightArray, uv, round(slice)).r);
}

float CupOHappinessTAMSampleLightTriplanar(float3 positionOS, float3 normalOS, float tone)
{
    float3 objectScale = max(float3(
        length(unity_ObjectToWorld._m00_m10_m20),
        length(unity_ObjectToWorld._m01_m11_m21),
        length(unity_ObjectToWorld._m02_m12_m22)),
        1e-5);
    float3 position = positionOS * objectScale * max(_TAMLightProjectionScale, 1e-5);
    float3 normal = normalize(normalOS / objectScale);

    // A sharper blend reduces visible mixing of unrelated strokes at edges
    // between projections, while normalization prevents brightness changes.
    float3 weights = pow(abs(normal), 4.0);
    weights /= max(weights.x + weights.y + weights.z, 1e-5);

    return CupOHappinessTAMSampleLightTone(position.zy, tone) * weights.x
        + CupOHappinessTAMSampleLightTone(position.xz, tone) * weights.y
        + CupOHappinessTAMSampleLightTone(position.xy, tone) * weights.z;
}

half CupOHappinessTAMLightCoverage(half directToonTone)
{
    // A range of one maps the complete direct-diffuse domain. Smaller positive
    // ranges select and remap only its brightest portion; zero is caller-guarded.
    return saturate((saturate(directToonTone) - (1.0h - _TAMLightRange)) / _TAMLightRange);
}

half3 CupOHappinessTAMComposeLightInk(
    half3 cupColor,
    float3 positionOS,
    float3 normalOS,
    half directToonTone,
    half highlightDomain,
    half3 directLightTint,
    half inkAlpha)
{
    // Branch before sampling so a disabled Light layer has no texture cost.
    if (_TAMLightRange <= 0.0h)
    {
        return cupColor;
    }

    // Highlights can mask Light TAM completely (the legacy behaviour) or let
    // it remain visible. The material author controls this independently of
    // the Highlight Punch-out layer.
    half highlightMask = lerp(1.0h, _TAMLightHighlightTransparency, step(1e-4h, highlightDomain));
    half lightCoverage = CupOHappinessTAMLightCoverage(directToonTone) * highlightMask;
    half inkCoverage = CupOHappinessTAMSampleLightTriplanar(
        positionOS, normalOS, saturate(lightCoverage + _TAMLightToneBias))
        * lightCoverage * _TAMLightInkOpacity;
    // Borrow only the light hue. Dividing by luminance prevents brighter lights
    // from making strokes denser; density remains an authored TAM decision.
    half3 lightingTint = directLightTint / max(Luminance(directLightTint), 1e-4h);
    return lerp(cupColor, _TAMLightInkColor.rgb * lightingTint * inkAlpha, saturate(inkCoverage));
}

// Highlight punch-out has no tint because it is a mask, not an ink layer. It
// removes only the combined specular/rim contribution so base lighting remains
// stable while highlights acquire the hatch shape.
float CupOHappinessTAMSampleHighlightPunchOutTone(float2 uv, float tone)
{
    uint width;
    uint height;
    uint sliceCount;
    _TAMHighlightPunchOutArray.GetDimensions(width, height, sliceCount);

    float slice = saturate(tone) * max(0.0, (float)sliceCount - 1.0);
    return saturate(SAMPLE_TEXTURE2D_ARRAY(
        _TAMHighlightPunchOutArray, sampler_TAMHighlightPunchOutArray, uv, round(slice)).r);
}

float CupOHappinessTAMSampleHighlightPunchOutTriplanar(float3 positionOS, float3 normalOS, float tone)
{
    float3 objectScale = max(float3(
        length(unity_ObjectToWorld._m00_m10_m20),
        length(unity_ObjectToWorld._m01_m11_m21),
        length(unity_ObjectToWorld._m02_m12_m22)),
        1e-5);
    float3 position = positionOS * objectScale * max(_TAMHighlightPunchOutProjectionScale, 1e-5);
    float3 normal = normalize(normalOS / objectScale);

    // A sharper blend reduces visible mixing of unrelated strokes at edges
    // between projections, while normalization prevents brightness changes.
    float3 weights = pow(abs(normal), 4.0);
    weights /= max(weights.x + weights.y + weights.z, 1e-5);

    return CupOHappinessTAMSampleHighlightPunchOutTone(position.zy, tone) * weights.x
        + CupOHappinessTAMSampleHighlightPunchOutTone(position.xz, tone) * weights.y
        + CupOHappinessTAMSampleHighlightPunchOutTone(position.xy, tone) * weights.z;
}

half3 CupOHappinessTAMComposeHighlightPunchOut(
    half3 composedColor,
    float3 positionOS,
    float3 normalOS,
    half highlightDomain,
    half3 highlightTint)
{
    // Opacity is the enable control. Branch before sampling so disabled
    // punch-out has no texture cost.
    if (_TAMHighlightPunchOutOpacity <= 0.0h)
    {
        return composedColor;
    }

    half hatchCoverage = CupOHappinessTAMSampleHighlightPunchOutTriplanar(
        positionOS, normalOS, saturate(highlightDomain + _TAMHighlightPunchOutToneBias));
    // Invert coverage but retain fractional values so highlights fade through
    // strokes instead of popping between fully visible and fully removed.
    half highlightMask = lerp(1.0h, 1.0h - saturate(hatchCoverage), _TAMHighlightPunchOutOpacity);
    return composedColor - highlightTint * (1.0h - highlightMask) * step(1e-4h, highlightDomain);
}

half3 CupOHappinessTAMDiagnosticColor(
    half3 composedColor,
    float3 positionOS,
    float3 normalOS,
    half directDiffuseTone,
    half shadowFormTone,
    half highlightDomain)
{
    if (_TAMDebugView == 1.0h) return directDiffuseTone.xxx;
    if (_TAMDebugView == 2.0h) return (1.0h - directDiffuseTone).xxx;
    if (_TAMDebugView == 3.0h) return highlightDomain.xxx;

    if (_TAMDebugView == 4.0h && _TAMShadowFormRange > 0.0h)
    {
        half shadowCoverage = CupOHappinessTAMShadowFormCoverage(shadowFormTone);
        half shadowInk = CupOHappinessTAMSampleShadowFormTriplanar(
            positionOS, normalOS, saturate(shadowCoverage + _TAMShadowFormToneBias))
            * shadowCoverage * _TAMShadowFormInkOpacity;
        half formInk = CupOHappinessTAMSampleShadowFormTriplanar(
            positionOS, normalOS, saturate(_TAMShadowFormToneBias)) * _TAMShadowFormOpacity;
        return saturate(shadowInk + formInk).xxx;
    }

    if (_TAMDebugView == 5.0h && _TAMLightRange > 0.0h)
    {
        half highlightMask = lerp(1.0h, _TAMLightHighlightTransparency, step(1e-4h, highlightDomain));
        half coverage = CupOHappinessTAMLightCoverage(directDiffuseTone) * highlightMask;
        half ink = CupOHappinessTAMSampleLightTriplanar(
            positionOS, normalOS, saturate(coverage + _TAMLightToneBias))
            * coverage * _TAMLightInkOpacity;
        return ink.xxx;
    }

    if (_TAMDebugView == 6.0h && _TAMHighlightPunchOutOpacity > 0.0h)
    {
        half coverage = CupOHappinessTAMSampleHighlightPunchOutTriplanar(
            positionOS, normalOS, saturate(highlightDomain + _TAMHighlightPunchOutToneBias));
        return (coverage * highlightDomain * _TAMHighlightPunchOutOpacity).xxx;
    }

    return composedColor;
}

#endif
