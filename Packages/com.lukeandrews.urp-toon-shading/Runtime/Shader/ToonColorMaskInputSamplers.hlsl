#ifndef TOON_COLOR_MASK_INPUT_SAMPLERS_INCLUDED
#define TOON_COLOR_MASK_INPUT_SAMPLERS_INCLUDED

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"

half4 SampleColorMask(float2 uv, TEXTURE2D_PARAM( colorMaskMap, sampler_colorMaskMap), half3 r, half3 g, half3 b, half3 a)
{
    return SAMPLE_TEXTURE2D(colorMaskMap, sampler_colorMaskMap, uv);
}

#endif
