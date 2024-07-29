using System;

using ToonShader;

using UnityEditor.ToonShaderGUI;

using UnityEngine;

namespace UnityEditor.Rendering.Universal.ShaderGUI
{
    internal class ToonShader : BaseShaderGUI
    {
        // Save editor flags

        private const string k_KeyPrefix = "UniversalRP:Material:UI_State:";
        private string m_HeaderStateKey = null;

        SavedBool m_advancedLightingFoldout;
        SavedBool m_SurfaceOptionsFoldout;
        SavedBool m_SurfaceInputsFoldout;
        SavedBool m_OutlineFoldout;
        SavedBool m_AdvancedFoldout;

        SavedString m_globalAmbient;

        SavedFloat m_globalMainShadow;
        SavedFloat m_globalMainHighlight;

        SavedFloat m_globalAddShadow;
        SavedFloat m_globalAddHighlight;

        SavedFloat m_globalMidtone;
        SavedFloat m_globalEdge;        

        // Properties
        private ToonGUI.ToonSimpleProperties toonStandardProperties;        


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
            toonStandardProperties = new ToonGUI.ToonSimpleProperties(properties);
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
            m_OutlineFoldout = new SavedBool($"{m_HeaderStateKey}.OutlineFoldout", false);
            m_AdvancedFoldout = new SavedBool($"{m_HeaderStateKey}.AdvancedFoldout", false);

            m_globalAmbient = new SavedString(ToonDefaultValues.uiKeyPrefix + ToonDefaultValues.ambientHeader, SavedString.GetDefaultColorString());
            Shader.SetGlobalColor("_AmbientColor", m_globalAmbient.GetValueAsColor());

            m_globalMainShadow = new SavedFloat(ToonDefaultValues.uiKeyPrefix + ToonDefaultValues.sufaceShadowHeader, ToonDefaultValues.sShadowDefault);
            Shader.SetGlobalFloat("_SurfaceShadowLimit", m_globalMainShadow.value);
            m_globalMainHighlight = new SavedFloat(ToonDefaultValues.uiKeyPrefix + ToonDefaultValues.sufaceHighlightHeader, ToonDefaultValues.sHighlightDefault);
            Shader.SetGlobalFloat("_SurfaceHighlightLimit", m_globalMainHighlight.value);

            m_globalAddShadow = new SavedFloat(ToonDefaultValues.uiKeyPrefix + ToonDefaultValues.attenuationShadowHeader, ToonDefaultValues.aShadowDefault);
            Shader.SetGlobalFloat("_AttenuationShadowLimit", m_globalAddShadow.value);
            m_globalAddHighlight = new SavedFloat(ToonDefaultValues.uiKeyPrefix + ToonDefaultValues.attenuationHighlightHeader, ToonDefaultValues.aHighlightDefault);
            Shader.SetGlobalFloat("_AttenuationHighlightLimit", m_globalAddHighlight.value);

            m_globalMidtone = new SavedFloat(ToonDefaultValues.uiKeyPrefix + ToonDefaultValues.midToneHeader, ToonDefaultValues.midtoneDefault);
            Shader.SetGlobalFloat("_MidtoneValue", m_globalMidtone.value);
            m_globalEdge = new SavedFloat(ToonDefaultValues.uiKeyPrefix + ToonDefaultValues.edgeSoftnessHeader, ToonDefaultValues.edgeDefault);
            Shader.SetGlobalFloat("_EdgeSoftness", m_globalEdge.value);

            foreach (var obj in materialEditor.targets)
                ValidateMaterial((Material)obj);
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

            m_OutlineFoldout.value = EditorGUILayout.BeginFoldoutHeaderGroup(m_OutlineFoldout.value, ToonMainGUI.Styles.OutlineSettingsText);
            if (m_OutlineFoldout.value)
            {
                DrawOutlineArea(toonStandardProperties, materialEditor, material);
                EditorGUILayout.Space();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            m_advancedLightingFoldout.value = EditorGUILayout.BeginFoldoutHeaderGroup(m_advancedLightingFoldout.value, ToonMainGUI.Styles.AdvLighting);
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

            if (EditorGUI.EndChangeCheck())
            {
                foreach (var obj in materialEditor.targets)
                    ValidateMaterial((Material)obj);
            }
        }

