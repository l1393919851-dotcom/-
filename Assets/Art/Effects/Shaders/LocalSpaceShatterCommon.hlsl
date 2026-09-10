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
    half   _Hole;
    half   _Radius;
    half   _EdgeSoft;
    half   _CellDensity;
    half   _ShatterAmount;
    half   _CrackWidth;
    half   _CrackDepth;
    half4  _CrackColor;
    half   _Rotation;
    half   _Chromatic;
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
    float intensity = saturate(_Intensity);
    float hole = saturate(_Hole);
    if (intensity <= 0.001 && hole <= 0.001)
        return half4(0, 0, 0, 0);

    float2 uv = i.uv;
    float2 p = uv * 2.0 - 1.0;
    float dist = length(p);

    float outer = _Radius;
    float soft = max(_EdgeSoft, 0.01);
    float disk = 1.0 - smoothstep(outer - soft, outer, dist);
    if (disk <= 0.001)
        return half4(0, 0, 0, 0);

    // 中心揭开：洞越大，越能直接看到后方怪物
    float holeRadius = lerp(0.0, outer * 0.92, hole);
    float holeMask = 1.0 - smoothstep(holeRadius - soft * 0.8, holeRadius + soft * 0.2, dist);
    // 洞完全透明
    if (holeMask > 0.98 && hole > 0.05)
        return half4(0, 0, 0, 0);

    float breakAmt = intensity * disk;

    float4 vor = Voronoi(uv, _CellDensity);
    float edge = vor.x;
    float2 cellId = vor.yz;
    float2 rnd = Hash22(cellId);

    float2 center = float2(0.5, 0.5);
    float2 radial = normalize(uv - center + 1e-5);
    float2 shardDir = normalize((rnd * 2.0 - 1.0) + radial * 0.7);

    float rot = (rnd.x - 0.5) * 6.2831853 * _Rotation * breakAmt;
    float2 local = uv - center;
    float2 rotated = Rotate(local, rot) + center;
    float2 offset = shardDir * (_ShatterAmount * breakAmt);

    float2 screenUV = i.screenPos.xy / max(i.screenPos.w, 1e-5);
    // 局部破碎：屏幕 UV 跟随碎片偏移
    float2 shatteredScreen = screenUV + offset * float2(1.0, _ScreenParams.x / max(_ScreenParams.y, 1.0));
    // 略带旋转感
    shatteredScreen += (rotated - uv) * 0.35 * breakAmt;

    float crack = pow(1.0 - smoothstep(0.0, _CrackWidth, edge), 1.3) * breakAmt;
    float chroma = _Chromatic * breakAmt;

    float3 col;
    col.r = SampleSceneColor(shatteredScreen + float2(-chroma, 0)).r;
    col.g = SampleSceneColor(shatteredScreen).g;
    col.b = SampleSceneColor(shatteredScreen + float2(chroma, 0)).b;

    float3 voidCol = lerp(_VoidColor.rgb, _CrackColor.rgb, 0.2);
    col = lerp(col, voidCol, crack * _CrackDepth);
    col += _CrackColor.rgb * crack * 0.65;

    // 洞边缘：碎裂环，洞内透明
    float ringKeep = 1.0 - holeMask;
    float alpha = saturate(breakAmt * 0.95 + crack * 0.4) * ringKeep * disk;
    // 洞刚打开时，洞边加一圈裂光
    float rim = saturate(1.0 - abs(dist - holeRadius) / max(soft * 1.5, 0.01)) * hole;
    col += _CrackColor.rgb * rim * 0.8;
    alpha = max(alpha, rim * 0.7);

    alpha *= i.color.a;
    col *= i.color.rgb;

    return half4(saturate(col), saturate(alpha));
}

#endif
