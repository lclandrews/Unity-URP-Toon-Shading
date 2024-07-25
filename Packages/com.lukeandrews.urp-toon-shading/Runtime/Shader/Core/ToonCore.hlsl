#ifndef TOON_CORE_INCLUDED
#define TOON_CORE_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"

// -------------------------------------
// Universal Pipeline keywords
// -------------------------------------
// _MAIN_LIGHT_SHADOWS
// _MAIN_LIGHT_SHADOWS_CASCADE
// _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
// _ADDITIONAL_LIGHT_SHADOWS
// _SHADOWS_SOFT
// _MIXED_LIGHTING_SUBTRACTIVE

// -------------------------------------
// Unity defined keywords
// -------------------------------------
// DIRLIGHTMAP_COMBINED
// LIGHTMAP_ON

// -------------------------------------
// Defines
// -------------------------------------

#if defined(_FOG_LINEAR) || defined(_FOG_EXP) || defined(_FOG_EXP2)
#define _FOG_ON 1
#else
#define _FOG_ON 0
#endif

// -------------------------------------
// Gradients
// -------------------------------------

struct ToonColorGradient
{
    int mode;
    int colorsLength;
    half4 colors[8];
};

ToonColorGradient NewToonGradient(int type, int colors, half4 keys[8])
{
    ToonColorGradient output =
    {
        type, colors, keys
    };
    return output;
}

half3 SampleRawToonGradient(int type, int colors, half4 keys[8], half time)
{
    half3 color = keys[0].rgb;
    UNITY_LOOP
    for (int c = 1; c < colors; c++)
    {
        half colorPos = saturate((time - keys[c - 1].w) / (keys[c].w - keys[c - 1].w)) * step(c, colors - 1);
        color = lerp(color, keys[c].rgb, lerp(colorPos, step(0.01, colorPos), type));
    }
#ifndef UNITY_COLORSPACE_GAMMA
    color = SRGBToLinear(color);
#endif
    return color;
}

half3 SampleToonColorGradient(ToonColorGradient gradient, half time)
{
    half3 color = gradient.colors[0].rgb;
    UNITY_LOOP
    for (int c = 1; c < gradient.colorsLength; c++)
    {
        half colorPos = saturate((time - gradient.colors[c - 1].w) / (gradient.colors[c].w - gradient.colors[c - 1].w)) * step(c, gradient.colorsLength - 1);
        color = lerp(color, gradient.colors[c].rgb, lerp(colorPos, step(0.01, colorPos), gradient.mode));
    }
#ifndef UNITY_COLORSPACE_GAMMA
    color = SRGBToLinear(color);
#endif
    return color;
}

// -------------------------------------
// Unlit Shader Shadows
// -------------------------------------

half ComputeCascadeIndex_Unlit(float3 positionWS)
{
    float3 fromCenter0 = positionWS - _CascadeShadowSplitSpheres0.xyz;
    float3 fromCenter1 = positionWS - _CascadeShadowSplitSpheres1.xyz;
    float3 fromCenter2 = positionWS - _CascadeShadowSplitSpheres2.xyz;
    float3 fromCenter3 = positionWS - _CascadeShadowSplitSpheres3.xyz;
    float4 distances2 = float4(dot(fromCenter0, fromCenter0), dot(fromCenter1, fromCenter1), dot(fromCenter2, fromCenter2), dot(fromCenter3, fromCenter3));

    half4 weights = half4(distances2 < _CascadeShadowSplitSphereRadii);
    weights.yzw = saturate(weights.yzw - weights.xyz);

    return 4 - dot(weights, half4(4, 3, 2, 1));
}

float4 TransformWorldToShadowCoord_Unlit(float3 positionWS)
{
    half cascadeIndex = ComputeCascadeIndex_Unlit(positionWS);
    return mul(_MainLightWorldToShadow[cascadeIndex], float4(positionWS, 1.0));
}