        // material changed check
        public override void ValidateMaterial(Material material)
        {
            if (material == null)
                throw new ArgumentNullException("material");

            ToonGUI.SetMaterialKeywordsAndPasses(material);
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
            DrawNormalArea(materialEditor, toonStandardProperties.BumpMapProp, toonStandardProperties.BumpScaleProp);
            DrawBacklightArea(toonStandardProperties, materialEditor, material);
            DrawEdgeShineArea(toonStandardProperties, materialEditor, material);
            DrawOcclusionArea(materialEditor, toonStandardProperties.MaskMap, toonStandardProperties.OcclusionStrength);
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
            ambientColor = ToonMainGUI.DrawColorField(ambientColor, ToonMainGUI.Styles.AmbientColorText, true, false, false);            

            if (EditorGUI.EndChangeCheck())
            {
                Shader.SetGlobalColor("_AmbientColor", ambientColor);
                m_globalAmbient.SetValueAsColor(ambientColor);
            }

            EditorGUILayout.LabelField(ToonMainGUI.Styles.MainLightingText);

            EditorGUI.indentLevel += 2;

            EditorGUI.BeginChangeCheck();
            float mShadow = Shader.GetGlobalFloat("_SurfaceShadowLimit");
            mShadow = EditorGUILayout.Slider(
                ToonMainGUI.Styles.MainShadowText, mShadow, ToonDefaultValues.sShadowRangeMin, ToonDefaultValues.sShadowRangeMax);
            if (EditorGUI.EndChangeCheck())
            {
                Shader.SetGlobalFloat("_SurfaceShadowLimit", mShadow);
                m_globalMainShadow.value = mShadow;
            }

            EditorGUI.BeginChangeCheck();
            float mHighlight = Shader.GetGlobalFloat("_SurfaceHighlightLimit");
            mHighlight = EditorGUILayout.Slider(
                ToonMainGUI.Styles.MainLightEdgeText, mHighlight, ToonDefaultValues.sHighlightRangeMin, ToonDefaultValues.sHighlightRangeMax);
            if (EditorGUI.EndChangeCheck())
            {
                Shader.SetGlobalFloat("_SurfaceHighlightLimit", mHighlight);
                m_globalMainHighlight.value = mHighlight;
            }

            EditorGUI.indentLevel -= 2;

            EditorGUILayout.LabelField(ToonMainGUI.Styles.AdditionalLightingText);

            EditorGUI.indentLevel += 2;

            EditorGUI.BeginChangeCheck();
            float aShadow = Shader.GetGlobalFloat("_AttenuationShadowLimit");
            aShadow = EditorGUILayout.Slider(
                ToonMainGUI.Styles.AddShadowText, aShadow, ToonDefaultValues.aShadowRangeMin, ToonDefaultValues.aShadowRangeMax);
            if (EditorGUI.EndChangeCheck())
            {
                Shader.SetGlobalFloat("_AttenuationShadowLimit", aShadow);
                m_globalAddShadow.value = aShadow;
            }

            EditorGUI.BeginChangeCheck();
            float aHighlight = Shader.GetGlobalFloat("_AttenuationHighlightLimit");
            aHighlight = EditorGUILayout.Slider(
                ToonMainGUI.Styles.AddLightEdgeText, aHighlight, ToonDefaultValues.aHighlightRangeMin, ToonDefaultValues.aHighlightRangeMax);
            if (EditorGUI.EndChangeCheck())
            {
                Shader.SetGlobalFloat("_AttenuationHighlightLimit", aHighlight);
                m_globalAddHighlight.value = aHighlight;
            }

            EditorGUI.indentLevel -= 2;

            EditorGUI.BeginChangeCheck();
            float midtone = Shader.GetGlobalFloat("_MidtoneValue");
            midtone = EditorGUILayout.Slider(
                ToonMainGUI.Styles.MidToneText, midtone, ToonDefaultValues.midtoneRangeMin, ToonDefaultValues.midtoneRangeMax);
            if (EditorGUI.EndChangeCheck())
            {
                Shader.SetGlobalFloat("_MidtoneValue", midtone);
                m_globalMidtone.value = midtone;
            }

            EditorGUI.BeginChangeCheck();
            float edge = Shader.GetGlobalFloat("_EdgeSoftness");
            edge = EditorGUILayout.Slider(
                ToonMainGUI.Styles.EdgeSoftnessText, edge, ToonDefaultValues.edgeRangeMin, ToonDefaultValues.edgeRangeMax);
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

        public void DrawBacklightArea(ToonGUI.ToonSimpleProperties properties, MaterialEditor materialEditor, Material material)
        {
            bool backlightEnabled = alphaClipProp.floatValue == 0.0f;

            if (backlightEnabled)
            {
                EditorGUI.BeginChangeCheck();
                materialEditor.ShaderProperty(toonStandardProperties.Backlight, ToonMainGUI.Styles.BacklightText);
                if (EditorGUI.EndChangeCheck())
                {
                    ValidateMaterial(material);
                }

                EditorGUI.BeginDisabledGroup(properties.Backlight.floatValue == 0.0f);
                {
                    EditorGUI.indentLevel += 2;
                    EditorGUI.BeginChangeCheck();
                    float backlightStrength = EditorGUILayout.Slider(
                        ToonMainGUI.Styles.BacklightStrengthText, properties.BacklightStrength.floatValue,
                        properties.BacklightStrength.rangeLimits.x, properties.BacklightStrength.rangeLimits.y);
                    if (EditorGUI.EndChangeCheck())
                    {
                        properties.BacklightStrength.floatValue = backlightStrength;
                    }
                    EditorGUI.indentLevel -= 2;
                }
                EditorGUI.EndDisabledGroup();
            }
        }

        public void DrawEdgeShineArea(ToonGUI.ToonSimpleProperties properties, MaterialEditor materialEditor, Material material)
        {
            bool edgeShineEnabled = alphaClipProp.floatValue == 0.0f;

            if (edgeShineEnabled)
            {
                EditorGUI.BeginChangeCheck();
                materialEditor.ShaderProperty(properties.EdgeShine, ToonMainGUI.Styles.EdgeShineText);
                if (EditorGUI.EndChangeCheck())
                {
                    ValidateMaterial(material);
                }

                EditorGUI.BeginDisabledGroup(properties.EdgeShine.floatValue == 0.0f);
                {
                    Color edgeColor = properties.EdgeShineColor.colorValue;
                    EditorGUI.indentLevel += 2;
                    EditorGUI.BeginChangeCheck();
                    edgeColor = ToonMainGUI.DrawColorField(edgeColor, ToonMainGUI.Styles.EdgeShineColorText, true, false, false);
                    if (EditorGUI.EndChangeCheck())
                    {
                        properties.EdgeShineColor.colorValue = edgeColor;
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
                    ToonMainGUI.Styles.OcclusionStrengthText, occlusionStrength.floatValue, occlusionStrength.rangeLimits.x, occlusionStrength.rangeLimits.y);
                if (EditorGUI.EndChangeCheck())
                {
                    occlusionStrength.floatValue = oS;
                }
                EditorGUI.showMixedValue = false;
            }            
        }

        public void DrawMaskMapArea(ToonGUI.ToonSimpleProperties properties, MaterialEditor materialEditor, Material material)
        {
            if (properties.MaskMap != null)
            {
                materialEditor.TexturePropertySingleLine(ToonMainGUI.Styles.MaskMapText, properties.MaskMap);

                if(properties.MaskMap.textureValue != null)
                {
                    EditorGUI.indentLevel += 2;
                    EditorGUI.BeginChangeCheck();
                    materialEditor.ShaderProperty(properties.FillSpecular, ToonMainGUI.Styles.MaskFillSpecular);
                    if (EditorGUI.EndChangeCheck())
                    {
                        ValidateMaterial(material);
                    }
                    EditorGUI.BeginChangeCheck();
                    materialEditor.ShaderProperty(properties.FillOcclusion, ToonMainGUI.Styles.MaskFillOcclusion);
                    if (EditorGUI.EndChangeCheck())
                    {
                        ValidateMaterial(material);
                    }
                    EditorGUI.BeginChangeCheck();
                    materialEditor.ShaderProperty(properties.FillEmission, ToonMainGUI.Styles.MaskFillEmission);
                    if (EditorGUI.EndChangeCheck())
                    {
                        ValidateMaterial(material);
                    }
                    EditorGUI.BeginChangeCheck();
                    materialEditor.ShaderProperty(properties.FillSmoothness, ToonMainGUI.Styles.MaskFillSmoothness);
                    if (EditorGUI.EndChangeCheck())
                    {
                        ValidateMaterial(material);
                    }
                    EditorGUI.indentLevel -= 2;
                }
            }
        }

        public void DrawSpecularArea(ToonGUI.ToonSimpleProperties properties, MaterialEditor materialEditor, Material material)
        {
            EditorGUI.BeginChangeCheck();
            materialEditor.ShaderProperty(properties.Specular, ToonMainGUI.Styles.HighlightsText);
            if (EditorGUI.EndChangeCheck())
            {
                ValidateMaterial(material);
            }

            EditorGUI.BeginDisabledGroup(properties.Specular.floatValue == 0.0f);
            {
                EditorGUI.BeginChangeCheck();
                Color specColor = ToonMainGUI.DrawColorField(properties.SpecColor.colorValue, ToonMainGUI.Styles.SpecColorText, true, false, false);
                if (EditorGUI.EndChangeCheck())
                {
                    properties.SpecColor.colorValue = specColor;
                }                

                DrawSpecularTextureArea(properties, materialEditor, material);

                EditorGUI.indentLevel += 2;
                EditorGUI.BeginChangeCheck();
                EditorGUI.showMixedValue = properties.Smoothness.hasMixedValue;
                float smoothness = EditorGUILayout.Slider(
                    ToonMainGUI.Styles.SmoothnessText, properties.Smoothness.floatValue, properties.Smoothness.rangeLimits.x, properties.Smoothness.rangeLimits.y);
                if (EditorGUI.EndChangeCheck())
                {
                    properties.Smoothness.floatValue = smoothness;
                }
                EditorGUI.showMixedValue = false;
                EditorGUI.indentLevel -= 2;
            }
            EditorGUI.EndDisabledGroup();
        }

        public void DrawSpecularTextureArea(ToonGUI.ToonSimpleProperties properties, MaterialEditor materialEditor, Material material)
        {
            //EditorGUI.BeginDisabledGroup(toonStandardProperties.specular.floatValue == 0.0f);
            if (toonStandardProperties.Specular.floatValue == 1.0f)
            {
                EditorGUI.indentLevel += 2;
                materialEditor.TexturePropertySingleLine(ToonMainGUI.Styles.SpecTex, properties.SpecTexMap);
                EditorGUI.indentLevel -= 2;

                if (properties.SpecTexMap.textureValue != null)
                {
                    EditorGUI.indentLevel += 4;
                    EditorGUI.BeginChangeCheck();
                    EditorGUI.showMixedValue = properties.SpecTexTile.hasMixedValue;
                    float specTexTile = EditorGUILayout.Slider(
                        ToonMainGUI.Styles.SpecTexTile, properties.SpecTexTile.floatValue, properties.SpecTexTile.rangeLimits.x, properties.SpecTexTile.rangeLimits.y);
                    if (EditorGUI.EndChangeCheck())
                    {
                        properties.SpecTexTile.floatValue = specTexTile;
                    }

                    EditorGUI.BeginChangeCheck();
                    EditorGUI.showMixedValue = properties.SpecTexRot.hasMixedValue;
                    float specTexRot = EditorGUILayout.Slider(
                        ToonMainGUI.Styles.SpecTexRot, properties.SpecTexRot.floatValue, properties.SpecTexRot.rangeLimits.x, properties.SpecTexRot.rangeLimits.y);
                    if (EditorGUI.EndChangeCheck())
                    {
                        properties.SpecTexRot.floatValue = specTexRot;
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
                Color emColor = ToonMainGUI.DrawColorField(emissionColorProp.colorValue, ToonMainGUI.Styles.EmissionColorText, true, false, true);
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
                    Color emColor = ToonMainGUI.DrawColorField(emissionColorProp.colorValue, ToonMainGUI.Styles.EmissionColorText, true, false, true);
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

        public void DrawOutlineArea(ToonGUI.ToonSimpleProperties properties, MaterialEditor materialEditor, Material material)
        {
            EditorGUI.BeginChangeCheck();
            materialEditor.ShaderProperty(properties.Outline, ToonMainGUI.Styles.OutlineText);
            if (EditorGUI.EndChangeCheck())
            {
                ValidateMaterial(material);
            }

            EditorGUI.BeginDisabledGroup(properties.Outline.floatValue == 0.0f);
            {                
                EditorGUI.BeginChangeCheck();
                EditorGUI.showMixedValue = properties.OutlineWidth.hasMixedValue;
                float width = EditorGUILayout.Slider(
                    ToonMainGUI.Styles.OutlineWidthText, properties.OutlineWidth.floatValue, properties.OutlineWidth.rangeLimits.x, properties.OutlineWidth.rangeLimits.y);
                if (EditorGUI.EndChangeCheck())
                {
                    properties.OutlineWidth.floatValue = width;
                }

                EditorGUI.indentLevel += 2;
                materialEditor.TexturePropertySingleLine(ToonMainGUI.Styles.OutlineWidthMapText, properties.OutlineWidthMap);
                EditorGUI.indentLevel -= 2;

                Color outlineColor = ToonMainGUI.DrawColorField(properties.OutlineColor.colorValue, ToonMainGUI.Styles.OutlineColorText, true, false, false);
                if (EditorGUI.EndChangeCheck())
                {
                    properties.OutlineColor.colorValue = outlineColor;
                }

                EditorGUI.BeginChangeCheck();
                materialEditor.ShaderProperty(properties.OutlineFade, ToonMainGUI.Styles.OutlineFadeText);
                if (EditorGUI.EndChangeCheck())
                {
                    ValidateMaterial(material);
                }

                if(properties.OutlineFade.floatValue == 0.0F)
                {
                    EditorGUI.BeginChangeCheck();
                    float cullingDistance = EditorGUILayout.FloatField(ToonMainGUI.Styles.OutlineCullText, properties.OutlineEndFadeDistance.floatValue);
                    if (EditorGUI.EndChangeCheck())
                    {
                        properties.OutlineEndFadeDistance.floatValue = cullingDistance;
                    }
                }
                else
                {
                    EditorGUI.BeginChangeCheck();
                    float startFadeDistance = EditorGUILayout.FloatField(ToonMainGUI.Styles.OutlineFadeStartText, properties.OutlineStartFadeDistance.floatValue);
                    if (EditorGUI.EndChangeCheck())
                    {
                        properties.OutlineStartFadeDistance.floatValue = startFadeDistance;
                    }

                    EditorGUI.BeginChangeCheck();
                    float endFadeDistance = EditorGUILayout.FloatField(ToonMainGUI.Styles.OutlineFadeEndText, properties.OutlineEndFadeDistance.floatValue);
                    if (EditorGUI.EndChangeCheck())
                    {
                        properties.OutlineEndFadeDistance.floatValue = endFadeDistance;
                    }
                }

                EditorGUI.BeginChangeCheck();
                float zOffset = EditorGUILayout.FloatField(ToonMainGUI.Styles.OutlineOffsetZ, properties.OutlineOffsetZ.floatValue);
                if (EditorGUI.EndChangeCheck())
                {
                    properties.OutlineOffsetZ.floatValue = zOffset;
                }                
            }
            EditorGUI.EndDisabledGroup();
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

            ValidateMaterial(material);
        }
    }    
}
