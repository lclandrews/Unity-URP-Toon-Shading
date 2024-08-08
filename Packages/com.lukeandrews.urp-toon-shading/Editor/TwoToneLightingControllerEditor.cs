using UnityEditor;

using UnityEngine;

namespace ToonShading.Editor
{
    [CustomEditor(typeof(TwoToneLightingController))]
    public class TwoToneLightingControllerEditor : UnityEditor.Editor
    {
        private SavedFloat _globalMainShadow = null;
        private SavedFloat _globalMainEdge = null;

        private SavedFloat _globalAddShadow = null;
        private SavedFloat _globalAddEdge = null;

        private SavedFloat _globalHighlight = null;
        private SavedFloat _globalShadow = null;

        private GUIContent _controllerFromShader = new GUIContent("Set From Shader", "Sets the controllers values to match those in the shader GUI.");
        private GUIContent _shaderFromController = new GUIContent("Set Shader Values", "Sets all shader GUI values to match the current controller.");

        private GUIContent _applyControllerValues = new GUIContent("Apply To Scene", "Sets the current controller values to the scene.");

        void OnEnable()
        {
            _globalMainShadow = new SavedFloat(GUIHeaders.GetStateKey("TwoTone", GUIHeaders.MainShadow), DefaultValues.ShadowStepDefault);
            _globalMainEdge = new SavedFloat(GUIHeaders.GetStateKey("TwoTone", GUIHeaders.MainEdgeSoftness), DefaultValues.EdgeDefault);

            _globalAddShadow = new SavedFloat(GUIHeaders.GetStateKey("TwoTone", GUIHeaders.AdditionalShadow), DefaultValues.ShadowStepDefault);
            _globalAddEdge = new SavedFloat(GUIHeaders.GetStateKey("TwoTone", GUIHeaders.AdditionalEdgeSoftness), DefaultValues.EdgeDefault);

            _globalHighlight = new SavedFloat(GUIHeaders.GetStateKey("TwoTone", GUIHeaders.Highlight), DefaultValues.HighlightDefault);
            _globalShadow = new SavedFloat(GUIHeaders.GetStateKey("TwoTone", GUIHeaders.Shadow), DefaultValues.ShadowDefault);
        }

        public override void OnInspectorGUI()
        {
            TwoToneLightingController targetController = (TwoToneLightingController)target;

            DrawDefaultInspector();

            if (targetController != null)
            {
                if (GUILayout.Button(_controllerFromShader))
                {
                    targetController.ApplyControllerSettings(_globalMainShadow.value, _globalMainEdge.value,
                        _globalAddShadow.value, _globalAddEdge.value, _globalHighlight.value, _globalShadow.value);
                }

                if (GUILayout.Button(_shaderFromController))
                {
                    targetController.ApplyControllerSettings();
                    _globalMainShadow.value = targetController.MainShadow;
                    _globalAddEdge.value = targetController.MainEdgeSoftness;
                    _globalAddShadow.value = targetController.AdditionalShadow;
                    _globalAddEdge.value = targetController.AdditionalEdgeSoftness;
                    _globalHighlight.value = targetController.HighlightValue;
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