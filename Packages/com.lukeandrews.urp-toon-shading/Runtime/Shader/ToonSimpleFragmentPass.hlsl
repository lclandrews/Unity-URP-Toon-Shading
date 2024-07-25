#ifndef TOON_SIMPLE_FRAG_PASS_INCLUDED
#define TOON_SIMPLE_FRAG_PASS_INCLUDED

#include "ToonSimpleSurface.hlsl" 

half4 ToonSimplePassFragment(Varyings input) : SV_Target
{
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

    float2 uv = float2(input.uAndNormalWS.x, input.vAndViewDirWS.x);

    ToonSimpleSurfaceData surfaceData;
    InitToonSimpleSurfaceData(uv, surfaceData);

    ToonSimpleInputData inputData;
    InitToonSimpleInputData(input, surfaceData.normalTS, inputData);

    Light mainLight = GetMainLight_Toon(inputData.shadowCoord);

#if !defined(_BACKLIGHT_OFF) || !defined(_EDGESHINE_OFF)
    half sVdotN = saturate(dot(inputData.viewDirectionWS, inputData.normalWS));
#endif

    half3 light = inputData.bakedGI + _AmbientColor;

    half NdotML = dot(inputData.normalWS, mainLight.direction);
    half rampedLight = GetSmoothStepShading(NdotML, mainLight.shadowAttenuation * surfaceData.occlusion, _SurfaceShadowLimit, _SurfaceHighlightLimit, _MidtoneValue, _EdgeSoftness);

#ifndef _BACKLIGHT_OFF
    light += max(rampedLight, ToonHardBacklight(inputData.cameraDirWS, mainLight.direction, _BacklightStrength, sVdotN)) * mainLight.color;
#else
    light += rampedLight * mainLight.color;
#endif    

#ifndef _SPECULAR_OFF
    half smoothness = exp2(10 * surfaceData.smoothness + 1);
    half specularValue = RGBValue(surfaceData.specular);

    half3 specular = ToonLightSpecular(mainLight.direction, mainLight.color, inputData.normalWS, inputData.viewDirectionWS,
        surfaceData.specular, specularValue, surfaceData.specularTexture, smoothness);
#endif

#ifdef _ADDITIONAL_LIGHTS
    int pixelLightCount = GetAdditionalLightsCount();
    for (int i = 0; i < pixelLightCount; ++i)
    {
        Light additionalLight = GetAdditionalLight_Toon(i, inputData.positionWS);
        half NdotAL = dot(inputData.normalWS, additionalLight.direction);

        half aSteppedShade = GetSmoothStepShading(NdotAL, additionalLight.shadowAttenuation * surfaceData.occlusion, _SurfaceShadowLimit, _SurfaceHighlightLimit, _MidtoneValue, _EdgeSoftness);
        half aSteppedAttenuation = GetSmoothStepAttenuation(additionalLight.distanceAttenuation, _AttenuationShadowLimit, _AttenuationHighlightLimit, _MidtoneValue, _EdgeSoftness);

#ifndef _BACKLIGHT_OFF
        light += additionalLight.color * max(ToonHardBacklight(inputData.cameraDirWS, additionalLight.direction, _BacklightStrength, sVdotN) * aSteppedAttenuation, aSteppedShade * aSteppedAttenuation);
#else
        light += additionalLight.color * (aSteppedShade * aSteppedAttenuation);
#endif

#ifndef _SPECULAR_OFF
        specular += ToonLightSpecular(additionalLight.direction, additionalLight.color * aSteppedAttenuation, inputData.normalWS, inputData.viewDirectionWS,
            surfaceData.specular, specularValue, surfaceData.specularTexture, smoothness);
#endif  
    }
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
