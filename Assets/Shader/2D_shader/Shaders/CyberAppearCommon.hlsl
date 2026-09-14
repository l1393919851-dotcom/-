#ifndef CYBER_APPEAR_COMMON_INCLUDED
#define CYBER_APPEAR_COMMON_INCLUDED

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
    UNITY_VERTEX_OUTPUT_STEREO
};

TEXTURE2D(_MainTex);
SAMPLER(sampler_MainTex);

CBUFFER_START(UnityPerMaterial)
    float4 _MainTex_ST;
    float4 _MainTex_TexelSize;
    half4  _Color;
    half   _Appear;
    half4  _GhostColor;
    half4  _EdgeColor;
    half   _GhostMix;
    half   _EdgeWidth;
    half   _SliceCount;
    half   _SliceAmount;
    half   _Misalign;
    half   _ChromaBoost;
    half   _BlurStrength;
    half   _Jitter;
    half   _DefocusHold;
CBUFFER_END

float Hash21(float2 p)
{
    p = frac(p * float2(123.34, 456.21));
    p += dot(p, p + 45.32);
    return frac(p.x * p.y);
}

void AppearCurves(half t, out half alpha, out half defocus, out half ghost, out half edge)
{
    t = saturate(t);
    half hold = saturate(_DefocusHold);

    alpha = smoothstep(0.0h, 0.35h, t);

    half rise = smoothstep(0.0h, 0.1h, t);
    half fall = 1.0h - smoothstep(hold, 1.0h, t);
    defocus = rise * fall;

    ghost = 1.0h - smoothstep(hold * 0.5h, 0.92h, t);
    edge  = saturate(1.0h - abs(t - (hold * 0.55h + 0.12h)) / max(_EdgeWidth, 0.01h));
    edge *= fall;
}

// 一条一条的横向条带错位 + RGB 分色重影（UV 空间，跟贴图分辨率无关）
half4 SampleStripMisalign(float2 uv, half amt, float n)
{
    float bands = max(_SliceCount, 2.0);
    float bandId = floor(uv.y * bands);
    // 时间微抖：条带偶尔跳一下
    float flicker = floor(n * lerp(2.0, 14.0, _Jitter));
    float h = Hash21(float2(bandId, flicker));
    float h2 = Hash21(float2(bandId + 17.0, flicker * 0.37));

    // 每条左右错开，幅度按 UV
    float stripShift = (h - 0.5) * 2.0 * _SliceAmount * amt;
    // 隔一条再加强，条带感更明显
    if (fmod(bandId, 2.0) < 0.5)
        stripShift *= 1.35;

    float rgbSplit = _Misalign * amt;
    float2 offR = float2(stripShift + rgbSplit, (h2 - 0.5) * 0.01 * amt);
    float2 offG = float2(stripShift * 0.25, 0);
    float2 offB = float2(stripShift - rgbSplit, (0.5 - h2) * 0.01 * amt);

    half4 sR = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + offR);
    half4 sG = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + offG);
    half4 sB = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + offB);
    half4 s0 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);

    // 主图按条带整体横移
    half4 sShift = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + float2(stripShift, 0));

    // 轻度模糊
    float2 b = float2(_BlurStrength, _BlurStrength) * amt;
    half4 blur =
        SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + float2(b.x + stripShift, 0)) +
        SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + float2(-b.x + stripShift, 0)) +
        SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + float2(stripShift, b.y)) +
        SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + float2(stripShift, -b.y));
    blur *= 0.25h;

    // 灰色贴图拆通道也几乎没颜色，所以用 alpha/亮度把重影染成纯红 / 纯青
    half lumR = max(sR.a, max(sR.r, max(sR.g, sR.b)));
    half lumB = max(sB.a, max(sB.r, max(sB.g, sB.b)));
    half boost = _ChromaBoost * amt;

    half3 rgb = lerp(s0.rgb, sShift.rgb, amt * 0.45h);
    rgb.r = lerp(rgb.r, sR.r, amt);
    rgb.g = lerp(rgb.g, sG.g, amt);
    rgb.b = lerp(rgb.b, sB.b, amt);

    rgb += half3(1.0h, 0.08h, 0.12h) * lumR * boost;
    rgb += half3(0.05h, 0.95h, 1.0h) * lumB * boost;
    rgb = lerp(rgb, blur.rgb, amt * 0.25h);

    half a = max(max(s0.a, sShift.a), max(sR.a, sB.a));
    a = lerp(s0.a, a, amt);
    return half4(saturate(rgb), a);
}

Varyings vert(Attributes v)
{
    Varyings o = (Varyings)0;
    UNITY_SETUP_INSTANCE_ID(v);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
    o.positionCS = TransformObjectToHClip(v.positionOS);
    o.uv = TRANSFORM_TEX(v.uv, _MainTex);
    o.color = v.color * _Color;
    return o;
}

half4 frag(Varyings i) : SV_Target
{
    half alphaCurve, defocus, ghostCurve, edgeCurve;
    AppearCurves(_Appear, alphaCurve, defocus, ghostCurve, edgeCurve);

    float n = _Time.y * 10.0 + _Appear * 40.0;
    half4 tex = SampleStripMisalign(i.uv, defocus, n);

    half3 rgb = tex.rgb * i.color.rgb;
    rgb = lerp(rgb, rgb * _GhostColor.rgb, ghostCurve * _GhostMix);
    rgb += _EdgeColor.rgb * edgeCurve * tex.a;

    half a = tex.a * i.color.a * alphaCurve;
    a *= lerp(0.4h, 1.0h, saturate((_Appear - 0.05h) / 0.5h));
    return half4(rgb, a);
}

#endif
