using UnityEngine;

namespace ToonShading
{
    public class ThreeToneLightingController : MonoBehaviour
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
        [Range(DefaultValues.HighlightStepRangeMin, DefaultValues.HighlightStepRangeMax)]
        private float m_MainHighlight = DefaultValues.HighlightStepDefault;
        public float MainHighlight
        {
            get { return _mainHighlight; }
            set
            {
                _mainHighlight = value;
                Shader.SetGlobalFloat(Properties.MainHighlightLimit, _mainHighlight);
            }
        }
        private float _mainHighlight = DefaultValues.HighlightStepDefault;

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
        [Range(DefaultValues.HighlightStepRangeMin, DefaultValues.HighlightStepRangeMax)]
        private float m_AdditionalHighlight = DefaultValues.HighlightStepDefault;
        public float AdditionalHighlight
        {
            get { return _additionalHighlight; }
            set
            {
                _additionalHighlight = value;
                Shader.SetGlobalFloat(Properties.AdditionalHighlightLimit, _additionalHighlight);
            }
        }
        private float _additionalHighlight = DefaultValues.HighlightStepDefault;

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
        [Range(DefaultValues.MidtoneRangeMin, DefaultValues.MidtoneRangeMax)]
        private float m_MidtoneValue = DefaultValues.MidtoneDefault;
        public float MidtoneValue
        {
            get { return _midtoneValue; }
            set
            {
                _midtoneValue = value;
                Shader.SetGlobalFloat(Properties.MidtoneValue, _midtoneValue);
            }
        }
        private float _midtoneValue = DefaultValues.MidtoneDefault;

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

            if (_mainHighlight != m_MainHighlight)
            {
                MainHighlight = m_MainHighlight;
            }

            if (_mainEdgeSoftness != m_MainEdgeSoftness)
            {
                MainEdgeSoftness = m_MainEdgeSoftness;
            }

            if (_additionalShadow != m_AdditionalShadow)
            {
                AdditionalShadow = m_AdditionalShadow;
            }

            if (_additionalHighlight != m_AdditionalHighlight)
            {
                AdditionalHighlight = m_AdditionalHighlight;
            }

            if (_additionalEdgeSoftness != m_AdditionalEdgeSoftness)
            {
                AdditionalEdgeSoftness = m_AdditionalEdgeSoftness;
            }

            if (_highlightValue != m_HighlightValue)
            {
                HighlightValue = m_HighlightValue;
            }

            if (_midtoneValue != m_MidtoneValue)
            {
                MidtoneValue = m_MidtoneValue;
            }

            if (_shadowValue != m_ShadowValue)
            {
                ShadowValue = m_ShadowValue;
            }
        }

        public void ApplyControllerSettings()
        {
            MainShadow = m_MainShadow;
            MainHighlight = m_MainHighlight;
            MainEdgeSoftness = m_MainEdgeSoftness;
            AdditionalShadow = m_AdditionalShadow;
            AdditionalHighlight = m_AdditionalHighlight;
            AdditionalEdgeSoftness = m_AdditionalEdgeSoftness;
            HighlightValue = m_HighlightValue;
            MidtoneValue = m_MidtoneValue;
            ShadowValue = m_ShadowValue;
        }

        public void ApplyControllerSettings(float mainShadow, float mainHighlight, float mainEdgeSoftness, float additionalShadow, float additionalHighlight,
            float additionalEdgeSoftness, float highlight, float midTone, float shadow)
        {
            m_MainShadow = mainShadow;
            m_MainHighlight = mainHighlight;
            m_MainEdgeSoftness = mainEdgeSoftness;
            m_AdditionalShadow = additionalShadow;
            m_AdditionalHighlight = additionalHighlight;
            m_AdditionalEdgeSoftness = additionalEdgeSoftness;
            m_HighlightValue = highlight;
            m_MidtoneValue = midTone;
            m_ShadowValue = shadow;
            ApplyControllerSettings();
        }
    }
}