real SampleShadowmap_Unlit(TEXTURE2D_SHADOW_PARAM( ShadowMap, sampler_ShadowMap),
float4 shadowCoord, ShadowSamplingDatasamplingData,
half4 shadowParams, bool isPerspectiveProjection = true)
{
    // Compiler will optimize this branch away as long as isPerspectiveProjection is known at compile time
    if (isPerspectiveProjection)
        shadowCoord.xyz /= shadowCoord.
w;

    real attenuation;
    real shadowStrength = shadowParams.x;

    UNITY_BRANCH
        if (shadowParams.y > 0)
            attenuation = SampleShadowmapFiltered(TEXTURE2D_SHADOW_ARGS(ShadowMap, sampler_ShadowMap), shadowCoord, samplingData);
        else            
            attenuation = SAMPLE_TEXTURE2D_SHADOW(ShadowMap, sampler_ShadowMap, shadowCoord.xyz); // 1-tap hardware comparison

    attenuation = LerpWhiteTo(attenuation, shadowStrength);

    // Shadow coords that fall out of the light frustum volume must always return attenuation 1.0
    // TODO: We could use branch here to save some perf on some platforms.
    return BEYOND_SHADOW_FAR(shadowCoord) ? 1.0 :
attenuation;
}

half MainLightRealtimeShadow_Unlit(float4 shadowCoord)
{
#if SHADOWS_SCREEN
    return SampleScreenSpaceShadowmap(shadowCoord);
#else
    ShadowSamplingData shadowSamplingData = GetMainLightShadowSamplingData();
    half4 shadowParams = GetMainLightShadowParams();
    return SampleShadowmap_Unlit(TEXTURE2D_ARGS(_MainLightShadowmapTexture, sampler_MainLightShadowmapTexture), shadowCoord, shadowSamplingData, shadowParams, false);
#endif
}

half AdditionalLightRealtimeShadow_Unlit(int lightIndex, float3 positionWS)
{
    ShadowSamplingData shadowSamplingData = GetAdditionalLightShadowSamplingData();

#if USE_STRUCTURED_BUFFER_FOR_LIGHT_DATA
    lightIndex = _AdditionalShadowsIndices[lightIndex];

    // We have to branch here as otherwise we would sample buffer with lightIndex == -1.
    // However this should be ok for platforms that store light in SSBO.
    UNITY_BRANCH
        if (lightIndex < 0)
            return 1.0;

    float4 shadowCoord = mul(_AdditionalShadowsBuffer[lightIndex].worldToShadowMatrix, float4(positionWS, 1.0));
#else
    float4 shadowCoord = mul(_AdditionalLightsWorldToShadow[lightIndex], float4(positionWS, 1.0));
#endif

    half4 shadowParams = GetAdditionalLightShadowParams(lightIndex);
    return SampleShadowmap_Unlit(TEXTURE2D_ARGS(_AdditionalLightsShadowmapTexture, sampler_AdditionalLightsShadowmapTexture), shadowCoord, shadowSamplingData, shadowParams, true);
}

// -------------------------------------
// Toon Shader Shadows
// -------------------------------------

half ComputeCascadeIndex_Toon(float3 positionWS)
{
    float3 fromCenter0 = positionWS - _CascadeShadowSplitSpheres0.xyz;
    float3 fromCenter1 = positionWS - _CascadeShadowSplitSpheres1.xyz;
    float3 fromCenter2 = positionWS - _CascadeShadowSplitSpheres2.xyz;
    float3 fromCenter3 = positionWS - _CascadeShadowSplitSpheres3.xyz;
    float4 distances2 = float4(dot(fromCenter0, fromCenter0), dot(fromCenter1, fromCenter1), dot(fromCenter2, fromCenter2), dot(fromCenter3, fromCenter3));

    half4 weights = half4(distances2 < _CascadeShadowSplitSphereRadii);
    weights.yzw = saturate(weights.yzw - weights.xyz);

    return 4 - dot(weights, half4(4, 3, 2, 1));
}

float4 TransformWorldToShadowCoord_Toon(float3 positionWS)
{
#ifdef _MAIN_LIGHT_SHADOWS_CASCADE
    half cascadeIndex = ComputeCascadeIndex_Toon(positionWS);
    return mul(_MainLightWorldToShadow[cascadeIndex], float4(positionWS, 1.0));
#else
    return mul(_MainLightWorldToShadow[0], float4(positionWS, 1.0));
#endif
}

