using System;

using UnityEditor.ToonShaderGUI;

using UnityEngine;
using UnityEngine.Rendering;

namespace UnityEditor.Rendering.Universal.ShaderGUI
{
    public static class StandardToonGUI
    {
        public struct ToonStandardProperties
        {

            // Surface Input Props
            public MaterialProperty ambientColorProp;
            public MaterialProperty backlightStrength;
            public MaterialProperty edgeShineColor;
            public MaterialProperty smoothness;
            public MaterialProperty specColor;
            public MaterialProperty specMap;
            public MaterialProperty specTexMap;
            public MaterialProperty specTexTile;
            public MaterialProperty specTexRot;
            public MaterialProperty bumpScaleProp;
            public MaterialProperty bumpMapProp;
            public MaterialProperty occlusionStrength;
            public MaterialProperty occlusionMap;

            // Advanced Props
            public MaterialProperty specular;
            public MaterialProperty edgeShine;
            public MaterialProperty backlight;

            public ToonStandardProperties(MaterialProperty[] properties)
            {
                ambientColorProp = BaseShaderGUI.FindProperty("_AmbientColor", properties, false);
                backlightStrength = BaseShaderGUI.FindProperty("_BacklightStrength", properties, false);
                edgeShineColor = BaseShaderGUI.FindProperty("_ShineColor", properties, false);
                smoothness = BaseShaderGUI.FindProperty("_Smoothness", properties, false);
                specColor = BaseShaderGUI.FindProperty("_SpecColor", properties, false);
                specMap = BaseShaderGUI.FindProperty("_SpecMap", properties, false);
                specTexMap = BaseShaderGUI.FindProperty("_SpecTexMap", properties, false);
                specTexTile = BaseShaderGUI.FindProperty("_SpecTexTile", properties, false);
                specTexRot = BaseShaderGUI.FindProperty("_SpecTexRot", properties, false);
                bumpMapProp = BaseShaderGUI.FindProperty("_BumpMap", properties, false);
                bumpScaleProp = BaseShaderGUI.FindProperty("_BumpScale", properties, false);
                occlusionStrength = BaseShaderGUI.FindProperty("_OcclusionStrength", properties, false);
                occlusionMap = BaseShaderGUI.FindProperty("_OcclusionMap", properties, false);
                // Advanced Props
                specular = BaseShaderGUI.FindProperty("_Specular", properties, false);
                backlight = BaseShaderGUI.FindProperty("_Backlight", properties, false);
                edgeShine = BaseShaderGUI.FindProperty("_EdgeShine", properties, false);
            }
        }

