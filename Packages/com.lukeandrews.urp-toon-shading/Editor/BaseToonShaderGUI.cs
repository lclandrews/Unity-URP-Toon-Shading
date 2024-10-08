﻿using System;

using UnityEditor;

using UnityEngine;
using UnityEngine.Rendering;

namespace ToonShading.Editor
{
    internal abstract class BaseToonShaderGUI : BaseShaderGUI
    {
        protected abstract string VariantName
        {
            get;
        }

        public struct MaterialProperties
        {
            // Surface Input Props
            public MaterialProperty BacklightStrength;
            public MaterialProperty EdgeShineColor;
            public MaterialProperty Smoothness;
            public MaterialProperty SpecColor;
            public MaterialProperty MaskMap;
            public MaterialProperty SpecTexMap;
            public MaterialProperty SpecTexTile;
            public MaterialProperty SpecTexRot;
            public MaterialProperty BumpScaleProp;
            public MaterialProperty BumpMapProp;
            public MaterialProperty OcclusionStrength;

            //Outline
            public MaterialProperty OutlineWidth;
            public MaterialProperty OutlineWidthMap;
            public MaterialProperty OutlineStartFadeDistance;
            public MaterialProperty OutlineEndFadeDistance;
            public MaterialProperty OutlineColor;
            public MaterialProperty OutlineOffsetZ;

            // Toggles
            public MaterialProperty FillSpecular;
            public MaterialProperty FillOcclusion;
            public MaterialProperty FillEmission;
            public MaterialProperty FillSmoothness;

            public MaterialProperty Specular;
            public MaterialProperty EdgeShine;
            public MaterialProperty Backlight;

            public MaterialProperty Outline;
            public MaterialProperty OutlineFade;

            public static readonly ShaderTagId CustomPassTagId = new ShaderTagId("SRPDefaultUnlit");

            public MaterialProperties(MaterialProperty[] properties)
            {
                BacklightStrength = BaseShaderGUI.FindProperty(Properties.BacklightStrength, properties, false);
                EdgeShineColor = BaseShaderGUI.FindProperty(Properties.ShineColor, properties, false);
                Smoothness = BaseShaderGUI.FindProperty(Properties.Smoothness, properties, false);
                SpecColor = BaseShaderGUI.FindProperty(Properties.SpecColor, properties, false);
                MaskMap = BaseShaderGUI.FindProperty(Properties.ToonMask, properties, false);
                SpecTexMap = BaseShaderGUI.FindProperty(Properties.SpecTexMap, properties, false);
                SpecTexTile = BaseShaderGUI.FindProperty(Properties.SpecTexTile, properties, false);
                SpecTexRot = BaseShaderGUI.FindProperty(Properties.SpecTexRot, properties, false);
                BumpMapProp = BaseShaderGUI.FindProperty(Properties.BumpMap, properties, false);
                BumpScaleProp = BaseShaderGUI.FindProperty(Properties.BumpScale, properties, false);
                OcclusionStrength = BaseShaderGUI.FindProperty(Properties.OcclusionStrength, properties, false);

                // Outline
                OutlineWidth = BaseShaderGUI.FindProperty(Properties.OutlineWidth, properties, false);
                OutlineWidthMap = BaseShaderGUI.FindProperty(Properties.OutlineMap, properties, false);
                OutlineStartFadeDistance = BaseShaderGUI.FindProperty(Properties.OutlineStartFadeDistance, properties, false);
                OutlineEndFadeDistance = BaseShaderGUI.FindProperty(Properties.OutlineEndFadeDistance, properties, false);
                OutlineColor = BaseShaderGUI.FindProperty(Properties.OutlineColor, properties, false);
                OutlineOffsetZ = BaseShaderGUI.FindProperty(Properties.OutlineOffsetZ, properties, false);

                // Toggles
                FillSpecular = BaseShaderGUI.FindProperty(Properties.FillSpecular, properties, false);
                FillOcclusion = BaseShaderGUI.FindProperty(Properties.FillOcclusion, properties, false);
                FillEmission = BaseShaderGUI.FindProperty(Properties.FillEmission, properties, false);
                FillSmoothness = BaseShaderGUI.FindProperty(Properties.FillSmoothness, properties, false);

                Specular = BaseShaderGUI.FindProperty(Properties.Specular, properties, false);
                Backlight = BaseShaderGUI.FindProperty(Properties.Backlight, properties, false);
                EdgeShine = BaseShaderGUI.FindProperty(Properties.EdgeShine, properties, false);
                Outline = BaseShaderGUI.FindProperty(Properties.Outline, properties, false);
                OutlineFade = BaseShaderGUI.FindProperty(Properties.OutlineFade, properties, false);
            }
        }

