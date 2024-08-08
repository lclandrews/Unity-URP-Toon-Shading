#ifndef TOON_THREE_TONE_LIGHTING_INCLUDED
#define TOON_THREE_TONE_LIGHTING_INCLUDED

#include "../Core/ToonLighting.hlsl"

half3 ToonThreeToneVertexLighting(float3 positionWS, half3 normalWS, half shadowLimit, half highlightLimit, half edgeSoftness)
{
    half3 vertexLightColor = half3(0.0, 0.0, 0.0);
#ifdef _ADDITIONAL_LIGHTS_VERTEX
    uint lightsCount = GetAdditionalLightsCount();
    uint meshRenderingLayers = GetMeshRenderingLayer();

    LIGHT_LOOP_BEGIN(lightsCount)
    Light light = GetAdditionalLight(lightIndex, positionWS);

#ifdef _LIGHT_LAYERS
    if (IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
#endif
    {
        half NdotL = dot(normalWS, light.direction);
        half rampedLight = GetThreeToneSmoothStepShading(NdotL, light.distanceAttenuation, shadowLimit, highlightLimit, _HighlightValue, _MidtoneValue, edgeSoftness);
        vertexLightColor += rampedLight * light.color;
    }

    LIGHT_LOOP_END
#endif
    return vertexLightColor;
}

half3 CalculateToonThreeToneLighting(Light light, ToonInputData inputData, ToonSurfaceData surfaceData, half saturatedViewDotNormal, half smoothness, half specular,
    half shadowLimit, half highlightLimit, half edgeSoftness)
{
    half NdotL = dot(inputData.normalWS, light.direction);
    half rampedLight = GetThreeToneSmoothStepShading(NdotL, light.distanceAttenuation * light.shadowAttenuation, shadowLimit, highlightLimit, _HighlightValue, _MidtoneValue, _ShadowValue, edgeSoftness);    
    //rampedLight *= shadowAttenuation;

#ifndef _BACKLIGHT_OFF
    half3 lightDiffuseColor = max(rampedLight, ToonHardBacklight(inputData.cameraDirWS, light.direction, _BacklightStrength, saturatedViewDotNormal)) * light.color;
#else
    half3 lightDiffuseColor = rampedLight * light.color;
#endif    
    lightDiffuseColor *= surfaceData.albedo;
#ifndef _SPECULAR_OFF
    lightDiffuseColor += ToonLightSpecular(light.direction, light.color, inputData.normalWS, inputData.viewDirectionWS,
        surfaceData.specular, specular, surfaceData.specularTexture, smoothness);
#endif
    return lightDiffuseColor;
}

half3 CalculateToonThreeToneAddLighting(Light light, ToonInputData inputData, ToonSurfaceData surfaceData, half saturatedViewDotNormal, half smoothness, half specular,
    half shadowLimit, half highlightLimit, half edgeSoftness)
{
    half NdotL = dot(inputData.normalWS, light.direction);
    half rampedLight = GetThreeToneSmoothStepShading(NdotL, light.distanceAttenuation * light.shadowAttenuation, shadowLimit, highlightLimit, _HighlightValue, _MidtoneValue, edgeSoftness);
    //rampedLight *= shadowAttenuation;

#ifndef _BACKLIGHT_OFF
    half3 lightDiffuseColor = max(rampedLight, ToonHardBacklight(inputData.cameraDirWS, light.direction, _BacklightStrength, saturatedViewDotNormal)) * light.color;
#else
    half3 lightDiffuseColor = rampedLight * light.color;
#endif    
    lightDiffuseColor *= surfaceData.albedo;
#ifndef _SPECULAR_OFF
    lightDiffuseColor += ToonLightSpecular(light.direction, light.color, inputData.normalWS, inputData.viewDirectionWS,
        surfaceData.specular, specular, surfaceData.specularTexture, smoothness);
#endif
    return lightDiffuseColor;
}

#endif