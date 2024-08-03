using UnityEditor;

using UnityEngine;

namespace ToonShading.Editor
{
    [CustomEditor(typeof(LightingController))]
    public class LightingControllerEditor : UnityEditor.Editor
    {
        private SavedFloat _globalMainShadow = null;
        private SavedFloat _globalMainHighlight = null;
        private SavedFloat _globalMainEdge = null;

        private SavedFloat _globalAddShadow = null;
        private SavedFloat _globalAddHighlight = null;
        private SavedFloat _globalAddEdge = null;

        private SavedFloat _globalMidtone = null;

        private GUIContent _controllerFromShader = new GUIContent("Set From Shader", "Sets the controllers values to match those in the shader GUI.");
        private GUIContent _shaderFromController = new GUIContent("Set Shader Values", "Sets all shader GUI values to match the current controller.");

        private GUIContent _applyControllerValues = new GUIContent("Apply To Scene", "Sets the current controller values to the scene.");

        void OnEnable()
        {
            _globalMainShadow = new SavedFloat(GUIHeaders.GetStateKey(GUIHeaders.MainShadow), DefaultValues.ShadowDefault);
            _globalMainHighlight = new SavedFloat(GUIHeaders.GetStateKey(GUIHeaders.MainHighlight), DefaultValues.HighlightDefault);
            _globalMainEdge = new SavedFloat(GUIHeaders.GetStateKey(GUIHeaders.MainEdgeSoftness), DefaultValues.EdgeDefault);

            _globalAddShadow = new SavedFloat(GUIHeaders.GetStateKey(GUIHeaders.AdditionalShadow), DefaultValues.ShadowDefault);
            _globalAddHighlight = new SavedFloat(GUIHeaders.GetStateKey(GUIHeaders.AdditionalHighlight), DefaultValues.HighlightDefault);
            _globalAddEdge = new SavedFloat(GUIHeaders.GetStateKey(GUIHeaders.AdditionalEdgeSoftness), DefaultValues.EdgeDefault);

            _globalMidtone = new SavedFloat(GUIHeaders.GetStateKey(GUIHeaders.MidTone), DefaultValues.MidtoneDefault);            
        }

        public override void OnInspectorGUI()
        {
            LightingController targetController = (LightingController)target;

            DrawDefaultInspector();

            if (targetController != null)
            {
                if (GUILayout.Button(_controllerFromShader))
                {
                    targetController.ApplyControllerSettings(_globalMainShadow.value, _globalMainHighlight.value, _globalMainEdge.value,
                        _globalAddShadow.value, _globalAddHighlight.value, _globalAddEdge.value, _globalMidtone.value);
                }

                if (GUILayout.Button(_shaderFromController))
                {
                    targetController.ApplyControllerSettings();
                    _globalMainShadow.value = targetController.MainShadow;
                    _globalMainHighlight.value = targetController.MainHighlight;
                    _globalAddEdge.value = targetController.MainEdgeSoftness;
                    _globalAddShadow.value = targetController.AdditionalShadow;
                    _globalAddHighlight.value = targetController.AdditionalHighlight;
                    _globalAddEdge.value = targetController.AdditionalEdgeSoftness;
                    _globalMidtone.value = targetController.MidtoneValue;                    
                }

                if (GUILayout.Button(_applyControllerValues))
                {
                    targetController.ApplyControllerSettings();
                }
            }
        }
    }
}