real SampleShadowmap_Toon(TEXTURE2D_SHADOW_PARAM( ShadowMap, sampler_ShadowMap),
float4 shadowCoord, ShadowSamplingDatasamplingData,
half4 shadowParams, bool isPerspectiveProjection = true)
{
    // Compiler will optimize this branch away as long as isPerspectiveProjection is known at compile time
    if (isPerspectiveProjection)
        shadowCoord.xyz /= shadowCoord.
w;

    real attenuation;
    real shadowStrength = shadowParams.x;

    UNITY_BRANCH
        if (shadowParams.y > 0)
            attenuation = SampleShadowmapFiltered(TEXTURE2D_SHADOW_ARGS(ShadowMap, sampler_ShadowMap), shadowCoord, samplingData);
        else
            attenuation = SAMPLE_TEXTURE2D_SHADOW(ShadowMap, sampler_ShadowMap, shadowCoord.xyz); // 1-tap hardware comparison

    attenuation = LerpWhiteTo(attenuation, shadowStrength);

    // Shadow coords that fall out of the light frustum volume must always return attenuation 1.0
    // TODO: We could use branch here to save some perf on some platforms.
    return BEYOND_SHADOW_FAR(shadowCoord) ? 1.0 :
attenuation;
}

half MainLightRealtimeShadow_Toon(float4 shadowCoord)
{
#if !defined(_MAIN_LIGHT_SHADOWS) || defined(_RECEIVE_SHADOWS_OFF)
    return 1.0h;
#endif

#if SHADOWS_SCREEN
    return SampleScreenSpaceShadowmap(shadowCoord);
#else
    ShadowSamplingData shadowSamplingData = GetMainLightShadowSamplingData();
    half4 shadowParams = GetMainLightShadowParams();
    return SampleShadowmap_Toon(TEXTURE2D_ARGS(_MainLightShadowmapTexture, sampler_MainLightShadowmapTexture), shadowCoord, shadowSamplingData, shadowParams, false);
#endif
}

half AdditionalLightRealtimeShadow_Toon(int lightIndex, float3 positionWS)
{
#if !defined(_ADDITIONAL_LIGHT_SHADOWS) || defined(_RECEIVE_SHADOWS_OFF)
    return 1.0h;
#endif

    ShadowSamplingData shadowSamplingData = GetAdditionalLightShadowSamplingData();

#if USE_STRUCTURED_BUFFER_FOR_LIGHT_DATA
    lightIndex = _AdditionalShadowsIndices[lightIndex];

    // We have to branch here as otherwise we would sample buffer with lightIndex == -1.
    // However this should be ok for platforms that store light in SSBO.
    UNITY_BRANCH
        if (lightIndex < 0)
            return 1.0;

    float4 shadowCoord = mul(_AdditionalShadowsBuffer[lightIndex].worldToShadowMatrix, float4(positionWS, 1.0));
#else
    float4 shadowCoord = mul(_AdditionalLightsWorldToShadow[lightIndex], float4(positionWS, 1.0));
#endif

    half4 shadowParams = GetAdditionalLightShadowParams(lightIndex);
    return SampleShadowmap_Toon(TEXTURE2D_ARGS(_AdditionalLightsShadowmapTexture, sampler_AdditionalLightsShadowmapTexture), shadowCoord, shadowSamplingData, shadowParams, true);
}

// -------------------------------------
// Unlit Shader Lighting
// -------------------------------------

Light GetMainLight_Unlit(float4 shadowCoord)
{
    Light light = GetMainLight();
    light.shadowAttenuation = MainLightRealtimeShadow_Unlit(shadowCoord);
    return light;
}

