#ifndef TOON_GRADIENT_FRAG_PASS_INCLUDED
#define TOON_GRADIENT_FRAG_PASS_INCLUDED

#include "Standard/ToonStandardSurface.hlsl" 

half4 ToonGradientPassFragment(Varyings input) : SV_Target
{
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

    float2 uv = float2(input.uAndNormalWS.x, input.vAndViewDirWS.x);

    ToonStandardSurfaceData surfaceData;
    InitToonStandardSurfaceData(uv, surfaceData);

    ToonStandardInputData inputData;
    InitToonStandardInputData(input, surfaceData.normalTS, inputData);

    Light mainLight = GetMainLight_Toon(inputData.shadowCoord);
    MixRealtimeAndBakedGI_Toon(mainLight, inputData.normalWS, inputData.bakedGI, half4(0, 0, 0, 0));

#if !defined(_BACKLIGHT_OFF) || !defined(_EDGESHINE_OFF)
    half sVdotN = saturate(dot(inputData.viewDirectionWS, inputData.normalWS));
#endif

    half3 light = inputData.bakedGI + _AmbientColor;    

    half NdotML = dot(inputData.normalWS, mainLight.direction);

    half mainIllumination = min(Remap(NdotML, half2(-0.5, 1.0), half2(0.0, 1.0)), mainLight.shadowAttenuation);
    half3 rampedMainLight = SampleRawToonGradient(_MainGradType, _MainGradColors, _MainGradKeys, mainIllumination * surfaceData.occlusion);
#ifndef _BACKLIGHT_OFF
    light += max(rampedMainLight, ToonHardBacklight(inputData.cameraDirWS, mainLight.direction, _BacklightStrength, sVdotN)) * mainLight.color;
#else
    light += rampedMainLight * mainLight.color;
#endif    

#ifndef _SPECULAR_OFF
    half smoothness = exp2(10 * surfaceData.smoothness + 1);
    half specularValue = RGBValue(surfaceData.specular);

    half3 mColor = mainLight.color * mainLight.shadowAttenuation;
    half3 specular = ToonLightSpecular(mainLight.direction, mColor, inputData.normalWS, inputData.viewDirectionWS, 
        surfaceData.specular, specularValue, surfaceData.specularTexture, smoothness );
#endif

#ifdef _ADDITIONAL_LIGHTS
    int pixelLightCount = GetAdditionalLightsCount();
    for (int i = 0; i < pixelLightCount; ++i)
    {
        Light additionalLight = GetAdditionalLight_Toon(i, inputData.positionWS);
        half3 alightColor = (additionalLight.distanceAttenuation * additionalLight.shadowAttenuation) * additionalLight.color;
        half NdotAL = dot(inputData.normalWS, additionalLight.direction);

        half additionalIllumination = Remap(NdotAL, half2(-0.5, 1.0), half2(0.0, 1.0));
        half3 rampedAdditionalLight = SampleRawToonGradient(_AddGradType, _AddGradColors, _AddGradKeys, additionalIllumination * surfaceData.occlusion);
#ifndef _BACKLIGHT_OFF
        light += alightColor * max(ToonHardBacklight(inputData.cameraDirWS, additionalLight.direction, _BacklightStrength, sVdotN), rampedAdditionalLight);
#else
        light += alightColor * rampedAdditionalLight;
#endif

#ifndef _SPECULAR_OFF
        specular += ToonLightSpecular(additionalLight.direction, alightColor, inputData.normalWS, inputData.viewDirectionWS,
            surfaceData.specular, specularValue, surfaceData.specularTexture, smoothness);
#endif  
    }
#endif

#ifdef _ADDITIONAL_LIGHTS_VERTEX
    light += ToonAdditionalVertexLightDiffuse(inputData.vertexLighting, surfaceData.occlusion, _AddGradType, _AddGradColors, _AddGradKeys);
#endif

    half3 albedo = light * surfaceData.albedo;

#ifndef _SPECULAR_OFF
    half4 color = half4(albedo.rgb + specular.rgb, surfaceData.alpha);
#else
    half4 color = half4(albedo.rgb, surfaceData.alpha);
#endif    

#ifndef _EDGESHINE_OFF
    half VdotL = dot(inputData.viewDirectionWS, mainLight.direction);
    color.rgb += ToonHardEdgeShine(mainLight.color, VdotL, NdotML, sVdotN) * mainLight.shadowAttenuation * _ShineColor;
#endif

    color.rgb += surfaceData.emission;
    color.rgb = MixFog(color.rgb, inputData.fogCoord);
    return color;
}

#endif
