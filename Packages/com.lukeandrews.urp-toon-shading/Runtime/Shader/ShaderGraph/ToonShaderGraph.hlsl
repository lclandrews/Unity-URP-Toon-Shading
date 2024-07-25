#ifndef TOON_SHADER_GRAPH_INCLUDED
#define TOON_SHADER_GRAPH_INCLUDED

#include "ToonCore.hlsl"
#include "Packages/com.unity.shadergraph/ShaderGraphLibrary/Functions.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"

// -------------------------------------
// Helpers
// -------------------------------------

half3 SampleGradientRGB(Gradient Gradient, half Time)
{
    half3 color = Gradient.colors[0].rgb;
    UNITY_UNROLL
    for (int c = 1; c < 8; c++)
    {
        half colorPos = saturate((Time - Gradient.colors[c - 1].w) / (Gradient.colors[c].w - Gradient.colors[c - 1].w)) * step(c, Gradient.colorsLength - 1);
        color = lerp(color, Gradient.colors[c].rgb, lerp(colorPos, step(0.01, colorPos), Gradient.type));
    }
#ifndef UNITY_COLORSPACE_GAMMA
    color = SRGBToLinear(color);
#endif
    return color;
}

// -------------------------------------
// Shader Graph Custom Functions
// -------------------------------------
void MainLight_float(float3 WorldPosition, float3 WorldNormal, float3 ViewDirection, float3 CameraDirection,
    float3 AmbientColor, Texture2D ShadingRamp, SamplerState ShadingSampler, float BacklightStrength, float3 EdgeShineColor,
    float3 DiffuseTexture, float3 DiffuseColor, float3 NormalMap, float NormalMapStrength, float3 SpecularColor,
    float SpecularMap, float Smoothness, float SpecularTexture, float AmbientOcclusionMap, float AmbientOcclusionStrength,
    out float3 Color)
{
    float3 diffuseColor = 0;
    float3 specularColor = 0;

#ifndef SHADERGRAPH_PREVIEW
    float4 shadowCoord = TransformWorldToShadowCoord_Unlit(WorldPosition);
    Light mainLight = GetMainLight_Unlit(shadowCoord);

    float3 surfaceNormal = GetBlendedNormal(float3(0.0, 0.0, 1.0), NormalMap, NormalMapStrength, WorldNormal);
    float attenuation = mainLight.distanceAttenuation * mainLight.shadowAttenuation;
    float shade = dot(surfaceNormal, mainLight.direction);
    float finalLight = clamp(min(Remap(shade, float2(-1.0, 1.0), float2(0.0, 1.0)), attenuation), 0.01, 0.99);
    float occlusion = saturate(AmbientOcclusionMap + (1.0 - AmbientOcclusionStrength));

    float3 rampedFinalLight = SAMPLE_TEXTURE2D(ShadingRamp, ShadingSampler, float2(finalLight * occlusion, 0.0));
    float3 ambient = DiffuseTexture * DiffuseColor * AmbientColor;
    //rampedFinalLight += AmbientColor;
    float3 lightColor = mainLight.color;// +;// AmbientColor;
    diffuseColor += max(rampedFinalLight, HardBacklight(CameraDirection, mainLight.direction, BacklightStrength, WorldNormal, ViewDirection)) * lightColor * DiffuseTexture * DiffuseColor;
    diffuseColor += HardEdgeShine(mainLight.color, mainLight.direction, WorldNormal, ViewDirection) * mainLight.shadowAttenuation * EdgeShineColor;
    //diffuseColor *= mainLight.shadowAttenuation;
    //diffuseColor = max(ambient, diffuseColor);

    ///specularColor = RGBValue(DirectSpecular(mainLight.color * attenuation, mainLight.direction, WorldNormal, ViewDirection, SpecularColor, Smoothness));
    specularColor = RGBValue(mainLight.color * attenuation) * RGBValue(SpecularColor) * DirectSpecularValue(mainLight.direction, WorldNormal, ViewDirection, Smoothness);
    specularColor = SpecularMap * SpecularTexture * specularColor;// DirectSpecularValue(mainLight.direction, WorldNormal, ViewDirection, Smoothness);
    specularColor = step(0.01f, specularColor) * SpecularColor * mainLight.color;
#endif

    Color = diffuseColor + specularColor;
}