Light GetAdditionalPerObjectLight_Unlit(int perObjectLightIndex, float3 positionWS)
{
    // Abstraction over Light input constants
#if USE_STRUCTURED_BUFFER_FOR_LIGHT_DATA
    float4 lightPositionWS = _AdditionalLightsBuffer[perObjectLightIndex].position;
    half3 color = _AdditionalLightsBuffer[perObjectLightIndex].color.rgb;
    half4 distanceAndSpotAttenuation = _AdditionalLightsBuffer[perObjectLightIndex].attenuation;
    half4 spotDirection = _AdditionalLightsBuffer[perObjectLightIndex].spotDirection;
    half4 lightOcclusionProbeInfo = _AdditionalLightsBuffer[perObjectLightIndex].occlusionProbeChannels;
#else
    float4 lightPositionWS = _AdditionalLightsPosition[perObjectLightIndex];
    half3 color = _AdditionalLightsColor[perObjectLightIndex].rgb;
    half4 distanceAndSpotAttenuation = _AdditionalLightsAttenuation[perObjectLightIndex];
    half4 spotDirection = _AdditionalLightsSpotDir[perObjectLightIndex];
    half4 lightOcclusionProbeInfo = _AdditionalLightsOcclusionProbes[perObjectLightIndex];
#endif

    // Directional lights store direction in lightPosition.xyz and have .w set to 0.0.
    // This way the following code will work for both directional and punctual lights.
    float3 lightVector = lightPositionWS.xyz - positionWS * lightPositionWS.w;
    float distanceSqr = max(dot(lightVector, lightVector), HALF_MIN);

    half3 lightDirection = half3(lightVector * rsqrt(distanceSqr));
    half attenuation = DistanceAttenuation(distanceSqr, distanceAndSpotAttenuation.xy) * AngleAttenuation(spotDirection.xyz, lightDirection, distanceAndSpotAttenuation.zw);

    Light light;
    light.direction = lightDirection;
    light.distanceAttenuation = attenuation;
    light.shadowAttenuation = AdditionalLightRealtimeShadow_Unlit(perObjectLightIndex, positionWS);
    light.color = color;

    return light;
}

Light GetAdditionalLight_Unlit(uint i, float3 positionWS)
{
    int perObjectLightIndex = GetPerObjectLightIndex(i);
    return GetAdditionalPerObjectLight_Unlit(perObjectLightIndex, positionWS);
}

// -------------------------------------
// Toon Shader Lighting
// -------------------------------------

Light GetMainLight_Toon(float4 shadowCoord)
{
    Light light = GetMainLight();
    light.shadowAttenuation = MainLightRealtimeShadow(shadowCoord);
    return light;
}

Light GetAdditionalPerObjectLight_Toon(int perObjectLightIndex, float3 positionWS)
{
    // Abstraction over Light input constants
#if USE_STRUCTURED_BUFFER_FOR_LIGHT_DATA
    float4 lightPositionWS = _AdditionalLightsBuffer[perObjectLightIndex].position;
    half3 color = _AdditionalLightsBuffer[perObjectLightIndex].color.rgb;
    half4 distanceAndSpotAttenuation = _AdditionalLightsBuffer[perObjectLightIndex].attenuation;
    half4 spotDirection = _AdditionalLightsBuffer[perObjectLightIndex].spotDirection;
    half4 lightOcclusionProbeInfo = _AdditionalLightsBuffer[perObjectLightIndex].occlusionProbeChannels;
#else
    float4 lightPositionWS = _AdditionalLightsPosition[perObjectLightIndex];
    half3 color = _AdditionalLightsColor[perObjectLightIndex].rgb;
    half4 distanceAndSpotAttenuation = _AdditionalLightsAttenuation[perObjectLightIndex];
    half4 spotDirection = _AdditionalLightsSpotDir[perObjectLightIndex];
    half4 lightOcclusionProbeInfo = _AdditionalLightsOcclusionProbes[perObjectLightIndex];
#endif

    // Directional lights store direction in lightPosition.xyz and have .w set to 0.0.
    // This way the following code will work for both directional and punctual lights.
    float3 lightVector = lightPositionWS.xyz - positionWS * lightPositionWS.w;
    float distanceSqr = max(dot(lightVector, lightVector), HALF_MIN);

    half3 lightDirection = half3(lightVector * rsqrt(distanceSqr));
    half attenuation = DistanceAttenuation(distanceSqr, distanceAndSpotAttenuation.xy) * AngleAttenuation(spotDirection.xyz, lightDirection, distanceAndSpotAttenuation.zw);

    Light light;
    light.direction = lightDirection;
    light.distanceAttenuation = attenuation;
    light.shadowAttenuation = AdditionalLightRealtimeShadow(perObjectLightIndex, positionWS);
    light.color = color;

    // In case we're using light probes, we can sample the attenuation from the `unity_ProbesOcclusion`
#if defined(LIGHTMAP_ON) || defined(_MIXED_LIGHTING_SUBTRACTIVE)
    // First find the probe channel from the light.
    // Then sample `unity_ProbesOcclusion` for the baked occlusion.
    // If the light is not baked, the channel is -1, and we need to apply no occlusion.

    // probeChannel is the index in 'unity_ProbesOcclusion' that holds the proper occlusion value.
    int probeChannel = lightOcclusionProbeInfo.x;

    // lightProbeContribution is set to 0 if we are indeed using a probe, otherwise set to 1.
    half lightProbeContribution = lightOcclusionProbeInfo.y;

    half probeOcclusionValue = unity_ProbesOcclusion[probeChannel];
    light.distanceAttenuation *= max(probeOcclusionValue, lightProbeContribution);
#endif

    return light;
}