        private SavedBool _advancedLightingFoldout = null;
        private SavedBool _surfaceOptionsFoldout = null;
        private SavedBool _surfaceInputsFoldout = null;
        private SavedBool _outlineFoldout = null;
        private SavedBool _advancedFoldout = null;

        // Properties
        protected MaterialProperties ToonStandardProperties;        


        // collect properties from the material properties
        public override void FindProperties(MaterialProperty[] properties)
        {            
            cullingProp = FindProperty(Properties.Cull, properties);
            alphaClipProp = FindProperty(Properties.AlphaClip, properties);
            alphaCutoffProp = FindProperty(Properties.Cutoff, properties);
            receiveShadowsProp = FindProperty(Properties.ReceiveShadows, properties, false);
            baseMapProp = FindProperty(Properties.BaseMap, properties, false);
            baseColorProp = FindProperty(Properties.BaseColor, properties, false);
            emissionColorProp = FindProperty(Properties.EmissionColor, properties, false);
            queueOffsetProp = FindProperty(Properties.QueueOffset, properties, false);
            ToonStandardProperties = new MaterialProperties(properties);
        }

        public override void OnGUI(MaterialEditor materialEditorIn, MaterialProperty[] properties)
        {
            if (materialEditorIn == null)
                throw new ArgumentNullException(nameof(materialEditorIn));

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
            _advancedLightingFoldout = new SavedBool(GUIHeaders.GetStateKey(VariantName, GUIHeaders.AdvancedLightingFoldout), true);
            _surfaceOptionsFoldout = new SavedBool(GUIHeaders.GetStateKey(VariantName, GUIHeaders.SurfaceOptionsFoldout), true);
            _surfaceInputsFoldout = new SavedBool(GUIHeaders.GetStateKey(VariantName, GUIHeaders.SurfaceInputsFoldout), true);
            _outlineFoldout = new SavedBool(GUIHeaders.GetStateKey(VariantName, GUIHeaders.OutlineFoldout), false);
            _advancedFoldout = new SavedBool(GUIHeaders.GetStateKey(VariantName, GUIHeaders.AdvancedFoldout), false);

            foreach (var obj in materialEditor.targets)
                ValidateMaterial((Material)obj);
        }

