#ifndef UNIVERSAL_H_LIT_META_PASS_SCENE_INCLUDED
#define UNIVERSAL_H_LIT_META_PASS_SCENE_INCLUDED

	#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/MetaInput.hlsl"

	struct Attributes
	{
		float4 positionOS   : POSITION;
		float3 normalOS     : NORMAL;
		float2 uv0          : TEXCOORD0;
		float2 uv1          : TEXCOORD1;
		float2 uv2          : TEXCOORD2;
		UNITY_VERTEX_INPUT_INSTANCE_ID
	};

	struct Varyings
	{
		float4 positionCS   : SV_POSITION;
		float4 uv           : TEXCOORD0;
		float2 occlusionUV  : TEXCOORD1;
		float3 normalWS : TEXCOORD2;
		#ifdef EDITOR_VISUALIZATION
			float2 VizUV        : TEXCOORD1;
			float4 LightCoord   : TEXCOORD2;
		#endif
	};

	float4 UnityMetaVertexPosition_New(float3 vertex, float2 uv1, float2 uv2, float4 lightmapST, float4 dynlightmapST, float3 positionW)
	{
		#ifndef EDITOR_VISUALIZATION
			if (unity_MetaVertexControl.x)
			{
				vertex.xy = uv1 * lightmapST.xy + lightmapST.zw;
				// OpenGL right now needs to actually use incoming vertex position,
				// so use it in a very dummy way
				vertex.z = vertex.z > 0 ? REAL_MIN : 0.0f;
			}
			if (unity_MetaVertexControl.y)
			{
				vertex.xy = uv2 * dynlightmapST.xy + dynlightmapST.zw;
				// OpenGL right now needs to actually use incoming vertex position,
				// so use it in a very dummy way
				vertex.z = vertex.z > 0 ? REAL_MIN : 0.0f;
			}
			return TransformWorldToHClip(vertex);
		#else
			return TransformWorldToHClip(positionWS);
		#endif
	}

	Varyings UniversalVertexMeta(Attributes input)
	{
		Varyings output = (Varyings)0;
		float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
		output.positionCS = UnityMetaVertexPosition_New(input.positionOS.xyz, input.uv1, input.uv2, unity_LightmapST, unity_DynamicLightmapST, positionWS);
		float2 uv = input.uv0.xy * _BaseMap_ST.xy;
		float3 globalWorldPos = positionWS;
		float2 worldUV = globalWorldPos.xz * _BaseMap_ST.xy;
		output.uv.xy = _UVType ? (_UVOffset_Toggle ? uv + _BaseMap_ST.zw * _Time.yy : uv) : worldUV; 
		output.uv.zw = worldUV;
		output.occlusionUV = _OcclusionUVType ? input.uv1 : output.uv.xy;
		output.normalWS = TransformObjectToWorldNormal(input.normalOS);;
		#ifdef EDITOR_VISUALIZATION
			UnityEditorVizData(input.positionOS.xyz, input.uv0, input.uv1, input.uv2, output.VizUV, output.LightCoord);
		#endif
		return output;
	}

	half4 UniversalFragmentMeta(Varyings fragIn, MetaInput metaInput)
	{
		#ifdef EDITOR_VISUALIZATION
			metaInput.VizUV = fragIn.VizUV;
			metaInput.LightCoord = fragIn.LightCoord;
		#endif

		return UnityMetaFragment(metaInput);
	}

	half4 UniversalFragmentMetaLit(Varyings input) : SV_Target
	{
		AddSurfData addSurfData = (AddSurfData)0;
		addSurfData.uv = input.uv;
		addSurfData.uv1 = input.occlusionUV.xy;
		addSurfData.normalWS = input.normalWS;
		
	    SurfaceData surfaceData;
	    InitializeStandardLitSurfaceData(addSurfData, surfaceData);

	    BRDFData brdfData;
	    InitializeBRDFData(surfaceData.albedo, surfaceData.metallic, surfaceData.specular, surfaceData.smoothness, surfaceData.alpha, brdfData);

	    MetaInput metaInput;
	    metaInput.Albedo = brdfData.diffuse + brdfData.specular * brdfData.roughness * 0.5;
	    metaInput.Emission = surfaceData.emission;
	    return UniversalFragmentMeta(input, metaInput);
	}

#endif
