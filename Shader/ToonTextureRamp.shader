Shader "Universal Render Pipeline/Toon Texture Ramp"
{
    Properties
    {
        _AmbientColor("Ambient Color", Color) = (0.1, 0.1, 0.1)

        _MainRamp("Main Light Ramp", 2D) = "white" {}
        _AddRamp("Additional Light Ramp", 2D) = "white" {}

        [MainColor] _BaseColor("Color", Color) = (0.5,0.5,0.5,1)
        [MainTexture] _BaseMap("Albedo", 2D) = "white" {}

        _Cutoff("Alpha Cutoff", Range(0.0, 1.0)) = 0.5

        _BacklightStrength("Backlight Stength", Range(0.0, 2.5)) = 2.3
        _ShineColor("Edge Shine Color", Color) = (1.0, 1.0, 1.0)

        _Smoothness("Smoothness", Range(0.0, 1.0)) = 0.4

        _SpecColor("Specular Color", Color) = (0.2, 0.2, 0.2)
        _SpecMap("Specular Map", 2D) = "white" {}

        _SpecTexMap("Specular Texture Map", 2D) = "white" {}
        _SpecTexTile("Specular Texture Tiling", Range(0.0, 50.0)) = 10.0
        _SpecTexRot("Specular Texture Rotation", Range(-2.0, 2.0)) = 0.0        

        _BumpScale("Normal Map Scale", Float) = 1.0
        _BumpMap("Normal Map", 2D) = "bump" {}

        _OcclusionStrength("Strength", Range(0.0, 1.0)) = 1.0
        _OcclusionMap("Occlusion Map", 2D) = "white" {}

        _EmissionColor("Emission Color", Color) = (0,0,0)
        _EmissionMap("Emission Map", 2D) = "white" {}

        // Editor toggles for keywords
        [ToggleOff] _Specular("Specular", Float) = 1.0
        [ToggleOff] _Backlight("Backlight", Float) = 1.0
        [ToggleOff] _EdgeShine("Edge Shine", Float) = 1.0

        // Blending state
        [HideInInspector] _AlphaClip("__clip", Float) = 0.0
        [HideInInspector] _Cull("__cull", Float) = 2.0

        // Shadows keyword
        _ReceiveShadows("Receive Shadows", Float) = 1.0

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

        // ------------------------------------------------------------------
        //  Forward pass. Shades all light in a single pass. GI + emission + Fog
        Pass
        {
            // Lightmode matches the ShaderPassName set in UniversalRenderPipeline.cs. SRPDefaultUnlit and passes with
            // no LightMode tag are also rendered by Universal Render Pipeline
            //Name "ForwardLit"
            Name "ForwardLit"
            Tags{"LightMode" = "UniversalForward"}

            ZWrite On
            Cull[_Cull]

            HLSLPROGRAM
            // Required to compile gles 2.0 with standard SRP library
            // All shaders must be compiled with HLSLcc and currently only gles is not using HLSLcc by default
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma target 2.0

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature _ALPHATEST_ON
            
            #pragma shader_feature _SPECULARMAP
            #pragma shader_feature _SPECTEXMAP
            #pragma shader_feature _NORMALMAP  
            #pragma shader_feature _OCCLUSIONMAP
            #pragma shader_feature _EMISSION
            #pragma shader_feature _EMISSIONMAP             

            #pragma shader_feature _SPECULAR_OFF
            #pragma shader_feature _BACKLIGHT_OFF
            #pragma shader_feature _EDGESHINE_OFF

            #pragma shader_feature _RECEIVE_SHADOWS_OFF

            // -------------------------------------
            // Universal Pipeline keywords
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile _ _SHADOWS_SOFT
            #pragma multi_compile _ _MIXED_LIGHTING_SUBTRACTIVE

            // -------------------------------------
            // Unity defined keywords
            #pragma multi_compile _ DIRLIGHTMAP_COMBINED
            #pragma multi_compile _ LIGHTMAP_ON
            #pragma multi_compile_fog

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing

            #include "ToonTextureInput.hlsl"
            #include "Standard/ToonStandardVertexPass.hlsl"
            #include "ToonTextureFragmentPass.hlsl"

            #pragma vertex ToonStandardPassVertex
            #pragma fragment ToonTexturePassFragment            
            
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags{"LightMode" = "ShadowCaster"}

            ZWrite On
            ZTest LEqual
            Cull[_Cull]

            HLSLPROGRAM
            // Required to compile gles 2.0 with standard srp library
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma target 2.0

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature _ALPHATEST_ON

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing

            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment
            
            #include "ToonTextureInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/ShadowCasterPass.hlsl"
            ENDHLSL
        }

        Pass
        {
            Name "DepthOnly"
            Tags{"LightMode" = "DepthOnly"}

            ZWrite On
            ColorMask 0
            Cull[_Cull]

            HLSLPROGRAM
            // Required to compile gles 2.0 with standard srp library
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma target 2.0

            #pragma vertex DepthOnlyVertex
            #pragma fragment DepthOnlyFragment

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature _ALPHATEST_ON

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing

            #include "ToonTextureInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/DepthOnlyPass.hlsl"
            ENDHLSL
        }

        // This pass it not used during regular rendering, only for lightmap baking.
        Pass
        {
            Name "Meta"
            Tags{"LightMode" = "Meta"}

            Cull Off

            HLSLPROGRAM
            // Required to compile gles 2.0 with standard srp library
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x

            #pragma vertex ToonVertexMeta
            #pragma fragment ToonFragmentMeta

            #pragma shader_feature _ALPHATEST_ON

            #pragma shader_feature _SPECULARMAP
            #pragma shader_feature _SPECTEXMAP
            #pragma shader_feature _EMISSION
            #pragma shader_feature _EMISSIONMAP            

            #pragma shader_feature _SPECULAR_OFF

            #include "ToonTextureInput.hlsl"
            #include "Standard/ToonMetaPass.hlsl"

            ENDHLSL
        }
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
    CustomEditor "UnityEditor.Rendering.Universal.ShaderGUI.ToonMasterTex2DShader"
}