        new public void ShaderPropertiesGUI(Material material)
        {
            if (material == null)
                throw new ArgumentNullException("material");

            EditorGUI.BeginChangeCheck();

            _surfaceOptionsFoldout.value = EditorGUILayout.BeginFoldoutHeaderGroup(_surfaceOptionsFoldout.value, Styles.SurfaceOptions);
            if (_surfaceOptionsFoldout.value)
            {
                DrawSurfaceOptions(material);
                EditorGUILayout.Space();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();            

            _surfaceInputsFoldout.value = EditorGUILayout.BeginFoldoutHeaderGroup(_surfaceInputsFoldout.value, Styles.SurfaceInputs);
            if (_surfaceInputsFoldout.value)
            {
                DrawSurfaceInputs(material);
                EditorGUILayout.Space();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            _outlineFoldout.value = EditorGUILayout.BeginFoldoutHeaderGroup(_outlineFoldout.value, GUIStyles.OutlineSettingsText);
            if (_outlineFoldout.value)
            {
                DrawOutlineArea(ToonStandardProperties, materialEditor, material);
                EditorGUILayout.Space();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            _advancedLightingFoldout.value = EditorGUILayout.BeginFoldoutHeaderGroup(_advancedLightingFoldout.value, GUIStyles.AdvLighting);
            if (_advancedLightingFoldout.value)
            {
                DrawLightingOptions();
                EditorGUILayout.Space();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();            

            _advancedFoldout.value = EditorGUILayout.BeginFoldoutHeaderGroup(_advancedFoldout.value, Styles.AdvancedLabel);
            if (_advancedFoldout.value)
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

            SetMaterialKeywordsAndPasses(material);
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
            DrawMaskMapArea(ToonStandardProperties, materialEditor, material);
            DrawSpecularArea(ToonStandardProperties, materialEditor, material);
            DrawNormalArea(materialEditor, ToonStandardProperties.BumpMapProp, ToonStandardProperties.BumpScaleProp);
            DrawBacklightArea(ToonStandardProperties, materialEditor, material);
            DrawEdgeShineArea(ToonStandardProperties, materialEditor, material);
            DrawOcclusionArea(materialEditor, ToonStandardProperties.MaskMap, ToonStandardProperties.OcclusionStrength);
            DrawEmissionProperties(material, true);
            DrawTileOffset(materialEditor, baseMapProp);
        }

        // material main advanced options
        public override void DrawAdvancedOptions(Material material)
        {
            base.DrawAdvancedOptions(material);
        }

        public virtual void DrawLightingOptions()
        {
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

        public void DrawBacklightArea(MaterialProperties properties, MaterialEditor materialEditor, Material material)
        {
            bool backlightEnabled = alphaClipProp.floatValue == 0.0f;

            if (backlightEnabled)
            {
                EditorGUI.BeginChangeCheck();
                materialEditor.ShaderProperty(ToonStandardProperties.Backlight, GUIStyles.BacklightText);
                if (EditorGUI.EndChangeCheck())
                {
                    ValidateMaterial(material);
                }

                EditorGUI.BeginDisabledGroup(properties.Backlight.floatValue == 0.0f);
                {
                    EditorGUI.indentLevel += 2;
                    EditorGUI.BeginChangeCheck();
                    float backlightStrength = EditorGUILayout.Slider(
                        GUIStyles.BacklightStrengthText, properties.BacklightStrength.floatValue,
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

        public void DrawEdgeShineArea(MaterialProperties properties, MaterialEditor materialEditor, Material material)
        {
            bool edgeShineEnabled = alphaClipProp.floatValue == 0.0f;

            if (edgeShineEnabled)
            {
                EditorGUI.BeginChangeCheck();
                materialEditor.ShaderProperty(properties.EdgeShine, GUIStyles.EdgeShineText);
                if (EditorGUI.EndChangeCheck())
                {
                    ValidateMaterial(material);
                }

                EditorGUI.BeginDisabledGroup(properties.EdgeShine.floatValue == 0.0f);
                {
                    Color edgeColor = properties.EdgeShineColor.colorValue;
                    EditorGUI.indentLevel += 2;
                    EditorGUI.BeginChangeCheck();
                    edgeColor = GUIHelpers.DrawColorField(edgeColor, GUIStyles.EdgeShineColorText, true, false, false);
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
                    GUIStyles.OcclusionStrengthText, occlusionStrength.floatValue, occlusionStrength.rangeLimits.x, occlusionStrength.rangeLimits.y);
                if (EditorGUI.EndChangeCheck())
                {
                    occlusionStrength.floatValue = oS;
                }
                EditorGUI.showMixedValue = false;
            }            
        }

        public void DrawMaskMapArea(MaterialProperties properties, MaterialEditor materialEditor, Material material)
        {
            if (properties.MaskMap != null)
            {
                materialEditor.TexturePropertySingleLine(GUIStyles.MaskMapText, properties.MaskMap);

                if(properties.MaskMap.textureValue != null)
                {
                    EditorGUI.indentLevel += 2;
                    EditorGUI.BeginChangeCheck();
                    materialEditor.ShaderProperty(properties.FillSpecular, GUIStyles.MaskFillSpecular);
                    if (EditorGUI.EndChangeCheck())
                    {
                        ValidateMaterial(material);
                    }
                    EditorGUI.BeginChangeCheck();
                    materialEditor.ShaderProperty(properties.FillOcclusion, GUIStyles.MaskFillOcclusion);
                    if (EditorGUI.EndChangeCheck())
                    {
                        ValidateMaterial(material);
                    }
                    EditorGUI.BeginChangeCheck();
                    materialEditor.ShaderProperty(properties.FillEmission, GUIStyles.MaskFillEmission);
                    if (EditorGUI.EndChangeCheck())
                    {
                        ValidateMaterial(material);
                    }
                    EditorGUI.BeginChangeCheck();
                    materialEditor.ShaderProperty(properties.FillSmoothness, GUIStyles.MaskFillSmoothness);
                    if (EditorGUI.EndChangeCheck())
                    {
                        ValidateMaterial(material);
                    }
                    EditorGUI.indentLevel -= 2;
                }
            }
        }

        public void DrawSpecularArea(MaterialProperties properties, MaterialEditor materialEditor, Material material)
        {
            EditorGUI.BeginChangeCheck();
            materialEditor.ShaderProperty(properties.Specular, GUIStyles.SpecularText);
            if (EditorGUI.EndChangeCheck())
            {
                ValidateMaterial(material);
            }

            EditorGUI.BeginDisabledGroup(properties.Specular.floatValue == 0.0f);
            {
                EditorGUI.BeginChangeCheck();
                Color specColor = GUIHelpers.DrawColorField(properties.SpecColor.colorValue, GUIStyles.SpecColorText, true, false, false);
                if (EditorGUI.EndChangeCheck())
                {
                    properties.SpecColor.colorValue = specColor;
                }                

                DrawSpecularTextureArea(properties, materialEditor, material);

                EditorGUI.indentLevel += 2;
                EditorGUI.BeginChangeCheck();
                EditorGUI.showMixedValue = properties.Smoothness.hasMixedValue;
                float smoothness = EditorGUILayout.Slider(
                    GUIStyles.SmoothnessText, properties.Smoothness.floatValue, properties.Smoothness.rangeLimits.x, properties.Smoothness.rangeLimits.y);
                if (EditorGUI.EndChangeCheck())
                {
                    properties.Smoothness.floatValue = smoothness;
                }
                EditorGUI.showMixedValue = false;
                EditorGUI.indentLevel -= 2;
            }
            EditorGUI.EndDisabledGroup();
        }

        public void DrawSpecularTextureArea(MaterialProperties properties, MaterialEditor materialEditor, Material material)
        {
            //EditorGUI.BeginDisabledGroup(toonStandardProperties.specular.floatValue == 0.0f);
            if (ToonStandardProperties.Specular.floatValue == 1.0f)
            {
                EditorGUI.indentLevel += 2;
                materialEditor.TexturePropertySingleLine(GUIStyles.SpecTex, properties.SpecTexMap);
                EditorGUI.indentLevel -= 2;

                if (properties.SpecTexMap.textureValue != null)
                {
                    EditorGUI.indentLevel += 4;
                    EditorGUI.BeginChangeCheck();
                    EditorGUI.showMixedValue = properties.SpecTexTile.hasMixedValue;
                    float specTexTile = EditorGUILayout.Slider(
                        GUIStyles.SpecTexTile, properties.SpecTexTile.floatValue, properties.SpecTexTile.rangeLimits.x, properties.SpecTexTile.rangeLimits.y);
                    if (EditorGUI.EndChangeCheck())
                    {
                        properties.SpecTexTile.floatValue = specTexTile;
                    }

                    EditorGUI.BeginChangeCheck();
                    EditorGUI.showMixedValue = properties.SpecTexRot.hasMixedValue;
                    float specTexRot = EditorGUILayout.Slider(
                        GUIStyles.SpecTexRot, properties.SpecTexRot.floatValue, properties.SpecTexRot.rangeLimits.x, properties.SpecTexRot.rangeLimits.y);
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
                Color emColor = GUIHelpers.DrawColorField(emissionColorProp.colorValue, GUIStyles.EmissionColorText, true, false, true);
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
                    Color emColor = GUIHelpers.DrawColorField(emissionColorProp.colorValue, GUIStyles.EmissionColorText, true, false, true);
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

        public void DrawOutlineArea(MaterialProperties properties, MaterialEditor materialEditor, Material material)
        {
            EditorGUI.BeginChangeCheck();
            materialEditor.ShaderProperty(properties.Outline, GUIStyles.OutlineText);
            if (EditorGUI.EndChangeCheck())
            {
                ValidateMaterial(material);
            }

            EditorGUI.BeginDisabledGroup(properties.Outline.floatValue == 0.0f);
            {                
                EditorGUI.BeginChangeCheck();
                EditorGUI.showMixedValue = properties.OutlineWidth.hasMixedValue;
                float width = EditorGUILayout.FloatField(GUIStyles.OutlineWidthText, properties.OutlineWidth.floatValue);
                if (EditorGUI.EndChangeCheck())
                {
                    properties.OutlineWidth.floatValue = width;
                }

                EditorGUI.indentLevel += 2;
                materialEditor.TexturePropertySingleLine(GUIStyles.OutlineWidthMapText, properties.OutlineWidthMap);
                EditorGUI.indentLevel -= 2;

                Color outlineColor = GUIHelpers.DrawColorField(properties.OutlineColor.colorValue, GUIStyles.OutlineColorText, true, false, false);
                if (EditorGUI.EndChangeCheck())
                {
                    properties.OutlineColor.colorValue = outlineColor;
                }

                EditorGUI.BeginChangeCheck();
                materialEditor.ShaderProperty(properties.OutlineFade, GUIStyles.OutlineFadeText);
                if (EditorGUI.EndChangeCheck())
                {
                    ValidateMaterial(material);
                }

                if(properties.OutlineFade.floatValue == 0.0F)
                {
                    EditorGUI.BeginChangeCheck();
                    float cullingDistance = EditorGUILayout.FloatField(GUIStyles.OutlineCullText, properties.OutlineEndFadeDistance.floatValue);
                    if (EditorGUI.EndChangeCheck())
                    {
                        properties.OutlineEndFadeDistance.floatValue = cullingDistance;
                    }
                }
                else
                {
                    EditorGUI.BeginChangeCheck();
                    float startFadeDistance = EditorGUILayout.FloatField(GUIStyles.OutlineFadeStartText, properties.OutlineStartFadeDistance.floatValue);
                    if (EditorGUI.EndChangeCheck())
                    {
                        properties.OutlineStartFadeDistance.floatValue = startFadeDistance;
                    }

                    EditorGUI.BeginChangeCheck();
                    float endFadeDistance = EditorGUILayout.FloatField(GUIStyles.OutlineFadeEndText, properties.OutlineEndFadeDistance.floatValue);
                    if (EditorGUI.EndChangeCheck())
                    {
                        properties.OutlineEndFadeDistance.floatValue = endFadeDistance;
                    }
                }

                EditorGUI.BeginChangeCheck();
                float zOffset = EditorGUILayout.FloatField(GUIStyles.OutlineOffsetZ, properties.OutlineOffsetZ.floatValue);
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
            if (material.HasProperty(Properties.Emission))
            {
                material.SetColor(Properties.EmissionColor, material.GetColor(Properties.Emission));
            }

            base.AssignNewShaderToMaterial(material, oldShader, newShader);
            if (oldShader == null || !oldShader.name.Contains("Legacy Shaders/"))
            {
                return;
            }

            if (oldShader.name.Contains("/Transparent/Cutout/"))
            {
                material.SetFloat(Properties.AlphaClip, 1);
            }

            ValidateMaterial(material);
        }

        public static void SetMaterialKeywordsAndPasses(Material material)
        {
            // Clear all keywords for fresh start
            material.shaderKeywords = null;
            // Setup blending - consistent across all Universal RP shaders

            if (material == null)
                throw new ArgumentNullException("material");

            bool alphaClip = material.GetFloat(Properties.AlphaClip) == 1;
            if (alphaClip)
            {
                material.EnableKeyword(Keywords.AlphaTestOn);
            }
            else
            {
                material.DisableKeyword(Keywords.AlphaTestOn);
            }

            var queueOffset = 0; // queueOffsetRange;
            if (material.HasProperty(Properties.QueueOffset))
            {
                queueOffset = 50 - (int)material.GetFloat(Properties.QueueOffset);
            }

            if (alphaClip)
            {
                material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.AlphaTest;
                material.SetOverrideTag("RenderType", "TransparentCutout");
            }
            else
            {
                material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Geometry;
                material.SetOverrideTag("RenderType", "Opaque");
            }
            material.renderQueue += queueOffset;
            material.SetShaderPassEnabled("ShadowCaster", true);

            // Receive Shadows
            if (material.HasProperty(Properties.ReceiveShadows))
            {
                CoreUtils.SetKeyword(material, Keywords.ReceiveShadowsOff, material.GetFloat(Properties.ReceiveShadows) == 0.0f);
            }
            // Emission
            if (material.HasProperty(Properties.EmissionColor))
            {
                MaterialEditor.FixupEmissiveFlag(material);
            }
            bool shouldEmissionBeEnabled = (material.globalIlluminationFlags & MaterialGlobalIlluminationFlags.EmissiveIsBlack) == 0;
            CoreUtils.SetKeyword(material, Keywords.Emission, shouldEmissionBeEnabled);
            // Normal Map
            if (material.HasProperty(Properties.BumpMap))
            {
                CoreUtils.SetKeyword(material, Keywords.NormalMap, material.GetTexture(Properties.BumpMap));
            }

            // Mask map
            bool maskMap = false;
            if (material.HasProperty(Properties.ToonMask))
            {
                maskMap = material.GetTexture(Properties.ToonMask);
                CoreUtils.SetKeyword(material, Keywords.MaskMap, maskMap);
            }

            if (material.HasProperty(Properties.FillSpecular))
            {
                CoreUtils.SetKeyword(material, Keywords.FillMaskSpecular, maskMap && material.GetFloat(Properties.FillSpecular) == 1.0f);
            }

            if (material.HasProperty(Properties.FillOcclusion))
            {
                CoreUtils.SetKeyword(material, Keywords.FillMaskOcclusion, maskMap && material.GetFloat(Properties.FillOcclusion) == 1.0f);
            }

            if (material.HasProperty(Properties.FillEmission))
            {
                CoreUtils.SetKeyword(material, Keywords.FillMaskEmission, maskMap && material.GetFloat(Properties.FillEmission) == 1.0f);
            }

            if (material.HasProperty(Properties.FillSmoothness))
            {
                CoreUtils.SetKeyword(material, Keywords.FillMaskSmoothness, maskMap && material.GetFloat(Properties.FillSmoothness) == 1.0f);
            }

            // Specular
            bool specular = true;
            if (material.HasProperty(Properties.Specular))
            {
                specular = material.GetFloat(Properties.Specular) == 1.0f;
                CoreUtils.SetKeyword(material, Keywords.SpecularOff, !specular);
            }
            // Specular Texture
            if (material.HasProperty(Properties.SpecTexMap))
            {
                CoreUtils.SetKeyword(material, Keywords.SpecularMap, material.GetTexture(Properties.SpecTexMap) && specular);
            }

            if (material.HasProperty(Properties.Backlight))
            {
                CoreUtils.SetKeyword(material, Keywords.BacklightOff, material.GetFloat(Properties.Backlight) == 0.0f || alphaClip);
            }

            if (material.HasProperty(Properties.EdgeShine))
            {
                CoreUtils.SetKeyword(material, Keywords.EdgeShineOff, material.GetFloat(Properties.EdgeShine) == 0.0f || alphaClip);
            }

            // Outline
            bool outline = false;
            if (material.HasProperty(Properties.Outline))
            {
                outline = material.GetFloat(Properties.Outline) == 1.0f;
                CoreUtils.SetKeyword(material, Keywords.Outline, outline);
                material.SetShaderPassEnabled(MaterialProperties.CustomPassTagId.name, outline);
            }

            if (material.HasProperty(Properties.OutlineFade))
            {
                CoreUtils.SetKeyword(material, Keywords.OutlineFade, material.GetFloat(Properties.OutlineFade) == 1.0f && outline);
            }
        }
    }    
}
