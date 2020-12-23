using System;
using UnityEngine;
using UnityEditor.ToonShaderGUI;

namespace UnityEditor.Rendering.Universal.ShaderGUI
{
    internal class ToonMasterShader : BaseShaderGUI
    {
        // Properties
        private StandardToonGUI.ToonStandardProperties toonStandardProperties;

        // collect properties from the material properties
        public override void FindProperties(MaterialProperty[] properties)
        {
            cullingProp = FindProperty("_Cull", properties);
            alphaClipProp = FindProperty("_AlphaClip", properties);
            alphaCutoffProp = FindProperty("_Cutoff", properties);
            receiveShadowsProp = FindProperty("_ReceiveShadows", properties, false);
            baseMapProp = FindProperty("_BaseMap", properties, false);
            baseColorProp = FindProperty("_BaseColor", properties, false);
            emissionMapProp = FindProperty("_EmissionMap", properties, false);
            emissionColorProp = FindProperty("_EmissionColor", properties, false);
            queueOffsetProp = FindProperty("_QueueOffset", properties, false);
            toonStandardProperties = new StandardToonGUI.ToonStandardProperties(properties);
        }

        public override void OnGUI(MaterialEditor materialEditorIn, MaterialProperty[] properties)
        {
            if (materialEditorIn == null)
                throw new ArgumentNullException("materialEditorIn");

            FindProperties(properties); // MaterialProperties can be animated so we do not cache them but fetch them every event to ensure animated values are updated correctly
            materialEditor = materialEditorIn;
            Material material = materialEditor.target as Material;

            // Make sure that needed setup (ie keywords/renderqueue) are set up if we're switching some existing
            // material to a universal shader.
            if (m_FirstTimeApply)
            {
                OnOpenGUI(material, materialEditorIn);
                m_FirstTimeApply = false;
            }

            ShaderPropertiesGUI(material);
        }

        // material changed check
        public override void MaterialChanged(Material material)
        {
            if (material == null)
                throw new ArgumentNullException("material");

            StandardToonGUI.SetMaterialKeywords(material);
        }

        // material main surface options
        public override void DrawSurfaceOptions(Material material)
        {
            if (material == null)
                throw new ArgumentNullException("material");

            // Use default labelWidth
            EditorGUIUtility.labelWidth = 0f;

            EditorGUI.BeginChangeCheck();
            EditorGUI.showMixedValue = cullingProp.hasMixedValue;
            var culling = (RenderFace)cullingProp.floatValue;
            culling = (RenderFace)EditorGUILayout.EnumPopup(Styles.cullingText, culling);
            if (EditorGUI.EndChangeCheck())
            {
                materialEditor.RegisterPropertyChangeUndo(Styles.cullingText.text);
                cullingProp.floatValue = (float)culling;
                material.doubleSidedGI = (RenderFace)cullingProp.floatValue != RenderFace.Front;
            }

            EditorGUI.showMixedValue = false;

            EditorGUI.BeginChangeCheck();
            EditorGUI.showMixedValue = alphaClipProp.hasMixedValue;
            var alphaClipEnabled = EditorGUILayout.Toggle(Styles.alphaClipText, alphaClipProp.floatValue == 1);
            if (EditorGUI.EndChangeCheck())
                alphaClipProp.floatValue = alphaClipEnabled ? 1 : 0;
            EditorGUI.showMixedValue = false;

            if (alphaClipProp.floatValue == 1)
                materialEditor.ShaderProperty(alphaCutoffProp, Styles.alphaClipThresholdText, 1);

            if (receiveShadowsProp != null)
            {
                EditorGUI.BeginChangeCheck();
                EditorGUI.showMixedValue = receiveShadowsProp.hasMixedValue;
                var receiveShadows =
                    EditorGUILayout.Toggle(Styles.receiveShadowText, receiveShadowsProp.floatValue == 1.0f);
                if (EditorGUI.EndChangeCheck())
                    receiveShadowsProp.floatValue = receiveShadows ? 1.0f : 0.0f;
                EditorGUI.showMixedValue = false;
            }
        }

        // material main surface inputs
        public override void DrawSurfaceInputs(Material material)
        {
            DrawLightingOptions(material);
            base.DrawSurfaceInputs(material);
            StandardToonGUI.DrawSpecularArea(toonStandardProperties, materialEditor, material, MaterialChanged);
            DrawNormalArea(materialEditor, toonStandardProperties.bumpMapProp, toonStandardProperties.bumpScaleProp);
            StandardToonGUI.DrawBacklightArea(toonStandardProperties, alphaClipProp, materialEditor, material, MaterialChanged);
            StandardToonGUI.DrawEdgeShineArea(toonStandardProperties, alphaClipProp, materialEditor, material, MaterialChanged);
            StandardToonGUI.DrawOcclusionArea(materialEditor, toonStandardProperties.occlusionMap, toonStandardProperties.occlusionStrength);
            DrawEmissionProperties(material, true);
            DrawTileOffset(materialEditor, baseMapProp);
        }