        public static void DrawBacklightArea(ToonStandardProperties properties, MaterialProperty alphaClipProp, MaterialEditor materialEditor, Material material,
            Action<Material> materialChanged = null)
        {
            bool backlightEnabled = alphaClipProp.floatValue == 0.0f;

            if (backlightEnabled)
            {
                EditorGUI.BeginChangeCheck();
                materialEditor.ShaderProperty(properties.backlight, ToonMainGUI.Styles.backlightText);
                if (EditorGUI.EndChangeCheck())
                {
                    if(materialChanged != null)
                    {
                        materialChanged.Invoke(material);
                    }
                }

                EditorGUI.BeginDisabledGroup(properties.backlight.floatValue == 0.0f);
                {
                    EditorGUI.indentLevel += 2;
                    EditorGUI.BeginChangeCheck();
                    float backlightStrength = EditorGUILayout.Slider(
                        ToonMainGUI.Styles.backlightStrengthText, properties.backlightStrength.floatValue,
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

        public static void DrawEdgeShineArea(ToonStandardProperties properties, MaterialProperty alphaClipProp, MaterialEditor materialEditor, Material material,
            Action<Material> materialChanged = null)
        {
            bool edgeShineEnabled = alphaClipProp.floatValue == 0.0f;

            if (edgeShineEnabled)
            {
                EditorGUI.BeginChangeCheck();
                materialEditor.ShaderProperty(properties.edgeShine, ToonMainGUI.Styles.edgeShineText);
                if (EditorGUI.EndChangeCheck())
                {
                    if (materialChanged != null)
                    {
                        materialChanged.Invoke(material);
                    }
                }

                EditorGUI.BeginDisabledGroup(properties.edgeShine.floatValue == 0.0f);
                {
                    Color edgeColor = properties.edgeShineColor.colorValue;
                    EditorGUI.indentLevel += 2;
                    EditorGUI.BeginChangeCheck();
                    edgeColor = ToonMainGUI.DrawColorField(edgeColor, ToonMainGUI.Styles.edgeShineColorText, true, false, false);
                    if (EditorGUI.EndChangeCheck())
                    {
                        properties.edgeShineColor.colorValue = edgeColor;
                    }
                    EditorGUI.indentLevel -= 2;
                }
                EditorGUI.EndDisabledGroup();
            }
        }

        public static void DrawOcclusionArea(MaterialEditor editor, MaterialProperty occlusionMap, MaterialProperty occlusionStrength)
        {
            if (occlusionMap != null)
            {
                editor.TexturePropertySingleLine(ToonMainGUI.Styles.occlusionText, occlusionMap, occlusionMap.textureValue != null ? occlusionStrength : null);
            }
        }

        public static void DrawSpecularArea(ToonStandardProperties properties, MaterialEditor materialEditor, Material material,
            Action<Material> materialChanged = null)
        {
            EditorGUI.BeginChangeCheck();
            materialEditor.ShaderProperty(properties.specular, ToonMainGUI.Styles.highlightsText);
            if (EditorGUI.EndChangeCheck())
            {
                if (materialChanged != null)
                {
                    materialChanged.Invoke(material);
                }
            }

            EditorGUI.BeginDisabledGroup(properties.specular.floatValue == 0.0f);
            {
                BaseShaderGUI.TextureColorProps(materialEditor, ToonMainGUI.Styles.specularMapText, properties.specMap, properties.specColor);

                DrawSpecularTextureArea(properties, materialEditor, material);

                EditorGUI.indentLevel += 2;
                EditorGUI.BeginChangeCheck();
                EditorGUI.showMixedValue = properties.smoothness.hasMixedValue;
                float smoothness = EditorGUILayout.Slider(
                    ToonMainGUI.Styles.smoothnessText, properties.smoothness.floatValue, properties.smoothness.rangeLimits.x, properties.smoothness.rangeLimits.y);
                if (EditorGUI.EndChangeCheck())
                {
                    properties.smoothness.floatValue = smoothness;
                }
                EditorGUI.showMixedValue = false;
                EditorGUI.indentLevel -= 2;
            }
            EditorGUI.EndDisabledGroup();
        }

        public static void DrawSpecularTextureArea(ToonStandardProperties properties, MaterialEditor materialEditor, Material material)
        {
            //EditorGUI.BeginDisabledGroup(toonMasterProperties.specular.floatValue == 0.0f);
            if (properties.specular.floatValue == 1.0f)
            {
                EditorGUI.indentLevel += 2;
                materialEditor.TexturePropertySingleLine(ToonMainGUI.Styles.specTex, properties.specTexMap);
                EditorGUI.indentLevel -= 2;

                if (properties.specTexMap.textureValue != null)
                {
                    EditorGUI.indentLevel += 4;
                    EditorGUI.BeginChangeCheck();
                    EditorGUI.showMixedValue = properties.specTexTile.hasMixedValue;
                    float specTexTile = EditorGUILayout.Slider(
                        ToonMainGUI.Styles.specTexTile, properties.specTexTile.floatValue, properties.specTexTile.rangeLimits.x, properties.specTexTile.rangeLimits.y);
                    if (EditorGUI.EndChangeCheck())
                    {
                        properties.specTexTile.floatValue = specTexTile;
                    }

                    EditorGUI.BeginChangeCheck();
                    EditorGUI.showMixedValue = properties.specTexRot.hasMixedValue;
                    float specTexRot = EditorGUILayout.Slider(
                        ToonMainGUI.Styles.specTexRot, properties.specTexRot.floatValue, properties.specTexRot.rangeLimits.x, properties.specTexRot.rangeLimits.y);
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

        public static void SetMaterialKeywords(Material material)
        {
            // Clear all keywords for fresh start
            material.shaderKeywords = null;
            // Setup blending - consistent across all Universal RP shaders

            if (material == null)
                throw new ArgumentNullException("material");

            bool alphaClip = material.GetFloat("_AlphaClip") == 1;
            if (alphaClip)
            {
                material.EnableKeyword("_ALPHATEST_ON");
            }
            else
            {
                material.DisableKeyword("_ALPHATEST_ON");
            }

            var queueOffset = 0; // queueOffsetRange;
            if (material.HasProperty("_QueueOffset"))
            {
                queueOffset = 50 - (int)material.GetFloat("_QueueOffset");
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
            if (material.HasProperty("_ReceiveShadows"))
            {
                CoreUtils.SetKeyword(material, "_RECEIVE_SHADOWS_OFF", material.GetFloat("_ReceiveShadows") == 0.0f);
            }
            // Emission
            if (material.HasProperty("_EmissionColor"))
            {
                MaterialEditor.FixupEmissiveFlag(material);
            }
            bool shouldEmissionBeEnabled = (material.globalIlluminationFlags & MaterialGlobalIlluminationFlags.EmissiveIsBlack) == 0;
            if (material.HasProperty("_EmissionEnabled") && !shouldEmissionBeEnabled)
            {
                shouldEmissionBeEnabled = material.GetFloat("_EmissionEnabled") >= 0.5f;
            }
            CoreUtils.SetKeyword(material, "_EMISSION", shouldEmissionBeEnabled);
            if (material.HasProperty("_EmissionMap"))
            {
                CoreUtils.SetKeyword(material, "_EMISSIONMAP", material.GetTexture("_EmissionMap") && shouldEmissionBeEnabled);
            }
            // Normal Map
            if (material.HasProperty("_BumpMap"))
            {
                CoreUtils.SetKeyword(material, "_NORMALMAP", material.GetTexture("_BumpMap"));
            }
            // Specular
            bool specular = true;
            if (material.HasProperty("_Specular"))
            {
                specular = material.GetFloat("_Specular") == 1.0f;
                CoreUtils.SetKeyword(material, "_SPECULAR_OFF", !specular);
            }
            // Specular Map
            if (material.HasProperty("_SpecMap"))
            {
                CoreUtils.SetKeyword(material, "_SPECULARMAP", material.GetTexture("_SpecMap") && specular);
            }
            // Specular Texture
            if (material.HasProperty("_SpecTexMap"))
            {
                CoreUtils.SetKeyword(material, "_SPECTEXMAP", material.GetTexture("_SpecTexMap") && specular);
            }

            if (material.HasProperty("_Backlight"))
            {
                CoreUtils.SetKeyword(material, "_BACKLIGHT_OFF", material.GetFloat("_Backlight") == 0.0f || alphaClip);
            }

            if (material.HasProperty("_EdgeShine"))
            {
                CoreUtils.SetKeyword(material, "_EDGESHINE_OFF", material.GetFloat("_EdgeShine") == 0.0f || alphaClip);
            }

            if (material.HasProperty("_OcclusionMap"))
            {
                CoreUtils.SetKeyword(material, "_OCCLUSIONMAP", material.GetTexture("_OcclusionMap"));
            }
        }
    }
}