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
            public MaterialProperty backlightStrength;
            public MaterialProperty edgeShineColor;
            public MaterialProperty smoothness;
            public MaterialProperty specColor;
            public MaterialProperty maskMap;
            public MaterialProperty specTexMap;
            public MaterialProperty specTexTile;
            public MaterialProperty specTexRot;
            public MaterialProperty bumpScaleProp;
            public MaterialProperty bumpMapProp;
            public MaterialProperty occlusionStrength;

            // Advanced Props
            public MaterialProperty fillSpecular;
            public MaterialProperty fillOcclusion;
            public MaterialProperty fillEmission;
            public MaterialProperty fillSmoothness;

            public MaterialProperty specular;
            public MaterialProperty edgeShine;
            public MaterialProperty backlight;

            public ToonSimpleProperties(MaterialProperty[] properties)
            {
                backlightStrength = BaseShaderGUI.FindProperty("_BacklightStrength", properties, false);
                edgeShineColor = BaseShaderGUI.FindProperty("_ShineColor", properties, false);
                smoothness = BaseShaderGUI.FindProperty("_Smoothness", properties, false);
                specColor = BaseShaderGUI.FindProperty("_SpecColor", properties, false);
                maskMap = BaseShaderGUI.FindProperty("_ToonMask", properties, false);
                specTexMap = BaseShaderGUI.FindProperty("_SpecTexMap", properties, false);
                specTexTile = BaseShaderGUI.FindProperty("_SpecTexTile", properties, false);
                specTexRot = BaseShaderGUI.FindProperty("_SpecTexRot", properties, false);
                bumpMapProp = BaseShaderGUI.FindProperty("_BumpMap", properties, false);
                bumpScaleProp = BaseShaderGUI.FindProperty("_BumpScale", properties, false);
                occlusionStrength = BaseShaderGUI.FindProperty("_OcclusionStrength", properties, false);
                // Advanced Props
                fillSpecular = BaseShaderGUI.FindProperty("_FillSpecular", properties, false);
                fillOcclusion = BaseShaderGUI.FindProperty("_FillOcclusion", properties, false);
                fillEmission = BaseShaderGUI.FindProperty("_FillEmission", properties, false);
                fillSmoothness = BaseShaderGUI.FindProperty("_FillSmoothness", properties, false);

                specular = BaseShaderGUI.FindProperty("_Specular", properties, false);
                backlight = BaseShaderGUI.FindProperty("_Backlight", properties, false);
                edgeShine = BaseShaderGUI.FindProperty("_EdgeShine", properties, false);
            }
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
        }
    }
}