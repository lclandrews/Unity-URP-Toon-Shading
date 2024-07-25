#ifndef TOON_META_PASS_INCLUDED
#define TOON_META_PASS_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/UniversalMetaPass.hlsl"

half4 UniversalFragmentMetaLit(Varyings input) : SV_Target
{
    ToonSurfaceData surfaceData;
    InitToonSurfaceData(input.uv, surfaceData);

    BRDFData brdfData;
    
    // Direct implementation of InitializeBRDFData to avoid SPECULAR_SETUP define
    half reflectivity = ReflectivitySpecular(surfaceData.specular);
    half oneMinusReflectivity = half(1.0) - reflectivity;
    half3 brdfDiffuse = surfaceData.albedo * oneMinusReflectivity;
    half3 brdfSpecular = surfaceData.specular;
    InitializeBRDFDataDirect(surfaceData.albedo, brdfDiffuse, brdfSpecular, reflectivity, oneMinusReflectivity, surfaceData.smoothness, surfaceData.alpha, brdfData);   

    MetaInput metaInput;
    metaInput.Albedo = brdfData.diffuse + brdfData.specular * brdfData.roughness * 0.5;
    metaInput.Emission = surfaceData.emission;
    return UniversalFragmentMeta(input, metaInput);
}
#endif