Light GetAdditionalLight_Toon(uint i, float3 positionWS)
{
    int perObjectLightIndex = GetPerObjectLightIndex(i);
    return GetAdditionalPerObjectLight_Toon(perObjectLightIndex, positionWS);
}

half3 SubtractDirectMainLightFromLightmap_Toon(Light mainLight, half3 normalWS, half3 bakedGI)
{
    // Let's try to make realtime shadows work on a surface, which already contains
    // baked lighting and shadowing from the main sun light.
    // Summary:
    // 1) Calculate possible value in the shadow by subtracting estimated light contribution from the places occluded by realtime shadow:
    //      a) preserves other baked lights and light bounces
    //      b) eliminates shadows on the geometry facing away from the light
    // 2) Clamp against user defined ShadowColor.
    // 3) Pick original lightmap value, if it is the darkest one.


    // 1) Gives good estimate of illumination as if light would've been shadowed during the bake.
    // We only subtract the main direction light. This is accounted in the contribution term below.
    half shadowStrength = GetMainLightShadowStrength();
    half contributionTerm = saturate(dot(mainLight.direction, normalWS));
    half3 lambert = mainLight.color * contributionTerm;
    half3 estimatedLightContributionMaskedByInverseOfShadow = lambert * (1.0 - mainLight.shadowAttenuation);
    half3 subtractedLightmap = bakedGI - estimatedLightContributionMaskedByInverseOfShadow;

    // 2) Allows user to define overall ambient of the scene and control situation when realtime shadow becomes too dark.
    half3 realtimeShadow = max(subtractedLightmap, _SubtractiveShadowColor.xyz);
    realtimeShadow = lerp(bakedGI, realtimeShadow, shadowStrength);

    // 3) Pick darkest color
    return min(bakedGI, realtimeShadow);
}

void MixRealtimeAndBakedGI_Toon(inout Light light, half3 normalWS, inout half3 bakedGI, half4 shadowMask)
{
#if defined(_MIXED_LIGHTING_SUBTRACTIVE) && defined(LIGHTMAP_ON)
    bakedGI = SubtractDirectMainLightFromLightmap_Toon(light, normalWS, bakedGI);
#endif
}

// -------------------------------------
// Helpers
// -------------------------------------

half3 NormalBlend(half3 A, half3 B)
{
    return normalize(half3(A.rg + B.rg, A.b * B.b));
}

half3 GetBlendedNormal(half3 base, half3 normalMap, half mapStrength, half3 surfaceNormal)
{
    return NormalBlend(lerp(base, normalMap, mapStrength), surfaceNormal);
}

half3 RGBtoHSV(half3 In)
{
    half4 K = half4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
    half4 P = lerp(half4(In.bg, K.wz), half4(In.gb, K.xy), step(In.b, In.g));
    half4 Q = lerp(half4(P.xyw, In.r), half4(In.r, P.yzx), step(P.x, In.r));
    half D = Q.x - min(Q.w, Q.y);
    half E = 1e-10;
    return half3(abs(Q.z + (Q.w - Q.y) / (6.0 * D + E)), D / (Q.x + E), Q.x);
}

half RGBValue(half3 color)
{
    half A = lerp(color.b, color.g, step(color.b, color.g));
    half B = lerp(A, color.r, step(A, color.r));
    return B;
}

half Remap(half In, half2 InMinMax, half2 OutMinMax)
{
    return OutMinMax.x + (In - InMinMax.x) * (OutMinMax.y - OutMinMax.x) / (InMinMax.y - InMinMax.x);
}