void AdditionalLights_float(float3 WorldPosition, float3 WorldNormal, float3 ViewDirection, float3 CameraDirection,
    Texture2D ShadingRamp, SamplerState ShadingSampler, float BacklightStrength, float3 DiffuseTexture, float3 DiffuseColor,
    float3 NormalMap, float NormalMapStrength, float3 SpecularColor, float SpecularMap, float Smoothness,
    float SpecularTexture, float AmbientOcclusionMap, float AmbientOcclusionStrength, out float3 Color)
{
    float3 diffuseColor = 0;
    float3 specularColor = 0;

#ifndef SHADERGRAPH_PREVIEW
    WorldNormal = normalize(WorldNormal);
    int pixelLightCount = GetAdditionalLightsCount();
    for (int i = 0; i < pixelLightCount; ++i)
    {
        Light light = GetAdditionalLight_Unlit(i, WorldPosition);

        float3 surfaceNormal = GetBlendedNormal(float3(0.0, 0.0, 1.0), NormalMap, NormalMapStrength, WorldNormal);
        float occlusion = saturate(AmbientOcclusionMap + (1.0 - AmbientOcclusionStrength));

        float3 attenuatedLightColor = light.color * (light.distanceAttenuation * light.shadowAttenuation);
        //float shade = saturate(dot(light.direction, WorldNormal));        
        float shade = Remap(dot(light.direction, surfaceNormal), float2(-1.0, 1.0), float2(0.001, 0.999));
        //diffuseColor += (attenuatedLightColor * max(shade, HardBacklight(CameraDirection, light.direction, 2.4, WorldNormal, ViewDirection))) * SampleGradientRGB(ShadingRamp, shade);
        //diffuseColor += attenuatedLightColor * SampleGradientRGB(ShadingRamp, shade);
        float3 ramp = SAMPLE_TEXTURE2D(ShadingRamp, ShadingSampler, float2(shade * occlusion, 0.0));
        diffuseColor += attenuatedLightColor * max(HardBacklight(CameraDirection, light.direction, BacklightStrength, WorldNormal, ViewDirection), ramp) * DiffuseTexture * DiffuseColor;

        //diffuseColor += LightingLambert(attenuatedLightColor, light.direction, WorldNormal);

        /*float3 spec = SpecularMap * SpecularTexture * DirectSpecularValue(light.direction, WorldNormal, ViewDirection, Smoothness);
        spec = step(0.01, spec) * SpecularColor * attenuatedLightColor;*/

        float3 spec = RGBValue(attenuatedLightColor) * RGBValue(SpecularColor) * DirectSpecularValue(light.direction, WorldNormal, ViewDirection, Smoothness);
        spec = SpecularMap * SpecularTexture * spec;// DirectSpecularValue(mainLight.direction, WorldNormal, ViewDirection, Smoothness);
        spec = step(0.01f, spec) * SpecularColor * attenuatedLightColor;

        specularColor += spec;
    }

    //diffuseColor = diffuseColor * DiffuseTexture * DiffuseColor;
#endif

    Color = diffuseColor + specularColor;
}



