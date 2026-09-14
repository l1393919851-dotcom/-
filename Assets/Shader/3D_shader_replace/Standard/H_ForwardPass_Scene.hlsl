#ifndef UNIVERSAL_FORWARD_PASS_SCENE_INCLUDED
#define UNIVERSAL_FORWARD_PASS_SCENE_INCLUDED

    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
	#ifdef _WINDTOGGLE_ON
		#include "GrassLib/GrassCore.hlsl"
	#endif

    // GLES2 has limited amount of interpolators
    #if defined(_PARALLAXMAP) && !defined(SHADER_API_GLES)
        #define REQUIRES_TANGENT_SPACE_VIEW_DIR_INTERPOLATOR
    #endif

    #if (defined(_NORMALMAP) || (defined(_PARALLAXMAP) && !defined(REQUIRES_TANGENT_SPACE_VIEW_DIR_INTERPOLATOR))) || defined(_DETAIL)
        #define REQUIRES_WORLD_SPACE_TANGENT_INTERPOLATOR
    #endif

    // keep this file in sync with LitGBufferPass.hlsl

    struct Attributes
    {
        float4 positionOS : POSITION;
        float3 normalOS : NORMAL;
        float4 tangentOS : TANGENT;
    	#if defined(_WINDTOGGLE_ON) || defined(_DISSOIVEKEY_ON)
    		half4 color : COLOR;
    	#endif
        float2 texcoord : TEXCOORD0;
        float2 staticLightmapUV : TEXCOORD1;
    	
    	#ifdef _DISSOIVEKEY_ON
    		float2 uv2   : TEXCOORD2;
    	#endif
    	
        UNITY_VERTEX_INPUT_INSTANCE_ID
    };

    struct Varyings
    {
        float4 uv : TEXCOORD0; // xy: uv, zw:fogFactor
        #if defined(REQUIRES_WORLD_SPACE_POS_INTERPOLATOR) || defined(_SKYSHADOW)
            float3 positionWS : TEXCOORD1;
        #endif

        float3 normalWS : TEXCOORD2;
        #if defined(REQUIRES_WORLD_SPACE_TANGENT_INTERPOLATOR)
            half4 tangentWS : TEXCOORD3; // xyz: tangent, w: sign
        #endif
        // float3 viewDirWS : TEXCOORD4;
    	float4 occlusionUV : TEXCOORD4;

        #ifdef _ADDITIONAL_LIGHTS_VERTEX
            half4 fogFactorAndVertexLight   : TEXCOORD5; // x: 暂时没用, yzw: vertex light
        #endif
    	
        #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
            float4 shadowCoord              : TEXCOORD6;
        #endif

        #if defined(REQUIRES_TANGENT_SPACE_VIEW_DIR_INTERPOLATOR)
            half3 viewDirTS : TEXCOORD7;
        #endif

        DECLARE_LIGHTMAP_OR_SH(staticLightmapUV, vertexSH, 8);

    	#ifdef _DYNAMIC_DISSOLVE_ON
    		float3 AbsoluteWorldSpacePosition : TEXCOORD9;
    	#endif
    	
    	#ifdef _DISSOIVEKEY_ON
    		#ifdef _HSV_ON
    			float4 uv2       : TEXCOORD10; // xy: uv2, z:dissolveMask, w:heightMask
    		#else
    			float2 uv2       : TEXCOORD10;
    		#endif
    	#endif
    	
        float4 positionCS : SV_POSITION;
    	
        UNITY_VERTEX_INPUT_INSTANCE_ID
    };

    // float3 Unity_NormalStrength_float(float3 In, float Strength)
    // {
    //     return float3(In.rg * Strength, lerp(1, In.b, saturate(Strength)));
    // }

	half3 GlossyEnvironmentReflection_New(half3 reflectVector, half occlusion, half indirectMask)
    {
    	#ifdef _REFLECTIONTOGGLE_ON
    		// half3 reflectVector2 = reflect(-viewDirectionWS, Unity_NormalStrength_float(normalWS, _ReflectionNoise));
    		half4 encodedIrradiance = half4(SAMPLE_TEXTURECUBE_LOD(_SpecCube, sampler_SpecCube, reflectVector, _SpecCubeMip));
    		half3 irradiance = DecodeHDREnvironment(encodedIrradiance, _SpecCube_HDR);
    		return irradiance * occlusion * indirectMask;
    	#else
    		return 0;
    	#endif
    }

    half3 GlobalIllumination_New(BRDFData brdfData, half3 bakedGI, half occlusion, float3 positionWS, half3 reflectVector, half NoV, half indirectMask,
         half3 viewDirectionWS, float3 normalWS)
    {
        half fresnelTerm = Pow4(1.0 - NoV);
        half3 indirectDiffuse = bakedGI;
    	// half3 indirectSpecular = GlossyEnvironmentReflection(reflectVector, positionWS, brdfData.perceptualRoughness, 1.0h);
    	half3 indirectSpecular = GlossyEnvironmentReflection_New(reflectVector, 1.0h, indirectMask);
        half3 color = EnvironmentBRDF(brdfData, indirectDiffuse, indirectSpecular, fresnelTerm);

        return color * occlusion;
    }

    half3 LightingPhysicallyBased_New(BRDFData brdfData,
    half3 lightColor, half3 lightDirectionWS, float lightAttenuation,
    half3 normalWS, half3 viewDirectionWS,
    bool specularHighlightsOff)
    {
        half NdotL = saturate(dot(normalWS, lightDirectionWS));
        half3 radiance = lightColor * lerp(_shadowIntensity.rgb, 1, lightAttenuation * NdotL);
        
        half3 brdf = brdfData.diffuse;
        #ifndef _SPECULARHIGHLIGHTS_OFF
        [branch] if (!specularHighlightsOff)
        {
            brdf += brdfData.specular * DirectBRDFSpecular(brdfData, normalWS, lightDirectionWS, viewDirectionWS);
        }
        #endif // _SPECULARHIGHLIGHTS_OFF

        return brdf * radiance;
    }

    half3 LightingPhysicallyBased_New(BRDFData brdfData, Light light, half3 normalWS, half3 viewDirectionWS, bool specularHighlightsOff)
    {
        return LightingPhysicallyBased_New(brdfData, light.color, light.direction, light.distanceAttenuation * light.shadowAttenuation, normalWS, viewDirectionWS, specularHighlightsOff);
    }

	half3 LightingPhysicallyBased_Add(BRDFData brdfData,
	half3 lightColor, half3 lightDirectionWS, half lightAttenuation,
	half3 normalWS, half3 viewDirectionWS,
	bool specularHighlightsOff)
    {
    	half NdotL = saturate(dot(normalWS, lightDirectionWS));
    	half3 radiance = lightColor * (lightAttenuation * NdotL);

    	half3 brdf = brdfData.diffuse;
    	#ifndef _SPECULARHIGHLIGHTS_OFF
    	[branch] if (!specularHighlightsOff)
    	{
    		brdf += brdfData.specular * DirectBRDFSpecular(brdfData, normalWS, lightDirectionWS, viewDirectionWS);
    	}
    	#endif // _SPECULARHIGHLIGHTS_OFF

    	return brdf * radiance;
    }

    half4 UniversalFragmentPBR_New(InputData inputData, SurfaceData surfaceData)
    {
        #if defined(_SPECULARHIGHLIGHTS_OFF)
            bool specularHighlightsOff = true;
        #else
            bool specularHighlightsOff = false;
        #endif
        BRDFData brdfData;
   
        // NOTE: can modify "surfaceData"...
        InitializeBRDFData(surfaceData, brdfData);
    	
        half4 shadowMask = CalculateShadowMask(inputData);
        // AmbientOcclusionFactor aoFactor = CreateAmbientOcclusionFactor(inputData, surfaceData);
        // uint meshRenderingLayers = GetMeshRenderingLightLayer();
        // Light mainLight = GetMainLight(inputData, shadowMask, aoFactor);
    	Light mainLight = GetMainLight(inputData.shadowCoord, inputData.positionWS, shadowMask);
        // NOTE: We don't apply AO to the GI here because it's done in the lighting calculation below...
        MixRealtimeAndBakedGI(mainLight, inputData.normalWS, inputData.bakedGI);
   
        LightingData lightingData = CreateLightingData(inputData, surfaceData);
        
        half3 reflectVector = reflect(-inputData.viewDirectionWS, inputData.normalWS);
        half NoV = saturate(dot(inputData.normalWS, inputData.viewDirectionWS));
   
        lightingData.giColor = GlobalIllumination_New(brdfData, inputData.bakedGI, surfaceData.occlusion, inputData.positionWS, reflectVector, NoV, surfaceData.clearCoatMask, inputData.viewDirectionWS, inputData.normalWS);
        lightingData.mainLightColor = LightingPhysicallyBased_New(brdfData,
                                                              mainLight,
                                                              inputData.normalWS, inputData.viewDirectionWS,
                                                              specularHighlightsOff);
    	
        #if defined(_ADDITIONAL_LIGHTS)
            uint pixelLightCount = GetAdditionalLightsCount();
            LIGHT_LOOP_BEGIN(pixelLightCount)
    			Light light = GetAdditionalLight(lightIndex, inputData.positionWS, shadowMask);
    			lightingData.additionalLightsColor += LightingPhysicallyBased_Add(brdfData,
    				light.color, light.direction, light.distanceAttenuation * light.shadowAttenuation, inputData.normalWS, inputData.viewDirectionWS,
    				specularHighlightsOff);
            LIGHT_LOOP_END
        #endif
   
        // #if defined(_ADDITIONAL_LIGHTS_VERTEX)
        //     lightingData.vertexLightingColor += inputData.vertexLighting * brdfData.diffuse;
        // #endif
   
    	// half3 finalColor = lightingData.giColor + lightingData.mainLightColor + lightingData.emissionColor
    	// + lightingData.additionalLightsColor + lightingData.vertexLightingColor;
    	half3 finalColor = lightingData.giColor + lightingData.mainLightColor + lightingData.emissionColor
			+ lightingData.additionalLightsColor;
        // half3 finalColor = CalculateLightingColor(lightingData, 1);
      
        return half4(finalColor, surfaceData.alpha);
    }

	half4 UniversalFragmentPBR_Lod(InputData inputData, SurfaceData surfaceData)
    {
    	BRDFData brdfData= (BRDFData)0;
    	brdfData.diffuse = surfaceData.albedo;
    	#ifdef _ALPHAPREMULTIPLY_ON
    		brdfData.diffuse *= surfaceData.alpha;
    		surfaceData.alpha = 1; // NOTE: alpha modified and propagated up.
    	#endif
    	
        half4 shadowMask = CalculateShadowMask(inputData);
    	Light mainLight = GetMainLight(inputData.shadowCoord, inputData.positionWS, shadowMask);
        MixRealtimeAndBakedGI(mainLight, inputData.normalWS, inputData.bakedGI);

        LightingData lightingData = CreateLightingData(inputData, surfaceData);
        lightingData.mainLightColor = LightingPhysicallyBased_New(brdfData,
                                                                  mainLight,
                                                                  inputData.normalWS, inputData.viewDirectionWS,
                                                                  true);
     
    	
    	half3 finalColor = inputData.bakedGI * brdfData.diffuse * surfaceData.occlusion + lightingData.mainLightColor + lightingData.emissionColor;
    	
        return half4(finalColor, surfaceData.alpha);
    }

    void InitializeInputData(Varyings input, half3 normalTS, out InputData inputData)
    {
        inputData = (InputData)0;

        #if defined(REQUIRES_WORLD_SPACE_POS_INTERPOLATOR)
            inputData.positionWS = input.positionWS;
        #endif

        half3 viewDirWS = GetWorldSpaceNormalizeViewDir(input.positionWS);
        #if defined(_NORMALMAP) || defined(_DETAIL)
            float sgn = input.tangentWS.w; // should be either +1 or -1
            float3 bitangent = sgn * cross(input.normalWS.xyz, input.tangentWS.xyz);
            half3x3 tangentToWorld = half3x3(input.tangentWS.xyz, bitangent.xyz, input.normalWS.xyz);

            #if defined(_NORMALMAP)
                inputData.tangentToWorld = tangentToWorld;
            #endif
        
            inputData.normalWS = TransformTangentToWorld(normalTS, tangentToWorld);
        #else
            inputData.normalWS = input.normalWS;
        #endif

        inputData.normalWS = NormalizeNormalPerPixel(inputData.normalWS);
        inputData.viewDirectionWS = viewDirWS;

        #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
            inputData.shadowCoord = input.shadowCoord;
        #elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
            inputData.shadowCoord = TransformWorldToShadowCoord(inputData.positionWS);
        #else
            inputData.shadowCoord = float4(0, 0, 0, 0);
        #endif
        
        #ifdef _ADDITIONAL_LIGHTS_VERTEX
            inputData.vertexLighting = input.fogFactorAndVertexLight.yzw;
        #endif
    	
        inputData.bakedGI = SAMPLE_GI(input.staticLightmapUV, input.vertexSH, inputData.normalWS);
    	
        inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(input.positionCS);
        inputData.shadowMask = SAMPLE_SHADOWMASK(input.staticLightmapUV);
    }

    ///////////////////////////////////////////////////////////////////////////////
    //                  Vertex and Fragment functions                            //
    ///////////////////////////////////////////////////////////////////////////////

    // Used in Standard (Physically Based) shader
    Varyings LitPassVertex(Attributes input)
    {
        Varyings output = (Varyings)0;

        UNITY_SETUP_INSTANCE_ID(input);
        UNITY_TRANSFER_INSTANCE_ID(input, output);
    	
    	float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
    	float3 globalWorldPos = positionWS;
    	#ifdef _WINDTOGGLE_ON
    		positionWS = WaveVertex(positionWS, input.positionOS.xyz, input.color.r).xyz;
    	#endif
    	float4 positionCS = TransformWorldToHClip(positionWS);
        float2 fogFactor = CalcFogFactor(positionWS, positionCS.z);
    	float2 uv = input.texcoord.xy * _BaseMap_ST.xy;
        output.uv.xy = _UVType ? (_UVOffset_Toggle ? uv + _BaseMap_ST.zw * _Time.yy : uv) : globalWorldPos.xz * _BaseMap_ST.xy;
    	output.uv.zw = _ProjectionUvType ? globalWorldPos.xz * _ProjectionMap_ST.x : input.staticLightmapUV;
    	
    	float3 normalWS = TransformObjectToWorldNormal(input.normalOS);
        output.normalWS.xyz = normalWS;
        #if defined(REQUIRES_WORLD_SPACE_TANGENT_INTERPOLATOR) || defined(REQUIRES_TANGENT_SPACE_VIEW_DIR_INTERPOLATOR)
            real sign = input.tangentOS.w * GetOddNegativeScale();
            real4 tangentWS = half4(TransformObjectToWorldDir(input.tangentOS.xyz), sign);
        #endif
        
        #if defined(REQUIRES_WORLD_SPACE_TANGENT_INTERPOLATOR)
            output.tangentWS = tangentWS;
        #endif

        #if defined(REQUIRES_TANGENT_SPACE_VIEW_DIR_INTERPOLATOR)
            half3 viewDirWS = GetWorldSpaceNormalizeViewDir(positionWS);
            half3 viewDirTS = GetViewDirectionTangentSpace(tangentWS, output.normalWS.xyz, viewDirWS);
            output.viewDirTS = viewDirTS;
        #endif

        OUTPUT_LIGHTMAP_UV(input.staticLightmapUV, unity_LightmapST, output.staticLightmapUV);
    	
    	// 美术加, ao想走2套UV
    	output.occlusionUV.xy = _OcclusionUVType ? input.staticLightmapUV : output.uv.xy;
    	output.occlusionUV.zw = fogFactor;
    	
        OUTPUT_SH(output.normalWS.xyz, output.vertexSH);
        
        #ifdef _ADDITIONAL_LIGHTS_VERTEX
    		half3 vertexLight = VertexLighting(positionWS, normalWS);
            output.fogFactorAndVertexLight = half4(1, vertexLight);
        #endif
    	

        #if defined(REQUIRES_WORLD_SPACE_POS_INTERPOLATOR) || defined(_SKYSHADOW)
            output.positionWS = positionWS;
        #endif

        #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
    		#if defined(_MAIN_LIGHT_SHADOWS_SCREEN) && !defined(_SURFACE_TYPE_TRANSPARENT)
    			output.shadowCoord =  ComputeScreenPos(positionCS);
    		#else
    			output.shadowCoord =  TransformWorldToShadowCoord(positionWS);
    		#endif
        #endif

        output.positionCS = positionCS;

    	#ifdef _DYNAMIC_DISSOLVE_ON
    		output.AbsoluteWorldSpacePosition = GetAbsolutePositionWS(positionWS);
    	#endif
    	
    	#ifdef _DISSOIVEKEY_ON
    		#ifdef _HSV_ON
    			output.uv2.xy = _DissolveType ? input.uv2.xy : positionWS.xz * _DissolveMap_ST.xy;
    			output.uv2.zw = input.color.gb;
    		#else
    			output.uv2 = _DissolveType ? input.uv2.xy : positionWS.xz * _DissolveMap_ST.xy;
    		#endif
    	#endif
        
        return output;
    }


    // Used in Standard (Physically Based) shader
    half4 LitPassFragment(Varyings input) : SV_Target
    {
        UNITY_SETUP_INSTANCE_ID(input);

        #if defined(_PARALLAXMAP)
            #if defined(REQUIRES_TANGENT_SPACE_VIEW_DIR_INTERPOLATOR)
                half3 viewDirTS = input.viewDirTS;
            #else
                half3 viewDirWS = GetWorldSpaceNormalizeViewDir(input.positionWS);
                half3 viewDirTS = GetViewDirectionTangentSpace(input.tangentWS, input.normalWS, viewDirWS);
            #endif
            ApplyPerPixelDisplacement(viewDirTS, input.uv.xy);
        #endif

    	AddSurfData addSurfData = (AddSurfData)0;
    	addSurfData.uv = input.uv;
    	addSurfData.uv1 = input.occlusionUV.xy;
    	addSurfData.normalWS = input.normalWS;
    	
    	#ifdef _DISSOIVEKEY_ON
    		addSurfData.dissolveUV = input.uv2.xy;
    		addSurfData.DissolveMask = input.uv2.zw;
		#endif
    	
    	#ifdef _DYNAMIC_DISSOLVE_ON
    		addSurfData.AbsoluteWorldSpacePosition = input.AbsoluteWorldSpacePosition;
    	#endif
    	
        SurfaceData surfaceData;
        InitializeStandardLitSurfaceData(addSurfData, surfaceData);

        InputData inputData;
        InitializeInputData(input, surfaceData.normalTS, inputData);
        SETUP_DEBUG_TEXTURE_DATA(inputData, input.uv, _BaseMap);

    	#ifdef LOD_LOW
			half4 color = UniversalFragmentPBR_Lod(inputData, surfaceData);
    	#else
    		half4 color = UniversalFragmentPBR_New(inputData, surfaceData);
    	#endif

    	color.xyz = lerp(color.xyz, color.xyz * 2.5 + 0.1, _EffectColor.w);
    	ApplyFog(color.rgb, input.occlusionUV.zw);
        
        color.a = OutputAlpha(color.a, _Surface);

        return color;
    }

#endif
