using UnityEngine;

namespace ToonShading
{
    public class LightingController : MonoBehaviour
    {
        // -------------------------------------
        // Main Light
        // -------------------------------------
        [SerializeField]
        [Range(DefaultValues.ShadowRangeMin, DefaultValues.ShadowRangeMax)]
        private float m_MainShadow = DefaultValues.ShadowDefault;
        public float MainShadow
        {
            get { return _mainShadow; }
            set
            {
                _mainShadow = value;
                Shader.SetGlobalFloat(Properties.MainShadowLimit, _mainShadow);
            }
        }
        private float _mainShadow = DefaultValues.ShadowDefault;

        [SerializeField]
        [Range(DefaultValues.HighlightRangeMin, DefaultValues.HighlightRangeMax)]
        private float m_MainHighlight = DefaultValues.HighlightDefault;
        public float MainHighlight
        {
            get { return _mainHighlight; }
            set
            {
                _mainHighlight = value;
                Shader.SetGlobalFloat(Properties.MainHighlightLimit, _mainHighlight);
            }
        }
        private float _mainHighlight = DefaultValues.HighlightDefault;

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
        [Range(DefaultValues.ShadowRangeMin, DefaultValues.ShadowRangeMax)]
        private float m_AdditionalShadow = DefaultValues.ShadowDefault;
        public float AdditionalShadow
        {
            get { return _additionalShadow; }
            set
            {
                _additionalShadow = value;
                Shader.SetGlobalFloat(Properties.AdditionalShadowLimit, _additionalShadow);
            }
        }
        private float _additionalShadow = DefaultValues.ShadowDefault;

        [SerializeField]
        [Range(DefaultValues.HighlightRangeMin, DefaultValues.HighlightRangeMax)]
        private float m_AdditionalHighlight = DefaultValues.HighlightDefault;
        public float AdditionalHighlight
        {
            get { return _additionalHighlight; }
            set
            {
                _additionalHighlight = value;
                Shader.SetGlobalFloat(Properties.AdditionalHighlightLimit, _additionalHighlight);
            }
        }
        private float _additionalHighlight = DefaultValues.HighlightDefault;

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
        // Midtone
        // -------------------------------------
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

        void Awake()
        {
            ApplyControllerSettings();
        }

        private void OnValidate()
        {
            if (_mainShadow != m_MainShadow)
            {
                MainShadow = _mainShadow;
            }

            if (_mainHighlight != m_MainHighlight)
            {
                MainHighlight = _mainHighlight;
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

            if (_midtoneValue != m_MidtoneValue)
            {
                MidtoneValue = m_MidtoneValue;
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
            MidtoneValue = m_MidtoneValue;
        }

        public void ApplyControllerSettings(float mainShadow, float mainHighlight, float mainEdgeSoftness, float additionalShadow, float additionalHighlight,
            float additionalEdgeSoftness, float midTone)
        {
            m_MainShadow = mainShadow;
            m_MainHighlight = mainHighlight;
            m_MainEdgeSoftness = mainEdgeSoftness;
            m_AdditionalShadow = additionalShadow;
            m_AdditionalHighlight = additionalHighlight;
            m_AdditionalEdgeSoftness = additionalEdgeSoftness;
            m_MidtoneValue = midTone;
            ApplyControllerSettings();
        }
    }
}