half FresnelEffect(half3 Normal, half3 ViewDir, half Power)
{
    return pow((1.0 - saturate(dot(Normal, ViewDir))), Power);
}

half FresnelEffect(half sNdotV, half Power)
{
    return pow((1.0 - sNdotV), Power);
}

half3 CameraDirection()
{
    return -1 * mul((float3x3) UNITY_MATRIX_M, UNITY_MATRIX_IT_MV[2].xyz);
}

float2 RotateUV(float2 uv, half2 centre, half rotation)
{
    //rotation matrix
    uv -= centre;
    float s = sin(rotation);
    float c = cos(rotation);

    //center rotation matrix
    float2x2 rMatrix = float2x2(c, -s, s, c);
    rMatrix *= 0.5;
    rMatrix += 0.5;
    rMatrix = rMatrix * 2 - 1;

    //multiply the UVs by the rotation matrix
    uv.xy = mul(uv.xy, rMatrix);
    uv += centre;
    return uv;
}

// -------------------------------------
// Lighting Helpers
// -------------------------------------

half GetSmoothStepAttenuation(float attenuation, half shadowEdge, half lightEdge, half midTone, half edgeSoftness)
{
    half highlight = smoothstep(lightEdge, lightEdge + edgeSoftness, attenuation);
    half mid = smoothstep(0.0, shadowEdge, attenuation);
    return (mid * midTone) + (highlight * (1.0 - midTone));
}

half GetSmoothStepShading(half NdotL, half shadowAttenuation, half shadowEdge, half lightEdge, half midTone, half edgeSoftness)
{
    half a = min(Remap(NdotL, half2(-0.5, 1.0), half2(0.0, 1.0)), shadowAttenuation);
    half highlight = smoothstep(lightEdge, lightEdge + edgeSoftness, a);
    half mid = smoothstep(shadowEdge, shadowEdge + edgeSoftness, a);
    return (mid * midTone) + (highlight * (1.0 - midTone));
}

half3 ToonDirectSpecular(half3 lightColor, half3 lightDir, half3 normal, half3 viewDir, half3 specular, half smoothnessExp)
{
    //smoothness = exp2(10 * smoothness + 1);

    float3 halfVec = SafeNormalize(float3(lightDir) + float3(viewDir));
    half NdotH = saturate(dot(normal, halfVec));
    half modifier = pow(NdotH, smoothnessExp);
    half3 specularReflection = specular * modifier;
    return lightColor * specularReflection;
}

half ToonDirectSpecularValue(half3 lightDir, half3 normal, half3 viewDir, half smoothnessExp)
{
    //smoothness = exp2(10 * smoothness + 1);

    float3 halfVec = SafeNormalize(float3(lightDir) + float3(viewDir));
    half NdotH = saturate(dot(normal, halfVec));
    return pow(NdotH, smoothnessExp);
}

half3 ToonHardEdgeShine(half3 lightColor, half3 lightDir, half3 normal, half3 viewDir)
{
    half vl = (dot(viewDir, lightDir) * -1.0) + -0.5;
    half nl = (dot(normal, lightDir) + -0.3) * 4;
    float edge = clamp((vl + nl) * -1.0, 0.2, 1.1);
    return step(edge, FresnelEffect(normal, viewDir, 8)) * lightColor;
}

half3 ToonHardEdgeShine(half3 lightColor, half VdotL, half NdotL, half sVdotN)
{
    half vl = (VdotL * -1.0) + -0.5;
    half nl = (NdotL + -0.3) * 4;
    float edge = clamp((vl + nl) * -1.0, 0.2, 1.1);
    return step(edge, FresnelEffect(sVdotN, 8)) * lightColor;
}

half3 ToonSoftEdgeShine(half3 lightColor, half3 lightDir, half3 normal, half3 viewDir)
{
    half vl = (dot(viewDir, lightDir) * -1.0F) + -0.5;
    half nl = (dot(normal, lightDir) + -0.3) * 4;
    float edge = clamp((vl + nl) * -1.0, 0.2, 1.1);
    return (1.0 - edge) * FresnelEffect(normal, viewDir, 8) * lightColor;
}