        // material main advanced options
        public override void DrawAdvancedOptions(Material material)
        {
            base.DrawAdvancedOptions(material);
        }

        public void DrawLightingOptions(Material material)
        {
            EditorGUI.BeginChangeCheck();
            Color ambient = ToonMasterGUI.DrawColorField(toonStandardProperties.ambientColorProp.colorValue, ToonMasterGUI.Styles.ambientColorText, true, false, false);
            if (EditorGUI.EndChangeCheck())
            {
                toonStandardProperties.ambientColorProp.colorValue = ambient;
            }

            int mainGradType = material.GetInt(Shader.PropertyToID("_MainGradType"));
            int mainGradColors = material.GetInt(Shader.PropertyToID("_MainGradColors"));
            Vector4[] mainGradArray = material.GetVectorArray(Shader.PropertyToID("_MainGradKeys"));

            Gradient mainGrad = ToonMasterGUI.ShaderValuesToGradient(mainGradType, mainGradColors, mainGradArray);
            EditorGUI.BeginChangeCheck();
            mainGrad = ToonMasterGUI.DrawMiniGradientField(mainGrad, ToonMasterGUI.Styles.mainGradText, false);

            if (EditorGUI.EndChangeCheck())
            {
                ToonMasterGUI.GradientToShaderValues(mainGrad, out mainGradType, out mainGradColors, out mainGradArray);
                material.SetInt(Shader.PropertyToID("_MainGradType"), mainGradType);
                material.SetInt(Shader.PropertyToID("_MainGradColors"), mainGradColors);
                material.SetVectorArray(Shader.PropertyToID("_MainGradKeys"), mainGradArray);
            }

            int addGradType = material.GetInt(Shader.PropertyToID("_AddGradType"));
            int addGradColors = material.GetInt(Shader.PropertyToID("_AddGradColors"));
            Vector4[] addGradArray = material.GetVectorArray(Shader.PropertyToID("_AddGradKeys"));

            Gradient addGrad = ToonMasterGUI.ShaderValuesToGradient(addGradType, addGradColors, addGradArray);
            EditorGUI.BeginChangeCheck();
            addGrad = ToonMasterGUI.DrawMiniGradientField(addGrad, ToonMasterGUI.Styles.addGradText, false);

            if (EditorGUI.EndChangeCheck())
            {
                ToonMasterGUI.GradientToShaderValues(addGrad, out addGradType, out addGradColors, out addGradArray);
                material.SetInt(Shader.PropertyToID("_AddGradType"), addGradType);
                material.SetInt(Shader.PropertyToID("_AddGradColors"), addGradColors);
                material.SetVectorArray(Shader.PropertyToID("_AddGradKeys"), addGradArray);
            }
        }

        public override void AssignNewShaderToMaterial(Material material, Shader oldShader, Shader newShader)
        {
            if (material == null)
                throw new ArgumentNullException("material");

            // _Emission property is lost after assigning Standard shader to the material
            // thus transfer it before assigning the new shader
            if (material.HasProperty("_Emission"))
            {
                material.SetColor("_EmissionColor", material.GetColor("_Emission"));
            }

            base.AssignNewShaderToMaterial(material, oldShader, newShader);

            if (oldShader == null || !oldShader.name.Contains("Legacy Shaders/"))
            {
                //SetupMaterialBlendMode(material);
                return;
            }

            SurfaceType surfaceType = SurfaceType.Opaque;
            BlendMode blendMode = BlendMode.Alpha;
            if (oldShader.name.Contains("/Transparent/Cutout/"))
            {
                surfaceType = SurfaceType.Opaque;
                material.SetFloat("_AlphaClip", 1);
            }
            else if (oldShader.name.Contains("/Transparent/"))
            {
                // NOTE: legacy shaders did not provide physically based transparency
                // therefore Fade mode
                surfaceType = SurfaceType.Transparent;
                blendMode = BlendMode.Alpha;
            }
            material.SetFloat("_Surface", (float)surfaceType);
            material.SetFloat("_Blend", (float)blendMode);

            MaterialChanged(material);
        }
    }    
}
