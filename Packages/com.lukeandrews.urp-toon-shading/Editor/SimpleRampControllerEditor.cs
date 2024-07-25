using ToonShader;

using UnityEditor;
using UnityEditor.ToonShaderGUI;

using UnityEngine;

[CustomEditor(typeof(SimpleRampController))]
public class SimpleRampControllerEditor : Editor
{
    SavedString m_globalAmbient;

    SavedFloat m_globalMainShadow;
    SavedFloat m_globalMainHighlight;

    SavedFloat m_globalAddShadow;
    SavedFloat m_globalAddHighlight;

    SavedFloat m_globalMidtone;
    SavedFloat m_globalEdge;

    GUIContent controllerFromShader = new GUIContent("Set From Shader", "Sets the controllers values to match those in the shader GUI.");
    GUIContent shaderFromController = new GUIContent("Set Shader Values", "Sets all shader GUI values to match the current controller.");

    GUIContent applyControllerValues = new GUIContent("Apply To Scene", "Sets the current controller values to the scene.");

    void OnEnable()
    {
        m_globalAmbient = new SavedString(ToonMainValues.uiKeyPrefix + ToonMainValues.ambientHeader, SavedString.GetDefaultColorString());

        m_globalMainShadow = new SavedFloat(ToonMainValues.uiKeyPrefix + ToonMainValues.sufaceShadowHeader, ToonMainValues.sShadowDefault);
        m_globalMainHighlight = new SavedFloat(ToonMainValues.uiKeyPrefix + ToonMainValues.sufaceHighlightHeader, ToonMainValues.sHighlightDefault);

        m_globalAddShadow = new SavedFloat(ToonMainValues.uiKeyPrefix + ToonMainValues.attenuationShadowHeader, ToonMainValues.aShadowDefault);
        m_globalAddHighlight = new SavedFloat(ToonMainValues.uiKeyPrefix + ToonMainValues.attenuationHighlightHeader, ToonMainValues.aHighlightDefault);

        m_globalMidtone = new SavedFloat(ToonMainValues.uiKeyPrefix + ToonMainValues.midToneHeader, ToonMainValues.midtoneDefault);
        m_globalEdge = new SavedFloat(ToonMainValues.uiKeyPrefix + ToonMainValues.edgeSoftnessHeader, ToonMainValues.edgeDefault);
    }

    public override void OnInspectorGUI()
    {
        SimpleRampController targetController = (SimpleRampController)target;

        DrawDefaultInspector();

        if (targetController != null)
        {
            if (GUILayout.Button(controllerFromShader))
            {
                targetController.ApplyControllerSettings(m_globalAmbient.GetValueAsColor(), m_globalMainShadow.value, m_globalMainHighlight.value, m_globalAddShadow.value,
                    m_globalAddHighlight.value, m_globalMidtone.value, m_globalEdge.value);
            }

            if (GUILayout.Button(shaderFromController))
            {
                targetController.ApplyControllerSettings();
                m_globalAmbient.SetValueAsColor(targetController.AmbientColor);
                m_globalMainShadow.value = targetController.SurfaceShadow;
                m_globalMainHighlight.value = targetController.SurfaceHighlight;
                m_globalAddShadow.value = targetController.AttenuationShadow;
                m_globalAddHighlight.value = targetController.AttenuationHighlight;
                m_globalMidtone.value = targetController.MidtoneValue;
                m_globalEdge.value = targetController.EdgeSoftness;
            }

            if (GUILayout.Button(applyControllerValues))
            {
                targetController.ApplyControllerSettings();
            }
        }        
    }
}