half3 ToonSoftEdgeShine(half3 lightColor, half VdotL, half NdotL, half sVdotN)
{
    half vl = (VdotL * -1.0F) + -0.5;
    half nl = (NdotL + -0.3) * 4;
    float edge = clamp((vl + nl) * -1.0, 0.2, 1.1);
    return (1.0 - edge) * FresnelEffect(sVdotN, 8) * lightColor;
}

half ToonHardBacklight(half3 cameraDir, half3 lightDir, half strength, half3 normal, half3 viewDir)
{
    float lc = saturate(dot(lightDir, cameraDir) + -0.6);
    float edge = (1.0 - (lc * strength)) + 0.01;
    return step(edge, FresnelEffect(normal, viewDir, 3.0));
}

half ToonHardBacklight(half3 cameraDir, half3 lightDir, half strength, half sVdotN)
{
    float lc = saturate(dot(lightDir, cameraDir) + -0.6);
    float edge = (1.0 - (lc * strength)) + 0.01;
    return step(edge, FresnelEffect(sVdotN, 3.0));
}

half ToonSoftBacklight(half3 cameraDir, half3 lightDir, half strength, half3 normal, half3 viewDir)
{
    float lc = saturate(dot(lightDir, cameraDir) + -0.6);
    float edge = (1.0 - (lc * strength)) + 0.01;
    return (1.0 - edge) * FresnelEffect(normal, viewDir, 3.0);
}

half ToonSoftBacklight(half3 cameraDir, half3 lightDir, half strength, half sVdotN)
{
    float lc = saturate(dot(lightDir, cameraDir) + -0.6);
    float edge = (1.0 - (lc * strength)) + 0.01;
    return (1.0 - edge) * FresnelEffect(sVdotN, 3.0);
}

half3 ToonLightSpecular(half3 lightDir, half3 attenuatedLightColor, half3 normalWS, half3 viewDir, half3 specular, half specularValue, half specularTexture, half smoothnessExp)
{
    half sV = RGBValue(attenuatedLightColor) * specularValue * ToonDirectSpecularValue(lightDir, normalWS, viewDir, smoothnessExp);
    sV *= specularTexture;
    return step(0.01, sV) * specular * attenuatedLightColor;
}

half3 ToonMainLightDiffuse(Light mainLight, half3 normalWS, half3 viewDir, half3 cameraDir, half occlusion, half backlightStrength, ToonColorGradient shadingRamp)
{
    half shade = dot(normalWS, mainLight.direction);
    half finalLight = min(Remap(shade, half2(-0.5, 1.0), half2(0.0, 1.0)), mainLight.shadowAttenuation);
    half3 rampedFinalLight = SampleToonColorGradient(shadingRamp, finalLight * occlusion);
#ifndef _BACKLIGHT_OFF
    half3 color = max(rampedFinalLight, ToonHardBacklight(cameraDir, mainLight.direction, backlightStrength, normalWS, viewDir)) * mainLight.color;
#else
    half3 color = rampedFinalLight * mainLight.color;
#endif    
    return color;
}

half3 ToonMainLightDiffuse(Light mainLight, half3 normalWS, half3 viewDir, half3 cameraDir, half occlusion, half backlightStrength, int rampType,
    int rampLength, half4 rampKeys[8])
{
    half shade = dot(normalWS, mainLight.direction);
    half finalLight = min(Remap(shade, half2(-0.5, 1.0), half2(0.0, 1.0)), mainLight.shadowAttenuation);
    half3 rampedFinalLight = SampleRawToonGradient(rampType, rampLength, rampKeys, finalLight * occlusion);
#ifndef _BACKLIGHT_OFF
    half3 color = max(rampedFinalLight, ToonHardBacklight(cameraDir, mainLight.direction, backlightStrength, normalWS, viewDir)) * mainLight.color;
#else
    half3 color = rampedFinalLight * mainLight.color;
#endif    
    return color;
}

half3 ToonAdditionalLightDiffuse(half3 lightDir, half3 attenuatedLightColor, half3 normalWS, half3 viewDir, half3 cameraDir, half occlusion, half backlightStrength,
    ToonColorGradient shadingRamp)
{
    half shade = Remap(dot(lightDir, normalWS), half2(-0.5, 1.0), half2(0.0, 1.0));
    half3 gradient = SampleToonColorGradient(shadingRamp, shade * occlusion);
#ifndef _BACKLIGHT_OFF
    return attenuatedLightColor * max(ToonHardBacklight(cameraDir, lightDir, backlightStrength, normalWS, viewDir), gradient);
#else
    return attenuatedLightColor * gradient;
#endif
}

