#ifndef TOON_SIMPLE_INPUT_DATA_INCLUDED
#define TOON_SIMPLE_INPUT_DATA_INCLUDED

struct ToonSimpleInputData
{
    float3  positionWS;
    half3   normalWS;
    half3   viewDirectionWS;
    float4  shadowCoord;
    half    fogCoord;
    half3   bakedGI;
    half3   cameraDirWS;
};

inline void InitToonSimpleInputData(Varyings input, half3 normalTS, out ToonSimpleInputData inputData)
{
    inputData = (ToonSimpleInputData)0;

    half3 viewDirWS = input.vAndViewDirWS.yzw;
    viewDirWS = SafeNormalize(viewDirWS);
    inputData.viewDirectionWS = viewDirWS;

    inputData.bakedGI = SampleSHPixel(input.vertexSHAndFog.xyz, inputData.normalWS);
    inputData.fogCoord = input.vertexSHAndFog.w;

#ifdef _NORMALMAP

#if !defined(_BACKLIGHT_OFF)

    inputData.positionWS = input.positionWS.xyz;
    inputData.cameraDirWS = half3(input.positionWS.w, input.tangentWS.w, input.bitangentWS.w);
    inputData.normalWS = TransformTangentToWorld(normalTS, half3x3(input.tangentWS.xyz, input.bitangentWS.xyz, input.uAndNormalWS.yzw));
#else

    inputData.positionWS = input.positionWS;
    inputData.cameraDirWS = 0;
    inputData.normalWS = TransformTangentToWorld(normalTS, half3x3(input.tangentWS, input.bitangentWS, input.uAndNormalWS.yzw));
#endif    

#else 

    inputData.normalWS = input.uAndNormalWS.yzw;
    inputData.positionWS = input.positionWS;

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
