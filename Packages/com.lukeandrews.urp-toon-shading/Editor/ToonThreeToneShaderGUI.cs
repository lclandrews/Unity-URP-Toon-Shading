using UnityEditor;

using UnityEngine;

namespace ToonShading.Editor
{
    internal class ToonThreeToneShaderGUI : BaseToonShaderGUI
    {
        private SavedFloat _globalMainShadow = null;
        private SavedFloat _globalMainHighlight = null;
        private SavedFloat _globalMainEdge = null;

        private SavedFloat _globalAddShadow = null;
        private SavedFloat _globalAddHighlight = null;
        private SavedFloat _globalAddEdge = null;

        private SavedFloat _globalHighlight = null;
        private SavedFloat _globalMidtone = null;
        private SavedFloat _globalShadow = null;

        protected override string VariantName
        {
            get { return "ThreeTone"; }
        }

        public override void OnOpenGUI(Material material, MaterialEditor materialEditor)
        {
            _globalMainShadow = new SavedFloat(GUIHeaders.GetStateKey(VariantName, GUIHeaders.MainShadow), DefaultValues.ShadowStepDefault);
            Shader.SetGlobalFloat(Properties.MainShadowLimit, _globalMainShadow.value);
            _globalMainHighlight = new SavedFloat(GUIHeaders.GetStateKey(VariantName, GUIHeaders.MainHighlight), DefaultValues.HighlightStepDefault);
            Shader.SetGlobalFloat(Properties.MainHighlightLimit, _globalMainHighlight.value);
            _globalMainEdge = new SavedFloat(GUIHeaders.GetStateKey(VariantName, GUIHeaders.MainEdgeSoftness), DefaultValues.EdgeDefault);
            Shader.SetGlobalFloat(Properties.MainEdgeSoftness, _globalMainEdge.value);

            _globalAddShadow = new SavedFloat(GUIHeaders.GetStateKey(VariantName, GUIHeaders.AdditionalShadow), DefaultValues.ShadowStepDefault);
            Shader.SetGlobalFloat(Properties.AdditionalShadowLimit, _globalAddShadow.value);
            _globalAddHighlight = new SavedFloat(GUIHeaders.GetStateKey(VariantName, GUIHeaders.AdditionalHighlight), DefaultValues.HighlightStepDefault);
            Shader.SetGlobalFloat(Properties.AdditionalHighlightLimit, _globalAddHighlight.value);
            _globalAddEdge = new SavedFloat(GUIHeaders.GetStateKey(VariantName, GUIHeaders.AdditionalEdgeSoftness), DefaultValues.EdgeDefault);
            Shader.SetGlobalFloat(Properties.AdditionalEdgeSoftness, _globalAddEdge.value);

            _globalHighlight = new SavedFloat(GUIHeaders.GetStateKey(VariantName, GUIHeaders.Highlight), DefaultValues.HighlightDefault);
            Shader.SetGlobalFloat(Properties.HighlightValue, _globalHighlight.value);
            _globalMidtone = new SavedFloat(GUIHeaders.GetStateKey(VariantName, GUIHeaders.Midtone), DefaultValues.MidtoneDefault);
            Shader.SetGlobalFloat(Properties.MidtoneValue, _globalMidtone.value);
            _globalShadow = new SavedFloat(GUIHeaders.GetStateKey(VariantName, GUIHeaders.Shadow), DefaultValues.ShadowDefault);
            Shader.SetGlobalFloat(Properties.ShadowValue, _globalShadow.value);

            base.OnOpenGUI(material, materialEditor);
        }

