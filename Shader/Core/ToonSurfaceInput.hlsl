#ifndef TOON_SURFACE_INPUT_INCLUDED
#define TOON_SURFACE_INPUT_INCLUDED

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

half4 SampleSpecular(float2 uv, half3 specularColor, half smoothness, TEXTURE2D_PARAM(specularMap, sampler_specularMap))
{
    half4 spec;
#ifdef _SPECULARMAP
    spec = SAMPLE_TEXTURE2D(specularMap, sampler_specularMap, uv);
    spec.a *= smoothness;
#else 
    spec.rgb = specularColor;
    spec.a = smoothness;
#endif
    return spec;
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

half3 SampleEmission(float2 uv, half3 emissionColor, TEXTURE2D_PARAM(emissionMap, sampler_emissionMap))
{
#ifndef _EMISSION
    return 0;
#else
#ifdef _EMISSIONMAP
    return SAMPLE_TEXTURE2D(emissionMap, sampler_emissionMap, uv).rgb * emissionColor;
#else
    return emissionColor;
#endif
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

half SampleOcclusion(float2 uv, half occlusionStrength, TEXTURE2D_PARAM(occlusionMap, sampler_occlusionMap))
{
#ifdef _OCCLUSIONMAP
    half occ = SAMPLE_TEXTURE2D(occlusionMap, sampler_occlusionMap, uv).g;
    return LerpWhiteTo(occ, occlusionStrength);
#else
    return 1.0h;
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
