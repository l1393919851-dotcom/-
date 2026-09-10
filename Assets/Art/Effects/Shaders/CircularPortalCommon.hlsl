#ifndef CIRCULAR_PORTAL_COMMON_INCLUDED
#define CIRCULAR_PORTAL_COMMON_INCLUDED

struct Attributes
{
    float3 positionOS : POSITION;
    float4 color      : COLOR;
    float2 uv         : TEXCOORD0;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Varyings
{
    float4 positionCS : SV_POSITION;
    half4  color      : COLOR;
    float2 uv         : TEXCOORD0;
    float2 uv01       : TEXCOORD1;
    float4 screenPos  : TEXCOORD2;
    UNITY_VERTEX_OUTPUT_STEREO
};

TEXTURE2D(_MainTex);
SAMPLER(sampler_MainTex);

CBUFFER_START(UnityPerMaterial)
    float4 _MainTex_ST;
    float4 _SpriteUVRect;
    half4  _Color;
    half   _Radius;
    half   _FogWidth;
    half   _EdgeSoft;
    half   _Open;
    half4  _PortalColor;
    half4  _RimGlow;
    half   _CoreAlpha;
    half4  _FogColor;
    half   _FogDensity;
    half   _FogScale;
    half   _FogSpeed;
    half   _FogTurbulence;
    half   _UseSceneWarp;
    half   _WarpStrength;
    half   _WarpWidth;
    half   _WarpScale;
    half   _WarpSpeed;
    half   _Chromatic;
CBUFFER_END

float Hash21(float2 p)
{
    p = frac(p * float2(123.34, 456.21));
    p += dot(p, p + 45.32);
    return frac(p.x * p.y);
}

float Noise2D(float2 p)
{
    float2 i = floor(p);
    float2 f = frac(p);
    f = f * f * (3.0 - 2.0 * f);
    float a = Hash21(i);
    float b = Hash21(i + float2(1, 0));
    float c = Hash21(i + float2(0, 1));
    float d = Hash21(i + float2(1, 1));
    return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
}

float FBM(float2 p)
{
    float v = 0.0;
    float a = 0.5;
    [unroll]
    for (int i = 0; i < 4; i++)
    {
        v += Noise2D(p) * a;
        p = p * 2.05 + 17.0;
        a *= 0.5;
    }
    return v;
}

float2 ToSpriteUV01(float2 uv)
{
    float2 mn = _SpriteUVRect.xy;
    float2 mx = _SpriteUVRect.zw;
    // 未设置时默认整张 0-1
    if (mx.x <= mn.x || mx.y <= mn.y)
        return saturate(uv);
    return saturate((uv - mn) / max(mx - mn, float2(1e-5, 1e-5)));
}

Varyings vert(Attributes v)
{
    Varyings o = (Varyings)0;
    UNITY_SETUP_INSTANCE_ID(v);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
    o.positionCS = TransformObjectToHClip(v.positionOS);
    o.uv = TRANSFORM_TEX(v.uv, _MainTex);
    o.uv01 = ToSpriteUV01(v.uv);
    o.color = v.color * _Color;
    o.screenPos = ComputeScreenPos(o.positionCS);
    return o;
}

half4 frag(Varyings i) : SV_Target
{
    float openAmt = saturate(_Open);
    if (openAmt <= 0.001)
        return half4(0, 0, 0, 0);

    // 形状完全由程序圆决定，不再乘 Sprite.a
    // （Circle 空心贴图会把整扇门裁成细环，看起来像“全透明”）
    float2 p = i.uv01 * 2.0 - 1.0;
    float dist = length(p);

    float radius = max(_Radius * openAmt, 0.05);
    float fogW = max(_FogWidth, 0.08);
    float fogOuter = radius + fogW;
    float soft = max(_EdgeSoft, 0.015);
    float warpW = max(_WarpWidth, 0.08);
    float maxR = fogOuter + warpW;

    if (dist > maxR)
        return half4(0, 0, 0, 0);

    float t = _Time.y;
    float ang = atan2(p.y, p.x);

    // 黑雾
    float2 fogCoord = float2(ang * 1.2, dist * _FogScale) + float2(t * _FogSpeed, t * _FogSpeed * 0.3);
    float fogN = FBM(fogCoord + FBM(fogCoord) * _FogTurbulence);
    float ring = smoothstep(radius - soft, radius + soft * 0.5, dist)
               * (1.0 - smoothstep(radius + fogW * 0.15, fogOuter, dist));
    float fogMask = saturate(ring * lerp(0.65, 1.0, fogN) * max(_FogDensity, 1.2) * openAmt);

    // 扭曲带
    float warpRing = smoothstep(radius, radius + soft, dist)
                   * (1.0 - smoothstep(fogOuter, maxR, dist));

    float2 wUV = p * _WarpScale + float2(t * _WarpSpeed, -t * _WarpSpeed * 0.55);
    float2 warpDir = float2(FBM(wUV), FBM(wUV + 5.2)) * 2.0 - 1.0;
    warpDir = normalize(warpDir + float2(-p.y, p.x) * 0.8 + 1e-5);

    float2 screenUV = i.screenPos.xy / max(i.screenPos.w, 1e-5);
    float2 warpedUV = screenUV + warpDir * (_WarpStrength * warpRing * openAmt);
    float chroma = _Chromatic * warpRing * openAmt;

    float swirl = FBM(float2(ang * 2.0 - t * 1.4, dist * 7.0 - t));
    float3 fakeWarp = lerp(float3(0.02, 0.02, 0.03), _RimGlow.rgb * 0.4, swirl);

    float3 warpCol = fakeWarp;
    if (_UseSceneWarp > 0.5)
    {
        float3 s;
        s.r = SampleSceneColor(warpedUV + float2(-chroma, 0)).r;
        s.g = SampleSceneColor(warpedUV).g;
        s.b = SampleSceneColor(warpedUV + float2(chroma, 0)).b;
        warpCol = lerp(fakeWarp, s, 0.8);
    }

    float hole = 1.0 - smoothstep(radius - soft, radius, dist);
    float rim = saturate(1.0 - abs(dist - radius) / max(fogW * 0.55, 0.02)) * openAmt;

    float3 col = 0;
    col = lerp(col, warpCol, warpRing);
    col = lerp(col, _FogColor.rgb, fogMask);
    col = lerp(col, _PortalColor.rgb, hole * max(_CoreAlpha, 0.5));
    col += _RimGlow.rgb * rim * 0.9;
    col *= i.color.rgb;

    float alpha = max(fogMask, hole * max(_CoreAlpha, 0.5));
    alpha = max(alpha, warpRing * 0.9);
    alpha = saturate(alpha * openAmt) * i.color.a;

    return half4(saturate(col), alpha);
}

#endif