void MainLightGradient_float(float3 WorldPosition, float3 WorldNormal, float3 ViewDirection, float3 CameraDirection,
    float3 AmbientColor, Gradient ShadingRamp, float BacklightStrength, float3 EdgeShineColor, float3 DiffuseTexture, float3 DiffuseColor,
    float3 NormalMap, float NormalMapStrength, float3 SpecularColor, float SpecularMap, float Smoothness,
    float SpecularTexture, float AmbientOcclusionMap, float AmbientOcclusionStrength, out float3 Color)
{
    float3 diffuseColor = 0;
    float3 specularColor = 0;

#ifndef SHADERGRAPH_PREVIEW
    float4 shadowCoord = TransformWorldToShadowCoord_Unlit(WorldPosition);
    Light mainLight = GetMainLight_Unlit(shadowCoord);

    float3 surfaceNormal = GetBlendedNormal(float3(0.0, 0.0, 1.0), NormalMap, NormalMapStrength, WorldNormal);
    float attenuation = mainLight.distanceAttenuation * mainLight.shadowAttenuation;
    float shade = dot(surfaceNormal, mainLight.direction);
    float finalLight = min(Remap(shade, float2(-0.5, 1.0), float2(0.0, 1.0)), attenuation);
    //float finalLight = min(shade, attenuation);
    float occlusion = saturate(AmbientOcclusionMap + (1.0 - AmbientOcclusionStrength));

    float3 rampedFinalLight = SampleGradientRGB(ShadingRamp, finalLight * occlusion);
    //float3 rampedFinalLight = SampleGradientRGB(ShadingRamp, Remap(finalLight, float2(-1.0, 1.0), float2(0.0, 1.0)) * occlusion);
    float3 ambient = DiffuseTexture * DiffuseColor * AmbientColor;
    float3 lightColor = mainLight.color + AmbientColor;
    diffuseColor += max(rampedFinalLight, HardBacklight(CameraDirection, mainLight.direction, BacklightStrength, WorldNormal, ViewDirection)) * lightColor * DiffuseTexture * DiffuseColor;
    diffuseColor += HardEdgeShine(mainLight.color, mainLight.direction, WorldNormal, ViewDirection) * mainLight.shadowAttenuation * EdgeShineColor;

    specularColor = RGBValue(mainLight.color * attenuation) * RGBValue(SpecularColor) * DirectSpecularValue(mainLight.direction, WorldNormal, ViewDirection, Smoothness);
    specularColor = SpecularMap * SpecularTexture * specularColor;
    specularColor = step(0.01f, specularColor) * SpecularColor * (mainLight.color * attenuation);
#endif

    Color = diffuseColor + specularColor;
}

void MainLightGradient_half(half3 WorldPosition, half3 WorldNormal, half3 ViewDirection, half3 CameraDirection,
    half3 AmbientColor, Gradient ShadingRamp, half BacklightStrength, half3 EdgeShineColor, half3 DiffuseTexture, half3 DiffuseColor,
    half3 NormalMap, half NormalMapStrength, half3 SpecularColor, half SpecularMap, half Smoothness,
    half SpecularTexture, half AmbientOcclusionMap, half AmbientOcclusionStrength, out half3 Color)
{
    half3 diffuseColor = 0;
    half3 specularColor = 0;

#ifndef SHADERGRAPH_PREVIEW
    half4 shadowCoord = TransformWorldToShadowCoord_Unlit(WorldPosition);
    Light mainLight = GetMainLight_Unlit(shadowCoord);

    half3 surfaceNormal = GetBlendedNormal(half3(0.0, 0.0, 1.0), NormalMap, NormalMapStrength, WorldNormal);
    half attenuation = mainLight.distanceAttenuation * mainLight.shadowAttenuation;
    half shade = dot(surfaceNormal, mainLight.direction);
    half finalLight = min(shade, attenuation);
    half occlusion = saturate(AmbientOcclusionMap + (1.0 - AmbientOcclusionStrength));

    half3 rampedFinalLight = SampleGradientRGB(ShadingRamp, Remap(finalLight, half2(-1.0, 1.0), half2(0.0, 1.0)) * occlusion);
    half3 ambient = DiffuseTexture * DiffuseColor * AmbientColor;
    half3 lightColor = mainLight.color + AmbientColor;
    diffuseColor += max(rampedFinalLight, HardBacklight(CameraDirection, mainLight.direction, BacklightStrength, WorldNormal, ViewDirection)) * lightColor * DiffuseTexture * DiffuseColor;
    diffuseColor += HardEdgeShine(mainLight.color, mainLight.direction, WorldNormal, ViewDirection) * mainLight.shadowAttenuation * EdgeShineColor;

    specularColor = RGBValue(mainLight.color * attenuation) * RGBValue(SpecularColor) * DirectSpecularValue(mainLight.direction, WorldNormal, ViewDirection, Smoothness);
    specularColor = SpecularMap * SpecularTexture * specularColor;
    specularColor = step(0.01f, specularColor) * SpecularColor * mainLight.color;
#endif

    Color = diffuseColor + specularColor;
}


