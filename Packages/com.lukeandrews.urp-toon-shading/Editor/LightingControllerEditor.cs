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

        private SavedFloat _globalHighlight = null;
        private SavedFloat _globalMidtone = null;
        private SavedFloat _globalShadow = null;

        private GUIContent _controllerFromShader = new GUIContent("Set From Shader", "Sets the controllers values to match those in the shader GUI.");
        private GUIContent _shaderFromController = new GUIContent("Set Shader Values", "Sets all shader GUI values to match the current controller.");

        private GUIContent _applyControllerValues = new GUIContent("Apply To Scene", "Sets the current controller values to the scene.");

        void OnEnable()
        {
            _globalMainShadow = new SavedFloat(GUIHeaders.GetStateKey(GUIHeaders.MainShadow), DefaultValues.ShadowStepDefault);
            _globalMainHighlight = new SavedFloat(GUIHeaders.GetStateKey(GUIHeaders.MainHighlight), DefaultValues.HighlightStepDefault);
            _globalMainEdge = new SavedFloat(GUIHeaders.GetStateKey(GUIHeaders.MainEdgeSoftness), DefaultValues.EdgeDefault);

            _globalAddShadow = new SavedFloat(GUIHeaders.GetStateKey(GUIHeaders.AdditionalShadow), DefaultValues.ShadowStepDefault);
            _globalAddHighlight = new SavedFloat(GUIHeaders.GetStateKey(GUIHeaders.AdditionalHighlight), DefaultValues.HighlightStepDefault);
            _globalAddEdge = new SavedFloat(GUIHeaders.GetStateKey(GUIHeaders.AdditionalEdgeSoftness), DefaultValues.EdgeDefault);

            _globalHighlight = new SavedFloat(GUIHeaders.GetStateKey(GUIHeaders.Highlight), DefaultValues.HighlightDefault);
            _globalMidtone = new SavedFloat(GUIHeaders.GetStateKey(GUIHeaders.Midtone), DefaultValues.MidtoneDefault);
            _globalShadow = new SavedFloat(GUIHeaders.GetStateKey(GUIHeaders.Shadow), DefaultValues.ShadowDefault);
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
                        _globalAddShadow.value, _globalAddHighlight.value, _globalAddEdge.value, _globalHighlight.value, _globalMidtone.value,
                        _globalShadow.value);
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
                    _globalHighlight.value = targetController.HighlightValue;
                    _globalMidtone.value = targetController.MidtoneValue;
                    _globalShadow.value = targetController.ShadowValue;
                }

                if (GUILayout.Button(_applyControllerValues))
                {
                    targetController.ApplyControllerSettings();
                }
            }
        }
    }
}