        public override void DrawLightingOptions()
        {
            EditorGUILayout.LabelField(GUIStyles.MainLightingText);

            EditorGUI.indentLevel += 2;

            EditorGUI.BeginChangeCheck();
            float mShadow = Shader.GetGlobalFloat(Properties.MainShadowLimit);
            mShadow = EditorGUILayout.Slider(
                GUIStyles.ShadowStepText, mShadow, DefaultValues.ShadowStepRangeMin, DefaultValues.ShadowStepRangeMax);
            if (EditorGUI.EndChangeCheck())
            {
                Shader.SetGlobalFloat(Properties.MainShadowLimit, mShadow);
                _globalMainShadow.value = mShadow;
            }

            EditorGUI.BeginChangeCheck();
            float mHighlight = Shader.GetGlobalFloat(Properties.MainHighlightLimit);
            mHighlight = EditorGUILayout.Slider(
                GUIStyles.HighlightStepText, mHighlight, DefaultValues.HighlightStepRangeMin, DefaultValues.HighlightStepRangeMax);
            if (EditorGUI.EndChangeCheck())
            {
                Shader.SetGlobalFloat(Properties.MainHighlightLimit, mHighlight);
                _globalMainHighlight.value = mHighlight;
            }

            EditorGUI.BeginChangeCheck();
            float mEdge = Shader.GetGlobalFloat(Properties.MainEdgeSoftness);
            mEdge = EditorGUILayout.Slider(
                GUIStyles.EdgeSoftnessText, mEdge, DefaultValues.EdgeRangeMin, DefaultValues.EdgeRangeMax);
            if (EditorGUI.EndChangeCheck())
            {
                Shader.SetGlobalFloat(Properties.MainEdgeSoftness, mEdge);
                _globalMainEdge.value = mEdge;
            }

            EditorGUI.indentLevel -= 2;

            EditorGUILayout.LabelField(GUIStyles.AdditionalLightingText);

            EditorGUI.indentLevel += 2;

            EditorGUI.BeginChangeCheck();
            float aShadow = Shader.GetGlobalFloat(Properties.AdditionalShadowLimit);
            aShadow = EditorGUILayout.Slider(
                GUIStyles.ShadowStepText, aShadow, DefaultValues.ShadowStepRangeMin, DefaultValues.ShadowStepRangeMax);
            if (EditorGUI.EndChangeCheck())
            {
                Shader.SetGlobalFloat(Properties.AdditionalShadowLimit, aShadow);
                _globalAddShadow.value = aShadow;
            }

            EditorGUI.BeginChangeCheck();
            float aHighlight = Shader.GetGlobalFloat(Properties.AdditionalHighlightLimit);
            aHighlight = EditorGUILayout.Slider(
                GUIStyles.HighlightStepText, aHighlight, DefaultValues.HighlightStepRangeMin, DefaultValues.HighlightStepRangeMax);
            if (EditorGUI.EndChangeCheck())
            {
                Shader.SetGlobalFloat(Properties.AdditionalHighlightLimit, aHighlight);
                _globalAddHighlight.value = aHighlight;
            }

            EditorGUI.BeginChangeCheck();
            float aEdge = Shader.GetGlobalFloat(Properties.AdditionalEdgeSoftness);
            aEdge = EditorGUILayout.Slider(
                GUIStyles.EdgeSoftnessText, aEdge, DefaultValues.EdgeRangeMin, DefaultValues.EdgeRangeMax);
            if (EditorGUI.EndChangeCheck())
            {
                Shader.SetGlobalFloat(Properties.AdditionalEdgeSoftness, aEdge);
                _globalMainEdge.value = aEdge;
            }

            EditorGUI.indentLevel -= 2;

            EditorGUI.BeginChangeCheck();
            float highlight = Shader.GetGlobalFloat(Properties.HighlightValue);
            highlight = EditorGUILayout.Slider(
                GUIStyles.HighlightText, highlight, DefaultValues.HighlightRangeMin, DefaultValues.HighlightRangeMax);
            if (EditorGUI.EndChangeCheck())
            {
                Shader.SetGlobalFloat(Properties.HighlightValue, highlight);
                _globalHighlight.value = highlight;
            }

            EditorGUI.BeginChangeCheck();
            float midtone = Shader.GetGlobalFloat(Properties.MidtoneValue);
            midtone = EditorGUILayout.Slider(
                GUIStyles.MidToneText, midtone, DefaultValues.MidtoneRangeMin, DefaultValues.MidtoneRangeMax);
            if (EditorGUI.EndChangeCheck())
            {
                Shader.SetGlobalFloat(Properties.MidtoneValue, midtone);
                _globalMidtone.value = midtone;
            }

            EditorGUI.BeginChangeCheck();
            float shadow = Shader.GetGlobalFloat(Properties.ShadowValue);
            shadow = EditorGUILayout.Slider(
                GUIStyles.ShadowText, shadow, DefaultValues.ShadowRangeMin, DefaultValues.ShadowRangeMax);
            if (EditorGUI.EndChangeCheck())
            {
                Shader.SetGlobalFloat(Properties.ShadowValue, shadow);
                _globalShadow.value = shadow;
            }

            base.DrawLightingOptions();
        }
    }    
}
