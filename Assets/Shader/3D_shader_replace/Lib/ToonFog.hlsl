
#ifndef TOON_FOG_HLSL
#define TOON_FOG_HLSL
	//#pragma multi_compile_fog
	#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

	// #if defined(USE_URP_SHADER)
	//     #undef branch_if
	//     #define branch_if UNITY_BRANCH if
	// #else
	// 	#define branch_if [branch] if
	// #endif

	#define IsFogOn() (_HasGlobalFog)
	#define DECLARE_FOG_FACTOR(index) float2 fogFactor : TEXCOORD##index
	//------------------------------ sphere fogFactor params
	//fog范围 near far bottom top
	float4 _FogSpace;
	float4 _HeightFogBottomColor, _HeightFogTopColor;

	//------------------------------  global fogFactor params
	float _GlobalFogIntensity;
	bool _HasGlobalFog, _HasHeightFog, _HasDepthFog;

	real ComputeFogFactorZ0ToFar_New(float z)
	{
		// float fogFactor = saturate(z * unity_FogParams.z + unity_FogParams.w);
		float fogFactor = saturate((_FogSpace.x - z) / (_FogSpace.x - _FogSpace.y));
		return real(fogFactor);
	}

	float2 CalcFogFactor(float3 worldPos, float zPositionCS)
	{
		float clipZ_0Far = UNITY_Z_0_FAR_FROM_CLIPSPACE(zPositionCS);
		// fogFactor.x = lerp(0, saturate((distance(worldPos, _WorldSpaceCameraPos) - _FogSpace.x)/(_FogSpace.y - _FogSpace.x)), _HasDepthFog);
		return lerp(0, float2(ComputeFogFactorZ0ToFar_New(clipZ_0Far),
			saturate((worldPos.y - _FogSpace.w) / (_FogSpace.z - _FogSpace.w))), int2(_HasDepthFog, _HasHeightFog));
	}

	void ApplyFog(inout half3 mainColor, float2 fogCoord)
	{
		// // fogCoord.x = InitializeInputDataFog_New(float4(positionWS, 1.0), fogCoord.x);
		[branch] if (_HasHeightFog && _HasDepthFog)
		{
			float3 heightFogColor = lerp(_HeightFogTopColor.rgb, _HeightFogBottomColor.rgb, fogCoord.y * 2 - 1);
			mainColor = lerp(mainColor, heightFogColor, fogCoord.y * fogCoord.x * _GlobalFogIntensity);
		}
		else if(_HasDepthFog)
		{
			mainColor = lerp(mainColor, _HeightFogTopColor.rgb, fogCoord.x * _GlobalFogIntensity);
		}
	}

	
#endif
