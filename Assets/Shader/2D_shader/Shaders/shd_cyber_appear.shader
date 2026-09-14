Shader "URP/2D/Sprite-Cyber-Appear"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1, 1, 1, 1)

        [Header(Appear)]
        _Appear ("Appear", Range(0, 1)) = 0
        [HDR] _GhostColor ("Ghost Color", Color) = (0.35, 0.95, 1.2, 1)
        [HDR] _EdgeColor ("Edge Flash", Color) = (1.2, 0.35, 0.95, 1)
        _GhostMix ("Ghost Mix", Range(0, 1)) = 0.75
        _EdgeWidth ("Edge Width", Range(0.01, 0.5)) = 0.22

        [Header(Blur And Misalign)]
        // 以下偏移都是 UV 比例（0.1 ≈ 图宽 10%），条带错位会非常明显
        _SliceCount ("Strip Count", Range(2, 32)) = 14
        _SliceAmount ("Strip Shift", Range(0, 0.55)) = 0.28
        _Misalign ("RGB Split", Range(0, 0.3)) = 0.14
        _ChromaBoost ("Red/Cyan Boost", Range(0, 3)) = 1.6
        _BlurStrength ("Blur Strength", Range(0, 0.08)) = 0.025
        _Jitter ("Strip Flicker", Range(0, 1)) = 0.55
        _DefocusHold ("Defocus Hold", Range(0.2, 0.85)) = 0.62
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

        Cull Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            Name "CyberAppear"
            Tags { "LightMode" = "Universal2D" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "CyberAppearCommon.hlsl"
            ENDHLSL
        }

        Pass
        {
            Name "CyberAppearForward"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "CyberAppearCommon.hlsl"
            ENDHLSL
        }
    }

    Fallback "Sprites/Default"
}
