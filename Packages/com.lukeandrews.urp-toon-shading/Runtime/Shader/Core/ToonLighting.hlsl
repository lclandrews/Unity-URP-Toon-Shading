#ifndef TOON_LIGHTING_INCLUDED
#define TOON_LIGHTING_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

// -------------------------------------
// Lighting Helpers
// -------------------------------------

half GetTwoToneSmoothStepShading(half NdotL, half attenuation, half shadowEdge, half highlight, half shadow, half edgeSoftness)
{
    half a = min(Remap(NdotL, half2(-0.5, 1.0), half2(0.0, 1.0)), attenuation);
    half high = smoothstep(shadowEdge, shadowEdge + edgeSoftness, a);
    return shadow + (high * (highlight - shadow));
}

half GetTwoToneSmoothStepShading(half NdotL, half attenuation, half shadowEdge, half highlight, half edgeSoftness)
{
    half a = min(Remap(NdotL, half2(-0.5, 1.0), half2(0.0, 1.0)), attenuation);
    half high = smoothstep(shadowEdge, shadowEdge + edgeSoftness, a);
    return high * highlight;
}

half GetThreeToneSmoothStepShading(half NdotL, half attenuation, half shadowEdge, half lightEdge, half highlight, half midtone, half shadow, half edgeSoftness)
{
    half a = min(Remap(NdotL, half2(-0.5, 1.0), half2(0.0, 1.0)), attenuation);
    half high = smoothstep(lightEdge, lightEdge + edgeSoftness, a);
    half mid = smoothstep(shadowEdge, shadowEdge + edgeSoftness, a);
    return shadow + (mid * (midtone - shadow)) + (high * (highlight - midtone));
}

half GetThreeToneSmoothStepShading(half NdotL, half attenuation, half shadowEdge, half lightEdge, half highlight, half midtone, half edgeSoftness)
{
    half a = min(Remap(NdotL, half2(-0.5, 1.0), half2(0.0, 1.0)), attenuation);
    half high = smoothstep(lightEdge, lightEdge + edgeSoftness, a);
    half mid = smoothstep(shadowEdge, shadowEdge + edgeSoftness, a);
    return (mid * midtone) + (high * (highlight - midtone));
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

#endif