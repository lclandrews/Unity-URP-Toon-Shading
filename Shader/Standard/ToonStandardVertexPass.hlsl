#ifndef TOON_STANDARD_VERT_PASS_INCLUDED
#define TOON_STANDARD_VERT_PASS_INCLUDED

// float2 UV
// float3 normalWS
// float3 viewDirWS

// _NORMALMAP
// float3 tangentWS
// float3 bitangentWS

// _LIGHTMAP_ON
// float2 lightmapUV

// _ADDITIONAL_LIGHTS
// float3 positionWS

// _ADDITIONAL_LIGHTS_VERTEX
// float3 vertexLight

// !_BACKLIGHT_OFF
// float3 cameraDirWS

// _FOG_LINEAR || _FOG_EXP || _FOG_EXP2
// float fogFactor

// _MAIN_LIGHT_SHADOWS
// float4 shadowCoord

struct Attributes
{
    float4 positionOS   : POSITION;
    float3 normalOS     : NORMAL;
    float4 tangentOS    : TANGENT;
    float2 texcoord     : TEXCOORD0;
#ifdef LIGHTMAP_ON
    float2 lightmapUV   : TEXCOORD1;
#endif
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Varyings
{
    float4 uAndNormalWS             : TEXCOORD0; // x: u, yzw: normalWS
    float4 vAndViewDirWS            : TEXCOORD1; // x: v, yzw: viewDirWS

#ifdef _LIGHTMAP_ON
    float3 lightMapAndFog           : TEXCOORD2;
#else
    half4 vertexSHAndFog           : TEXCOORD2;
#endif

#ifdef _NORMALMAP

#if defined(_ADDITIONAL_LIGHTS)

#if !defined(_BACKLIGHT_OFF)
    float4 positionWS               : TEXCOORD3;
#else
    float3 positionWS               : TEXCOORD3;
#endif

#elif defined (_ADDITIONAL_LIGHTS_VERTEX)

#if !defined(_BACKLIGHT_OFF)
    half4 vertexLight               : TEXCOORD3;
#else
    half3 vertexLight               : TEXCOORD3;
#endif

#elif !defined(_BACKLIGHT_OFF)
    half cameraDirWSx               : TEXCOORD3;
#endif

#if !defined(_BACKLIGHT_OFF)
    half4 tangentWS                : TEXCOORD4;    // xyz: tangent, w: cameraDir.y
    half4 bitangentWS              : TEXCOORD5;    // xyz: bitangent, w: cameraDir.z
#else
    half3 tangentWS                : TEXCOORD4;
    half3 bitangentWS              : TEXCOORD5;
#endif    
#else 

#if defined(_ADDITIONAL_LIGHTS)
    float3 positionWS               : TEXCOORD3;
#elif defined (_ADDITIONAL_LIGHTS_VERTEX)
    half3 vertexLight               : TEXCOORD3;
#endif

#if !defined(_BACKLIGHT_OFF)
    half3 cameraDirWS              : TEXCOORD4;
#endif

#endif

#ifdef _MAIN_LIGHT_SHADOWS
    float4 shadowCoord              : TEXCOORD6;
#endif

    float4 positionCS               : SV_POSITION;
    UNITY_VERTEX_INPUT_INSTANCE_ID
    UNITY_VERTEX_OUTPUT_STEREO
};

#include "ToonStandardInput.hlsl"

Varyings ToonStandardPassVertex(Attributes input)
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

#if defined(_ADDITIONAL_LIGHTS_VERTEX)
    half3 vertexLight = VertexLighting(vertexPositionWS, normalInput.normalWS);
#endif

#if !defined(_BACKLIGHT_OFF)
    half3 cameraDir = CameraDirection();
#endif

#ifdef _NORMALMAP
    output.uAndNormalWS = float4(uv.x, normalInput.normalWS.xyz);

#if defined(_ADDITIONAL_LIGHTS)

#if !defined(_BACKLIGHT_OFF)
    output.positionWS = float4(vertexPositionWS, cameraDir.x);
#else
    output.positionWS = vertexPositionWS;
#endif

#elif defined (_ADDITIONAL_LIGHTS_VERTEX)
#if !defined(_BACKLIGHT_OFF)
    output.vertexLight = half4(vertexLight, cameraDir.x);
#else
    output.vertexLight = vertexLight;
#endif

#elif !defined(_BACKLIGHT_OFF)
    output.cameraDirWSx = cameraDir.x;
#endif

#if !defined(_BACKLIGHT_OFF)
    output.tangentWS = half4(normalInput.tangentWS, cameraDir.y);
    output.bitangentWS = half4(normalInput.bitangentWS, cameraDir.z);
#else
    output.tangentWS = normalInput.tangentWS;
    output.bitangentWS = normalInput.bitangentWS;
#endif    
#else 
    output.uAndNormalWS = float4(uv.x, NormalizeNormalPerVertex(normalInput.normalWS));

#if defined(_ADDITIONAL_LIGHTS)
    output.positionWS = vertexPositionWS;
#elif defined (_ADDITIONAL_LIGHTS_VERTEX)
    output.vertexLight = vertexLight;
#endif

#if !defined(_BACKLIGHT_OFF)
    output.cameraDirWS = cameraDir;
#endif

#endif

#ifdef _LIGHTMAP_ON
    output.lightMapAndFog = float3(input.lightmapUV.xy * unity_LightmapST.xy + unity_LightmapST.zw, fogFactor);
#else
    output.vertexSHAndFog = half4 (SampleSHVertex(output.uAndNormalWS.yzw), fogFactor);
#endif

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