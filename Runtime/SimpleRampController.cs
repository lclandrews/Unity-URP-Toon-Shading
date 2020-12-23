using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ToonShader;

public class SimpleRampController : MonoBehaviour
{
    [SerializeField]
    private Color m_AmbientColor = Color.black;

    public Color AmbientColor
    {
        get { return _AmbientColor; }
        set
        {
            _AmbientColor = value;
            Shader.SetGlobalColor("_AmbientColor", _AmbientColor);
        }
    }
    private Color _AmbientColor = Color.black;

    [SerializeField]
    [Range(ToonMasterValues.sShadowRangeMin, ToonMasterValues.sShadowRangeMax)]
    private float m_SurfaceShadow = ToonMasterValues.sShadowDefault;

    public float SurfaceShadow
    {
        get { return _SurfaceShadow; }
        set
        {
            _SurfaceShadow = value;
            Shader.SetGlobalFloat("_SurfaceShadowLimit", _SurfaceShadow);
        }
    }
    private float _SurfaceShadow = ToonMasterValues.sShadowDefault;

    [SerializeField]
    [Range(ToonMasterValues.sHighlightRangeMin, ToonMasterValues.sHighlightRangeMax)]
    private float m_SurfaceHighlight = ToonMasterValues.sHighlightDefault;

    public float SurfaceHighlight
    {
        get { return _SurfaceHighlight; }
        set
        {
            _SurfaceHighlight = value;
            Shader.SetGlobalFloat("_SurfaceHighlightLimit", _SurfaceHighlight);
        }
    }
    private float _SurfaceHighlight = ToonMasterValues.sHighlightDefault;

    [SerializeField]
    [Range(ToonMasterValues.aShadowRangeMin, ToonMasterValues.aShadowRangeMax)]
    private float m_AttenuationShadow = ToonMasterValues.aShadowDefault;

    public float AttenuationShadow
    {
        get { return _AttenuationShadow; }
        set
        {
            _AttenuationShadow = value;
            Shader.SetGlobalFloat("_AttenuationShadowLimit", _AttenuationShadow);
        }
    }
    private float _AttenuationShadow = ToonMasterValues.aShadowDefault;

    [SerializeField]
    [Range(ToonMasterValues.aHighlightRangeMin, ToonMasterValues.aHighlightRangeMax)]
    private float m_AttenuationHighlight = ToonMasterValues.aHighlightDefault;

    public float AttenuationHighlight
    {
        get { return _AttenuationHighlight; }
        set
        {
            _AttenuationHighlight = value;
            Shader.SetGlobalFloat("_AttenuationHighlightLimit", _AttenuationHighlight);
        }
    }
    private float _AttenuationHighlight = ToonMasterValues.aHighlightDefault;

    [SerializeField]
    [Range(ToonMasterValues.midtoneRangeMin, ToonMasterValues.midtoneRangeMax)]
    private float m_MidtoneValue = ToonMasterValues.midtoneDefault;

    public float MidtoneValue
    {
        get { return _MidtoneValue; }
        set
        {
            _MidtoneValue = value;
            Shader.SetGlobalFloat("_MidtoneValue", _MidtoneValue);
        }
    }
    private float _MidtoneValue = ToonMasterValues.midtoneDefault;

    [SerializeField]
    [Range(ToonMasterValues.edgeRangeMin, ToonMasterValues.edgeRangeMax)]
    private float m_EdgeSoftness = ToonMasterValues.edgeDefault;

    public float EdgeSoftness
    {
        get { return _EdgeSoftness; }
        set
        {
            _EdgeSoftness = value;
            Shader.SetGlobalFloat("_EdgeSoftness", _EdgeSoftness);
        }
    }
    private float _EdgeSoftness = ToonMasterValues.edgeDefault;    

    void Awake()
    {
        ApplyControllerSettings();
    }

    private void OnValidate()
    {
        if(_AmbientColor != m_AmbientColor)
        {
            AmbientColor = m_AmbientColor; 
        }

        if (_SurfaceShadow != m_SurfaceShadow)
        {
            SurfaceShadow = m_SurfaceShadow;
        }

        if (_SurfaceHighlight != m_SurfaceHighlight)
        {
            SurfaceHighlight = m_SurfaceHighlight;
        }

        if (_AttenuationShadow != m_AttenuationShadow)
        {
            AttenuationShadow = m_AttenuationShadow;
        }

        if (_AttenuationHighlight != m_AttenuationHighlight)
        {
            AttenuationHighlight = m_AttenuationHighlight;
        }

        if (_MidtoneValue != m_MidtoneValue)
        {
            MidtoneValue = m_MidtoneValue;
        }

        if (_EdgeSoftness != m_EdgeSoftness)
        {
            EdgeSoftness = m_EdgeSoftness;
        }
    }

    public void ApplyControllerSettings()
    {
        AmbientColor = m_AmbientColor;
        SurfaceShadow = m_SurfaceShadow;
        SurfaceHighlight = m_SurfaceHighlight;
        AttenuationShadow = m_AttenuationShadow;
        AttenuationHighlight = m_AttenuationHighlight;
        MidtoneValue = m_MidtoneValue;
        EdgeSoftness = m_EdgeSoftness;
    }

    public void ApplyControllerSettings(Color ambient, float surfaceShadow, float surfaceHighlight, float attenuationShadow, float attenuationHighlight,
        float midTone, float edgeSoftness)
    {
        m_AmbientColor = ambient;
        m_SurfaceShadow = surfaceShadow;
        m_SurfaceHighlight = surfaceHighlight;
        m_AttenuationShadow = attenuationShadow;
        m_AttenuationHighlight = attenuationHighlight;
        m_MidtoneValue = midTone;
        m_EdgeSoftness = edgeSoftness;
        ApplyControllerSettings();
    }
}
