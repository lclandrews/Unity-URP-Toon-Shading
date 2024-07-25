#ifndef TOON_STANDARD_INPUT_INCLUDED
#define TOON_STANDARD_INPUT_INCLUDED

struct ToonStandardInputData
{
    float3  positionWS;
    half3   normalWS;
    half3   viewDirectionWS;
    float4  shadowCoord;
    half    fogCoord;
    half3   vertexLighting;
    half3   bakedGI;
    half3   cameraDirWS;
};

inline void InitToonStandardInputData(Varyings input, half3 normalTS, out ToonStandardInputData inputData)
{
    inputData = (ToonStandardInputData)0;

    half3 viewDirWS = input.vAndViewDirWS.yzw;
    viewDirWS = SafeNormalize(viewDirWS);
    inputData.viewDirectionWS = viewDirWS;

#ifdef _LIGHTMAP_ON
    inputData.bakedGI = SampleLightmap(input.lightMapAndFog.xyz, inputData.normalWS);
    inputData.fogCoord = input.lightMapAndFog.w;
#else
    inputData.bakedGI = SampleSHPixel(input.vertexSHAndFog.xyz, inputData.normalWS);
    inputData.fogCoord = input.vertexSHAndFog.w;
#endif 

#ifdef _NORMALMAP

#if defined(_ADDITIONAL_LIGHTS)

#if !defined(_BACKLIGHT_OFF)
    inputData.positionWS = input.positionWS.xyz;
#else
    inputData.positionWS = input.positionWS;
#endif
    inputData.vertexLighting = 0;

#elif defined (_ADDITIONAL_LIGHTS_VERTEX)

#if !defined(_BACKLIGHT_OFF)
    inputData.vertexLighting = input.vertexLight.xyz;
#else
    inputData.vertexLighting = input.vertexLight;
#endif
    inputData.positionWS = 0;
#else
    inputData.vertexLighting = 0;
    inputData.positionWS = 0;
#endif

#if !defined(_BACKLIGHT_OFF)

#if defined(_ADDITIONAL_LIGHTS)
    inputData.cameraDirWS = half3(input.positionWS.w, input.tangentWS.w, input.bitangentWS.w);
#elif defined (_ADDITIONAL_LIGHTS_VERTEX)
    inputData.cameraDirWS = half3(input.vertexLight.w, input.tangentWS.w, input.bitangentWS.w);
#else
    inputData.cameraDirWS = half3(input.cameraDirWSx, input.tangentWS.w, input.bitangentWS.w);
#endif

    inputData.normalWS = TransformTangentToWorld(normalTS, half3x3(input.tangentWS.xyz, input.bitangentWS.xyz, input.uAndNormalWS.yzw));
#else
    inputData.cameraDirWS = 0;

    inputData.normalWS = TransformTangentToWorld(normalTS, half3x3(input.tangentWS, input.bitangentWS, input.uAndNormalWS.yzw));
#endif    
#else 

    inputData.normalWS = input.uAndNormalWS.yzw;

#if defined(_ADDITIONAL_LIGHTS)
    inputData.positionWS = input.positionWS;
    inputData.vertexLighting = 0;
#elif defined (_ADDITIONAL_LIGHTS_VERTEX)    
    inputData.vertexLighting = input.vertexLight;
    inputData.positionWS = 0;
#else
    inputData.vertexLighting = 0;
    inputData.positionWS = 0;
#endif

#if !defined(_BACKLIGHT_OFF)
    inputData.cameraDirWS = input.cameraDirWS;
#else
    inputData.cameraDirWS = 0;
#endif

#endif

    inputData.normalWS = NormalizeNormalPerPixel(inputData.normalWS);

#if defined(_MAIN_LIGHT_SHADOWS) && !defined(_RECEIVE_SHADOWS_OFF)
    inputData.shadowCoord = input.shadowCoord;
#else
    inputData.shadowCoord = float4(0, 0, 0, 0);
#endif       
}

#endif
