#ifndef TOON_SIMPLE_SURFACE_INCLUDED
#define TOON_SIMPLE_SURFACE_INCLUDED

struct ToonSimpleSurfaceData
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

inline void InitToonSimpleSurfaceData(float2 uv, out ToonSimpleSurfaceData outData)
{
    half4 albedoAlpha = SampleAlbedoAlpha(uv, TEXTURE2D_ARGS(_BaseMap, sampler_BaseMap));
    outData.albedo = albedoAlpha.rgb * _BaseColor.rgb;
    outData.alpha = Alpha(albedoAlpha.a, _BaseColor, _Cutoff);
    outData.normalTS = SampleNormal(uv, TEXTURE2D_ARGS(_BumpMap, sampler_BumpMap), _BumpScale);
    half4 maskValues = SampleToonMask(uv, TEXTURE2D_ARGS(_ToonMask, sampler_ToonMask));
    outData.specular = SampleSpecular(maskValues.r, _SpecColor, outData.albedo);
    outData.smoothness = maskValues.a * _Smoothness;
    float2 texUV = RotateUV(uv, half2(0.5, 0.5), _SpecTexRot);
    outData.specularTexture = SampleSpecularTexture(texUV * _SpecTexTile, TEXTURE2D_ARGS(_SpecTexMap, sampler_SpecTexMap));    
    outData.occlusion = SampleOcclusion(maskValues.g, _OcclusionStrength);
    outData.emission = SampleEmission(outData.albedo, _EmissionColor, maskValues.b);
}

#endif
