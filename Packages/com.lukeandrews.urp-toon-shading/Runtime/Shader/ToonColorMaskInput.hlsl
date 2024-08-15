#ifndef TOON_COLOR_MASK_INPUT_INCLUDED
#define TOON_COLOR_MASK_INPUT_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

CBUFFER_START(UnityPerMaterial)
float4 _BaseMap_ST;
half4 _BaseColor;
half3 _ShineColor;
half3 _SpecColor;
half3 _EmissionColor;
half _Cutoff;
half _BacklightStrength;
half _Smoothness;
half _SpecTexTile;
half _SpecTexRot;
half _BumpScale;
half _OcclusionStrength;

half3 _ColorMaskRColor;
half3 _ColorMaskGColor;
half3 _ColorMaskBColor;
half3 _ColorMaskAColor;

// OUTLINE 
half _OutlineWidth;
half _OutlineStartFadeDistance;
half _OutlineEndFadeDistance;
float4 _OutlineMap_ST;
half4 _OutlineColor;
half _OutlineOffsetZ;
CBUFFER_END

TEXTURE2D(_BaseMap);            SAMPLER(sampler_BaseMap);

TEXTURE2D(_ColorMaskMap);       SAMPLER(sampler_ColorMaskMap);

TEXTURE2D(_ToonMask);           SAMPLER(sampler_ToonMask);
TEXTURE2D(_SpecTexMap);         SAMPLER(sampler_SpecTexMap);
TEXTURE2D(_BumpMap);            SAMPLER(sampler_BumpMap);

TEXTURE2D(_OutlineMap);            SAMPLER(sampler_OutlineMap);

#include "ToonInputSamplers.hlsl"
#include "ToonColorMaskInputSamplers.hlsl"

#endif
