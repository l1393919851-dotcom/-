#ifndef UNIVERSAL_DEPTH_ONLY_PASS_INCLUDED
#define UNIVERSAL_DEPTH_ONLY_PASS_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#ifdef _WINDTOGGLE_ON
	#include "GrassLib/GrassCore.hlsl"
#endif

struct Attributes
{
    float4 position     : POSITION;
    float2 texcoord     : TEXCOORD0;
	#if defined(_WINDTOGGLE_ON) || defined(_DISSOIVEKEY_ON)
		half4 color : COLOR;
	#endif

	#ifdef _DISSOIVEKEY_ON
		float2 uv2   : TEXCOORD2;
	#endif
	
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Varyings
{
    float2 uv           : TEXCOORD0;
    float4 positionCS   : SV_POSITION;
	float3 positionWS	: TEXCOORD1;
	#ifdef _DISSOIVEKEY_ON
		#ifdef _HSV_ON
			float4 uv2       : TEXCOORD2; // xy: uv2, z:dissolveMask, w:heightMask
		#else
			float2 uv2       : TEXCOORD2;
		#endif
	#endif

	#ifdef _DYNAMIC_DISSOLVE_ON
		float3 AbsoluteWorldSpacePosition : TEXCOORD3;
	#endif
	
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

Varyings DepthOnlyVertex(Attributes input)
{
    Varyings output = (Varyings)0;
    UNITY_SETUP_INSTANCE_ID(input);
	UNITY_TRANSFER_INSTANCE_ID(input, output);

    output.uv = TRANSFORM_TEX(input.texcoord, _BaseMap);
	output.positionWS = TransformObjectToWorld(input.position.xyz);
	#ifdef _WINDTOGGLE_ON
		output.positionWS = WaveVertex(output.positionWS, input.position.xyz, input.color.r).xyz;
	#endif
    output.positionCS = TransformWorldToHClip(output.positionWS);

	#ifdef _DISSOIVEKEY_ON
		#ifdef _HSV_ON
			output.uv2.xy = input.uv2.xy;
			output.uv2.zw = input.color.gb;
		#else
			output.uv2 = input.uv2;
		#endif
	#endif

	#ifdef _DYNAMIC_DISSOLVE_ON
		output.AbsoluteWorldSpacePosition = GetAbsolutePositionWS(output.positionWS);
	#endif
	
    return output;
}

half4 DepthOnlyFragment(Varyings input) : SV_TARGET
{
	UNITY_SETUP_INSTANCE_ID(input);
	half alpha = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv).a;
	
	#ifdef _DYNAMIC_DISSOLVE_ON
		float finalMask = ComputeEdgeColorAndMask(input.AbsoluteWorldSpacePosition, input.uv, _isGlobal,
			_DynamicRadialMasks_DATA1, _DynamicRadialMasks_DATA2, _DynamicRadialMasks_NoiseUvScale,
			_InvertDissolveEffect);
		alpha *= _BaseColor.a;
		clip(alpha * (1 - finalMask) - _Cutoff);
	#elif _DISSOIVEKEY_ON
		Dissoives(alpha, noiseTex(_DissolveType ? input.uv2.xy : input.positionWS.xz * _DissolveMap_ST.xy), _Edge_size
		#ifdef _HSV_ON
			, input.uv2.z
		#endif
		);
	#else
		Alpha(SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv).a, _BaseColor, _Cutoff);
	#endif
	
    return 0;
}
#endif
