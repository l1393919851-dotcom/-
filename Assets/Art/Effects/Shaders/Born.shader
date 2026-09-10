Shader "URP/2D/CyberPsychosis"
{
    Properties
    {
        _MainTex ("Screen Texture", 2D) = "white" {}
        _Intensity ("Effect Intensity", Range(0, 1)) = 0
        _RGBSplit ("RGB Separation", Range(0, 0.1)) = 0.02
        _Distortion ("Distortion", Range(0, 0.1)) = 0.02
        _Scanline ("Scanline", Range(0, 1)) = 0.15
        _Noise ("Noise", Range(0, 1)) = 0.1
        _Blur ("Blur", Range(0, 0.02)) = 0.005
        _Vignette ("Vignette", Range(0, 1)) = 0.2
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Overlay"
        }

        Pass
        {
            Name "CyberPsychosis"

            ZWrite Off
            ZTest Always
            Cull Off

            HLSLPROGRAM

            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D_X(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float _Intensity;
                float _RGBSplit;
                float _Distortion;
                float _Scanline;
                float _Noise;
                float _Blur;
                float _Vignette;
            CBUFFER_END

            struct Attributes
            {
                uint vertexID : SV_VertexID;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;

                output.positionCS = GetFullScreenTriangleVertexPosition(
                    input.vertexID
                );

                output.uv = GetFullScreenTriangleTexCoord(
                    input.vertexID
                );

                return output;
            }

            // 简单伪随机
            float Hash(float2 p)
            {
                return frac(
                    sin(dot(p, float2(12.9898, 78.233))) * 43758.5453
                );
            }

            // 平滑噪声
            float Noise2D(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);

                f = f * f * (3.0 - 2.0 * f);

                float a = Hash(i);
                float b = Hash(i + float2(1, 0));
                float c = Hash(i + float2(0, 1));
                float d = Hash(i + float2(1, 1));

                return lerp(
                    lerp(a, b, f.x),
                    lerp(c, d, f.x),
                    f.y
                );
            }

            // 时间
            float GetTime()
            {
                return _Time.y;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float2 uv = input.uv;
                float2 originalUV = uv;

                float t = GetTime();

                // 1. 整体强度
                float intensity = _Intensity;

                // 2. 横向撕裂
                // 不同高度的画面向不同方向偏移
                float lineNoise = Noise2D(
                    float2(
                        floor(uv.y * 80.0),
                        floor(t * 12.0)
                    )
                );

                float tear = (lineNoise - 0.5)
                    * _Distortion
                    * intensity;

                // 偶尔出现更强烈的错位
                float glitchBlock = step(
                    0.72,
                    Noise2D(float2(floor(t * 8.0), 0.0))
                );

                tear *= lerp(1.0, 3.0, glitchBlock);

                uv.x += tear;

                // 3. 局部失焦 / 波动
                float wave = sin(
                    uv.y * 35.0 +
                    t * 8.0
                );

                uv.x += wave
                    * 0.003
                    * intensity;

                // 4. RGB 色彩分离
                float rgbOffset = _RGBSplit * intensity;

                // 红色通道向左
                float2 uvR = uv;
                uvR.x -= rgbOffset;

                // 蓝色通道向右
                float2 uvB = uv;
                uvB.x += rgbOffset;

                // 绿色保持中间
                float2 uvG = uv;

                // 5. 简单模糊
                float blur = _Blur * intensity;

                float3 color = 0;

                // 中心采样
                color.r = SAMPLE_TEXTURE2D_X(
                    _MainTex,
                    sampler_MainTex,
                    uvR
                ).r;

                color.g = SAMPLE_TEXTURE2D_X(
                    _MainTex,
                    sampler_MainTex,
                    uvG
                ).g;

                color.b = SAMPLE_TEXTURE2D_X(
                    _MainTex,
                    sampler_MainTex,
                    uvB
                ).b;

                // 6. 轻微失焦
                float3 blurColor = 0;

                blurColor += SAMPLE_TEXTURE2D_X(
                    _MainTex,
                    sampler_MainTex,
                    uv + float2(blur, 0)
                ).rgb;

                blurColor += SAMPLE_TEXTURE2D_X(
                    _MainTex,
                    sampler_MainTex,
                    uv - float2(blur, 0)
                ).rgb;

                blurColor += SAMPLE_TEXTURE2D_X(
                    _MainTex,
                    sampler_MainTex,
                    uv + float2(0, blur)
                ).rgb;

                blurColor += SAMPLE_TEXTURE2D_X(
                    _MainTex,
                    sampler_MainTex,
                    uv - float2(0, blur)
                ).rgb;

                blurColor *= 0.25;

                color = lerp(
                    color,
                    blurColor,
                    intensity * 0.35
                );

                // 7. 扫描线
                float scanline = sin(
                    uv.y * 900.0
                );

                color *= 1.0
                    - scanline * _Scanline * intensity;

                // 8. 噪声
                float noise = Hash(
                    uv * 1000.0 +
                    t * 100.0
                );

                color += (noise - 0.5)
                    * _Noise
                    * intensity;

                // 9. 暗角
                float2 vignetteUV = uv - 0.5;

                float vignette = 1.0
                    - dot(vignetteUV, vignetteUV)
                    * _Vignette
                    * intensity;

                color *= vignette;

                // 10. 颜色增强
                color = lerp(
                    color,
                    color * float3(1.1, 0.95, 1.15),
                    intensity * 0.25
                );

                return half4(
                    saturate(color),
                    1.0
                );
            }

            ENDHLSL
        }
    }
}
