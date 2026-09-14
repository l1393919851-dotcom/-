#ifndef LOCAL_SPACE_SHATTER_COMMON_INCLUDED
#define LOCAL_SPACE_SHATTER_COMMON_INCLUDED

struct Attributes
{
    float3 positionOS : POSITION;
    float4 color : COLOR;
    float2 uv : TEXCOORD0;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Varyings
{
    float4 positionCS : SV_POSITION;
    half4 color : COLOR;
    float2 uv : TEXCOORD0;
    float4 screenPos : TEXCOORD1;
    UNITY_VERTEX_OUTPUT_STEREO
};

TEXTURE2D(_MainTex);
SAMPLER(sampler_MainTex);

CBUFFER_START(UnityPerMaterial)
    float4 _MainTex_ST;
    half4  _Color;
    half   _Intensity;
    half   _Expand;
    half   _Hole;
    half   _Stage;
    half   _Radius;
    half   _StartRadius;
    half   _EdgeSoft;
    half   _FrontierWidth;
    half   _CellDensity;
    half   _ShatterAmount;
    half   _CrackWidth;
    half   _CrackDepth;
    half4  _CrackColor;
    half   _Rotation;
    half   _Chromatic;
    half4  _VoidColor;
    half   _UseSceneWarp;
    half   _FillOpacity;
    half   _GlassTint;
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

float4 Voronoi(float2 uv, float density)
{
    float2 g = floor(uv * density);
    float2 f = frac(uv * density);
    float minD = 8.0;
    float minD2 = 8.0;
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
    float s, c;
    sincos(a, s, c);
    return float2(c * v.x - s * v.y, s * v.x + c * v.y);
}

// 放射状玻璃裂纹（从中心炸开的线）
float RadialCracks(float2 p, float expand, float stage)
{
    float ang = atan2(p.y, p.x);
    float dist = length(p);

    // 阶段越高，裂纹条数越多
    float spokes = lerp(5.0, 14.0, saturate((stage - 1.0) / 3.0));
    float a = ang / 6.2831853;
    float spoke = abs(frac(a * spokes + Hash21(float2(floor(a * spokes), stage)) * 0.15) - 0.5);
    float spokeLine = 1.0 - smoothstep(0.0, 0.018 + expand * 0.01, spoke);

    // 环向裂纹（同心碎痕），后期才明显
    float rings = lerp(1.0, 5.0, expand);
    float ring = abs(frac(dist * rings * 2.2 + stage * 0.17) - 0.5);
    float ringLine = (1.0 - smoothstep(0.0, 0.03, ring)) * saturate(expand * 1.4);

    float mask = 1.0 - smoothstep(expand * 0.55, expand * 0.55 + 0.08, dist);
    return saturate(spokeLine * 1.2 + ringLine * 0.85) * mask;
}

Varyings vert(Attributes v)
{
    Varyings o = (Varyings)0;
    UNITY_SETUP_INSTANCE_ID(v);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
    o.positionCS = TransformObjectToHClip(v.positionOS);
    o.uv = TRANSFORM_TEX(v.uv, _MainTex);
    o.color = v.color * _Color;
    o.screenPos = ComputeScreenPos(o.positionCS);
    return o;
}

half4 frag(Varyings i) : SV_Target
{
    float intensity = max(saturate(_Intensity), 0.001);
    float expand = saturate(_Expand);
    float hole = saturate(_Hole);
    float stage = max(_Stage, 1.0);

    if (expand <= 0.001 && hole <= 0.001)
        return half4(0, 0, 0, 0);

    float2 uv = i.uv;
    float2 p = uv * 2.0 - 1.0;
    float dist = length(p);

    float soft = max(_EdgeSoft, 0.02);
    float startR = max(_StartRadius, 0.05);
    float maxR = max(_Radius, startR + 0.08);
    float growR = lerp(startR, maxR, expand);

    float ang = atan2(p.y, p.x);
    float jagged = Hash21(float2(floor(ang * 10.0), stage));
    float growRJagged = growR * lerp(0.88, 1.08, jagged);

    if (dist > growRJagged + soft && hole < 0.01)
        return half4(0, 0, 0, 0);

    float maxDisk = 1.0 - smoothstep(maxR - soft, maxR, dist);
    float inside = 1.0 - smoothstep(growRJagged - soft, growRJagged, dist);

    float holeRadius = lerp(0.0, growR * 0.9, hole);
    float holeMask = 1.0 - smoothstep(holeRadius - soft * 0.8, holeRadius + soft * 0.2, dist);
    if (holeMask > 0.98 && hole > 0.05)
        return half4(0, 0, 0, 0);

    float breakAmt = intensity * inside;

    // 阶段越高：Voronoi 越碎、错位越大（阶跃感靠脚本改 expand/stage，这里跟 stage 绑定）
    float stage01 = saturate((stage - 1.0) / 3.0);
    float density = lerp(_CellDensity * 0.45, _CellDensity * 1.15, stage01);
    float4 vor = Voronoi(uv + stage * 0.03, density);
    float edge = vor.x;
    float2 cellId = vor.yz;
    float2 rnd = Hash22(cellId + stage);

    float2 center = float2(0.5, 0.5);
    float2 radial = normalize(uv - center + 1e-5);
    float2 shardDir = normalize((rnd * 2.0 - 1.0) + radial * 0.55);

    float shardMul = lerp(0.15, 1.0, stage01); // 前期几乎只裂不开，后期才错位
    float rot = (rnd.x - 0.5) * 6.2831853 * _Rotation * breakAmt * shardMul * stage01;
    float2 local = uv - center;
    float2 rotated = Rotate(local, rot) + center;
    float2 offset = shardDir * (_ShatterAmount * breakAmt * shardMul);

    float voronoiCrack = 1.0 - smoothstep(0.0, max(_CrackWidth, 0.02), edge);
    voronoiCrack = pow(saturate(voronoiCrack), 1.05) * breakAmt;

    float radialCrack = RadialCracks(p / max(growRJagged, 0.05), expand, stage) * breakAmt;

    // 玻璃感：前期几乎透明，只有裂纹线；后期才有碎块填充/错位
    float glassClear = lerp(0.08, max(_FillOpacity, 0.35), stage01 * stage01);
    float3 col = lerp(float3(0.75, 0.85, 0.95) * _GlassTint, _VoidColor.rgb, glassClear * breakAmt);

    if (_UseSceneWarp > 0.5)
    {
        float2 screenUV = i.screenPos.xy / max(i.screenPos.w, 1e-5);
        float2 shatteredScreen = screenUV + offset * float2(1.0, _ScreenParams.x / max(_ScreenParams.y, 1.0));
        shatteredScreen += (rotated - uv) * 0.25 * breakAmt * shardMul;
        float chroma = _Chromatic * breakAmt * stage01;
        float3 scene;
        scene.r = SampleSceneColor(shatteredScreen + float2(-chroma, 0)).r;
        scene.g = SampleSceneColor(shatteredScreen).g;
        scene.b = SampleSceneColor(shatteredScreen + float2(chroma, 0)).b;
        col = lerp(col, scene, lerp(0.15, 0.55, stage01));
    }

    float crack = saturate(voronoiCrack * lerp(0.35, 1.0, stage01) + radialCrack * lerp(1.2, 0.7, stage01));
    col = lerp(col, _VoidColor.rgb * 0.15, crack * _CrackDepth);
    col += _CrackColor.rgb * crack * lerp(0.9, 1.3, stage01);

    // 碎块边缘高光（后期）
    col += _CrackColor.rgb * voronoiCrack * stage01 * 0.35;

    float ringKeep = 1.0 - holeMask;
    float alpha = saturate(crack * 1.15 + glassClear * breakAmt * 0.65) * ringKeep * maxDisk;
    // 保证裂纹线可见
    alpha = max(alpha, crack * 0.95 * ringKeep);

    float rim = saturate(1.0 - abs(dist - holeRadius) / max(soft * 1.5, 0.01)) * hole;
    col += _CrackColor.rgb * rim * 0.8;
    alpha = max(alpha, rim * 0.7);

    alpha *= i.color.a;
    col *= i.color.rgb;
    return half4(saturate(col), saturate(alpha));
}

#endif
