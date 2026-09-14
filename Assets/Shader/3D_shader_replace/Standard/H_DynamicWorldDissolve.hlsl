#ifndef UNIVERSAL_DYNAMIC_WORLD_DISSOLVE_INCLUDED
#define UNIVERSAL_DYNAMIC_WORLD_DISSOLVE_INCLUDED

    TEXTURE2D(_DynamicRadialMasks_Global_MaskNoiseMap);   SAMPLER(sampler_DynamicRadialMasks_Global_MaskNoiseMap);
    uniform float4 _DynamicRadialMasks_Global_DATA1;	
    uniform float4 _DynamicRadialMasks_Global_DATA2;
    uniform half4 _DynamicRadialMasks_Global_MaskEdgeColor;
    uniform half _DynamicRadialMasks_Global_NoiseUvScale;

    inline float DynamicRadialMasks(float3 vertexWorldPosition, float3 maskPosition, float radius, float intensity, float edgeSize, float smooth, float noise)
    {
        //Distance
        float d = distance(maskPosition, vertexWorldPosition);
        //Noise
        radius += noise;
        float shape = 1 - saturate(max(0, d - radius + edgeSize) / edgeSize);
        //Smooth
        shape = pow(shape, smooth + 0.01);	//BUG: does not work if do not add 0.01. Hmmmm
        //Fade
        shape *= intensity;
                    
        return shape;
    }

    inline float DynamicRadialMasks_Global(float3 positionWS, float noise, float4 _DynamicRadialMasks_DATA1, float4 _DynamicRadialMasks_DATA2)
    {
        // float retValue = 1.0;
        float3 centerPos     = _DynamicRadialMasks_DATA1.xyz;
        float  radius        = _DynamicRadialMasks_DATA1.w;

        float  intensity     = _DynamicRadialMasks_DATA2.x;
        float  edgeSize      = _DynamicRadialMasks_DATA2.y;
        float  smooth        = _DynamicRadialMasks_DATA2.z;
        float  noiseStrength = _DynamicRadialMasks_DATA2.w * noise;

        float mask = DynamicRadialMasks(positionWS, centerPos, radius, intensity, edgeSize, smooth, noiseStrength);
        // retValue *= (1.0 - saturate(mask));
        //             
        // return 1.0 - retValue;
        return saturate(mask);
    }

    // 给地表专用
    inline float ComputeEdgeColorAndMask(float3 worldPos, float2 uv, out half4 edgeColor)
    {
        float2 noiseUV = uv * _DynamicRadialMasks_Global_NoiseUvScale;
        half noiseSample = SAMPLE_TEXTURE2D(_DynamicRadialMasks_Global_MaskNoiseMap, sampler_DynamicRadialMasks_Global_MaskNoiseMap, noiseUV).r;

    	float4 DATA1 = _DynamicRadialMasks_Global_DATA1;
    	float4 DATA2 = _DynamicRadialMasks_Global_DATA2;
        float mask = DynamicRadialMasks_Global(worldPos, noiseSample, DATA1, DATA2);
        edgeColor = _DynamicRadialMasks_Global_MaskEdgeColor * mask.xxxx;
        mask *= 2;
        return mask;
    }

    inline float ComputeEdgeColorAndMask(float3 worldPos, float2 uv, int isGlobal,
    	float4 _DynamicRadialMasks_DATA1, float4 _DynamicRadialMasks_DATA2,
    	half _DynamicRadialMasks_NoiseUvScale, int _InvertDissolveEffect)
    {
        float2 noiseUV = uv * (isGlobal ? _DynamicRadialMasks_Global_NoiseUvScale : _DynamicRadialMasks_NoiseUvScale);
        half noiseSample = SAMPLE_TEXTURE2D(_DynamicRadialMasks_Global_MaskNoiseMap, sampler_DynamicRadialMasks_Global_MaskNoiseMap, noiseUV).r;

    	float4 DATA1 = isGlobal ? _DynamicRadialMasks_Global_DATA1 : _DynamicRadialMasks_DATA1;
    	float4 DATA2 = isGlobal ? _DynamicRadialMasks_Global_DATA2 : _DynamicRadialMasks_DATA2;
    	float mask = DynamicRadialMasks_Global(worldPos, noiseSample, DATA1, DATA2);
        float maskInverted = 1.0 - mask;
        float finalMask = _InvertDissolveEffect ? maskInverted : mask;
        return finalMask * 2;
    }

    inline float ComputeEdgeColorAndMask(float3 worldPos, float2 uv, int isGlobal,
    	float4 _DynamicRadialMasks_DATA1, float4 _DynamicRadialMasks_DATA2,
		half4 _DynamicRadialMasks_MaskEdgeColor, half _DynamicRadialMasks_NoiseUvScale,
		int _InvertDissolveEffect, out half4 edgeColor)
    {
    	float2 noiseUV = uv * (isGlobal ? _DynamicRadialMasks_Global_NoiseUvScale : _DynamicRadialMasks_NoiseUvScale);
        half noiseSample = SAMPLE_TEXTURE2D(_DynamicRadialMasks_Global_MaskNoiseMap, sampler_DynamicRadialMasks_Global_MaskNoiseMap, noiseUV).r;

    	float4 DATA1 = isGlobal ? _DynamicRadialMasks_Global_DATA1 : _DynamicRadialMasks_DATA1;
    	float4 DATA2 = isGlobal ? _DynamicRadialMasks_Global_DATA2 : _DynamicRadialMasks_DATA2;
    	float mask = DynamicRadialMasks_Global(worldPos, noiseSample, DATA1, DATA2);
        float maskInverted = 1.0 - mask;
        float finalMask = _InvertDissolveEffect ? maskInverted : mask;
    	edgeColor = (isGlobal ? _DynamicRadialMasks_Global_MaskEdgeColor : _DynamicRadialMasks_MaskEdgeColor) * finalMask.xxxx;
        return finalMask * 2;
    }

	// 草地专用
    inline float ComputeEdgeColorAndMask(float3 worldPos, float2 uv, int _InvertDissolveEffect)
    {
        float2 noiseUV = uv * _DynamicRadialMasks_Global_NoiseUvScale;
        half noiseSample = SAMPLE_TEXTURE2D(_DynamicRadialMasks_Global_MaskNoiseMap, sampler_DynamicRadialMasks_Global_MaskNoiseMap, noiseUV).r;
    	float mask = DynamicRadialMasks_Global(worldPos, noiseSample, _DynamicRadialMasks_Global_DATA1, _DynamicRadialMasks_Global_DATA2);
        float maskInverted = 1.0 - mask;
        float finalMask = _InvertDissolveEffect ? maskInverted : mask;
        return finalMask * 2;
    }
	// 草地专用
    inline float ComputeEdgeColorAndMask(float3 worldPos, float2 uv, int _InvertDissolveEffect, out half4 edgeColor)
    {
    	float2 noiseUV = uv * _DynamicRadialMasks_Global_NoiseUvScale;
        half noiseSample = SAMPLE_TEXTURE2D(_DynamicRadialMasks_Global_MaskNoiseMap, sampler_DynamicRadialMasks_Global_MaskNoiseMap, noiseUV).r;
    	float mask = DynamicRadialMasks_Global(worldPos, noiseSample, _DynamicRadialMasks_Global_DATA1, _DynamicRadialMasks_Global_DATA2);
        float maskInverted = 1.0 - mask;
        float finalMask = _InvertDissolveEffect ? maskInverted : mask;
    	edgeColor = _DynamicRadialMasks_Global_MaskEdgeColor * finalMask.xxxx;
        return finalMask * 2;
    }


#endif
