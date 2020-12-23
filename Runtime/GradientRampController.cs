using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GradientRampController : MonoBehaviour
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
    private Gradient m_MainLightRamp = new Gradient();

    public Gradient MainLightRamp
    {
        get { return _MainLightRamp; }
        set
        {
            _MainLightRamp = value;
            int mainGradType = 0;
            int mainGradColors = 0;
            Vector4[] mainGradArray = new Vector4[8];
            GradientToShaderValues(_MainLightRamp, out mainGradType, out mainGradColors, out mainGradArray);
            Shader.SetGlobalInt("_MainGradType", mainGradType);
            Shader.SetGlobalInt("_MainGradColors", mainGradColors);
            Shader.SetGlobalVectorArray("_MainGradKeys", mainGradArray);
        }
    }
    private Gradient _MainLightRamp = new Gradient();

    [SerializeField]
    private Gradient m_AdditionalLightRamp = new Gradient();

    public Gradient AdditionalLightRamp
    {
        get { return _AdditionalLightRamp; }
        set
        {
            _AdditionalLightRamp = value;
            int addGradType = 0;
            int addGradColors = 0;
            Vector4[] addGradArray = new Vector4[8];
            GradientToShaderValues(_AdditionalLightRamp, out addGradType, out addGradColors, out addGradArray);
            Shader.SetGlobalInt("_AddGradType", addGradType);
            Shader.SetGlobalInt("_AddGradColors", addGradColors);
            Shader.SetGlobalVectorArray("_AddGradKeys", addGradArray);
        }
    }
    private Gradient _AdditionalLightRamp = new Gradient();

    void Awake()
    {
        AmbientColor = m_AmbientColor;
        MainLightRamp = m_MainLightRamp;
        AdditionalLightRamp = m_AdditionalLightRamp;
    }

    private void OnValidate()
    {
        if(_AmbientColor != m_AmbientColor)
        {
            AmbientColor = m_AmbientColor; 
        }

        MainLightRamp = m_MainLightRamp;
        AdditionalLightRamp = m_AdditionalLightRamp;
    }

    private void GradientToShaderValues(Gradient grad, out int type, out int length, out Vector4[] keys)
    {
        type = grad.mode == GradientMode.Blend ? 0 : 1;
        length = grad.colorKeys.Length;
        keys = new Vector4[8];
        Vector4 v = new Vector4();
        for (int i = 0; i < length; i++)
        {
            if (i <= 7)
            {
                v.x = grad.colorKeys[i].color.r;
                v.y = grad.colorKeys[i].color.g;
                v.z = grad.colorKeys[i].color.b;
                v.w = grad.colorKeys[i].time;
                keys[i] = v;
            }
        }
    }
}
