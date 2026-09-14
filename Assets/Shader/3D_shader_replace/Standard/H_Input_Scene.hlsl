#ifndef UNIVERSAL_INPUT_SCENE_INCLUDED
#define UNIVERSAL_INPUT_SCENE_INCLUDED

    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/ParallaxMapping.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceData.hlsl"
	#include "../lib/ToonFog.hlsl"

	#ifdef _DYNAMIC_DISSOLVE_ON
		#include "H_DynamicWorldDissolve.hlsl"
	#endif

	#ifdef _DYETOGGLE_ON
		#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
	#endif

    CBUFFER_START(UnityPerMaterial)
        float4 _BaseMap_ST;
        half4 _BaseColor;
        half4 _EmissionColor;
        half _Cutoff;
        half _Smoothness;
        half _Metallic;
        half _BumpScale;
        half _Parallax;
        half _OcclusionStrength;

        half _Surface;
        half _ReflectionIntensity;
        half _ReflectionNoise;
        float4 _SpecCube_HDR;
        half4 _shadowIntensity;
        half _SpecCubeMip;
		// half4 _BaseColor1;
		// half4 _BaseColor2;
		int _UVType;

		// half _WaveSpeed;
		// half _WaveIntensity;
		// half _WaveScale;
		// // half4 _WindDirection;
		// half4 _Wiggle; //xy:Speed z:Scale w:YOffset
		//
		// half _Dissolve;
		// int _DissolveType;
		// half4 _DissolveColor;
		// half4 _DissolveMap_ST;
		// half _Edge_size;

		half4 _HSVoffset;
		int _UVOffset_Toggle;
		int _OcclusionUVType;

		float4 _ProjectionMap_ST;
		half4 _ProjectionDir;
		half _ProjectionSmoothness;
		half _ProjectionDown;
		half _ProjectionUp;
		int _ProjectionUvType;

		int _OutlineType;
		half4 _OutlineColor;
		half4 _OutlineParams;
		half4 _OutlineWidthParams;
		
		half4 _EffectColor;
		int _InvertDissolveEffect;

		int _isGlobal; 
		float4 _DynamicRadialMasks_DATA1;	
		float4 _DynamicRadialMasks_DATA2;
		half4 _DynamicRadialMasks_MaskEdgeColor;
		half _DynamicRadialMasks_NoiseUvScale;
    CBUFFER_END

#ifdef _PROJECTIONTOGGLE_ON
	TEXTURE2D(_ProjectionMap);
	SAMPLER(sampler_ProjectionMap);
#endif

#if defined(_EMISSION) || defined(_OCCLUSIONMAP)
    TEXTURE2D(_MetallicGlossMap);
    SAMPLER(sampler_MetallicGlossMap);
