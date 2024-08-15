#ifndef TOON_THREE_TONE_COLOR_MASK_INPUT_INCLUDED
#define TOON_THREE_TONE_COLOR_MASK_INPUT_INCLUDED

half _MainShadowLimit;
half _MainHighlightLimit;
half _MainEdgeSoftness;

half _AdditionalShadowLimit;
half _AdditionalHighlightLimit;
half _AdditionalEdgeSoftness;

half _HighlightValue;
half _MidtoneValue;
half _ShadowValue;

#endif

#include "../ToonColorMaskInput.hlsl"