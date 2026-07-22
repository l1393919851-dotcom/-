Shader "URP/2D/Sprite-Neon-Additive"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        [HDR] _GlowColor ("Glow Color", Color) = (1,1,1,1)
        _GlowIntensity ("Glow Intensity", Range(0, 3)) = 1.0
        _GlowSoftness ("Edge Softness", Range(0, 0.02)) = 0.005
    }

    SubShader
    {
        Tags {"Queue" = "Transparent" "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" "IgnoreProjector" = "True" "PreviewType" = "Plane"}

        Blend One One
        Cull Off
        ZWrite Off

        Pass
        {
            Tags { "LightMode" = "Universal2D" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float3 positionOS   : POSITION;
                float4 color        : COLOR;
                float2 uv           : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4  positionCS  : SV_POSITION;
                half4   color       : COLOR;
                float2  uv          : TEXCOORD0;
                float2  uvOffset    : TEXCOORD1;  // base uv offset for glow spread
                UNITY_VERTEX_OUTPUT_STEREO
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            float4 _MainTex_ST;
            float4 _MainTex_TexelSize;
            float4 _Color;
            float4 _GlowColor;
            float _GlowIntensity;
            float _GlowSoftness;

            Varyings vert(Attributes v)
            {
                Varyings o = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                o.positionCS = TransformObjectToHClip(v.positionOS);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color * _Color;

                // Precompute UV offset (in texel space)
                o.uvOffset = _MainTex_TexelSize.xy * _GlowSoftness * 100.0;

                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                // Center sample
                half4 center = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);

                // 4-tap cross for soft glow (use precomputed offset)
                float2 o = i.uvOffset;
                half4 n1 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + float2(o.x, 0));
                half4 n2 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv - float2(o.x, 0));
                half4 n3 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + float2(0, o.y));
                half4 n4 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv - float2(0, o.y));

                // Combine: take max brightness from all samples
                // This preserves sharp core while softening edges
                half4 glow = max(center, max(n1, max(n2, max(n3, n4))));

                // Apply glow color, intensity multiplier, and per-vertex tint
                glow.rgb *= _GlowColor.rgb * i.color.rgb * _GlowIntensity;

                return glow;
            }
            ENDHLSL
        }

        // Fallback pass for UniversalForward (backward compatibility)
        Pass
        {
            Tags { "LightMode" = "UniversalForward" "Queue" = "Transparent" "RenderType" = "Transparent"}

            Blend One One
            Cull Off
            ZWrite Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float3 positionOS   : POSITION;
                float4 color        : COLOR;
                float2 uv           : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4  positionCS  : SV_POSITION;
                half4   color       : COLOR;
                float2  uv          : TEXCOORD0;
                float2  uvOffset    : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            float4 _MainTex_ST;
            float4 _MainTex_TexelSize;
            float4 _Color;
            float4 _GlowColor;
            float _GlowIntensity;
            float _GlowSoftness;

            Varyings vert(Attributes v)
            {
                Varyings o = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.positionCS = TransformObjectToHClip(v.positionOS);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color * _Color;
                o.uvOffset = _MainTex_TexelSize.xy * _GlowSoftness * 100.0;
                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                half4 center = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                float2 o = i.uvOffset;
                half4 n1 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + float2(o.x, 0));
                half4 n2 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv - float2(o.x, 0));
                half4 n3 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + float2(0, o.y));
                half4 n4 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv - float2(0, o.y));
                half4 glow = max(center, max(n1, max(n2, max(n3, n4))));
                glow.rgb *= _GlowColor.rgb * i.color.rgb * _GlowIntensity;
                return glow;
            }
            ENDHLSL
        }
    }

    Fallback "Sprites/Default"
}
