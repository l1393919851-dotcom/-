Shader "SBPK/StylizedOcean"
{
    Properties
    {
        _MainTex ("Water Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1, 1, 1, 1)
        _WorldTiling ("Meters Per Tile", Float) = 24
        _Scroll1 ("Scroll Layer1 XY", Vector) = (0.03, 0.015, 0, 0)
        _Scroll2 ("Scroll Layer2 XY", Vector) = (-0.02, 0.028, 0, 0)
        _Layer2Scale ("Layer2 UV Scale", Float) = 1.65
        _Layer2Mix ("Layer2 Mix", Range(0, 1)) = 0.4
        _WaveHeight ("Wave Height", Float) = 0.12
        _WaveFreq ("Wave Frequency", Float) = 0.12
        _WaveSpeed ("Wave Speed", Float) = 1.1
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Opaque"
            "Queue" = "Geometry"
        }

        Pass
        {
            Name "StylizedOcean"
            Tags { "LightMode" = "UniversalForward" }

            Cull Off
            ZWrite On

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _Color;
                float4 _Scroll1;
                float4 _Scroll2;
                float _WorldTiling;
                float _Layer2Scale;
                float _Layer2Mix;
                float _WaveHeight;
                float _WaveFreq;
                float _WaveSpeed;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float fogFactor : TEXCOORD1;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);

                float t = _Time.y * _WaveSpeed;
                float wave =
                    sin((positionWS.x + positionWS.z) * _WaveFreq + t) * 0.55 +
                    sin((positionWS.x * 0.7 - positionWS.z * 1.1) * (_WaveFreq * 1.4) + t * 1.3) * 0.45;
                positionWS.y += wave * _WaveHeight;

                output.positionWS = positionWS;
                output.positionCS = TransformWorldToHClip(positionWS);
                output.fogFactor = ComputeFogFactor(output.positionCS.z);
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float tiling = max(_WorldTiling, 0.001);
                float2 uv1 = input.positionWS.xz / tiling + _Scroll1.xy * _Time.y;
                float2 uv2 = input.positionWS.xz / (tiling / max(_Layer2Scale, 0.001)) + _Scroll2.xy * _Time.y;

                half4 c1 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv1);
                half4 c2 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv2);
                half4 col = lerp(c1, c2, _Layer2Mix) * _Color;
                col.rgb = MixFog(col.rgb, input.fogFactor);
                return half4(col.rgb, 1);
            }
            ENDHLSL
        }
    }

    FallBack Off
}
