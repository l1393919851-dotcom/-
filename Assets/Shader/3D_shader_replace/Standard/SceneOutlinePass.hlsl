#ifndef UNIVERSAL_SCENE_OUTLINE_PASS_INCLUDED
#define UNIVERSAL_SCENE_OUTLINE_PASS_INCLUDED

	#ifdef _WINDTOGGLE_ON
		#include "GrassLib/GrassCore.hlsl"
	#endif

	struct Attributes
	{
	    float4 positionOS   : POSITION;
	    float3 normalOS     : NORMAL;
		float2 uv           : TEXCOORD0;
		#if defined(_WINDTOGGLE_ON)
			half4 color : COLOR;
		#endif
		
		UNITY_VERTEX_INPUT_INSTANCE_ID
	};

	struct Varyings
	{
	    float4 positionCS   : SV_POSITION;
		float2 uv           : TEXCOORD0;
		UNITY_VERTEX_INPUT_INSTANCE_ID
	};

	float GetOutlineWidth(float positionVS_Z)
	{
		float z = _WorldSpaceCameraPos.y;
		float k = saturate((z - 10) / (20 - 10));
		float width = lerp(0.06, 0.15, k);
		
	    return width;
	}

	float4 GetOutlinePosition(float3 positionVS, float3 normalWS)
	{
	    float z = positionVS.z;
		float width = GetOutlineWidth(z);

	    half3 normalVS = TransformWorldToViewNormal(normalWS);
	    normalVS = SafeNormalize(half3(normalVS.xy, 0.0));

	    float3 V = positionVS;
	    V += SafeNormalize(V);
	    V += width * normalVS;

	    float4 positionCS = TransformWViewToHClip(V);

	    return positionCS;
	}

	Varyings OutlinePassVertex(Attributes input)
	{
		Varyings output = (Varyings)0;
		UNITY_SETUP_INSTANCE_ID(input);
		UNITY_TRANSFER_INSTANCE_ID(input, output);
		output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
		output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
		// output.vertex.z *= 0.9;
		half3 worldNormal = TransformObjectToWorldNormal(input.normalOS);
		half3 normalClip = mul((half3x3)UNITY_MATRIX_VP, worldNormal);
		
		output.positionCS.xy += normalize(normalClip.xy) * 0.005 * output.positionCS.w;
		return output;
		
		// Varyings output = (Varyings)0;
		// UNITY_SETUP_INSTANCE_ID(input);
		// UNITY_TRANSFER_INSTANCE_ID(input, output);
		// float3 normalWS = TransformObjectToWorldNormal(input.normalOS);
		// float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
		// #ifdef _WINDTOGGLE_ON
		// 	positionWS = WaveVertex(positionWS, input.positionOS.xyz, input.color.r).xyz;
		// #endif
		//
		// float3 positionVS = TransformWorldToView(positionWS);
		//
	 //    float4 positionCS = GetOutlinePosition(positionVS, normalWS);
		// output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
	 //    output.positionCS = positionCS;
	 //
	 //    return output;
	}

	half4 OutlinePassFragment(Varyings input) : SV_TARGET
	{
		UNITY_SETUP_INSTANCE_ID(input);
		#if defined(_ALPHATEST_ON)
			half4 color = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv);
			clip(color.a - _Cutoff);
		#endif
		
	    return _OutlineType >= 1 ? half4(0.8705882, 0.8705882, 0.8705882, 1) : half4(0.9058824, 0.8823529, 0.2705882, 1);
	}

	float GetCityOutlineWidth(float positionVS_Z)
	{
		// #ifdef _OUTLINE_WIDTH_PROPERTIES
		float z = _WorldSpaceCameraPos.y;
		float k = saturate((z - _OutlineWidthParams.x) / (_OutlineWidthParams.y - _OutlineWidthParams.x));
		float width = lerp(_OutlineWidthParams.z, _OutlineWidthParams.w, k);
		// #else
		// float fovFactor = 2.414 / UNITY_MATRIX_P[1].y;
		// float z = abs(positionVS_Z * fovFactor);
		// float k = saturate(z / 5);
		// float width = lerp(0, 1, k);
		// #endif
				
		return 0.01 * _OutlineParams.z * width;
	}

	float4 GetCityOutlinePosition(float3 positionVS, float3 normalWS)
	{
		float z = positionVS.z;
		// float width = GetOutlineWidth(z) * vertexColor.a;
		float width = GetCityOutlineWidth(z);

		half3 normalVS = TransformWorldToViewNormal(normalWS);
		normalVS = SafeNormalize(half3(normalVS.xy, 0.0));

		float3 V = positionVS;
		V += 0.01 * _OutlineParams.w * SafeNormalize(V);
		V += width * normalVS;

		float4 positionCS = TransformWViewToHClip(V);
		// positionCS.xy += _ScreenOffset.zw * positionCS.w;
		positionCS.xy += _OutlineParams.xy * positionCS.w;

		return positionCS;
	}

	Varyings CityOutlinePassVertex(Attributes input)
	{
		Varyings output = (Varyings)0;
		UNITY_SETUP_INSTANCE_ID(input);
		UNITY_TRANSFER_INSTANCE_ID(input, output);
		float3 normalWS = TransformObjectToWorldNormal(input.normalOS);
		float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
		#ifdef _WINDTOGGLE_ON
			positionWS = WaveVertex(positionWS, input.positionOS.xyz, input.color.r).xyz;
		#endif
		float3 positionVS = TransformWorldToView(positionWS);
		float4 positionCS = GetCityOutlinePosition(positionVS, normalWS);
		output.positionCS = positionCS;
		output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
		// output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
		// output.vertex.z *= 0.9;
		half3 worldNormal = TransformObjectToWorldNormal(input.normalOS);
		half3 normalClip = mul((half3x3)UNITY_MATRIX_VP, worldNormal);
		
		output.positionCS.xy += normalize(normalClip.xy) * 0.005 * output.positionCS.w;
		return output;
	}


	half4 CityOutlinePassFragment(Varyings input) : SV_TARGET
	{
		UNITY_SETUP_INSTANCE_ID(input);
		#if defined(_ALPHATEST_ON)
		half4 color = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv);
		clip(color.a - _Cutoff);
		#endif
			
		return half4(_OutlineColor.rgb, 1);
	}

#endif