void AdditionalLightsGradient_float(float3 WorldPosition, float3 WorldNormal, float3 ViewDirection, float3 CameraDirection,
    Gradient ShadingRamp, float BacklightStrength, float3 DiffuseTexture, float3 DiffuseColor,
    float3 NormalMap, float NormalMapStrength, float3 SpecularColor, float SpecularMap, float Smoothness,
    float SpecularTexture, float AmbientOcclusionMap, float AmbientOcclusionStrength, out float3 Color)
{
    float3 diffuseColor = 0;
    float3 specularColor = 0;

#ifndef SHADERGRAPH_PREVIEW
    WorldNormal = normalize(WorldNormal);
    int pixelLightCount = GetAdditionalLightsCount();
    for (int i = 0; i < pixelLightCount; ++i)
    {
        Light light = GetAdditionalLight_Unlit(i, WorldPosition);

        float3 surfaceNormal = GetBlendedNormal(float3(0.0, 0.0, 1.0), NormalMap, NormalMapStrength, WorldNormal);
        float occlusion = saturate(AmbientOcclusionMap + (1.0 - AmbientOcclusionStrength));

        float3 attenuatedLightColor = light.color * (light.distanceAttenuation * light.shadowAttenuation);
        float shade = Remap(dot(light.direction, surfaceNormal), float2(-0.5, 1.0), float2(0.0, 1.0));
        float3 gradient = SampleGradientRGB(ShadingRamp, shade * occlusion);
        diffuseColor += max(0.0, attenuatedLightColor * max(HardBacklight(CameraDirection, light.direction, BacklightStrength, WorldNormal, ViewDirection), gradient)) * DiffuseTexture * DiffuseColor;

        float3 spec = RGBValue(attenuatedLightColor) * RGBValue(SpecularColor) * DirectSpecularValue(light.direction, WorldNormal, ViewDirection, Smoothness);
        spec = SpecularMap * SpecularTexture * spec;
        spec = step(0.01f, spec) * SpecularColor * attenuatedLightColor;

        specularColor += spec;
    }
#endif

    Color = diffuseColor + specularColor;
}

void AdditionalLightsGradient_half(half3 WorldPosition, half3 WorldNormal, half3 ViewDirection, half3 CameraDirection,
    Gradient ShadingRamp, half BacklightStrength, half3 DiffuseTexture, half3 DiffuseColor,
    half3 NormalMap, half NormalMapStrength, half3 SpecularColor, half SpecularMap, half Smoothness,
    half SpecularTexture, half AmbientOcclusionMap, half AmbientOcclusionStrength, out half3 Color)
{
    half3 diffuseColor = 0;
    half3 specularColor = 0;

#ifndef SHADERGRAPH_PREVIEW
    WorldNormal = normalize(WorldNormal);
    int pixelLightCount = GetAdditionalLightsCount();
    for (int i = 0; i < pixelLightCount; ++i)
    {
        Light light = GetAdditionalLight_Unlit(i, WorldPosition);

        half3 surfaceNormal = GetBlendedNormal(half3(0.0, 0.0, 1.0), NormalMap, NormalMapStrength, WorldNormal);
        half occlusion = saturate(AmbientOcclusionMap + (1.0 - AmbientOcclusionStrength));

        half3 attenuatedLightColor = light.color * (light.distanceAttenuation * light.shadowAttenuation);
        half shade = Remap(dot(light.direction, surfaceNormal), half2(-1.0, 1.0), half2(0.0, 1.0));
        half3 gradient = SampleGradientRGB(ShadingRamp, shade * occlusion);
        diffuseColor += max(0.0, attenuatedLightColor * max(HardBacklight(CameraDirection, light.direction, BacklightStrength, WorldNormal, ViewDirection), gradient)) * DiffuseTexture * DiffuseColor;

        half3 spec = RGBValue(attenuatedLightColor) * RGBValue(SpecularColor) * DirectSpecularValue(light.direction, WorldNormal, ViewDirection, Smoothness);
        spec = SpecularMap * SpecularTexture * spec;
        spec = step(0.01f, spec) * SpecularColor * attenuatedLightColor;

        specularColor += spec;
    }
#endif

    Color = diffuseColor + specularColor;
}


#endif