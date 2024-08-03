#ifndef TOON_HELPERS_INCLUDED
#define TOON_HELPERS_INCLUDED

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/SpaceTransforms.hlsl"

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
    half  E = 1e-10;
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
    return -1 * mul((float3x3)UNITY_MATRIX_M, UNITY_MATRIX_IT_MV[2].xyz);
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

#endif