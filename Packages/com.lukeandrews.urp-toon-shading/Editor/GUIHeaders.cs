namespace ToonShading.Editor
{
    public static class GUIHeaders
    {
        private const string _uiKeyPrefix = "UniversalRP:Material:UI_State:ToonShader";

        public const string MainShadow = "GlobalMainShadow";
        public const string MainHighlight = "GlobalMainHighlight";
        public const string MainEdgeSoftness = "GlobalMainEdgeSoftness";

        public const string AdditionalShadow = "GlobalAddShadow";
        public const string AdditionalHighlight = "GlobalAddHighlight";
        public const string AdditionalEdgeSoftness = "GlobalAddEdgeSoftness";

        public const string AdvancedLightingFoldout = "AdvancedLightingFoldout";
        public const string SurfaceOptionsFoldout = "SurfaceOptionsFoldout";
        public const string SurfaceInputsFoldout = "SurfaceInputsFoldout";
        public const string OutlineFoldout = "OutlineFoldout";
        public const string AdvancedFoldout = "AdvancedFoldout";

        public const string Highlight = "GlobalHighlight";
        public const string Midtone = "GlobalMidtone";
        public const string Shadow = "GlobalShadow";

        public static string GetStateKey(string variant, string header)
        {
            return $"{_uiKeyPrefix}:{variant}.{header}";
        }
    }
}