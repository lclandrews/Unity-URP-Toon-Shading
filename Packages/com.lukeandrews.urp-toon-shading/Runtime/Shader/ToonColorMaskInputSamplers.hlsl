#ifndef TOON_COLOR_MASK_INPUT_SAMPLERS_INCLUDED
#define TOON_COLOR_MASK_INPUT_SAMPLERS_INCLUDED

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"

half3 SampleColorMask(float2 uv, TEXTURE2D_PARAM( colorMaskMap, sampler_colorMaskMap), half3 r, half3 g, half3 b, half3 a)
{
    half4 mask = SAMPLE_TEXTURE2D(colorMaskMap, sampler_colorMaskMap, uv);
    half3 color = half3(0.0, 0.0, 0.0);
    color += r * mask.r;
    color += g * mask.g;
    color += b * mask.b;
    color += a * mask.a;
    return color;
}

#endif
