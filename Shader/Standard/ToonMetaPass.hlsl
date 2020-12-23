#ifndef TOON_META_PASS_INCLUDED
#define TOON_META_PASS_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/MetaInput.hlsl"
#include "ToonStandardSurface.hlsl"

struct Attributes
{
    float4 positionOS   : POSITION;
    float3 normalOS     : NORMAL;
    float2 uv0          : TEXCOORD0;
    float2 uv1          : TEXCOORD1;
    float2 uv2          : TEXCOORD2;
#ifdef _TANGENT_TO_WORLD
    float4 tangentOS     : TANGENT;
#endif
};

struct Varyings
{
    float4 positionCS   : SV_POSITION;
    float2 uv           : TEXCOORD0;
};

Varyings ToonVertexMeta(Attributes input)
{
    Varyings output;
    output.positionCS = MetaVertexPosition(input.positionOS, input.uv1, input.uv2,
    unity_LightmapST, unity_DynamicLightmapST);
    output.uv = TRANSFORM_TEX(input.uv0, _BaseMap);
    return output;
}

half4 ToonFragmentMeta(Varyings input) : SV_Target
{
    ToonStandardSurfaceData surfaceData;
    InitToonStandardSurfaceData(input.uv, surfaceData);

    MetaInput metaInput;
    metaInput.Albedo = surfaceData.albedo;
    metaInput.SpecularColor = surfaceData.specular;
    metaInput.Emission = surfaceData.emission;

    //metaInput.Emission = surfaceData.emission;

    return MetaFragment(metaInput);
}

//LWRP -> Universal Backwards Compatibility
Varyings LightweightVertexMeta(Attributes input)
{
    return ToonVertexMeta(input);
}

half4 LightweightFragmentMeta(Varyings input) : SV_Target
{
    return ToonFragmentMeta(input);
}

#endif
