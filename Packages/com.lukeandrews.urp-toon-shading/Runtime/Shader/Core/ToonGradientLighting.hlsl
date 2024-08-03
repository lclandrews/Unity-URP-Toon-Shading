#ifndef TOON_GRADIENT_LIGHTING_INCLUDED
#define TOON_GRADIENT_LIGHTING_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

#include "ToonGradient.hlsl"

// -------------------------------------
// Gradient Lighting Helpers
// -------------------------------------

half3 ToonMainLightDiffuse(Light mainLight, half3 normalWS, half3 viewDir, half3 cameraDir, half occlusion, half backlightStrength, ToonColorGradient shadingRamp)
{
    half shade = dot(normalWS, mainLight.direction);
    half finalLight = min(Remap(shade, half2(-0.5, 1.0), half2(0.0, 1.0)), mainLight.shadowAttenuation);
    half3 rampedFinalLight = SampleToonColorGradient(shadingRamp, finalLight * occlusion);
#ifndef _BACKLIGHT_OFF
    half3 color = max(rampedFinalLight, ToonHardBacklight(cameraDir, mainLight.direction, backlightStrength, normalWS, viewDir)) * mainLight.color;
#else
    half3 color = rampedFinalLight * mainLight.color;
#endif    
    return color;
}

half3 ToonMainLightDiffuse(Light mainLight, half3 normalWS, half3 viewDir, half3 cameraDir, half occlusion, half backlightStrength, int rampType,
    int rampLength, half4 rampKeys[8])
{
    half shade = dot(normalWS, mainLight.direction);
    half finalLight = min(Remap(shade, half2(-0.5, 1.0), half2(0.0, 1.0)), mainLight.shadowAttenuation);
    half3 rampedFinalLight = SampleRawToonGradient(rampType, rampLength, rampKeys, finalLight * occlusion);
#ifndef _BACKLIGHT_OFF
    half3 color = max(rampedFinalLight, ToonHardBacklight(cameraDir, mainLight.direction, backlightStrength, normalWS, viewDir)) * mainLight.color;
#else
    half3 color = rampedFinalLight * mainLight.color;
#endif    
    return color;
}

half3 ToonAdditionalLightDiffuse(half3 lightDir, half3 attenuatedLightColor, half3 normalWS, half3 viewDir, half3 cameraDir, half occlusion, half backlightStrength, 
    ToonColorGradient shadingRamp)
{
    half shade = Remap(dot(lightDir, normalWS), half2(-0.5, 1.0), half2(0.0, 1.0));
    half3 gradient = SampleToonColorGradient(shadingRamp, shade * occlusion);
#ifndef _BACKLIGHT_OFF
    return attenuatedLightColor * max(ToonHardBacklight(cameraDir, lightDir, backlightStrength, normalWS, viewDir), gradient);
#else
    return attenuatedLightColor * gradient;
#endif
}

half3 ToonAdditionalLightDiffuse(half3 lightDir, half3 attenuatedLightColor, half3 normalWS, half3 viewDir, half3 cameraDir, half occlusion,
    half backlightStrength, int rampType, int rampLength, half4 rampKeys[8])
{
    half shade = Remap(dot(lightDir, normalWS), half2(-0.5, 1.0), half2(0.0, 1.0));
    half3 gradient = SampleRawToonGradient(rampType, rampLength, rampKeys, shade * occlusion);
#ifndef _BACKLIGHT_OFF
    return attenuatedLightColor * max(ToonHardBacklight(cameraDir, lightDir, backlightStrength, normalWS, viewDir), gradient);
#else
    return attenuatedLightColor * gradient;
#endif
}

half3 ToonAdditionalVertexLightDiffuse(half3 light, half occlusion, ToonColorGradient shadingRamp)
{
    half3 gradient = SampleToonColorGradient(shadingRamp, RGBValue(light) * occlusion);
    return light * gradient;
}

half3 ToonAdditionalVertexLightDiffuse(half3 light, half occlusion, int rampType, int rampLength, half4 rampKeys[8])
{
    half3 gradient = SampleRawToonGradient(rampType, rampLength, rampKeys, RGBValue(light) * occlusion);
    return light * gradient;
}

half3 ToonAdditionalVertexLightDiffuse(half3 light, half occlusion, Texture2D rampTexture, sampler rampSampler)
{
    half3 gradient = SAMPLE_TEXTURE2D(rampTexture, rampSampler, clamp(float2(RGBValue(light) * occlusion, 0.0f), 0.05f, 0.95f)).xyz;
    return light * gradient;
}

half3 ToonGlobalIllumination(half3 gi, half occlusion, ToonColorGradient shadingRamp)
{
    half3 gradient = SampleToonColorGradient(shadingRamp, RGBValue(gi) * occlusion);
    return gi * gradient;
}

half3 ToonGlobalIllumination(half3 gi, half occlusion, int rampType, int rampLength, half4 rampKeys[8])
{
    half3 gradient = SampleRawToonGradient(rampType, rampLength, rampKeys, RGBValue(gi) * occlusion);
    return gi * gradient;
}

#endif