half3 ToonAdditionalLightDiffuse(half3 lightDir, half3 attenuatedLightColor, half3 normalWS, half3 viewDir, half3 cameraDir, half occlusion,
    half backlightStrength, int rampType, int rampLength, half4 rampKeys[8])
{
    half shade = Remap(dot(lightDir, normalWS), half2(-0.5, 1.0), half2(0.0, 1.0));
    half3 gradient = SampleRawToonGradient(rampType, rampLength, rampKeys, shade * occlusion);
#ifndef _BACKLIGHT_OFF
    return attenuatedLightColor * max(ToonHardBacklight(cameraDir, lightDir, backlightStrength, normalWS, viewDir), gradient);
#else
    return attenuatedLightColor * gradient;
#endif
}

half3 ToonAdditionalVertexLightDiffuse(half3 light, half occlusion, ToonColorGradient shadingRamp)
{
    half3 gradient = SampleToonColorGradient(shadingRamp, RGBValue(light) * occlusion);
    return light * gradient;
}

half3 ToonAdditionalVertexLightDiffuse(half3 light, half occlusion, int rampType, int rampLength, half4 rampKeys[8])
{
    half3 gradient = SampleRawToonGradient(rampType, rampLength, rampKeys, RGBValue(light) * occlusion);
    return light * gradient;
}

half3 ToonAdditionalVertexLightDiffuse(half3 light, half occlusion, Texture2D rampTexture, sampler rampSampler)
{
    half3 gradient = SAMPLE_TEXTURE2D(rampTexture, rampSampler, clamp(float2(RGBValue(light) * occlusion, 0.0f), 0.05f, 0.95f)).xyz;
    return light * gradient;
}

half3 ToonGlobalIllumination(half3 gi, half occlusion, ToonColorGradient shadingRamp)
{
    half3 gradient = SampleToonColorGradient(shadingRamp, RGBValue(gi) * occlusion);
    return gi * gradient;
}

half3 ToonGlobalIllumination(half3 gi, half occlusion, int rampType, int rampLength, half4 rampKeys[8])
{
    half3 gradient = SampleRawToonGradient(rampType, rampLength, rampKeys, RGBValue(gi) * occlusion);
    return gi * gradient;
}

#endif

// -------------------------------------
// Final Lighting Params
// -------------------------------------

// Light
// -- Ambient Light
// -- Main Light Gradient
// -- Additional Light Gradient
// -- Back Light Stength
// -- Edge Shine Color

// Color
// -- Diffuse Color
// -- Diffuse Texture

// Normal
// -- Normal Map
// -- Normal Map Strength

// Maps
// -- Specular / Specular Texture / Ambient Occlusion

// Specular
// -- Specular Color
// -- Smoothness

// Specular Texture
// -- Specular Texture Tiling
// -- Specular Texture Rotation

// Ambient Occlusion
// -- Ambient Occlusion Strength

// Texture
// -- Texture Tiling

// -------------------------------------
// Notes
// -------------------------------------

// The intensity of a light is built into the .color property
// The original color of the light is multiplied by intensity

// Don't need to wrap variables in directives as gfx api should
// optmise these away

// Edge shine needs tweaking, would be nice to control the size and
// to get better control of the angle

// Constant floats are causing issues with blending normals
// Should research constants further

// For some reason specular still doesn't match up with the
// original

// Ambient color is definitely off

// So objects are stil not casting shadows on themselves
// shadows are cast from spot lights but not from directional
// could be shadow cascades again
// Realted to cascades when it is set ot none in settings

// Still need to find an appropriate solution for the gradients
// Would be nice to create a tool to create a texture from a gradient editor

// Changed edge shine to be multiplied by shadow attenuation

// May have to look into re-implementing effects due to light probes

// Bug where intensity of lights seems to be additive is still
// causing issues

// Need to implement fog also

// Ultilise float array's and custom editor to add editor gradient
// to final shader