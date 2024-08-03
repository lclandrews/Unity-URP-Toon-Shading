using UnityEngine;

namespace ToonShading.Editor
{
    public static class GUIStyles
    {
        public static GUIContent MainLightingText = new GUIContent("Main Light Shading",
            "The ramp properties of the main light.");

        public static GUIContent AdditionalLightingText = new GUIContent("Additional Light Shading",
            "The ramp properties of additional lights.");

        public static GUIContent ShadowText = new GUIContent("Shadow Limit",
            "Used to control the shadow limit of a surfaces shading.");

        public static GUIContent HighlightText = new GUIContent("Highlight Limit",
            "Used to control the highlight limit of a surfaces shading.");

        public static GUIContent EdgeSoftnessText = new GUIContent("Edge Softness",
            "Used to control the edge softness between lighting bands.");

        public static GUIContent MidToneText = new GUIContent("Mid Tone Value",
            "Controls the value of the mid tones.");

        public static GUIContent MaskMapText = new GUIContent("Mask Map",
            "Map that stores: Specular (R), Occlusion (G), Emission (B), Smoothness (A).");

        public static GUIContent MaskFillSpecular = new GUIContent("Fill Specular",
            "Ignore the (R) channel of the mask map.");

        public static GUIContent MaskFillOcclusion = new GUIContent("Fill Occlusion",
            "Ignore the (G) channel of the mask map.");

        public static GUIContent MaskFillEmission = new GUIContent("Fill Emission",
            "Ignore the (B) channel of the mask map.");

        public static GUIContent MaskFillSmoothness = new GUIContent("Fill Smoothness",
            "Ignore the (A) channel of the mask map.");

        public static GUIContent SpecularMapText = new GUIContent("Specular Map",
            "Sets and configures the map and color for the Specular lighting.");

        public static GUIContent SmoothnessText = new GUIContent("Smoothness",
            "Controls the spread of highlights and reflections on the surface.");

        public static GUIContent SpecularText = new GUIContent("Specular",
            "When enabled, the Material reflects the shine from direct lighting.");

        public static GUIContent SpecColorText = new GUIContent("Specular Color",
            "Controls the specular tint of highlights.");

        public static GUIContent EmissionColorText = new GUIContent("Emission Color",
            "Controls the tint of emissive colors.");

        public static GUIContent SpecTex = new GUIContent("Specular Texture",
            "Map used to add texture to specular highlights.");

        public static GUIContent SpecTexTile = new GUIContent("Tiling",
            "Used to uv scale and rotation of the specular texture map.");

        public static GUIContent SpecTexRot = new GUIContent("Rotation",
            "Used to uv scale and rotation of the specular texture map.");

        public static GUIContent EdgeShineText = new GUIContent("Edge Shine",
            "Enables edge shine.");

        public static GUIContent EdgeShineColorText = new GUIContent("Edge Shine Color",
            "Edge shine color.");

        public static GUIContent BacklightText = new GUIContent("Back Lighting",
            "Enables back lighting.");

        public static GUIContent BacklightStrengthText = new GUIContent("Strength",
            "Back lighting strength.");

        public static GUIContent OcclusionText = new GUIContent("Occlusion Map",
            "Sets an occlusion map to simulate shadowing from ambient lighting.");

        public static GUIContent OcclusionStrengthText = new GUIContent("Occlusion Strength",
            "Sets the strength of the occlusion map.");

        public static readonly GUIContent AdvLighting = new GUIContent("Lighting",
            "These settings describe the way light interacts with the surface.");

        public static readonly GUIContent OutlineText = new GUIContent("Enable",
            "Enables outline.");

        public static readonly GUIContent OutlineSettingsText = new GUIContent("Outline",
            "All outline settings.");

        public static readonly GUIContent OutlineWidthText = new GUIContent("Width",
            "Controls with width of the mesh outline.");

        public static readonly GUIContent OutlineWidthMapText = new GUIContent("Width Map",
            "An additional map used to variably control the width of the outline.");

        public static readonly GUIContent OutlineColorText = new GUIContent("Color",
            "Color applied to the outline.");

        public static readonly GUIContent OutlineFadeText = new GUIContent("Fade",
            "Whether to smoothly fade out the outline when culled.");

        public static readonly GUIContent OutlineFadeStartText = new GUIContent("Fade Start",
            "The distance at which the outline should begin to fade out.");

        public static readonly GUIContent OutlineFadeEndText = new GUIContent("Fade End",
            "The distance at which the outline should have faded out completely.");

        public static readonly GUIContent OutlineCullText = new GUIContent("Cull Distance",
            "The distance at which the outline should be culled.");

        public static readonly GUIContent OutlineOffsetZ = new GUIContent("Depth Offset",
            "Offsets the outline in the depth (Z) direction of the camera.");
    }
}