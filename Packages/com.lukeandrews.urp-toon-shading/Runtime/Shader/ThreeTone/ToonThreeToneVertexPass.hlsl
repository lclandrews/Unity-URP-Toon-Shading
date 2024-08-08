#ifndef TOON_THREE_TONE_VERT_PASS_INCLUDED
#define TOON_THREE_TONE_VERT_PASS_INCLUDED

struct Attributes
{
    float4 positionOS   : POSITION;
    float3 normalOS     : NORMAL;
    float4 tangentOS    : TANGENT;
    float2 texcoord     : TEXCOORD0;
    float2 staticLightmapUV : TEXCOORD1;
    float2 dynamicLightmapUV : TEXCOORD2;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

#include "Core/ToonURP.hlsl"

struct Varyings
{
    float4 uAndNormalWS             : TEXCOORD0; // x: u, yzw: normalWS
    float4 vAndViewDirWS            : TEXCOORD1; // x: v, yzw: viewDirWS
    
    TOON_DECLARE_LIGHTMAP_OR_SH(staticLightmapUvAndFog, vertexSHAndFog, 2);
//#if defined(LIGHTMAP_ON)
//    float3 staticLightmapUvAndFog   : TEXCOORD2; // xy: lm, z: fog
//#else
//    half4 vertexSHAndFog            : TEXCOORD2; // xyz: sh, w: fog
//#endif        

#ifdef _NORMALMAP

#if !defined(_BACKLIGHT_OFF)
    float4 positionWS               : TEXCOORD3; // xyz: position world, w: cameraDir.x
    half4 tangentWS                 : TEXCOORD4; // xyz: tangent, w: cameraDir.y
    half4 bitangentWS               : TEXCOORD5; // xyz: bitangent, w: cameraDir.z
#else
    float3 positionWS               : TEXCOORD3;
    half3 tangentWS                 : TEXCOORD4;
    half3 bitangentWS               : TEXCOORD5;
#endif   
    
#else // !_NormalMap
    float3 positionWS               : TEXCOORD3;

#if !defined(_BACKLIGHT_OFF)
    half3 cameraDirWS               : TEXCOORD4;
#endif    
    
#endif

#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
    float4 shadowCoord              : TEXCOORD6;
#endif

#ifdef DYNAMICLIGHTMAP_ON
    float2  dynamicLightmapUV       : TEXCOORD7;
#endif
    
#ifdef _ADDITIONAL_LIGHTS_VERTEX
    half3 vertexLight               : TEXCOORD8;
#endif
    
    float4 positionCS               : SV_POSITION;
    UNITY_VERTEX_INPUT_INSTANCE_ID
    UNITY_VERTEX_OUTPUT_STEREO
};
    
#include "ToonInputData.hlsl"
#include "ToonSurfaceData.hlsl"
#include "Core/ToonHelpers.hlsl"
#include "ToonThreeToneLighting.hlsl"

Varyings ToonPassVertex(Attributes input)
{
    Varyings output = (Varyings)0;

    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, output);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);       
        
    float3 vertexPositionWS = TransformObjectToWorld(input.positionOS.xyz);
    float4 vertexPositionCS = TransformWorldToHClip(vertexPositionWS);

    VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);

    half3 viewDirWS = GetCameraPositionWS() - vertexPositionWS;
        
#if defined(_FOG_FRAGMENT)
    half fogFactor = 0;
#else
    half fogFactor = ComputeFogFactor(vertexPositionCS.z);
#endif 

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

    TOON_OUTPUT_LIGHTMAP_UV(input.staticLightmapUV, unity_LightmapST, output.staticLightmapUvAndFog);
#ifdef DYNAMICLIGHTMAP_ON
    output.dynamicLightmapUV = input.dynamicLightmapUV.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
#endif
    TOON_OUTPUT_SH(output.uAndNormalWS.yzw, output.vertexSHAndFog);

#ifdef _ADDITIONAL_LIGHTS_VERTEX
    output.vertexLight = 
        ToonThreeToneVertexLighting(vertexPositionWS, normalInput.normalWS, _AdditionalShadowLimit, _AdditionalHighlightLimit, _AdditionalEdgeSoftness);
#endif
    TOON_OUTPUT_FOG(output.staticLightmapUvAndFog, output.vertexSHAndFog, fogFactor);

#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
#if defined(_MAIN_LIGHT_SHADOWS_SCREEN)
    output.shadowCoord = ComputeScreenPos(vertexPositionCS);
#else
    output.shadowCoord = TransformWorldToShadowCoord(vertexPositionWS);
#endif
#endif

    output.positionCS = vertexPositionCS;
    return output;
}

#endif