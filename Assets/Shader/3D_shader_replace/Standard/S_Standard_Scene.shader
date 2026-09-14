Shader "Standards/S_Standard_Scene"
{
    Properties
    {
	    [ToggleOff] _DyeToggle("Dye Toggle", Float) = 0.0
    	_UVType("UV Type", int) = 1
    	// _MaskMap("Mask Map", 2D) = "white" {}
    	[MainColor] _BaseColor("Color", Color) = (1,1,1,1)
//    	_BaseColor1("ColorG", Color) = (1,1,1,1)
//    	_BaseColor2("ColorB", Color) = (1,1,1,1)
        [MainTexture] _BaseMap("Albedo", 2D) = "white" {}
	    [Toggle]_UVOffset_Toggle("Toggle UV Offset", Float) = 0
        _Cutoff("Alpha Cutoff", Range(0.0, 1.0)) = 0.5

        _Smoothness("Smoothness", Range(0.0, 1.0)) = 0.5
        _SmoothnessTextureChannel("Smoothness texture channel", Float) = 0

        _Metallic("Metallic", Range(0.0, 1.0)) = 0.0
        _MetallicGlossMap("Metallic", 2D) = "white" {}

        [ToggleOff] _SpecularHighlights("Specular Highlights", Float) = 1.0
        [ToggleOff] _EnvironmentReflections("Environment Reflections", Float) = 1.0

        _BumpScale("Scale", Float) = 1.0
        _BumpTex("Normal Map", 2D) = "bump" {}

        [ToggleOff] _ParallaxToggle("Parallax Toggle", Float) = 0.0
        _Parallax("Scale", Range(0.005, 0.08)) = 0.005

    	// [ToggleOff] _OcclusionToggle("Occlusion Toggle", Float) = 0.0
    	[Enum(UV0, 0, UV1, 1)]_OcclusionUVType("Occlusion UV Type", int) = 0
        _OcclusionStrength("Strength", Range(0.0, 1.0)) = 1.0

        [HDR] _EmissionColor("Color", Color) = (0,0,0)
        _EmissionMap("Emission", 2D) = "white" {}
        _shadowIntensity("暗部颜色", Color) = (0,0,0)

        // Blending state
        _Surface("__surface", Float) = 0.0
        _Blend("__blend", Float) = 0.0
        _Cull("__cull", Float) = 2.0
        [ToggleUI] _AlphaClip("__clip", Float) = 0.0
        [HideInInspector] _SrcBlend("__src", Float) = 1.0
        [HideInInspector] _DstBlend("__dst", Float) = 0.0
        [HideInInspector] _ZWrite("__zw", Float) = 1.0

        _ReflectionToggle("Reflection Toggle", Float) = 1.0
    	_SpecCube("Cube", CUBE) = "white" {}
    	_ReflectionIntensity("Reflection Intensity" , Range(0.0,1)) = 1.0
    	_ReflectionNoise("Reflection Noise" , Range(0.0, 0.05)) = 0.01
        _SpecCubeMip("Reflection Noise" , Range(0.0, 5)) = 0
        
        // Editmode props
        _QueueOffset("Queue offset", Float) = 0.0

    	// Wind
//    	[Toggle] _WindToggle("Wind Toggle", int) = 0
//	    _WaveSpeed("WaveSpeed",float) = 1
//        _WaveIntensity("WaveIntensity",float) = 0.05
//        _WaveScale("WaveScale",float) = 80
//    	// _WindDirection("WindDirection", Vector) = (0,5,0,1)
//    	_Wiggle("xy:Speed z:Scale w:YOffset", Vector) = (0,30,0.05,0.5)
//    	
//    	[Toggle(_DISSOIVEKEY_ON)] _DissoiveKey("DissoiveKey", int) = 0
//    	[Enum(World, 0, UV2, 1)]_DissolveType("DissolveType", int) = 0
//    	[HDR]_DissolveColor("DissolveColor", Color) = (7,0,0.1,1)
//    	_DissolveMap("DissolveMap", 2D) = "DissolveMap" {}
//    	_Dissolve("Dissolve", Range( 0 , 1)) = 0
//    	_Edge_size("EdgeSize", Range(0.01, 0.2)) = 0.04
    	
    	// [Toggle(_HSV_ON)] _HSVKey("开启染色", int) = 0
    	_HSVoffset("染色Color A:alpha", Vector) = (0,1,1,1)
    	
    	[Toggle(_PROJECTIONTOGGLE_ON)]_ProjectionToggle("投影开关", Float) = 0
	    _ProjectionMap("投影/A:粗糙", 2D) = "white" {}
    	_ProjectionSmoothness("ProjectionSmoothness", Range(0.0, 1.0)) = 0.0
    	_ProjectionDir ("混合方向", Vector) = (0,1,0,0)
        _ProjectionDown ("混合边缘下界", range(-0.5,1.5)) = 0.5
        _ProjectionUp ("混合边缘上界", range(-0.5,1.5)) = 0.6
    	_ProjectionUvType ("投影UV类型", int) = 0
    	[HideInInspector][Toggle]_BigObject("大的物体", int) = 0
    	
    	_OutlineType("OutlineType", int) = 0
    	
    	[Toggle]_OutlineEnabled ("Outline Enabled", Float) = 0.0
    	_OutlineColor("Outline Color", Color) = (0,0,0,1)
		_OutlineParams("Screen/Width/ZOffset", Vector) = (0,0,1,1)
		_OutlineWidthParams("Outline Width Params", Vector) = (0,5,0,1)
    	
    	[Header(Effect)]
		[HDR]_EffectColor("EffectColor rgb:效果颜色 a:程度", Color) = (0.6, 0.6, 0.6, 0)
    	
    	[Toggle] _AdditionalLights("Additional Lights", Float) = 0

    	// 溶解反转属性
    	[Toggle]_InvertDissolveEffect("Invert Dissolve Effect", int) = 0
    	[Toggle]_isGlobal ("Is Global", int) = 1
    	[HideInInspector]_DynamicRadialMasks_DATA1("Dynamic Radial Masks DATA1", Vector) = (1,1,1,1)
    	[HideInInspector]_DynamicRadialMasks_DATA2("Dynamic Radial Masks DATA2", Vector) = (1,1,1,1)
    	[HideInInspector][HDR]_DynamicRadialMasks_MaskEdgeColor("Dynamic Radial Masks Mask Edge Color", Color) = (0,0,0,1)
    	[HideInInspector]_DynamicRadialMasks_NoiseUvScale("Dynamic Radial Masks Noise UV Scale", Float) = 1
    	
        [HideInInspector][NoScaleOffset]unity_Lightmaps("unity_Lightmaps", 2DArray) = "" {}
        [HideInInspector][NoScaleOffset]unity_LightmapsInd("unity_LightmapsInd", 2DArray) = "" {}
        [HideInInspector][NoScaleOffset]unity_ShadowMasks("unity_ShadowMasks", 2DArray) = "" {}
        
    }
    
    SubShader
    {
        Tags{"RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "UniversalMaterialType" = "Lit" "IgnoreProjector" = "True" "ShaderModel"="3.5"}
        LOD 200

        Pass
		{
			Name "CityOutline"
			Tags
			{
				"LightMode" = "CitySceneOutline"
			}
			
			ZWrite On

			Cull Front

			HLSLPROGRAM
			#pragma vertex CityOutlinePassVertex
			#pragma fragment CityOutlinePassFragment
	
			// #pragma shader_feature_local _WINDTOGGLE_ON
			#pragma shader_feature_local_fragment _ALPHATEST_ON
			#include "H_Input_Scene.hlsl"
			#include "SceneOutlinePass.hlsl"
			ENDHLSL
		}

        Pass
		{
			Name "Outline"
			Tags
			{
				"LightMode" = "SceneOutline"
			}
			
			ZWrite On

			Cull Front

			HLSLPROGRAM
			#pragma vertex OutlinePassVertex
			#pragma fragment OutlinePassFragment
			// #pragma shader_feature_local _WINDTOGGLE_ON
			#pragma shader_feature_local_fragment _ALPHATEST_ON
			
			#include "H_Input_Scene.hlsl"
			#include "SceneOutlinePass.hlsl"
			ENDHLSL
		}

        Pass
        {
            Name "ForwardLit"
            Tags{"LightMode" = "UniversalForward"}

            Blend[_SrcBlend][_DstBlend]
            ZWrite[_ZWrite]
            Cull[_Cull]

            Stencil
        	{
        		Ref 240
        		Comp Always
        		Pass Replace
        		Fail Keep
        	}
            
            HLSLPROGRAM
            #pragma target 2.0

            //--------------------------------------
            // GPU Instancing
            // #pragma multi_compile_instancing
            #pragma instancing_options renderinglayer

            // -------------------------------------
            // Material Keywords
            // 法线开启
            #pragma shader_feature_local _NORMALMAP
            // 视差
            // #pragma shader_feature_local _PARALLAXMAP
            // 染色
            // #pragma multi_compile_local_fragment _ _DYETOGGLE_ON
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            // #pragma shader_feature_local_fragment _ALPHAPREMULTIPLY_ON				// 先关闭吧 目前只给透明的Premultiply模式用
            // #pragma shader_feature_local _WINDTOGGLE_ON
			// 开启投影
            #pragma shader_feature_local_fragment _PROJECTIONTOGGLE_ON
            
            // 可关闭, 节省变体用
            // #pragma shader_feature_local_fragment _EMISSION
            #define _EMISSION 1
            #pragma shader_feature_local_fragment _OCCLUSIONMAP
            // #define _OCCLUSIONMAP 1
            
            #pragma shader_feature_local _REFLECTIONTOGGLE_ON
            
            // 也可以优化
            // #pragma shader_feature_local_fragment _SPECULARHIGHLIGHTS_OFF
            // #pragma shader_feature_local_fragment _ENVIRONMENTREFLECTIONS_OFF
            // -------------------------------------
            // Universal Pipeline keywords
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS // _MAIN_LIGHT_SHADOWS_CASCADE
            //#pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            // #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            // #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma shader_feature_local _ADDITIONALLIGHTS_ON
            #ifdef _ADDITIONALLIGHTS_ON
				#define _ADDITIONAL_LIGHTS 1
            #endif
            
            // 动态溶解
            #pragma multi_compile_local _ _DYNAMIC_DISSOLVE_ON
            
            // 软阴影
            // #pragma multi_compile_fragment _ _SHADOWS_SOFT
            // 溶解
			// #pragma multi_compile _ _DISSOIVEKEY_ON
            // 溶解染色
            // #pragma shader_feature_local _HSV_ON
            // 2021灯光层级支持
            //#pragma multi_compile_fragment _ _LIGHT_LAYERS
            // 光源的Cookie纹理
            //#pragma multi_compile_fragment _ _LIGHT_COOKIES

            // -------------------------------------
            // Unity defined keywords
            #pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
            // 烘焙和动态阴影
            // #pragma multi_compile _ SHADOWS_SHADOWMASK

            // 开启烘焙
            // #pragma multi_compile _ LIGHTMAP_ON
            // 需要和美术确定烘焙模式
            // #pragma multi_compile _ DIRLIGHTMAP_COMBINED
            #define DIRLIGHTMAP_COMBINED 1
            // #pragma multi_compile_fog
            //#pragma multi_compile _ _HEIGHTFOG

            #pragma vertex LitPassVertex
            #pragma fragment LitPassFragment

            #include "H_Input_Scene.hlsl"
            #include "H_ForwardPass_Scene.hlsl"
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags{"LightMode" = "ShadowCaster"}

            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull[_Cull]

            HLSLPROGRAM
            #pragma target 2.0

            //--------------------------------------
            // GPU Instancing
            // #pragma multi_compile_instancing
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            // #pragma shader_feature_local _WINDTOGGLE_ON
            #pragma multi_compile_local _ _DYNAMIC_DISSOLVE_ON

            // 溶解
			// #pragma multi_compile _ _DISSOIVEKEY_ON
            // 溶解染色
            // #pragma shader_feature_local _HSV_ON
            
            // 点光源阴影
            // #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW

            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment

            #include "H_Input_Scene.hlsl"
            #include "H_ShadowCasterPass_Scene.hlsl"
            ENDHLSL
        }
        
        Pass
        {
            Name "DepthOnly"
            Tags{"LightMode" = "DepthOnly"}

            ZWrite On
            ColorMask 0
            Cull[_Cull]

            HLSLPROGRAM
            #pragma target 2.0

            #pragma vertex DepthOnlyVertex
            #pragma fragment DepthOnlyFragment
            
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            // #pragma shader_feature_local _WINDTOGGLE_ON
            #pragma multi_compile_local _ _DYNAMIC_DISSOLVE_ON
            // #pragma multi_compile_instancing
            //#pragma multi_compile _ DOTS_INSTANCING_ON

            // 溶解
			// #pragma multi_compile _ _DISSOIVEKEY_ON
            // 溶解染色
            // #pragma shader_feature_local _HSV_ON

            #include "H_Input_Scene.hlsl"
            #include "H_DepthOnlyPass_Scene.hlsl"
            
            ENDHLSL
        }

        Pass
        {
            Name "Meta"
            Tags{"LightMode" = "Meta"}

            Cull Off

            HLSLPROGRAM
            #pragma target 2.0 

            #pragma vertex UniversalVertexMeta
            #pragma fragment UniversalFragmentMetaLit

            #pragma shader_feature EDITOR_VISUALIZATION
            // 需要看下自发光代码
            //#pragma shader_feature_local_fragment _EMISSION
            #pragma shader_feature_local_fragment _ALPHATEST_ON

            #include "H_Input_Scene.hlsl"
            #include "H_LitMetaPass_Scene.hlsl"

            ENDHLSL
        }
    }

    SubShader
    {

        Tags{"RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "UniversalMaterialType" = "Lit" "IgnoreProjector" = "True" "ShaderModel"="3.5"}
        LOD 100

        Pass
		{
			Name "Outline"
			Tags
			{
				"LightMode" = "SceneOutline"
			}
			
			ZWrite On

			Cull Front

			HLSLPROGRAM
			#pragma vertex OutlinePassVertex
			#pragma fragment OutlinePassFragment
			// #pragma shader_feature_local _WINDTOGGLE_ON
			#pragma shader_feature_local_fragment _ALPHATEST_ON
			#include "H_Input_Scene.hlsl"
			#include "SceneOutlinePass.hlsl"
			ENDHLSL
		}

        Pass
        {
            Name "ForwardLit"
            Tags{"LightMode" = "UniversalForward"}

            Blend[_SrcBlend][_DstBlend]
            ZWrite[_ZWrite]
            Cull[_Cull]
            
            Stencil
        	{
        		Ref 240
        		Comp Always
        		Pass Replace
        		Fail Keep
        	}

            HLSLPROGRAM
            #pragma target 2.0

            //--------------------------------------
            // GPU Instancing
            // #pragma multi_compile_instancing
            #pragma instancing_options renderinglayer

            #define LOD_LOW 1
            // -------------------------------------
            // Material Keywords
            // 法线开启
            // #pragma shader_feature_local _NORMALMAP
            // 视差
            // #pragma shader_feature_local _PARALLAXMAP
            // 染色
            // #pragma shader_feature_local_fragment _DYETOGGLE_ON
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            // #pragma shader_feature_local_fragment _ALPHAPREMULTIPLY_ON				// 先关闭吧 目前只给透明的Premultiply模式用
            // #pragma shader_feature_local _WINDTOGGLE_ON
            // 开启投影
            #pragma shader_feature_local_fragment _PROJECTIONTOGGLE_ON
            #pragma multi_compile_local _ _DYNAMIC_DISSOLVE_ON
            
            // 可关闭, 节省变体用
            // #pragma shader_feature_local_fragment _EMISSION
            // #define _EMISSION 1
            // #pragma shader_feature_local_fragment _OCCLUSIONMAP
            // #define _OCCLUSIONMAP 1
            
            #pragma shader_feature_local _REFLECTIONTOGGLE_ON
            
            // 也可以优化
            // #pragma shader_feature_local_fragment _SPECULARHIGHLIGHTS_OFF
            #define _SPECULARHIGHLIGHTS_OFF 1
            // #pragma shader_feature_local_fragment _ENVIRONMENTREFLECTIONS_OFF
            // -------------------------------------
            // Universal Pipeline keywords
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS // _MAIN_LIGHT_SHADOWS_CASCADE
            //#pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            // #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            // #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma shader_feature_local _ADDITIONALLIGHTS_ON
            #ifdef _ADDITIONALLIGHTS_ON
				#define _ADDITIONAL_LIGHTS 1
            #endif
            
            // 软阴影
            // #pragma multi_compile_fragment _ _SHADOWS_SOFT
            // 溶解
			// #pragma multi_compile _ _DISSOIVEKEY_ON
            // 溶解染色
            // #pragma shader_feature_local _HSV_ON
            // 2021灯光层级支持
            //#pragma multi_compile_fragment _ _LIGHT_LAYERS
            // 光源的Cookie纹理
            //#pragma multi_compile_fragment _ _LIGHT_COOKIES

            // -------------------------------------
            // Unity defined keywords
            #pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
            // 烘焙和动态阴影
            // #pragma multi_compile _ SHADOWS_SHADOWMASK

            // 开启烘焙
            // #pragma multi_compile _ LIGHTMAP_ON
            // 需要和美术确定烘焙模式
            // #pragma multi_compile _ DIRLIGHTMAP_COMBINED
            #define DIRLIGHTMAP_COMBINED 1
            // #pragma multi_compile_fog
            //#pragma multi_compile _ _HEIGHTFOG

            #pragma vertex LitPassVertex
            #pragma fragment LitPassFragment

            #include "H_Input_Scene.hlsl"
            #include "H_ForwardPass_Scene.hlsl"
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags{"LightMode" = "ShadowCaster"}

            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull[_Cull]

            HLSLPROGRAM
            #pragma target 2.0

            //--------------------------------------
            // GPU Instancing
            // #pragma multi_compile_instancing
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            // #pragma shader_feature_local _WINDTOGGLE_ON
            #pragma multi_compile_local _ _DYNAMIC_DISSOLVE_ON
            #define LOD_LOW 1

            // 溶解
			// #pragma multi_compile _ _DISSOIVEKEY_ON
            // 溶解染色
            // #pragma shader_feature_local _HSV_ON
            
            // 点光源阴影
            // #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW

            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment

            #include "H_Input_Scene.hlsl"
            #include "H_ShadowCasterPass_Scene.hlsl"
            ENDHLSL
        }
        
        Pass
        {
            Name "DepthOnly"
            Tags{"LightMode" = "DepthOnly"}

            ZWrite On
            ColorMask 0
            Cull[_Cull]

            HLSLPROGRAM
            #pragma target 2.0

            #pragma vertex DepthOnlyVertex
            #pragma fragment DepthOnlyFragment
            
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            // #pragma shader_feature_local _WINDTOGGLE_ON
            #pragma multi_compile_local _ _DYNAMIC_DISSOLVE_ON
            // #pragma multi_compile_instancing
            #define LOD_LOW 1
            //#pragma multi_compile _ DOTS_INSTANCING_ON

            // 溶解
			// #pragma multi_compile _ _DISSOIVEKEY_ON
            // 溶解染色
            // #pragma shader_feature_local _HSV_ON

            #include "H_Input_Scene.hlsl"
            #include "H_DepthOnlyPass_Scene.hlsl"
            
            ENDHLSL
        }

        Pass
        {
            Name "Meta"
            Tags{"LightMode" = "Meta"}

            Cull Off

            HLSLPROGRAM
            #pragma target 2.0

            #pragma vertex UniversalVertexMeta
            #pragma fragment UniversalFragmentMetaLit

            #pragma shader_feature EDITOR_VISUALIZATION
            // 需要看下自发光代码
            //#pragma shader_feature_local_fragment _EMISSION
            #pragma shader_feature_local_fragment _ALPHATEST_ON

            #include "H_Input_Scene.hlsl"
            #include "H_LitMetaPass_Scene.hlsl"

            ENDHLSL
        }
    }

    CustomEditor "GUI_Standard_Scene"
}
