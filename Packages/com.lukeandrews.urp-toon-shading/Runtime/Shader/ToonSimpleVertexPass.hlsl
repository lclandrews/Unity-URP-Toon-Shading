#ifndef TOON_SIMPLE_VERT_PASS_INCLUDED
#define TOON_SIPLE_VERT_PASS_INCLUDED

struct Attributes
{
    float4 positionOS   : POSITION;
    float3 normalOS     : NORMAL;
    float4 tangentOS    : TANGENT;
    float2 texcoord     : TEXCOORD0;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Varyings
{
    float4 uAndNormalWS            : TEXCOORD0; // x: u, yzw: normalWS
    float4 vAndViewDirWS           : TEXCOORD1; // x: v, yzw: viewDirWS

    half4 vertexSHAndFog           : TEXCOORD2;

#ifdef _NORMALMAP

#if !defined(_BACKLIGHT_OFF)
    float4 positionWS              : TEXCOORD3; // xyz: position world, w: cameraDir.x
    half4 tangentWS                : TEXCOORD4; // xyz: tangent, w: cameraDir.y
    half4 bitangentWS              : TEXCOORD5; // xyz: bitangent, w: cameraDir.z
#else
    float3 positionWS              : TEXCOORD3;
    half3 tangentWS                : TEXCOORD4;
    half3 bitangentWS              : TEXCOORD5;
#endif
#ifdef _MAIN_LIGHT_SHADOWS
    float4 shadowCoord             : TEXCOORD6;
#endif

#else 
    float3 positionWS              : TEXCOORD3;
#if !defined(_BACKLIGHT_OFF)
    half3 cameraDirWS              : TEXCOORD4;
#endif
#ifdef _MAIN_LIGHT_SHADOWS
    float4 shadowCoord             : TEXCOORD5;
#endif

#endif

    float4 positionCS               : SV_POSITION;
    UNITY_VERTEX_INPUT_INSTANCE_ID
    UNITY_VERTEX_OUTPUT_STEREO
};

#include "ToonSimpleInputData.hlsl"

Varyings ToonSimplePassVertex(Attributes input)
{
    Varyings output = (Varyings)0;

    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, output);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

    float3 vertexPositionWS = TransformObjectToWorld(input.positionOS.xyz);
    float4 vertexPositionCS = TransformWorldToHClip(vertexPositionWS);

    VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);

    half3 viewDirWS = GetCameraPositionWS() - vertexPositionWS;
    half fogFactor = ComputeFogFactor(vertexPositionCS.z);

    float2 uv = TRANSFORM_TEX(input.texcoord, _BaseMap);
    output.vAndViewDirWS = float4(uv.y, viewDirWS);

#if !defined(_BACKLIGHT_OFF)
    half3 cameraDir = CameraDirection();
#endif

#ifdef _NORMALMAP
    output.uAndNormalWS = float4(uv.x, normalInput.normalWS.xyz);
#if !defined(_BACKLIGHT_OFF)
    output.positionWS = float4(vertexPositionWS, cameraDir.x);
    output.tangentWS = half4(normalInput.tangentWS, cameraDir.y);
    output.bitangentWS = half4(normalInput.bitangentWS, cameraDir.z);
#else
    output.positionWS = vertexPositionWS;
    output.tangentWS = normalInput.tangentWS;
    output.bitangentWS = normalInput.bitangentWS;    
#endif    


#else 
    output.uAndNormalWS = float4(uv.x, NormalizeNormalPerVertex(normalInput.normalWS));
    output.positionWS = vertexPositionWS;

#if !defined(_BACKLIGHT_OFF)
    output.cameraDirWS = cameraDir;
#endif

#endif
    output.vertexSHAndFog = half4 (SampleSHVertex(output.uAndNormalWS.yzw), fogFactor);

#if defined(_MAIN_LIGHT_SHADOWS) && !defined(_RECEIVE_SHADOWS_OFF)
#if SHADOWS_SCREEN
    output.shadowCoord = ComputeScreenPos(vertexPositionCS);
#else
    output.shadowCoord = TransformWorldToShadowCoord(vertexPositionWS);
#endif
#endif

    output.positionCS = vertexPositionCS;
    return output;
}

#endif