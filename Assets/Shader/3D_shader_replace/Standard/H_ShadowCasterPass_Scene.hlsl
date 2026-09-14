#ifndef UNIVERSAL_SHADOW_CASTER_PASS_INCLUDED
#define UNIVERSAL_SHADOW_CASTER_PASS_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
#ifdef _WINDTOGGLE_ON
	#include "GrassLib/GrassCore.hlsl"
#endif
// Shadow Casting Light geometric parameters. These variables are used when applying the shadow Normal Bias and are set by UnityEngine.Rendering.Universal.ShadowUtils.SetupShadowCasterConstantBuffer in com.unity.render-pipelines.universal/Runtime/ShadowUtils.cs
// For Directional lights, _LightDirection is used when applying shadow Normal Bias.
// For Spot lights and Point lights, _LightPosition is used to compute the actual light direction because it is different at each shadow caster geometry vertex.
float3 _LightDirection;
float3 _LightPosition;

struct Attributes
{
    float4 positionOS   : POSITION;
    float3 normalOS     : NORMAL;
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

Varyings ShadowPassVertex(Attributes input)
{
    Varyings output = (Varyings)0;
	UNITY_SETUP_INSTANCE_ID(input);
	UNITY_TRANSFER_INSTANCE_ID(input, output);
	
    output.uv = TRANSFORM_TEX(input.texcoord, _BaseMap);

	output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
	float3 normalWS = TransformObjectToWorldNormal(input.normalOS);

	#ifdef _WINDTOGGLE_ON
		output.positionWS = WaveVertex(output.positionWS, input.positionOS.xyz, input.color.r).xyz;
	#endif
	
	#if _CASTING_PUNCTUAL_LIGHT_SHADOW
		float3 lightDirectionWS = normalize(_LightPosition - positionWS);
	#else
		float3 lightDirectionWS = _LightDirection;
	#endif

	float4 positionCS = TransformWorldToHClip(ApplyShadowBias(output.positionWS, normalWS, lightDirectionWS));

	#if UNITY_REVERSED_Z
		positionCS.z = min(positionCS.z, UNITY_NEAR_CLIP_VALUE);
	#else
		positionCS.z = max(positionCS.z, UNITY_NEAR_CLIP_VALUE);
	#endif
	
    output.positionCS = positionCS;

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

half4 ShadowPassFragment(Varyings input) : SV_TARGET
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
		Alpha(alpha, _BaseColor, _Cutoff);
	#endif
    return 0;
}

#endif
