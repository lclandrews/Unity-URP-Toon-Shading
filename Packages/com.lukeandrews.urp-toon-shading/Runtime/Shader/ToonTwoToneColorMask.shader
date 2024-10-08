﻿Shader "Universal Render Pipeline/Toon Two Tone Color Mask"
{
    Properties
    {        
        [MainColor] _BaseColor("Color", Color) = (0.5,0.5,0.5,1)
        [MainTexture] _BaseMap("Albedo", 2D) = "white" {}        

        _Cutoff("Alpha Cutoff", Range(0.0, 1.0)) = 0.5

        _ColorMaskMap("Color Mask", 2D) = "white" {}
        _ColorMaskRColor("Color Mask (R) Color", Color) = (0.5,0.5,0.5)
        _ColorMaskGColor("Color Mask (G) Color", Color) = (0.5,0.5,0.5)
        _ColorMaskBColor("Color Mask (B) Color", Color) = (0.5,0.5,0.5)
        _ColorMaskAColor("Color Mask (A) Color", Color) = (0.5,0.5,0.5)

        _BacklightStrength("Backlight Stength", Range(0.0, 2.5)) = 2.3
        _ShineColor("Edge Shine Color", Color) = (1.0, 1.0, 1.0)

        _ToonMask("Toon Mask Map", 2D) = "white" {}

        _Smoothness("Smoothness", Range(0.0, 1.0)) = 0.4

        _SpecColor("Specular Color", Color) = (0.2, 0.2, 0.2)        

        _SpecTexMap("Specular Texture Map", 2D) = "white" {}
        _SpecTexTile("Specular Texture Tiling", Range(0.0, 50.0)) = 10.0
        _SpecTexRot("Specular Texture Rotation", Range(-2.0, 2.0)) = 0.0

        _BumpScale("Normal Map Scale", Float) = 1.0
        _BumpMap("Normal Map", 2D) = "bump" {}

        _OcclusionStrength("Strength", Range(0.0, 1.0)) = 1.0

        _EmissionColor("Emission Color", Color) = (0,0,0)

        _OutlineWidth ("Outline Width", Float) = 0.1
        _OutlineMap ("Outline Width Map", 2D) = "white" {}

        _OutlineStartFadeDistance ("Outline Start Fade Distance", Float ) = 0.5
        _OutlineEndFadeDistance ("Outline End Fade Distance", Float ) = 100       

        _OutlineColor ("Outline Color", Color) = (0.5,0.5,0.5,1)
        _OutlineOffsetZ ("Outline Camera Z Offset", Float) = 0

        // Editor toggles for keywords
        [ToggleOff] _FillSpecular("Fill Specular Channel", Float) = 1.0
        [ToggleOff] _FillOcclusion("Fill Occlusion Channel", Float) = 1.0
        [ToggleOff] _FillEmission("Fill Emission Channel", Float) = 1.0
        [ToggleOff] _FillSmoothness("Fill Smoothness Channel", Float) = 1.0

        [ToggleOff] _Specular("Specular", Float) = 1.0
        [ToggleOff] _Backlight("Backlight", Float) = 1.0
        [ToggleOff] _EdgeShine("Edge Shine", Float) = 1.0

        [ToggleOff] _Outline("Outline", Float) = 0.0
        [ToggleOff] _OutlineFade("Outline Fade", Float) = 0.0

        // Blending state
        [ToggleUI] _AlphaClip("__clip", Float) = 0.0
        [HideInInspector] _Cull("__cull", Float) = 2.0
        [HideInInspector] _ZWrite("__zw", Float) = 1.0
        [HideInInspector] _AlphaToMask("__alphaToMask", Float) = 0.0

        // Shadows keyword
        [ToggleUI] _ReceiveShadows("Receive Shadows", Float) = 1.0

        // Editmode props
        [HideInInspector] _QueueOffset("Queue offset", Float) = 0.0
    }        

    SubShader
    {
        // Universal Pipeline tag is required. If Universal render pipeline is not set in the graphics settings
        // this Subshader will fail. One can add a subshader below or fallback to Standard built-in to make this
        // material work with both Universal Render Pipeline and Builtin Unity Pipeline
        Tags{"RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "IgnoreProjector" = "True"}
        LOD 300

        Pass 
        {
            Name "Outline"
            Tags 
            {
                "LightMode" = "SRPDefaultUnlit"
            }

            Cull Front

            HLSLPROGRAM
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma target 2.0          

            #pragma shader_feature _ALPHATEST_ON
            #pragma shader_feature _OUTLINE
            #pragma shader_feature _OUTLINE_FADE

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing

            #include "TwoTone/ToonTwoToneColorMaskInput.hlsl"
            #include "ToonOutlinePass.hlsl"

            #pragma vertex ToonOutlinePassVertex
            #pragma fragment ToonOutlinePassFragment
            ENDHLSL
        }

        // ------------------------------------------------------------------
        //  Forward pass. Shades all light in a single pass. GI + emission + Fog
        Pass
        {
            // Lightmode matches the ShaderPassName set in UniversalRenderPipeline.cs. SRPDefaultUnlit and passes with
            // no LightMode tag are also rendered by Universal Render Pipeline
            //Name "ForwardLit"
            Name "ForwardLit"
            Tags{"LightMode" = "UniversalForward"}

            ZWrite[_ZWrite]
            Cull[_Cull]
            AlphaToMask[_AlphaToMask]

            HLSLPROGRAM
            // Required to compile gles 2.0 with standard SRP library
            // All shaders must be compiled with HLSLcc and currently only gles is not using HLSLcc by default
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma target 2.0

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature _ALPHATEST_ON

            #pragma shader_feature _MASKMAP

            #pragma shader_feature _FILLMASK_SPECULAR
            #pragma shader_feature _FILLMASK_OCCLUSION
            #pragma shader_feature _FILLMASK_EMISSION
            #pragma shader_feature _FILLMASK_SMOOTHNESS

            #pragma shader_feature _SPECTEXMAP
            #pragma shader_feature _NORMALMAP  
            #pragma shader_feature _EMISSION

            #pragma shader_feature _SPECULAR_OFF
            #pragma shader_feature _BACKLIGHT_OFF
            #pragma shader_feature _EDGESHINE_OFF

            #pragma shader_feature _RECEIVE_SHADOWS_OFF

            // -------------------------------------
            // Universal Pipeline keywords
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile _ EVALUATE_SH_MIXED EVALUATE_SH_VERTEX
            #pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
            #pragma multi_compile _ SHADOWS_SHADOWMASK
            #pragma multi_compile _ _LIGHT_LAYERS
            #pragma multi_compile _ _FORWARD_PLUS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_fragment _ _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
            #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
            #pragma multi_compile_fragment _ _DBUFFER_MRT1 _DBUFFER_MRT2 _DBUFFER_MRT3
            #pragma multi_compile_fragment _ _LIGHT_COOKIES
            #include_with_pragmas "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRenderingKeywords.hlsl"
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RenderingLayers.hlsl"

            // -------------------------------------
            // Unity defined keywords
            #pragma multi_compile _ DIRLIGHTMAP_COMBINED
            #pragma multi_compile _ LIGHTMAP_ON
            #pragma multi_compile _ DYNAMICLIGHTMAP_ON
            #pragma multi_compile_fog
            #pragma multi_compile_fragment _ DEBUG_DISPLAY
            #pragma multi_compile_fragment _ LOD_FADE_CROSSFADE

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing
            #pragma instancing_options renderinglayer
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"

            //--------------------------------------
            // Defines
            #define BUMP_SCALE_NOT_SUPPORTED 1

            // -------------------------------------
            // Includes
            #include "TwoTone/ToonTwoToneColorMaskInput.hlsl"
            #include "TwoTone/ToonTwoToneVertexPass.hlsl"
            #include "TwoTone/ToonTwoToneColorMaskFragmentPass.hlsl"

            #pragma vertex ToonPassVertex
            #pragma fragment ToonPassFragment            

            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags
            {
                "LightMode" = "ShadowCaster"
            }

            // -------------------------------------
            // Render State Commands
            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull[_Cull]

            HLSLPROGRAM
            #pragma target 2.0

            // -------------------------------------
            // Shader Stages
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature_local _ALPHATEST_ON
            #pragma shader_feature_local_fragment _GLOSSINESS_FROM_BASE_ALPHA

            // -------------------------------------
            // Unity defined keywords
            #pragma multi_compile_fragment _ LOD_FADE_CROSSFADE

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"

            // This is used during shadow map generation to differentiate between directional and punctual light shadows, as they use different formulas to apply Normal Bias
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW

            // -------------------------------------
            // Includes
            #include "TwoTone/ToonTwoToneColorMaskInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/ShadowCasterPass.hlsl"
            ENDHLSL
        }

        // Pass
        // {
        //     Name "GBuffer"
        //     Tags
        //     {
        //         "LightMode" = "UniversalGBuffer"
        //     }

        //     // -------------------------------------
        //     // Render State Commands
        //     ZWrite[_ZWrite]
        //     ZTest LEqual
        //     Cull[_Cull]

        //     HLSLPROGRAM
        //     #pragma target 4.5

        //     // Deferred Rendering Path does not support the OpenGL-based graphics API:
        //     // Desktop OpenGL, OpenGL ES 3.0, WebGL 2.0.
        //     #pragma exclude_renderers gles3 glcore

        //     // -------------------------------------
        //     // Shader Stages
        //     #pragma vertex LitPassVertexSimple
        //     #pragma fragment LitPassFragmentSimple

        //     // -------------------------------------
        //     // Material Keywords
        //     #pragma shader_feature_local_fragment _ALPHATEST_ON
        //     //#pragma shader_feature _ALPHAPREMULTIPLY_ON
        //     #pragma shader_feature_local_fragment _ _SPECGLOSSMAP _SPECULAR_COLOR
        //     #pragma shader_feature_local_fragment _GLOSSINESS_FROM_BASE_ALPHA
        //     #pragma shader_feature_local _NORMALMAP
        //     #pragma shader_feature_local_fragment _EMISSION
        //     #pragma shader_feature_local _RECEIVE_SHADOWS_OFF

        //     // -------------------------------------
        //     // Universal Pipeline keywords
        //     #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
        //     //#pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
        //     //#pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
        //     #pragma multi_compile_fragment _ _SHADOWS_SOFT
        //     #pragma multi_compile_fragment _ _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
        //     #pragma multi_compile_fragment _ _DBUFFER_MRT1 _DBUFFER_MRT2 _DBUFFER_MRT3
        //     #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RenderingLayers.hlsl"

        //     // -------------------------------------
        //     // Unity defined keywords
        //     #pragma multi_compile _ DIRLIGHTMAP_COMBINED
        //     #pragma multi_compile _ LIGHTMAP_ON
        //     #pragma multi_compile _ DYNAMICLIGHTMAP_ON
        //     #pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
        //     #pragma multi_compile _ SHADOWS_SHADOWMASK
        //     #pragma multi_compile_fragment _ _GBUFFER_NORMALS_OCT
        //     #pragma multi_compile_fragment _ _RENDER_PASS_ENABLED
        //     #pragma multi_compile_fragment _ LOD_FADE_CROSSFADE

        //     //--------------------------------------
        //     // GPU Instancing
        //     #pragma multi_compile_instancing
        //     #pragma instancing_options renderinglayer
        //     #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"

        //     //--------------------------------------
        //     // Defines
        //     #define BUMP_SCALE_NOT_SUPPORTED 1

        //     // -------------------------------------
        //     // Includes
        //     #include "Packages/com.unity.render-pipelines.universal/Shaders/SimpleLitInput.hlsl"
        //     #include "Packages/com.unity.render-pipelines.universal/Shaders/SimpleLitGBufferPass.hlsl"
        //     ENDHLSL
        // }

        Pass
        {
            Name "DepthOnly"
            Tags
            {
                "LightMode" = "DepthOnly"
            }

            // -------------------------------------
            // Render State Commands
            ZWrite On
            ColorMask R
            Cull[_Cull]

            HLSLPROGRAM
            #pragma target 2.0

            // -------------------------------------
            // Shader Stages
            #pragma vertex DepthOnlyVertex
            #pragma fragment DepthOnlyFragment

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature_local _ALPHATEST_ON
            #pragma shader_feature_local_fragment _GLOSSINESS_FROM_BASE_ALPHA

            // -------------------------------------
            // Unity defined keywords
            #pragma multi_compile_fragment _ LOD_FADE_CROSSFADE

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"

            // -------------------------------------
            // Includes
            #include "TwoTone/ToonTwoToneColorMaskInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/DepthOnlyPass.hlsl"
            ENDHLSL
        }

        // This pass is used when drawing to a _CameraNormalsTexture texture
        Pass
        {
            Name "DepthNormals"
            Tags
            {
                "LightMode" = "DepthNormals"
            }

            // -------------------------------------
            // Render State Commands
            ZWrite On
            Cull[_Cull]

            HLSLPROGRAM
            #pragma target 2.0

            // -------------------------------------
            // Shader Stages
            #pragma vertex DepthNormalsVertex
            #pragma fragment DepthNormalsFragment

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature_local _NORMALMAP
            #pragma shader_feature_local _ALPHATEST_ON
            #pragma shader_feature_local_fragment _GLOSSINESS_FROM_BASE_ALPHA

            // -------------------------------------
            // Unity defined keywords
            #pragma multi_compile_fragment _ LOD_FADE_CROSSFADE

            // Universal Pipeline keywords
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RenderingLayers.hlsl"

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"

            // -------------------------------------
            // Includes
            #include "TwoTone/ToonTwoToneColorMaskInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/SimpleLitDepthNormalsPass.hlsl"
            ENDHLSL
        }

        // This pass it not used during regular rendering, only for lightmap baking.
        // Pass
        // {
        //     Name "Meta"
        //     Tags
        //     {
        //         "LightMode" = "Meta"
        //     }

        //     // -------------------------------------
        //     // Render State Commands
        //     Cull Off

        //     HLSLPROGRAM
        //     #pragma target 2.0

        //     // -------------------------------------
        //     // Shader Stages
        //     #pragma vertex UniversalVertexMeta
        //     #pragma fragment UniversalFragmentMetaSimple

        //     // -------------------------------------
        //     // Material Keywords
        //     #pragma shader_feature_local_fragment _EMISSION
        //     #pragma shader_feature_local_fragment _SPECGLOSSMAP
        //     #pragma shader_feature EDITOR_VISUALIZATION

        //     // -------------------------------------
        //     // Includes
        //     #include "Packages/com.unity.render-pipelines.universal/Shaders/SimpleLitInput.hlsl"
        //     #include "Packages/com.unity.render-pipelines.universal/Shaders/SimpleLitMetaPass.hlsl"

        //     ENDHLSL
        // }

        // Pass
        // {
        //     Name "Universal2D"
        //     Tags
        //     {
        //         "LightMode" = "Universal2D"
        //         "RenderType" = "Transparent"
        //         "Queue" = "Transparent"
        //     }

        //     HLSLPROGRAM
        //     #pragma target 2.0

        //     // -------------------------------------
        //     // Shader Stages
        //     #pragma vertex vert
        //     #pragma fragment frag

        //     // -------------------------------------
        //     // Material Keywords
        //     #pragma shader_feature_local_fragment _ALPHATEST_ON
        //     #pragma shader_feature_local_fragment _ALPHAPREMULTIPLY_ON

        //     // -------------------------------------
        //     // Includes
        //     #include "Packages/com.unity.render-pipelines.universal/Shaders/SimpleLitInput.hlsl"
        //     #include "Packages/com.unity.render-pipelines.universal/Shaders/Utils/Universal2D.hlsl"
        //     ENDHLSL
        // }
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
    CustomEditor "ToonShading.Editor.ToonTwoToneColorMaskShaderGUI"
}
