uniform float4 _LightColor0; // this is not set in c# code ?

struct Attributes
{
    float4 positionOS : POSITION;
    float3 normalOS : NORMAL;
    float2 texcoord : TEXCOORD0;
    
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Varyings
{
    float4 positionCS : SV_POSITION;
    float2 uv : TEXCOORD0;
    
    UNITY_VERTEX_INPUT_INSTANCE_ID
    UNITY_VERTEX_OUTPUT_STEREO
};

Varyings ToonOutlinePassVertex (Attributes input) {
    Varyings output = (Varyings)0;

    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, output);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

    output.uv = TRANSFORM_TEX(input.texcoord, _OutlineMap);
    
    half4 outline = SAMPLE_TEXTURE2D_LOD(_OutlineMap, sampler_OutlineMap, output.uv, 0);
    float3 objectPositionWS = TransformObjectToWorld(float3(0,0,0));
    float4 cameraClipSpacePosition = ComputeClipSpacePosition(GetCameraPositionWS(), GetWorldToHClipMatrix());   
    
    // Implement _OUTLINE_FADE
    // Step is faster but you'll get outline popping
#if defined(_OUTLINE_FADE)   
    half distanceStep = smoothstep(_OutlineEndFadeDistance, _OutlineStartFadeDistance, distance(objectPositionWS.xyz, GetCameraPositionWS()));
#else
    half distanceStep = 1 - step(_OutlineEndFadeDistance, distance(objectPositionWS.xyz, GetCameraPositionWS()));
#endif
    half width = (_OutlineWidth * 0.01 * distanceStep * outline.rgb).r;
        
#if defined(UNITY_REVERSED_Z)
    float zOffset = _OutlineOffsetZ * -0.01;
#else
    float zOffset = _OutlineOffsetZ * 0.01;
#endif
    
    output.positionCS = TransformObjectToHClip(input.positionOS.xyz + input.normalOS * width);    
    output.positionCS.z = output.positionCS.z + zOffset * cameraClipSpacePosition.z;   
    return output;
}

float4 ToonOutlinePassFragment(Varyings input) : SV_Target
{
#if defined(_ALPHATEST_ON)        
    half4 albedoAlpha = SampleAlbedoAlpha(input.uv, TEXTURE2D_ARGS(_BaseMap, sampler_BaseMap));
    half alpha = Alpha(albedoAlpha.a, _BaseColor, _Cutoff);
    
    return float4(_OutlineColor.rgb, alpha);
#else
    return float4(_OutlineColor.rgb, 1);
#endif
}