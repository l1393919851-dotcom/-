Shader "URP/Effects/SpaceShatter"
{
    Properties
    {
        _MainTex ("Unused", 2D) = "white" {}
        _Intensity ("Intensity", Range(0, 1)) = 0

        [Header(Shatter Cells)]
        _CellDensity ("Cell Density", Range(2, 40)) = 12
        _ShatterAmount ("Shard Offset", Range(0, 0.25)) = 0.08
        _CrackWidth ("Crack Width", Range(0.001, 0.08)) = 0.018
        _CrackDepth ("Crack Darkness", Range(0, 1)) = 0.85
        [HDR] _CrackColor ("Crack Glow", Color) = (0.3, 0.85, 1.2, 1)

        [Header(Break Pattern)]
        _Epicenter ("Epicenter (UV)", Vector) = (0.5, 0.5, 0, 0)
        _Radius ("Break Radius", Range(0.05, 1.5)) = 0.85
        _EdgeSoft ("Radius Softness", Range(0.01, 0.5)) = 0.25
        _Rotation ("Shard Rotate", Range(0, 1)) = 0.35
        _DepthPop ("Shard Depth Pop", Range(0, 0.15)) = 0.04

        [Header(Look)]
        _Chromatic ("Chromatic", Range(0, 0.05)) = 0.015
        _Desaturate ("Crack Desaturate", Range(0, 1)) = 0.35
        _Vignette ("Vignette", Range(0, 1)) = 0.2
        [HDR] _VoidColor ("Void Color", Color) = (0.01, 0.01, 0.02, 1)
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "IgnoreProjector" = "True"
        }

        Pass
        {
            Name "SpaceShatter"
            Tags { "LightMode" = "UniversalForward" }

            ZWrite Off
            ZTest Always
            Cull Off
            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 screenPos : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            CBUFFER_START(UnityPerMaterial)
                half   _Intensity;
                half   _CellDensity;
                half   _ShatterAmount;
                half   _CrackWidth;
                half   _CrackDepth;
                half4  _CrackColor;
                float4 _Epicenter;
                half   _Radius;
                half   _EdgeSoft;
                half   _Rotation;
                half   _DepthPop;
                half   _Chromatic;
                half   _Desaturate;
                half   _Vignette;
                half4  _VoidColor;
            CBUFFER_END

            float2 Hash22(float2 p)
            {
                float3 p3 = frac(float3(p.xyx) * float3(0.1031, 0.1030, 0.0973));
                p3 += dot(p3, p3.yzx + 33.33);
                return frac((p3.xx + p3.yz) * p3.zy);
            }

            float Hash21(float2 p)
            {
                return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453);
            }

            // return: x = edge dist, yz = cell id-ish, w = cell random
            float4 Voronoi(float2 uv, float density)
            {
                float2 g = floor(uv * density);
                float2 f = frac(uv * density);

                float minD = 8.0;
                float minD2 = 8.0;
                float2 minOff = 0;
                float2 minId = 0;

                [unroll]
                for (int y = -1; y <= 1; y++)
                {
                    [unroll]
                    for (int x = -1; x <= 1; x++)
                    {
                        float2 cell = float2(x, y);
                        float2 rnd = Hash22(g + cell);
                        float2 r = cell + rnd - f;
                        float d = dot(r, r);
                        if (d < minD)
                        {
                            minD2 = minD;
                            minD = d;
                            minOff = r;
                            minId = g + cell;
                        }
                        else if (d < minD2)
                        {
                            minD2 = d;
                        }
                    }
                }

                float edge = sqrt(minD2) - sqrt(minD);
                return float4(edge, minId, Hash21(minId));
            }

            float2 Rotate(float2 v, float a)
            {
                float s = sin(a);
                float c = cos(a);
                return float2(c * v.x - s * v.y, s * v.x + c * v.y);
            }

            Varyings vert(Attributes v)
            {
                Varyings o = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.positionCS = TransformObjectToHClip(v.positionOS.xyz);
                o.uv = v.uv;
                o.screenPos = ComputeScreenPos(o.positionCS);
                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                float intensity = saturate(_Intensity);
                if (intensity <= 0.001)
                    return half4(0, 0, 0, 0);

                float2 screenUV = i.screenPos.xy / max(i.screenPos.w, 1e-5);
                float2 center = _Epicenter.xy;
                float dist = distance(screenUV, center);

                // 从震源向外碎裂
                float breakMask = 1.0 - smoothstep(_Radius * intensity, _Radius * intensity + _EdgeSoft, dist);
                breakMask = saturate(breakMask * intensity);
                if (breakMask <= 0.001)
                    return half4(0, 0, 0, 0);

                float4 vor = Voronoi(screenUV, _CellDensity);
                float edge = vor.x;
                float2 cellId = vor.yz;
                float cellRnd = vor.w;

                float2 rnd = Hash22(cellId);
                float2 shardDir = rnd * 2.0 - 1.0;
                // 略微沿径向甩开
                float2 radial = normalize(screenUV - center + 1e-5);
                shardDir = normalize(shardDir + radial * 0.65);

                float rot = (rnd.x - 0.5) * 6.2831853 * _Rotation * breakMask;
                float2 local = screenUV - center;
                float2 rotated = Rotate(local, rot) + center;

                float pop = (rnd.y - 0.5) * _DepthPop * breakMask;
                float2 offset = shardDir * (_ShatterAmount * breakMask) + radial * pop;

                float2 shatteredUV = rotated + offset;

                // 裂隙：Voronoi 边
                float crack = 1.0 - smoothstep(0.0, _CrackWidth, edge);
                crack = pow(saturate(crack), 1.35) * breakMask;

                float chroma = _Chromatic * breakMask;
                float3 col;
                col.r = SampleSceneColor(shatteredUV + float2(-chroma, chroma * 0.3)).r;
                col.g = SampleSceneColor(shatteredUV).g;
                col.b = SampleSceneColor(shatteredUV + float2(chroma, -chroma * 0.25)).b;

                // 裂缝里露出虚空 + 微光
                float3 voidCol = lerp(_VoidColor.rgb, _CrackColor.rgb, 0.15 + 0.35 * cellRnd);
                col = lerp(col, voidCol, crack * _CrackDepth);
                col += _CrackColor.rgb * crack * 0.55;

                float gray = dot(col, float3(0.299, 0.587, 0.114));
                col = lerp(col, gray.xxx, _Desaturate * breakMask * 0.5);

                float2 vigUV = screenUV - 0.5;
                col *= 1.0 - dot(vigUV, vigUV) * _Vignette * intensity;

                // 碎裂区域用较高 alpha 盖住原画面；裂缝更实
                float alpha = saturate(breakMask * 0.92 + crack * 0.35);
                return half4(saturate(col), alpha);
            }
            ENDHLSL
        }
    }

    Fallback Off
}
