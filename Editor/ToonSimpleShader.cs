using System;
using UnityEngine;
using UnityEditor.ToonShaderGUI;
using ToonShader;

namespace UnityEditor.Rendering.Universal.ShaderGUI
{
    internal class ToonSimpleShader : BaseShaderGUI
    {
        // Save editor flags

        private const string k_KeyPrefix = "UniversalRP:Material:UI_State:";
        private string m_HeaderStateKey = null;

        SavedBool m_advancedLightingFoldout;
        SavedBool m_SurfaceOptionsFoldout;
        SavedBool m_SurfaceInputsFoldout;
        SavedBool m_AdvancedFoldout;

        SavedString m_globalAmbient;

        SavedFloat m_globalMainShadow;
        SavedFloat m_globalMainHighlight;

        SavedFloat m_globalAddShadow;
        SavedFloat m_globalAddHighlight;

        SavedFloat m_globalMidtone;
        SavedFloat m_globalEdge;        

        // Properties
        private SimpleToonGUI.ToonSimpleProperties toonStandardProperties;        


        // collect properties from the material properties
        public override void FindProperties(MaterialProperty[] properties)
        {            
            cullingProp = FindProperty("_Cull", properties);
            cullingProp = FindProperty("_Cull", properties);
            alphaClipProp = FindProperty("_AlphaClip", properties);
            alphaCutoffProp = FindProperty("_Cutoff", properties);
            receiveShadowsProp = FindProperty("_ReceiveShadows", properties, false);
            baseMapProp = FindProperty("_BaseMap", properties, false);
            baseColorProp = FindProperty("_BaseColor", properties, false);
            emissionMapProp = FindProperty("_EmissionMap", properties, false);
            emissionColorProp = FindProperty("_EmissionColor", properties, false);
            queueOffsetProp = FindProperty("_QueueOffset", properties, false);
            toonStandardProperties = new SimpleToonGUI.ToonSimpleProperties(properties);
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

        public override void OnOpenGUI(Material material, MaterialEditor materialEditor)
        {
            // Foldout states
            m_HeaderStateKey = k_KeyPrefix + material.shader.name; // Create key string for editor prefs
            m_advancedLightingFoldout = new SavedBool($"{m_HeaderStateKey}.AdvancedLightingFoldout", true);
            m_SurfaceOptionsFoldout = new SavedBool($"{m_HeaderStateKey}.SurfaceOptionsFoldout", true);
            m_SurfaceInputsFoldout = new SavedBool($"{m_HeaderStateKey}.SurfaceInputsFoldout", true);
            m_AdvancedFoldout = new SavedBool($"{m_HeaderStateKey}.AdvancedFoldout", false);

            m_globalAmbient = new SavedString(ToonMasterValues.uiKeyPrefix + ToonMasterValues.ambientHeader, SavedString.GetDefaultColorString());
            Shader.SetGlobalColor("_AmbientColor", m_globalAmbient.GetValueAsColor());

            m_globalMainShadow = new SavedFloat(ToonMasterValues.uiKeyPrefix + ToonMasterValues.sufaceShadowHeader, ToonMasterValues.sShadowDefault);
            Shader.SetGlobalFloat("_SurfaceShadowLimit", m_globalMainShadow.value);
            m_globalMainHighlight = new SavedFloat(ToonMasterValues.uiKeyPrefix + ToonMasterValues.sufaceHighlightHeader, ToonMasterValues.sHighlightDefault);
            Shader.SetGlobalFloat("_SurfaceHighlightLimit", m_globalMainHighlight.value);

            m_globalAddShadow = new SavedFloat(ToonMasterValues.uiKeyPrefix + ToonMasterValues.attenuationShadowHeader, ToonMasterValues.aShadowDefault);
            Shader.SetGlobalFloat("_AttenuationShadowLimit", m_globalAddShadow.value);
            m_globalAddHighlight = new SavedFloat(ToonMasterValues.uiKeyPrefix + ToonMasterValues.attenuationHighlightHeader, ToonMasterValues.aHighlightDefault);
            Shader.SetGlobalFloat("_AttenuationHighlightLimit", m_globalAddHighlight.value);

            m_globalMidtone = new SavedFloat(ToonMasterValues.uiKeyPrefix + ToonMasterValues.midToneHeader, ToonMasterValues.midtoneDefault);
            Shader.SetGlobalFloat("_MidtoneValue", m_globalMidtone.value);
            m_globalEdge = new SavedFloat(ToonMasterValues.uiKeyPrefix + ToonMasterValues.edgeSoftnessHeader, ToonMasterValues.edgeDefault);
            Shader.SetGlobalFloat("_EdgeSoftness", m_globalEdge.value);

            foreach (var obj in materialEditor.targets)
                MaterialChanged((Material)obj);
        }

        new public void ShaderPropertiesGUI(Material material)
        {
            if (material == null)
                throw new ArgumentNullException("material");

            EditorGUI.BeginChangeCheck();

            m_SurfaceOptionsFoldout.value = EditorGUILayout.BeginFoldoutHeaderGroup(m_SurfaceOptionsFoldout.value, Styles.SurfaceOptions);
            if (m_SurfaceOptionsFoldout.value)
            {
                DrawSurfaceOptions(material);
                EditorGUILayout.Space();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();            

            m_SurfaceInputsFoldout.value = EditorGUILayout.BeginFoldoutHeaderGroup(m_SurfaceInputsFoldout.value, Styles.SurfaceInputs);
            if (m_SurfaceInputsFoldout.value)
            {
                DrawSurfaceInputs(material);
                EditorGUILayout.Space();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            m_advancedLightingFoldout.value = EditorGUILayout.BeginFoldoutHeaderGroup(m_advancedLightingFoldout.value, ToonMasterGUI.Styles.AdvLighting);
            if (m_advancedLightingFoldout.value)
            {
                DrawLightingOptions();
                EditorGUILayout.Space();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            m_AdvancedFoldout.value = EditorGUILayout.BeginFoldoutHeaderGroup(m_AdvancedFoldout.value, Styles.AdvancedLabel);
            if (m_AdvancedFoldout.value)
            {
                DrawAdvancedOptions(material);
                EditorGUILayout.Space();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            DrawAdditionalFoldouts(material);

            if (EditorGUI.EndChangeCheck())
            {
                foreach (var obj in materialEditor.targets)
                    MaterialChanged((Material)obj);
            }
        }

        // material changed check
        public override void MaterialChanged(Material material)
        {
            if (material == null)
                throw new ArgumentNullException("material");

            SimpleToonGUI.SetMaterialKeywords(material);
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
        }

        // material main surface inputs
        public override void DrawSurfaceInputs(Material material)
        {
            //DrawLightingOptions();
            base.DrawSurfaceInputs(material);
            DrawMaskMapArea(toonStandardProperties, materialEditor, material);
            DrawSpecularArea(toonStandardProperties, materialEditor, material);
            DrawNormalArea(materialEditor, toonStandardProperties.bumpMapProp, toonStandardProperties.bumpScaleProp);
            DrawBacklightArea(toonStandardProperties, materialEditor, material);
            DrawEdgeShineArea(toonStandardProperties, materialEditor, material);
            DrawOcclusionArea(materialEditor, toonStandardProperties.maskMap, toonStandardProperties.occlusionStrength);
            DrawEmissionProperties(material, true);
            DrawTileOffset(materialEditor, baseMapProp);
        }

        // material main advanced options
        public override void DrawAdvancedOptions(Material material)
        {
            base.DrawAdvancedOptions(material);
        }

        public void DrawLightingOptions()
        {
            Color ambientColor = Shader.GetGlobalColor("_AmbientColor");
            EditorGUI.BeginChangeCheck();
            ambientColor = ToonMasterGUI.DrawColorField(ambientColor, ToonMasterGUI.Styles.ambientColorText, true, false, false);            

            if (EditorGUI.EndChangeCheck())
            {
                Shader.SetGlobalColor("_AmbientColor", ambientColor);
                m_globalAmbient.SetValueAsColor(ambientColor);
            }

            EditorGUILayout.LabelField(ToonMasterGUI.Styles.mainLightingText);

            EditorGUI.indentLevel += 2;

            EditorGUI.BeginChangeCheck();
            float mShadow = Shader.GetGlobalFloat("_SurfaceShadowLimit");
            mShadow = EditorGUILayout.Slider(
                ToonMasterGUI.Styles.mainShadowText, mShadow, ToonMasterValues.sShadowRangeMin, ToonMasterValues.sShadowRangeMax);
            if (EditorGUI.EndChangeCheck())
            {
                Shader.SetGlobalFloat("_SurfaceShadowLimit", mShadow);
                m_globalMainShadow.value = mShadow;
            }

            EditorGUI.BeginChangeCheck();
            float mHighlight = Shader.GetGlobalFloat("_SurfaceHighlightLimit");
            mHighlight = EditorGUILayout.Slider(
                ToonMasterGUI.Styles.mainLightEdgeText, mHighlight, ToonMasterValues.sHighlightRangeMin, ToonMasterValues.sHighlightRangeMax);
            if (EditorGUI.EndChangeCheck())
            {
                Shader.SetGlobalFloat("_SurfaceHighlightLimit", mHighlight);
                m_globalMainHighlight.value = mHighlight;
            }

            EditorGUI.indentLevel -= 2;

            EditorGUILayout.LabelField(ToonMasterGUI.Styles.additionalLightingText);

            EditorGUI.indentLevel += 2;

            EditorGUI.BeginChangeCheck();
            float aShadow = Shader.GetGlobalFloat("_AttenuationShadowLimit");
            aShadow = EditorGUILayout.Slider(
                ToonMasterGUI.Styles.addShadowText, aShadow, ToonMasterValues.aShadowRangeMin, ToonMasterValues.aShadowRangeMax);
            if (EditorGUI.EndChangeCheck())
            {
                Shader.SetGlobalFloat("_AttenuationShadowLimit", aShadow);
                m_globalAddShadow.value = aShadow;
            }

            EditorGUI.BeginChangeCheck();
            float aHighlight = Shader.GetGlobalFloat("_AttenuationHighlightLimit");
            aHighlight = EditorGUILayout.Slider(
                ToonMasterGUI.Styles.addLightEdgeText, aHighlight, ToonMasterValues.aHighlightRangeMin, ToonMasterValues.aHighlightRangeMax);
            if (EditorGUI.EndChangeCheck())
            {
                Shader.SetGlobalFloat("_AttenuationHighlightLimit", aHighlight);
                m_globalAddHighlight.value = aHighlight;
            }

            EditorGUI.indentLevel -= 2;

            EditorGUI.BeginChangeCheck();
            float midtone = Shader.GetGlobalFloat("_MidtoneValue");
            midtone = EditorGUILayout.Slider(
                ToonMasterGUI.Styles.midToneText, midtone, ToonMasterValues.midtoneRangeMin, ToonMasterValues.midtoneRangeMax);
            if (EditorGUI.EndChangeCheck())
            {
                Shader.SetGlobalFloat("_MidtoneValue", midtone);
                m_globalMidtone.value = midtone;
            }

            EditorGUI.BeginChangeCheck();
            float edge = Shader.GetGlobalFloat("_EdgeSoftness");
            edge = EditorGUILayout.Slider(
                ToonMasterGUI.Styles.edgeSoftnessText, edge, ToonMasterValues.edgeRangeMin, ToonMasterValues.edgeRangeMax);
            if (EditorGUI.EndChangeCheck())
            {
                Shader.SetGlobalFloat("_EdgeSoftness", edge);
                m_globalEdge.value = edge;
            }

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

        public void DrawBacklightArea(SimpleToonGUI.ToonSimpleProperties properties, MaterialEditor materialEditor, Material material)
        {
            bool backlightEnabled = alphaClipProp.floatValue == 0.0f;

            if (backlightEnabled)
            {
                EditorGUI.BeginChangeCheck();
                materialEditor.ShaderProperty(toonStandardProperties.backlight, ToonMasterGUI.Styles.backlightText);
                if (EditorGUI.EndChangeCheck())
                {
                    MaterialChanged(material);
                }

                EditorGUI.BeginDisabledGroup(properties.backlight.floatValue == 0.0f);
                {
                    EditorGUI.indentLevel += 2;
                    EditorGUI.BeginChangeCheck();
                    float backlightStrength = EditorGUILayout.Slider(
                        ToonMasterGUI.Styles.backlightStrengthText, properties.backlightStrength.floatValue,
                        properties.backlightStrength.rangeLimits.x, properties.backlightStrength.rangeLimits.y);
                    if (EditorGUI.EndChangeCheck())
                    {
                        properties.backlightStrength.floatValue = backlightStrength;
                    }
                    EditorGUI.indentLevel -= 2;
                }
                EditorGUI.EndDisabledGroup();
            }
        }

        public void DrawEdgeShineArea(SimpleToonGUI.ToonSimpleProperties properties, MaterialEditor materialEditor, Material material)
        {
            bool edgeShineEnabled = alphaClipProp.floatValue == 0.0f;

            if (edgeShineEnabled)
            {
                EditorGUI.BeginChangeCheck();
                materialEditor.ShaderProperty(properties.edgeShine, ToonMasterGUI.Styles.edgeShineText);
                if (EditorGUI.EndChangeCheck())
                {
                    MaterialChanged(material);
                }

                EditorGUI.BeginDisabledGroup(properties.edgeShine.floatValue == 0.0f);
                {
                    Color edgeColor = properties.edgeShineColor.colorValue;
                    EditorGUI.indentLevel += 2;
                    EditorGUI.BeginChangeCheck();
                    edgeColor = ToonMasterGUI.DrawColorField(edgeColor, ToonMasterGUI.Styles.edgeShineColorText, true, false, false);
                    if (EditorGUI.EndChangeCheck())
                    {
                        properties.edgeShineColor.colorValue = edgeColor;
                    }
                    EditorGUI.indentLevel -= 2;
                }
                EditorGUI.EndDisabledGroup();
            }
        }

        public void DrawOcclusionArea(MaterialEditor editor, MaterialProperty maskMap, MaterialProperty occlusionStrength)
        {
            if (maskMap != null && maskMap.textureValue != null)
            {
                EditorGUI.BeginChangeCheck();
                EditorGUI.showMixedValue = occlusionStrength.hasMixedValue;
                float oS = EditorGUILayout.Slider(
                    ToonMasterGUI.Styles.occlusionStrengthText, occlusionStrength.floatValue, occlusionStrength.rangeLimits.x, occlusionStrength.rangeLimits.y);
                if (EditorGUI.EndChangeCheck())
                {
                    occlusionStrength.floatValue = oS;
                }
                EditorGUI.showMixedValue = false;
            }            
        }

        public void DrawMaskMapArea(SimpleToonGUI.ToonSimpleProperties properties, MaterialEditor materialEditor, Material material)
        {
            if (properties.maskMap != null)
            {
                materialEditor.TexturePropertySingleLine(ToonMasterGUI.Styles.maskMapText, properties.maskMap);

                if(properties.maskMap.textureValue != null)
                {
                    EditorGUI.indentLevel += 2;
                    EditorGUI.BeginChangeCheck();
                    materialEditor.ShaderProperty(properties.fillSpecular, ToonMasterGUI.Styles.maskFillSpecular);
                    if (EditorGUI.EndChangeCheck())
                    {
                        MaterialChanged(material);
                    }
                    EditorGUI.BeginChangeCheck();
                    materialEditor.ShaderProperty(properties.fillOcclusion, ToonMasterGUI.Styles.maskFillOcclusion);
                    if (EditorGUI.EndChangeCheck())
                    {
                        MaterialChanged(material);
                    }
                    EditorGUI.BeginChangeCheck();
                    materialEditor.ShaderProperty(properties.fillEmission, ToonMasterGUI.Styles.maskFillEmission);
                    if (EditorGUI.EndChangeCheck())
                    {
                        MaterialChanged(material);
                    }
                    EditorGUI.BeginChangeCheck();
                    materialEditor.ShaderProperty(properties.fillSmoothness, ToonMasterGUI.Styles.maskFillSmoothness);
                    if (EditorGUI.EndChangeCheck())
                    {
                        MaterialChanged(material);
                    }
                    EditorGUI.indentLevel -= 2;
                }
            }
        }

        public void DrawSpecularArea(SimpleToonGUI.ToonSimpleProperties properties, MaterialEditor materialEditor, Material material)
        {
            EditorGUI.BeginChangeCheck();
            materialEditor.ShaderProperty(properties.specular, ToonMasterGUI.Styles.highlightsText);
            if (EditorGUI.EndChangeCheck())
            {
                MaterialChanged(material);
            }

            EditorGUI.BeginDisabledGroup(properties.specular.floatValue == 0.0f);
            {
                EditorGUI.BeginChangeCheck();
                Color specColor = ToonMasterGUI.DrawColorField(properties.specColor.colorValue, ToonMasterGUI.Styles.specColorText, true, false, false);
                if (EditorGUI.EndChangeCheck())
                {
                    properties.specColor.colorValue = specColor;
                }                

                DrawSpecularTextureArea(properties, materialEditor, material);

                EditorGUI.indentLevel += 2;
                EditorGUI.BeginChangeCheck();
                EditorGUI.showMixedValue = properties.smoothness.hasMixedValue;
                float smoothness = EditorGUILayout.Slider(
                    ToonMasterGUI.Styles.smoothnessText, properties.smoothness.floatValue, properties.smoothness.rangeLimits.x, properties.smoothness.rangeLimits.y);
                if (EditorGUI.EndChangeCheck())
                {
                    properties.smoothness.floatValue = smoothness;
                }
                EditorGUI.showMixedValue = false;
                EditorGUI.indentLevel -= 2;
            }
            EditorGUI.EndDisabledGroup();
        }

        public void DrawSpecularTextureArea(SimpleToonGUI.ToonSimpleProperties properties, MaterialEditor materialEditor, Material material)
        {
            //EditorGUI.BeginDisabledGroup(toonStandardProperties.specular.floatValue == 0.0f);
            if (toonStandardProperties.specular.floatValue == 1.0f)
            {
                EditorGUI.indentLevel += 2;
                materialEditor.TexturePropertySingleLine(ToonMasterGUI.Styles.specTex, properties.specTexMap);
                EditorGUI.indentLevel -= 2;

                if (properties.specTexMap.textureValue != null)
                {
                    EditorGUI.indentLevel += 4;
                    EditorGUI.BeginChangeCheck();
                    EditorGUI.showMixedValue = properties.specTexTile.hasMixedValue;
                    float specTexTile = EditorGUILayout.Slider(
                        ToonMasterGUI.Styles.specTexTile, properties.specTexTile.floatValue, properties.specTexTile.rangeLimits.x, properties.specTexTile.rangeLimits.y);
                    if (EditorGUI.EndChangeCheck())
                    {
                        properties.specTexTile.floatValue = specTexTile;
                    }

                    EditorGUI.BeginChangeCheck();
                    EditorGUI.showMixedValue = properties.specTexRot.hasMixedValue;
                    float specTexRot = EditorGUILayout.Slider(
                        ToonMasterGUI.Styles.specTexRot, properties.specTexRot.floatValue, properties.specTexRot.rangeLimits.x, properties.specTexRot.rangeLimits.y);
                    if (EditorGUI.EndChangeCheck())
                    {
                        properties.specTexRot.floatValue = specTexRot;
                    }

                    EditorGUI.showMixedValue = false;
                    EditorGUI.indentLevel -= 4;
                }
            }
            //EditorGUI.EndDisabledGroup();
        }

        protected override void DrawEmissionProperties(Material material, bool keyword)
        {
            var emissive = true;

            if (!keyword)
            {
                EditorGUI.BeginChangeCheck();
                Color emColor = ToonMasterGUI.DrawColorField(emissionColorProp.colorValue, ToonMasterGUI.Styles.emissionColorText, true, false, true);
                if (EditorGUI.EndChangeCheck())
                {
                    emissionColorProp.colorValue = emColor;
                }
            }
            else
            {
                // Emission for GI?
                emissive = materialEditor.EmissionEnabledProperty();

                EditorGUI.BeginDisabledGroup(!emissive);
                {
                    EditorGUI.BeginChangeCheck();
                    Color emColor = ToonMasterGUI.DrawColorField(emissionColorProp.colorValue, ToonMasterGUI.Styles.emissionColorText, true, false, true);
                    if (EditorGUI.EndChangeCheck())
                    {
                        emissionColorProp.colorValue = emColor;
                    }
                }
                EditorGUI.EndDisabledGroup();
            }

            // UniversalRP does not support RealtimeEmissive. We set it to bake emissive and handle the emissive is black right.
            if (emissive)
            {
                var brightness = emissionColorProp.colorValue.maxColorComponent;
                material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.BakedEmissive;
                if (brightness <= 0f)
                    material.globalIlluminationFlags |= MaterialGlobalIlluminationFlags.EmissiveIsBlack;
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
