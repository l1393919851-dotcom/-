Shader "URP/2D/CircularPortal"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite", 2D) = "white" {}
        _Color ("Tint", Color) = (1, 1, 1, 1)
        _SpriteUVRect ("Sprite UV Rect", Vector) = (0, 0, 1, 1)

        [Header(Portal Shape)]
        _Radius ("Hole Radius", Range(0.05, 0.48)) = 0.28
        _FogWidth ("Fog Ring Width", Range(0.02, 0.35)) = 0.16
        _EdgeSoft ("Edge Softness", Range(0.001, 0.08)) = 0.03
        _Open ("Open Amount", Range(0, 1)) = 1

        [Header(Portal Core)]
        [HDR] _PortalColor ("Portal Color", Color) = (0.2, 0.7, 1.4, 1)
        [HDR] _RimGlow ("Rim Glow", Color) = (0.45, 1.0, 1.5, 1)
        _CoreAlpha ("Core Alpha", Range(0, 1)) = 0.55

        [Header(Black Fog)]
        _FogColor ("Fog Color", Color) = (0.02, 0.02, 0.03, 1)
        _FogDensity ("Fog Density", Range(0, 3)) = 1.8
        _FogScale ("Fog Scale", Range(1, 20)) = 6
        _FogSpeed ("Fog Speed", Range(0, 3)) = 0.55
        _FogTurbulence ("Fog Turbulence", Range(0, 2)) = 1.0

        [Header(Space Warp)]
        [Toggle] _UseSceneWarp ("Use Scene Warp", Float) = 0
        _WarpStrength ("Warp Strength", Range(0, 0.2)) = 0.07
        _WarpWidth ("Warp Ring Width", Range(0.02, 0.45)) = 0.22
        _WarpScale ("Warp Noise Scale", Range(1, 20)) = 5
        _WarpSpeed ("Warp Speed", Range(0, 4)) = 0.7
        _Chromatic ("Warp Chromatic", Range(0, 0.04)) = 0.012
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "IgnoreProjector" = "True"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off

        Pass
        {
            Name "CircularPortal2D"
            Tags { "LightMode" = "Universal2D" }
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"
            #include "CircularPortalCommon.hlsl"
            ENDHLSL
        }

        Pass
        {
            Name "CircularPortalForward"
            Tags { "LightMode" = "UniversalForward" }
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"
            #include "CircularPortalCommon.hlsl"
            ENDHLSL
        }
    }

    Fallback "Sprites/Default"
}
