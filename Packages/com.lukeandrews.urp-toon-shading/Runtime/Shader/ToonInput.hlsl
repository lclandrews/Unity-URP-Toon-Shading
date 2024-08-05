#ifndef TOON_INPUT_INCLUDED
#define TOON_INPUT_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"

half _MainShadowLimit;
half _MainHighlightLimit;
half _MainEdgeSoftness;

half _AdditionalShadowLimit;
half _AdditionalHighlightLimit;
half _AdditionalEdgeSoftness;

half _HighlightValue;
half _MidtoneValue;
half _ShadowValue;

CBUFFER_START(UnityPerMaterial)
float4 _BaseMap_ST;
half4 _BaseColor;
half3 _ShineColor;
half3 _SpecColor;
half3 _EmissionColor;
half _Cutoff;
half _BacklightStrength;
half _Smoothness;
half _SpecTexTile;
half _SpecTexRot;
half _BumpScale;
half _OcclusionStrength;

// OUTLINE 
half _OutlineWidth;
half _OutlineStartFadeDistance;
half _OutlineEndFadeDistance;
float4 _OutlineMap_ST;
half4 _OutlineColor;
half _OutlineOffsetZ;
CBUFFER_END

TEXTURE2D(_BaseMap);            SAMPLER(sampler_BaseMap);

TEXTURE2D(_ToonMask);           SAMPLER(sampler_ToonMask);
TEXTURE2D(_SpecTexMap);         SAMPLER(sampler_SpecTexMap);
TEXTURE2D(_BumpMap);            SAMPLER(sampler_BumpMap);

TEXTURE2D(_OutlineMap);            SAMPLER(sampler_OutlineMap);

half Alpha(half albedoAlpha, half4 color, half cutoff)
{
    half alpha = albedoAlpha * color.a;

#if defined(_ALPHATEST_ON)
    clip(alpha - cutoff);
#endif

    return alpha;
}

half4 SampleAlbedoAlpha(float2 uv, TEXTURE2D_PARAM(albedoAlphaMap, sampler_albedoAlphaMap))
{
    return SAMPLE_TEXTURE2D(albedoAlphaMap, sampler_albedoAlphaMap, uv);
}

half3 SampleSpecular(half specularMap, half3 specularColor, half3 albedo)
{
    half3 spec = specularMap * albedo * specularColor;
    return spec;
}

half SampleSpecularTexture(float2 uv, TEXTURE2D_PARAM(specularTexMap, sampler_specularTexMap))
{
#ifdef _SPECTEXMAP
    return SAMPLE_TEXTURE2D(specularTexMap, sampler_specularTexMap, uv).r;
#else 
    return 1.0h;
#endif
}

half3 SampleNormal(float2 uv, TEXTURE2D_PARAM(normalMap, sampler_normalMap), half scale = 1.0h)
{
#ifdef _NORMALMAP
    half4 n = SAMPLE_TEXTURE2D(normalMap, sampler_normalMap, uv);
#if BUMP_SCALE_NOT_SUPPORTED
    return UnpackNormal(n);
#else
    return UnpackNormalScale(n, scale);
#endif
#else
    return half3(0.0h, 0.0h, 1.0h);
#endif
}

half3 SampleEmission(half3 albedo, half3 emissionColor, half emission)
{
#ifndef _EMISSION
    return 0;
#else
    return emission * (emissionColor * albedo);
#endif
}

half SampleOcclusion(half occlusion, half occlusionStrength)
{
    return LerpWhiteTo(occlusion, occlusionStrength);
}

half4 SampleToonMask(float2 uv, TEXTURE2D_PARAM(toonMaskMap, sampler_toonMaskMap))
{
    half4 value = half4(1.0, 1.0, 1.0, 1.0);
#ifdef _MASKMAP
    value = SAMPLE_TEXTURE2D(toonMaskMap, sampler_toonMaskMap, uv);

#ifdef _FILLMASK_SPECULAR
    value.r = 1.0;
#endif
#ifdef _FILLMASK_OCCLUSION
    value.g = 1.0;
#endif
#ifdef _FILLMASK_EMISSION
    value.b = 1.0;
#endif
#ifdef _FILLMASK_SMOOTHNESS
    value.a = 1.0;
#endif

#endif
    return value;
}

#endif
