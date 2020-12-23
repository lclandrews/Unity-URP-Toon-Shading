using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ToonShader;

public class AmbientLightAnimationController : MonoBehaviour
{
    public bool AnimateAmbientLight = false;
    public float AmbientLightAnimSpeed = 5.0F;

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

    void Start()
    {
        _AmbientColor = ToonMasterValues.GetToonAmbientLight();
    }

    private void Update()
    {
        if(AnimateAmbientLight)
        {
            float h, s, v;
            Color.RGBToHSV(AmbientColor, out h, out s, out v);
            h += AmbientLightAnimSpeed * Time.deltaTime;
            AmbientColor = Color.HSVToRGB(h, s, v);
        }        
    }
}
