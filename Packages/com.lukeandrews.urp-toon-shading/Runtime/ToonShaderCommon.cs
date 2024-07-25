using UnityEngine;

namespace ToonShader
{
    public static class ToonDefaultValues
    {
        public const float sShadowRangeMin = 0.0f;
        public const float sShadowRangeMax = 0.4f;

        public const float sShadowDefault = 0.0f;

        public const float sHighlightRangeMin = 0.4f;
        public const float sHighlightRangeMax = 1.0f;

        public const float sHighlightDefault = 0.75f;

        public const float aShadowRangeMin = 0.01f;
        public const float aShadowRangeMax = 0.06f;

        public const float aShadowDefault = 0.025f;

        public const float aHighlightRangeMin = 0.02f;
        public const float aHighlightRangeMax = 0.4f;

        public const float aHighlightDefault = 0.1f;

        public const float midtoneRangeMin = 0.2f;
        public const float midtoneRangeMax = 0.8f;

        public const float midtoneDefault = 0.5f;

        public const float edgeRangeMin = 0.01f;
        public const float edgeRangeMax = 0.2f;

        public const float edgeDefault = 0.05f;

        public const string uiKeyPrefix = "UniversalRP:Material:UI_State:ToonShader";

        public const string ambientHeader = ".GlobalAmbient";

        public const string sufaceShadowHeader = ".GlobalSurShadow";
        public const string sufaceHighlightHeader = ".GlobalSurHighlight";

        public const string attenuationShadowHeader = ".GlobalAttShadow";
        public const string attenuationHighlightHeader = ".GlobalAttHighlight";

        public const string midToneHeader = ".GlobalMidtone";

        public const string edgeSoftnessHeader = ".GlobalEdgeSoftness";

        public static Color GetToonAmbientLight()
        {
            return Shader.GetGlobalColor("_AmbientColor");
        }

        public static void SetSimpleRampGlobalValues(Color ambient, float surfaceShadow, float surfaceHighlight, float attenuationShadow, float attenuationHighlight,
        float midTone, float edgeSoftness)
        {
            Shader.SetGlobalColor("_AmbientColor", ambient);
            Shader.SetGlobalFloat("_SurfaceShadowLimit", surfaceShadow);
            Shader.SetGlobalFloat("_SurfaceHighlightLimit", surfaceHighlight);
            Shader.SetGlobalFloat("_AttenuationShadowLimit", attenuationShadow);
            Shader.SetGlobalFloat("_AttenuationHighlightLimit", attenuationHighlight);
            Shader.SetGlobalFloat("_MidtoneValue", midTone);
            Shader.SetGlobalFloat("_EdgeSoftness", edgeSoftness);
        }

        public static void GetSimpleRampGlobalValues(out Color ambient, out float surfaceShadow, out float surfaceHighlight, out float attenuationShadow, out float attenuationHighlight,
        out float midTone, out float edgeSoftness)
        {
            ambient = Shader.GetGlobalColor("_AmbientColor");
            surfaceShadow = Shader.GetGlobalFloat("_SurfaceShadowLimit");
            surfaceHighlight = Shader.GetGlobalFloat("_SurfaceHighlightLimit");
            attenuationShadow = Shader.GetGlobalFloat("_AttenuationShadowLimit");
            attenuationHighlight = Shader.GetGlobalFloat("_AttenuationHighlightLimit");
            midTone = Shader.GetGlobalFloat("_MidtoneValue");
            edgeSoftness = Shader.GetGlobalFloat("_EdgeSoftness");
        }
    }
}