#endif

    TEXTURE2D(_BaseMap);
    SAMPLER(sampler_BaseMap);
    float4 _BaseMap_TexelSize;
    float4 _BaseMap_MipInfo;
    TEXTURE2D(_BumpTex);
    SAMPLER(sampler_BumpTex);

	// #ifdef _DYETOGGLE_ON
	// 	TEXTURE2D(_MaskMap);
	// 	SAMPLER(sampler_MaskMap);
	// #endif

    #ifdef _REFLECTIONTOGGLE_ON
        TEXTURECUBE(_SpecCube);    SAMPLER(sampler_SpecCube);
    #endif

	#ifdef _DISSOIVEKEY_ON
		#include "../Common/H_Dissoived.hlsl"
	// #else
	// 	//x,y:unity x z轴方向的偏移 z:1/图片所占米数
	// 	float4 _GlobalCityMapParams;
	#endif

	// int _GlobalSceneDarkenToggle;

    half Alpha(half albedoAlpha, half4 color, half cutoff)
    {
        half alpha = albedoAlpha * color.a;
    	
        #if defined(_ALPHATEST_ON) && !defined(_DISSOIVEKEY_ON)
            clip(alpha - cutoff);
        #endif

        return alpha;
    }

	#ifdef _DYETOGGLE_ON
		// hsv
		inline float3 ColorGradingHSV(float3 c, float hueScaleOffset, float saturateScale, float brightScale)
		{
    		c = RgbToHsv(c);
    		c.x = c.x + hueScaleOffset.x;
    		c.y *= saturateScale;
    		c.z *= brightScale;
    		return HsvToRgb(c);
	    }
	#endif

    half4 SampleAlbedoAlpha(float2 uv, TEXTURE2D_PARAM(albedoAlphaMap, sampler_albedoAlphaMap))
    {
    	half4 col = SAMPLE_TEXTURE2D(albedoAlphaMap, sampler_albedoAlphaMap, uv);
    	// #ifndef _DYETOGGLE_ON
    		col.rgb *= _BaseColor.rgb;
    	// #endif
    	
        return col;
    }

    half SampleOcclusion(half occ)
    {
        #ifdef _OCCLUSIONMAP
            #if defined(SHADER_API_GLES)
                return occ;
            #else
                return LerpWhiteTo(occ, _OcclusionStrength);
            #endif
        #else
            return half(1.0);
        #endif
    }

    float2 ParallaxMapping(half h, half3 viewDirTS, half scale, float2 uv)
    {
        float2 offset = ParallaxOffset1Step(h, scale, viewDirTS);
        return offset;
    }

    void ApplyPerPixelDisplacement(half3 viewDirTS, inout float2 uv)
    {
        #if defined(_PARALLAXMAP)
            half h = SAMPLE_TEXTURE2D(_MetallicGlossMap, sampler_BumpTex, uv).b;
            uv += ParallaxMapping(h, viewDirTS, _Parallax, uv);
        #endif
    }

	float3 UnpackNormalRG(half2 RG, half BumpScale = 1)
    {
    	RG = RG * 2 - 1;
    	float normalZ = 1 - saturate(dot(RG, RG));
    	// float normalZ = max(1.0e-16, sqrt(1 - saturate(dot(RG, RG))));
    	RG *= BumpScale;
    	return float3(RG, sqrt(normalZ));
    }

	struct AddSurfData
    {
    	float4 uv;
    	float2 uv1;
    	float3 normalWS;

    	#ifdef _DISSOIVEKEY_ON
    		float2 dissolveUV;
    		float2 DissolveMask;
    	#endif

    	#ifdef _DYNAMIC_DISSOLVE_ON
    		float3 AbsoluteWorldSpacePosition;
    	#endif
    };

    inline void InitializeStandardLitSurfaceData(AddSurfData addSurfData, out SurfaceData outSurfaceData)
    {
        half4 albedoAlpha = SampleAlbedoAlpha(addSurfData.uv.xy, TEXTURE2D_ARGS(_BaseMap, sampler_BaseMap));

    	#ifdef _DYNAMIC_DISSOLVE_ON
    		half4 edgeColor;
    		float finalMask = ComputeEdgeColorAndMask(addSurfData.AbsoluteWorldSpacePosition, addSurfData.uv.xy, _isGlobal,
    		_DynamicRadialMasks_DATA1, _DynamicRadialMasks_DATA2, _DynamicRadialMasks_MaskEdgeColor, _DynamicRadialMasks_NoiseUvScale,
    		_InvertDissolveEffect, edgeColor);
    		outSurfaceData.alpha = albedoAlpha.a * _BaseColor.a;
    		clip(outSurfaceData.alpha * (1 - finalMask) - _Cutoff);
    	#else
    		outSurfaceData.alpha = Alpha(albedoAlpha.a, _BaseColor, _Cutoff);
    	#endif
    	
    	#if defined(_NORMALMAP)
    		half4 normalTS = SAMPLE_TEXTURE2D(_BumpTex, sampler_BumpTex, addSurfData.uv.xy);
    		normalTS.b *= _Metallic;
    		normalTS.a *= _Smoothness;
    	#else
    		half4 normalTS = half4(0.5h, 0.5h, _Metallic, _Smoothness);
    	#endif
    	
    	#ifdef _PROJECTIONTOGGLE_ON
    		float blendFactor = dot(_ProjectionDir.xyz, NormalizeNormalPerPixel(addSurfData.normalWS));
    		blendFactor = smoothstep(_ProjectionDown, _ProjectionUp, blendFactor);
    		half4 ProjectionColor = SAMPLE_TEXTURE2D(_ProjectionMap, sampler_ProjectionMap, addSurfData.uv.zw);
    		albedoAlpha.rgb = lerp(albedoAlpha.rgb, ProjectionColor.rgb, blendFactor);
    		normalTS = lerp(normalTS, half4(0.5h, 0.5h, 0.0h, ProjectionColor.a * _ProjectionSmoothness), blendFactor);
    	#endif

    	// 染色
    	#ifdef _DYETOGGLE_ON
    		albedoAlpha.rgb = ColorGradingHSV(albedoAlpha.rgb, _HSVoffset.x, _HSVoffset.y, _HSVoffset.z);
    	#endif

    	#if defined(_NORMALMAP)
    		float3 nt = UnpackNormalRG(normalTS.rg, _BumpScale);
    	#else
    		float3 nt = float3(-1, -1, 0);
    	#endif

    	// 目前用于AO和自发光还有反射以及高度
    	#if defined(_OCCLUSIONMAP)
    		half4 specGloss = SAMPLE_TEXTURE2D(_MetallicGlossMap, sampler_MetallicGlossMap, addSurfData.uv1.xy);
    	#else
    		half4 specGloss = half4(1, 0, 0, 1);
    	#endif
    	
    	
    	half3 emission = 0;
    	#ifdef _EMISSION
    		emission = specGloss.a * _EmissionColor.rgb * albedoAlpha.rgb;
    	#endif
    	
    	#ifdef _DISSOIVEKEY_ON
    		Dissoives(outSurfaceData.alpha, albedoAlpha.rgb, emission, noiseTex(addSurfData.dissolveUV), _Edge_size, addSurfData.DissolveMask.x, addSurfData.DissolveMask.y);
    	#endif

    	#ifdef _DYNAMIC_DISSOLVE_ON
    		emission += edgeColor.rgb;
    	#endif
    	
        outSurfaceData.albedo = albedoAlpha.rgb;
        outSurfaceData.metallic = normalTS.b;
        outSurfaceData.specular = half3(0.0, 0.0, 0.0);
        outSurfaceData.smoothness = normalTS.a;
    	outSurfaceData.emission = emission;
    	
        outSurfaceData.normalTS = nt;
        outSurfaceData.occlusion = SampleOcclusion(specGloss.r);

        // 临时当反射的通道用了
        #if defined(_REFLECTIONTOGGLE_ON)
            outSurfaceData.clearCoatMask = specGloss.g * _ReflectionIntensity;
        #else
            outSurfaceData.clearCoatMask = half(0.0);
        #endif
        outSurfaceData.clearCoatSmoothness = half(0.0);
       
    }

#endif // UNIVERSAL_INPUT_SURFACE_PBR_INCLUDED
