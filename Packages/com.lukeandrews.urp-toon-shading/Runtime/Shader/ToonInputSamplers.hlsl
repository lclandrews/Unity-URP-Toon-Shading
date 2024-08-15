#ifndef TOON_INPUT_SAMPLERS_INCLUDED
#define TOON_INPUT_SAMPLERS_INCLUDED

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"

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

half3 SampleSpecular(half specularMap, half3 specularColor)
{
    half3 spec = specularMap * specularColor;
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
