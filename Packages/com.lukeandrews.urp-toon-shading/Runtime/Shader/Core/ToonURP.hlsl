#ifndef TOON_URP_INCLUDED
#define TOON_URP_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

// -------------------------------------
// URP Replacements
// -------------------------------------

#if defined(LIGHTMAP_ON)
    #define TOON_DECLARE_LIGHTMAP_OR_SH(lmName, shName, index) float3 lmName : TEXCOORD##index
    #define TOON_OUTPUT_LIGHTMAP_UV(lightmapUV, lightmapScaleOffset, OUT) OUT.xy = lightmapUV.xy * lightmapScaleOffset.xy + lightmapScaleOffset.zw
    #define TOON_OUTPUT_SH(normalWS, OUT)
    #define TOON_OUTPUT_FOG(lm, sh, fog) lm.z = fog
#else
    #define TOON_DECLARE_LIGHTMAP_OR_SH(lmName, shName, index) half4 shName : TEXCOORD##index
    #define TOON_OUTPUT_LIGHTMAP_UV(lightmapUV, lightmapScaleOffset, OUT)
    #define TOON_OUTPUT_SH(normalWS, OUT) OUT.xyz = SampleSHVertex(normalWS)
    #define TOON_OUTPUT_FOG(lm, sh, fog) sh.w = fog
#endif

half4 ToonCalculateShadowMask(half4 inShadowMask)
{
    // To ensure backward compatibility we have to avoid using shadowMask input, as it is not present in older shaders
#if defined(SHADOWS_SHADOWMASK) && defined(LIGHTMAP_ON)
    half4 shadowMask = inShadowMask;
#elif !defined (LIGHTMAP_ON)
    half4 shadowMask = unity_ProbesOcclusion;
#else
    half4 shadowMask = half4(1, 1, 1, 1);
#endif
    return shadowMask;
}

Light ToonGetMainLight(float4 shadowCoord, float3 positionWS, half4 shadowMask, AmbientOcclusionFactor aoFactor)
{
    Light light = GetMainLight(shadowCoord, positionWS, shadowMask);
#if defined(_SCREEN_SPACE_OCCLUSION) && !defined(_SURFACE_TYPE_TRANSPARENT)
    if (IsLightingFeatureEnabled(DEBUGLIGHTINGFEATUREFLAGS_AMBIENT_OCCLUSION))
    {
        light.color *= aoFactor.directAmbientOcclusion;
    }
#endif
    return light;
}

Light ToonGetAdditionalLight(uint i, float3 positionWS, half4 shadowMask, AmbientOcclusionFactor aoFactor)
{
    Light light = GetAdditionalLight(i, positionWS, shadowMask);
#if defined(_SCREEN_SPACE_OCCLUSION) && !defined(_SURFACE_TYPE_TRANSPARENT)
    if (IsLightingFeatureEnabled(DEBUGLIGHTINGFEATUREFLAGS_AMBIENT_OCCLUSION))
    {
        light.color *= aoFactor.directAmbientOcclusion;
    }
#endif
    return light;
}

LightingData ToonCreateLightingData(half3 bakedGI, half3 emission)
{
    LightingData lightingData;

    lightingData.giColor = bakedGI;
    lightingData.emissionColor = emission;
    lightingData.vertexLightingColor = 0;
    lightingData.mainLightColor = 0;
    lightingData.additionalLightsColor = 0;
    return lightingData;
}

#endif