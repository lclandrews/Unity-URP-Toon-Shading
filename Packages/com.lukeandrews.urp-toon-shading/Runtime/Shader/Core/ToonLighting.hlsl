#ifndef TOON_LIGHTING_INCLUDED
#define TOON_LIGHTING_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

// -------------------------------------
// Lighting Helpers
// -------------------------------------

half GetSmoothStepShading(half NdotL, half attenuation, half shadowEdge, half lightEdge, half highlight, half midtone, half shadow, half edgeSoftness)
{
    half a = min(Remap(NdotL, half2(-0.5, 1.0), half2(0.0, 1.0)), attenuation);
    half high = smoothstep(lightEdge, lightEdge + edgeSoftness, a);
    half mid = smoothstep(shadowEdge, shadowEdge + edgeSoftness, a);
    return shadow + (mid * (midtone - shadow)) + (high * (highlight - midtone));
}

half3 ToonDirectSpecular(half3 lightColor, half3 lightDir, half3 normal, half3 viewDir, half3 specular, half smoothnessExp)
{
    //smoothness = exp2(10 * smoothness + 1);
    float3 halfVec = SafeNormalize(float3(lightDir)+float3(viewDir));
    half NdotH = saturate(dot(normal, halfVec));
    half modifier = pow(NdotH, smoothnessExp);
    half3 specularReflection = specular * modifier;
    return lightColor * specularReflection;
}

half ToonDirectSpecularValue(half3 lightDir, half3 normal, half3 viewDir, half smoothnessExp)
{
    //smoothness = exp2(10 * smoothness + 1);
    float3 halfVec = SafeNormalize(float3(lightDir)+float3(viewDir));
    half NdotH = saturate(dot(normal, halfVec));
    return pow(NdotH, smoothnessExp);
}

half3 ToonHardEdgeShine(half3 lightColor, half3 lightDir, half3 normal, half3 viewDir)
{
    half vl = (dot(viewDir, lightDir) * -1.0) + -0.5;
    half nl = (dot(normal, lightDir) + -0.3) * 4;
    float edge = clamp((vl + nl) * -1.0, 0.2, 1.1);
    return step(edge, FresnelEffect(normal, viewDir, 8)) *lightColor;
}

half3 ToonHardEdgeShine(half3 lightColor, half VdotL, half NdotL, half sVdotN)
{
    half vl = (VdotL * -1.0) + -0.5;
    half nl = (NdotL + -0.3) * 4;
    float edge = clamp((vl + nl) * -1.0, 0.2, 1.1);
    return step(edge, FresnelEffect(sVdotN, 8)) * lightColor;
}

half3 ToonSoftEdgeShine(half3 lightColor, half3 lightDir, half3 normal, half3 viewDir)
{
    half vl = (dot(viewDir, lightDir) * -1.0F) + -0.5;
    half nl = (dot(normal, lightDir) + -0.3) * 4;
    float edge = clamp((vl + nl) * -1.0, 0.2, 1.1);
    return (1.0 - edge) * FresnelEffect(normal, viewDir, 8) * lightColor;
}

half3 ToonSoftEdgeShine(half3 lightColor, half VdotL, half NdotL, half sVdotN)
{
    half vl = (VdotL * -1.0F) + -0.5;
    half nl = (NdotL + -0.3) * 4;
    float edge = clamp((vl + nl) * -1.0, 0.2, 1.1);
    return (1.0 - edge) * FresnelEffect(sVdotN, 8) * lightColor;
}

half ToonHardBacklight(half3 cameraDir, half3 lightDir, half strength, half3 normal, half3 viewDir)
{
    float lc = saturate(dot(lightDir, cameraDir) + -0.6);
    float edge = (1.0 - (lc * strength)) + 0.01;
    return step(edge, FresnelEffect(normal, viewDir, 3.0));
}

half ToonHardBacklight(half3 cameraDir, half3 lightDir, half strength, half sVdotN)
{
    float lc = saturate(dot(lightDir, cameraDir) + -0.6);
    float edge = (1.0 - (lc * strength)) + 0.01;
    return step(edge, FresnelEffect(sVdotN, 3.0));
}

half ToonSoftBacklight(half3 cameraDir, half3 lightDir, half strength, half3 normal, half3 viewDir)
{
    float lc = saturate(dot(lightDir, cameraDir) + -0.6);
    float edge = (1.0 - (lc * strength)) + 0.01;
    return (1.0 - edge) * FresnelEffect(normal, viewDir, 3.0);
}

half ToonSoftBacklight(half3 cameraDir, half3 lightDir, half strength, half sVdotN)
{
    float lc = saturate(dot(lightDir, cameraDir) + -0.6);
    float edge = (1.0 - (lc * strength)) + 0.01;
    return (1.0 - edge) * FresnelEffect(sVdotN, 3.0);
}

half3 ToonLightSpecular(half3 lightDir, half3 attenuatedLightColor, half3 normalWS, half3 viewDir, half3 specular, half specularValue, half specularTexture, half smoothnessExp)
{
    half sV = RGBValue(attenuatedLightColor) * specularValue * ToonDirectSpecularValue(lightDir, normalWS, viewDir, smoothnessExp);
    sV *= specularTexture;
    return step(0.01, sV) * specular * attenuatedLightColor;
}

half3 ToonVertexLighting(float3 positionWS, half3 normalWS, half shadowLimit, half highlightLimit, half edgeSoftness)
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
        half rampedLight = GetSmoothStepShading(NdotL, light.distanceAttenuation, shadowLimit, highlightLimit, _HighlightValue, _MidtoneValue, _ShadowValue, edgeSoftness);
        vertexLightColor += rampedLight * light.color;
    }

    LIGHT_LOOP_END
#endif
    return vertexLightColor;
}

half3 CalculateToonLighting(Light light, ToonInputData inputData, ToonSurfaceData surfaceData, half saturatedViewDotNormal, half smoothness, half specular,
    half shadowLimit, half highlightLimit, half edgeSoftness)
{
    half NdotL = dot(inputData.normalWS, light.direction);
    half rampedLight = GetSmoothStepShading(NdotL, light.distanceAttenuation * light.shadowAttenuation, shadowLimit, highlightLimit, _HighlightValue, _MidtoneValue, _ShadowValue, edgeSoftness);    
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