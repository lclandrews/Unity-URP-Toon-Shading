#ifndef TOON_TEXTURE_INPUT_INCLUDED
#define TOON_TEXTURE_INPUT_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"

#include "Core/ToonCore.hlsl"

CBUFFER_START(UnityPerMaterial)
half3 _AmbientColor;

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
CBUFFER_END

TEXTURE2D(_BaseMap);            SAMPLER(sampler_BaseMap);

TEXTURE2D(_MainRamp);           SAMPLER(sampler_MainRamp);
TEXTURE2D(_AddRamp);            SAMPLER(sampler_AddRamp);

TEXTURE2D(_SpecMap);            SAMPLER(sampler_SpecMap);
TEXTURE2D(_SpecTexMap);         SAMPLER(sampler_SpecTexMap);
TEXTURE2D(_BumpMap);            SAMPLER(sampler_BumpMap);
TEXTURE2D(_OcclusionMap);       SAMPLER(sampler_OcclusionMap);
TEXTURE2D(_EmissionMap);        SAMPLER(sampler_EmissionMap);

#include "Core/ToonSurfaceInput.hlsl"

#endif
