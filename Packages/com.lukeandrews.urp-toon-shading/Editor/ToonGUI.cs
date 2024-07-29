using System;

using UnityEngine;
using UnityEngine.Rendering;

namespace UnityEditor.Rendering.Universal.ShaderGUI
{
    public static class ToonGUI
    {
        public struct ToonSimpleProperties
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

            public ToonSimpleProperties(MaterialProperty[] properties)
            {
                BacklightStrength = BaseShaderGUI.FindProperty("_BacklightStrength", properties, false);
                EdgeShineColor = BaseShaderGUI.FindProperty("_ShineColor", properties, false);
                Smoothness = BaseShaderGUI.FindProperty("_Smoothness", properties, false);
                SpecColor = BaseShaderGUI.FindProperty("_SpecColor", properties, false);
                MaskMap = BaseShaderGUI.FindProperty("_ToonMask", properties, false);
                SpecTexMap = BaseShaderGUI.FindProperty("_SpecTexMap", properties, false);
                SpecTexTile = BaseShaderGUI.FindProperty("_SpecTexTile", properties, false);
                SpecTexRot = BaseShaderGUI.FindProperty("_SpecTexRot", properties, false);
                BumpMapProp = BaseShaderGUI.FindProperty("_BumpMap", properties, false);
                BumpScaleProp = BaseShaderGUI.FindProperty("_BumpScale", properties, false);
                OcclusionStrength = BaseShaderGUI.FindProperty("_OcclusionStrength", properties, false);

                // Outline
                OutlineWidth = BaseShaderGUI.FindProperty("_OutlineWidth", properties, false);
                OutlineWidthMap = BaseShaderGUI.FindProperty("_OutlineMap", properties, false);
                OutlineStartFadeDistance = BaseShaderGUI.FindProperty("_OutlineStartFadeDistance", properties, false);
                OutlineEndFadeDistance = BaseShaderGUI.FindProperty("_OutlineEndFadeDistance", properties, false);
                OutlineColor = BaseShaderGUI.FindProperty("_OutlineColor", properties, false);
                OutlineOffsetZ = BaseShaderGUI.FindProperty("_OutlineOffsetZ", properties, false);

                // Toggles
                FillSpecular = BaseShaderGUI.FindProperty("_FillSpecular", properties, false);
                FillOcclusion = BaseShaderGUI.FindProperty("_FillOcclusion", properties, false);
                FillEmission = BaseShaderGUI.FindProperty("_FillEmission", properties, false);
                FillSmoothness = BaseShaderGUI.FindProperty("_FillSmoothness", properties, false);

                Specular = BaseShaderGUI.FindProperty("_Specular", properties, false);
                Backlight = BaseShaderGUI.FindProperty("_Backlight", properties, false);
                EdgeShine = BaseShaderGUI.FindProperty("_EdgeShine", properties, false);
                Outline = BaseShaderGUI.FindProperty("_Outline", properties, false);
                OutlineFade = BaseShaderGUI.FindProperty("_OutlineFade", properties, false);
            }
        }

        public static void SetMaterialKeywordsAndPasses(Material material)
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

            // Mask map
            bool maskMap = false;
            if (material.HasProperty("_ToonMask"))
            {
                maskMap = material.GetTexture("_ToonMask");
                CoreUtils.SetKeyword(material, "_MASKMAP", maskMap);
            }

            if (material.HasProperty("_FillSpecular"))
            {                
                CoreUtils.SetKeyword(material, "_FILLMASK_SPECULAR", maskMap && material.GetFloat("_FillSpecular") == 1.0f);
            }

            if (material.HasProperty("_FillOcclusion"))
            {
                CoreUtils.SetKeyword(material, "_FILLMASK_OCCLUSION", maskMap && material.GetFloat("_FillOcclusion") == 1.0f);
            }

            if (material.HasProperty("_FillEmission"))
            {
                CoreUtils.SetKeyword(material, "_FILLMASK_EMISSION", maskMap && material.GetFloat("_FillEmission") == 1.0f);
            }

            if (material.HasProperty("_FillSmoothness"))
            {
                CoreUtils.SetKeyword(material, "_FILLMASK_SMOOTHNESS", maskMap && material.GetFloat("_FillSmoothness") == 1.0f);
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

            // Outline
            bool outline = false;
            if (material.HasProperty("_Outline"))
            {
                outline = material.GetFloat("_Outline") == 1.0f;
                CoreUtils.SetKeyword(material, "_OUTLINE", outline);
                material.SetShaderPassEnabled(ToonSimpleProperties.CustomPassTagId.name, outline);
            }
            
            if (material.HasProperty("_OutlineFade"))
            {
                CoreUtils.SetKeyword(material, "_OUTLINE_FADE", material.GetFloat("_OutlineFade") == 1.0f && outline);
            }
        }
    }
}