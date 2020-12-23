#ifndef TOON_STANDARD_SURFACE_INCLUDED
#define TOON_STANDARD_SURFACE_INCLUDED

struct ToonStandardSurfaceData
{
    half3 albedo;
    half3 specular;
    half  specularTexture;
    half  smoothness;
    half3 normalTS;
    half3 emission;
    half  occlusion;
    half  alpha;
};

inline void InitToonStandardSurfaceData(float2 uv, out ToonStandardSurfaceData outData)
{
    half4 albedoAlpha = SampleAlbedoAlpha(uv, TEXTURE2D_ARGS(_BaseMap, sampler_BaseMap));
    outData.albedo = albedoAlpha.rgb * _BaseColor.rgb;
    outData.alpha = Alpha(albedoAlpha.a, _BaseColor, _Cutoff);
    half4 specSmooth = SampleSpecular(uv, _SpecColor, _Smoothness, TEXTURE2D_ARGS(_SpecMap, sampler_SpecMap));
    outData.specular = specSmooth.rgb;
    float2 texUV = RotateUV(uv, half2(0.5, 0.5), _SpecTexRot);
    outData.specularTexture = SampleSpecularTexture(texUV * _SpecTexTile, TEXTURE2D_ARGS(_SpecTexMap, sampler_SpecTexMap));
    outData.smoothness = specSmooth.a;
    outData.normalTS = SampleNormal(uv, TEXTURE2D_ARGS(_BumpMap, sampler_BumpMap), _BumpScale);
    outData.occlusion = SampleOcclusion(uv, _OcclusionStrength, TEXTURE2D_ARGS(_OcclusionMap, sampler_OcclusionMap));
    outData.emission = SampleEmission(uv, _EmissionColor.rgb, TEXTURE2D_ARGS(_EmissionMap, sampler_EmissionMap));
}

#endif
