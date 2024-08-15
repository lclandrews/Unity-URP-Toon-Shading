#ifndef TOON_INPUT_DATA_INCLUDED
#define TOON_INPUT_DATA_INCLUDED

//struct ToonInputData
//{
//    float3 positionWS;
//    half3 normalWS;
//    half3 viewDirectionWS;
//    float4 shadowCoord;
//    half fogCoord;
//    half3 bakedGI;
//    half3 cameraDirWS;
//};

struct ToonInputData
{
    float3 positionWS;
    float3 normalWS;
    half3 viewDirectionWS;
    half3 cameraDirWS;
    float4 shadowCoord;
    half fogCoord;
    half3 vertexLighting;
    half3 bakedGI;
    float2 normalizedScreenSpaceUV;
    half4 shadowMask;
    half3x3 tangentToWorld;

#if defined(DEBUG_DISPLAY)
    half2   dynamicLightmapUV;
    half2   staticLightmapUV;
    float3  vertexSH;

    half3 brdfDiffuse;
    half3 brdfSpecular;
    float2 uv;
    uint mipCount;

    // texelSize :
    // x = 1 / width
    // y = 1 / height
    // z = width
    // w = height
    float4 texelSize;

    // mipInfo :
    // x = quality settings minStreamingMipLevel
    // y = original mip count for texture
    // z = desired on screen mip level
    // w = loaded mip level
    float4 mipInfo;
#endif    
};

inline void InitToonInputData(Varyings input, half3 normalTS, out ToonInputData inputData)  
{
    inputData = (ToonInputData)0;

    half3 viewDirWS = input.vAndViewDirWS.yzw;
    viewDirWS = SafeNormalize(viewDirWS);
    inputData.viewDirectionWS = viewDirWS;  

#ifdef _NORMALMAP

#if !defined(_BACKLIGHT_OFF)

    inputData.positionWS = input.positionWS.xyz;
    inputData.cameraDirWS = half3(input.positionWS.w, input.tangentWS.w, input.bitangentWS.w);
    inputData.tangentToWorld = half3x3(input.tangentWS.xyz, input.bitangentWS.xyz, input.uAndNormalWS.yzw);
    inputData.normalWS = TransformTangentToWorld(normalTS, inputData.tangentToWorld);
#else

    inputData.positionWS = input.positionWS;
    inputData.cameraDirWS = 0;
    inputData.tangentToWorld = half3x3(input.tangentWS, input.bitangentWS, input.uAndNormalWS.yzw);
    inputData.normalWS = TransformTangentToWorld(normalTS, inputData.tangentToWorld);
#endif    

#else // !_NORMALMAP

    inputData.normalWS = input.uAndNormalWS.yzw;
    inputData.positionWS = input.positionWS;

#if !defined(_BACKLIGHT_OFF)
    inputData.cameraDirWS = input.cameraDirWS;
#else
    inputData.cameraDirWS = 0;
#endif

#endif
    inputData.normalWS = NormalizeNormalPerPixel(inputData.normalWS);

#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
    inputData.shadowCoord = input.shadowCoord;
#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
    inputData.shadowCoord = TransformWorldToShadowCoord(inputData.positionWS);
#else
    inputData.shadowCoord = float4(0, 0, 0, 0);
#endif // end _NORMALMAP

#if defined(LIGHTMAP_ON)
    inputData.fogCoord = InitializeInputDataFog(float4(inputData.positionWS, 1.0), input.staticLightmapUvAndFog.z);
#else
    inputData.fogCoord = InitializeInputDataFog (float4(inputData.positionWS, 1.0), input.vertexSHAndFog.w);
#endif
    
#ifdef _ADDITIONAL_LIGHTS_VERTEX
    inputData.vertexLighting = input.vertexLight;
#else
    inputData.vertexLighting = half3(0, 0, 0);
#endif

#if defined(DYNAMICLIGHTMAP_ON)
    inputData.bakedGI = SAMPLE_GI(input.staticLightmapUvAndFog.xy, input.dynamicLightmapUV, input.vertexSHAndFog.xyz, inputData.normalWS);
#else
    inputData.bakedGI = SAMPLE_GI(input.staticLightmapUvAndFog.xy, input.vertexSHAndFog.xyz, inputData.normalWS);
#endif

    inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(input.positionCS);
    inputData.shadowMask = SAMPLE_SHADOWMASK(input.staticLightmapUV);  
    
#if defined(DEBUG_DISPLAY)
#if defined(DYNAMICLIGHTMAP_ON)
    inputData.dynamicLightmapUV = input.dynamicLightmapUV.xy;
#endif
#if defined(LIGHTMAP_ON)
    inputData.staticLightmapUV = input.staticLightmapUvAndFog.xy;
#else
    inputData.vertexSH = input.vertexSHAndFog.xyz;
#endif
#endif
};

#endif
