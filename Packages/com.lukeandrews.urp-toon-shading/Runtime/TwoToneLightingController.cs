using UnityEngine;

namespace ToonShading
{
    public class TwoToneLightingController : MonoBehaviour
    {
        // -------------------------------------
        // Main Light
        // -------------------------------------
        [SerializeField]
        [Range(DefaultValues.ShadowStepRangeMin, DefaultValues.ShadowStepRangeMax)]
        private float m_MainShadow = DefaultValues.ShadowStepDefault;
        public float MainShadow
        {
            get { return _mainShadow; }
            set
            {
                _mainShadow = value;
                Shader.SetGlobalFloat(Properties.MainShadowLimit, _mainShadow);
            }
        }
        private float _mainShadow = DefaultValues.ShadowStepDefault;

        [SerializeField]
        [Range(DefaultValues.EdgeRangeMin, DefaultValues.EdgeRangeMax)]
        private float m_MainEdgeSoftness = DefaultValues.EdgeDefault;
        public float MainEdgeSoftness
        {
            get { return _mainEdgeSoftness; }
            set
            {
                _mainEdgeSoftness = value;
                Shader.SetGlobalFloat(Properties.MainEdgeSoftness, _mainEdgeSoftness);
            }
        }
        private float _mainEdgeSoftness = DefaultValues.EdgeDefault;

        // -------------------------------------
        // Additional Light
        // -------------------------------------
        [SerializeField]
        [Range(DefaultValues.ShadowStepRangeMin, DefaultValues.ShadowStepRangeMax)]
        private float m_AdditionalShadow = DefaultValues.ShadowStepDefault;
        public float AdditionalShadow
        {
            get { return _additionalShadow; }
            set
            {
                _additionalShadow = value;
                Shader.SetGlobalFloat(Properties.AdditionalShadowLimit, _additionalShadow);
            }
        }
        private float _additionalShadow = DefaultValues.ShadowStepDefault;

        [SerializeField]
        [Range(DefaultValues.EdgeRangeMin, DefaultValues.EdgeRangeMax)]
        private float m_AdditionalEdgeSoftness = DefaultValues.EdgeDefault;
        public float AdditionalEdgeSoftness
        {
            get { return _additionalEdgeSoftness; }
            set
            {
                _additionalEdgeSoftness = value;
                Shader.SetGlobalFloat(Properties.AdditionalEdgeSoftness, _additionalEdgeSoftness);
            }
        }
        private float _additionalEdgeSoftness = DefaultValues.EdgeDefault;

        // -------------------------------------
        // Highlights, Midtone & Shadows
        // -------------------------------------
        [SerializeField]
        [Range(DefaultValues.HighlightRangeMin, DefaultValues.HighlightRangeMax)]
        private float m_HighlightValue = DefaultValues.HighlightDefault;
        public float HighlightValue
        {
            get { return _highlightValue; }
            set
            {
                _highlightValue = value;
                Shader.SetGlobalFloat(Properties.HighlightValue, _highlightValue);
            }
        }
        private float _highlightValue = DefaultValues.HighlightDefault;

        [SerializeField]
        [Range(DefaultValues.ShadowRangeMin, DefaultValues.ShadowRangeMax)]
        private float m_ShadowValue = DefaultValues.ShadowDefault;
        public float ShadowValue
        {
            get { return _shadowValue; }
            set
            {
                _shadowValue = value;
                Shader.SetGlobalFloat(Properties.ShadowValue, _shadowValue);
            }
        }
        private float _shadowValue = DefaultValues.ShadowDefault;

        void Awake()
        {
            ApplyControllerSettings();
        }

        private void OnValidate()
        {
            if (_mainShadow != m_MainShadow)
            {
                MainShadow = m_MainShadow;
            }

            if (_mainEdgeSoftness != m_MainEdgeSoftness)
            {
                MainEdgeSoftness = m_MainEdgeSoftness;
            }

            if (_additionalShadow != m_AdditionalShadow)
            {
                AdditionalShadow = m_AdditionalShadow;
            }

            if (_additionalEdgeSoftness != m_AdditionalEdgeSoftness)
            {
                AdditionalEdgeSoftness = m_AdditionalEdgeSoftness;
            }

            if (_highlightValue != m_HighlightValue)
            {
                HighlightValue = m_HighlightValue;
            }

            if (_shadowValue != m_ShadowValue)
            {
                ShadowValue = m_ShadowValue;
            }
        }

        public void ApplyControllerSettings()
        {
            MainShadow = m_MainShadow;
            MainEdgeSoftness = m_MainEdgeSoftness;
            AdditionalShadow = m_AdditionalShadow;
            AdditionalEdgeSoftness = m_AdditionalEdgeSoftness;
            HighlightValue = m_HighlightValue;
            ShadowValue = m_ShadowValue;
        }

        public void ApplyControllerSettings(float mainShadow, float mainEdgeSoftness, float additionalShadow, float additionalEdgeSoftness, 
            float highlight, float shadow)
        {
            m_MainShadow = mainShadow;
            m_MainEdgeSoftness = mainEdgeSoftness;
            m_AdditionalShadow = additionalShadow;
            m_AdditionalEdgeSoftness = additionalEdgeSoftness;
            m_HighlightValue = highlight;
            m_ShadowValue = shadow;
            ApplyControllerSettings();
        }
    }
}