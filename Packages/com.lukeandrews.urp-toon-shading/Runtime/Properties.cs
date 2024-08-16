using UnityEngine;

namespace ToonShading
{
    public static class Properties
    {
        public const string Cull = "_Cull";
        public static readonly int CullId = Shader.PropertyToID(Cull);

        public const string AlphaClip = "_AlphaClip";
        public static readonly int AlphaClipId = Shader.PropertyToID(AlphaClip);

        public const string Cutoff = "_Cutoff";
        public static readonly int CutoffId = Shader.PropertyToID(Cutoff);

        public const string ReceiveShadows = "_ReceiveShadows";
        public static readonly int ReceiveShadowsId = Shader.PropertyToID(ReceiveShadows);

        public const string BaseMap = "_BaseMap";
        public static readonly int BaseMapId = Shader.PropertyToID(BaseMap);

        public const string BaseColor = "_BaseColor";
        public static readonly int BaseColorId = Shader.PropertyToID(BaseColor);

        public const string Emission = "_Emission";
        public static readonly int EmissionId = Shader.PropertyToID(Emission);

        public const string EmissionColor = "_EmissionColor";
        public static readonly int EmissionColorId = Shader.PropertyToID(EmissionColor);

        public const string QueueOffset = "_QueueOffset";
        public static readonly int QueueOffsetId = Shader.PropertyToID(QueueOffset);

        public const string BacklightStrength = "_BacklightStrength";
        public static readonly int BacklightStrengthId = Shader.PropertyToID(BaseMap);

        public const string ShineColor = "_ShineColor";
        public static readonly int ShineColorId = Shader.PropertyToID(ShineColor);

        public const string Smoothness = "_Smoothness";
        public static readonly int SmoothnessId = Shader.PropertyToID(Smoothness);

        public const string SpecColor = "_SpecColor";
        public static readonly int SpecColorId = Shader.PropertyToID(SpecColor);

        public const string ToonMask = "_ToonMask";
        public static readonly int ToonMaskId = Shader.PropertyToID(ToonMask);

        public const string SpecTexMap = "_SpecTexMap";
        public static readonly int SpecTexMapId = Shader.PropertyToID(SpecTexMap);

        public const string SpecTexTile = "_SpecTexTile";
        public static readonly int SpecTexTileId = Shader.PropertyToID(SpecTexTile);

        public const string SpecTexRot = "_SpecTexRot";
        public static readonly int SpecTexRotId = Shader.PropertyToID(SpecTexRot);

        public const string BumpMap = "_BumpMap";
        public static readonly int BumpMapId = Shader.PropertyToID(BumpMap);

        public const string BumpScale = "_BumpScale"; 
        public static readonly int BumpScaleId = Shader.PropertyToID(BumpScale);

        public const string OcclusionStrength = "_OcclusionStrength";
        public static readonly int OcclusionStrengthId = Shader.PropertyToID(OcclusionStrength);

        public const string ColorMaskMap = "_ColorMaskMap";
        public static readonly int ColorMaskMapId = Shader.PropertyToID(ColorMaskMap);

        public const string ColorMaskRColor = "_ColorMaskRColor";
        public static readonly int ColorMaskRColorId = Shader.PropertyToID(ColorMaskRColor);

        public const string ColorMaskGColor = "_ColorMaskGColor";
        public static readonly int ColorMaskGColorId = Shader.PropertyToID(ColorMaskGColor);

        public const string ColorMaskBColor = "_ColorMaskBColor";
        public static readonly int ColorMaskBColorId = Shader.PropertyToID(ColorMaskBColor);

        public const string ColorMaskAColor = "_ColorMaskAColor";
        public static readonly int ColorMaskAColorId = Shader.PropertyToID(ColorMaskAColor);

        public const string OutlineWidth = "_OutlineWidth";
        public static readonly int OutlineWidthId = Shader.PropertyToID(OutlineWidth);

        public const string OutlineMap = "_OutlineMap";
        public static readonly int OutlineMapId = Shader.PropertyToID(OutlineMap);

        public const string OutlineStartFadeDistance = "_OutlineStartFadeDistance";
        public static readonly int OutlineStartFadeDistanceId = Shader.PropertyToID(OutlineStartFadeDistance);

        public const string OutlineEndFadeDistance = "_OutlineEndFadeDistance";
        public static readonly int OutlineEndFadeDistanceId = Shader.PropertyToID(OutlineEndFadeDistance);

        public const string OutlineColor = "_OutlineColor";
        public static readonly int OutlineColorId = Shader.PropertyToID(OutlineColor);

        public const string OutlineOffsetZ = "_OutlineOffsetZ";
        public static readonly int OutlineOffsetZId = Shader.PropertyToID(OutlineOffsetZ);

        public const string FillSpecular = "_FillSpecular";
        public static readonly int FillSpecularId = Shader.PropertyToID(FillSpecular);

        public const string FillOcclusion = "_FillOcclusion";
        public static readonly int FillOcclusionId = Shader.PropertyToID(FillOcclusion);

        public const string FillEmission = "_FillEmission";
        public static readonly int FillEmissionId = Shader.PropertyToID(FillEmission);

        public const string FillSmoothness = "_FillSmoothness";
        public static readonly int FillSmoothnessId = Shader.PropertyToID(FillSmoothness);

        public const string Specular = "_Specular";
        public static readonly int SpecularId = Shader.PropertyToID(Specular);

        public const string Backlight = "_Backlight";
        public static readonly int BacklightId = Shader.PropertyToID(Backlight);

        public const string EdgeShine = "_EdgeShine";
        public static readonly int EdgeShineId = Shader.PropertyToID(EdgeShine);

        public const string Outline = "_Outline";
        public static readonly int OutlineId = Shader.PropertyToID(Outline);

        public const string OutlineFade = "_OutlineFade";
        public static readonly int OutlineFadeId = Shader.PropertyToID(OutlineFade);

        public const string MainShadowLimit = "_MainShadowLimit";
        public static readonly int MainShadowLimitId = Shader.PropertyToID(MainShadowLimit);

        public const string MainHighlightLimit = "_MainHighlightLimit";
        public static readonly int MainHighlightLimitId = Shader.PropertyToID(MainHighlightLimit);

        public const string MainEdgeSoftness = "_MainEdgeSoftness";
        public static readonly int MainEdgeSoftnessId = Shader.PropertyToID(MainEdgeSoftness);

        public const string AdditionalShadowLimit = "_AdditionalShadowLimit";
        public static readonly int AdditionalShadowLimitId = Shader.PropertyToID(AdditionalShadowLimit);

        public const string AdditionalHighlightLimit = "_AdditionalHighlightLimit";
        public static readonly int AdditionalHighlightLimitId = Shader.PropertyToID(AdditionalHighlightLimit);

        public const string AdditionalEdgeSoftness = "_AdditionalEdgeSoftness";
        public static readonly int AdditionalEdgeSoftnessId = Shader.PropertyToID(AdditionalEdgeSoftness);

        public const string HighlightValue = "_HighlightValue";
        public static readonly int HighlightValueId = Shader.PropertyToID(HighlightValue);

        public const string MidtoneValue = "_MidtoneValue";
        public static readonly int MidtoneValueId = Shader.PropertyToID(MidtoneValue);

        public const string ShadowValue = "_ShadowValue";
        public static readonly int ShadowValueId = Shader.PropertyToID(ShadowValue);
    }
}
