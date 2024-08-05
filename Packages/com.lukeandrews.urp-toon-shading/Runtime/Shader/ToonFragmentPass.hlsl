#ifndef TOON_FRAG_PASS_INCLUDED
#define TOON_FRAG_PASS_INCLUDED

#if defined(LOD_FADE_CROSSFADE)
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/LODCrossFade.hlsl"
#endif

#include "ToonSurfaceData.hlsl"

#include "Core/ToonURP.hlsl"
#include "Core/ToonLighting.hlsl"

void ToonPassFragment(Varyings input, out half4 outColor : SV_Target0
#ifdef _WRITE_RENDERING_LAYERS
    , out float4 outRenderingLayers : SV_Target1
#endif
)
{
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

    float2 uv = float2(input.uAndNormalWS.x, input.vAndViewDirWS.x);

    ToonSurfaceData surfaceData;
    InitToonSurfaceData(uv, surfaceData);    

#ifdef LOD_FADE_CROSSFADE
    LODFadeCrossFade(input.positionCS);
#endif
    
    ToonInputData inputData;
    InitToonInputData(input, surfaceData.normalTS, inputData);
    
#ifdef _DBUFFER
    ApplyDecal(positionCS,
        surfaceData.albedo,
        surfaceData.specular,
        inputData.normalWS,
        metallic,
        surfaceData.occlusion,
        surfaceData.smoothness);
#endif
  
// TODO: Finish implementing debug view
//#if defined(DEBUG_DISPLAY)
//    half4 debugColor;
//
//    if (CanDebugOverrideOutputColor(inputData, surfaceData, debugColor))
//    {
//        outColor = debugColor;
//        return;
//    }
//#endif
    
    uint meshRenderingLayers = GetMeshRenderingLayer();
    half4 shadowMask = ToonCalculateShadowMask(inputData.shadowMask);
    AmbientOcclusionFactor aoFactor = CreateAmbientOcclusionFactor(inputData.normalizedScreenSpaceUV, surfaceData.occlusion);
    Light mainLight = ToonGetMainLight(inputData.shadowCoord, inputData.positionWS, shadowMask, aoFactor);

    MixRealtimeAndBakedGI(mainLight, inputData.normalWS, inputData.bakedGI, aoFactor);   
    inputData.bakedGI *= surfaceData.albedo;
    
    half sVdotN = 0;
#if !defined(_BACKLIGHT_OFF) || !defined(_EDGESHINE_OFF)
    sVdotN = saturate(dot(inputData.viewDirectionWS, inputData.normalWS));
#endif
    
    half smoothness = 0;
    half specular = 0;
#ifndef _SPECULAR_OFF
    smoothness = exp2(10 * surfaceData.smoothness + 1);
    specular = RGBValue(surfaceData.specular);
#endif
    
    LightingData lightingData = ToonCreateLightingData(inputData.bakedGI, surfaceData.emission);
#ifdef _LIGHT_LAYERS
    if (IsMatchingLightLayer(mainLight.layerMask, meshRenderingLayers))
#endif
    {
        lightingData.mainLightColor += CalculateToonLighting(mainLight, inputData, surfaceData, sVdotN, smoothness, specular,
            _MainShadowLimit, _MainHighlightLimit, _MainEdgeSoftness);
    }
    
#if defined(_ADDITIONAL_LIGHTS)
    uint pixelLightCount = GetAdditionalLightsCount();

#if USE_FORWARD_PLUS
    for (uint lightIndex = 0; lightIndex < min(URP_FP_DIRECTIONAL_LIGHTS_COUNT, MAX_VISIBLE_LIGHTS); lightIndex++)
    {
        FORWARD_PLUS_SUBTRACTIVE_LIGHT_CHECK

        Light light = ToonGetAdditionalLight(lightIndex, inputData.positionWS, shadowMask, aoFactor);
#ifdef _LIGHT_LAYERS
        if (IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
#endif
        {
            lightingData.additionalLightsColor += CalculateToonLighting(light, inputData, surfaceData, sVdotN, smoothness, specular, 
                    _AdditionalShadowLimit, _AdditionalHighlightLimit, _AdditionalEdgeSoftness);
        }
    }
#endif // End USE_FORWARD_PLUS

    LIGHT_LOOP_BEGIN(pixelLightCount)
    Light light = ToonGetAdditionalLight(lightIndex, inputData.positionWS, shadowMask, aoFactor);
#ifdef _LIGHT_LAYERS
    if (IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
#endif
    {
        lightingData.additionalLightsColor += CalculateToonLighting(light, inputData, surfaceData, sVdotN, smoothness, specular, 
                _AdditionalShadowLimit, _AdditionalHighlightLimit, _AdditionalEdgeSoftness);
    }
    LIGHT_LOOP_END
#endif

#if defined(_ADDITIONAL_LIGHTS_VERTEX)
    lightingData.vertexLightingColor += inputData.vertexLighting * surfaceData.albedo;
    outColor.rgb = lightingData.vertexLightingColor;
    return;
#endif // End defined(_ADDITIONAL_LIGHTS)

    half4 color = CalculateFinalColor(lightingData, surfaceData.alpha);
    
#ifndef _EDGESHINE_OFF
    half VdotL = dot(inputData.viewDirectionWS, mainLight.direction);
    half NdotL = dot(inputData.normalWS, mainLight.direction);
    color.rgb += ToonHardEdgeShine(mainLight.color, VdotL, NdotL, sVdotN) * mainLight.shadowAttenuation * _ShineColor;
#endif

    color.rgb = MixFog(color.rgb, inputData.fogCoord);    
    outColor = color;

#ifdef _WRITE_RENDERING_LAYERS
    uint renderingLayers = GetMeshRenderingLayer();
    outRenderingLayers = float4(EncodeMeshRenderingLayer(renderingLayers), 0, 0, 0);
#endif
}

#endif
