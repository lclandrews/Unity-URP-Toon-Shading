#ifndef TOON_GRADIENT_INCLUDED
#define TOON_GRADIENT_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"

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

#endif