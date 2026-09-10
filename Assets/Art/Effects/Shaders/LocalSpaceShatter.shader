Shader "URP/Effects/LocalSpaceShatter"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite", 2D) = "white" {}
        _Color ("Tint", Color) = (1, 1, 1, 1)

        [Header(Progress)]
        _Intensity ("Shatter Intensity", Range(0, 1)) = 0
        _Hole ("Reveal Hole", Range(0, 1)) = 0

        [Header(Shape)]
        _Radius ("Outer Radius", Range(0.1, 0.7)) = 0.48
        _EdgeSoft ("Edge Softness", Range(0.01, 0.2)) = 0.06

        [Header(Shatter)]
        _CellDensity ("Cell Density", Range(3, 30)) = 10
        _ShatterAmount ("Shard Offset", Range(0, 0.3)) = 0.1
        _CrackWidth ("Crack Width", Range(0.002, 0.1)) = 0.025
        _CrackDepth ("Crack Darkness", Range(0, 1)) = 0.9
        [HDR] _CrackColor ("Crack Glow", Color) = (0.35, 0.9, 1.3, 1)
        _Rotation ("Shard Rotate", Range(0, 1)) = 0.4
        _Chromatic ("Chromatic", Range(0, 0.05)) = 0.018
        [HDR] _VoidColor ("Void Color", Color) = (0.02, 0.02, 0.04, 1)
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "IgnoreProjector" = "True"
            "CanUseSpriteAtlas" = "True"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            Name "LocalSpaceShatter"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"
            #include "LocalSpaceShatterCommon.hlsl"
            ENDHLSL
        }

        Pass
        {
            Name "LocalSpaceShatter2D"
            Tags { "LightMode" = "Universal2D" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"
            #include "LocalSpaceShatterCommon.hlsl"
            ENDHLSL
        }
    }

    Fallback Off
}
